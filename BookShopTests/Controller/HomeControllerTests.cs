using System.Linq;
using System.Reflection;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Core.BookService;  // Assuming IBookService is defined here
using Core.ServiceFile;  // Assuming IFileService is defined here
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class HomeControllerTests
    {
        private IFixture _fixture;
        private Mock<ILogger<HomeController>> _mockLogger;
        private Mock<IBookService> _mockBookService;
        private Mock<IFileService> _mockFileService;
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            // Initialize AutoFixture with AutoMoq customization.
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Remove the default recursion behavior and add one that omits recursion.
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Freeze dependencies so that every request for these types returns the same instance.
            _mockLogger = _fixture.Freeze<Mock<ILogger<HomeController>>>();
            _mockBookService = _fixture.Freeze<Mock<IBookService>>();
            _mockFileService = _fixture.Freeze<Mock<IFileService>>();

            // Create the system under test (SUT).
            _sut = new HomeController(_mockLogger.Object, _mockBookService.Object, _mockFileService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose of the SUT if necessary (for example, if it implements IDisposable)
            _sut?.Dispose();
        }

        [Test]
        public void Index_Returns_ViewResult()
        {
            // Act
            var result = _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void AboutUs_Returns_ViewResult()
        {
            // Act
            var result = _sut.AboutUs();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void AboutUs_Has_AuthorizeAttribute()
        {
            // Arrange
            MethodInfo? methodInfo = typeof(HomeController).GetMethod(nameof(HomeController.AboutUs));

            // Act
            var attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

            // Assert
            attributes.Should().NotBeNull();
            attributes.Should().NotBeEmpty("AboutUs should have the [Authorize] attribute applied.");
        }
    }
}




