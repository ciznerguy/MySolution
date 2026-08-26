using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL;
using Model;

namespace MyProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly PurchaseDB _purchaseDb;

        // הזרקת שכבת ה-DAL דרך הבנאי (Constructor Injection)
        public PurchasesController(PurchaseDB purchaseDb)
        {
            _purchaseDb = purchaseDb;
        }

        // GET: api/Purchases
        [HttpGet]
        public async Task<ActionResult<List<Purchase>>> GetAll()
        {
            List<Purchase> purchases = await _purchaseDb.SelectAllAsync();
            return Ok(purchases);
        }
    }
}