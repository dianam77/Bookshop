using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Bookshop.Models;
using Core.OrderService;
using DataAccess.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class OrderControllerTests
    {
        private IFixture _fixture = null!;
        private OrderController _sut = null!;
        private Mock<IOrderService> _mockOrderService = null!;
        private Mock<HttpContext> _mockHttpContext = null!;
        private Mock<ClaimsPrincipal> _mockClaimsPrincipal = null!;

        [SetUp]
        public void Setup()
        {
            // Optionally use AutoFixture with AutoMoq customization.
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Remove the ThrowingRecursionBehavior and add OmitOnRecursionBehavior to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Mocking the IOrderService
            _mockOrderService = new Mock<IOrderService>();

            // Create a ClaimsPrincipal with a user identity
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "userId123")
    };
            var identity = new ClaimsIdentity(claims, "mock");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create a mock HttpContext and assign the ClaimsPrincipal
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            // Create the OrderController (system under test).
            _sut = new OrderController(_mockOrderService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }


        [TearDown]
        public void TearDown()
        {
            if (_sut != null)
            {
                _sut.Dispose();
            }
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
        public async Task Basket_Returns_View_WithUserBasket_WhenUserIsAuthorized()
        {
            // Arrange: Create a basket model and mock the IOrderService
            var basketItems = _fixture.CreateMany<BasketItems?>(3).ToList();
            _mockOrderService.Setup(s => s.GetUserBasket(It.IsAny<string>())).ReturnsAsync(basketItems);


            // Act
            var result = await _sut.Basket();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(basketItems);
        }

        [Test]
        public async Task AddToBasket_Returns_Success_WhenItemIsAdded()
        {
            // Arrange: Create an AddBasketDto and simulate the userId
            var model = _fixture.Create<AddBasketDto>();
            var userId = "userId123";

            // Create a ClaimsPrincipal with a valid NameIdentifier
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Mock HttpContext and set the User property to the mock ClaimsPrincipal
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            // Initialize the controller with the mocked HttpContext
            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            // Simulate a successful result from the AddToBasket method
            _mockOrderService.Setup(s => s.AddToBasket(model.bookId, model.qty, userId)).ReturnsAsync(true);

            // Act: Call the AddToBasket method
            var result = await _sut.AddToBasket(model);

            // Assert: Expect a 200 OK with the result being true
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = true });
        }

        [Test]
        public async Task Basket_Returns_Unauthorized_WhenUserIsNotAuthorized()
        {
            // Arrange: Create a ClaimsPrincipal with no NameIdentifier (unauthenticated user)
            var claimsIdentity = new ClaimsIdentity();  // No claims, means the user is not logged in
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Mock HttpContext and set the User property to the mock ClaimsPrincipal
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            // Initialize the controller with the mocked HttpContext
            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            // Act: Call the Basket method
            var result = await _sut.Basket();

            // Assert: Expect a redirect to the login page
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Account"); // Ensure it's redirecting to the Account controller
        }



        [Test]
        public async Task AddToBasket_Returns_Error_WhenUserIsNotLoggedIn()
        {
            // Arrange: Mock an unauthenticated user (null for userId)
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();

            // Instead of mocking FindFirstValue, mock the User property to simulate a non-authenticated user
            mockClaimsPrincipal.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null!);

            // Mock HttpContext and set the User property to the mock ClaimsPrincipal
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(mockClaimsPrincipal.Object);

            // Initialize the controller with the mocked HttpContext
            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            // Act: Create an AddBasketDto and call the AddToBasket method
            var model = _fixture.Create<AddBasketDto>();
            var result = await _sut.AddToBasket(model);

            // Assert: Expect a 200 OK with the error message indicating the user is not logged in
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = false, msg = "شما لاگین نکرده اید" });
        }


        [Test]
        public async Task RemoveBasket_Returns_Success_WhenItemIsRemoved()
        {
            // Arrange: Create a RemoveBasketDto
            var model = _fixture.Create<RemoveBasketDto>();

            // Simulate a successful removal
            _mockOrderService.Setup(s => s.RemoveItemBasket(model.Id)).ReturnsAsync(true);

            // Act
            var result = await _sut.RemoveBasket(model);

            // Assert: Expect a 200 OK with the result being true
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = true });
        }

        [Test]
        public async Task RemoveBasket_Returns_Failure_WhenRemovalFails()
        {
            // Arrange: Create a RemoveBasketDto
            var model = _fixture.Create<RemoveBasketDto>();

            // Simulate a failed removal
            _mockOrderService.Setup(s => s.RemoveItemBasket(model.Id)).ReturnsAsync(false);

            // Act
            var result = await _sut.RemoveBasket(model);

            // Assert: Expect a 200 OK with the result being false
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = false });
        }
    }
}
