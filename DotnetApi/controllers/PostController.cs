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
      string parameters = "";

      if (postId != 0)
      {
        parameters += $", @PostId={postId.ToString()}";
      }
      if (userId != 0)
      {
        parameters += $", @UserId={userId.ToString()}";
      }
      if (param != "None")
      {
        parameters += $", @SearchValue='{param}'";
      }

      if (parameters.Length > 0)
      {
        sql += parameters.Substring(1);
      }

      return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("my.posts")]
    public IEnumerable<Post> GetMyPosts()
    {
      string sql = $"EXEC AppSchema.spPosts_Get @UserId='{this.User.FindFirst("usr")?.Value}'";

      return _dapper.LoadData<Post>(sql);
    }

    [HttpPost("upsert.post")]
    public IActionResult UpsertPost(Post post)
    {
      string sql = @$"EXEC AppSchema.spPosts_Upsert
        @UserId={this.User.FindFirst("usr")?.Value},
        @PostTitle={post.PostTitle},
        @PostContent={post.PostContent}";

      if (post.PostId > 0)
      {
        sql += ", @PostId={post.PostId}";
      }

      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }
      throw new Exception("Failed to upsert post.");
    }

    [HttpDelete("delete.post/{userId}")]
    public IActionResult DeletePost(int postId = 0)
    {
      string sql = @$"EXEC AppSchema.spPosts_Delete
        @UserId={this.User.FindFirst("usr")?.Value}";

      if (postId > 0)
      {
        sql += ", @PostId={postId.ToString()}";
      }

      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }
      throw new Exception("Failed to delete post.");
    }
  }
}