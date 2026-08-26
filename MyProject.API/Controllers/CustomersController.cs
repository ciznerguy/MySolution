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
        private readonly PersonDB _personDb;

        // הזרקת שכבת ה-DAL דרך הבנאי (Constructor Injection)
        public PersonController(PersonDB personDb)
        {
            _personDb = personDb;
        }

        // GET: api/person/all
        [HttpGet("all")]
        public async Task<ActionResult<List<Person>>> GetAll()
        {
            List<Person> list = await _personDb.SelectAllAsync();
            return Ok(list);
        }
    }
}