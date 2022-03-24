namespace ClinicSystem.Responses
{
  public class SuccessResponse<T>
  {
    public string StatusMessage = ResponseContent.Success;
    public T ResultData { get; set; }
  }
}
