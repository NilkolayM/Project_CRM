using System.Numerics;
using System.Xml.Linq;

namespace Project_CRM.Models
{
    public class SheduleCell
    {
        public Guid Cell_ID { get; set; }

        public Guid Service_ID {get; set;}

        public string Place { get; set;}

        public DateTime CellDateTime { get; set; }

        public Guid Employee_ID { get; set; }

        public byte Status { get; set; }

        public Guid Record_ID { get; set; }

        public Guid Client_ID { get; set; }

        public SheduleCell()
        {
            Cell_ID = Guid.NewGuid();
            Service_ID = Guid.Empty;
            Place = "none";
            CellDateTime = DateTime.Now;
            Employee_ID = Guid.Empty;
            Status = 0x0;
            Record_ID = Guid.Empty;
            Client_ID = Guid.Empty;
        }
    }
   
}
