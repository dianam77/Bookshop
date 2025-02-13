using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.ServiceFile;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class FileDisplayControllerTests
    {
        private IFixture _fixture;  // AutoFixture instance
        private Mock<IFileService> _mockFileService;
        private FileDisplayController _Sut;

        [SetUp]
        public void Setup()
        {
            // Initialize AutoFixture with AutoMoq customization to automatically create mocks
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Create the mock using AutoFixture
            _mockFileService = _fixture.Freeze<Mock<IFileService>>();

            // Instantiate the controller with the mocked IFileService
            _Sut = new FileDisplayController(_mockFileService.Object);
        }

        [Test]
        public async Task DisplayFile_Should_Return_File_When_File_Is_Found()
        {
            // Arrange
            var fileName = "sample.txt";
            var mockMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Sample file content"));

            // Setup the mock to return a MemoryStream for the file
            _mockFileService.Setup(x => x.DownloadFileAsync(It.Is<string>(s => s == fileName)))
                            .ReturnsAsync(mockMemoryStream);

            // Act
            var result = await _Sut.DisplayFile(fileName);

            // Assert
            var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
            fileResult.ContentType.Should().Be("application/octet-stream");
            fileResult.FileDownloadName.Should().Be(fileName);

            using var resultStream = new MemoryStream();
            await fileResult.FileStream.CopyToAsync(resultStream);

            // Verify that the content is the same as the mock data
            resultStream.ToArray().Should().BeEquivalentTo(mockMemoryStream.ToArray());
        }


        [Test]
        public async Task DisplayFile_Should_Return_NotFound_When_File_Is_Not_Found()
        {
            // Arrange
            var fileName = "nonexistentfile.txt";

            // Setup the mock to throw a FileNotFoundException
            _mockFileService.Setup(x => x.DownloadFileAsync(It.IsAny<string>()))
                            .ThrowsAsync(new FileNotFoundException());

            // Act
            var result = await _Sut.DisplayFile(fileName);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("File not found in source project.");
        }

        [Test]
        public async Task DisplayFile_Should_Return_InternalServerError_When_Unexpected_Error_Occurs()
        {
            // Arrange
            var fileName = "sample.txt";

            // Setup the mock to throw a general exception for unexpected errors
            _mockFileService.Setup(x => x.DownloadFileAsync(It.IsAny<string>()))
                            .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _Sut.DisplayFile(fileName);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("An unexpected error occurred.");
        }

        // Edge case: Test with an empty file name
        [Test]
        public async Task DisplayFile_Should_Return_BadRequest_When_File_Name_Is_Empty()
        {
            // Arrange
            var fileName = string.Empty;

            // Act
            var result = await _Sut.DisplayFile(fileName);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("File name cannot be empty.");
        }

        // Edge case: Test with an invalid file name (e.g., file with illegal characters)
        [Test]
        public async Task DisplayFile_Should_Return_BadRequest_When_File_Name_Is_Invalid()
        {
            // Arrange
            var fileName = "<invalid:file>name.txt";

            // Act
            var result = await _Sut.DisplayFile(fileName);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid file name.");
        }
    }
}
