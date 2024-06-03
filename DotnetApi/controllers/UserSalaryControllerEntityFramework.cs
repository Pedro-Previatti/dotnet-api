using AutoMapper;
using DotnetApi.Models;
using DotnetAPI.Controllers;
using DotnetAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserSalaryControllerEntityFramework : ControllerBase
{
  IUserRepository _userRepository;
  IMapper _mapper;

  public UserSalaryControllerEntityFramework(IConfiguration config, IUserRepository userRepository)
  {
    _userRepository = userRepository;

    _mapper = new Mapper(new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<UserSalary, UserSalary>().ReverseMap();
    }));

  }

  [HttpGet("ef.get.user.salary/{id}")]
  public UserSalary GetUserSalary(int id)
  {
    return _userRepository.GetUserSalary(id);
  }

  [HttpPost("ef.post.user.salary")]
  public IActionResult PostUserSalary(UserSalary userSalary)
  {
    _userRepository.AddEntity<UserSalary>(userSalary);
    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Adding UserSalary failed on save");
  }

  [HttpPut("ef.put.user")]
  public IActionResult PutUserSalary(UserSalary userSalary)
  {
    UserSalary? userToUpdate = _userRepository.GetUserSalary(userSalary.UserId);

    if (userToUpdate != null)
    {
      _mapper.Map(userSalary, userToUpdate);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Updating UserSalary failed on save");
    }
    throw new Exception("Failed to find UserSalary to Update");
  }

  [HttpDelete("ef.delete.user/{id}")]
  public IActionResult DeleteUser(int id)
  {
    UserSalary? userToDelete = _userRepository.GetUserSalary(id);

    if (userToDelete != null)
    {
      _userRepository.RemoveEntity<UserSalary>(userToDelete);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Deleting UserSalary failed on save");
    }
    throw new Exception("Failed to find UserSalary to delete");
  }
}