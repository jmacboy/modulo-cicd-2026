using Inventory.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

namespace Inventory.Domain.Items;

public class Item : AggregateRoot
{
    public string Name { get; private set; }
    public QuantityValue Stock { get; private set; }
    public QuantityValue Reserved { get; private set; }
    public QuantityValue Available { get; private set; }
    public CostValue UnitaryCost { get; private set; }

    public Item(Guid id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(ItemErrors.NameIsRequired());
        }
        Name = name;
        Stock = 0;
        Reserved = 0;
        Available = 0;
        UnitaryCost = 0;
    }

    /// <summary>
    /// Adds stock to the item. If the item already has stock, it will calculate the new unitary cost based on the average cost of the items in stock and the new items being added.
    /// </summary>
    /// <param name="quantityToAdd">Quantity of the Stock to add</param>
    /// <param name="unitaryCost">Unitary cost of new stock added</param>
    /// <param name="costStrategy">Cost strategy to calculate the new unitary cost</param>"
    /// <exception cref="DomainException"></exception>
    public void AddStock(int quantityToAdd, decimal unitaryCost,
        ICostStrategy costStrategy)
    {
        if (quantityToAdd <= 0)
        {
            throw new DomainException(ItemErrors.NonNegativeStock());
        }
        if (costStrategy == null)
        {
            throw new DomainException(ItemErrors.CostStrategyNotProvided());
        }
        // Calculate the new unitary cost based on the provided cost strategy
        int totalStock = Stock + quantityToAdd;
        UnitaryCost = costStrategy.CalculateNewCost(Stock, UnitaryCost,
            quantityToAdd, unitaryCost);

        Stock = totalStock;
        Available = Stock - Reserved;
    }

    public void ApplyReservation(int quantityToSubstract)
    {
        if (quantityToSubstract > Reserved)
        {
            throw new DomainException(ItemErrors.InsuficentStockToSubstract(Reserved, quantityToSubstract));
        }
        Reserved -= quantityToSubstract;
        Stock -= quantityToSubstract;
        Available = Stock - Reserved;
    }

    public void ReserveStock(int quantityToReserve)
    {
        if (quantityToReserve > Available)
        {
            throw new DomainException(ItemErrors.InsuficentStockToReserve(Stock, quantityToReserve));
        }
        Reserved += quantityToReserve;
        Available -= quantityToReserve;
    }

    public void UnreserveStock(int quantityToUnreserve)
    {
        if (quantityToUnreserve > Reserved)
        {
            throw new DomainException(ItemErrors.InsuficentStockToSubstract(Reserved, quantityToUnreserve));
        }
        Reserved -= quantityToUnreserve;
        Available += quantityToUnreserve;
    }

    //Need for EF Core
    private Item() { }

}
