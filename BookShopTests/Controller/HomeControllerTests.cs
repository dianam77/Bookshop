using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Core.BookService;  
using Core.ServiceFile; 
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

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
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockLogger = _fixture.Freeze<Mock<ILogger<HomeController>>>();
            _mockBookService = _fixture.Freeze<Mock<IBookService>>();
            _mockFileService = _fixture.Freeze<Mock<IFileService>>();

            _sut = new HomeController(_mockLogger.Object, _mockBookService.Object, _mockFileService.Object);
        }

        [TearDown]
        public void TearDown()
        {
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




