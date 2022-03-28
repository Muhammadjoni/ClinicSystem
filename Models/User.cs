using System.Collections.Generic;

namespace ClinicSystem.Models
{
  // [Index(nameof(email), IsUnique = true)]
  public class User

  {
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }

    // public string DocInfoId { get; set; }
    // public DocInfo DocInfo { get; set; }

    // public string PatientInfoId { get; set; }
    // public PatientInfo PatientInfo { get; set; }

    //Navigation Properties
    public List<DocInfo> DocInfos { get; set; }

  }
}
