using System.Data;
using Dapper;
using DotnetApi.Data;
using DotnetApi.Models;

namespace DotnetApi.Helpers
{
  public class ReusableSql
  {
    private readonly DataContextDapper _dapper;
    public ReusableSql(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
    }

    public bool UpsertUser(User user)
    {
      string sql = @"EXEC AppSchema.spUser_Upsert 
        @FirstName=@FirstNameParam,
        @LastName=@LastNameParam, 
        @Email=@EmailParam,
        @Gender=@GenderParam,
        @Active=@ActiveParam,
        @JobTitle=@JobTitleParam,
        @Department=@DepartmentParam,
        @Salary=@SalaryParam,
        @UserId=0";

      DynamicParameters parameters = new DynamicParameters();

      parameters.Add("@FirstNameParam", user.FirstName, DbType.String);
      parameters.Add("@LastNameParam", user.LastName, DbType.String);
      parameters.Add("@EmailParam", user.Email, DbType.String);
      parameters.Add("@GenderParam", user.Gender, DbType.String);
      parameters.Add("@ActiveParam", user.Active, DbType.Boolean);
      parameters.Add("@JobTitleParam", user.JobTitle, DbType.String);
      parameters.Add("@DepartmentParam", user.Department, DbType.Decimal);
      parameters.Add("@SalaryParam", user.Salary, DbType.Int32);

      return _dapper.ExecuteSqlWithParameters(sql, parameters);
    }
  }
}