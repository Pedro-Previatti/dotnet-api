namespace DotnetApi.Dtos
{
  public partial class LoginDto
  {
    public string Email { get; set; }
    public string Password { get; set; }

    public LoginDto()
    {
      Email ??= "";
      Password ??= "";
    }
  }
}