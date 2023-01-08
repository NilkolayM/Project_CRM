using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace Project_CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ClientA = @"if ((select Count(*) FROM dbo.Client WHERE User_Login=@UserLogin AND User_Password=@UserPassword) = 1)
		                                    begin
		                                    declare @User uniqueidentifier = (select User_ID FROM dbo.Client WHERE User_Login=@UserLogin AND User_Password=@UserPassword)
		                                    declare @Token uniqueidentifier = newid()
		                                    if exists (select 1 from dbo.Client_Active_Session where dbo.Client_Active_Session.User_ID = @User) 
		                                            begin
		                                            update dbo.Client_Active_Session
		                                            set dbo.Client_Active_Session.Token = @Token, dbo.Client_Active_Session.Last_Updated = GETUTCDATE() where dbo.Client_Active_Session.User_ID = @User
		                                    end else 
			                                        begin
			                                        INSERT INTO dbo.Client_Active_Session (Token, Last_Updated, User_ID) VALUES (@Token, GETUTCDATE(), @User)
			                                        end
		                                    select @Token as answer, null as role
	                                        end";

        private string EmployeeA = @"if ((select Count(*) FROM dbo.Employee WHERE User_Login=@UserLogin AND User_Password=@UserPassword) = 1)
		                                    begin
		                                    declare @User uniqueidentifier = (select Employee_ID FROM dbo.Employee WHERE User_Login=@UserLogin AND User_Password=@UserPassword)
		                                    declare @Token uniqueidentifier = newid()
		                                    if exists (select 1 from dbo.Employee_active_session where dbo.Employee_active_session.Employee_ID = @User) 
		                                            begin
		                                            update dbo.Employee_active_session
		                                            set dbo.Employee_active_session.Token = @Token, dbo.Employee_active_session.Last_Updated = GETUTCDATE() where dbo.Employee_active_session.Employee_ID = @User
		                                    end else 
			                                        begin
			                                        INSERT INTO dbo.Employee_active_session (Token, Last_Updated, Employee_ID) VALUES (@Token, GETUTCDATE(), @User)
			                                        end
		                                    select @Token as token, (select Employee_role FROM dbo.Employee WHERE Employee_ID=@User) as role
	                                        end";


        /// <summary>
        /// Функция ответственная за получение токена путем авторизации
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// "not_a_user" - нет такого пользователя
        /// <br/>
        /// "wrong_password" - неверный пароль
        /// <br/>
        /// если клиент - возврат токена и роли = null
        /// если сотрудник - возврат токена и роли цифрой больше 1
        /// </returns>
        [Route("Autorization")]
        [HttpGet]

        public JsonResult Autorization(string password, string login)
        {
            string query = @" if ((select Count(*) FROM dbo.Client WHERE User_Login=@UserLogin) = 1) select 1 as answer             
                              else 
                              if ((select Count(*) FROM dbo.Employee WHERE dbo.Employee.User_Login = @UserLogin) = 1) select 2 as answer
                              else 
                              select 0 as answer";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@UserLogin", login));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());

                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();

                }
                myCon.Close();
            }

            JsonResult return_msg;

            if (table.Rows.Count != 1) return new JsonResult("err");
            else
            {
                switch (table.Rows[0][0])
                {
                    case 1:
                        {
                            return_msg = NewToken(ClientA, password, login);
                        }
                        break;
                    case 2:
                        {
                            return_msg = NewToken(EmployeeA, password, login);
                        }
                        break;
                    default:
                        {
                            return_msg = new JsonResult("not_a_user");
                        }
                        break;
                }
            }
            return return_msg;
        }

        private JsonResult NewToken(string query, string password, string login)
        {

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@UserLogin", login));
                    list.Add(new SqlParameter("@UserPassword", password));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
                    
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();
                    
                }
                myCon.Close();
            }

            if (table.Rows.Count == 1) 
            {
                return new JsonResult(table);
            }
            else
            {
                return new JsonResult("wrong_password");
            }

        }

        string ClientT = @"     declare @date datetime = (select Last_Updated FROM dbo.Client_Active_Session WHERE Token = @Token)
				                declare @now datetime = GETUTCDATE()
				                if (DATEDIFF(MINUTE, @date, @now) < 5) begin
														declare @newtoken uniqueidentifier = NEWID()
														update dbo.Client_Active_Session
														set dbo.Client_Active_Session.Token = @newtoken, dbo.Client_Active_Session.Last_Updated = @now where Client_Active_Session.Token = @Token
														select @newtoken as answer
														end
														else begin
																select 0 as answer
															 end";

        string EmployeeT = @"   declare @date datetime = (select Last_Updated FROM dbo.Employee_active_session WHERE Token = @Token)
				                declare @now datetime = GETUTCDATE()
				                if (DATEDIFF(MINUTE, @date, @now) < 5) begin
														declare @newtoken uniqueidentifier = NEWID()
														update dbo.Employee_active_session
														set dbo.Employee_active_session.Token = @newtoken, dbo.Employee_active_session.Last_Updated = @now where Employee_active_session.Token = @Token
														select @newtoken as answer
														end
														else begin
																select 0 as answer
															 end";
        

        /// <summary>
        /// Функция ответственная за обновление токена
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// "not_a_user" - нет такого токена
        /// <br/>
        /// "invalid_token" - токен утратил значимость
        /// <br/>
        /// иначе - токен
        /// </returns>
        [Route("Update")]
        [HttpGet]
        public JsonResult Update(Guid token)
        {

            string query = @"if ((select Count(*) FROM dbo.Client_Active_Session WHERE Token = @UserToken) = 1) select 1 as answer             
                              else 
                              if ((select Count(*) FROM dbo.Employee_active_session WHERE Token = @UserToken) = 1) select 2 as answer
                              else 
                              select 0 as answer";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@UserToken", token));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());

                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();

                }
                myCon.Close();
            }

            JsonResult return_msg;

            if (table.Rows.Count != 1) return new JsonResult("err");
            else
            {
                switch (table.Rows[0][0])
                {
                    case 1:
                        {
                            return_msg = GetToken(ClientT, token);
                        }
                        break;
                    case 2:
                        {
                            return_msg = GetToken(EmployeeT, token);
                        }
                        break;
                    default:
                        {
                            return_msg = new JsonResult("not_a_user");
                        }
                        break;
                }
            }
            return return_msg;
        }

        private JsonResult GetToken(string query, Guid token)
        {   

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@Token", token));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();

                }
                myCon.Close();
            }

            if (table.Rows.Count != 1) return new JsonResult("err");
            else
            {
                if(table.Rows[0][0].ToString() == "0")
                {    
                   return new JsonResult("invalid_token");
                }
                else
                {
                   return new JsonResult(table.Rows[0][0].ToString());
                }
            }
 
        }

        [Route("Delete")]
        [HttpDelete]
        public void Dismiss(Guid token)
        {
            string query = @"DELETE FROM dbo.Client_Active_Session where Token = @UserToken
                             DELETE FROM dbo.Employee_active_session where Token = @UserToken";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;
                
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@UserToken", token));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());

                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();

                }
                myCon.Close();
            }
        }

    }
}
