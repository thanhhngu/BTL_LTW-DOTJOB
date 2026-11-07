namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    // Candidate lọc theo đúng chuỗi lưu trong DB, nên HR chọn từ danh sách thay vì nhập tự do
    public static class JobOptions
    {
        public static readonly string[] JobTypes =
        {
            "Toàn thời gian", "Bán thời gian", "Thực tập", "Hợp đồng", "Làm việc từ xa"
        };

        // Viết đúng như dữ liệu seed ("TP. Hồ Chí Minh" có dấu chấm)
        public static readonly string[] Locations =
        {
            "Hà Nội", "TP. Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ",
            "Bắc Ninh", "Đồng Nai", "Khánh Hòa"
        };
    }
}
