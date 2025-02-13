using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Bookshop.Models;         // Contains the Payment model.
using Core.OrderService;       // Contains IOrderService.
using DataAccess.Models;       // Contains Basket.
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class PaymentControllerTests
    {
        private IFixture _fixture;
        private Mock<IOrderService> _mockOrderService;
        private PaymentController _sut;

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

            // Freeze the IOrderService mock so all dependencies use the same instance.
            _mockOrderService = _fixture.Freeze<Mock<IOrderService>>();

            // Create the PaymentController (System Under Test).
            _sut = new PaymentController(_mockOrderService.Object);
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
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public void Index_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task Index_Post_Should_Return_ViewResult_WithPaymentModel_WhenUserIsAuthenticated()
        {
            // Arrange: Set up a user with a NameIdentifier claim.
            string userId = _fixture.Create<string>();
            SetUpAuthenticatedUser(userId);

            // Set up the IOrderService to return basket items.
            var basketItems = _fixture.CreateMany<BasketItems>(3)
                                      .Cast<BasketItems?>()
                                      .ToList();

            _mockOrderService.Setup(s => s.GetUserBasket(userId))
                             .ReturnsAsync(basketItems);

            // Arrange: Create a total price.
            decimal totalPrice = _fixture.Create<decimal>();

            // Act: Call the POST Index action with totalPrice.
            var result = await _sut.Index(totalPrice);

            // Assert: The returned view should have a Payment model with the correct Amount.
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<Payment>()
                      .Which.Amount.Should().Be(totalPrice);
        }

        [Test]
        public async Task ProcessPayment_Should_RedirectToProfile_WhenModelStateIsValid_AndPaymentSucceeds()
        {
            // Arrange: Set up a user with a NameIdentifier claim.
            string userId = _fixture.Create<string>();
            SetUpAuthenticatedUser(userId);

            // Arrange: Create a valid Payment object.
            var payment = _fixture.Create<Payment>();

            // Ensure the IOrderService.MarkBasketItemsAsPaid method returns true.
            _mockOrderService.Setup(s => s.MarkBasketItemsAsPaid(userId))
                             .ReturnsAsync(true);

            // Act
            var result = await _sut.ProcessPayment(payment);

            // Assert: Should redirect to the Profile controller's Index action.
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Profile");

            // Verify that MarkBasketItemsAsPaid was called once.
            _mockOrderService.Verify(s => s.MarkBasketItemsAsPaid(userId), Times.Once);
        }

        [Test]
        public async Task ProcessPayment_Should_Return_IndexView_WithPaymentModel_WhenModelStateIsInvalid()
        {
            // Arrange: Create a Payment object.
            var payment = _fixture.Create<Payment>();

            // Force the ModelState to be invalid.
            _sut.ModelState.AddModelError("TestError", "Error");

            // Act
            var result = await _sut.ProcessPayment(payment);

            // Assert: Should return the "Index" view with the same Payment model.
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().BeEquivalentTo(payment);

            // Verify that MarkBasketItemsAsPaid was never called.
            _mockOrderService.Verify(s => s.MarkBasketItemsAsPaid(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Failure_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Failure();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void Success_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Success();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
    }
}
