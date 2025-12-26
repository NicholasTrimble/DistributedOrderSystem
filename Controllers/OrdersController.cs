using Microsoft.AspNetCore.Mvc;
using DistributedOrderSystem.Infrastructure.Data;
using DistributedOrderSystem.Domain.Models;
using DistributedOrderSystem.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DistributedOrderSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        
        public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =======================
        // GET ALL ORDERS
        // =======================
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

        // =======================
        // GET ORDER BY ID
        // =======================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found.");

            var productDict = await _context.Products.ToDictionaryAsync(p => p.Id);

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

        // =======================
        // CREATE ORDER
        // =======================
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto dto)
        {
            _logger.LogInformation("Creating order with {ItemCount} items.", dto.Items.Count);

            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found while creating order.", item.ProductId);
                    return BadRequest($"Product {item.ProductId} does not exist.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductName}. Requested {Requested}, Available {Available}.",
                        product.Name, item.Quantity, product.StockQuantity
                   );

                    return BadRequest($"Insufficient stock for product {product.Name}.");
                }
            }

            var order = new Order
            {
                Status = OrderStatus.Pending,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.StockQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} created successfully.", order.Id);

            var orderReadDto = new OrderReadDto
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                TotalPrice = products.Sum(p =>
                {
                    var qty = dto.Items.First(i => i.ProductId == p.Id).Quantity;
                    return p.Price * qty;
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

        // =======================
        // UPDATE ORDER STATUS
        // =======================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            _logger.LogInformation("Updating status for order {OrderId} to {NewStatus}", id, dto.NewStatus);

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found.", id);
                return NotFound("Order not found.");
            }

            if (!Enum.TryParse<OrderStatus>(dto.NewStatus, true, out var newStatus))
            {
                _logger.LogWarning("Invalid order status '{NewStatus}' for order {OrderId}.", dto.NewStatus, id);
                return BadRequest("Invalid order status.");
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

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
