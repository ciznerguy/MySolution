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

        // הזרקת שירות ה-DAL דרך הבנאי (Constructor Injection)
        public PurchasesController(PurchaseDB purchaseDb)
        {
            _purchaseDb = purchaseDb;
        }

        // GET: api/Purchases
        // שליפת כל הרכישות הקיימות במערכת
        [HttpGet]
        public async Task<ActionResult<List<Purchase>>> GetAll()
        {
            List<Purchase> purchases = await _purchaseDb.SelectAllAsync();
            return Ok(purchases);
        }

        // GET: api/Purchases/customer/5
        // שליפת כל הרכישות השייכות ללקוח ספציפי לפי קוד הלקוח
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<Purchase>>> GetByCustomerId(int customerId)
        {
            List<Purchase> purchases = await _purchaseDb.SelectByCustomerIdAsync(customerId);
            return Ok(purchases);
        }
    }
}