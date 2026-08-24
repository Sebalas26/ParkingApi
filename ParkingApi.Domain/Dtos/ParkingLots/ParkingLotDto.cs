using System;
using System.Collections.Generic;
using ParkingApi.Domain.Dtos.Users;

namespace ParkingApi.Domain.Dtos.ParkingLots;

public class ParkingLotDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMainImage { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<GetUsersDto> EnrolledUsers { get; set; } = new List<GetUsersDto>();
}
