
namespace QuoTra.Models
{
    public class CalendarSchedule
    {
        public string? id { get; set; }
        public string? refreshToken { get; set; }
    }

    public class CountryCompanyList
    {
        public List<CountryCompanyData> companyList { get; set; }=new List<CountryCompanyData>();
    }

    public class CountryCompanyData
    {
        public string? countryCode { get; set; }
        public string? countryName { get; set; }
        public List<CompanyData> companyData { get; set; } = new List<CompanyData>();
    }

    public class CompanyData
    {
        public string? companyCode { get; set; }
        public string? companyName { get; set; }
        public string? companyEnglishName { get; set; }
        public string? countryCode { get; set; }
        public string? countryName { get; set; }
        public string? Address1 { get; set; }
    }

}

