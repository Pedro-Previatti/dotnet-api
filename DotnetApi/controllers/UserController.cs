using System.Data;
using Dapper;
using DotnetApi.Data;
using DotnetApi.Helpers;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class UserController : ControllerBase
  {
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;
    public UserController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
      _reusableSql = new ReusableSql(config);
    }

    [HttpGet("get.users/{id}/{active}")]
    public IEnumerable<User> GetUsers(int id, bool active)
    {
      string sql = "EXEC AppSchema.spUser_Get";
      string sqlParameters = "";
      DynamicParameters parameters = new DynamicParameters();

      if (id != 0)
      {
        sqlParameters += ", @UserId=@UserIdParameter";
        parameters.Add("@UserIdParameter", id, DbType.Int32);
      }
      if (active)
      {
        sqlParameters += ", @Active=@ActiveParameter";
        parameters.Add("@ActiveParameter", active, DbType.Int32);
      }

      if (sqlParameters.Length > 0)
      {
        sql += sqlParameters.Substring(1);
      }

      IEnumerable<User> users = _dapper.LoadDataWithParameters<User>(sql, parameters);
      return users;
    }

    [HttpPut("upsert.user")]
    public IActionResult UpsertUser(User user)
    {
      if (_reusableSql.UpsertUser(user))
      {
        return Ok();
      }

      throw new Exception("Unable to upsert user");
    }

    [HttpDelete("delete.user/{id}")]
    public IActionResult DeleteUser(int id)
    {
      string sql = $@"AppSchema.spUser_Delete
        @UserId=@UserIdParameter";

      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("@UserIdParameter", id, DbType.Int32);

      if (_dapper.ExecuteSqlWithParameters(sql, parameters))
      {
        return Ok();
      }

      throw new Exception("Unable to delete to user");
    }
  }
}