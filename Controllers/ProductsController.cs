using DistributedOrderSystem.Domain.Models;
using DistributedOrderSystem.DTOs;
using DistributedOrderSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedOrderSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public ProductsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 10)
        {
            var cacheKey = $"products_page_{page}_size_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<Product> products))
            {
                products = await _context.Products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5));
            }

            return Ok(products);
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                StockQuantity = productDto.Stock
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productReadDto = new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.StockQuantity
            };

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, productReadDto);
        }
    }
}
