using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QuoTra.DAO;
using QuoTra.Models;
using System;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuoTra.Controllers
{


    [ApiController]
    [Route("[controller]")]


    public class ContinuousLoginController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public ContinuousLoginController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        public async Task<IActionResult> ContinuousLogin([FromBody] LoginUser user)
        {

            var baseQuery = new BaseQuery();

            Logger.WriteLog("INFO", "ContinuousLogin -開始 リクエスト");
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    HashedRecordUser loggedInUser = authentication(user);

                    if (loggedInUser == null) return Unauthorized();

                    int TokenDurationMinutes = loggedInUser.tokenDurationMinutes;

                    var claims = new[]
                   {
                new Claim(ClaimTypes.Name, loggedInUser.deviceIdHash),
                new Claim(ClaimTypes.SerialNumber, loggedInUser.uuidHash),
                new Claim(ClaimTypes.Role, loggedInUser.role)
            };

                    var token = CreateToken(claims.ToList(), TokenDurationMinutes);

                    var refreshToken = GenerateRefreshToken();
                    loggedInUser.refreshToken = refreshToken;
                    loggedInUser.refreshTokenExpiryTime = DateTime.Now.AddMinutes(TokenDurationMinutes);

                    int count = 0;
                    ApiQuery dao = new ApiQuery(HttpContext, cn);

                    count = dao.SetRefreashToken(loggedInUser);


                    Logger.WriteLog("INFO", "ContinuousLogin -完了");
                    return Ok(new
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        RefreshToken = refreshToken,
                        TokenDurationMinutes = loggedInUser.tokenDurationMinutes,
                        uuid = loggedInUser.uuidHash
                    });
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "ContinuousLogin -エラー" + ex.ToString());
                    Console.WriteLine(ex.ToString());
                }
                Logger.WriteLog("INFO", "ContinuousLogin -完了");
                return Ok();
            }

        }

        [HttpPost]
        [Route("AcquireClientCertificate")]
        public async Task<IActionResult> AcquireClientCertificate([FromBody] LoginUser user)
        {
            Logger.WriteLog("INFO", "AcquireClientCertificate -開始 リクエスト");
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    HashedRecordUser loggedInUser = authentication(user);

                    if (loggedInUser == null) return Unauthorized();

                    CRYPTO.CRYPTO crypt = new CRYPTO.CRYPTO();

                    // IVの生成
                    byte[] iv;
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(user.deviceId));
                    }
                    Array.Resize(ref iv, 16); // 最初の16バイトを使用

                    // クライアント証明書の読み込み
                    byte[] certByteData;
                    using (FileStream fs = new FileStream("Certificate/RenrakuCL.p12", FileMode.Open, FileAccess.Read))
                    {
                        certByteData = new byte[fs.Length];
                        fs.Read(certByteData, 0, (int)fs.Length);
                    }

                    string ivdevid = Convert.ToBase64String(iv);
                    string certData = Convert.ToBase64String(certByteData);

                    string encClirntCert = crypt.aesEncrypt(certData, ivdevid);
                    Logger.WriteLog("INFO", "AcquireClientCertificate -完了");
                    return Ok(
                        encClirntCert
                    );
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("ERROR", "AcquireClientCertificate -エラー" + ex.ToString());
                    return BadRequest(ex.ToString());
                }
            }


        }

        private HashedRecordUser authentication(LoginUser user)
        {
            Logger.WriteLog("INFO", "authentication -開始");
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    if (!string.IsNullOrEmpty(user.deviceId) && !string.IsNullOrEmpty(user.encAppKey))
                    {
                        CRYPTO.CRYPTO crypt = new CRYPTO.CRYPTO();

                        string pepperTxt = CRYPTO.CRYPTO.pepperTxt;
                        byte[] pepperBytes = Encoding.UTF8.GetBytes(pepperTxt);

                        byte[] hashBytesUUID = CRYPTO.CRYPTO.CreatePBKDF2Hash(user.uuid, pepperBytes);
                        byte[] hashBytesDevID = CRYPTO.CRYPTO.CreatePBKDF2Hash(user.deviceId, pepperBytes);
                        string hashTextUUID = Convert.ToBase64String(hashBytesUUID);
                        string hashTextDevID = Convert.ToBase64String(hashBytesDevID);

                        HashedLoginUser hashedLoginUser = new HashedLoginUser();
                        hashedLoginUser.uuidHash = hashTextUUID;
                        hashedLoginUser.deviceIdHash = hashTextDevID;

                        ApiQuery apidao = new ApiQuery(HttpContext, cn);
                        DataTable dt = apidao.GetLoginUser(hashedLoginUser);
                        if (dt == null || dt.Rows.Count != 1 || dt.Rows[0]["AppKeyIV1"] == DBNull.Value) return null;

                        DataRow userdr = dt.Rows[0];

                        HashedRecordUser loggedInUser = new HashedRecordUser
                        {
                            uuidHash = userdr["UUIDHash"].ToString(),
                            deviceIdHash = userdr["DeviceIdHash"].ToString(),
                            role = userdr["Role"].ToString(),
                            refreshTimes = Int32.Parse(userdr["RefreshTimes"].ToString()),
                            tokenDurationMinutes = int.Parse(userdr["TokenDurationMinutes"].ToString())
                        };

                        string IV1 = userdr["AppKeyIV1"].ToString();
                        string IV2 = userdr["AppKeyIV2"].ToString();

                        string decappkey = string.Empty;
                        try
                        {
                            decappkey = crypt.aesDecrypt(crypt.aesDecrypt(user.encAppKey, IV2), IV1);
                        }
                        catch (Exception e)
                        {
                            return null;
                        }

                        // 認証の際は入力された文字列と保存していたsaltを使用してハッシュを生成
                        byte[] inputHashBytes = CRYPTO.CRYPTO.CreatePBKDF2Hash(decappkey, Convert.FromBase64String(userdr["AppKeySalt"].ToString()));
                        string inputHashText = Convert.ToBase64String(inputHashBytes);

                        // 保存していたハッシュと入力文字から生成したハッシュを比較して認証を行う
                        if (userdr["AppKeyHash"].ToString() != inputHashText) return null;

                        else
                        {
                            Logger.WriteLog("INFO", "authentication -完了");
                            return loggedInUser;
                        }
                    }
                    else
                    {

                        Logger.WriteLog("ERROR", "authentication -エラー");
                        return null;
                    }

                }
                catch (Exception e)
                {
                    Logger.WriteLog("ERROR", "authentication -エラー" + e.ToString());
                    return null;
                }
            }


        }

        [HttpPost]
        [Route("refresh-token")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            Logger.WriteLog("INFO", "RefreshToken -開始 リクエスト");
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    if (tokenModel is null)
                    {
                        return BadRequest("Invalid client request");
                    }

                    string? accessToken = tokenModel.accessToken;
                    string? refreshToken = tokenModel.refreshToken;

                    var principal = GetPrincipalFromExpiredToken(accessToken);
                    if (principal == null)
                    {
                        return BadRequest("Invalid access token or refresh token");
                    }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    string deviceidHash = principal.Identity.Name;
                    string uuidHash = principal.Claims.Where(c => c.Type == ClaimTypes.SerialNumber).Select(c => c.Value).SingleOrDefault();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    DataTable dt = new DataTable();
                    //     ApplicationUser user = new ApplicationUser();
                    HashedRecordUser user = new HashedRecordUser();

                    ApiQuery dao = new ApiQuery(HttpContext, cn);
                    user.deviceIdHash = deviceidHash;
                    user.uuidHash = uuidHash;
                    dt = dao.GetRefreashToken(user);

                    DateTime ExpiryTime = DateTime.Parse(dt.Rows[0]["RefreshTokenExpiryTime"].ToString());
                    string RegisteredRefreshToken = dt.Rows[0]["RefreshToken"].ToString();
                    int TokenDurationMinutes = int.Parse(dt.Rows[0]["TokenDurationMinutes"].ToString());

                    if (dt == null || dt.Rows.Count != 1 || RegisteredRefreshToken != refreshToken || ExpiryTime < DateTime.Now)
                    {
                        return BadRequest("Invalid access token or refresh token");
                    }

                    var newAccessToken = CreateToken(principal.Claims.ToList(), TokenDurationMinutes);
                    var newRefreshToken = GenerateRefreshToken();

                    user.refreshToken = newRefreshToken;
                    user.refreshTokenExpiryTime = DateTime.Now.AddMinutes(TokenDurationMinutes);

                    int count = 0;
                    count = dao.SetRefreashToken(user);
                    if (count == 0) { return BadRequest("Invalid client request"); }
                    Logger.WriteLog("INFO", "RefreshToken -完了");

                    return new ObjectResult(new
                    {
                        accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                        refreshToken = newRefreshToken,
                        uuid = uuidHash,
                    });
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("ERROR", "RefreshToken -エラー" + ex.ToString());

                    return BadRequest();

                }
            }


        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims, int tokenDurationMinutes)
        {
            Logger.WriteLog("INFO", "CreateToken -開始 ");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddMinutes(tokenDurationMinutes),
                notBefore: DateTime.UtcNow,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            Logger.WriteLog("INFO", "CreateToken -完了");
            return token;
        }

        public static string GenerateRefreshToken()
        {
            Logger.WriteLog("INFO", "GenerateRefreshToken -開始");
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            Logger.WriteLog("INFO", "GenerateRefreshToken -完了");
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            Logger.WriteLog("INFO", "GetPrincipalFromExpiredToken -開始");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])),
                ValidateLifetime = false
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            Logger.WriteLog("INFO", "GetPrincipalFromExpiredToken -完了");

            return principal;

        }

        [HttpPost]
        [Route("GetLoginUserData")]
        public async Task<IActionResult> GetLoginUserData([FromBody] ChallengeUser userdata)
        {
            Logger.WriteLog("INFO", "GetLoginUserData -開始 リクエスト");
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    ApiQuery apidao = new ApiQuery(HttpContext, cn);

                    DataTable dt = apidao.GetLoginUserData(userdata.uuid);
                    SendUserDetail sendUserDetail = new SendUserDetail();
                    sendUserDetail.uid = dt.Rows[0]["UUIDHash"].ToString();
                    sendUserDetail.name = dt.Rows[0]["Name"].ToString();
                    sendUserDetail.nickName = dt.Rows[0]["NickName"].ToString();
                    byte[] data = dt.Rows[0]["Icon"] != DBNull.Value ? (byte[])dt.Rows[0]["Icon"] : new byte[0];

                    sendUserDetail.Icon = Convert.ToBase64String(data);
                    sendUserDetail.roleId = dt.Rows[0]["RoleId"].ToString();
                    sendUserDetail.mailAddress = dt.Rows[0]["MailAddress"].ToString();
                    sendUserDetail.salesArea = dt.Rows[0]["SalesArea"].ToString();
                    if (sendUserDetail != null)
                    {
                        string jsonString = JsonSerializer.Serialize(sendUserDetail);
                        Logger.WriteLog("INFO", "GetLoginUserData -完了");
                        return Ok(jsonString);
                    }
                    else {
                        Logger.WriteLog("ERROR", "GetLoginUserData -エラー");

                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {

                    Logger.WriteLog("ERROR", "GetLoginUserData -エラー"+ex.ToString());
                    return BadRequest(ex.ToString());
                }
            }

        }
    }
}