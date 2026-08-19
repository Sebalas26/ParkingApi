using System;

namespace ParkingApi.Domain.Dtos.Agreements;

public class CommercialAgreementDto
{
    public Guid AgreementId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinPurchaseAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public int? MaxHoursApplicable { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAgreementDto
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinPurchaseAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public int? MaxHoursApplicable { get; set; }
}

public class UpdateAgreementDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MinPurchaseAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public int? MaxHoursApplicable { get; set; }
    public bool IsActive { get; set; } = true;
}
