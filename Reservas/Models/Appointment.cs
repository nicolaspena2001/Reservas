namespace Reservas.Models;

using System.ComponentModel.DataAnnotations;

public class Appointment
{
    public int Id { get; set; }

    [Required] public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; } // Sin [Required]

    [Required] public int PatientId { get; set; }
    public Patient? Patient { get; set; } // Sin [Required]

    [Required] public DateTime AppointmentDate { get; set; }

    [Required] public string Reason { get; set; }

    [Required] public Constants.AppointmentStatus Status { get; set; }
}