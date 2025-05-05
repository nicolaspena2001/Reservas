using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class Constants
    {
        public enum AppointmentStatus
        {
            [Display(Name = "Pendiente")] Scheduled = 0,

            [Display(Name = "Completada")] Completed = 1,

            [Display(Name = "Cancelada")] Cancelled = 2
        }
    }
}