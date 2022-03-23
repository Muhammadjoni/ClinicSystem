using System;

namespace ClinicSystem.Models
{
  public class SlotRequest
  {
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string DoctorID { get; set; }
    public string PatientID { get; set; }
  }
}
