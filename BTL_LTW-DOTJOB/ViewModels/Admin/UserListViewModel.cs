namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    public class UserListViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new();

        public string? Keyword { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}