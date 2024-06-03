using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserSalaryController : ControllerBase
{
  DataContextDapper _dapper;
  public UserSalaryController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  [HttpGet("get.user.salary/{id}")]
  public UserSalary GetUserSalary(int id)
  {
    string sql = $@"
    SELECT CAST([Salary] AS DECIMAL(10,2)) AS [Salary] 
    FROM  AppSchema.UserSalary
    WHERE [UserId] = {id};
    ";
    UserSalary userSalary = _dapper.LoadDataSingle<UserSalary>(sql);
    return userSalary;
  }

  [HttpPost("post.user.salary")]
  public IActionResult PostUserSalary(UserSalary userSalary)
  {
    string sql = $@"
    INSERT INTO AppSchema.UserSalary(
      [UserId], [Salary]
    ) VALUES (
      {userSalary.UserId},
      {userSalary.Salary}
    );
    ";
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }

    throw new Exception("Unable to post user's salary");
  }

  [HttpPut("put.user")]
  public IActionResult PutUserSalary(UserSalary userSalary)
  {
    string sql = $@"
    UPDATE AppSchema.Users
      SET 
        [Salary] = {userSalary.Salary}
    WHERE [UserId] = {userSalary.UserId};
    ";
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }

    throw new Exception("Unable to put to user's salary");
  }

  [HttpDelete("delete.user/{id}")]
  public IActionResult DeleteUser(int id)
  {
    string sql = $@"
    DELETE AppSchema.UserSalary
    WHERE [UserId] = {id};
    ";
    if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
    {
      return Ok();
    }

    throw new Exception("Unable to delete to user's salary");
  }
}