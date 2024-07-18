using System.Data;
using Dapper;
using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class PostController : ControllerBase
  {
    private readonly DataContextDapper _dapper;
    public PostController(IConfiguration config)
    {
      _dapper = new(config);
    }

    [HttpGet("get.posts/{postId}/{userId}/{param}")]
    public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string param = "None")
    {
      string sql = "EXEC AppSchema.spPosts_Get";
      string stringParameters = "";

      DynamicParameters parameters = new DynamicParameters();

      if (postId != 0)
      {
        stringParameters += ", @PostId=@PostIdParameter";
        parameters.Add("@PostIdParameter", postId, DbType.Int32);
      }
      if (userId != 0)
      {
        stringParameters += ", @UserId=@UserIdParameter";
        parameters.Add("@UserIdParameter", userId, DbType.Int32);
      }
      if (param != "None")
      {
        stringParameters += ", @SearchValue=@SearchValueParameter";
        parameters.Add("@SearchValueParameter", param, DbType.String);
      }

      if (stringParameters.Length > 0)
      {
        sql += stringParameters.Substring(1);
      }

      return _dapper.LoadDataWithParameters<Post>(sql, parameters);
    }

    [HttpGet("my.posts")]
    public IEnumerable<Post> GetMyPosts()
    {
      string sql = "EXEC AppSchema.spPosts_Get @UserId=@UserIdParameter";
      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("@UserIdParameter", this.User.FindFirst("usr")?.Value, DbType.Int32);

      return _dapper.LoadDataWithParameters<Post>(sql, parameters);
    }

    [HttpPost("upsert.post")]
    public IActionResult UpsertPost(Post post)
    {
      string sql = @"EXEC AppSchema.spPosts_Upsert
        @UserId=@UserIdParameter,
        @PostTitle=@PostTitleParameter
        @PostContent=@PostContentParameter";

      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("@UserIdParameter", this.User.FindFirst("usr")?.Value, DbType.Int32);
      parameters.Add("@PostTitleParameter", post.PostTitle, DbType.String);
      parameters.Add("@PostContentParameter", post.PostContent, DbType.String);

      if (post.PostId > 0)
      {
        sql += ", @PostId=@PostIdParameter";
        parameters.Add("@PostContentParameter", post.PostId, DbType.Int32);
      }

      if (_dapper.ExecuteSqlWithParameters(sql, parameters))
      {
        return Ok();
      }

      throw new Exception("Failed to upsert post.");
    }

    [HttpDelete("delete.post/{userId}")]
    public IActionResult DeletePost(int postId)
    {
      string sql = @"EXEC AppSchema.spPosts_Delete
        @UserId=@UserIdParameter,
        @PostId=@PostIdParameter";

      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("@UserIdParameter", this.User.FindFirst("usr")?.Value, DbType.Int32);
      parameters.Add("@PostIdParameter", postId, DbType.Int32);

      if (_dapper.ExecuteSqlWithParameters(sql, parameters))
      {
        return Ok();
      }

      throw new Exception("Failed to delete post.");
    }
  }
}