namespace TransportPlatform.Ticketing.Domain.Entities;

public class Route
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Origin { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int TotalSeats { get; private set; }
    public bool IsActive { get; private set; }

    private Route() { }

    public static Route Create(
        string name,
        string origin,
        string destination,
        decimal price,
        int totalSeats) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Origin = origin,
            Destination = destination,
            Price = price,
            TotalSeats = totalSeats,
            IsActive = true
        };
}
