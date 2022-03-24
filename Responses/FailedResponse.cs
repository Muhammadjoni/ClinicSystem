using System;

namespace ClinicSystem.Responses
{
  public class FailedResponse
  {
    public string StatusMessage { get; set; }
    public Exception Error { get; set; }
  }
}
