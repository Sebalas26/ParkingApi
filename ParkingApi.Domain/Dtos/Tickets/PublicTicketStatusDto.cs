using System;

namespace ParkingApi.Domain.Dtos.Tickets;

public class PublicTicketStatusDto
{
    public bool IsFound { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleTypeName { get; set; } = string.Empty;
    public int VehicleType { get; set; }
    public DateTime? EntryTimeUtc { get; set; }
    public DateTime? EntryTimeLocal { get; set; }
    public DateTime? ExitTimeUtc { get; set; }
    public DateTime? ExitTimeLocal { get; set; }
    public int ElapsedMinutes { get; set; }
    public string FormattedDuration { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal EstimatedAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public int Status { get; set; } // 0 = Active, 1 = Completed, 2 = Cancelled
    public string StatusDescription { get; set; } = string.Empty;
    public string ParkingName { get; set; } = "Parqueadero ParkFlow";
    public string ConsultationUrl { get; set; } = string.Empty;
}
