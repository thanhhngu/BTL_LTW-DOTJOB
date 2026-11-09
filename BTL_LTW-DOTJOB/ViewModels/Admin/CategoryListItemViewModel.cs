namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Một dòng trong bảng danh sách danh mục
    public class CategoryListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int JobCount { get; set; } // Số tin tuyển dụng đang thuộc danh mục này
    }
}