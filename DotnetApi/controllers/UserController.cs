using DotnetAPI;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : Controller
{
  DataContextDapper _dapper;
  public UserController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  [HttpGet("test.connection")]
  public DateTime TestConnection()
  {
    return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
  }

  [HttpGet("get.user/{user}")]
  public string[] GetUsers(string user)
  {
    return new string[] { "user1", "user2", user };
  }

  [HttpGet("get.users")]
  public string[] GetUsers()
  {
    return new string[] { "user1", "user2" };
  }

}
