using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetApi.Data
{
  class DataContextDapper
  {
    private readonly IConfiguration _config;
    public DataContextDapper(IConfiguration config)
    {
      _config = config;
    }

    public IEnumerable<T> LoadData<T>(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      return dbConnection.Query<T>(sql);
    }

    public T LoadDataSingle<T>(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      return dbConnection.QuerySingle<T>(sql);
    }

    public bool ExecuteSql(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      return dbConnection.Execute(sql) > 0;
    }

    public int ExecuteSqlWithRowCount(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      return dbConnection.Execute(sql);
    }

    public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> parameters)
    {
      SqlCommand commandWithParams = new SqlCommand(sql);

      foreach (SqlParameter parameter in parameters)
      {
        commandWithParams.Parameters.Add(parameter);
      }

      SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      dbConnection.Open();

      commandWithParams.Connection = dbConnection;

      int affectedRows = commandWithParams.ExecuteNonQuery();

      dbConnection.Close();

      return affectedRows > 0;
    }
  }
}