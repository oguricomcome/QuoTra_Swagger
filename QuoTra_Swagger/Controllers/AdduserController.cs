
using Microsoft.AspNetCore.Mvc;
using QuoTra.CRYPTO;
using QuoTra.DAO;
using QuoTra.Models;
using System.Data;
using System.Security.Cryptography;
using System.Text;

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
        public async Task<IActionResult> Adduser([FromBody] ChallengeUser user)
        {
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

                ApiQuery apidao = new ApiQuery();
                DataTable dt = apidao.GetLoginUser(hashedLoginUser);

                if (dt != null && dt.Rows.Count > 0)
                {
                    System.Console.WriteLine("User Already Exits.");
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
                    tokenDurationMinutes = 60,
                    refreshTimes = 60,
                    appKeyIV1 = iv1
                };

                int count = apidao.AddLoginUser(hashedRecordUser);
                if (count == 1)
                {
                    SendUserDetail sendUserDetail = new SendUserDetail();
                    sendUserDetail.uid = hashedRecordUser.uuidHash;
                    apidao.AddUserData(sendUserDetail);
                    return Ok(encAppKey);
                }
                else return BadRequest();
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("UserDetail")]
        public async Task<IActionResult> UserDetail([FromBody] SendUserDetail user)
        {

            ApiQuery apidao = new ApiQuery();

            bool isSuccess = apidao.UpdateUserData(user);
            if (isSuccess)
            {
                return Ok();
            }
            else return BadRequest();
        }
    }
}
