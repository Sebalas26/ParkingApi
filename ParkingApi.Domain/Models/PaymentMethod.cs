using System;

namespace ParkingApi.Domain.Models;

public class PaymentMethod : GeneralEntity
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
