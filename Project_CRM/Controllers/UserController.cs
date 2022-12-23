using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Project_CRM.Models;
using System.Runtime.Intrinsics.X86;
using Newtonsoft.Json;

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

        [Route("UserExist")]
        [HttpGet]
        public JsonResult UserExist(string u_password, string u_login)
        {
            //string query = @"SELECT Count(1) as N FROM dbo.User_table WHERE User_Login= '" + u_login+"' AND User_Password= '" + u_password+ "'";
            string query = @"SELECT User_ID as token FROM dbo.User_table WHERE User_Login= '" + u_login+"' AND User_Password= '" + u_password+ "'";

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


            if (table.Rows.Count == 1) return new JsonResult(table.Rows[0][0].ToString());
            else
            {
                return new JsonResult("not_a_user");
            }

            //if (table.Rows[0][0].ToString() == "1" ) return new JsonResult("success");
            //else
            //{
            //    return new JsonResult("not_a_user");
            //}

        }


        [HttpGet]
        public JsonResult Get()
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
        public JsonResult Post(User user1)
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

       // [Route("ChangeRecord")]
        [HttpPut]
        public JsonResult Put(User user1)
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

        [HttpDelete]
        public JsonResult Delete(Guid User_Guid)
        {
            string query = @"
                            delete from dbo.User_table
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

                    myCommand.Parameters.Add(new SqlParameter("@User_ID", User_Guid));
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
