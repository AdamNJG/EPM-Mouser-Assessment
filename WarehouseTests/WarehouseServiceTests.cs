using EMP.Mouser.Inverview.Application.Exceptions;
using EMP.Mouser.Inverview.Application.Services;
using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WarehouseTests
{
    [TestClass]
    public class WarehouseServiceTests
    {
        private WarehouseService _warehouseService;
        private WarehouseRepository _warehouseRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _warehouseRepository = new WarehouseRepository();
            _warehouseService = new WarehouseService(_warehouseRepository);
        }

        [TestMethod]
        public async Task GetSingleProduct_ValidId_ReturnProduct()
        {

            long productId = 25;

            Product product = await _warehouseService.GetProduct(productId);

            Assert.IsNotNull(product);
            Assert.AreEqual(productId, product.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidRequestException), "Product not found.")]
        public async Task GetSingleProduct_InvalidId_ReturnException()
        {
            long productId = -1;
            
            Product product = await _warehouseService.GetProduct(productId);
        }

        [TestMethod]
        public async Task GetInStockProducts()
        {
            List<Product> products = await _warehouseService.GetInstockProducts();

            Assert.IsNotNull(products);

            products.ForEach(p => Assert.IsTrue(p.InStockQuantity > p.ReservedQuantity && (p.InStockQuantity - p.ReservedQuantity) > 0));
        }

        [TestMethod]
        public async Task ProcessOrder_ValidRequest_SuccessResponse()
        {
            // Got to find an in stock product to make sure order is valid first!
            // This test may fail if the test data supplied does not have a single in stock product!
            List<Product> inStockProducts = await _warehouseService.GetInstockProducts();
            int quantity = 1;
            long productId = inStockProducts.First(p => p.ReservedQuantity < p.InStockQuantity).Id;
            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = productId,
                Quantity = quantity
            };

            Product productBeforeOrder = await _warehouseService.GetProduct(productId);
            int reserved = productBeforeOrder.ReservedQuantity;

            UpdateResponse updateResponse = await _warehouseService.ProcessOrder(order);
            Product productAfterOrder = await _warehouseService.GetProduct(productId);

            Assert.AreEqual(reserved + quantity, productAfterOrder.ReservedQuantity);
            Assert.AreEqual(true, updateResponse.Success);
            Assert.AreEqual(null, updateResponse.ErrorReason);
        }

        [DataTestMethod]
        [DataRow(5)]
        [DataRow(1)]
        public async Task ProcessOrder_NotEnoughQuantity_FailureResonse(int quantity)
        {
            List<Product> products = await _warehouseRepository.List();
            long productId = products.First(p => p.InStockQuantity == p.ReservedQuantity + (quantity - 1)).Id;
            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = productId,
                Quantity = quantity
            };

            Product productBeforeOrder = await _warehouseService.GetProduct(productId);
            int reserved = productBeforeOrder.ReservedQuantity;

            UpdateResponse updateResponse = await _warehouseService.ProcessOrder(order);
            Product productAfterOrder = await _warehouseService.GetProduct(productId);


            Assert.AreEqual(reserved, productAfterOrder.ReservedQuantity);
            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.NotEnoughQuantity, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task ProcessOrder_QuantityInvalid_FailureResonse()
        {
            int quantity = -1;

            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = 1,
                Quantity = quantity
            };

            UpdateResponse updateResponse = await _warehouseService.ProcessOrder(order);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task ProcessOrder_InvalidRequest_FailureResonse()
        {
            int quantity = 1;

            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = -1,
                Quantity = quantity
            };

            UpdateResponse updateResponse = await _warehouseService.ProcessOrder(order);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task ShipOrder_ValidRequest_SuccessResponse()
        {
            List<Product> inStockProducts = await _warehouseService.GetInstockProducts();
            int quantity = 1;
            long productId = inStockProducts.First(p => p.InStockQuantity > quantity && p.ReservedQuantity > quantity).Id;
            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = productId,
                Quantity = quantity
            };

            Product productBeforeOrder = await _warehouseService.GetProduct(productId);
            int inStockQuantityBefore = productBeforeOrder.InStockQuantity;
            int reservedBefore = productBeforeOrder.ReservedQuantity;

            UpdateResponse updateResponse = await _warehouseService.ShipOrder(order);
            Product productAfterOrder = await _warehouseService.GetProduct(productId);

            Assert.AreEqual(inStockQuantityBefore - quantity, productAfterOrder.InStockQuantity);
            Assert.AreEqual(reservedBefore - quantity, productAfterOrder.ReservedQuantity);
            Assert.AreEqual(true, updateResponse.Success);
            Assert.AreEqual(null, updateResponse.ErrorReason);
        }

        [DataTestMethod]
        [DataRow(5)]
        [DataRow(1)]
        public async Task ShipOrder_NotEnoughQuantity_FailureResonse(int quantity)
        {
            List<Product> products = await _warehouseRepository.List();
            long productId = products.First(p => p.InStockQuantity - quantity < 0 || p.ReservedQuantity - quantity < 0).Id;
            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = productId,
                Quantity = quantity
            };

            Product productBeforeOrder = await _warehouseService.GetProduct(productId);
            int reserved = productBeforeOrder.ReservedQuantity;
            int stock = productBeforeOrder.InStockQuantity;

            UpdateResponse updateResponse = await _warehouseService.ShipOrder(order);
            Product productAfterOrder = await _warehouseService.GetProduct(productId);


            Assert.AreEqual(reserved, productAfterOrder.ReservedQuantity);
            Assert.AreEqual(stock, productAfterOrder.InStockQuantity);
            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.NotEnoughQuantity, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task ShipOrder_QuantityInvalid_FailureResonse()
        {
            int quantity = -1;

            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = 1,
                Quantity = quantity
            };

            UpdateResponse updateResponse = await _warehouseService.ShipOrder(order);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task ShipOrder_InvalidRequest_FailureResonse()
        {
            int quantity = 1;

            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = -1,
                Quantity = quantity
            };

            UpdateResponse updateResponse = await _warehouseService.ShipOrder(order);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task Restock_ValidRequest_SuccessResponse()
        {
            List<Product> products = await _warehouseRepository.List();
            int quantity = 1;
            long productId = products.First().Id;
            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = productId,
                Quantity = quantity
            };

            Product productBeforeOrder = await _warehouseService.GetProduct(productId);
            int inStockQuantityBefore = productBeforeOrder.InStockQuantity;

            UpdateResponse updateResponse = await _warehouseService.Restock(order);
            Product productAfterOrder = await _warehouseService.GetProduct(productId);

            Assert.AreEqual(inStockQuantityBefore + quantity, productAfterOrder.InStockQuantity);
            Assert.AreEqual(true, updateResponse.Success);
            Assert.AreEqual(null, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task Restock_QuantityInvalid_FailureResonse()
        {
            int quantity = -1;

            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = 1,
                Quantity = quantity
            };

            UpdateResponse updateResponse = await _warehouseService.Restock(order);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task Restock_InvalidRequest_FailureResonse()
        {
            int quantity = 1;

            UpdateQuantityRequest order = new UpdateQuantityRequest()
            {
                Id = -1,
                Quantity = quantity
            };

            UpdateResponse updateResponse = await _warehouseService.Restock(order);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
        }

        [TestMethod]
        public async Task Add_ValidRequest_CanRetrieve()
        {
            Product product = new Product()
            {
                Id = 1,
                InStockQuantity = 5,
                ReservedQuantity = 1,
                Name = "test"
            };

            CreateResponse<Product> response = await _warehouseService.AddItem(product);

            Assert.AreEqual(product.InStockQuantity, response.Model.InStockQuantity);
            Assert.AreEqual(0, response.Model.ReservedQuantity);
            Assert.AreEqual(product.Name, response.Model.Name);
        }

        [TestMethod]
        public async Task Add_ValidRequest_NameIncremented()
        {
            List<Product> products = await _warehouseRepository.List();

            string name = products.First().Name;

            Product product = new Product()
            {
                Id = 1,
                InStockQuantity = 5,
                ReservedQuantity = 1,
                Name = name
            };

            CreateResponse<Product> response = await _warehouseService.AddItem(product);

            Assert.AreEqual(product.InStockQuantity, response.Model.InStockQuantity);
            Assert.AreEqual(0, response.Model.ReservedQuantity);
            Assert.IsTrue(Regex.IsMatch(response.Model.Name, $"{name}[0-9]+"));
        }

        [TestMethod]
        public async Task Add_QuantityInvalid_FailureResonse()
        {
            int quantity = -1;

            Product product = new Product()
            {
                Id = 1,
                InStockQuantity = quantity,
                ReservedQuantity = 1,
                Name = "Test"
            };

            CreateResponse<Product> response = await _warehouseService.AddItem(product);

            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, response.ErrorReason);
        }

        [TestMethod]
        public async Task Add_InvalidRequest_FailureResonse()
        {
            int quantity = 1;

            Product product = new Product()
            {
                Id = 1,
                InStockQuantity = quantity,
                ReservedQuantity = 1,
                Name = ""
            };

            UpdateResponse updateResponse = await _warehouseService.AddItem(product);

            Assert.AreEqual(false, updateResponse.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
        }
    }
}