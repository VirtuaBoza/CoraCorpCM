﻿using CoraCorpCM.Common.Membership;
using CoraCorpCM.Common.Tests;
using CoraCorpCM.Web.Controllers;
using CoraCorpCM.Application.Interfaces.Infrastructure;
using CoraCorpCM.Web.ViewModels.ManageViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoraCorpCM.Web.Services.Shared;
using System;
using CoraCorpCM.Web.ExtensionWrappers;
using CoraCorpCM.Web.Services.Manage;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using System.Linq;

namespace CoraCorpCM.Web.Tests
{
    [TestClass]
    public class ManageControllerTests
    {
        ManageController controller;

        Mock<UserManager<ApplicationUser>> mockUserManager;
        Mock<SignInManager<ApplicationUser>> mockSignInManager;
        Mock<IEmailSender> mockEmailSender;
        Mock<ILogger<ManageController>> mockLogger;
        Mock<ICallbackUrlCreator> mockCallbackUrlCreator;
        Mock<IAuthenticationHttpContextExtensionsWrapper> mockHttpContextExtensionsWrapper;
        Mock<IUrlHelperExtensionsWrapper> mockUrlHelperExtensionsWrapper;
        Mock<ITwoFactorAuthenticationViewModelFactory> mockTwoFactorAuthenticationViewModelFactory;
        Mock<IEnableAuthenticatorViewModelFactory> mockEnableAuthenticatorViewModelFactory;

        ApplicationUser user;

        [TestInitialize]
        public void SetUp()
        {
            user = new ApplicationUser { Id = "1" };
            mockUserManager = CommonMockHelper.GetMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            mockSignInManager = WebMockHelper.GetMockSignInManager();

            mockEmailSender = new Mock<IEmailSender>();

            mockLogger = new Mock<ILogger<ManageController>>();

            mockCallbackUrlCreator = new Mock<ICallbackUrlCreator>();
            mockCallbackUrlCreator
                .Setup(c => c.CreateEmailConfirmationLink(It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("callbackUrl");

            mockHttpContextExtensionsWrapper = new Mock<IAuthenticationHttpContextExtensionsWrapper>();

            mockUrlHelperExtensionsWrapper = new Mock<IUrlHelperExtensionsWrapper>();

            mockTwoFactorAuthenticationViewModelFactory = new Mock<ITwoFactorAuthenticationViewModelFactory>();

            mockEnableAuthenticatorViewModelFactory = new Mock<IEnableAuthenticatorViewModelFactory>();

            controller = new ManageController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockEmailSender.Object, 
                mockLogger.Object, 
                mockCallbackUrlCreator.Object,
                mockHttpContextExtensionsWrapper.Object,
                mockUrlHelperExtensionsWrapper.Object,
                mockTwoFactorAuthenticationViewModelFactory.Object,
                mockEnableAuthenticatorViewModelFactory.Object);

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Scheme).Returns("scheme");
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
        }

        [TestMethod]
        public async Task ChangePassword_OnGet_WhenUserHasPassword_ReturnsViewResultWithChangePasswordViewModel()
        {
            // Arrange
            mockUserManager.Setup(um => um.HasPasswordAsync(user)).ReturnsAsync(true);

            // Act
            var result = await controller.ChangePassword() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(ChangePasswordViewModel));
        }

        [TestMethod]
        public async Task ChangePassword_OnGet_UnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.ChangePassword());
        }

        [TestMethod]
        public async Task ChangePassword_OnGet_WhenUserDoesNotHavePassword_ReturnsRedirectToActionResult()
        {
            // Arrange
            mockUserManager.Setup(um => um.HasPasswordAsync(user)).ReturnsAsync(false);

            // Act
            var result = await controller.ChangePassword();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task ChangePassword_OnGet_WhenUserDoesNotHavePassword_RedirectToSetPassword()
        {
            // Arrange
            mockUserManager.Setup(um => um.HasPasswordAsync(user)).ReturnsAsync(false);

            // Act
            var result = await controller.ChangePassword() as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.SetPassword), result.ActionName);
        }

        [TestMethod]
        public async Task ChangePassword_OnPost_ReturnsRedirectToActionResult()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel();
            mockUserManager.Setup(um => um.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.ChangePassword(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task ChangePassword_OnPost_WithInvalidModel_ReturnsViewResult()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel();
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.ChangePassword(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task ChangePassword_OnPost_UnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel();
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.ChangePassword(viewModel));
        }

        [TestMethod]
        public async Task ChangePassword_OnPost_WhenChangePasswordFails_AddErrorsToModelState()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel();
            var identityError = new IdentityError { Code = "something", Description = "Something" };
            mockUserManager.Setup(um => um.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            await controller.ChangePassword(viewModel);

            // Assert
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public async Task ChangePassword_OnPost_WhenChangePasswordFails_ReturnsViewResult()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel();
            var identityError = new IdentityError { Code = "something", Description = "Something" };
            mockUserManager.Setup(um => um.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await controller.ChangePassword(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task ChangePassword_OnPost_WhenChangePasswordFails_ReturnsViewResultWithChangePasswordViewModel()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel();
            var identityError = new IdentityError { Code = "something", Description = "something" };
            mockUserManager.Setup(um => um.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await controller.ChangePassword(viewModel) as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(ChangePasswordViewModel));
        }

        [TestMethod]
        public async Task Disable2fa_ReturnsRedirectToActionResult()
        {
            // Arrange
            mockUserManager.Setup(um => um.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.Disable2fa();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task Disable2fa_RedirectsToTwoFactorAuthentication()
        {
            // Arrange
            mockUserManager.Setup(um => um.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.Disable2fa() as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.TwoFactorAuthentication), result.ActionName);
        }

        [TestMethod]
        public async Task Disable2fa_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Disable2fa());
        }

        [TestMethod]
        public async Task Disable2fa_GivenFailureToDisable2fa_ThrowsApplicationException()
        {
            // Arrange
            mockUserManager.Setup(um => um.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Failed());

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Disable2fa());
        }

        [TestMethod]
        public async Task Disable2faWarning_ReturnsViewResultWithDisable2faView()
        {
            // Arrange
            user.TwoFactorEnabled = true;

            // Act
            var result = await controller.Disable2faWarning() as ViewResult;

            // Assert
            Assert.AreEqual(nameof(controller.Disable2fa), result.ViewName);
        }

        [TestMethod]
        public async Task Disable2faWarning_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Disable2faWarning());
        }

        [TestMethod]
        public async Task Disable2faWarning_GivenUsersTwoFactorWasDisabled_ThrowsApplicationException()
        {
            // Arrange
            user.TwoFactorEnabled = false;

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Disable2faWarning());
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnGet_ReturnsViewResultWithEnableAuthenticatorViewModel()
        {
            // Arrange
            mockUserManager.Setup(um => um.GetAuthenticatorKeyAsync(user)).ReturnsAsync("key");
            mockEnableAuthenticatorViewModelFactory.Setup(vmf => vmf.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(new EnableAuthenticatorViewModel());

            // Act
            var result = await controller.EnableAuthenticator() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(EnableAuthenticatorViewModel));
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnGet_UnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.EnableAuthenticator());
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnGet_WithNoAuthenticatorKey_ResetsAuthenticatorKey()
        {
            // Arrange

            // Act
            var result = await controller.EnableAuthenticator();

            // Assert
            mockUserManager.Verify(um => um.ResetAuthenticatorKeyAsync(user));
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_ReturnsRedirectToActionResult()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            viewModel.Code = "code";
            mockUserManager.Setup(um => um.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await controller.EnableAuthenticator(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_RedirectsToGenerateRecoveryCodes()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            viewModel.Code = "code";
            mockUserManager.Setup(um => um.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await controller.EnableAuthenticator(viewModel) as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.GenerateRecoveryCodes), result.ActionName);
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_SetsTwoFactorEnabled()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            viewModel.Code = "code";
            mockUserManager.Setup(um => um.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await controller.EnableAuthenticator(viewModel);

            // Assert
            mockUserManager.Verify(um => um.SetTwoFactorEnabledAsync(user, true));
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_WithInvalidModel_ReturnsViewResultWithSameViewModel()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.EnableAuthenticator(viewModel) as ViewResult;

            // Assert
            Assert.AreSame(viewModel, result.Model);
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_UnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.EnableAuthenticator(viewModel));
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_WithInvalidToken_ReturnsViewResultWithSameViewModel()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            viewModel.Code = "code";
            mockUserManager.Setup(um => um.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await controller.EnableAuthenticator(viewModel) as ViewResult;

            // Assert
            Assert.AreSame(viewModel, result.Model);
        }

        [TestMethod]
        public async Task EnableAuthenticator_OnPost_WithInvalidToken_AddsErrorToModelState()
        {
            // Arrange
            var viewModel = new EnableAuthenticatorViewModel();
            viewModel.Code = "code";
            mockUserManager.Setup(um => um.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await controller.EnableAuthenticator(viewModel);

            // Assert
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public async Task ExternalLogins_ReturnsViewResultWithExternalLoginsViewModel()
        {
            // Arrange
            IList<UserLoginInfo> logins = new List<UserLoginInfo>();
            mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync(logins);

            // Act
            var result = await controller.ExternalLogins() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(ExternalLoginsViewModel));
        }

        [TestMethod]
        public async Task ExternalLogins_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.ExternalLogins());
        }

        [TestMethod]
        public async Task ExternalLogins_GivenOtherLoginsExit_SetsOtherLogins()
        {
            // Arrange
            IList<UserLoginInfo> logins = new List<UserLoginInfo>();
            mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync(logins);
            IEnumerable<AuthenticationScheme> auths = new List<AuthenticationScheme>
            {
                new AuthenticationScheme("name",null,typeof(IAuthenticationHandler))
            };
            mockSignInManager.Setup(sm => sm.GetExternalAuthenticationSchemesAsync()).ReturnsAsync(auths);

            // Act
            var result = await controller.ExternalLogins() as ViewResult;

            // Assert
            Assert.AreEqual(1, (result.Model as ExternalLoginsViewModel).OtherLogins.Count);
        }

        [TestMethod]
        public async Task ExternalLogins_GivenUserDoesNotHavePasswordOrCurrentLogins_SetsViewModelShowRemoveButtonToFalse()
        {
            // Arrange
            var logins = new List<UserLoginInfo>();
            mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync(logins);

            // Act
            var result = await controller.ExternalLogins() as ViewResult;

            // Assert
            Assert.AreEqual(false, (result.Model as ExternalLoginsViewModel).ShowRemoveButton);
        }

        [TestMethod]
        public async Task ExternalLogins_GivenUserHasPassword_SetsViewModelShowRemoveButtonToTrue()
        {
            // Arrange
            var logins = new List<UserLoginInfo>();
            mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync(logins);
            mockUserManager.Setup(um => um.HasPasswordAsync(user)).ReturnsAsync(true);

            // Act
            var result = await controller.ExternalLogins() as ViewResult;

            // Assert
            Assert.AreEqual(true, (result.Model as ExternalLoginsViewModel).ShowRemoveButton);
        }

        [TestMethod]
        public async Task ExternalLogins_GivenUserHasMultipleCurrentLogins_SetsViewModelShowRemoveButtonToTrue()
        {
            // Arrange
            var logins = new List<UserLoginInfo>
            {
                new UserLoginInfo("provider", "key", "name"),
                new UserLoginInfo("provider", "key", "name")
            };
            mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync(logins);

            // Act
            var result = await controller.ExternalLogins() as ViewResult;

            // Assert
            Assert.AreEqual(true, (result.Model as ExternalLoginsViewModel).ShowRemoveButton);
        }

        [TestMethod]
        public async Task ExternalLogins_SetsViewModelStatusMessageToControllerStatusMessage()
        {
            // Arrange
            var logins = new List<UserLoginInfo>();
            mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync(logins);

            // Act
            var result = await controller.ExternalLogins() as ViewResult;

            // Assert
            Assert.AreSame(controller.StatusMessage, (result.Model as ExternalLoginsViewModel).StatusMessage);
        }

        [TestMethod]
        public async Task GenerateRecoveryCodes_ReturnsViewResultWithGenerateRecoveryCodesViewModel()
        {
            // Arrange
            user.TwoFactorEnabled = true;

            // Act
            var result = await controller.GenerateRecoveryCodes() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(GenerateRecoveryCodesViewModel));
        }

        [TestMethod]
        public async Task GenerateRecoveryCodes_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.GenerateRecoveryCodes());
        }

        [TestMethod]
        public async Task GenerateRecoveryCodes_GivenUsers2faIsDisabled_ThrowsApplicationException()
        {
            // Arrange
            user.TwoFactorEnabled = false;

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.GenerateRecoveryCodes());
        }

        [TestMethod]
        public async Task Index_OnGet_ReturnsViewResultWithIndexViewModel()
        {
            // Arrange

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(IndexViewModel));
        }

        [TestMethod]
        public async Task Index_OnGet_WhenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Index());
        }

        [TestMethod]
        public async Task Index_OnPost_WithValidModel_ReturnsRedirectsToActionResult()
        {
            // Arrange
            var viewModel = new IndexViewModel();

            // Act
            var result = await controller.Index(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task Index_OnPost_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var viewModel = new IndexViewModel();

            // Act
            var result = await controller.Index(viewModel) as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.Index), result.ActionName);
        }

        [TestMethod]
        public async Task Index_OnPost_WithValidModel_SetsStatusMessage()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            controller.StatusMessage = null;

            // Act
            var result = await controller.Index(viewModel);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(controller.StatusMessage));
        }

        [TestMethod]
        public async Task Index_OnPost_WithInvalidModel_ReturnsViewResultWithModel()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.Index(viewModel) as ViewResult;

            // Assert
            Assert.AreSame(viewModel, result.Model);
        }

        [TestMethod]
        public async Task Index_OnPost_WhenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var viewModel = new IndexViewModel();

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Index(viewModel));
        }

        [TestMethod]
        public async Task Index_OnPost_WhenUnableToSetUserEmail_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            viewModel.Email = "someEmail";
            mockUserManager.Setup(um => um.SetEmailAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Index(viewModel));
        }

        [TestMethod]
        public async Task Index_OnPost_WhenUnableToSetUserPhoneNumber_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            viewModel.PhoneNumber = "somePhoneNumber";
            mockUserManager.Setup(um => um.SetPhoneNumberAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.Index(viewModel));
        }

        [TestMethod]
        public async Task LinkLogin_ReturnsChallengeResultWithGivenProvider()
        {
            // Arrange
            var provider = "provider";

            // Act
            var result = await controller.LinkLogin(provider) as ChallengeResult;

            // Assert
            Assert.AreSame(provider, result.AuthenticationSchemes.Single());
        }

        [TestMethod]
        public async Task LinkLogin_ReturnsChallengeResultWithCongifuredProperties()
        {
            // Arrange
            var provider = "provider";
            var properties = new AuthenticationProperties();
            mockSignInManager.Setup(sm => sm.ConfigureExternalAuthenticationProperties(provider, It.IsAny<string>(), It.IsAny<string>())).Returns(properties);

            // Act
            var result = await controller.LinkLogin(provider) as ChallengeResult;

            // Assert
            Assert.AreSame(properties, result.Properties);
        }

        [TestMethod]
        public async Task LinkLoginCallback_ReturnsRedirectToActionResult()
        {
            // Arrange
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "provider", "key", "name");
            mockSignInManager.Setup(sm => sm.GetExternalLoginInfoAsync(It.IsAny<string>())).ReturnsAsync(externalLoginInfo);
            mockUserManager.Setup(um => um.AddLoginAsync(user, It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.LinkLoginCallback();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task LinkLoginCallback_SignsUserOut()
        {
            // Arrange
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "provider", "key", "name");
            mockSignInManager.Setup(sm => sm.GetExternalLoginInfoAsync(It.IsAny<string>())).ReturnsAsync(externalLoginInfo);
            mockUserManager.Setup(um => um.AddLoginAsync(user, It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.LinkLoginCallback();

            // Assert
            mockHttpContextExtensionsWrapper.Verify(w => w.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>()));
        }

        [TestMethod]
        public async Task LinkLoginCallback_SetsStatusMessage()
        {
            // Arrange
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "provider", "key", "name");
            mockSignInManager.Setup(sm => sm.GetExternalLoginInfoAsync(It.IsAny<string>())).ReturnsAsync(externalLoginInfo);
            mockUserManager.Setup(um => um.AddLoginAsync(user, It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);
            controller.StatusMessage = null;

            // Act
            var result = await controller.LinkLoginCallback();

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(controller.StatusMessage));
        }

        [TestMethod]
        public async Task LinkLoginCallback_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.LinkLoginCallback());
        }

        [TestMethod]
        public async Task LinkLoginCallback_GivenErrorLoadingExternalLoginInfo_ThrowsApplicationException()
        {
            // Arrange

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.LinkLoginCallback());
        }

        [TestMethod]
        public async Task LinkLoginCallback_GivenErrorAddingExternalLoginInfo_ThrowsApplicationException()
        {
            // Arrange
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "provider", "key", "name");
            mockSignInManager.Setup(sm => sm.GetExternalLoginInfoAsync(It.IsAny<string>())).ReturnsAsync(externalLoginInfo);
            mockUserManager.Setup(um => um.AddLoginAsync(user, It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Failed());

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.LinkLoginCallback());
        }

        [TestMethod]
        public async Task RemoveLogin_GivenValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var viewModel = new RemoveLoginViewModel();
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.RemoveLogin(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task RemoveLogin_GivenValidModel_RedirectsToExternalLogins()
        {
            // Arrange
            var viewModel = new RemoveLoginViewModel();
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.RemoveLogin(viewModel) as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.ExternalLogins), result.ActionName);
        }

        [TestMethod]
        public async Task RemoveLogin_GivenValidModel_SignsInUser()
        {
            // Arrange
            var viewModel = new RemoveLoginViewModel();
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.RemoveLogin(viewModel) as RedirectToActionResult;

            // Assert
            mockSignInManager.Verify(sm => sm.SignInAsync(user, false, null));
        }

        [TestMethod]
        public async Task RemoveLogin_GivenValidModel_SetsStatusMessage()
        {
            // Arrange
            var viewModel = new RemoveLoginViewModel();
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            controller.StatusMessage = null;

            // Act
            var result = await controller.RemoveLogin(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(controller.StatusMessage));
        }

        [TestMethod]
        public async Task RemoveLogin_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new RemoveLoginViewModel();
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.RemoveLogin(viewModel));
        }

        [TestMethod]
        public async Task RemoveLogin_GivenErrorRemovingLogin_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new RemoveLoginViewModel();
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.RemoveLogin(viewModel));
        }

        [TestMethod]
        public async Task ResetAuthenticator_RedirectsToEnableAuthenticator()
        {
            // Arrange

            // Act
            var result = await controller.ResetAuthenticator() as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.EnableAuthenticator), result.ActionName);
        }

        [TestMethod]
        public async Task ResetAuthenticator_Disables2fa()
        {
            // Arrange

            // Act
            var result = await controller.ResetAuthenticator();

            // Assert
            mockUserManager.Verify(um => um.SetTwoFactorEnabledAsync(user, false));
        }

        [TestMethod]
        public async Task ResetAuthenticator_ResetsAuthenticator()
        {
            // Arrange

            // Act
            var result = await controller.ResetAuthenticator();

            // Assert
            mockUserManager.Verify(um => um.ResetAuthenticatorKeyAsync(user));
        }

        [TestMethod]
        public async Task ResetAuthenticator_GivenToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.ResetAuthenticator());
        }

        [TestMethod]
        public void ResetAuthenticatorWarning_ReturnsResetAuthenticatorView()
        {
            // Arrange

            // Act
            var result = controller.ResetAuthenticatorWarning() as ViewResult;

            // Assert
            Assert.AreEqual(nameof(controller.ResetAuthenticator), result.ViewName);
        }

        [TestMethod]
        public async Task SendVerificationEmail_GivenValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var viewModel = new IndexViewModel();

            // Act
            var result = await controller.SendVerificationEmail(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task SendVerificationEmail_GivenValidModel_RedirectsToIndex()
        {
            // Arrange
            var viewModel = new IndexViewModel();

            // Act
            var result = await controller.SendVerificationEmail(viewModel) as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.Index), result.ActionName);
        }

        [TestMethod]
        public async Task SendVerificationEmail_GivenValidModel_SetsStatusMessage()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            controller.StatusMessage = null;

            // Act
            var result = await controller.SendVerificationEmail(viewModel);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(controller.StatusMessage));
        }

        [TestMethod]
        public async Task SendVerificationEmail_GivenValidModel_SendsEmailConfirmation()
        {
            // Arrange
            var viewModel = new IndexViewModel();

            // Act
            var result = await controller.SendVerificationEmail(viewModel);

            // Assert
            mockEmailSender.Verify(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        public async Task SendVerificationEmail_GivenInvalidValidModel_ReturnsViewResult()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.SendVerificationEmail(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task SendVerificationEmail_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.SendVerificationEmail(viewModel));
        }

        [TestMethod]
        public async Task SetPassword_OnGet_ReturnsViewResultWithSetPasswordViewModel()
        {
            // Arrange

            // Act
            var result = await controller.SetPassword() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(SetPasswordViewModel));
        }

        [TestMethod]
        public async Task SetPassword_OnGet_UnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.SetPassword());
        }

        [TestMethod]
        public async Task SetPassword_OnGet_UserHasPassword_ReturnsRedirectToActionResult()
        {
            // Arrange
            mockUserManager.Setup(um => um.HasPasswordAsync(user)).ReturnsAsync(true);

            // Act
            var result = await controller.SetPassword();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task SetPassword_OnGet_UserHasPassword_RedirectsToChangePassword()
        {
            // Arrange
            mockUserManager.Setup(um => um.HasPasswordAsync(user)).ReturnsAsync(true);

            // Act
            var result = await controller.SetPassword() as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.ChangePassword), result.ActionName);
        }

        [TestMethod]
        public async Task SetPassword_OnPost_WithValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            mockUserManager.Setup(um => um.AddPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.SetPassword(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task SetPassword_OnPost_WithValidModel_RedirectsToSetPassword()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            mockUserManager.Setup(um => um.AddPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.SetPassword(viewModel) as RedirectToActionResult;

            // Assert
            Assert.AreEqual(nameof(controller.SetPassword), result.ActionName);
        }

        [TestMethod]
        public async Task SetPassword_OnPost_WithValidModel_SignsUserIn()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            mockUserManager.Setup(um => um.AddPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.SetPassword(viewModel);

            // Assert
            mockSignInManager.Verify(s => s.SignInAsync(user, false, null));
        }

        [TestMethod]
        public async Task SetPassword_OnPost_WithValidModel_SetsStatusMessage()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            mockUserManager.Setup(um => um.AddPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            controller.StatusMessage = null;

            // Act
            var result = await controller.SetPassword(viewModel);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(controller.StatusMessage));
        }

        [TestMethod]
        public async Task SetPassword_OnPost_WithInvalidModel_ReturnsViewResultWithSameModel()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.SetPassword(viewModel) as ViewResult;

            // Assert
            Assert.AreSame(viewModel, result.Model);
        }

        [TestMethod]
        public async Task SetPassword_OnPost_UnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.SetPassword(viewModel));
        }

        [TestMethod]
        public async Task SetPassword_OnPost_FailsToAddPassword_ReturnsViewResultWithModel()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            var identityError = new IdentityError { Code = "something", Description = "something" };
            mockUserManager.Setup(um => um.AddPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await controller.SetPassword(viewModel) as ViewResult;

            // Assert
            Assert.AreSame(viewModel, result.Model);
        }

        [TestMethod]
        public async Task SetPassword_OnPost_FailsToAddPassword_AddsErrorToModelState()
        {
            // Arrange
            var viewModel = new SetPasswordViewModel();
            var identityError = new IdentityError { Code = "something", Description = "something" };
            mockUserManager.Setup(um => um.AddPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await controller.SetPassword(viewModel);

            // Assert
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public async Task TwoFactorAuthentication_ReturnsViewResultWithTwoFactorAuthenticationViewModel()
        {
            // Arrange
            mockTwoFactorAuthenticationViewModelFactory.Setup(t => t.Create(user)).ReturnsAsync(new TwoFactorAuthenticationViewModel());

            // Act
            var result = await controller.TwoFactorAuthentication() as ViewResult;

            // Assert
            Assert.IsInstanceOfType(result.Model, typeof(TwoFactorAuthenticationViewModel));
        }

        [TestMethod]
        public async Task TwoFactorAuthentication_GivenUnableToLoadUser_ThrowsApplicationException()
        {
            // Arrange
            user = null;
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ApplicationException>(() => controller.TwoFactorAuthentication());
        }
    }
}
