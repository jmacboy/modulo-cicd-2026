namespace Inventory.Domain.Shared;

public record QuantityValue
{
    public int Value { get; private set; }
    public QuantityValue(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(value));
        Value = value;
    }

    public static implicit operator int(QuantityValue quantity)
    {
        return quantity == null ? 0 : quantity.Value;
    }

    public static implicit operator QuantityValue(int a)
    {
        return new QuantityValue(a);
    }

    public static QuantityValue operator +(QuantityValue a, int b) => new(a.Value + b);

    public static QuantityValue operator -(QuantityValue a, int b) => new(a.Value - b);


}
