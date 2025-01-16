
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoTra.DAO;
using QuoTra.Models;
using System.Data;
using System.Drawing;
using System.Text.Json;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using NUlid;
using System;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Data.SqlClient;
using static Azure.Core.HttpHeader;
using Microsoft.AspNetCore.Mvc.Formatters;

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
        BaseQuery baseQuery = new BaseQuery();

        [HttpPost]
        [Route("GetUserData")]
        public async Task<IActionResult> GetUserData([FromBody] SendUserDetail sendUserDetail)
        {
            string aaaaa = sendUserDetail.name;
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    Logger.WriteLog("INFO", "GetUserData 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    UserDataList userDataList = new UserDataList();
                    Logger.WriteLog("INFO-Query", "GetUserDataList 処理-開始");
                    var companyData = query.GetUserDataList(sendUserDetail);
                    Logger.WriteLog("INFO-Query", "GetUserDataList 処理-終了");
                    foreach (DataRow row in companyData.Rows)
                    {
                        byte[] data = row["Icon"] != DBNull.Value ? (byte[])row["Icon"] : new byte[0];

                        string sendUserDetailIcon = Convert.ToBase64String(data);

                        SendUserDetail userDetail = new SendUserDetail
                        {
                            uid = row["UUIDHash"]?.ToString() ?? string.Empty,
                            name = row["Name"]?.ToString() ?? string.Empty,
                            nickName = row["NickName"]?.ToString() ?? string.Empty,

                            Icon = sendUserDetailIcon,
                            roleId = row["RoleId"]?.ToString() ?? string.Empty,
                            mailAddress = row["MailAddress"]?.ToString() ?? string.Empty,
                            salesArea = row["SalesArea"]?.ToString() ?? string.Empty,
                        };
                        userDataList.sendUserDetails.Add(userDetail);

                    }
                    // リストをJSON形式にシリアライズ
                    string retJson = System.Text.Json.JsonSerializer.Serialize(userDataList);
                    Logger.WriteLog("INFO", "GetUserData 処理-完了");
                    return Ok(retJson);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetUserData 処理-エラー" + ex.ToString());
                    return BadRequest(ex);
                }
            }


        }

        [HttpPost]
        [Route("GetFavoriteUserData")]
        public async Task<IActionResult> GetFavoriteUserData([FromBody] List<string> uids)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    UserDataList userDataList = new UserDataList();
                    foreach (string uid in uids)
                    {
                        Logger.WriteLog("INFO", "GetUserData 処理-開始　リクエスト");
                        CalendarQuery query = new CalendarQuery(HttpContext, cn);
                        Logger.WriteLog("INFO-Query", "GetUserDataList 処理-開始");
                        var companyData = query.GetFavoriteUserDataList(uid);
                        Logger.WriteLog("INFO-Query", "GetUserDataList 処理-終了");
                        foreach (DataRow row in companyData.Rows)
                        {
                            byte[] data = row["Icon"] != DBNull.Value ? (byte[])row["Icon"] : new byte[0];

                            string sendUserDetailIcon = Convert.ToBase64String(data);

                            SendUserDetail userDetail = new SendUserDetail
                            {
                                uid = row["UUIDHash"]?.ToString() ?? string.Empty,
                                name = row["Name"]?.ToString() ?? string.Empty,
                                nickName = row["NickName"]?.ToString() ?? string.Empty,

                                Icon = sendUserDetailIcon,
                                roleId = row["RoleId"]?.ToString() ?? string.Empty,
                                mailAddress = row["MailAddress"]?.ToString() ?? string.Empty,
                                salesArea = row["SalesArea"]?.ToString() ?? string.Empty,
                            };
                            userDataList.sendUserDetails.Add(userDetail);

                        }
                    }

                    // リストをJSON形式にシリアライズ
                    string retJson = System.Text.Json.JsonSerializer.Serialize(userDataList);
                    Logger.WriteLog("INFO", "GetUserData 処理-完了");
                    return Ok(retJson);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetUserData 処理-エラー" + ex.ToString());
                    return BadRequest(ex);
                }
            }


        }


        [HttpPost]
        [Route("GetCompanyData")]
        public async Task<IActionResult> GetCompanyData()
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "GetCompanyData 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);

                    Logger.WriteLog("INFO-Query", "GetCompanyData 処理-開始");
                    var companyData = query.GetCompanyData();
                    Logger.WriteLog("INFO-Query", "GetCompanyData 処理-完了");

                    var SalesAreaCompanyDataDictionary = new Dictionary<string, SalesAreaCompanyData>();

                    foreach (DataRow row in companyData.Rows)
                    {
                        string SalesArea = row["SalesArea"]?.ToString() ?? string.Empty;

                        // countryCode が null または空の場合、代替キーを使用
                        string key = string.IsNullOrEmpty(SalesArea) ? "UNKNOWN" : SalesArea;

                        if (!SalesAreaCompanyDataDictionary.ContainsKey(key))
                        {
                            SalesAreaCompanyDataDictionary[key] = new SalesAreaCompanyData
                            {
                                SalesAreaName = SalesArea,
                                companyData = new List<CompanyData>() // ここで初期化
                            };
                        }

                        try
                        {
                            SalesAreaCompanyDataDictionary[key].companyData.Add(new CompanyData
                            {
                                companyCode = row["CompanyCode"]?.ToString() ?? string.Empty,
                                companyName = row["CompanyName"]?.ToString() ?? string.Empty,
                                companyEnglishName = row["CompanyEnglishName"]?.ToString() ?? string.Empty,
                                SalesArea = SalesArea,
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);

                        }


                    }

                    if (SalesAreaCompanyDataDictionary.Values != null)
                    {
                        SalesAreaCompanyList SalesAreaCompanyList = new SalesAreaCompanyList();
                        foreach (var countryCompany in SalesAreaCompanyDataDictionary)
                        {
                            var countryData = new SalesAreaCompanyData();
                            countryData.SalesAreaName = string.IsNullOrEmpty(countryCompany.Value.SalesAreaName) ? "" : countryCompany.Value.SalesAreaName;
                            countryData.companyData = countryCompany.Value.companyData;

                            SalesAreaCompanyList.companyList.Add(countryData);
                        }

                        // リストをJSON形式にシリアライズ
                        string retJson = System.Text.Json.JsonSerializer.Serialize(SalesAreaCompanyList);
                        Logger.WriteLog("INFO", "GetCompanyData 処理-完了");

                        return Ok(retJson);
                    }
                    else return BadRequest();
                }
                catch (Exception e)
                {
                    Logger.WriteLog("Error", "GetCompanyData エラー" + e.ToString());
                    return BadRequest(e);

                }
            }




        }

        [HttpPost]
        [Route("GetCompanySection")]
        public async Task<IActionResult> GetCompanySection([FromBody] CompanyData args)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "GetCompanySection 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);

                    Logger.WriteLog("INFO-Query", "GetCompanySection 処理-開始");
                    var CompanySectionTable = query.GetCompanySection(args.companyCode);
                    Logger.WriteLog("INFO-Query", "GetCompanySection 処理-終了");

                    if (CompanySectionTable.Rows.Count == 0)
                    {
                        throw new ArgumentException("DataTable is empty.");
                    }

                    // 最初の行から基本情報を取得
                    DataRow firstRow = CompanySectionTable.Rows[0];
                    var companySectionData = new CompanySectionList
                    {
                        companyCode = firstRow["CompanyCode"].ToString(),
                        companyName = firstRow["CompanyName"].ToString(),
                        companyEnglishName = firstRow["CompanyEnglishName"].ToString(),
                        Address1 = firstRow["Address1"].ToString(),
                        SalesArea = firstRow["SalesArea"].ToString()
                    };



                    // 各行のセクション情報を追加
                    foreach (DataRow row in CompanySectionTable.Rows)
                    {
                        var sectionData = new CompanySectionData
                        {
                            SectionCode = row["SectionCode"].ToString(),
                            SectionName = row["Name"].ToString(),
                            SectionShortName = row["ShortName"].ToString()
                        };

                        Logger.WriteLog("INFO-Query", "GetCompanySection 処理-開始");
                        var CustomerRepresentativeTable = query.GetCustomerRepresentative(args.companyCode, sectionData.SectionCode);
                        Logger.WriteLog("INFO-Query", "GetCompanySection 処理-終了");
                        if (CustomerRepresentativeTable.Rows.Count == 0)
                        {
                            continue;
                        }

                        foreach (DataRow crRow in CustomerRepresentativeTable.Rows)
                        {
                            if (string.IsNullOrEmpty(crRow["Attention"].ToString()))
                            {

                                continue;
                            }
                            var customerData = new CustomerRepresentativeData
                            {
                                CRCode = crRow["CRCode"].ToString(),
                                CRName = crRow["Attention"].ToString(),
                                Sections = crRow["SectionNames"].ToString(),
                            };

                            //// すべてのセクションをチェックして一致するCRCodeを持つセクションコードを集める
                            //var sectionCodes = CompanySectionTable.AsEnumerable()
                            //    .Where(secRow => query.GetCustomerRepresentative(args.companyCode, secRow["SectionCode"].ToString())
                            //        .AsEnumerable().Any(cr => cr["CRCode"].ToString() == customerData.CRCode))
                            //    .Select(secRow => secRow["Name"].ToString())
                            //    .Distinct();

                            //customerData.Sections = string.Join(",", sectionCodes);

                            sectionData.CustomerRepresentativeList.Add(customerData);
                        }

                        companySectionData.SectionList.Add(sectionData);
                    }



                    // CompanySectionListをJSON形式に変換
                    string retJSON = System.Text.Json.JsonSerializer.Serialize(companySectionData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });


                    Logger.WriteLog("INFO", "GetCompanySection 処理-完了");
                    return Ok(retJSON);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetCompanySection エラー" + ex.ToString());
                    return BadRequest();
                }
            }


        }

        [HttpPost]
        [Route("GetColor")]
        public async Task<IActionResult> GetColor()
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "GetColor 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);

                    Logger.WriteLog("INFO-Query", "GetColorData 処理-開始");
                    string retJsonData = query.GetColorData();
                    Logger.WriteLog("INFO-Query", "GetColorData 処理-終了");

                    //if (ColorDataTable.Rows.Count == 0)
                    //{
                    //    throw new ArgumentException("DataTable is empty.");
                    //}
                    CalendarColorList calendarColors = new CalendarColorList();

                    //// 各行のカラー情報を追加
                    //foreach (DataRow row in ColorDataTable.Rows)
                    //{
                    //    var ColorData = new CalendarColor();
                    //    ColorData.colorId = row["ColorId"].ToString();
                    //    ColorData.red = int.Parse(row["red"].ToString());
                    //    ColorData.green = int.Parse(row["green"].ToString());
                    //    ColorData.blue = int.Parse(row["blue"].ToString());
                    //    ColorData.hex_code = row["hex_code"].ToString();

                    //    calendarColors.calendarColorsList.Add(ColorData);
                    //}
                    //string json = JsonConvert.SerializeObject(calendarColors, Newtonsoft.Json.Formatting.Indented);
                    Logger.WriteLog("INFO", "GetColor 処理-終了");

                    return Ok(retJsonData);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetColor エラー" + ex.ToString());
                    return BadRequest();
                }
            }


        }


        [HttpPost]
        [Route("GetPurpose")]
        public async Task<IActionResult> GetPurpose()
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    Logger.WriteLog("INFO", "GetPurpose 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);

                    Logger.WriteLog("INFO-Query", "GetPurposeData 処理-開始");
                    string jsonData = query.GetPurposeData(true);
                    Logger.WriteLog("INFO-Query", "GetPurposeData 処理-完了");
                    //if (PurposeDataTable.Rows.Count == 0)
                    //{
                    //    throw new ArgumentException("DataTable is empty.");
                    //}
                    CalendarPurposeList calendarPurpose = new CalendarPurposeList();
                    return Ok(jsonData);
                    calendarPurpose = JsonConvert.DeserializeObject<CalendarPurposeList>(jsonData);
                    // 各行のPurpose情報を追加
                    //foreach (DataRow row in PurposeDataTable.Rows)
                    //{
                    //    var PurposeData = new CalendarPurpose();
                    //    PurposeData.purposeCode = row["PurposeId"].ToString();
                    //    PurposeData.purposeName = row["PurposeName"].ToString();
                    //    PurposeData.purposeShortName = row["PurposeShortName"].ToString();
                    //    PurposeData.isSelectetd = 0;

                    //    calendarPurpose.calendarPurposeList.Add(PurposeData);
                    //}
                    if (calendarPurpose != null)
                    {
                    }
                    string json = JsonConvert.SerializeObject(calendarPurpose, Newtonsoft.Json.Formatting.Indented);

                    Logger.WriteLog("INFO", "GetPurpose 処理-完了");
                    return Ok(jsonData);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetPurpose エラー" + ex.ToString());
                    return BadRequest();
                }
            }


        }
        [HttpPost]
        [Route("GetSection")]
        public async Task<IActionResult> GetSection()
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();
                try
                {
                    Logger.WriteLog("INFO", "GetSection 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);

                    Logger.WriteLog("INFO-Query", "GetSection 処理-開始");
                    string jsonData = query.GetSectionData(true);
                    Logger.WriteLog("INFO-Query", "GetSection 処理-完了");
                    //if (PurposeDataTable.Rows.Count == 0)
                    //{
                    //    throw new ArgumentException("DataTable is empty.");
                    //}
                    CalendarPurposeList calendarPurpose = new CalendarPurposeList();
                    return Ok(jsonData);

                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetPurpose エラー" + ex.ToString());
                    return BadRequest();
                }
            }


        }

        [HttpPost]
        [Route("RegisterSchedule")]
        public async Task<IActionResult> RegisterSchedule(CalendarSchedule schedule)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "RegisterSchedule 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    if (!string.IsNullOrEmpty(schedule.scheduleId))
                    {
                        query.DeleteCalendarData(schedule.scheduleId);
                    }


                    // ULIDを生成する
                    Ulid ulid = Ulid.NewUlid();

                    string U = ulid.ToString();
                    // Scheduleテーブルインサート
                    Logger.WriteLog("INFO-Query", "RegisterSchedule 処理-開始");
                    bool isSuccess = query.RegisterSchedule(schedule, ulid);
                    Logger.WriteLog("INFO-Query", "RegisterSchedule 処理-完了");


                    Logger.WriteLog("INFO-Query", "RegisterScheduleMemo 処理-開始");
                    // Memoのインサート
                    foreach (var memoData in schedule.notes)
                    {

                        isSuccess = query.RegisterScheduleMemo(memoData, ulid);
                    }
                    Logger.WriteLog("INFO-Query", "RegisterScheduleMemo 処理-完了");

                    // CustRepのインサート
                    Logger.WriteLog("INFO-Query", "RegisterScheduleCustRep 処理-開始");
                    foreach (var sectionData in schedule.sectionDataList)
                    {
                        isSuccess = query.RegisterScheduleCustRep(sectionData, ulid);
                    }
                    Logger.WriteLog("INFO-Query", "RegisterScheduleCustRep 処理-完了");

                    // CustRepのインサート

                    Logger.WriteLog("INFO-Query", "RegisterSchedulePurPose 処理-開始");
                    foreach (var purposeData in schedule.calendarPurpose)
                    {
                        isSuccess = query.RegisterSchedulePurPose(purposeData, ulid);
                    }
                    Logger.WriteLog("INFO-Query", "RegisterSchedulePurPose 処理-完了");

                    string retJson = JsonConvert.SerializeObject(ulid);

                    Logger.WriteLog("INFO", "RegisterSchedule 処理-完了");
                    return Ok(retJson);
                }
                catch (Exception ex)
                {

                    Logger.WriteLog("Error", "RegisterSchedule エラー" + ex.ToString());
                    return BadRequest();
                }
            }


        }
        /// <summary>
        /// Schedule詳細取得
        /// </summary>
        /// <param name="targetScheduleData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSchedule")]
        public IActionResult GetSchedule(TargetSchedule targetScheduleData)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "GetSchedule Dayスケジュール取得処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    string CalendarScheduleTable = query.GetDetailedCalendaSchedule(targetScheduleData);

                    CalendarScheduleList calendarScheduleList = JsonConvert.DeserializeObject<CalendarScheduleList>(CalendarScheduleTable);

                    string json = JsonConvert.SerializeObject(calendarScheduleList, Newtonsoft.Json.Formatting.Indented);

                    Logger.WriteLog("INFO", "GetSchedule Dayスケジュール取得処理-完了");
                    return Ok(json);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetSchedule Dayスケジュール 取得処理 エラー" + ex.ToString());
                    return BadRequest();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetScheduleData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDaySchedule")]
        public IActionResult GetDaySchedule(TargetSchedule targetScheduleData)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "GetDaySchedule Dayスケジュール取得処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    string CalendarScheduleTable = query.GetDetailedCalendarDaySchedule(targetScheduleData);

                    Calendar calendarScheduleList = JsonConvert.DeserializeObject<Calendar>(CalendarScheduleTable);
                    MonthlyCalendar monthlyCalendar = new MonthlyCalendar();
                    monthlyCalendar.calendarDate = targetScheduleData.targetData;
                    monthlyCalendar.schedules = calendarScheduleList.schedules;
                    string json = JsonConvert.SerializeObject(monthlyCalendar, Newtonsoft.Json.Formatting.Indented);

                    Logger.WriteLog("INFO", "GetDaySchedule Dayスケジュール取得処理-完了");
                    return Ok(json);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "GetDaySchedule Dayスケジュール 取得処理 エラー" + ex.ToString());
                    return BadRequest();
                }
            }
        }

        /// <summary>
        /// 1ヶ月分のスケジュールデータ取得
        /// </summary>
        /// <param name="targetScheduleData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetLocalSchedule")]
        public IActionResult SetLocalSchedule(TargetSchedule targetScheduleData)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "SetLocalSchedule 1ヶ月スケジュール取得処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    string CalendarScheduleTable = query.GetDetailedCalendarSchedule(targetScheduleData);

                    Calendar calendarScheduleList = JsonConvert.DeserializeObject<Calendar>(CalendarScheduleTable);
                    MonthlyCalendar monthlyCalendar = new MonthlyCalendar();
                    monthlyCalendar.calendarDate = targetScheduleData.targetData;
                    monthlyCalendar.schedules = calendarScheduleList.schedules;
                    string json = JsonConvert.SerializeObject(monthlyCalendar, Newtonsoft.Json.Formatting.Indented);

                    Logger.WriteLog("INFO", "SetLocalSchedule Dayスケジュール取得処理-完了");
                    return Ok(json);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Error", "SetLocalSchedule Dayスケジュール 取得処理 エラー" + ex.ToString());
                    return BadRequest();
                }

            }
        }

        //public async Task<IActionResult> GetDaySchedule(TargetSchedule targetScheduleData)
        //{
        //    using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
        //    {
        //        cn.Open();

        //        try
        //        {
        //            Logger.WriteLog("INFO", "GetDaySchedule Dayスケジュール取得処理-開始　リクエスト");
        //            CalendarQuery query = new CalendarQuery(HttpContext, cn);

        //            var calendarScheduleList = new CalendarScheduleList();
        //            Logger.WriteLog("INFO-Query", "GetCalendarSchedule 処理-開始");
        //            var CalendarScheduleTable = query.GetCalendarDaySchedule(targetScheduleData);
        //            Logger.WriteLog("INFO-Query", "GetCalendarSchedule 処理-完了");

        //            // 各行のスケジュール情報を追加
        //            foreach (DataRow row in CalendarScheduleTable.Rows)
        //            {
        //                string scheduleId = row["ScheduleId"].ToString();
        //                // 会社データ取得
        //                var companyData = new CompanyData
        //                {
        //                    companyCode = row["CompanyCode"].ToString(),
        //                    companyName = row["CompanyName"].ToString(),
        //                    companyEnglishName = row["CompanyEnglishName"].ToString(),
        //                    Address1 = row["Address1"].ToString(),
        //                    SalesArea = row["SalesArea"].ToString(),

        //                };
        //                // カラーデータ取得
        //                var calendarColor = new CalendarColor
        //                {
        //                    colorId = row["ColorId"].ToString(),
        //                    red = int.Parse(row["red"].ToString()),
        //                    green = int.Parse(row["green"].ToString()),
        //                    blue = int.Parse(row["blue"].ToString()),
        //                    hex_code = row["hex_code"].ToString(),

        //                };
        //                // メモデータ取得

        //                Logger.WriteLog("INFO-Query", "GetCalendarScheduleNotes 処理-開始");
        //                var ScheduleNotesTable = query.GetCalendarScheduleNotes(scheduleId);
        //                Logger.WriteLog("INFO-Query", "GetCalendarScheduleNotes 処理-完了");
        //                List<ScheduleMemo> Notes = new List<ScheduleMemo>();
        //                foreach (DataRow noteRow in ScheduleNotesTable.Rows)
        //                {
        //                    // メモ取得
        //                    var memoData = new ScheduleMemo
        //                    {
        //                        MemoId = noteRow["MemoId"].ToString(),
        //                        Content = noteRow["Memo"].ToString(),
        //                    };

        //                    Notes.Add(memoData);
        //                }

        //                // 部門　担当者取得
        //                var companySections = new List<CompanySectionData>();
        //                Logger.WriteLog("INFO-Query", "GetCalendarScheduleSection 処理-開始");
        //                var ScheduleSectionTable = query.GetCalendarScheduleSection();
        //                Logger.WriteLog("INFO-Query", "GetCalendarSchedulePurPose 処理-完了");
        //                foreach (DataRow sectionRow in ScheduleSectionTable.Rows)
        //                {
        //                    string sectionCode = sectionRow["SectionCode"].ToString();
        //                    var ScheduleCustomerTable = query.GetCalendarScheduleCustomer(scheduleId, sectionCode);
        //                    List<CustomerRepresentativeData> customerRepresentatives = new List<CustomerRepresentativeData>();
        //                    foreach (DataRow customerRow in ScheduleCustomerTable.Rows)
        //                    {
        //                        var customer = new CustomerRepresentativeData
        //                        {
        //                            CRCode = customerRow["CRCode"].ToString(),
        //                            CRName = customerRow["Attention"].ToString(),
        //                            Sections = customerRow["SectionNames"].ToString(),
        //                        };
        //                        customerRepresentatives.Add(customer);

        //                    }
        //                    var companySection = new CompanySectionData
        //                    {
        //                        SectionCode = sectionRow["SectionCode"].ToString(),
        //                        SectionName = sectionRow["Name"].ToString(),
        //                        SectionShortName = sectionRow["ShortName"].ToString(),
        //                        CustomerRepresentativeList = customerRepresentatives,
        //                    };

        //                    companySections.Add(companySection);
        //                }

        //                // Perpose取得

        //                Logger.WriteLog("INFO-Query", "GetCalendarSchedulePurPose 処理-開始");
        //                var SchedulePurposesTable = query.GetCalendarSchedulePurPose(scheduleId);
        //                Logger.WriteLog("INFO-Query", "GetCalendarSchedulePurPose 処理-完了");
        //                List<CalendarPurpose> purposes = new List<CalendarPurpose>();
        //                foreach (DataRow PurposesRow in SchedulePurposesTable.Rows)
        //                {
        //                    // カラーデータ取得
        //                    var calendarPurpose = new CalendarPurpose
        //                    {
        //                        purposeCode = PurposesRow["PurposeId"].ToString(),
        //                        purposeName = PurposesRow["PurposeName"].ToString(),
        //                        purposeShortName = PurposesRow["PurposeShortName"].ToString(),
        //                        isSelected = int.Parse(PurposesRow["IsSelected"].ToString()),
        //                    };

        //                    purposes.Add(calendarPurpose);
        //                }



        //                var calendarSchedule = new CalendarSchedule
        //                {
        //                    uUIDHash = row["UUIDHash"].ToString(),
        //                    scheduleId = row["ScheduleId"].ToString(),
        //                    startTime = DateTime.Parse(row["StartTime"].ToString()).ToString("o"),
        //                    endTime = DateTime.Parse(row["EndTime"].ToString()).ToString("o"),
        //                    companyData = companyData,
        //                    color = calendarColor,
        //                    notes = Notes,
        //                    sectionDataList = companySections,
        //                    calendarPurpose = purposes,

        //                };

        //                calendarScheduleList.calendarScheduleList.Add(calendarSchedule);

        //            }
        //            string json = JsonConvert.SerializeObject(calendarScheduleList, Newtonsoft.Json.Formatting.Indented);

        //            Logger.WriteLog("INFO", "GetDaySchedule Dayスケジュール取得処理-完了");
        //            return Ok(json);
        //        }
        //        catch (Exception ex)
        //        {

        //            Logger.WriteLog("Error", "GetDaySchedule Dayスケジュール 取得処理 エラー" + ex.ToString());
        //            return BadRequest();
        //        }
        //    }


        //}

        [HttpPost]
        [Route("GetWeekSchedule")]
        public async Task<IActionResult> GetWeekSchedule(TargetSchedule targetScheduleData)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "GetWeekSchedule 週表示取得 処理-開始　リクエスト");
                    Logger.WriteLog("INFO", "日付情報" + targetScheduleData.targetData.ToString());
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    string CalendarScheduleTable = query.GetCalendarWeakSchedule(targetScheduleData);

                    Calendar calendarScheduleList = JsonConvert.DeserializeObject<Calendar>(CalendarScheduleTable);

                    string json = JsonConvert.SerializeObject(calendarScheduleList, Newtonsoft.Json.Formatting.Indented);

                    Logger.WriteLog("INFO", "GetWeekSchedule Dayスケジュール取得処理-完了");
                    return Ok(json);
                }
                catch (Exception ex)
                {

                    Logger.WriteLog("Error", "GetWeekSchedule エラー" + ex.ToString());
                    return BadRequest();
                }
            }


        }

        [HttpPost]
        [Route("GetMonthSchedule")]
        //public async Task<IActionResult> GetMonthSchedule(TargetSchedule targetScheduleData)
        //{
        //    using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
        //    {
        //        cn.Open();
        //        try
        //        {
        //            Logger.WriteLog("INFO", "GetSchedule 処理-開始　リクエスト");
        //            CalendarQuery query = new CalendarQuery(HttpContext, cn);

        //            var calendarScheduleList = new CalendarScheduleList();
        //            Logger.WriteLog("INFO-Query", "GetCalendarSchedule 処理-開始");
        //            var CalendarScheduleTable = query.GetCalendarMonthSchedule(targetScheduleData);
        //            Logger.WriteLog("INFO-Query", "GetCalendarSchedule 処理-完了");
        //            // 各行のスケジュール情報を追加
        //            foreach (DataRow row in CalendarScheduleTable.Rows)
        //            {
        //                string scheduleId = row["ScheduleId"].ToString();
        //                // 会社データ取得
        //                var companyData = new CompanyData
        //                {
        //                    companyCode = row["CompanyCode"].ToString(),
        //                    companyName = row["CompanyName"].ToString(),
        //                    companyEnglishName = row["CompanyEnglishName"].ToString(),
        //                    Address1 = row["Address1"].ToString(),
        //                    SalesArea = row["SalesArea"].ToString(),

        //                };
        //                // カラーデータ取得
        //                var calendarColor = new CalendarColor
        //                {
        //                    colorId = row["ColorId"].ToString(),
        //                    red = int.Parse(row["red"].ToString()),
        //                    green = int.Parse(row["green"].ToString()),
        //                    blue = int.Parse(row["blue"].ToString()),
        //                    hex_code = row["hex_code"].ToString(),

        //                };

        //                var calendarSchedule = new CalendarSchedule
        //                {
        //                    uUIDHash = row["UUIDHash"].ToString(),
        //                    scheduleId = row["ScheduleId"].ToString(),
        //                    startTime = DateTime.Parse(row["StartTime"].ToString()).ToString("o"),
        //                    endTime = DateTime.Parse(row["EndTime"].ToString()).ToString("o"),
        //                    companyData = companyData,
        //                    color = calendarColor,
        //                };

        //                calendarScheduleList.calendarScheduleList.Add(calendarSchedule);
        //            }
        //            string json = JsonConvert.SerializeObject(calendarScheduleList, Newtonsoft.Json.Formatting.Indented);

        //            Logger.WriteLog("INFO", "GetCalendarSchedule 処理-完了");
        //            return Ok(json);
        //        }
        //        catch (Exception ex)
        //        {

        //            Logger.WriteLog("Error", "GetCalendarSchedule エラー" + ex.ToString());
        //            return BadRequest();
        //        }

        //    }


        //}

        [HttpPost]
        [Route("DeleteSchedule")]
        public async Task<IActionResult> DeleteSchedule(CalendarSchedule schedule)
        {
            using (SqlConnection cn = new SqlConnection(baseQuery.connectionString))
            {
                cn.Open();

                try
                {
                    Logger.WriteLog("INFO", "DeleteSchedule 処理-開始　リクエスト");
                    CalendarQuery query = new CalendarQuery(HttpContext, cn);
                    if (!string.IsNullOrEmpty(schedule.scheduleId))
                    {

                        Logger.WriteLog("INFO-Query", "DeleteCalendarData 処理-開始");
                        query.DeleteCalendarData(schedule.scheduleId);
                        Logger.WriteLog("INFO-Query", "DeleteCalendarData 処理-完了");

                    }
                    Logger.WriteLog("INFO", "DeleteSchedule 処理-完了");
                    return Ok(query);
                }
                catch (Exception ex)
                {

                    Logger.WriteLog("Error", "DeleteSchedule エラー" + ex.ToString());
                    return BadRequest();
                }

            }

        }

    }
}
