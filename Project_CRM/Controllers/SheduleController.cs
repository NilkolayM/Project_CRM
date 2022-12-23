using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Project_CRM.Models;
using System.Reflection.Metadata.Ecma335;

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
                SELECT Client_ID, Date_time, Place, Employe.User_name, Shedule_Cell.Status, Cell_ID
                FROM Shedule_Cell
                INNER JOIN Employe
                ON Shedule_Cell.Employe_ID = Employe.Employe_ID 
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


        [HttpPut]
        public JsonResult Put(Guid user1, Guid cell1)
        {
            {
                //  Проверить Cell[cell1].Status на занятость:
                //    {
                //    == 0 -  Ячейка свободна - занять:{
                //                                       Cell[cell1].User_ID = user1;
                //                                       Cell[cell1].Status = 1;
                //                                      }, вернуть "sign_success"
                //    == 1 -  Проверить Cell[cell].User_ID занявшего ячейку на равенство текущему:
                //            {
                //              == user1 -  Ячейка занята - освободить: {
                //                                                       Cell[cell1].User_ID = 0;
                //                                                       Cell[cell1].Status = 0;
                //                                                       }, вернуть "unsign_success"
                //              != user1 -  Ячейка занята другим пользователем, вернуть "wrong_user"
                //            }
                //    else -  Ячейка служебная, вернуть "cant_sign"
                //    }
            }

            //User_ID, User_Name, User_Login, User_Password, User_Phone_num, User_email

            if (user1 == Guid.Empty) return new JsonResult("not_a_user");

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
                    list.Add(new SqlParameter("@Cell_ID", cell1));
                    list.Add(new SqlParameter("@User_ID", user1));
                    myCommand.Parameters.AddRange(list.ToArray<SqlParameter>());
                    mySQLreader = myCommand.ExecuteReader();
                    table.Load(mySQLreader);
                    mySQLreader.Close();        
                }
                myCon.Close();

                if (table.Rows.Count != 1) return new JsonResult("err");
                else
                {
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
