using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using DAL;

namespace MyProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        // GET: api/person/all
        [HttpGet("all")]
        public async Task<ActionResult<List<Person>>> GetAll()
        {
            PersonDB db = new PersonDB();
            List<Person> list = await db.SelectAllAsync();
            return Ok(list); 
        }
    }
}