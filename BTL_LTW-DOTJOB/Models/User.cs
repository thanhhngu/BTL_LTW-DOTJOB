namespace BTL_LTW_DOTJOB.Models
{
    public class User
    {
        public int Id { get; set; }    
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        //
        public Company? Company { get; set; }
    }
}
