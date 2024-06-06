using DotnetApi.Models;

namespace DotnetApi.Data
{
  public interface IUserRepository
  {
    public bool SaveChanges();
    public void AddEntity<T>(T entityToAdd);
    public void RemoveEntity<T>(T entityToAdd);
    public IEnumerable<User> GetUsers();
    public User GetUser(int userId);
    public UserSalary GetUserSalary(int userId);
    public UserJobInfo GetUserJobInfo(int userId);
  }
}