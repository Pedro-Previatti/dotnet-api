using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace DotnetApi.Controllers
{
  public class AuthController : ControllerBase
  {
    private readonly DataContextDapper _dapper;
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
      _config = config;
    }

    [HttpPost("register")]
    public IActionResult Register(RegistrationDto registrationUser)
    {
      if (registrationUser.Password == registrationUser.PasswordConfirm)
      {
        string sql = $"SELECT * FROM AppSchema.Auth WHERE Email = '{registrationUser.Email}'";

        IEnumerable<string> existingUsers = _dapper.LoadData<string>(sql);
        if (existingUsers.Count() == 0)
        {
          byte[] passwordSalt = new byte[128 / 8];
          using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
          {
            rng.GetNonZeroBytes(passwordSalt);
          }

          string passwordSaltAndKey = _config.GetSection("AppSettings:PasswordKey").Value +
            Convert.ToBase64String(passwordSalt);

          byte[] passwordHash = KeyDerivation.Pbkdf2(
            password: registrationUser.Password,
            salt: Encoding.ASCII.GetBytes(passwordSaltAndKey),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 1000000,
            numBytesRequested: 256 / 8
          );

          sql = $@"
            INSERT INTO AppSchema.Auth(
              [Email],
              [PasswordHash],
              [PasswordSalt]
            ) VALUES (
              '{registrationUser.Email}',
              @PasswordHash,
              @PasswordSalt
            );
          ";

          List<SqlParameter> sqlParameters = new List<SqlParameter>();

          SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
          passwordHashParameter.Value = passwordHash;

          SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
          passwordSaltParameter.Value = passwordSalt;

          sqlParameters.Add(passwordHashParameter);
          sqlParameters.Add(passwordSaltParameter);

          if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
          {
            return Ok();
          }
          throw new Exception("Failed to register user");
        }
        throw new Exception("User already exists");
      }
      throw new Exception("Passwords do not match");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto loginDto)
    {
      return Ok();
    }
  }
}