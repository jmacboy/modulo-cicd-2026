using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.IntTests.Setup
{
    public class ItemCreateResponse
    {
        public Guid Value { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsFailure { get; set; }
    }
}
