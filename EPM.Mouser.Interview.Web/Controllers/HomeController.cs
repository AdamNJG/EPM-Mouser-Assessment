using EMP.Mouser.Inverview.Application.Services;
using EPM.Mouser.Interview.Models;
using EPM.Mouser.Interview.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly WarehouseService _warehouseService;

        public HomeController(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _warehouseService.GetInstockProducts();

            ProductsModel ViewModel = new ProductsModel()
            {
                Products = products
            };

            return View(ViewModel);
        }
    }
}
