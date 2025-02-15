using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Core.OrderService;      
using DataAccess.Models;       
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors
                    .OfType<ThrowingRecursionBehavior>()
                    .ToList()
                    .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockOrderService = _fixture.Freeze<Mock<IOrderService>>();

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
            // Arrange
            string userId = _fixture.Create<string>();
            SetUpAuthenticatedUser(userId);

            var basketItems = _fixture.CreateMany<BasketItems>(3)
                                      .Cast<BasketItems?>()
                                      .ToList();

            _mockOrderService.Setup(s => s.GetUserBasket(userId))
                             .ReturnsAsync(basketItems);

            decimal totalPrice = _fixture.Create<decimal>();

            // Act
            var result = await _sut.Index(totalPrice);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<Payment>()
                      .Which.Amount.Should().Be(totalPrice);
        }

        [Test]
        public async Task ProcessPayment_Should_RedirectToProfile_WhenModelStateIsValid_AndPaymentSucceeds()
        {
            // Arrange
            string userId = _fixture.Create<string>();
            SetUpAuthenticatedUser(userId);

            var payment = _fixture.Create<Payment>();

            _mockOrderService.Setup(s => s.MarkBasketItemsAsPaid(userId))
                             .ReturnsAsync(true);

            // Act
            var result = await _sut.ProcessPayment(payment);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Profile");

            _mockOrderService.Verify(s => s.MarkBasketItemsAsPaid(userId), Times.Once);
        }

        [Test]
        public async Task ProcessPayment_Should_Return_IndexView_WithPaymentModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var payment = _fixture.Create<Payment>();

            _sut.ModelState.AddModelError("TestError", "Error");

            // Act
            var result = await _sut.ProcessPayment(payment);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().BeEquivalentTo(payment);
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
