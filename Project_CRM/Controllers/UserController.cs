using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Project_CRM.Models;

namespace Project_CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult get()
        {
            string query = @"
                select User_ID, User_Name, User_Login, User_Password, User_Phone_num, User_email from
                dbo.User_table
                ";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();
                    myCon.Close();

                }

            }
            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult post(User user1)
        {
            string query = @"
                            insert into dbo.User_table
                            values (@User_ID, @User_Name, @User_Login, @User_Password, @User_Phone_num, @User_email)
                            ";

            //User_ID, User_Name, User_Login, User_Password, User_Phone_num, User_email from

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {


                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@User_ID", user1.ID));
                    list.Add(new SqlParameter("@User_Name", user1.Name));
                    list.Add(new SqlParameter("@User_Login", user1.Login));
                    list.Add(new SqlParameter("@User_Password", user1.Password));
                    list.Add(new SqlParameter("@User_Phone_num", user1.Phone));
                    list.Add(new SqlParameter("@User_email", user1.Email));

                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();
                    myCon.Close();

                }

            }
            return new JsonResult(table);
        }


        [HttpPut]
        public JsonResult put(User user1)
        {
            string query = @"
                            update dbo.User_table
                            set User_Name = @User_Name, User_Login = @User_Login, User_Password = @User_Password, User_Phone_num = @User_Phone_num, User_email = @User_email
                            where User_ID = @User_ID
                            ";

          //User_ID, User_Name, User_Login, User_Password, User_Phone_num, User_email

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@User_ID", user1.ID));
                    list.Add(new SqlParameter("@User_Name", user1.Name));
                    list.Add(new SqlParameter("@User_Login", user1.Login));
                    list.Add(new SqlParameter("@User_Password", user1.Password));
                    list.Add(new SqlParameter("@User_Phone_num", user1.Phone));
                    list.Add(new SqlParameter("@User_email", user1.Email));

                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
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
