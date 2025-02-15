using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Core.OrderService;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Moq;
using DataAccess.Models;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class ProfileControllerTests
    {
        private IFixture _fixture = null!;
        private Mock<IOrderService> _mockOrderService = null!;
        private ProfileController _sut = null!;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors
                    .OfType<ThrowingRecursionBehavior>()
                    .ToList()
                    .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockOrderService = _fixture.Freeze<Mock<IOrderService>>();

            _sut = new ProfileController(_mockOrderService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (_sut != null)
            {
                _sut.Dispose();
            }
        }

        private void SetUpAuthenticatedUser(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task Index_Should_RedirectToLogin_When_UserIdIsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _sut.Index();

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Account");

            _mockOrderService.Verify(service => service.GetUserOrders(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Index_Should_ReturnViewResult_WithOrders_When_UserIdIsNotNull()
        {
            // Arrange
            string userId = _fixture.Create<string>();
            SetUpAuthenticatedUser(userId);

            var orders = _fixture.CreateMany<Basket>(3)
                                 .Cast<Basket?>()
                                 .ToList();

            _mockOrderService.Setup(service => service.GetUserOrders(userId))
                             .ReturnsAsync(orders);

            // Act
            var result = await _sut.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(orders);
            _mockOrderService.Verify(service => service.GetUserOrders(userId), Times.Once);
        }

        [Test]
        public void ProfileController_Should_Have_AuthorizeAttribute()
        {
            // Arrange & Act
            var attributes = typeof(ProfileController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

            // Assert
            attributes.Should().NotBeEmpty("ProfileController should be decorated with the [Authorize] attribute.");
        }
    }
}
