
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoTra.DAO;
using QuoTra.Models;
using System.Data;
using System.Text.Json;

namespace ERP_DB_UTC60.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class CalendarController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CalendarController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //[HttpPost]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public IEnumerable<ProfileDetail> Post(Profile profile)
        //{
        //    ApiQuery apiDAO = new ApiQuery();

        //    DataTable dt = apiDAO.getProfileDetail(profile);

        //    List <ProfileDetail> profileDetails = new List <ProfileDetail>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        ProfileDetail profileDetail = new ProfileDetail();
        //        profileDetail.prefecture = row["Prefecture"].ToString();
        //        profileDetail.comment = row["Comment"].ToString();
        //        profileDetails.Add(profileDetail);
        //    }
        //    return profileDetails;
        //}

        [HttpPost]
        [Route("GetCalendarData")]
        public async Task<IActionResult> GetCalendarData([FromBody] SendUserDetail user)
        {

            ApiQuery apidao = new ApiQuery();

            bool isSuccess = apidao.UpdateUserData(user);
            if (isSuccess)
            {
                return Ok();
            }
            else return BadRequest();
        }


        [HttpPost]
        [Route("GetCompanyData")]
        public async Task<IActionResult> GetCompanyData()
        {

            CalendarQuery query = new CalendarQuery();

            var companyData = query.GetCompanyData();

            var countryCompanyDataDictionary = new Dictionary<string, CountryCompanyData>();

            foreach (DataRow row in companyData.Rows)
            {
                string countryCode = row["CountryCode"]?.ToString() ?? string.Empty;
                string countryName = row["CommonName"]?.ToString() ?? "UNKNOWN";

                // countryCode が null または空の場合、代替キーを使用
                string key = string.IsNullOrEmpty(countryCode) ? "UNKNOWN" : countryCode;

                if (!countryCompanyDataDictionary.ContainsKey(key))
                {
                    countryCompanyDataDictionary[key] = new CountryCompanyData
                    {
                        countryCode = string.IsNullOrEmpty(countryCode) ? null : countryCode,
                        countryName = countryName,
                        companyData = new List<CompanyData>() // ここで初期化
                    };
                }

                countryCompanyDataDictionary[key].companyData.Add(new CompanyData
                {
                    companyCode = row["CompanyCode"]?.ToString() ?? string.Empty,
                    companyName = row["CompanyName"]?.ToString() ?? string.Empty,
                    companyEnglishName = row["CompanyEnglishName"]?.ToString() ?? string.Empty,
                    countryCode = countryCode,
                    countryName = countryName,
                    Address1 = row["Address1"]?.ToString() ?? string.Empty
                });
            }

            if (countryCompanyDataDictionary.Values != null)
            {
                CountryCompanyList countryCompanyList = new CountryCompanyList();
                foreach (var countryCompany in countryCompanyDataDictionary)
                {
                    var countryData = new CountryCompanyData();
                    countryData.countryName = string.IsNullOrEmpty(countryCompany.Value.countryName) ? "" : countryCompany.Value.countryName;
                    countryData.countryCode = string.IsNullOrEmpty(countryCompany.Value.countryCode) ? "" : countryCompany.Value.countryCode;
                    countryData.companyData = countryCompany.Value.companyData;

                    countryCompanyList.companyList.Add(countryData);
                }

                // リストをJSON形式にシリアライズ
                string retJson = JsonSerializer.Serialize(countryCompanyList);
                return Ok(retJson);
            }
            else return BadRequest();
        }

    }
}
