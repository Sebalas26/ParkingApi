namespace ParkingApi.Domain.Common.Enums;

public enum VehicleType
{
    Car = 0,
    Motorcycle = 1,
    Truck = 2,
    Van = 3,
    Bicycle = 4,
    Suv = 5
}

public enum TicketStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

public enum PaymentMethod
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    Transfer = 3
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1,
    FreeMinutes = 2
}
