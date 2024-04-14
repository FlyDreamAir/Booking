using FlyDreamAir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

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
            return RedirectToAction(
                "/Account/LoginWith2fa",
                new { ReturnUrl = returnUrl, IsPersistent = isPersistent });
        }
        else if (result.IsLockedOut)
        {
            return Redirect("/Account/Lockout");
        }
        else
        {
            return BadRequest("Invalid login.");
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
        [EmailAddress]
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
            return BadRequest(ModelState.First().Value);
        }

        var user = new ApplicationUser();

        await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
        await ((IUserEmailStore<ApplicationUser>)_userStore)
            .SetEmailAsync(user, email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.First().Description);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = $"{Request.Scheme}://" +
            $"{Request.Host.ToUriComponent()}" +
            $"api/Account/{nameof(ConfirmEmail)}" +
            $"?userId={Uri.EscapeDataString(userId)}" +
            $"&code={Uri.EscapeDataString(code)}" +
            $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

        await _emailSender.SendConfirmationLinkAsync(user, email,
            HtmlEncoder.Default.Encode(callbackUrl));

        if (_userManager.Options.SignIn.RequireConfirmedAccount)
        {
            return RedirectToAction("/Account/RegisterConfirmation",
                new { Email = email, ReturnUrl = returnUrl });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return Redirect(returnUrl);
    }

    [HttpPost(nameof(ExternalLogin))]
    public ActionResult ExternalLogin(
        HttpContext context,
        [FromQuery]
        string provider,
        [FromForm]
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

        var redirectUrl = UriHelper.BuildRelative(
            context.Request.PathBase,
            nameof(ExternalCallback),
            QueryString.Create(query));

        var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, [provider]);
    }

    [HttpGet(nameof(ExternalCallback))]
    [Authorize]
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
            return BadRequest("Invalid login.");
        }

        switch (action)
        {
            case LoginCallbackAction:
            {
                var result = await _signInManager.ExternalLoginSignInAsync(
                    externalLoginInfo.LoginProvider,
                    externalLoginInfo.ProviderKey,
                    isPersistent,
                    bypassTwoFactor: true);

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

                return RedirectToAction("/Account/ExternalLogin",
                    new { Email = email, UserName = userName, ReturnUrl = returnUrl });
            }
            default:
                return BadRequest("Invalid action.");
        }
    }

    [HttpGet(nameof(ConfirmEmail))]
    public async Task<ActionResult> ConfirmEmail(
        [FromQuery]
        string userId,
        [FromQuery]
        string code,
        [FromQuery]
        string? returnUrl
    )
    {
        returnUrl ??= "/";

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return BadRequest("Invalid confirmation link.");
        }
        else
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return BadRequest(result.Errors.First().Description);
            }
        }
    }
}
