using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DistributedOrderSystem.Infrastructure.Data;
using DistributedOrderSystem.Domain.Models;
using DistributedOrderSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();

            var orderDtos = orders.Select(order => new OrderReadDto
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                TotalPrice = order.Items.Sum(item =>
                    _context.Products.First(p => p.Id == item.ProductId).Price * item.Quantity),
                Items = order.Items.Select(item => new OrderItemReadDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    LineTotal = _context.Products.First(p => p.Id == item.ProductId).Price * item.Quantity
                }).ToList()
            });

            return Ok(orderDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found.");

            // Load products once
            var productDict = await _context.Products
                .ToDictionaryAsync(p => p.Id);

            var dto = new OrderReadDto
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                TotalPrice = order.Items.Sum(i =>
                    productDict[i.ProductId].Price * i.Quantity),
                Items = order.Items.Select(i => new OrderItemReadDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    LineTotal = productDict[i.ProductId].Price * i.Quantity
                }).ToList()
            };

            return Ok(dto);

        }





        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto dto)
        {
            // Load all necessary products
            var productIds = dto.Items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // Validate stock
            foreach (var item in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                    return BadRequest($"Product {item.ProductId} does not exist.");

                if (product.StockQuantity < item.Quantity)
                    return BadRequest($"Insufficient stock for product {product.Name}.");
            }

            // Build Order
            var order = new Order
            {
                Status = OrderStatus.Pending,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                }).ToList()
            };

            // Deduct stock
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.StockQuantity -= item.Quantity;
            }

            // Save order
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create read DTO response
            var orderReadDto = new OrderReadDto
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                TotalPrice = products.Sum(p =>
                {
                    var quantity = dto.Items.First(i => i.ProductId == p.Id).Quantity;
                    return p.Price * quantity;
                }),
                Items = order.Items.Select(i => new OrderItemReadDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    LineTotal = products.First(p => p.Id == i.ProductId).Price * i.Quantity
                }).ToList()
            };

            return Ok(orderReadDto);

        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            // Load the order (with items too if needed later)
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found.");

            // Try to parse the incoming status string into the enum
            if (!Enum.TryParse<OrderStatus>(dto.NewStatus, true, out var newStatus))
                return BadRequest("Invalid order status.");

            // Optional: business rule validation
            // For example: Pending → Completed should not be allowed
            // You can add rules here later if you want

            // Update the status
            order.Status = newStatus;

            await _context.SaveChangesAsync();

            // Return updated order DTO
            var orderReadDto = new OrderReadDto
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                TotalPrice = order.Items.Sum(i =>
                    _context.Products.First(p => p.Id == i.ProductId).Price * i.Quantity),
                Items = order.Items.Select(i => new OrderItemReadDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    LineTotal = _context.Products.First(p => p.Id == i.ProductId).Price * i.Quantity
                }).ToList()
            };

            return Ok(orderReadDto);
        }



    }
}