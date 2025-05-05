using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Data;
using Reservas.Models;

namespace Reservas.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Doctors"] = _context.Doctors.ToList();
            ViewData["Patients"] = _context.Patients.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            var hora = appointment.AppointmentDate.TimeOfDay;

            if (appointment.AppointmentDate < DateTime.Now)
                ModelState.AddModelError("AppointmentDate", "No se pueden agendar citas en fechas pasadas.");

            if (hora < TimeSpan.FromHours(6) || hora > TimeSpan.FromHours(18) || hora.Minutes % 30 != 0)
                ModelState.AddModelError("AppointmentDate", "La hora debe estar entre 6:00 a.m. y 6:00 p.m. en intervalos de 30 minutos.");

            bool hayConflicto = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate);

            if (hayConflicto)
                ModelState.AddModelError("", "El doctor ya tiene una cita en ese horario.");

            if (ModelState.IsValid)
            {
                appointment.Status = Constants.AppointmentStatus.Scheduled;
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Doctors"] = _context.Doctors.ToList();
            ViewData["Patients"] = _context.Patients.ToList();
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewData["Doctors"] = _context.Doctors.ToList();
            ViewData["Patients"] = _context.Patients.ToList();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Appointment appointment)
        {
            var hora = appointment.AppointmentDate.TimeOfDay;

            if (appointment.AppointmentDate < DateTime.Now)
                ModelState.AddModelError("AppointmentDate", "No se pueden seleccionar fechas pasadas.");

            if (hora < TimeSpan.FromHours(6) || hora > TimeSpan.FromHours(18) || hora.Minutes % 30 != 0)
                ModelState.AddModelError("AppointmentDate", "La hora debe estar entre 6:00 a.m. y 6:00 p.m. en intervalos de 30 minutos.");

            bool hayConflicto = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.Id != appointment.Id);

            if (hayConflicto)
                ModelState.AddModelError("", "El doctor ya tiene una cita en ese horario.");

            if (ModelState.IsValid)
            {
                appointment.Status = Constants.AppointmentStatus.Scheduled;
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Doctors"] = _context.Doctors.ToList();
            ViewData["Patients"] = _context.Patients.ToList();
            return View(appointment);
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("CancelConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = Constants.AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableHours(int doctorId, DateTime date)
        {
            var start = date.Date.AddHours(6);
            var end = date.Date.AddHours(18);

            var horariosOcupados = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate >= start &&
                            a.AppointmentDate < end)
                .Select(a => a.AppointmentDate.TimeOfDay)
                .ToListAsync();

            var disponibles = new List<string>();
            for (TimeSpan hora = TimeSpan.FromHours(6); hora < TimeSpan.FromHours(18); hora += TimeSpan.FromMinutes(30))
            {
                if (!horariosOcupados.Contains(hora))
                {
                    disponibles.Add(hora.ToString(@"hh\:mm"));
                }
            }

            return Json(disponibles);
        }
    }
}
