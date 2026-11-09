namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    public class ReportListViewModel
    {
        public List<ReportListItemViewModel> Reports { get; set; } = new();

        public string? Status { get; set; }
        public string? TargetType { get; set; }

        public int PendingCount { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}