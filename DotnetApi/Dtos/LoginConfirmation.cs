namespace DotnetApi.Dtos
{
  partial class LoginConfirmationDto
  {
    byte[] PasswordHash { get; set; }
    byte[] PasswordSalt { get; set; }

    public LoginConfirmationDto()
    {
      PasswordHash ??= new byte[0];
      PasswordSalt ??= new byte[0];
    }
  }
}