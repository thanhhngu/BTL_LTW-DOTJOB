namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    // Một dòng trong danh sách tin của HR: chỉ gồm các cột cần hiển thị.
    // Số CV được đếm ngay trong câu truy vấn, không tải cả danh sách đơn ứng tuyển lên bộ nhớ
    public class JobListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }
        public int ApplicationCount { get; set; }
    }
}
