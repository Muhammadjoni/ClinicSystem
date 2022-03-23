using System.Collections.Generic;
using ClinicSystem.Models;

namespace ClinicSystem.Models
{
  public class PatientInfo
  {
    public string Id { get; set; }
    public List<Appointment> History { get; set; }
  }
}
