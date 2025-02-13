using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Bookshop.Controllers;
using Bookshop.Models;             // Contains RegisterDto, LoginViewModel, Payment, etc.
using DataAccess.Models;           // Contains User
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

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
            // Optionally use AutoFixture with AutoMoq customization.
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Create a dummy IUserStore<User> (required to create a UserManager).
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);

            // Create the SignInManager. We need an IHttpContextAccessor and IUserClaimsPrincipalFactory<User>.
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                httpContextAccessor.Object,
                userClaimsPrincipalFactory.Object,
                null!, null!, null!, null!);

            // Create the AccountController (system under test).
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
            // Arrange: Create a RegisterDto model.
            var model = _fixture.Create<RegisterDto>();

            // Setup the UserManager to return success when creating a user.
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Setup the SignInManager to simulate a successful sign-in.
            _mockSignInManager.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Register(model);

            // Assert: Expect a redirect to the Home controller's Index action.
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }

        [Test]
        public async Task RegisterPostReturnsViewWithModelWhenRegistrationFails()
        {
            // Arrange: Create a RegisterDto model.
            var model = _fixture.Create<RegisterDto>();

            // Setup the UserManager to return a failed result.
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _sut.Register(model);

            // Assert: Expect the same view with the model returned.
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
            // Arrange: Create a RegisterDto model.
            var model = _fixture.Create<RegisterDto>();

            // Setup the UserManager to return success when creating a user.
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);  // Fully qualify here

            // Setup the SignInManager to simulate a successful sign-in.
            _mockSignInManager.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Register(model);

            // Assert: Expect a redirect to the Home controller's Index action.
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }



        [Test]
        public async Task Login_Post_AddsError_WhenUserNotFound()
        {
            // Arrange: Create a LoginViewModel.
            var model = _fixture.Create<LoginViewModel>();

            // Setup FindByEmailAsync to return null (user not found).
            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email.ToUpper()))
                .ReturnsAsync((User)null!);

            // Act
            var result = await _sut.Login(model);

            // Assert: Expect the view returned with the model and ModelState error.
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(model);
            _sut.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        

        [Test]
        public async Task Register_Post_Returns_View_WithModel_WhenRegistrationFails()
        {
            // Arrange: Create a RegisterDto model.
            var model = _fixture.Create<RegisterDto>();

            // Setup the UserManager to return a failed result.
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Failed(new Microsoft.AspNetCore.Identity.IdentityError { Description = "Error" }));  // Fully qualify here

            // Act
            var result = await _sut.Register(model);

            // Assert: Expect the same view with the model returned.
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(model);
        }


        [Test]
        public async Task Logout_Returns_RedirectToHome()
        {
            // Arrange: Setup SignOutAsync.
            _mockSignInManager.Setup(sm => sm.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Logout();

            // Assert: Expect a redirect to Home/Index.
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }
    }
}
