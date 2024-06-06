namespace DotnetApi.Dtos
{
  partial class LoginConfirmationDto
  {
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public LoginConfirmationDto()
    {
      PasswordHash ??= new byte[0];
      PasswordSalt ??= new byte[0];
    }
  }
}