
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using QuoTra.CRYPTO;
using QuoTra.DAO;
using QuoTra.Models;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace QuoTra.Controllers
{



    [Route("[controller]")]

    public class AdduserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdduserController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }



        [HttpPost]
        [Route("APIConnectionTest")]
        public async Task<IActionResult> APIConnectionTest()
        {


            return Ok("Connection OK");


        }
        [HttpPost]
        [Route("DBConnectionTest")]
        public async Task<IActionResult> DBConnectionTest()
        {
            var baseQuery = new BaseQuery();
            try
            {
                using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
                {
                    cn.Open();
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    var test = query.TEST();
                    string ret = test.Rows.Count.ToString();
                    Logger.WriteLog("INFO", "TEST");
                    return Ok(ret);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }


        [HttpPost]
        public async Task<IActionResult> Adduser([FromBody] ChallengeUser user)
        {
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    Logger.WriteLog("INFO", "Adduser 処理-開始 リクエスト");
                    if (!string.IsNullOrEmpty(user.deviceId) && !string.IsNullOrEmpty(user.uuid))
                    {
                        string appKey = CRYPTO.Password.Generate(24, 1) + "#";

                        string iv1 = CRYPTO.CRYPTO.getIV();

                        CRYPTO.CRYPTO crypt = new CRYPTO.CRYPTO();
                        string encAppKey = crypt.aesEncrypt(appKey, iv1);

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

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            System.Console.WriteLine("User Already Exits.");
                            Logger.WriteLog("Error", "Adduser 処理-終了 User Already Exits.");
                            return BadRequest("User Already Exists.");
                        }

                        // ソルトを生成する
                        byte[] saltBytes = CRYPTO.CRYPTO.CreateSalt();

                        // PBKDF2によるハッシュを生成
                        byte[] hashBytesAK = CRYPTO.CRYPTO.CreatePBKDF2Hash(appKey, saltBytes);

                        // Base64 文字列に変換
                        string saltText = Convert.ToBase64String(saltBytes);
                        string hashTextAK = Convert.ToBase64String(hashBytesAK);

                        HashedRecordUser hashedRecordUser = new HashedRecordUser()
                        {
                            uuidHash = hashedLoginUser.uuidHash,
                            deviceIdHash = hashedLoginUser.deviceIdHash,
                            appKeyHash = hashTextAK,
                            appKeySalt = saltText,
                            role = "User",
                            tokenDurationMinutes = 360,
                            refreshTimes = 60,
                            appKeyIV1 = iv1
                        };
                        Logger.WriteLog("INFO", "AddLoginUser 処理-開始 OK");
                        int count = apidao.AddLoginUser(hashedRecordUser);
                        Logger.WriteLog("INFO", "AddLoginUser 処理-完了 OK");
                        int AlreadyCount = apidao.GetUserProfilesCount(hashedRecordUser.uuidHash);
                        if (AlreadyCount == 0)
                        {
                            if (count == 1)
                            {
                                SendUserDetail sendUserDetail = new SendUserDetail();
                                sendUserDetail.uid = hashedRecordUser.uuidHash;

                                Logger.WriteLog("INFO", "AddUserData 処理-開始 OK");
                                apidao.AddUserData(sendUserDetail);
                                Logger.WriteLog("INFO", "AddUserData 処理-完了 OK");
                                Logger.WriteLog("Success", "Adduser 処理 - 完了");

                                return Ok(encAppKey);
                            }
                            else return BadRequest();
                        }
                        else
                        {
                            return Ok(encAppKey);
                        }

                    }
                    else
                    {
                        Logger.WriteLog("ERROR", "Adduser 処理-終了");
                        return BadRequest();
                    }

                }
                catch (Exception ex)
                {
                    Logger.WriteLog("ERROR", "Adduser 処理-終了" + ex.ToString());

                    BadRequest(ex);
                }
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("AdduserLogin")]
        public async Task<IActionResult> AdduserLogin([FromBody] ChallengeUser user)
        {
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    Logger.WriteLog("INFO", "Adduser 処理-開始 リクエスト");
                    if (!string.IsNullOrEmpty(user.deviceId) && !string.IsNullOrEmpty(user.uuid))
                    {
                        string appKey = CRYPTO.Password.Generate(24, 1) + "#";

                        string iv1 = CRYPTO.CRYPTO.getIV();

                        CRYPTO.CRYPTO crypt = new CRYPTO.CRYPTO();
                        string encAppKey = crypt.aesEncrypt(appKey, iv1);

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
                        //DataTable dt = apidao.GetLoginUser(hashedLoginUser);

                        //if (dt != null && dt.Rows.Count > 0)
                        //{
                        //    System.Console.WriteLine("User Already Exits.");
                        //    Logger.WriteLog("Error", "Adduser 処理-終了 User Already Exits.");
                        //    return BadRequest("User Already Exists.");
                        //}

                        // ソルトを生成する
                        byte[] saltBytes = CRYPTO.CRYPTO.CreateSalt();

                        // PBKDF2によるハッシュを生成
                        byte[] hashBytesAK = CRYPTO.CRYPTO.CreatePBKDF2Hash(appKey, saltBytes);

                        // Base64 文字列に変換
                        string saltText = Convert.ToBase64String(saltBytes);
                        string hashTextAK = Convert.ToBase64String(hashBytesAK);

                        HashedRecordUser hashedRecordUser = new HashedRecordUser()
                        {
                            uuidHash = hashedLoginUser.uuidHash,
                            deviceIdHash = hashedLoginUser.deviceIdHash,
                            appKeyHash = hashTextAK,
                            appKeySalt = saltText,
                            role = "User",
                            tokenDurationMinutes = 360,
                            refreshTimes = 60,
                            appKeyIV1 = iv1
                        };
                        Logger.WriteLog("INFO", "AddLoginUser 処理-開始 OK");
                        int count = apidao.AddLoginUser(hashedRecordUser);
                        Logger.WriteLog("INFO", "AddLoginUser 処理-完了 OK");
                        int AlreadyCount = apidao.GetUserProfilesCount(hashedRecordUser.uuidHash);
                        if (AlreadyCount == 0)
                        {
                            if (count == 1)
                            {
                                SendUserDetail sendUserDetail = new SendUserDetail();
                                sendUserDetail.uid = hashedRecordUser.uuidHash;

                                Logger.WriteLog("INFO", "AddUserData 処理-開始 OK");
                                apidao.AddUserData(sendUserDetail);
                                Logger.WriteLog("INFO", "AddUserData 処理-完了 OK");
                                Logger.WriteLog("Success", "Adduser 処理 - 完了");

                                return Ok(encAppKey);
                            }
                            else return BadRequest();
                        }
                        else
                        {
                            return Ok(encAppKey);
                        }

                    }
                    else
                    {
                        Logger.WriteLog("ERROR", "Adduser 処理-終了");
                        return BadRequest();
                    }

                }
                catch (Exception ex)
                {
                    Logger.WriteLog("ERROR", "Adduser 処理-終了" + ex.ToString());

                    BadRequest(ex);
                }
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("UppdateUserDetail")]
        public async Task<IActionResult> UppdateUserDetail([FromBody] SendUserDetail user)
        {
            var baseQuery = new BaseQuery();
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                Logger.WriteLog("INFO", "UserDetail -開始　リクエスト");
                try
                {
                    ApiQuery apidao = new ApiQuery(HttpContext, cn);

                    Logger.WriteLog("INFO", "UserDetail UPDATE処理-開始");
                    Logger.WriteLog("INFO", "メールアドレス これです" + user.mailAddress);
                    bool isSuccess = apidao.UpdateUserData(user);
                    Logger.WriteLog("INFO", "UserDetail UPDATE処理-完了");
                    if (isSuccess)
                    {
                        Logger.WriteLog("INFO", "UserDetail-完了");
                        return Ok();
                    }
                    else
                    {
                        Logger.WriteLog("INFO", "UserDetail-完了");
                        return BadRequest();
                    }

                }
                catch (Exception ex)
                {
                    Logger.WriteLog("ERROR", "UserDetail内でエラーが発生しました：" + ex.ToString());
                    return BadRequest(ex);
                }

            }
        }


    }


}
