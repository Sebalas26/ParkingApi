using System;

namespace ParkingApi.Domain.Dtos.Stores;

public class StoreDto
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ContactName { get; set; }
    public bool IsActive { get; set; }
    public int AgreementsCount { get; set; }
}

public class CreateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ContactName { get; set; }
}

public class UpdateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ContactName { get; set; }
    public bool IsActive { get; set; } = true;
}
