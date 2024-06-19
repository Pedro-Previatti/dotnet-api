using AutoMapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers
{

  [ApiController]
  [Route("[controller]")]
  public class EFUserController : ControllerBase
  {
    IUserRepository _userRepository;
    IMapper _mapper;

    public EFUserController(IConfiguration config, IUserRepository userRepository)
    {
      _userRepository = userRepository;

      _mapper = new Mapper(new MapperConfiguration(cfg =>
      {
        cfg.CreateMap<UserDto, User>();
      }));

    }

    [HttpGet("ef.get.users")]
    public IEnumerable<User> GetUsers()
    {
      IEnumerable<User> users = _userRepository.GetUsers();
      return users;
    }

    [HttpGet("ef.get.user/{userId}")]
    public User GetUser(int userId)
    {
      return _userRepository.GetUser(userId);
    }

    [HttpPost("ef.post.user")]
    public IActionResult PostUser(UserDto user)
    {
      User userDb = _mapper.Map<User>(user);

      _userRepository.AddEntity<User>(userDb);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }

      throw new Exception("Failed to Add User");
    }

    [HttpPut("ef.put.user")]
    public IActionResult PutUser(User user)
    {
      User? userDb = _userRepository.GetUser(user.UserId);

      if (userDb != null)
      {
        userDb.Active = user.Active;
        userDb.FirstName = user.FirstName;
        userDb.LastName = user.LastName;
        userDb.Email = user.Email;
        userDb.Gender = user.Gender;
        if (_userRepository.SaveChanges())
        {
          return Ok();
        }

        throw new Exception("Failed to Update User");
      }

      throw new Exception("Failed to Get User");
    }

    [HttpDelete("ef.delete.user/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
      User? userDb = _userRepository.GetUser(userId);

      if (userDb != null)
      {
        _userRepository.RemoveEntity<User>(userDb);
        if (_userRepository.SaveChanges())
        {
          return Ok();
        }

        throw new Exception("Failed to Delete User");
      }

      throw new Exception("Failed to Get User");
    }
  }
}