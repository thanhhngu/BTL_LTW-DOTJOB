using Ganss.Xss;
using Microsoft.AspNetCore.Html;

// Đặt trong namespace này để mọi view dùng được @Html.SafeHtml(...) mà không cần thêm @using
namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class SafeHtmlExtensions
    {
        // Hiển thị HTML do người dùng soạn bằng editor (mô tả công việc, yêu cầu, giới thiệu công ty...).
        // Luôn làm sạch lại (xóa <script>, onerror=..., javascript:...) trước khi xuất ra trang,
        // thay cho @Html.Raw(...) trần, phòng khi dữ liệu trong DB không đi qua bước làm sạch lúc lưu
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
