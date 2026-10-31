namespace Inventory.Application.Items.GetItems;

public record ItemDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int Available { get; set; }
    public int Reserved { get; set; }
    public decimal UnitaryCost { get; set; }
}
