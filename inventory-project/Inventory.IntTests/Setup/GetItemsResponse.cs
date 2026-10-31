using Inventory.Application.Items.GetItems;
using System.Collections.Generic;

namespace Inventory.IntTests.Setup
{
    public class GetItemsResponse
    {
        public ICollection<ItemDto> Value { get; set; } = new List<ItemDto>();
        public bool IsSuccess { get; set; }
        public bool IsFailure { get; set; }
    }
}
