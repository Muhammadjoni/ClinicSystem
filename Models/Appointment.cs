using System;
using System.Collections.Generic;

namespace ClinicSystem.Models
{
  public class Appointment
  {
    public string Id { get; set; }
    // public int AppointmentDuration { get; set; }
    // public int  Room { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status = "PENDING";

    public string DoctorID { get; set; }
    public string PatientID { get; set; }

    //Navigation Properties
    public List<DocInfo> DocInfos { get; set; }

  }
}
