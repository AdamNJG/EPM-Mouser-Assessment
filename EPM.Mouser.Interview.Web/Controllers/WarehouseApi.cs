using EMP.Mouser.Inverview.Application.Exceptions;
using EMP.Mouser.Inverview.Application.Services;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EPM.Mouser.Interview.Web.Controllers
{
    public class WarehouseApi : Controller
    {
        private readonly WarehouseService _warehouseService;

        public WarehouseApi(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
        [HttpGet("api/warehouse/{id}")]
        public async Task<JsonResult> GetProduct(long id)
        {
            try
            {
                return Json(await _warehouseService.GetProduct(id));
            }
            catch (InvalidRequestException ex)
            {
                return new JsonResult(ex.Message) 
                {
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet ("api/warehouse")]
        public async Task<JsonResult> GetPublicInStockProducts()
        {
            return Json(await _warehouseService.GetInstockProducts());
        }


        /*
         *  Action: GET --- This should be a post as it would be adding a new record
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPost ("api/warehouse/order")]
        public async Task<JsonResult> OrderItem([FromBody]UpdateQuantityRequest updateQuantityRequest)
        {
            return Json(await _warehouseService.ProcessOrder(updateQuantityRequest));
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPut("api/warehouse/ship")]
        public async Task<JsonResult> ShipItem([FromBody] UpdateQuantityRequest updateQuantityRequest)
        {
            return Json(await _warehouseService.ShipOrder(updateQuantityRequest));
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPut("api/warehouse/restock")]
        public async Task<JsonResult> RestockItem([FromBody] UpdateQuantityRequest updateQuantityRequest)
        {
            return Json(await _warehouseService.Restock(updateQuantityRequest));
        }

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        [HttpPost("api/warehouse/add")]
        public async Task<JsonResult> AddNewProduct([FromBody] Product product)
        {
            return Json(await _warehouseService.AddItem(product));
        }
    }
}
