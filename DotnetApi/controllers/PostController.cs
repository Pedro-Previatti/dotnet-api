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

    [HttpGet("get.posts")]
    public IEnumerable<Post> GetPosts()
    {
      string sql = @"
        SELECT 
          [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated] 
        FROM AppSchema.Posts
      ";

      return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("get.post/{postId}")]
    public Post GetPost(int postId)
    {
      string sql = @$"
        SELECT 
          [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated] 
        FROM AppSchema.Posts
        WHERE PostId = {postId.ToString()}
      ";

      return _dapper.LoadDataSingle<Post>(sql);
    }

    [HttpGet("get.user.posts/{userId}")]
    public IEnumerable<Post> GetUserPosts(string userId)
    {
      string sql = @$"
        SELECT 
          [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated] 
        FROM AppSchema.Posts
        WHERE UserId = {userId.ToString()}
      ";

      return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("get.my.posts")]
    public IEnumerable<Post> GetMyPosts()
    {
      string sql = @$"
        SELECT 
          [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated] 
        FROM AppSchema.Posts
        WHERE UserId = {this.User.FindFirst("usr")?.Value}
      ";

      return _dapper.LoadData<Post>(sql);
    }

    [HttpPost("add.post")]
    public IActionResult AddPost(PostToAddDto post)
    {
      string sql = @$"
        INSERT INTO AppSchema.Posts (
          [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
        ) VALUES (
          {this.User.FindFirst("usr")?.Value},
          '{post.PostTitle}',
          '{post.PostContent}',
          GETDATE(),
          GETDATE()
        )
      ";

      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }
      throw new Exception("Failed to create post.");
    }

    [HttpPut("edit.post")]
    public IActionResult EditPost(PostToEditDto post)
    {
      string sql = @$"
        UPDATE AppSchema.Posts
          SET 
            [PostTitle] = '{post.PostTitle}',
            [PostContent] = '{post.PostContent}',
            [PostUpdated] = GETDATE()
        WHERE [PostId] = {post.PostId.ToString()}
        AND [UserId] = {this.User.FindFirst("usr")?.Value};
      ";

      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }
      throw new Exception("Failed to edit post.");
    }

    [HttpDelete("delete.post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
      string sql = @$"
        DELETE FROM AppSchema.Posts 
        WHERE [PostId] = {postId.ToString()}
        AND [UserId] = {this.User.FindFirst("usr")?.Value}
      ";

      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }
      throw new Exception("Failed to delete post.");
    }

    [HttpGet("search.post/{searchParam}")]
    public IEnumerable<Post> SearchPosts(string searchParam)
    {
      string sql = @$"
        SELECT 
          [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated] 
        FROM AppSchema.Posts
        WHERE [PostTitle] LIKE '%{searchParam}%'
        OR [PostContent] LIKE '%{searchParam}%'
      ";

      return _dapper.LoadData<Post>(sql);
    }
  }
}