
namespace QuoTra.Models
{

    public class TargetSchedule
    {
        public string uuid { get; set; }
        public string deviceId { get; set; }
        public string targetData { get; set; }
        public string scheduleId { get; set; }

    }
    public class SalesAreaCompanyList
    {
        public List<SalesAreaCompanyData> companyList { get; set; } = new List<SalesAreaCompanyData>();
    }

    public class SalesAreaCompanyData
    {
        public string? SalesAreaName { get; set; }
        public List<CompanyData> companyData { get; set; } = new List<CompanyData>();
    }

    public class CompanyData
    {
        public string companyCode { get; set; } = "";
        public string? companyName { get; set; }
        public string? companyEnglishName { get; set; }
        public string? Address1 { get; set; }
        public string? SalesArea { get; set; }

    }

    public class CompanySectionList : CompanyData
    {
        public List<CompanySectionData> SectionList { get; set; } = new List<CompanySectionData>();

    }

    public class CompanySectionData
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public string SectionShortName { get; set; }
        public List<CustomerRepresentativeData> CustomerRepresentativeList { get; set; } = new List<CustomerRepresentativeData>();

    }

    public class CustomerRepresentativeData
    {
        public string CRCode { get; set; }
        public string CRName { get; set; }

        public string Sections { get; set; } = string.Empty;

    }

    public class CalendarColorList
    {
        public List<CalendarColor> calendarColorsList { get; set; } = new List<CalendarColor>();
    }

    public class CalendarColor
    {
        public string colorId { get; set; }
        public int red { get; set; }
        public int green { get; set; }
        public int blue { get; set; }
        public string hex_code { get; set; }
    }

    public class CalendarPurposeList
    {
        public List<CalendarPurpose> calendarPurposeList { get; set; } = new List<CalendarPurpose>();
    }

    public class CalendarPurpose
    {
        public string purposeCode { get; set; }
        public string purposeName { get; set; }
        public string purposeShortName { get; set; }
        public int isSelected { get; set; }
    }

    public class CalendarScheduleList
    {
        public List<CalendarSchedule> calendarScheduleList { get; set; } = new List<CalendarSchedule>();
    }

    public class CalendarSchedule
    {
        public string uUIDHash { get; set; }
        public string scheduleId { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public CompanyData companyData { get; set; } = new CompanyData();
        public CalendarColor color { get; set; } = new CalendarColor();
        public List<ScheduleMemo> notes { get; set; } = new List<ScheduleMemo>();
        public List<CompanySectionData> sectionDataList { get; set; } = new List<CompanySectionData>();
        public List<CalendarPurpose> calendarPurpose { get; set; } = new List<CalendarPurpose>();
    }

    public class ScheduleMemo
    {
        public string MemoId { get; set; }
        public string Content { get; set; }
    }
    public class MonthlyCalendar
    {
        public List<Schedule> schedules { get; set; } = new List<Schedule>();
        public string calendarDate { get; set; } = string.Empty;

    }

    public class Calendar
    {
        public List<Schedule> schedules { get; set; } = new List<Schedule>();

    }


    public class Schedule
    {
        public string uUIDHash { get; set; } = string.Empty;
        public string scheduleId { get; set; } = string.Empty;
        public string startTime { get; set; } = string.Empty;
        public string endTime { get; set; } = string.Empty;
        public string companyName { get; set; } = string.Empty;
        public string colorCode { get; set; } = string.Empty;
        public List<Purpose> selectPurposeList { get; set; } = new List<Purpose>();
        public List<CustomerRepresentative> selectCustomerRepresentativeList { get; set; } = new List<CustomerRepresentative>();
        public List<note> noteList { get; set; } = new List<note>();

    }

    public class Purpose
    {
        public string selectPurposeId { get; set; } = string.Empty;
    }

    public class CustomerRepresentative
    {
        public string CustomerRepresentativeId { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }

    public class note
    {
        public string comment { get; set; } = string.Empty;
    }

}

