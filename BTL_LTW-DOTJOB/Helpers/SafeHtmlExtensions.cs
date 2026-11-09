using Ganss.Xss;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class SafeHtmlExtensions
    {
        public static IHtmlContent SafeHtml(this IHtmlHelper html, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return HtmlString.Empty;
            }

            return new HtmlString(new HtmlSanitizer().Sanitize(value));
        }
    }
}
