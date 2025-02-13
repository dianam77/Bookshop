using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Core.OrderService; // Assumes IOrderService and Order are defined here.
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
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
            // Initialize AutoFixture with AutoMoq customization.
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Replace the default recursion behavior with one that omits recursion.
            _fixture.Behaviors
                    .OfType<ThrowingRecursionBehavior>()
                    .ToList()
                    .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Freeze the IOrderService mock so that all requests get the same instance.
            _mockOrderService = _fixture.Freeze<Mock<IOrderService>>();

            // Create the ProfileController (System Under Test).
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
        // Removed TearDown as it's unnecessary unless ProfileController implements IDisposable.

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
            // Arrange: Create a ClaimsPrincipal without a NameIdentifier claim.
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _sut.Index();

            // Assert: Should redirect to the "Login" action on the "Account" controller.
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Account");

            // Verify that GetUserOrders was never called.
            _mockOrderService.Verify(service => service.GetUserOrders(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Index_Should_ReturnViewResult_WithOrders_When_UserIdIsNotNull()
        {
            // Arrange: Create a ClaimsPrincipal with a NameIdentifier claim.
            string userId = _fixture.Create<string>();
            SetUpAuthenticatedUser(userId);

            // Arrange: Create a list of orders (using the Basket type) and cast it to List<Basket?>.
            var orders = _fixture.CreateMany<Basket>(3)
                                 .Cast<Basket?>()
                                 .ToList();

            _mockOrderService.Setup(service => service.GetUserOrders(userId))
                             .ReturnsAsync(orders);

            // Act
            var result = await _sut.Index();

            // Assert: Verify the result is a ViewResult containing the expected orders.
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(orders);
            _mockOrderService.Verify(service => service.GetUserOrders(userId), Times.Once);
        }

        [Test]
        public void ProfileController_Should_Have_AuthorizeAttribute()
        {
            // Arrange & Act: Get the custom attributes applied to the controller.
            var attributes = typeof(ProfileController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

            // Assert: The controller should be decorated with the [Authorize] attribute.
            attributes.Should().NotBeEmpty("ProfileController should be decorated with the [Authorize] attribute.");
        }
    }
}
