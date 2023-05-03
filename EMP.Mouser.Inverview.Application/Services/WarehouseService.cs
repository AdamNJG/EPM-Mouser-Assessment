using EMP.Mouser.Inverview.Application.Exceptions;
using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using System.Diagnostics.Tracing;

namespace EMP.Mouser.Inverview.Application.Services
{
    public class WarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<Product> GetProduct(long id)
        {
            Product? product = await _warehouseRepository.Get(id);

            return product == null ? throw new InvalidRequestException("Product not found.", ErrorReason.InvalidRequest) : product;
        }

        public async Task<List<Product>> GetInstockProducts()
        {
            List<Product> allProducts = await _warehouseRepository.List();

            return allProducts.Where(InStock).ToList();
        }

        public async Task<UpdateResponse> ProcessOrder(UpdateQuantityRequest order)
        {
            try
            {
                CheckNegative(order.Quantity);

                await UpdateProduct(ReserveProduct(await GetProduct(order.Id), order.Quantity));

                return new UpdateResponse()
                {
                    Success = true
                };
            } 
            catch (InvalidRequestException ex)
            {
                return new UpdateResponse()
                {
                    Success = false,
                    ErrorReason = ex.errorReason
                };
            }
        }

        public async Task<UpdateResponse> ShipOrder(UpdateQuantityRequest order)
        {
            try
            {
                CheckNegative(order.Quantity);

                await UpdateProduct(ShipProduct(await GetProduct(order.Id), order.Quantity));
                return new UpdateResponse()
                {
                    Success = true
                };
            }
            catch (InvalidRequestException ex)
            {
                return new UpdateResponse()
                {
                    Success = false,
                    ErrorReason = ex.errorReason
                };
            }
        }

        public async Task<UpdateResponse> Restock(UpdateQuantityRequest restockRequest)
        {
            try
            {
                CheckNegative(restockRequest.Quantity);

                await UpdateProduct(RestockItem(await GetProduct(restockRequest.Id), restockRequest.Quantity));

                return new UpdateResponse()
                {
                    Success = true
                };
            }
            catch (InvalidRequestException ex)
            {
                return new UpdateResponse()
                {
                    Success = false,
                    ErrorReason = ex.errorReason
                };
            }
        }

        public async Task<CreateResponse<Product>> AddItem(Product product)
        {
            try
            {
                CheckNegative(product.InStockQuantity);

                Product insertedProduct = await _warehouseRepository.Insert(await CheckName(product));

                return new CreateResponse<Product>
                {
                    Success = true,
                    Model = insertedProduct
                };
            }
            catch (InvalidRequestException ex)
            {
                return new CreateResponse<Product>
                {
                    Success = false,
                    ErrorReason = ex.errorReason
                };
            }
        }

        private async Task<Product> CheckName(Product product)
        {
            if (String.IsNullOrEmpty(product.Name))
            {
                throw new InvalidRequestException("Name of product is null or empty!", ErrorReason.InvalidRequest);
            }

            List<Product> products = await _warehouseRepository.List();

            int matchingProducts = products.Where(p => p.Name.Trim() == product.Name.Trim()).Count();

            if (matchingProducts == 0)
            {
                return product;
            }

            product.Name = product.Name + (matchingProducts);

            return product;
        }

        private async Task UpdateProduct(Product product)
        {
            await _warehouseRepository.UpdateQuantities(product);
        }

        private Product ReserveProduct(Product product, int quantity)
        {
            if(!CheckStock(product, product.ReservedQuantity + quantity))
            {
                throw new InvalidRequestException($"Not enough of product:{product.Id} to reserve.", ErrorReason.NotEnoughQuantity);
            }

            product.ReservedQuantity += quantity;

            return product;
        }

        private Product ShipProduct(Product product, int quantity)
        {
            if (!CanShip(product, quantity))
            {
                throw new InvalidRequestException($"Not enough of product:{product.Id} to ship.", ErrorReason.NotEnoughQuantity);
            }

            product.ReservedQuantity -= quantity;
            product.InStockQuantity -= quantity;

            return product;
        }

        private Product RestockItem(Product product, int quantity)
        {
            product.InStockQuantity += quantity;

            return product;
        }

        private bool CheckStock(Product product, int quantity)
        {
            return product.InStockQuantity > quantity;
        }

        private bool InStock(Product product)
        {
            return CheckStock(product, product.ReservedQuantity) && (product.InStockQuantity - product.ReservedQuantity) > 0;
        }

        private bool CanShip(Product product, int quantity)
        {
            return CheckStock(product, quantity) && product.ReservedQuantity - quantity >= 0;
        }

        private void CheckNegative(int number)
        {
            if (number < 0)
            {
                throw new InvalidRequestException("Negative integer entered!", ErrorReason.QuantityInvalid);
            }
        }
    }
}