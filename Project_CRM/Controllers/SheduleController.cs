using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Project_CRM.Models;

namespace Project_CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheduleController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public SheduleController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult get()
        {
            string query = @"
                select Cell_ID, Service_ID, Place, Date_time, Employe_ID, Status, Record_ID, Client_ID from
                dbo.Shedule_Cell order by Date_time
                ";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon= new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using(SqlCommand myCommand = new SqlCommand (query, myCon))
                {
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();
                    myCon.Close();
                }

            }
            return new JsonResult(table);
        }

    }
}
