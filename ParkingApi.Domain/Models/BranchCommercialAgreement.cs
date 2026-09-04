using System;

namespace ParkingApi.Domain.Models;

public class BranchCommercialAgreement : GeneralEntity
{
    public int BranchId { get; set; }
    public Guid AgreementId { get; set; }

    public virtual Branch Branch { get; set; } = null!;
    public virtual CommercialAgreement CommercialAgreement { get; set; } = null!;
}
