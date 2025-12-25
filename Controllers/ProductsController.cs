using DistributedOrderSystem.Domain.Models;
using DistributedOrderSystem.DTOs;
using DistributedOrderSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace DistributedOrderSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
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
                Stock = product.StockQuantity,
            };
            
            
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, productReadDto);
        }
    }
}
