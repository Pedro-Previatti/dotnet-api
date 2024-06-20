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
  }
}