namespace DistributedOrderSystem.DTOs
{
    public class OrderReadDto
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemReadDto> Items { get; set; } = new();
    }

    public class OrderItemReadDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}

