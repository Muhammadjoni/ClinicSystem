using System;
using System.Collections.Generic;

namespace ClinicSystem.Models
{
  public class DocSlot
  {
    public string DoctorId { get; set; }
    public List<String> Slots { get; set; }
  }
}
