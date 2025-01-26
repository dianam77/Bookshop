using AdminBookShop.Controllers;
using AdminBookShop.Models;
using Core.OrderService;
using Core.OrderService.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;


namespace AdminBookShop.Tests
{
    [TestFixture]
    public class OrdersControllerTests
    {
        private Mock<IOrderService> _mockOrderService = null!;
        private OrdersController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _mockOrderService = new Mock<IOrderService>();
            _sut = new OrdersController(_mockOrderService.Object); // Initializing the controller (SUT)
        }

        [Test]
        public async Task Index_ShouldReturnViewResult_WithAllOrders()
        {
            // Arrange
            var orders = new List<AdmiOrderDto>
            {
                new AdmiOrderDto { Id = 1, UserId = "user1", UserName = "John Doe", Status = DataAccess.Enums.Status.Final },
                new AdmiOrderDto { Id = 2, UserId = "user2", UserName = "Jane Doe", Status = DataAccess.Enums.Status.Accepted }
            };
            _mockOrderService.Setup(service => service.GetAllOrders()).ReturnsAsync(orders);

            // Act
            var result = await _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as List<AdmiOrderDto>;
            model.Should().NotBeNull();
            model!.Should().HaveCount(2);
            model[0].UserName.Should().Be("John Doe");
            model[1].UserName.Should().Be("Jane Doe");
        }

        [Test]
        public async Task Index_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetAllOrders()).ReturnsAsync(new List<AdmiOrderDto>());

            // Act
            var result = await _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult!.Model as List<AdmiOrderDto>;
            model.Should().NotBeNull();
            model!.Should().BeEmpty();
        }

        [Test]
        public async Task Index_ShouldHandleError_WhenServiceThrowsException()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetAllOrders()).ThrowsAsync(new Exception("Database Error"));

            // Act
            var result = await _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.ViewName.Should().Be("Error");
        }

        [Test]
        public async Task Edit_ShouldReturnViewResult_WithCorrectOrderData()
        {
            // Arrange
            var orderId = 1;
            var order = new AdmiOrderDto
            {
                Id = orderId,
                UserId = "user1",
                UserName = "John Doe",
                Status = DataAccess.Enums.Status.Final,
                Address = "123 Main St"
            };
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
            model.UserName.Should().Be("John Doe");
        }

        [Test]
        public async Task Edit_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 999; // assuming this ID does not exist

            // Ensure null is returned for the mock setup, explicitly specifying the nullable type.
            _mockOrderService.Setup(service => service.GetOrderWithId(orderId))
                             .ReturnsAsync((AdmiOrderDto?)null); // Ensure null is returned as nullable AdmiOrderDto

            // Act
            var result = await _sut.Edit(orderId); // SUT under test

            // Assert
            result.Should().BeOfType<NotFoundResult>(); // Assert that a NotFoundResult is returned
        }


        [Test]
        public async Task Edit_ShouldHandleError_WhenServiceThrowsException()
        {
            // Arrange
            var orderId = 1;
            _mockOrderService.Setup(service => service.GetOrderWithId(orderId)).ThrowsAsync(new Exception("Database Error"));

            // Act
            var result = await _sut.Edit(orderId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult?.ViewName.Should().Be("Error");
        }

        [Test]
        public async Task SetStatusCommand_ShouldReturnOkResult_WithTrueResponse()
        {
            // Arrange
            var statusDto = new StatusDto { Id = 1, State = true };
            _mockOrderService.Setup(service => service.SetStatus(statusDto.Id, statusDto.State)).ReturnsAsync(true);

            // Act
            var result = await _sut.SetStatusCommand(statusDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(new { res = true });
        }

        [Test]
        public async Task SetStatusCommand_ShouldReturnOkResult_WithFalseResponse()
        {
            // Arrange
            var statusDto = new StatusDto { Id = 1, State = false };
            _mockOrderService.Setup(service => service.SetStatus(statusDto.Id, statusDto.State)).ReturnsAsync(false);

            // Act
            var result = await _sut.SetStatusCommand(statusDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(new { res = false });
        }

        [Test]
        public async Task SetStatusCommand_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var invalidStatusDto = new StatusDto { Id = 0, State = true }; // Invalid Id
            _sut.ModelState.AddModelError("Id", "Required");

            // Act
            var result = await _sut.SetStatusCommand(invalidStatusDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.Value.Should().BeEquivalentTo(new { error = "Invalid model" });
        }

        [Test]
        public async Task SetStatusCommand_ShouldHandleError_WhenServiceThrowsException()
        {
            // Arrange
            var statusDto = new StatusDto { Id = 1, State = true };
            _mockOrderService.Setup(service => service.SetStatus(statusDto.Id, statusDto.State)).ThrowsAsync(new Exception("Service Error"));

            // Act
            var result = await _sut.SetStatusCommand(statusDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.Value.Should().BeEquivalentTo(new { error = "Service Error" });
        }
    }
}
