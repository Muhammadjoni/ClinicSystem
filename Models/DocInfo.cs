using System.Collections.Generic;

namespace ClinicSystem.Models
{
  public class DocInfo
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
    // public string Username { get; set; }
    // public string Email { get; set; }

    // public ICollection<User> Users { get; set; }
  }
}
