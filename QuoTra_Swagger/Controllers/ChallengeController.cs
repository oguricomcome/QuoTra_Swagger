
using Microsoft.AspNetCore.Mvc;
using QuoTra.DAO;
using QuoTra.Models;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace QuoTra.Controllers
{

    [Route("[controller]")]

    public class ChallengeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ChallengeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Challenge([FromBody] ChallengeUser user)
        {
             return SendIV2(user);
        }

        [HttpPost]
        [Route("ChallengeClientCertificate")]
        public async Task<IActionResult> ChallengeClientCertificate([FromBody] ChallengeUser user)
        {
            return SendIV2(user);
        }

        private ActionResult SendIV2(ChallengeUser user)
        {
            if (!string.IsNullOrEmpty(user.deviceId) && !string.IsNullOrEmpty(user.uuid))
            {
                string iv2 = CRYPTO.CRYPTO.getIV();

                string pepperTxt = CRYPTO.CRYPTO.pepperTxt;
                byte[] pepperBytes = Encoding.UTF8.GetBytes(pepperTxt);

                byte[] hashBytesUUID = CRYPTO.CRYPTO.CreatePBKDF2Hash(user.uuid, pepperBytes);
                byte[] hashBytesDevID = CRYPTO.CRYPTO.CreatePBKDF2Hash(user.deviceId, pepperBytes);
                string hashTextUUID = Convert.ToBase64String(hashBytesUUID);
                string hashTextDevID = Convert.ToBase64String(hashBytesDevID);

                HashedRecordUser hashedRecordUser = new HashedRecordUser();
                hashedRecordUser.uuidHash = hashTextUUID;
                hashedRecordUser.deviceIdHash = hashTextDevID;
                hashedRecordUser.appKeyIV2 = iv2;

                int count = 0;

                try
                {
                    ApiQuery apidao = new ApiQuery();
                    count = apidao.SetIV2(hashedRecordUser);
                }
                catch (Exception ex) 
                {
                    if (ex.Message == "対象のレコードが存在しませんでした") return Unauthorized(ex.Message);
                    else return BadRequest(ex.Message);
                }

                if (count == 1) return Ok(iv2);
                else return BadRequest();
            }
            return BadRequest();
        }

    }
}
