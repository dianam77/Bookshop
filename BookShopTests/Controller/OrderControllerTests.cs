using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Bookshop.Models;
using Core.OrderService;
using DataAccess.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;


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
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockOrderService = new Mock<IOrderService>();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "userId123")
    };
            var identity = new ClaimsIdentity(claims, "mock");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

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
            // Arrange
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
            // Arrange
            var model = _fixture.Create<AddBasketDto>();
            var userId = "userId123";

            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            _mockOrderService.Setup(s => s.AddToBasket(model.bookId, model.qty, userId)).ReturnsAsync(true);

            // Act
            var result = await _sut.AddToBasket(model);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = true });
        }

        [Test]
        public async Task Basket_Returns_Unauthorized_WhenUserIsNotAuthorized()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity();  
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            // Act
            var result = await _sut.Basket();

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Account"); 
        }



        [Test]
        public async Task AddToBasket_Returns_Error_WhenUserIsNotLoggedIn()
        {
            // Arrange
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();

            mockClaimsPrincipal.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null!);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(mockClaimsPrincipal.Object);

            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            // Act
            var model = _fixture.Create<AddBasketDto>();
            var result = await _sut.AddToBasket(model);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = false, msg = "شما لاگین نکرده اید" });
        }


        [Test]
        public async Task RemoveBasket_Returns_Success_WhenItemIsRemoved()
        {
            // Arrange
            var model = _fixture.Create<RemoveBasketDto>();

            _mockOrderService.Setup(s => s.RemoveItemBasket(model.Id)).ReturnsAsync(true);

            // Act
            var result = await _sut.RemoveBasket(model);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = true });
        }

        [Test]
        public async Task RemoveBasket_Returns_Failure_WhenRemovalFails()
        {
            // Arrange
            var model = _fixture.Create<RemoveBasketDto>();

            _mockOrderService.Setup(s => s.RemoveItemBasket(model.Id)).ReturnsAsync(false);

            // Act
            var result = await _sut.RemoveBasket(model);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { res = false });
        }
    }
}
