using AutoMapper;
using DotnetApi.Data;
using DotnetApi.Models;
using DotnetAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserJobInfoControllerEntityFramework : ControllerBase
{
  IUserRepository _userRepository;
  IMapper _mapper;

  public UserJobInfoControllerEntityFramework(IConfiguration config, IUserRepository userRepository)
  {
    _userRepository = userRepository;

    _mapper = new Mapper(new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<UserJobInfo, UserJobInfo>().ReverseMap();
    }));

  }

  [HttpGet("ef.get.user.job.info/{id}")]
  public UserJobInfo GetUserJobInfo(int id)
  {
    return _userRepository.GetUserJobInfo(id);
  }

  [HttpPost("ef.post.user.job.info")]
  public IActionResult PostUserJobInfo(UserJobInfo userJobInfo)
  {
    _userRepository.AddEntity<UserJobInfo>(userJobInfo);
    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Adding UserJobInfo failed on save");
  }

  [HttpPut("ef.put.user.job.info")]
  public IActionResult PutUser(UserJobInfo userJobInfo)
  {
    UserJobInfo? userToUpdate = _userRepository.GetUserJobInfo(userJobInfo.UserId);

    if (userToUpdate != null)
    {
      _mapper.Map(userJobInfo, userToUpdate);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Updating UserJobInfo failed on save");
    }
    throw new Exception("Failed to find UserJobInfo to Update");
  }

  [HttpDelete("ef.delete.user.job.info/{id}")]
  public IActionResult DeleteUser(int id)
  {
    UserJobInfo? userToDelete = _userRepository.GetUserJobInfo(id);

    if (userToDelete != null)
    {
      _userRepository.RemoveEntity<UserJobInfo>(userToDelete);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Deleting UserJobInfo failed on save");
    }
    throw new Exception("Failed to find UserJobInfo to delete");
  }
}