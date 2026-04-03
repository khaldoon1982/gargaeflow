namespace GarageFlow.Domain.Enums;

public enum MaintenanceStatus
{
    Gepland = 0,     // Scheduled
    InBehandeling = 1, // In progress
    Afgerond = 2,    // Completed
    Geannuleerd = 3  // Cancelled
}
