using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace FlyDreamAir.Utils;

public static class ControllerExtensions
{
    public static RedirectResult RedirectWithQuery(this ControllerBase controller,
        string returnUrl, IEnumerable<KeyValuePair<string, object?>> args)
    {
        return controller.Redirect(QueryHelpers.AddQueryString(returnUrl, args.Select((kvp) =>
        {
            return new KeyValuePair<string, string?>(kvp.Key, kvp.Value switch
            {
                bool b => b ? "true" : "false",
                _ => kvp.Value.ToString()
            });
        })));
    }
}
