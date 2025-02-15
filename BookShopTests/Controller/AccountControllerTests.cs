using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Bookshop.Models;            
using DataAccess.Models;          
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookShopTests.Controller
{
    [TestFixture]
    public class AccountControllerTests
    {
        private IFixture _fixture = null!;
        private AccountController _sut = null!;
        private Mock<UserManager<User>> _mockUserManager = null!;
        private Mock<SignInManager<User>> _mockSignInManager = null!;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                httpContextAccessor.Object,
                userClaimsPrincipalFactory.Object,
                null!, null!, null!, null!);

            _sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
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
        public void Register_Get_Returns_ViewResult()
        {
            // Act
            var result = _sut.Register();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task RegisterPostReturnsRedirectToHomeWhenRegistrationSucceeds()
        {
            // Arrange
            var model = _fixture.Create<RegisterDto>();

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Register(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }

        [Test]
        public async Task RegisterPostReturnsViewWithModelWhenRegistrationFails()
        {
            // Arrange
            var model = _fixture.Create<RegisterDto>();

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _sut.Register(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(model);
        }

        [Test]
        public void Login_Get_Returns_ViewResult()
        {
            // Act
            var result = _sut.Login();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task Register_Post_Returns_RedirectToHome_WhenRegistrationSucceeds()
        {
            // Arrange
            var model = _fixture.Create<RegisterDto>();

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);  

            _mockSignInManager.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Register(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }



        [Test]
        public async Task Login_Post_AddsError_WhenUserNotFound()
        {
            // Arrange
            var model = _fixture.Create<LoginViewModel>();

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email.ToUpper()))
                .ReturnsAsync((User)null!);

            // Act
            var result = await _sut.Login(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(model);
            _sut.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        

        [Test]
        public async Task Register_Post_Returns_View_WithModel_WhenRegistrationFails()
        {
            // Arrange
            var model = _fixture.Create<RegisterDto>();

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Failed(new Microsoft.AspNetCore.Identity.IdentityError { Description = "Error" }));  // Fully qualify here

            // Act
            var result = await _sut.Register(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(model);
        }


        [Test]
        public async Task Logout_Returns_RedirectToHome()
        {
            // Arrange
            _mockSignInManager.Setup(sm => sm.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Logout();

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }
    }
}
