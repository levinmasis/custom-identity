using custom_identity.Data.Entities;
using custom_identity.Extensions;
using custom_identity.Infrastructure.Messaging;
using custom_identity.Models;
using custom_identity.Models.Partials;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace custom_identity.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<AuthController> logger,
            IUserStore<User> userStore,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _userStore = userStore;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> ManageAsync()
        {
            await Task.Run(() => { }); // TODO : Delete statement

            var selectedTab = ViewBag.SelectedTab;

            return View();
        }

        public async Task<IActionResult> LoginAsync(string? returnUrl = null)
        {
            var model = new LoginModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = new Microsoft.AspNetCore.Identity.SignInResult();
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                if (model.Username.Contains("@"))
                {
                    var user = await _userManager.FindByEmailAsync(model.Username);
                    result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                }
                else
                {
                    result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(model.ReturnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult Lockout()
        {
            return View();
        }

        public async Task<IActionResult> LogoutAsync(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null) return LocalRedirect(returnUrl);

            return RedirectToAction(string.Empty, string.Empty);
        }

        public async Task<IActionResult> RegisterAsync(string? returnUrl = null)
        {
            var model = new RegisterModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                };

                var mailAddress = new MailAddress(model.Email);
                await _userStore.SetUserNameAsync(user, mailAddress.User, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var callbackUrl = await GenerateLink(user, "auth", "confirmemail", TokenTypes.EMAIL);

                    var emailMessage = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicking here</a>.";

                    await _emailSender.SendAsync(model.Email, "Confirm your email", emailMessage);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email, returnUrl = model.ReturnUrl, message = "Error: Please verify your email." });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(model.ReturnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public IActionResult RegisterConfirmation(string email, string returnUrl, string? message = null)
        {
            var model = new RegisterConfirmationModel() { StatusMessage = message };

            return View(model);
        }

        private async Task<string> GenerateLink(User user, string callbackController, string callbackAction, TokenTypes tokenType)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            string? code = null;
            switch(tokenType)
            {
                case TokenTypes.EMAIL:
                    code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    break;
                case TokenTypes.PASSWORDRESET:
                    code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    break;
            }
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code ?? string.Empty));
            var callbackUrl = Url.ActionLink(callbackAction, callbackController, 
                new { userId = userId, code = code }, protocol: Request.Scheme);

            return callbackUrl ?? string.Empty;
        }

        public async Task<IActionResult> ResendConfirmationEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new RegisterConfirmationModel() {  };

            if (user != null)
            {
                var callbackUrl = await GenerateLink(user, "auth", nameof(ConfirmEmailAsync), TokenTypes.EMAIL);

                var emailMessage = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicking here</a>.";

                try
                {
                    await _emailSender.SendAsync(user.Email ?? string.Empty, "Confirm your email", emailMessage);

                    model.StatusMessage = $"Email message sent successfully. ";

                }
                catch (Exception ex)
                {
                    model.StatusMessage = $"Error. {ex.Message}. ";
                }
            }
            else
            {
                model.StatusMessage += $"Error occured while processing request. {user} not found.";
            }

            return View(nameof(RegisterConfirmation), model);
        }

        public async Task<IActionResult> ConfirmEmailAsync(string? userId = null, string? code = null)
        {
            if (userId == null || code == null) { return View("Error"); }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var model = new ConfirmEmailModel();

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            model.StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return View(model);
        }

        public async Task<IActionResult> ForgotPasswordAsync()
        {
            await Task.Run(() => { }); // TODO : Delete statement

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email)) return BadRequest("Error : Invalid");

                return await ForgotPasswordConfirmationAsync(model.Email); 
            }
            else
            {
                ModelState.AddModelError(model.Email, "An error occured while processing your request. Please try again");

                return View(model);
            }
        }

        public async Task<IActionResult> ForgotPasswordConfirmationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null) 
            {
                var callbackUrl = await GenerateLink(user, "auth", nameof(ResetPassword), TokenTypes.PASSWORDRESET);

                var emailMessage = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicking here</a>.";

                try
                {
                    await _emailSender.SendAsync(user.Email ?? string.Empty, "Confirm your email", emailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error. {ex.Message}. ");
                }

            }
            return View("ForgotPasswordConfirmation");
        }

        public async Task<IActionResult> AccessDeniedAsync()
        {
            await Task.Run(() => { }); // TODO : Delete statement

            return View();
        }

        public async Task<IActionResult> NotFoundAsync()
        {
            await Task.Run(() => { });

            return PartialView("_404");
        }

        public IActionResult ResetPassword(string? userId = null, string? code = null)
        {
            if (string.IsNullOrEmpty(code)) 
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordModel() 
            { Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)) };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "An error occured. Please try again");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "An error occured. Please try again.");
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation), new { successful = true });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public IActionResult ResetPasswordConfirmation(bool successful = false)
        {
            var model = new ResetPasswordConfirmationModel()
            {
                StatusMessage = successful 
                   ? $"Password reset successful. Proceed to login" 
                   : $"Error: Something wrong."
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPersonalDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(User).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString());
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty);

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        public async Task<IActionResult> DeletePersonalDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new DeletePersonalDataPartialModel() 
            {
                RequirePassword = await _userManager.HasPasswordAsync(user)
            };


            return PartialView("Auth/_DeletePersonalDataPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePersonalDataAsync(DeletePersonalDataPartialModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            model.RequirePassword = await _userManager.HasPasswordAsync(user);
            if (model.RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return PartialView("Auth/_DeletePersonalDataPartial", model);
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}
