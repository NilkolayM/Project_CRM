namespace Project_CRM.Models
{
    public class SheduleCell
    {
        public Guid Id { get; set; }
        public Guid ServiceId {get; set;}

        public string Place { get; set;}
        public DateTime CellDateTime { get; set; }

        public Guid Employee { get; set; }

        public Guid Client { get; set; }
    }
}
