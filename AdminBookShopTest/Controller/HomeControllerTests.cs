using AdminBookShop.Controllers;
using Core.OrderService;
using Moq;
using FluentAssertions;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Mvc;
using Core.OrderService.Model;

namespace AdminBookShop.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private IFixture _fixture = null!;
        private Mock<IOrderService> _mockOrderService = null!;
        private HomeController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockOrderService = _fixture.Freeze<Mock<IOrderService>>();
            _sut = new HomeController(_mockOrderService.Object);
        }

        [Test]
        public async Task Delete_ShouldReturnViewResult_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var order = _fixture.Build<AdmiOrderDto>()
                .With(o => o.Id, orderId)
                .Create();
            _mockOrderService.Setup(service => service.GetOrderWithId(orderId)).ReturnsAsync(order);

            // Act
            var result = await _sut.Delete(orderId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as AdmiOrderDto;
            model.Should().NotBeNull();
            model!.Id.Should().Be(orderId);
        }

        [Test]
        public async Task DeleteConfirmed_ShouldReturnRedirectToAction_WhenOrderDeleted()
        {
            // Arrange
            var orderId = 1;
            var order = _fixture.Build<AdmiOrderDto>()
                .With(o => o.Id, orderId)
                .Create();
            _mockOrderService.Setup(service => service.GetOrderWithId(orderId)).ReturnsAsync(order);
            _mockOrderService.Setup(service => service.DeleteOrder(orderId)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteConfirmed(orderId);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Test]
        public async Task Edit_ShouldReturnViewResult_WithOrderData()
        {
            // Arrange
            var orderId = 1;
            var order = _fixture.Build<AdmiOrderDto>()
                .With(o => o.Id, orderId)
                .With(o => o.UserName, "Test User")
                .Create();
            _mockOrderService.Setup(service => service.GetOrderWithId(orderId)).ReturnsAsync(order);

            // Act
            var result = await _sut.Edit(orderId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as AdmiOrderDto;
            model.Should().NotBeNull();
            model!.Id.Should().Be(orderId);
            model.UserName.Should().Be("Test User");
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }


        [Test]
        public async Task Edit_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var orderId = 1;
            var order = new AdmiOrderDto { Id = orderId, UserName = "" }; 
            _sut.ModelState.AddModelError("UserName", "Required");

            // Act
            var result = await _sut.Edit(orderId, order);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.ViewData?.ModelState?.GetValueOrDefault("UserName")?.Errors.Should().ContainSingle();
        }
    }
}