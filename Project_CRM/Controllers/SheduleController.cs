using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Project_CRM.Models;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel;

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
                SELECT Client_ID, Date_time, Place, Employee.User_name, Shedule_Cell.Status, Cell_ID
                FROM Shedule_Cell
                INNER JOIN Employee
                ON Shedule_Cell.Employee_ID = Employee.Employee_ID
                ORDER BY Date_time
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

        /// <summary>
        /// Функция ответственная за обновление токена пользователя при запросе к API
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <br/>
        /// 0 - токен обновлен
        /// <br/>
        /// 1 - нет такого токена
        /// <br/>
        /// 2 - токен утратил значимость
        /// <br/>
        /// 3 - ошибка перевода токена из строки
        /// <br/>
        /// 4 - ошибка получения данных из базы
        /// </returns>
        private byte UpdateClientToken(ref Guid token)
        {
            string query = @"       if ((select Count(*) FROM dbo.Client_Active_Session WHERE Token = @UserToken) != 1) select 0 as answer
                                    else begin
                                    declare @date datetime = (select Last_Updated FROM dbo.Client_Active_Session WHERE Token = @Token)
				                    declare @now datetime = GETUTCDATE()
				                    if (DATEDIFF(MINUTE, @date, @now) < 5) begin
														declare @newtoken uniqueidentifier = NEWID()
														update dbo.Client_Active_Session
														set dbo.Client_Active_Session.Token = @newtoken, dbo.Client_Active_Session.Last_Updated = @now where Client_Active_Session.Token = @Token
														select @newtoken as answer
														end
														else begin
																select 1 as answer
															 end
                                    end";

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

            if (table.Rows.Count != 1) return 4;
            else
            {
                switch (table.Rows[0][0])
                {
                    case 0: return 1;
                     
                    case 1: return 2;
                       
                    default:
                        {
                            Guid newtoken;
                            if (Guid.TryParse(table.Rows[0][0].ToString(), out newtoken))
                            {
                                token = newtoken;
                                return 0;
                            }
                            else return 3;
                        }   
                }
            }
            
        }

        private Guid GetUserIdByToken(Guid token)
        {
            string client = @" Select User_ID from Client_Active_Session where Token = @Token";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(client, myCon))
                {
                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@Token", token));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();
                }
            }

            Guid uid;
            if (Guid.TryParse(table.Rows[0][0].ToString(), out uid)) return uid; else return Guid.Empty;

        }

        [Route("UserAssign")]
        [HttpPut]
        public JsonResult Put(Guid token, Guid cell)
        {

            switch (UpdateClientToken(ref token))
            {
                case 0: break;

                case 1: return new JsonResult("not_a_user");

                case 2: return new JsonResult("wrong_token");

                default: return new JsonResult("error");
            }

            Guid user = GetUserIdByToken(token);

            {
                /*  Проверить Cell[cell1].Status на занятость:
                    {
                    == 0 -  Ячейка свободна - занять:{
                                                       Cell[cell1].User_ID = user1;
                                                       Cell[cell1].Status = 1;
                                                      }, вернуть "sign_success"
                    == 1 -  Проверить Cell[cell].User_ID занявшего ячейку на равенство текущему:
                            {
                              == user1 -  Ячейка занята - освободить: {
                                                                       Cell[cell1].Status = 0;
                                                                       Cell[cell1].User_ID = 0;
                                                                       }, вернуть "unsign_success"
                             != user1 -  Ячейка занята другим пользователем, вернуть "wrong_user"
                            }
                    else -  Ячейка служебная, вернуть "cant_sign" */
            }

            string query = @"Declare @Status bit = 0
                             Declare @info int
                             Select @Status = Status from dbo.Shedule_Cell where Shedule_Cell.Cell_ID = @Cell_ID
                             if @Status = 0
                                    begin
		                                update dbo.Shedule_Cell
		                                set Shedule_Cell.Client_ID = @User_ID, Shedule_Cell.Status = 1
		                                where Cell_ID = @Cell_ID
		                                select 0 as answer
                                    end
                             else
                                    begin
		                            if @Status = 1
		                                            begin
				                                        select @info = Count(*)
				                                        from dbo.Shedule_Cell
				                                        where (Cell_ID = @Cell_ID and Client_ID = @User_ID)
				                                        if @info = 1 
				                                                    begin
					                                                    update dbo.Shedule_Cell
					                                                    set Shedule_Cell.Client_ID = null, Shedule_Cell.Status = 0
					                                                    where Cell_ID = @Cell_ID
					                                                    select 1 as answer
				                                                    end
				                                        else select 2 as answer
		                                            end
		                                            else select 3 as answer
                                    end";

            DataTable table = new DataTable();

            string sqlDataSource = _configuration.GetConnectionString("CRM_app_con");

            SqlDataReader mySQLreader;

            JsonResult return_msg;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    List<SqlParameter> list = new List<SqlParameter>();
                    list.Add(new SqlParameter("@Cell_ID", cell));
                    list.Add(new SqlParameter("@User_ID", user));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();        
                }
                myCon.Close();

                if (table.Rows.Count != 1) return new JsonResult("err");
                else
                {
                    TokenController TC = new TokenController(_configuration);

                    switch (table.Rows[0][0])
                    {
                        case 0:
                            {
                                return_msg = new JsonResult("sign_success");
                            }
                            break;
                        case 1:
                            {
                                return_msg = new JsonResult("unsign_success");
                            }
                            break;
                        case 2:
                            {
                                return_msg = new JsonResult("cant_sign");
                            }
                            break;
                        case 3:
                            {
                                return_msg = new JsonResult("wrong_user");
                            }
                            break;
                        default:
                            {
                                return_msg = new JsonResult("err");
                            }
                            break;
                    }
                }

            }
            return return_msg;
        }
    }

}
