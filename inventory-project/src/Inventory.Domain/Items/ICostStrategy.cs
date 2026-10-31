using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Domain.Items;

public interface ICostStrategy
{
    decimal CalculateNewCost(int currentStock, decimal currentUnitaryCost, int newStock, decimal newUnitaryCost);
}
