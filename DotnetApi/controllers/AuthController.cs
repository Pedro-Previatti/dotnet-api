using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly DataContextDapper _dapper;
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
      _config = config;
    }

    [AllowAnonymous]
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

          byte[] passwordHash = GetPasswordHash(registrationUser.Password, passwordSalt);

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

            string addUserSql = $@"
              INSERT INTO AppSchema.Users(
                [FirstName], [LastName], [Email], [Gender], [Active]
              ) VALUES (
                '{registrationUser.FirstName}',
                '{registrationUser.LastName}', 
                '{registrationUser.Email}',
                '{registrationUser.Gender}',
                1
              );
            ";
            if (_dapper.ExecuteSql(addUserSql))
            {
              return Ok();
            }
            throw new Exception("Failed to add user");
          }
          throw new Exception("Failed to register user");
        }
        throw new Exception("User already exists");
      }
      throw new Exception("Passwords do not match");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login(LoginDto loginUser)
    {
      string sql = @$"
        SELECT [PasswordHash],
          [PasswordSalt] 
        FROM AppSchema.Auth WHERE Email = '{loginUser.Email}'
      ";
      LoginConfirmationDto registeredUser = _dapper.LoadDataSingle<LoginConfirmationDto>(sql);

      byte[] passwordHash = GetPasswordHash(loginUser.Password, registeredUser.PasswordSalt);

      for (int i = 0; i < passwordHash.Length; i++)
      {
        if (passwordHash[i] != registeredUser.PasswordHash[i])
        {
          return StatusCode(401, "Incorrect password");
        }
      }

      string userIdSql = @$"
        SELECT UserId 
        FROM AppSchema.Users 
        WHERE Email = '{loginUser.Email}'
      ";
      int userId = _dapper.LoadDataSingle<int>(userIdSql);

      return Ok(new Dictionary<string, string> {
        {"token", CreateToken(userId)}
      });
    }

    [HttpGet("refresh.token")]
    public IActionResult RefreshToken()
    {
      string? userIdClaim = User.FindFirst("usr")?.Value;
      if (string.IsNullOrEmpty(userIdClaim))
      {
        return Unauthorized("Invalid token: user ID not found.");
      }

      int userId;
      if (!int.TryParse(userIdClaim, out userId))
      {
        return Unauthorized("Invalid token: user ID is not valid.");
      }

      string sql = $@"SELECT UserId FROM AppSchema.Users WHERE UserId = {userId}";
      int userIdFromDb;
      try
      {
        userIdFromDb = _dapper.LoadDataSingle<int>(sql);
      }
      catch (InvalidOperationException)
      {
        return NotFound("User not found");
      }

      return Ok(new { token = CreateToken(userIdFromDb) });
    }

    private byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
      string passwordSaltAndKey = _config.GetSection("AppSettings:PasswordKey").Value +
        Convert.ToBase64String(passwordSalt);

      return KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(passwordSaltAndKey),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 1000000,
            numBytesRequested: 256 / 8
          );
    }

    private string CreateToken(long userId)
    {
      Claim[] claims = new Claim[] {
        new Claim("usr", userId.ToString())
      };

      string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
      SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
          tokenKeyString != null ? tokenKeyString : ""
        )
      );

      SigningCredentials credentials = new SigningCredentials(
        tokenKey,
        SecurityAlgorithms.HmacSha512Signature
      );

      SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
      {
        Subject = new ClaimsIdentity(claims),
        SigningCredentials = credentials,
        Expires = DateTime.Now.AddDays(1)
      };

      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

      SecurityToken token = tokenHandler.CreateToken(descriptor);

      return tokenHandler.WriteToken(token);
    }
  }
}