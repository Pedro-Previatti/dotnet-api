using DotnetApi.Dtos;
using DotnetAPI;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
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

  [HttpGet("get.users")]
  public IEnumerable<User> GetUsers()
  {
    string sql = @"
    SELECT [UserId],
      [FirstName],
      [LastName],
      [Email],
      [Gender],
      [Active] 
    FROM  AppSchema.Users;
    ";
    IEnumerable<User> users = _dapper.LoadData<User>(sql);
    return users;
  }

  [HttpGet("get.user/{id}")]
  public User GetUsers(int id)
  {
    string sql = $@"
    SELECT [UserId],
      [FirstName],
      [LastName],
      [Email],
      [Gender],
      [Active] 
    FROM  AppSchema.Users
    WHERE [UserId] = {id};
    ";
    User user = _dapper.LoadDataSingle<User>(sql);
    return user;
  }

  [HttpPost("post.user")]
  public IActionResult PostUser(UserDto user)
  {
    string sql = $@"
    INSERT INTO AppSchema.Users(
      [FirstName], [LastName], [Email], [Gender], [Active]
    ) VALUES (
      '{user.FirstName}',
      '{user.LastName}', 
      '{user.Email}',
      '{user.Gender}',
      '{user.Active}'
    );
    ";
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }

    throw new Exception("Unable to post user");
  }

  [HttpPut("put.user")]
  public IActionResult PutUser(User user)
  {
    string sql = $@"
    UPDATE AppSchema.Users
      SET 
        [FirstName] = '{user.FirstName}',
        [LastName] = '{user.LastName}', 
        [Email] = '{user.Email}',
        [Gender] = '{user.Gender}',
        [Active] = '{user.Active}'
    WHERE [UserId] = {user.UserId};
    ";
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }

    throw new Exception("Unable to put to user");
  }

  [HttpDelete("delete.user/{id}")]
  public IActionResult DeleteUser(int id)
  {
    string sql = $@"
    DELETE AppSchema.Users
    WHERE [UserId] = {id};
    ";
    if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
    {
      return Ok();
    }

    throw new Exception("Unable to delete to user");
  }
}
