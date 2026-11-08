using System.Text.Json;

namespace BTL_LTW_DOTJOB.Helpers
{
    // Thông tin người đăng nhập lưu trong Session["User"]
    public class SessionUser
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public static class SessionUserExtensions
    {
        public const string UserKey = "User";

        // Session chỉ lưu được chuỗi/byte nên đổi đối tượng sang JSON
        public static void SetUser(this ISession session, SessionUser user)
        {
            session.SetString(UserKey, JsonSerializer.Serialize(user));
        }

        public static SessionUser? GetUser(this ISession session)
        {
            var json = session.GetString(UserKey);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SessionUser>(json);
        }
    }
}
