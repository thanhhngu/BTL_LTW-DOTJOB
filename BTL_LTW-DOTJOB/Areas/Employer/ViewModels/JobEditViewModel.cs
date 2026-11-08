namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    // Form sửa tin giống hệt form tạo tin, chỉ thêm Id để biết đang sửa tin nào.
    // Kế thừa nên dùng lại toàn bộ DataAnnotations và hàm Validate của JobCreateViewModel
    public class JobEditViewModel : JobCreateViewModel
    {
        public string Id { get; set; } = string.Empty;
    }
}
