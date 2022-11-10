namespace Project_CRM.Models
{
    public class User
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public User() {
                ID = Guid.NewGuid();
                Name = "none";
                Login = "none";
                Password = "none";
                Email = "none";
                Phone = "none";
              }
    }
}
