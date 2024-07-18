using System.Data;
using AutoMapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Helpers;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetApi.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _authHelper;
    private readonly ReusableSql _reusableSql;
    private readonly IMapper _mapper;

    public AuthController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
      _authHelper = new AuthHelper(config);
      _reusableSql = new ReusableSql(config);
      _mapper = new Mapper(new MapperConfiguration(cfg =>
      {
        cfg.CreateMap<RegistrationDto, User>();
      }));
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
          LoginDto passwordSetupUser = new LoginDto()
          {
            Email = registrationUser.Email,
            Password = registrationUser.Password
          };
          if (_authHelper.SetPassword(passwordSetupUser))
          {
            User user = _mapper.Map<User>(registrationUser);
            if (_reusableSql.UpsertUser(user))
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

    [HttpPost("reset.password")]
    public IActionResult ResetPassword(LoginDto user)
    {
      if (_authHelper.SetPassword(user))
      {
        return Ok();
      }
      throw new Exception("Failed to update password");
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

      byte[] passwordHash = _authHelper.GetPasswordHash(loginUser.Password, registeredUser.PasswordSalt);

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
        {"token", _authHelper.CreateToken(userId)}
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

      return Ok(new { token = _authHelper.CreateToken(userIdFromDb) });
    }
  }
}