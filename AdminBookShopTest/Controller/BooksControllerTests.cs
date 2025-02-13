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
        private BooksController _sut; // System Under Test

        [SetUp]
        public void Setup()
        {
            // Set up AutoFixture to use Moq.
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Remove the default behavior that throws on recursion and add one that omits circular references.
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Freeze dependencies so that every request returns the same instance.
            _bookServiceMock = _fixture.Freeze<Mock<IBookService>>();  // Mocking interface
            _authorServiceMock = _fixture.Freeze<Mock<IAuthorService>>();
            _fileServiceMock = _fixture.Freeze<Mock<IFileService>>();
            _webHostEnvironmentMock = _fixture.Freeze<Mock<IWebHostEnvironment>>();

            // Create the SUT (BooksController) with the mocked dependencies.
            _sut = new BooksController(
                _bookServiceMock.Object,
                _authorServiceMock.Object,
                _fileServiceMock.Object,
                _webHostEnvironmentMock.Object);
        }

        // Dispose of _sut after each test to satisfy analyzers and free resources.
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
            // Arrange: Generate a list of books.
            var expectedBooks = _fixture.CreateMany<Book>().ToList();

            // Setup the mock to return books.
            _bookServiceMock
                .Setup(s => s.GetBooksWithAuthors(It.IsAny<Expression<Func<Book, bool>>>()))
                .ReturnsAsync(expectedBooks);

            // Act
            var result = await _sut.Index();

            // Assert using FluentAssertions.
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
            // Arrange: Create a valid book.
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
            // Arrange: Set up authors to be returned by _authorService.
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Act: Call the GET Create action.
            var result = await _sut.Create();

            // Assert: The result should be of type ViewResult.
            result.Should().BeOfType<ViewResult>("GET Create should return a ViewResult.");

            // Assert: Ensure ViewData["AuthorId"] is a SelectList containing authors.
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var selectList = viewResult.ViewData["AuthorId"] as SelectList;
            selectList.Should().NotBeNull("ViewData['AuthorId'] should be a SelectList.");

            // Assert: Use CollectionAssertions on SelectList.Items to check the count.
            var selectListItems = selectList.Items.Cast<object>().ToList();
            selectListItems.Should().HaveCount(authors.Count, "SelectList should contain the correct number of authors.");
        }

        [Test]
        public async Task Create_Post_ValidModelState_Redirects_To_Index()
        {
            // Arrange: Create a valid BookDto.
            var bookDto = _fixture.Create<BookDto>();
            var fileMock = new Mock<IFormFile>();  // Mocking file input (if needed).

            // Arrange: Mock _authorService and _bookService.
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(new List<Author>());
            _bookServiceMock.Setup(s => s.CreateBook(It.IsAny<BookDto>())).Returns(Task.CompletedTask);

            // Act: Call the POST Create action with a valid BookDto.
            var result = await _sut.Create(bookDto, fileMock.Object);

            // Assert: Ensure the result is a redirect to the Index action.
            result.Should().BeOfType<RedirectToActionResult>("After a successful create, user should be redirected to the Index action.");
            var redirectResult = result as RedirectToActionResult;
            redirectResult?.ActionName.Should().Be(nameof(_sut.Index), "The action should redirect to Index.");
        }

        [Test]
        public async Task Create_Post_InvalidModelState_Returns_ViewResult_With_BookDto()
        {
            // Arrange: Create an invalid BookDto (e.g., with invalid data).
            var bookDto = _fixture.Create<BookDto>();
            bookDto.Title = null!;  // Make the model invalid, for example, if Title is a required field.

            // Manually mark the ModelState as invalid
            _sut.ModelState.AddModelError("Title", "Title is required.");

            // Act: Call the POST Create action with the invalid BookDto.
            var result = await _sut.Create(bookDto, null!);  // No file provided here.

            // Assert: Ensure the result is a ViewResult, meaning the form has validation errors.
            result.Should().BeOfType<ViewResult>("If the model state is invalid, the action should return a ViewResult with validation errors.");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            // Assert: Ensure that ViewData contains the authors and the model passed back is the same.
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
            // Arrange: Create a BookDto with invalid data.
            var bookDto = _fixture.Create<BookDto>();
            bookDto.Title = null!;  // Null Title should trigger validation error.

            // Setup valid authors for the SelectList.
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Act: Call the POST Edit action with invalid BookDto (invalid model state).
            _sut.ModelState.AddModelError("Title", "Title is required.");
            var result = await _sut.Edit(bookDto.Id, bookDto);

            // Assert: Ensure the result returns the View with the model.
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.Model.Should().Be(bookDto);
        }


        [Test]
        public async Task Edit_Post_ConcurrencyError_ReturnsViewWithModel()
        {
            // Arrange: Create a valid BookDto.
            var bookDto = _fixture.Create<BookDto>();

            // Setup valid authors for the SelectList.
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Simulate a concurrency exception during the update.
            _bookServiceMock.Setup(s => s.UpdateBook(It.IsAny<BookDto>()))
                            .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act: Call the POST Edit action with the valid BookDto.
            var result = await _sut.Edit(bookDto.Id, bookDto);

            // Assert: Ensure the result returns the View with the model.
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.Model.Should().Be(bookDto);
        }


        [Test]
        public async Task Edit_Post_IdMismatch_Returns_NotFound()
        {
            // Arrange: Create a valid BookDto with a mismatched id.
            var bookDto = _fixture.Create<BookDto>();
            bookDto.Id = _fixture.Create<int>();  // Set some id.
            int id = _fixture.Create<int>();  // Different id to simulate the mismatch.

            // Act: Call the POST Edit action with mismatched id and BookDto.
            var result = await _sut.Edit(id, bookDto);

            // Assert: Ensure the result is a NotFoundResult when the ids don't match.
            result.Should().BeOfType<NotFoundResult>("When the ids don't match, the action should return NotFound.");
        }



        [Test]
        public async Task Edit_Post_ValidModelState_Redirects_To_Index()
        {
            // Arrange: Create a valid BookDto.
            var bookDto = _fixture.Create<BookDto>();

            // Setup valid authors for the SelectList.
            var authors = _fixture.CreateMany<Author>().ToList();
            _authorServiceMock.Setup(s => s.GetAuthors()).ReturnsAsync(authors);

            // Act: Call the POST Edit action with the valid BookDto.
            var result = await _sut.Edit(bookDto.Id, bookDto);

            // Assert: Ensure the result is a RedirectToActionResult to the Index action.
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
            var webRootPath = "C:\\TestWebRoot";  // Ensure WebRootPath is not null
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            var filePath = "Files/testfile.txt";
            var rootPath = Path.Combine(webRootPath, filePath);

            // Ensure directory exists before creating the file
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
            var webRootPath = "C:\\TestWebRoot";  // Ensure WebRootPath is set
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
            var webRootPath = "C:\\TestWebRoot";  // Ensure WebRootPath is set
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            var rootPath = Path.Combine(webRootPath, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(rootPath) ?? throw new ArgumentNullException(nameof(rootPath)));
            await File.WriteAllTextAsync(rootPath, "Test content");

            // Lock the file to simulate an IOException
            using (var stream = new FileStream(rootPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                var result = await _sut.DeleteFile($"/{filePath}");

                // Assert
                result.Should().BeOfType<OkResult>();
                File.Exists(rootPath).Should().BeTrue("The file should still exist because it was locked and could not be deleted");
            }

            // Cleanup: Unlock and delete file
            File.Delete(rootPath);
        }

        [Test]
        public void UploadFile_WhenFileUploaded_SavesFileAndReturnsFileInfo()
        {
            // Arrange
            var path = Guid.NewGuid().ToString(); // Unique folder name for isolation
            var fileName = "testfile.jpg";          // Original file name (only extension is used)
            var fileContent = "Sample file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", fileName);
            var formFileCollection = new FormFileCollection { formFile };

            // Set up HttpContext and Request to provide form files
            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Form.Files).Returns(formFileCollection);
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

            // Assuming _sut is your controller instance:
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };

            // Set up a temporary web root folder to simulate the environment
            var testWebRoot = Path.Combine(Path.GetTempPath(), "TestWebRoot");
            Directory.CreateDirectory(testWebRoot);
            _webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(testWebRoot);

            // Act
            var result = _sut.UploadFile(path);

            // Assert
            result.Should().BeOfType<OkObjectResult>("we expect an OkObjectResult if the file upload is successful");
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("The result should be of type OkObjectResult.");

            // Use reflection to get the FileAddress property from the anonymous object
            var valueType = okResult.Value.GetType();
            var fileAddressProperty = valueType.GetProperty("FileAddress");
            fileAddressProperty.Should().NotBeNull("The returned object should have a 'FileAddress' property.");
            string fileAddress = fileAddressProperty.GetValue(okResult.Value)?.ToString();
            fileAddress.Should().NotBeNullOrEmpty("FileAddress should not be null or empty.");

            fileAddress.Should().StartWith($"/Files/{path}/", "the file address should be under the correct folder");
            fileAddress.Should().EndWith(".jpg", "the file extension should be preserved");

            // Verify that the generated file name (without extension) is a valid GUID.
            var savedFileName = Path.GetFileName(fileAddress); // e.g., "6d976e75-f9e5-40d2-bcba-8f902beb954b.jpg"
            var guidPart = Path.GetFileNameWithoutExtension(savedFileName);
            Guid.TryParse(guidPart, out Guid guid).Should().BeTrue("the file name should be a valid GUID");

            // Verify that the file was indeed saved to disk.
            var expectedFolderPath = Path.Combine(testWebRoot, "Files", path);
            var savedFilePath = Path.Combine(expectedFolderPath, savedFileName);
            File.Exists(savedFilePath).Should().BeTrue("the uploaded file should be saved on disk");

            // Cleanup: remove the temporary test directory
            Directory.Delete(testWebRoot, recursive: true);
        }


    }
}