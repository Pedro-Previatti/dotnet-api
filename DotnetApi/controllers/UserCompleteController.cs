using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class UserCompleteController : ControllerBase
  {
    DataContextDapper _dapper;
    public UserCompleteController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
    }

    [HttpGet("get.users/{id}/{active}")]
    public IEnumerable<UserComplete> GetUsers(int id, bool active)
    {
      string sql = "EXEC AppSchema.spUser_Get";
      string parameters = "";

      if (id != 0)
      {
        parameters += $", @UserId={id.ToString()}";
      }
      if (active)
      {
        parameters += $", @Active={active.ToString()}";
      }

      if (parameters.Length > 0)
      {
        sql += parameters.Substring(1);
      }

      IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
      return users;
    }

    [HttpPut("upsert.user")]
    public IActionResult UpsertUser(UserComplete user)
    {
      string sql = $@"EXEC AppSchema.spUser_Upsert 
        @FirstName='{user.FirstName}',
        @LastName='{user.LastName}', 
        @Email='{user.Email}',
        @Gender='{user.Gender}',
        @JobTitle='{user.JobTitle}',
        @Department='{user.Department}',
        @Salary='{user.Salary}',
        @Active='{user.Active}',
        @UserId='{user.UserId}'";
      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }

      throw new Exception("Unable to upsert user");
    }

    [HttpDelete("delete.user/{id}")]
    public IActionResult DeleteUser(int id)
    {
      string sql = $@"AppSchema.spUser_Delete
    @UserId={id.ToString()}";
      if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
      {
        return Ok();
      }

      throw new Exception("Unable to delete to user");
    }
  }
}