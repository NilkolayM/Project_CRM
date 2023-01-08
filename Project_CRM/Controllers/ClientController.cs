using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_CRM.Models;
using System.Data.SqlClient;
using System.Data;

namespace Project_CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ClientController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("GetClientInfo")]
        [HttpGet]
        public JsonResult GetClientInfoByToken(Guid token)
        {
            string query = @"
                            declare @TempID uniqueidentifier = (select  dbo.Client_Active_Session.User_ID from  dbo.Client_Active_Session where Token = @TempToken)
                            select User_Name, User_Phone_num, User_email, User_ID as ID from dbo.Client where User_ID = @TempID
                            ";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@TempToken", token));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());

                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();
                }
                myCon.Close();
            }
            return new JsonResult(table);
        }


    }
}
