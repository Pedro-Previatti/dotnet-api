using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserJobInfoController : ControllerBase
{
  DataContextDapper _dapper;

  public UserJobInfoController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  [HttpGet("get.users.job.info")]
  public IEnumerable<UserJobInfo> GetUsersJobInfo()
  {
    string sql = @"
    SELECT  [UserId],
      [JobTitle],
      [Department] 
    FROM  AppSchema.UserJobInfo;
    ";
    IEnumerable<UserJobInfo> usersJobInfo = _dapper.LoadData<UserJobInfo>(sql);
    return usersJobInfo;
  }

  [HttpGet("get.user.job.info")]
  public IEnumerable<UserJobInfo> GetUserJobInfo(int id)
  {
    string sql = $@"
    SELECT  [UserId],
      [JobTitle],
      [Department] 
    FROM  AppSchema.UserJobInfo
    WHERE [UserId] = {id};
    ";
    IEnumerable<UserJobInfo> userJobInfo = _dapper.LoadData<UserJobInfo>(sql);
    return userJobInfo;
  }

  [HttpPost("post.user.job.info")]
  public IActionResult PostUserJobInfo(UserJobInfo userJobInfo)
  {
    string sql = $@"
    INSERT INTO AppSchema.UserJobInfo(
      [UserId], [JobTitle], [Department] 
    ) VALUES (
      {userJobInfo.UserId},
      '{userJobInfo.JobTitle}',
      '{userJobInfo.Department}'
    );
    ";
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }

    throw new Exception("Unable to post user's job info");
  }

  [HttpPut("put.user.job.info")]
  public IActionResult PutUser(UserJobInfo userJobInfo)
  {
    string sql = $@"
    UPDATE AppSchema.UserJobInfo
      SET 
        [JobTitle] = '{userJobInfo.JobTitle}', 
        [Department] = '{userJobInfo.Department}'
    WHERE [UserId] = {userJobInfo.UserId}; 
    ";
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }

    throw new Exception("Unable to put to user's job info");
  }

  [HttpDelete("delete.user.job.info/{id}")]
  public IActionResult DeleteUser(int id)
  {
    string sql = $@"
    DELETE AppSchema.UserJobInfo
    WHERE [UserId] = {id};
    ";
    if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
    {
      return Ok();
    }

    throw new Exception("Unable to delete to user's job info");
  }
}