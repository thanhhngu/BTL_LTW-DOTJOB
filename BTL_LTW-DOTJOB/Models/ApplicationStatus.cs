namespace BTL_LTW_DOTJOB.Models
{
    public static class ApplicationStatus
    {
        public const string Pending = "Pending";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";

        public static readonly string[] All = { Pending, Accepted, Rejected };

        public static string Label(string status) => status switch
        {
            Accepted => "Hồ sơ phù hợp",
            Rejected => "Không phù hợp",
            _ => "Chờ duyệt"
        };

        public static string BadgeClass(string status) => status switch
        {
            Accepted => "text-bg-success",
            Rejected => "text-bg-danger",
            _ => "text-bg-secondary"
        };
    }
}
