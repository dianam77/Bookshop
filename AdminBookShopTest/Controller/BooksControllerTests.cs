using System.Linq.Expressions;
using AdminBookShop.Controllers;
using AutoFixture;
using AutoFixture.AutoMoq;
using Core.BookService;
using Core.ServiceFile;
using DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AdminBookShop.Tests
{
    [TestFixture]
    public class BooksControllerTests
    {
        private IFixture _fixture;
        private Mock<IBookService> _bookServiceMock;
        private Mock<IAuthorService> _authorServiceMock;
        private Mock<IFileService> _fileServiceMock;
        private Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private BooksController _sut; 

        [SetUp]
        public void Setup()
        {
    
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _bookServiceMock = _fixture.Freeze<Mock<IBookService>>();  
            _authorServiceMock = _fixture.Freeze<Mock<IAuthorService>>();
            _fileServiceMock = _fixture.Freeze<Mock<IFileService>>();
            _webHostEnvironmentMock = _fixture.Freeze<Mock<IWebHostEnvironment>>();

            _sut = new BooksController(
                _bookServiceMock.Object,
                _authorServiceMock.Object,
                _fileServiceMock.Object,
                _webHostEnvironmentMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (_sut != null)
            {
                _sut.Dispose();
            }
        }

        [Test]
        public async Task Index_Returns_ViewResult_With_Books()
        {
            // Arrange
            var expectedBooks = _fixture.CreateMany<Book>().ToList();

            _bookServiceMock
                .Setup(s => s.GetBooksWithAuthors(It.IsAny<Expression<Func<Book, bool>>>()))
                .ReturnsAsync(expectedBooks);

            // Act
            var result = await _sut.Index();

            // Assert 
            result.Should().BeOfType<ViewResult>("Index should return a ViewResult.");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull("a non-null ViewResult is expected.");
            viewResult!.Model.Should().BeEquivalentTo(expectedBooks, "the model should be the list of books returned by the service.");
        }

        [Test]
        public async Task Details_NullId_Returns_NotFound()
        {
            // Act
            var result = await _sut.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>("passing a null id should return NotFoundResult.");
        }

        [Test]
        public async Task Details_BookNotFound_Returns_NotFound()
        {
            // Arrange
            int testId = _fixture.Create<int>();
            _bookServiceMock
                .Setup(s => s.GetBookById(testId))
                .ReturnsAsync((Book)null!);

            // Act
            var result = await _sut.Details(testId);

            // Assert
            result.Should().BeOfType<NotFoundResult>("when no book is found, NotFoundResult should be returned.");
        }

        [Test]
        public async Task Details_ValidId_Returns_ViewResult_With_Book()
        {
            // Arrange
            var expectedBook = _fixture.Create<Book>();
            _bookServiceMock
                .Setup(s => s.GetBookById(expectedBook.Id))
                .ReturnsAsync(expectedBook);

            // Act
            var result = await _sut.Details(expectedBook.Id);

            // Assert
            result.Should().BeOfType<ViewResult>("a valid id should return a ViewResult.");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull("a non-null ViewResult is expected.");
            viewResult!.Model.Should().BeEquivalentTo(expectedBook, "the model should be the book returned by the service.");
        }

        [Test]
        public async Task Create_Get_Returns_ViewResult_With_Author_SelectList()
        {
            // Arrange
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Act
            var result = await _sut.Create();

            // Assert
            result.Should().BeOfType<ViewResult>("GET Create should return a ViewResult.");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var selectList = viewResult.ViewData["AuthorId"] as SelectList;
            selectList.Should().NotBeNull("ViewData['AuthorId'] should be a SelectList.");
            var selectListItems = selectList.Items.Cast<object>().ToList();
            selectListItems.Should().HaveCount(authors.Count, "SelectList should contain the correct number of authors.");
        }

        [Test]
        public async Task Create_Post_ValidModelState_Redirects_To_Index()
        {
            // Arrange
            var bookDto = _fixture.Create<BookDto>();
            var fileMock = new Mock<IFormFile>();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(new List<Author>());
            _bookServiceMock.Setup(s => s.CreateBook(It.IsAny<BookDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Create(bookDto, fileMock.Object);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>("After a successful create, user should be redirected to the Index action.");
            var redirectResult = result as RedirectToActionResult;
            redirectResult?.ActionName.Should().Be(nameof(_sut.Index), "The action should redirect to Index.");
        }

        [Test]
        public async Task Create_Post_InvalidModelState_Returns_ViewResult_With_BookDto()
        {
            // Arrange
            var bookDto = _fixture.Create<BookDto>();
            bookDto.Title = null!;  
            _sut.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _sut.Create(bookDto, null!);  

            // Assert
            result.Should().BeOfType<ViewResult>("If the model state is invalid, the action should return a ViewResult with validation errors.");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var selectList = viewResult.ViewData["AuthorId"] as SelectList;
            selectList.Should().NotBeNull("ViewData['AuthorId'] should contain a SelectList.");
            viewResult.Model.Should().BeEquivalentTo(bookDto, "The model returned should be the BookDto with validation errors.");
        }
        [Test]
        public async Task Edit_WhenIdIsNull_ReturnsNotFound()
        {
            // Act
            var result = await _sut.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_WhenBookIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var testId = 1;
            _bookServiceMock.Setup(s => s.GetBookDtoById(testId)).ReturnsAsync((BookDto)null!);

            // Act
            var result = await _sut.Edit(testId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var bookDto = _fixture.Create<BookDto>();
            bookDto.Title = null!; 

            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Act
            _sut.ModelState.AddModelError("Title", "Title is required.");
            var result = await _sut.Edit(bookDto.Id, bookDto);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.Model.Should().Be(bookDto);
        }


        [Test]
        public async Task Edit_Post_ConcurrencyError_ReturnsViewWithModel()
        {
            // Arrange
            var bookDto = _fixture.Create<BookDto>();
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);
            _bookServiceMock.Setup(s => s.UpdateBook(It.IsAny<BookDto>()))
                            .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _sut.Edit(bookDto.Id, bookDto);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.Model.Should().Be(bookDto);
        }


        [Test]
        public async Task Edit_Post_IdMismatch_Returns_NotFound()
        {
            // Arrange
            var bookDto = _fixture.Create<BookDto>();
            bookDto.Id = _fixture.Create<int>(); 
            int id = _fixture.Create<int>();  

            // Act
            var result = await _sut.Edit(id, bookDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>("When the ids don't match, the action should return NotFound.");
        }



        [Test]
        public async Task Edit_Post_ValidModelState_Redirects_To_Index()
        {
            // Arrange
            var bookDto = _fixture.Create<BookDto>();
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Act
            var result = await _sut.Edit(bookDto.Id, bookDto);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>("If the model is valid, the action should redirect.");
            var redirectResult = result as RedirectToActionResult;
            redirectResult?.ActionName.Should().Be("Index", "After a successful edit, the user should be redirected to the Index action.");
        }

        [Test]
        public async Task Delete_Get_WhenIdIsNull_ReturnsNotFound()
        {
            // Act
            var result = await _sut.Delete(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>("Passing a null id should return NotFound.");
        }

        [Test]
        public async Task Delete_Get_WhenBookNotFound_ReturnsNotFound()
        {
            // Arrange
            int testId = _fixture.Create<int>();
            _bookServiceMock.Setup(s => s.GetBookById(testId)).ReturnsAsync((Book?)null);

            // Act
            var result = await _sut.Delete(testId);

            // Assert
            result.Should().BeOfType<NotFoundResult>("If the book is not found, should return NotFound.");
        }

        [Test]
        public async Task Delete_Get_WhenBookExists_ReturnsViewWithBook()
        {
            // Arrange
            var expectedBook = _fixture.Create<Book>();
            _bookServiceMock.Setup(s => s.GetBookById(expectedBook.Id)).ReturnsAsync(expectedBook);

            // Act
            var result = await _sut.Delete(expectedBook.Id);

            // Assert
            result.Should().BeOfType<ViewResult>("If the book exists, should return ViewResult.");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull("The ViewResult should not be null.");
            viewResult.Model.Should().NotBeNull("The model should not be null.");
            viewResult.Model.Should().BeEquivalentTo(expectedBook, "The model should be the book retrieved.");
        }


        [Test]
        public async Task DeleteConfirmed_WhenBookNotFound_ReturnsNotFound()
        {
            // Arrange
            int testId = _fixture.Create<int>();
            _bookServiceMock.Setup(s => s.GetBookById(testId)).ReturnsAsync((Book?)null);

            // Act
            var result = await _sut.DeleteConfirmed(testId);

            // Assert
            result.Should().BeOfType<NotFoundResult>("If the book does not exist, should return NotFound.");
        }

        [Test]
        public async Task DeleteConfirmed_WhenBookExists_DeletesBookAndRedirects()
        {
            // Arrange
            var book = _fixture.Create<Book>();
            _bookServiceMock.Setup(s => s.GetBookById(book.Id)).ReturnsAsync(book);
            _bookServiceMock.Setup(s => s.DeleteBook(book)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteConfirmed(book.Id);

            // Assert
            _bookServiceMock.Verify(s => s.DeleteBook(book), Times.Once, "DeleteBook should be called once.");
            result.Should().BeOfType<RedirectToActionResult>("Should redirect after successful deletion.");
            var redirectResult = result as RedirectToActionResult;
            if (redirectResult is RedirectToActionResult redirectToActionResult)
            {
                redirectToActionResult.ActionName.Should().Be(nameof(_sut.Index), "Should redirect to Index action.");
            }
            else
            {
                Assert.Fail("Expected RedirectToActionResult but got a different result type.");
            }
        }
        

        [Test]
        public async Task DeleteFile_WhenFileExists_DeletesFileAndReturnsOk()
        {
            // Arrange
            var webRootPath = "C:\\TestWebRoot";  
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            var filePath = "Files/testfile.txt";
            var rootPath = Path.Combine(webRootPath, filePath);

            Directory.CreateDirectory(Path.GetDirectoryName(rootPath) ?? throw new ArgumentNullException(nameof(rootPath)));
            await File.WriteAllTextAsync(rootPath, "Test content");

            File.Exists(rootPath).Should().BeTrue("The test file should exist before deletion");

            // Act
            var result = await _sut.DeleteFile($"/{filePath}");

            // Assert
            result.Should().BeOfType<OkResult>();
            File.Exists(rootPath).Should().BeFalse("The file should be deleted after the method call");
        }

        [Test]
        public async Task DeleteFile_WhenFileDoesNotExist_ReturnsOk()
        {
            // Arrange
            var filePath = "Files/nonexistentfile.txt";
            var webRootPath = "C:\\TestWebRoot";  
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            var rootPath = Path.Combine(webRootPath, filePath);

            // Act
            var result = await _sut.DeleteFile($"/{filePath}");

            // Assert
            result.Should().BeOfType<OkResult>();
            File.Exists(rootPath).Should().BeFalse("The file should not exist after the method call");
        }

        [Test]
        public async Task DeleteFile_WhenIOExceptionOccurs_CatchesExceptionAndReturnsOk()
        {
            // Arrange
            var filePath = "Files/lockedfile.txt";
            var webRootPath = "C:\\TestWebRoot"; 
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            var rootPath = Path.Combine(webRootPath, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(rootPath) ?? throw new ArgumentNullException(nameof(rootPath)));
            await File.WriteAllTextAsync(rootPath, "Test content");

            using (var stream = new FileStream(rootPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                var result = await _sut.DeleteFile($"/{filePath}");

                // Assert
                result.Should().BeOfType<OkResult>();
                File.Exists(rootPath).Should().BeTrue("The file should still exist because it was locked and could not be deleted");
            }

            File.Delete(rootPath);
        }

        [Test]
        public void UploadFile_WhenFileUploaded_SavesFileAndReturnsFileInfo()
        {
            // Arrange
            var path = Guid.NewGuid().ToString();
            var fileName = "testfile.jpg";        
            var fileContent = "Sample file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", fileName);
            var formFileCollection = new FormFileCollection { formFile };
            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Form.Files).Returns(formFileCollection);
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };
            var testWebRoot = Path.Combine(Path.GetTempPath(), "TestWebRoot");
            Directory.CreateDirectory(testWebRoot);
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(testWebRoot);

            // Act
            var result = _sut.UploadFile(path);

            // Assert
            result.Should().BeOfType<OkObjectResult>("we expect an OkObjectResult if the file upload is successful");
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("The result should be of type OkObjectResult.");

            var valueType = okResult.Value?.GetType();
            var fileAddressProperty = valueType?.GetProperty("FileAddress");
            fileAddressProperty.Should().NotBeNull("The returned object should have a 'FileAddress' property.");
            string? fileAddress = fileAddressProperty.GetValue(okResult.Value)?.ToString();
            fileAddress.Should().NotBeNullOrEmpty("FileAddress should not be null or empty.");

            fileAddress.Should().StartWith($"/Files/{path}/", "the file address should be under the correct folder");
            fileAddress.Should().EndWith(".jpg", "the file extension should be preserved");

            var savedFileName = Path.GetFileName(fileAddress);
            var guidPart = Path.GetFileNameWithoutExtension(savedFileName);
            Guid.TryParse(guidPart, out Guid guid).Should().BeTrue("the file name should be a valid GUID");

            var expectedFolderPath = Path.Combine(testWebRoot, "Files", path);
            var savedFilePath = Path.Combine(expectedFolderPath, savedFileName);
            File.Exists(savedFilePath).Should().BeTrue("the uploaded file should be saved on disk");
            Directory.Delete(testWebRoot, recursive: true);
        }


    }
}