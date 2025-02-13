using AdminBookShop.Controllers;
using DataAccess.Models;
using DataAccess.Repositories.CommentRepo;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace AdminBookShop.Tests
{
    [TestFixture]
    public class CommentsControllerTests
    {
        private IFixture _fixture = null!;
        private Mock<ICommentRepository> _mockCommentRepository = null!;
        private CommentsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Add OmitOnRecursionBehavior to avoid circular reference issues
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockCommentRepository = _fixture.Freeze<Mock<ICommentRepository>>();
            _sut = new CommentsController(_mockCommentRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose of the controller instance to release resources
            _sut?.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnViewResult_WithAllComments()
        {
            // Arrange
            var comments = _fixture.CreateMany<Comment>(2).ToList();
            _mockCommentRepository.Setup(repo => repo.GetAllComments()).ReturnsAsync(comments);

            // Act
            var result = await _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as List<Comment>;
            model.Should().NotBeNull();
            model.Should().HaveCount(2);
            model[0].Text.Should().Be(comments[0].Text);
            model[1].Text.Should().Be(comments[1].Text);
        }

        [Test]
        public async Task Index_ShouldReturnEmptyList_WhenNoCommentsExist()
        {
            // Arrange
            _mockCommentRepository.Setup(repo => repo.GetAllComments()).ReturnsAsync(new List<Comment>());

            // Act
            var result = await _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as List<Comment>;
            model.Should().NotBeNull();
            model.Should().BeEmpty();
        }

        [Test]
        public async Task Details_ShouldReturnViewResult_WithCommentData()
        {
            // Arrange
            var commentId = 1;
            var comment = _fixture.Build<Comment>()
                .With(c => c.Id, commentId)
                .With(c => c.Text, "Sample Comment")
                .Create();
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync(comment);
            var replies = _fixture.CreateMany<Comment>(2).ToList();
            _mockCommentRepository.Setup(repo => repo.GetRepliesByCommentId(commentId)).ReturnsAsync(replies);

            // Act
            var result = await _sut.Details(commentId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as Comment;
            model.Should().NotBeNull();
            model!.Id.Should().Be(commentId);
            model.Text.Should().Be("Sample Comment");
            model.Replies.Should().HaveCount(2);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var commentId = 999; // assuming this ID does not exist
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId))
                .ReturnsAsync((Comment)null!);

            // Act
            var result = await _sut.Details(commentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Reply_ShouldReturnRedirectToAction_WhenValidReply()
        {
            // Arrange
            var commentId = 1;
            var replyText = "This is a reply.";
            var comment = _fixture.Build<Comment>()
                .With(c => c.Id, commentId)
                .With(c => c.Text, "Original Comment")
                .Create();
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync(comment);

            // Act
            var result = await _sut.Reply(commentId, replyText);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>()
            .Which;

            redirectResult.ActionName.Should().Be("Details");
            redirectResult.RouteValues.Should().NotBeNull();
            redirectResult.RouteValues["id"].Should().Be(commentId);

        }

        [Test]
        public async Task Reply_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var commentId = 999; // This ID should not exist in the mocked repository
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync((Comment?)null);

            // Act
            var result = await _sut.Reply(commentId, "This is a reply.");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_ShouldReturnViewResult_WithCorrectCommentData()
        {
            // Arrange
            var commentId = 1;
            var comment = _fixture.Build<Comment>()
                .With(c => c.Id, commentId)
                .With(c => c.Text, "Editable Comment")
                .Create();

            // Mocking repository to return a valid comment
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId))
                .ReturnsAsync(comment);  // Ensure the repository returns a comment for the given ID

            // Act
            var result = await _sut.Edit(commentId);

            // Debugging to check what type is returned
            Console.WriteLine($"Returned result type: {result.GetType()}");  // Should be ViewResult

            // Assert
            result.Should().BeOfType<ViewResult>();  // Expecting a ViewResult
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as Comment;
            model.Should().NotBeNull();
            model!.Id.Should().Be(commentId);
            model.Text.Should().Be("Editable Comment");
        }


        [Test]
        public async Task Edit_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var commentId = 999; // assuming this ID does not exist
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync((Comment?)null);

            // Act
            var result = await _sut.Edit(commentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_ShouldReturnBadRequest_WhenInvalidCommentData()
        {
            // Arrange
            var commentId = 1;
            var comment = new Comment { Id = commentId, Text = string.Empty }; // Invalid model
            _sut.ModelState.AddModelError("Text", "Required");

            // Act
            var result = await _sut.Edit(commentId, comment);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.ViewData?.ModelState?.GetValueOrDefault("Text")?.Errors.Should().ContainSingle();
        }

        [Test]
        public async Task Delete_ShouldReturnViewResult_WhenCommentExists()
        {
            // Arrange
            var commentId = 1;
            var comment = _fixture.Build<Comment>()
                .With(c => c.Id, commentId)
                .With(c => c.Text, "Comment to be deleted")
                .Create();
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync(comment);

            // Act
            var result = await _sut.Delete(commentId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as Comment;
            model.Should().NotBeNull();
            model!.Id.Should().Be(commentId);
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var commentId = 999; // assuming this ID does not exist
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync((Comment?)null);

            // Act
            var result = await _sut.Delete(commentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task DeleteConfirmed_ShouldReturnRedirectToAction_WhenCommentDeleted()
        {
            // Arrange
            var commentId = 1;
            var comment = _fixture.Build<Comment>()
                .With(c => c.Id, commentId)
                .Create();
            _mockCommentRepository.Setup(repo => repo.GetCommentById(commentId)).ReturnsAsync(comment);
            _mockCommentRepository.Setup(repo => repo.Delete(commentId)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteConfirmed(commentId);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }
    }
}