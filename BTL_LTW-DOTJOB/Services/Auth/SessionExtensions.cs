using System.Text.Json;

namespace BTL_LTW_DOTJOB.Services.Auth
{
    public static class SessionExtensions
    {
        public const string UserKey = "User";

        public static void SetUser(this ISession session, SessionUser user)
        {
            session.SetString(UserKey, JsonSerializer.Serialize(user));
        }

        public static SessionUser? GetUser(this ISession session)
        {
            var json = session.GetString(UserKey);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<SessionUser>(json);
        }

        public static void ClearUser(this ISession session)
        {
            session.Remove(UserKey);
        }
    }
}
