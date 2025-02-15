using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Core.BookService;
using Core.ServiceFile;
using DataAccess.Models;
using DataAccess.Repositories.RateBookRepo;
using Bookshop.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text;
using Bookshop.Models;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class BookControllerTests
    {
        private IFixture _fixture;
        private Mock<IBookService> _mockBookService;
        private Mock<IFileService> _mockFileService;
        private Mock<IRateBookRepository> _mockRateBookRepository;
        private BookController _Sut;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockBookService = _fixture.Freeze<Mock<IBookService>>();
            _mockFileService = _fixture.Freeze<Mock<IFileService>>();
            _mockRateBookRepository = _fixture.Freeze<Mock<IRateBookRepository>>();

            _Sut = new BookController(_mockBookService.Object, _mockFileService.Object, _mockRateBookRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (_Sut != null)
            {
                _Sut.Dispose();
            }
        }

        [Test]
        public async Task Index_Returns_NotFound_When_Book_Does_Not_Exist()
        {
            // Arrange
            _mockBookService.Setup(x => x.GetBookById(It.IsAny<int>())).ReturnsAsync((Book?)null);

            // Act
            var result = await _Sut.Index(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Book not found.");
        }

        [Test]
        public async Task Index_Returns_ViewResult_With_BookDetails()
        {
            // Arrange
            var book = _fixture.Create<Book>(); 
            var booksBySameAuthor = new[] { _fixture.Create<Book>() };
            var comments = new[] { _fixture.Create<Comment>() };

            _mockBookService.Setup(x => x.GetBookById(1)).ReturnsAsync(book);
            _mockBookService.Setup(x => x.GetBooksBySameAuthor(1, 10)).ReturnsAsync(booksBySameAuthor);
            _mockBookService.Setup(x => x.GetCommentsByBookId(1)).ReturnsAsync(comments);

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "mock"));
            _Sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claims } };

            // Act
            var result = await _Sut.Index(1);

            // Assert
            result.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<BookWithSameAuthorViewModel>();
        }

        [Test]
        public async Task RateBook_Returns_Unauthorized_When_User_Not_Authenticated()
        {
            // Arrange
            var model = _fixture.Create<RateBookModel>();
            _Sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await _Sut.RateBook(model);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("برای امتیاز دادن باید وارد سیستم شوید.");
        }

        [Test]
        public async Task RateBook_Returns_BadRequest_When_UserId_Is_Missing()
        {
            // Arrange
            var model = _fixture.Create<RateBookModel>();
            model.UserId = ""; 

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "mock"));
            _Sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claims } };

            // Act
            var result = await _Sut.RateBook(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("UserId is required to rate the book.");
        }

        [Test]
        public async Task DownloadFile_Returns_File_When_File_Is_Found()
        {
            // Arrange
            var filePath = "sample.txt";
            var mockMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Sample file content"));
            _mockFileService.Setup(x => x.DownloadFileAsync(It.IsAny<string>())).ReturnsAsync(mockMemoryStream);

            // Act
            var result = await _Sut.DownloadFile(filePath);

            // Assert
            var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
            fileResult.ContentType.Should().Be("application/octet-stream");
        }

        [Test]
        public async Task DownloadFile_Returns_NotFound_When_File_Is_Not_Found()
        {
            // Arrange
            _mockFileService.Setup(x => x.DownloadFileAsync(It.IsAny<string>())).ThrowsAsync(new FileNotFoundException());

            // Act
            var result = await _Sut.DownloadFile("nonexistentfile.txt");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("File not found in source project.");
        }
    }
}
