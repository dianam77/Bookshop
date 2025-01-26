using DataAccess.Repositories.AuthorRepo;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using FluentAssertions;
using AdminBookShop.Controllers;
using DataAccess.Models;

namespace AdminBookShop.Tests.Controllers
{
    [TestFixture]
    public class AuthorsControllerTests
    {
        private Mock<IAuthorService> _mockAuthorService = null!;
        private Mock<IAuthorRepository> _mockAuthorRepository = null!;
        private AuthorsController _sut = null!;

        [SetUp]
        public void Setup()
        {
            _mockAuthorRepository = new Mock<IAuthorRepository>();
            _mockAuthorService = new Mock<IAuthorService>();
            _sut = new AuthorsController(_mockAuthorService.Object);
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithListOfAuthors()
        {
            //Arrange
            var authors = new List<Author>
            {
                new Author { Id = 1, Name = "Author 1" },
                new Author { Id = 2, Name = "Author 2" }
            };
            _mockAuthorService.Setup(service => service.GetAllAuthorsAsync()).ReturnsAsync(authors);

            //Act
            var result = await _sut.Index();

            //Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<Author>>().Subject;
            model.Should().BeEquivalentTo(authors);

            _mockAuthorService.Verify(service => service.GetAllAuthorsAsync(), Times.Once);
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            //Arrange
            // No additional setup required

            //Act
            var result = await _sut.Details(null);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenAuthorNotFound()
        {
            //Arrange
            int authorId = 1000;
            _mockAuthorService.Setup(service => service.GetAuthorByIdAsync(authorId)).ReturnsAsync((Author)null!);

            //Act
            var result = await _sut.Details(authorId);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockAuthorService.Verify(service => service.GetAuthorByIdAsync(authorId), Times.Once);
        }

        [Test]
        public async Task Details_ReturnsViewResult_WhenAuthorExists()
        {
            //Arrange
            var author = new Author { Id = 1, Name = "Author 1" };
            _mockAuthorService.Setup(service => service.GetAuthorByIdAsync(author.Id)).ReturnsAsync(author);

            //Act
            var result = await _sut.Details(author.Id);

            //Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(author);
        }

        [Test]
        public async Task Create_Post_ReturnsRedirectToIndex_WhenModelStateIsValid()
        {
            //Arrange
            var author = new Author { Id = 1, Name = "Test Author" };

            //Act
            var result = await _sut.Create(author);

            //Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(_sut.Index));
            _mockAuthorService.Verify(service => service.AddAuthorAsync(author), Times.Once);
        }

        [Test]
        public async Task Create_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            //Arrange
            var author = new Author { Id = 1, Name = null! };
            _sut.ModelState.AddModelError("Name", "Required");

            //Act
            var result = await _sut.Create(author);

            //Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(author);
            _mockAuthorService.Verify(service => service.AddAuthorAsync(It.IsAny<Author>()), Times.Never);
        }

        [Test]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            //Arrange
            // No additional setup required

            //Act
            var result = await _sut.Edit(null);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_Get_ReturnsNotFound_WhenAuthorNotFound()
        {
            //Arrange
            int authorId = 1000;
            _mockAuthorService.Setup(service => service.GetAuthorByIdAsync(authorId)).ReturnsAsync((Author)null!);

            //Act
            var result = await _sut.Edit(authorId);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_Post_ReturnsRedirectToIndex_WhenModelStateIsValid()
        {
            //Arrange
            var author = new Author { Id = 1, Name = "Updated Name" };

            //Act
            var result = await _sut.Edit(author.Id, author);

            //Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(_sut.Index));
            _mockAuthorService.Verify(service => service.UpdateAuthorAsync(author), Times.Once);
        }

        [Test]
        public async Task Edit_Post_ReturnsNotFound_WhenAuthorIdDoesNotMatch()
        {
            //Arrange
            var author = new Author { Id = 1, Name = "Mismatch" };

            //Act
            var result = await _sut.Edit(2, author);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockAuthorService.Verify(service => service.UpdateAuthorAsync(It.IsAny<Author>()), Times.Never);
        }

        [Test]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            //Arrange
            // No additional setup required

            //Act
            var result = await _sut.Delete(null);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Delete_Get_ReturnsNotFound_WhenAuthorNotFound()
        {
            //Arrange
            int authorId = 1000;
            _mockAuthorService.Setup(service => service.GetAuthorByIdAsync(authorId)).ReturnsAsync((Author)null!);

            //Act
            var result = await _sut.Delete(authorId);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task DeleteConfirmed_RemovesAuthorAndRedirectsToIndex()
        {
            //Arrange
            int authorId = 1;

            //Act
            var result = await _sut.DeleteConfirmed(authorId);

            //Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(_sut.Index));
            _mockAuthorService.Verify(service => service.DeleteAuthorAsync(authorId), Times.Once);
        }
    }
}