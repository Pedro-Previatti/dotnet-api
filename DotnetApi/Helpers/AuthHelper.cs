using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Helpers
{
  public class AuthHelper
  {
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;
    public AuthHelper(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
      _config = config;
    }

    public byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
      string passwordSaltAndKey = _config.GetSection("AppSettings:PasswordKey").Value +
        Convert.ToBase64String(passwordSalt);

      return KeyDerivation.Pbkdf2(
        password: password,
        salt: Encoding.ASCII.GetBytes(passwordSaltAndKey),
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 1000000,
        numBytesRequested: 256 / 8
      );
    }

    public string CreateToken(long userId)
    {
      Claim[] claims = new Claim[] {
        new("usr", userId.ToString())
      };

      string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
      SymmetricSecurityKey tokenKey = new(
        Encoding.UTF8.GetBytes(
          tokenKeyString ?? ""
        )
      );

      SigningCredentials credentials = new(
        tokenKey,
        SecurityAlgorithms.HmacSha512Signature
      );

      SecurityTokenDescriptor descriptor = new()
      {
        Subject = new ClaimsIdentity(claims),
        SigningCredentials = credentials,
        Expires = DateTime.Now.AddDays(1)
      };

      JwtSecurityTokenHandler tokenHandler = new();

      SecurityToken token = tokenHandler.CreateToken(descriptor);

      return tokenHandler.WriteToken(token);
    }

    public bool SetPassword(LoginDto user)
    {
      byte[] passwordSalt = new byte[128 / 8];
      using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
      {
        rng.GetNonZeroBytes(passwordSalt);
      }

      byte[] passwordHash = GetPasswordHash(user.Password, passwordSalt);

      string sql = @"AppSchema.spRegistration_Upsert
                @Email=@EmailParam,
                @PasswordHash=@PHashParam,
                @PasswordSalt=@PSaltParam";

      List<SqlParameter> parameters = new List<SqlParameter>
      {
          new SqlParameter("@EmailParam", SqlDbType.NVarChar) { Value = user.Email },
          new SqlParameter("@PHashParam", SqlDbType.VarBinary) { Value = passwordHash },
          new SqlParameter("@PSaltParam", SqlDbType.VarBinary) { Value = passwordSalt }
      };

      return _dapper.ExecuteSqlWithParameters(sql, parameters);
    }
  }
}