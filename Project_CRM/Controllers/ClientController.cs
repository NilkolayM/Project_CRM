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

        
        
    }
}
