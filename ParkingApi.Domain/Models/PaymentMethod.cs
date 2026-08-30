using System;

namespace ParkingApi.Domain.Models;

public class PaymentMethod : GeneralEntity
{
    public int? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public virtual Company? Company { get; set; }
}
