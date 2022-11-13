using System.Numerics;
using System.Xml.Linq;

namespace Project_CRM.Models
{
    public class SheduleCell
    {
        public Guid Id { get; set; }
        public byte ServiceId {get; set;}

        public string Place { get; set;}
        public DateTime CellDateTime { get; set; }

        public Guid Employee { get; set; }

        public Guid Client { get; set; }

        public bool is_done { get; set; }

        public SheduleCell(in DateTime date)
        {
            Id = Guid.NewGuid();
            ServiceId = 0;
            Place = "none";
            CellDateTime = date;
            Employee = Guid.Empty;
            Client = Guid.Empty;
            is_done = false;
        }
    }
   
}
