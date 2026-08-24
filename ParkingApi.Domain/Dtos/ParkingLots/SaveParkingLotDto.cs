using System.Collections.Generic;

namespace ParkingApi.Domain.Dtos.ParkingLots;

public class SaveParkingLotDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMainImage { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public List<int> EnrolledUserIds { get; set; } = new List<int>();
}
