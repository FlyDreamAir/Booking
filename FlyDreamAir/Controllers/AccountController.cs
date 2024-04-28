using FlyDreamAir.Data;
using FlyDreamAir.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

namespace FlyDreamAir.Controllers;

[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private const string LoginCallbackAction = "LoginCallback";

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IEmailSender<ApplicationUser> _emailSender;


    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        IEmailSender<ApplicationUser> emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailSender = emailSender;
    }

    [HttpPost(nameof(Login))]
    public async Task<ActionResult> Login(
        [FromForm]
        string userName,
        [FromForm]
        string password,
        [FromForm]
        bool isPersistent,
        [FromQuery]
        string returnUrl
    )
    {
        var result = await _signInManager.PasswordSignInAsync(userName, password, isPersistent, false);

        if (result.Succeeded)
        {
            return Redirect(returnUrl);
        }
        else if (result.RequiresTwoFactor)
        {
            return this.RedirectWithQuery("/Account/LoginWith2fa",
                new Dictionary<string, object?>()
                {
                    { nameof(returnUrl), returnUrl },
                    { nameof(isPersistent), isPersistent }
                });
        }
        else if (result.IsLockedOut)
        {
            return Redirect("/Account/Lockout");
        }
        else
        {
            return this.RedirectWithQuery("/Account/Login",
                new Dictionary<string, object?>() {
                    { nameof(returnUrl), returnUrl },
                    { "error", "Invaild username or password." }
                });
        }
    }

    [HttpPost(nameof(Logout))]
    [Authorize]
    public async Task<ActionResult> Logout(
        [FromQuery]
        string? returnUrl
    )
    {
        await _signInManager.SignOutAsync();
        return Redirect(returnUrl ?? "/");
    }

    [HttpPost(nameof(Register))]
    public async Task<ActionResult> Register(
        [FromForm]
        [Required]
        string userName,
        [FromForm]
        [Required]
        [EmailAddress(ErrorMessage = "The email is invalid.")]
        string email,
        [FromForm]
        [Required]
        [StringLength(int.MaxValue,
            ErrorMessage = "The {0} must be at least {2} characters long.",
            MinimumLength = 8)]
        string password,
        [FromQuery]
        string returnUrl
    )
    {
        if (!ModelState.IsValid)
        {
            return this.RedirectWithQuery("/Account/Register",
                new Dictionary<string, object?>()
                {
                    { nameof(returnUrl), returnUrl },
                    {
                        "error",
                        ModelState.First(
                            ms => ms.Value?.ValidationState == ModelValidationState.Invalid
                        ).Value!.Errors.First().ErrorMessage
                    },
                }
            );
        }

        var user = new ApplicationUser();

        await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
        await ((IUserEmailStore<ApplicationUser>)_userStore)
            .SetEmailAsync(user, email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return this.RedirectWithQuery("/Account/Register",
                new Dictionary<string, object?>()
                {
                    { nameof(returnUrl), returnUrl },
                    { "error", result.Errors.First().Description },
                }
            );
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = $"{Request.Scheme}://" +
            $"{Request.Host.ToUriComponent()}" +
            $"/Account/ConfirmEmail" +
            $"?userId={Uri.EscapeDataString(userId)}" +
            $"&code={Uri.EscapeDataString(code)}" +
            $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

        await _emailSender.SendConfirmationLinkAsync(user, email, callbackUrl);

        if (_userManager.Options.SignIn.RequireConfirmedAccount)
        {
            return this.RedirectWithQuery("/Account/RegisterConfirmation",
                new Dictionary<string, object?>() {
                    { nameof(returnUrl), returnUrl },
                    { nameof(email), email }
                });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return Redirect(returnUrl);
    }

    [HttpGet(nameof(ExternalLogin))]
    public ActionResult ExternalLogin(
        [FromQuery]
        string provider,
        [FromQuery]
        bool isPersistent,
        [FromQuery]
        string returnUrl
    )
    {
        var query = new Dictionary<string, string?> {
            { nameof(returnUrl), returnUrl },
            { nameof(isPersistent), isPersistent.ToString().ToLowerInvariant() },
            { "action", LoginCallbackAction }
        };

        var redirectUrl = $"{Request.Scheme}://" +
            $"{Request.Host.ToUriComponent()}" +
            $"/api/Account/{nameof(ExternalCallback)}" +
            QueryString.Create(query);

        var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, [provider]);
    }

    [HttpGet(nameof(ExternalCallback))]
    public async Task<ActionResult> ExternalCallback(
        [FromQuery]
        string action,
        [FromQuery]
        bool isPersistent,
        [FromQuery]
        string returnUrl
    )
    {
        var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo is null)
        {
            return this.RedirectWithQuery("/Account/Login",
                new Dictionary<string, object?>()
                {
                    { nameof(returnUrl), returnUrl },
                    { "error", "Failed to sign in with the selected provider." }
                });
        }

        switch (action)
        {
            case LoginCallbackAction:
            {
                var result = await _signInManager.ExternalLoginSignInAsync(
                    externalLoginInfo.LoginProvider,
                    externalLoginInfo.ProviderKey,
                    isPersistent,
                    bypassTwoFactor: true
                );

                if (result.Succeeded)
                {
                    return Redirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    return Redirect("/Account/Lockout");
                }

                var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
                var userName = email[0..Math.Min(email.Length, email.IndexOf('@'))];
                var providerDisplayName = externalLoginInfo.ProviderDisplayName;

                return this.RedirectWithQuery("/Account/ExternalLogin",
                    new Dictionary<string, object?>() {
                        { nameof(email), email },
                        { nameof(userName), userName },
                        { nameof(providerDisplayName), providerDisplayName },
                        { nameof(returnUrl), returnUrl }
                    }
                );
            }
            default:
                return BadRequest("Invalid action.");
        }
    }

    [HttpPost(nameof(ExternalRegister))]
    public async Task<ActionResult> ExternalRegister(
        [FromForm]
        [EmailAddress(ErrorMessage = "The email is invalid.")]
        string email,
        [FromForm]
        string userName,
        [FromQuery]
        string returnUrl
    )
    {
        var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo is null)
        {
            return this.RedirectWithQuery("/Account/Login",
                new Dictionary<string, object?>()
                {
                    { nameof(returnUrl), returnUrl },
                    { "error", "Failed to sign in with the selected provider." }
                }
            );
        }

        var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
        var user = Activator.CreateInstance<ApplicationUser>();

        await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
            result = await _userManager.AddLoginAsync(user, externalLoginInfo);
            if (result.Succeeded)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = $"{Request.Scheme}://" +
                    $"{Request.Host.ToUriComponent()}" +
                    $"/Account/ConfirmEmail" +
                    $"?userId={Uri.EscapeDataString(userId)}" +
                    $"&code={Uri.EscapeDataString(code)}" +
                    $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

                await _emailSender.SendConfirmationLinkAsync(user, email, callbackUrl);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return this.RedirectWithQuery("/Account/RegisterConfirmation",
                        new Dictionary<string, object?>() {
                            { nameof(returnUrl), returnUrl },
                            { nameof(email), email }
                        });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnUrl);
            }
        }

        return this.RedirectWithQuery("/Account/ExternalLogin",
            new Dictionary<string, object?>()
            {
                { nameof(returnUrl), returnUrl },
                { nameof(email), email },
                { nameof(userName), userName },
                {
                    "error",
                    string
                        .Join("; ", result.Errors.Select(error => error.Description))
                }
            }
        );
    }

    [HttpPost(nameof(ConfirmEmail))]
    public async Task<ActionResult> ConfirmEmail(
        [FromForm]
        string userId,
        [FromForm]
        string code,
        [FromQuery]
        string? returnUrl
    )
    {
        returnUrl ??= "/";

        try
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("Invalid user ID.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return this.RedirectWithQuery("/Account/Login",
                    new Dictionary<string, object?>()
                    {
                        { nameof(returnUrl), returnUrl }
                    }
                );
            }
            else
            {
                return this.RedirectWithQuery("/Account/ConfirmEmail",
                    new Dictionary<string, object?>()
                    {
                        { nameof(returnUrl), returnUrl },
                        { "error", result.Errors.First().Description },
                    }
                );
            }
        }
        catch
        {
            return this.RedirectWithQuery("/Account/ConfirmEmail",
                new Dictionary<string, object?>()
                {
                    { nameof(returnUrl), returnUrl },
                    { "error", "Invalid confirmation link." },
                }
            );
        }
    }
}
