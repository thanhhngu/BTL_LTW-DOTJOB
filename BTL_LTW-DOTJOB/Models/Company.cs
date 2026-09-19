namespace BTL_LTW_DOTJOB.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string WorkLocation { get; set; }
        public string CompanyAddress { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        //
        public int UserId { get; set; }
        public User User { get; set; }

    }
}
