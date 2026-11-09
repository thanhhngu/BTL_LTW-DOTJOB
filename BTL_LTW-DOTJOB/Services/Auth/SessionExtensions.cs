using System.Text.Json;

namespace BTL_LTW_DOTJOB.Services.Auth
{
    public static class SessionExtensions
    {
        // Khóa dùng chung, tương đương Session["User"] trong ASP.NET cũ
        public const string UserKey = "User";

        // Lưu thông tin người dùng vào Session: object -> chuỗi JSON
        public static void SetUser(this ISession session, SessionUser user)
        {
            session.SetString(UserKey, JsonSerializer.Serialize(user));
        }

        // Đọc thông tin người dùng từ Session: chuỗi JSON -> object.
        // Trả về null nếu chưa đăng nhập hoặc Session đã hết hạn.
        public static SessionUser? GetUser(this ISession session)
        {
            var json = session.GetString(UserKey);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<SessionUser>(json);
        }

        // Xóa Session khi đăng xuất
        public static void ClearUser(this ISession session)
        {
            session.Remove(UserKey);
        }
    }
}
