using QuoTra.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using NUlid;
using System.Reflection;
using System.Diagnostics;
using System.IO.Compression;

namespace QuoTra.DAO
{
    public class CalendarQuery
    {


        readonly SqlConnection cn;      // SQL接続文字列
        readonly SqlTransaction? tran;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sqlConnection"></param>
        public CalendarQuery(HttpContext httpContext, SqlConnection sqlConnection, SqlTransaction? sqlTransaction = null)
        {
            cn = sqlConnection;
            tran = sqlTransaction;
        }
        public DataTable TEST()
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                var sql = new StringBuilder() { Length = 0 };
                sql.AppendLine("SELECT TOP (1) [CompanyCode]");
                sql.AppendLine("  FROM [dbo].[M_Sales_Company]");

                cmd.CommandText = sql.ToString();

                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }

        }


        /// <summary>
        /// ユーザー一覧取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetUserDataList(SendUserDetail sendUserDetail)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT  [UUIDHash],[Name] ,[NickName],[RoleId],[Icon],[MailAddress],[SalesArea] ,[CreateDate],[AccontCode] ,[DeleteFlag]");
                sql.AppendLine("  FROM [dbo].[QuoTra_T_UserProfiles]");
                if(sendUserDetail.roleId!="2")
                {
                    sql.AppendLine("  WHERE RoleId != 2");

                }

                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }

            }

        }

        /// <summary>
        /// お気に入りユーザー一覧取得
        /// </summary>
        /// <param name="sendUserDetail"></param>
        /// <returns></returns>
        public DataTable GetFavoriteUserDataList(string favoriteUid)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT  [UUIDHash],[Name] ,[NickName],[RoleId],[Icon],[MailAddress],[SalesArea] ,[CreateDate],[AccontCode] ,[DeleteFlag]");
                sql.AppendLine("  FROM [dbo].[QuoTra_T_UserProfiles]");
                sql.AppendLine(" WHERE [UUIDHash] = @UUIDHash");


                cmd.CommandText = sql.ToString();

                cmd.Parameters.Add("@UUIDHash", SqlDbType.NVarChar, 50).Value = favoriteUid;
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }

            }

        }


        //public async Task<DataTable> GetCalendarDaySchedule(TargetSchedule targetSchedule)
        //{
        //    Logger.WriteLog("INFO-Query", $"GetCalendarDaySchedule 処理-開始 (ScheduleId: {targetSchedule.uuid})");

        //    using (var cmd = new SqlCommand(string.Empty, cn))
        //    {
        //        StringBuilder sql = new StringBuilder();
        //        sql.AppendLine("SELECT");
        //        sql.AppendLine("QTS.UUIDHash, QTS.ScheduleId, QTS.StartTime, QTS.EndTime, QTS.CompanyCode,");
        //        sql.AppendLine("MSC.CompanyName, MSC.CompanyEnglishName, MSC.Address1, MSC.SalesArea, QTS.ColorId, QMC.red, QMC.green, QMC.blue, QMC.hex_code");
        //        sql.AppendLine("FROM [dbo].[QuoTra_T_Schedule] QTS");
        //        sql.AppendLine("INNER JOIN [dbo].[QuoTra_M_Color] QMC ON QMC.ColorId = QTS.ColorId");
        //        sql.AppendLine("INNER JOIN [dbo].[M_Sales_Company] MSC ON MSC.CompanyCode = QTS.CompanyCode");
        //        sql.AppendLine("WHERE QTS.UUIDHash = @uuid");
        //        sql.AppendLine("AND (QTS.StartTime BETWEEN DATEADD(DAY, -3, @target_date) AND DATEADD(DAY, 4, @target_date)");
        //        sql.AppendLine("OR QTS.EndTime BETWEEN DATEADD(DAY, -3, @target_date) AND DATEADD(DAY, 4, @target_date))");
        //        sql.AppendLine("AND QTS.IsDelete = 0;");

        //        cmd.CommandText = sql.ToString();
        //        cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
        //        cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;

        //        DataTable dt = new DataTable();
        //        using (var reader = await cmd.ExecuteReaderAsync())
        //        {
        //            dt.Load(reader);
        //        }
        //        Logger.WriteLog("INFO-Query", $"GetCalendarDaySchedule 処理-完了 (ScheduleId: {targetSchedule.uuid})");
        //        return dt;
        //    }

        //}

        /// <summary>
        /// Calendar-スケジュール　取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public string GetDetailedCalendaSchedule(TargetSchedule targetSchedule)
        {
            Logger.WriteLog("INFO-Query", $"GetDetailedCalendarDaySchedule 処理-開始 (ScheduleId: {targetSchedule.uuid})");

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("   SELECT ");
                sql.AppendLine("     ( ");

                sql.AppendLine("SELECT");
                sql.AppendLine("QTS.UUIDHash AS uUIDHash,");
                sql.AppendLine("QTS.ScheduleId AS scheduleId,");
                sql.AppendLine("QTS.StartTime AS startTime,");
                sql.AppendLine("QTS.EndTime AS endTime,");

                sql.AppendLine("    JSON_QUERY(");
                sql.AppendLine("        (");
                sql.AppendLine("            SELECT");
                sql.AppendLine("                CAST(MSC.CompanyCode AS NVARCHAR) AS companyCode,");
                sql.AppendLine("                MSC.CompanyName AS companyName,");
                sql.AppendLine("                MSC.CompanyEnglishName AS companyEnglishName,");
                sql.AppendLine("                MSC.Address1 AS Address1,");
                sql.AppendLine("                MSC.SalesArea AS SalesArea");
                sql.AppendLine("            FROM [dbo].[M_Sales_Company] MSC");
                sql.AppendLine("            WHERE MSC.CompanyCode = QTS.CompanyCode");
                sql.AppendLine("            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER");
                sql.AppendLine("        )");
                sql.AppendLine("    ) AS companyData,");
                sql.AppendLine("");
                sql.AppendLine("    JSON_QUERY(");
                sql.AppendLine("        (");
                sql.AppendLine("            SELECT");
                sql.AppendLine("                CAST(QMC.ColorId AS NVARCHAR) AS ColorId,");
                sql.AppendLine("                QMC.red,");
                sql.AppendLine("                QMC.green,");
                sql.AppendLine("                QMC.blue,");
                sql.AppendLine("                QMC.hex_code");
                sql.AppendLine("            FROM [dbo].[QuoTra_M_Color] QMC");
                sql.AppendLine("            WHERE QMC.ColorId = QTS.ColorId");
                sql.AppendLine("            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER");
                sql.AppendLine("        )");
                sql.AppendLine("    ) AS color,");

                sql.AppendLine("(");
                sql.AppendLine("SELECT CAST(QSM.MemoId AS NVARCHAR) AS MemoId,");
                sql.AppendLine("QSM.Memo AS Content");
                sql.AppendLine("FROM [dbo].[QuoTra_T_ScheduleMemo] QSM");
                sql.AppendLine("WHERE QSM.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("FOR JSON PATH");
                sql.AppendLine(") AS notes,");

                sql.AppendLine("(");
                sql.AppendLine("SELECT CAST(QMS.SectionCode AS NVARCHAR) AS SectionCode,");
                sql.AppendLine("QMS.Name AS SectionName,");
                sql.AppendLine("QMS.ShortName AS SectionShortName,");

                sql.AppendLine("(");
                sql.AppendLine("SELECT CAST(QTSC.CRCode AS NVARCHAR) AS CRCode,");
                sql.AppendLine("MSA.Attention AS CRName,");
                sql.AppendLine("STUFF((");
                sql.AppendLine("SELECT ',' + QMS.Name");
                sql.AppendLine("FROM [dbo].[M_Sales_Link_CustomerSections] QMLC");
                sql.AppendLine("INNER JOIN [dbo].[M_Sales_Section] QMS ON QMS.SectionCode = QMLC.SectionCode");
                sql.AppendLine("WHERE QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Sections");
                sql.AppendLine("FROM [dbo].[QuoTra_T_ScheduleCustRep] QTSC");
                sql.AppendLine("INNER JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("INNER JOIN [dbo].[M_Sales_Attention] MSA ON MSA.Attention = QMLC.Attention AND MSA.CompanyCode = QMLC.CompanyCode");
                sql.AppendLine("WHERE QTSC.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("AND QMLC.SectionCode = QMS.SectionCode");
                sql.AppendLine("AND QTSC.[IsDelete] = 0");
                sql.AppendLine("GROUP BY QTSC.CRCode, MSA.Attention");
                sql.AppendLine("FOR JSON PATH");
                sql.AppendLine(") AS CustomerRepresentativeList");

                sql.AppendLine("FROM [dbo].[M_Sales_Section] QMS");
                sql.AppendLine("FOR JSON PATH");
                sql.AppendLine(") AS sectionDataList,");

                sql.AppendLine("(");
                sql.AppendLine("SELECT CAST(QMP.PurposeId AS NVARCHAR) AS purposeCode,");
                sql.AppendLine("QMP.PurposeName AS purposeName,");
                sql.AppendLine("QMP.PurposeShortName AS purposeShortName,");
                sql.AppendLine("CASE WHEN QTSP.PurposeId IS NOT NULL THEN 1 ELSE 0 END AS isSelected");
                sql.AppendLine("FROM [dbo].[QuoTra_M_Purpose] QMP");
                sql.AppendLine("LEFT JOIN [dbo].[QuoTra_T_SchedulePurpose] QTSP ON QMP.PurposeId = QTSP.PurposeId AND QTSP.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("FOR JSON PATH");
                sql.AppendLine(") AS calendarPurpose");

                sql.AppendLine("FROM [dbo].[QuoTra_T_Schedule] QTS");
                sql.AppendLine("WHERE QTS.UUIDHash = @uuid");
                sql.AppendLine("AND QTS.ScheduleId= @scheduleId");
                sql.AppendLine("AND QTS.IsDelete = 0");
                sql.AppendLine("       FOR JSON PATH ");
                sql.AppendLine("     ) AS calendarScheduleList ");

                cmd.CommandText = sql.ToString();
                //cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
                cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;
                cmd.Parameters.Add("@scheduleId", SqlDbType.NVarChar, 50).Value = targetSchedule.scheduleId;


                string json = ExecuteReaderJson(cmd, true);

                return json;
            }
        }

        /// <summary>
        /// Calendar-スケジュール　Day表示　取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public string GetDetailedCalendarDaySchedule(TargetSchedule targetSchedule)
        {
            Logger.WriteLog("INFO-Query", $"GetDetailedCalendarDaySchedule 処理-開始 (ScheduleId: {targetSchedule.uuid})");

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("");
                sql.AppendLine("SELECT (");
                sql.AppendLine("    SELECT");
                sql.AppendLine("        QTS.UUIDHash AS uUIDHash,");
                sql.AppendLine("        QTS.ScheduleId AS scheduleId,");
                sql.AppendLine("        QTS.StartTime AS startTime,");
                sql.AppendLine("        QTS.EndTime AS endTime,");
                sql.AppendLine("        CAST(QTS.ColorId AS NVARCHAR) AS colorCode,");
                sql.AppendLine("        CAST(MSC.CompanyCode AS NVARCHAR) AS companyCode,");
                sql.AppendLine("        MSC.CompanyName AS companyName,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT CONVERT(NVARCHAR(50), QTSP.PurposeId) AS selectPurposeId");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_SchedulePurpose] QTSP");
                sql.AppendLine("             WHERE QTSP.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS selectPurposeList,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT QSM.Memo AS comment");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_ScheduleMemo] QSM");
                sql.AppendLine("             WHERE QSM.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS noteList,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT QMLC.Attention AS CustomerRepresentativeId");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_ScheduleCustRep] QTSC");
                sql.AppendLine("             INNER JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("             WHERE QTSC.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS selectCustomerRepresentative");
                sql.AppendLine("");
                sql.AppendLine("    FROM [dbo].[QuoTra_T_Schedule] QTS");
                sql.AppendLine("    INNER JOIN [dbo].[M_Sales_Company] MSC ON MSC.CompanyCode = QTS.CompanyCode");
                sql.AppendLine("    WHERE QTS.UUIDHash = @uuid");
                sql.AppendLine("    AND (QTS.StartTime BETWEEN DATEADD(DAY, -3, @target_date) AND DATEADD(DAY, 3, @target_date)");
                sql.AppendLine("    OR QTS.EndTime BETWEEN DATEADD(DAY, -3, @target_date) AND DATEADD(DAY, 3, @target_date))");
                sql.AppendLine("    AND QTS.IsDelete = 0");
                sql.AppendLine("    FOR JSON PATH");
                sql.AppendLine(") AS schedules");




                cmd.CommandText = sql.ToString();
                cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
                cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;


                string json = ExecuteReaderJson(cmd, true);

                return json;
            }
        }

        /// <summary>
        /// 1月分のスケジュールデータ取得
        /// </summary>
        /// <param name="targetSchedule"></param>
        /// <returns></returns>
        public string GetDetailedCalendarSchedule(TargetSchedule targetSchedule)
        {
            Logger.WriteLog("INFO-Query", $"GetDetailedCalendarSchedule 処理-開始");

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("");
                sql.AppendLine("SELECT (");
                sql.AppendLine("    SELECT");
                sql.AppendLine("        QTS.UUIDHash AS uUIDHash,");
                sql.AppendLine("        QTS.ScheduleId AS scheduleId,");
                sql.AppendLine("        QTS.StartTime AS startTime,");
                sql.AppendLine("        QTS.EndTime AS endTime,");
                sql.AppendLine("        CAST(QTS.ColorId AS NVARCHAR) AS colorCode,");
                sql.AppendLine("        CAST(MSC.CompanyCode AS NVARCHAR) AS companyCode,");
                sql.AppendLine("        MSC.CompanyName AS companyName,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT CONVERT(NVARCHAR(50), QTSP.PurposeId) AS selectPurposeId");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_SchedulePurpose] QTSP");
                sql.AppendLine("             WHERE QTSP.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS selectPurposeList,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT QSM.Memo AS comment");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_ScheduleMemo] QSM");
                sql.AppendLine("             WHERE QSM.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS noteList,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT QMLC.Attention AS CustomerRepresentativeId ,QMS.Name AS SectionName");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_ScheduleCustRep] QTSC");
                sql.AppendLine("             INNER JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("　　　　　　 INNER JOIN [dbo].[M_Sales_Section] QMS ON QMS.SectionCode = QMLC.SectionCode");
                sql.AppendLine("             WHERE QTSC.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS selectCustomerRepresentativeList");
                sql.AppendLine("");
                sql.AppendLine("    FROM [dbo].[QuoTra_T_Schedule] QTS");
                sql.AppendLine("    INNER JOIN [dbo].[M_Sales_Company] MSC ON MSC.CompanyCode = QTS.CompanyCode");
                sql.AppendLine("    WHERE QTS.UUIDHash = @uuid");
                sql.AppendLine("    AND (YEAR(QTS.StartTime) = YEAR(@target_date) AND MONTH(QTS.StartTime) = MONTH(@target_date)");
                sql.AppendLine("    OR YEAR(QTS.EndTime) = YEAR(@target_date) AND MONTH(QTS.EndTime) = MONTH(@target_date))");
                sql.AppendLine("    AND QTS.IsDelete = 0");
                sql.AppendLine("    FOR JSON PATH");
                sql.AppendLine(") AS schedules");




                cmd.CommandText = sql.ToString();
                cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
                cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;


                string json = ExecuteReaderJson(cmd, true);

                return json;
            }
        }


        /// <summary>
        /// Calendar-スケジュール　Weak表示 取得
        /// </summary>
        /// <param name="targetSchedule"></param>
        /// <returns></returns>
        //public DataTable GetCalendarWeakSchedule(TargetSchedule targetSchedule)
        //{
        //    using (var cmd = new SqlCommand(string.Empty, cn))
        //    {
        //        StringBuilder sql = new StringBuilder();
        //        sql.AppendLine("SELECT");
        //        sql.AppendLine("QTS.UUIDHash,QTS.ScheduleId,QTS.StartTime,QTS.EndTime,QTS.CompanyCode,");
        //        sql.AppendLine("MSC.CompanyName,MSC.CompanyEnglishName,MSC.Address1,MSC.SalesArea,QTS.ColorId,QMC.red,QMC.green,QMC.blue,QMC.hex_code");
        //        sql.AppendLine("FROM  [dbo].[QuoTra_T_Schedule] QTS");
        //        sql.AppendLine("INNER JOIN [dbo].[QuoTra_M_Color] QMC ON QMC.ColorId =QTS.ColorId");
        //        sql.AppendLine("INNER JOIN [dbo].[M_Sales_Company] MSC ON MSC.CompanyCode =QTS.CompanyCode");
        //        sql.AppendLine("WHERE QTS.UUIDHash=@uuid ");
        //        sql.AppendLine("AND (QTS.StartTime BETWEEN DATEADD(WEEK, -3, @target_date) AND DATEADD(WEEK, 3, @target_date)");
        //        sql.AppendLine("OR QTS.EndTime BETWEEN DATEADD(WEEK, -3, @target_date) AND DATEADD(WEEK, 3, @target_date))");
        //        sql.AppendLine("AND QTS.IsDelete=0;");
        //        cmd.CommandText = sql.ToString();
        //        DataTable dt = new DataTable();
        //        cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
        //        cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            dt.Load(reader);
        //            return dt;
        //        }
        //    }

        //}

        /// <summary>
        /// Calendar-スケジュール　Weak表示 取得
        /// </summary>
        /// <param name="targetSchedule"></param>
        /// <returns></returns>
        public string GetCalendarWeakSchedule(TargetSchedule targetSchedule)
        {
            Logger.WriteLog("INFO-Query", $"GetDetailedCalendarDaySchedule 処理-開始 (ScheduleId: {targetSchedule.uuid})");

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("");
                sql.AppendLine("SELECT (");
                sql.AppendLine("    SELECT");
                sql.AppendLine("        QTS.UUIDHash AS uUIDHash,");
                sql.AppendLine("        QTS.ScheduleId AS scheduleId,");
                sql.AppendLine("        QTS.StartTime AS startTime,");
                sql.AppendLine("        QTS.EndTime AS endTime,");
                sql.AppendLine("        CAST(QTS.ColorId AS NVARCHAR) AS colorCode,");
                sql.AppendLine("        CAST(MSC.CompanyCode AS NVARCHAR) AS companyCode,");
                sql.AppendLine("        MSC.CompanyName AS companyName,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT CONVERT(NVARCHAR(50), QTSP.PurposeId) AS selectPurposeId");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_SchedulePurpose] QTSP");
                sql.AppendLine("             WHERE QTSP.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS selectPurposeList,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT QSM.Memo AS comment");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_ScheduleMemo] QSM");
                sql.AppendLine("             WHERE QSM.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS noteList,");
                sql.AppendLine("");
                sql.AppendLine("        ISNULL(");
                sql.AppendLine("            (SELECT QMLC.Attention AS CustomerRepresentativeId");
                sql.AppendLine("             FROM [dbo].[QuoTra_T_ScheduleCustRep] QTSC");
                sql.AppendLine("             INNER JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("             WHERE QTSC.ScheduleId = QTS.ScheduleId");
                sql.AppendLine("             FOR JSON AUTO");
                sql.AppendLine("            ), '[]') AS selectCustomerRepresentative");
                sql.AppendLine("");
                sql.AppendLine("    FROM [dbo].[QuoTra_T_Schedule] QTS");
                sql.AppendLine("    INNER JOIN [dbo].[M_Sales_Company] MSC ON MSC.CompanyCode = QTS.CompanyCode");
                sql.AppendLine("    WHERE QTS.UUIDHash = @uuid");
                sql.AppendLine("    AND (QTS.StartTime BETWEEN DATEADD(WEEK, -3, @target_date) AND DATEADD(WEEK, 3, @target_date)");
                sql.AppendLine("    OR QTS.EndTime BETWEEN DATEADD(WEEK, -3, @target_date) AND DATEADD(WEEK, 3, @target_date))");
                sql.AppendLine("    AND QTS.IsDelete = 0");
                sql.AppendLine("    FOR JSON PATH");
                sql.AppendLine(") AS schedules");




                cmd.CommandText = sql.ToString();
                cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
                cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;
                string json = ExecuteReaderJson(cmd, true);

                return json;
            }
        }

        /// <summary>
        /// Calendar-スケジュール　月表示 取得
        /// </summary>
        /// <param name="targetSchedule"></param>
        /// <returns></returns>
        public DataTable GetCalendarMonthSchedule(TargetSchedule targetSchedule)
        {
            StringBuilder sql = new StringBuilder();

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                sql.AppendLine("SELECT");
                sql.AppendLine("QTS.UUIDHash,QTS.ScheduleId,QTS.StartTime,QTS.EndTime,QTS.CompanyCode,");
                sql.AppendLine("MSC.CompanyName,MSC.CompanyEnglishName,MSC.Address1,MSC.SalesArea,QTS.ColorId,QMC.red,QMC.green,QMC.blue,QMC.hex_code");
                sql.AppendLine("FROM  [dbo].[QuoTra_T_Schedule] QTS");
                sql.AppendLine("INNER JOIN [dbo].[QuoTra_M_Color] QMC ON QMC.ColorId =QTS.ColorId");
                sql.AppendLine("INNER JOIN [dbo].[M_Sales_Company] MSC ON MSC.CompanyCode =QTS.CompanyCode");
                sql.AppendLine("WHERE QTS.UUIDHash=@uuid ");
                sql.AppendLine("AND (QTS.StartTime BETWEEN DATEADD(MONTH, -3, @target_date) AND DATEADD(MONTH, 4, @target_date)");
                sql.AppendLine("OR QTS.EndTime BETWEEN DATEADD(MONTH, -3, @target_date) AND DATEADD(MONTH, 4, @target_date))");
                sql.AppendLine("AND QTS.IsDelete=0;");


                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                cmd.Parameters.Add("@target_date", SqlDbType.DateTime2).Value = DateTime.Parse(targetSchedule.targetData, null, System.Globalization.DateTimeStyles.RoundtripKind);
                cmd.Parameters.Add("@uuid", SqlDbType.NVarChar, 50).Value = targetSchedule.uuid;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        /// <summary>
        /// スケジュール　メモ取得
        /// </summary>
        /// <param name="targetSchedule"></param>
        /// <returns></returns>
        public async Task<DataTable> GetCalendarScheduleNotes(string ScheduleId)
        {
            Logger.WriteLog("INFO-Query", $"GetCalendarScheduleNotes 処理-開始 (ScheduleId: {ScheduleId})");
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT [ScheduleId], [MemoId], [Memo], [IsDelete]");
                sql.AppendLine("FROM [dbo].[QuoTra_T_ScheduleMemo]");
                sql.AppendLine("WHERE ScheduleId = @ScheduleId");

                cmd.CommandText = sql.ToString();
                cmd.Parameters.Add("@ScheduleId", SqlDbType.NVarChar, 50).Value = ScheduleId;

                DataTable dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
                Logger.WriteLog("INFO-Query", $"GetCalendarScheduleNotes 処理-完了 (ScheduleId: {ScheduleId})");
                return dt;
            }
        }


        /// <summary>
        /// スケジュール　セクション取得
        /// </summary>
        /// <param name="ScheduleId"></param>
        /// <returns></returns>
        public async Task<DataTable> GetCalendarScheduleSection()
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT [SectionCode], [Name], [ShortName]");
                sql.AppendLine("FROM [dbo].[M_Sales_Section]");

                cmd.CommandText = sql.ToString();

                DataTable dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
                return dt;
            }
        }

        /// <summary>
        /// スケジュール　担当者取得
        /// </summary>
        /// <param name="targetSchedule"></param>
        /// <returns></returns>
        public async Task<DataTable> GetCalendarScheduleCustomer(string ScheduleId, string sectionCode)
        {
            Logger.WriteLog("INFO-Query", $"GetCalendarScheduleCustomer 処理-開始 (ScheduleId: {ScheduleId})");

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT");
                sql.AppendLine("    QTSC.[CRCode], MSA.Attention,");
                sql.AppendLine("    STUFF((");
                sql.AppendLine("        SELECT ',' + QMS.Name");
                sql.AppendLine("        FROM [dbo].[M_Sales_Link_CustomerSections] QMLC");
                sql.AppendLine("        INNER JOIN [dbo].[M_Sales_Section] QMS ON QMS.SectionCode = QMLC.SectionCode");
                sql.AppendLine("        WHERE QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("        FOR XML PATH(''), TYPE");
                sql.AppendLine("    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS SectionNames");
                sql.AppendLine("FROM [dbo].[QuoTra_T_ScheduleCustRep] QTSC");
                sql.AppendLine("INNER JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON QMLC.CRCode = QTSC.CRCode");
                sql.AppendLine("INNER JOIN [dbo].[M_Sales_Attention] MSA ON MSA.Attention = QMLC.Attention AND MSA.CompanyCode = QMLC.CompanyCode");
                sql.AppendLine("WHERE QTSC.ScheduleId = @ScheduleId");
                sql.AppendLine("  AND QMLC.SectionCode = @sectionCode");
                sql.AppendLine("  AND QTSC.[IsDelete] = 0");
                sql.AppendLine("GROUP BY QTSC.[CRCode], MSA.Attention;");

                cmd.Parameters.Add("@ScheduleId", SqlDbType.NVarChar, 50).Value = ScheduleId;
                cmd.Parameters.Add("@sectionCode", SqlDbType.Int).Value = sectionCode;
                cmd.CommandText = sql.ToString();

                DataTable dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
                Logger.WriteLog("INFO-Query", $"GetCalendarScheduleCustomer 処理-完了 (ScheduleId: {ScheduleId})");
                return dt;
            }
        }

        /// <summary>
        /// スケジュール　目的取得
        /// </summary>
        /// <param name="ScheduleId"></param>
        /// <returns></returns>
        public async Task<DataTable> GetCalendarSchedulePurPose(string ScheduleId)
        {
            Logger.WriteLog("INFO-Query", $"GetCalendarSchedulePurPose 処理-開始 (ScheduleId: {ScheduleId})");
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT QMP.[PurposeId], QMP.[PurposeName], QMP.[PurposeShortName], CASE WHEN QTSP.[PurposeId] IS NOT NULL THEN 1 ELSE 0 END AS IsSelected");
                sql.AppendLine("FROM [dbo].[QuoTra_M_Purpose] QMP");
                sql.AppendLine("LEFT JOIN [dbo].[QuoTra_T_SchedulePurpose] QTSP ON QMP.[PurposeId] = QTSP.[PurposeId]");
                sql.AppendLine("AND QTSP.[ScheduleId] = @ScheduleId");

                cmd.Parameters.Add("@ScheduleId", SqlDbType.NVarChar, 50).Value = ScheduleId;
                cmd.CommandText = sql.ToString();

                DataTable dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
                Logger.WriteLog("INFO-Query", $"GetCalendarSchedulePurPose 処理-完了 (ScheduleId: {ScheduleId})");
                return dt;
            }
        }



        ///// <summary>
        ///// Calendar-スケジュール登録
        ///// </summary>
        ///// <param name="hashedLoginUser"></param>
        ///// <returns></returns>
        //public DataTable SetCalendarData(HashedLoginUser hashedLoginUser)
        //{
        //    using (var cmd = new SqlCommand(string.Empty, cn))
        //    {
        //        StringBuilder sql = new StringBuilder();
        //        sql.AppendLine("SELECT CLU.*");
        //        sql.AppendLine("FROM [dbo].[QuoTra_M_AuthAccount] CLU");
        //        sql.AppendLine("WHERE CLU.UUIDHash=@UUIDhash and CLU.DeviceIDHash=@deviceidhash");

        //        cmd.CommandText = sql.ToString();
        //        cmd.Parameters.Add("@UUIDhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uuidHash;
        //        cmd.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.deviceIdHash;
        //        DataTable dt = new DataTable();
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            dt.Load(reader);
        //            return dt;
        //        }
        //    }
        //}

        /// <summary>
        /// Calendar-スケジュールアップデート
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable UpdateCalendarData(HashedLoginUser hashedLoginUser)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT CLU.*");
                sql.AppendLine("FROM [dbo].[QuoTra_M_AuthAccount] CLU");
                sql.AppendLine("WHERE CLU.UUIDHash=@UUIDhash and CLU.DeviceIDHash=@deviceidhash");
                cmd.Parameters.Add("@UUIDhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uuidHash;
                cmd.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.deviceIdHash;
                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Calendar-スケジュール　削除
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable DeleteCalendarData(string scheduleId)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("DELETE FROM [dbo].[QuoTra_T_Schedule] WHERE scheduleId = @scheduleId;");
                sql.AppendLine("DELETE FROM [dbo].[QuoTra_T_ScheduleCustRep] WHERE scheduleId = @scheduleId;");
                sql.AppendLine("DELETE FROM [dbo].[QuoTra_T_ScheduleMemo] WHERE scheduleId = @scheduleId;");
                sql.AppendLine("DELETE FROM [dbo].[QuoTra_T_SchedulePurpose] WHERE scheduleId = @scheduleId;");
                cmd.Parameters.Add("@scheduleId", SqlDbType.NVarChar, 50).Value = scheduleId;
                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }
        }
        /// <summary>
        /// 会社情報取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetCompanyData()
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT MCP.CompanyCode,MCP.CompanyName,MCP.CompanyEnglishName,MCP.CountryCode,MCP.Group1,CASE WHEN MCP.SalesArea IS NULL THEN 'BLANK'WHEN MCP.SalesArea = '' THEN 'BLANK' ELSE MCP.SalesArea END AS SalesArea");
                sql.AppendLine("  FROM [dbo].[M_Sales_Company] AS MCP");
                sql.AppendLine("    WHERE CompanyType = 'C'  AND SalesArea != 'DELETE'");
                sql.AppendLine("   ORDER BY CASE WHEN MCP.SalesArea IS NULL THEN 1 WHEN MCP.SalesArea = '' THEN 1 ELSE 0 END, MCP.SalesArea ASC;");

                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }

        }

        /// <summary>
        /// 会社セクション情報取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetCompanySection(string CompanyCode)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT  MCP.[CompanyCode],MCP.[CompanyName],MCP.[CompanyEnglishName],MCP.[Address1],MCP.SalesArea,QMS.SectionCode,QMS.Name,QMS.ShortName");
                sql.AppendLine("  FROM [dbo].[M_Sales_Company] MCP");
                sql.AppendLine("  LEFT JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON MCP.CompanyCode=QMLC.CompanyCode");
                sql.AppendLine("  LEFT JOIN [dbo].[M_Sales_Section] QMS ON QMLC.SectionCode=QMS.SectionCode OR QMS.SectionCode!=QMLC.SectionCode");
                sql.AppendLine("  WHERE  MCP.CompanyCode=@CompanyCode");
                sql.AppendLine("  GROUP BY MCP.[CompanyCode],MCP.[CompanyName],MCP.[CompanyEnglishName],MCP.[Address1],MCP.SalesArea,QMS.SectionCode,QMS.Name,QMS.ShortName");

                cmd.Parameters.Add("@CompanyCode", SqlDbType.NVarChar, 50).Value = CompanyCode;
                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        /// <summary>
        /// 担当者情報取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetCustomerRepresentative(string CompanyCode, string SectionCode)
        {

            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT QMLC.[CRCode],QMCR.[Attention],  STRING_AGG(CAST(QMS.Name AS VARCHAR), ',') AS SectionNames");
                sql.AppendLine("  FROM [dbo].[M_Sales_Attention] QMCR");
                sql.AppendLine("  LEFT JOIN [dbo].[M_Sales_Link_CustomerSections] QMLC ON QMCR.CompanyCode = QMLC.CompanyCode AND QMCR.Attention = QMLC.Attention   ");
                sql.AppendLine("  LEFT JOIN [dbo].[M_Sales_Section] QMS ON QMLC.SectionCode = QMS.SectionCode ");
                sql.AppendLine("  WHERE QMCR.CompanyCode=@CompanyCode AND QMS.SectionCode=@SectionCode ");
                sql.AppendLine("  GROUP BY QMLC.CRCode,QMCR.Attention ");
                cmd.Parameters.Add("@CompanyCode", SqlDbType.NVarChar, 50).Value = CompanyCode;
                cmd.Parameters.Add("@SectionCode", SqlDbType.NVarChar, 50).Value = SectionCode;
                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;
                }
            }

        }

        /// <summary>
        /// カラー取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public string GetColorData()
        {

            try
            {
                using (var cmd = new SqlCommand(string.Empty, cn))
                {

                    StringBuilder sql = new StringBuilder();


                    sql.AppendLine("   SELECT ");
                    sql.AppendLine("     ( ");
                    sql.AppendLine("       SELECT CAST([ColorId] AS NVARCHAR) AS colorId, [red] AS red, [green] AS green, [blue] AS blue, [hex_code] AS hex_code ");
                    sql.AppendLine("       FROM [dbo].[QuoTra_M_Color] ");
                    sql.AppendLine("       FOR JSON PATH ");
                    sql.AppendLine("     ) AS calendarColorsList ");
                    cmd.CommandText = sql.ToString();

                    string json = ExecuteReaderJson(cmd, true);

                    return json;
                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// Purpose取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        //public DataTable GetPurposeData()
        //{
        //    using (var cmd = new SqlCommand(string.Empty, cn))
        //    {
        //        StringBuilder sql = new StringBuilder();
        //        sql.AppendLine("SELECT [PurposeId],[PurposeName],[purposeShortName],[IsDelete]");
        //        sql.AppendLine("  FROM [dbo].[QuoTra_M_Purpose]");

        //        cmd.CommandText = sql.ToString();
        //        DataTable dt = new DataTable();
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            dt.Load(reader);
        //            return dt;
        //        }
        //    }

        //}

        /// <summary>
        /// Schedule登録
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public bool RegisterSchedule(CalendarSchedule schedule, Ulid ulid)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("  INSERT INTO [dbo].[QuoTra_T_Schedule] ([UUIDHash], [ScheduleId], [StartTime], [EndTime], [CompanyCode], [ColorId], [IsDelete])");
                    sql.AppendLine("  VALUES(@uUIDHash, @ULID, @startTime, @endTime, @companyCode, @colorId, 0)");

                    DateTime startTime = DateTime.Parse(schedule.startTime);
                    DateTime endTime = DateTime.Parse(schedule.endTime);
                    cmd.Parameters.Add("@uUIDHash", SqlDbType.NVarChar, 50).Value = schedule.uUIDHash;
                    cmd.Parameters.Add("@ULID", SqlDbType.NVarChar, 50).Value = ulid.ToString();
                    cmd.Parameters.Add("@startTime", SqlDbType.DateTime2).Value = startTime;
                    cmd.Parameters.Add("@endTime", SqlDbType.DateTime2).Value = endTime;
                    cmd.Parameters.Add("@companyCode", SqlDbType.NVarChar, 50).Value = schedule.companyData.companyCode;
                    cmd.Parameters.Add("@colorId", SqlDbType.Int).Value = schedule.color.colorId;

                    cmd.CommandText = sql.ToString();
                    DataTable dt = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return true;
                    }

                }
                catch
                (Exception ex)
                {
                    return false;
                }
            }


        }

        /// <summary>
        /// Memoの登録
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public bool RegisterScheduleMemo(ScheduleMemo scheduleMemo, Ulid ulid)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(" INSERT INTO [dbo].[QuoTra_T_ScheduleMemo] ([ScheduleId],[MemoId],[Memo],[IsDelete])");
                    sql.AppendLine(" VALUES(@ULID,@MemoId,@Content, 0)");

                    Ulid memoId = Ulid.NewUlid();
                    cmd.Parameters.Add("@ULID", SqlDbType.NVarChar, 50).Value = ulid.ToString();
                    cmd.Parameters.Add("@MemoId", SqlDbType.NVarChar, 50).Value = memoId.ToString();
                    cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 500).Value = scheduleMemo.Content;
                    cmd.CommandText = sql.ToString();

                    cmd.CommandText = sql.ToString();
                    DataTable dt = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return true;
                    }

                }
                catch
                (Exception ex)
                {
                    return false;
                }
            }


        }


        /// <summary>
        /// CustRep 登録
        /// </summary>
        /// <param name="scheduleMemo"></param>
        /// <param name="ulid"></param>
        /// <returns></returns>
        public bool RegisterScheduleCustRep(CompanySectionData scheduleSectionData, Ulid ulid)
        {

            try
            {
                foreach (var CRData in scheduleSectionData.CustomerRepresentativeList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(" INSERT INTO  [dbo].[QuoTra_T_ScheduleCustRep] ([ScheduleId],[CRCode],[SectiontCode],[IsDelete])");
                    sql.AppendLine("  VALUES(@ULID,@CRCode,@SectiontCode, 0)");
                    using (var cmd = new SqlCommand(string.Empty, cn))
                    {
                        cmd.Parameters.Add("@ULID", SqlDbType.NVarChar, 50).Value = ulid.ToString();
                        cmd.Parameters.Add("@CRCode", SqlDbType.NVarChar, 50).Value = CRData.CRCode;
                        cmd.Parameters.Add("@SectiontCode", SqlDbType.NVarChar, 50).Value = scheduleSectionData.SectionCode;

                        cmd.CommandText = sql.ToString();
                        DataTable dt = new DataTable();
                        using (SqlDataReader reader = cmd.ExecuteReader()) { }

                    }
                }
                return true;
            }
            catch
            (Exception ex)
            {
                return false;
            }



        }

        /// <summary>
        /// PurPose　登録
        /// </summary>
        /// <param name="scheduleSectionData"></param>
        /// <param name="ulid"></param>
        /// <returns></returns>
        public bool RegisterSchedulePurPose(CalendarPurpose schedulePurPoseData, Ulid ulid)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(" INSERT INTO [dbo].[QuoTra_T_SchedulePurpose] ([ScheduleId],[PurposeId],[IsDelete])");
                    sql.AppendLine(" VALUES(@ULID,@PurposeId, 0)");


                    cmd.Parameters.Add("@ULID", SqlDbType.NVarChar, 50).Value = ulid.ToString();
                    cmd.Parameters.Add("@PurposeId", SqlDbType.NVarChar, 50).Value = schedulePurPoseData.purposeCode;
                    cmd.CommandText = sql.ToString();
                    DataTable dt = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader()) { }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }

        /// <summary>
        /// Purpose取得
        /// </summary>
        /// <param name="IsCompress"></param>
        /// <returns></returns>
        public string GetPurposeData(bool IsCompress = false)
        {
            try
            {
                using (var cmd = new SqlCommand(string.Empty, cn))
                {

                    StringBuilder sql = new StringBuilder();


                    sql.AppendLine("  SELECT");
                    sql.AppendLine("    (");
                    sql.AppendLine("      SELECT CAST([PurposeId] AS NVARCHAR) AS purposeCode, [PurposeName] AS purposeName, [purposeShortName] AS purposeShortName, 0 AS isSelected");
                    sql.AppendLine("      FROM [dbo].[QuoTra_M_Purpose]");
                    sql.AppendLine("      FOR JSON PATH");
                    sql.AppendLine("    ) AS calendarPurposeList");
                    cmd.CommandText = sql.ToString();

                    string json = ExecuteReaderJson(cmd, true);

                    return json;
                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                throw;
            }

        }

        public string GetSectionData(bool IsCompress = false)
        {
            try
            {
                using (var cmd = new SqlCommand(string.Empty, cn))
                {

                    StringBuilder sql = new StringBuilder();


                    sql.AppendLine("  SELECT");
                    sql.AppendLine("    (");
                    sql.AppendLine("      SELECT  CAST([SectionCode] AS NVARCHAR) AS SectionCode,[Name] AS SectionName,[ShortName] AS SectionShortName");
                    sql.AppendLine("        FROM [dbo].[M_Sales_Section]");
                    sql.AppendLine("        WHERE SectionCode !=0");
                    sql.AppendLine("      FOR JSON PATH");
                    sql.AppendLine("    ) AS sectionList");
                    cmd.CommandText = sql.ToString();

                    string json = ExecuteReaderJson(cmd, true);

                    return json;
                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// Json形式のDataやり取り
        /// </summary>
        /// <param name="IsCompress"></param>
        /// <returns></returns>
        public string ExecuteReaderJson(SqlCommand cmd, bool IsCompress = false)
        {
            try
            {
                string json = string.Empty;

                StringBuilder sql = new StringBuilder();
                if (IsCompress)
                {
                    // 圧縮
                    sql.AppendLine(" DECLARE @json NVARCHAR(MAX); ");
                    sql.AppendLine(" SELECT @json = ( ");                                           // 圧縮対象のJSONを一旦NVARCHAR(MAX)に入れる

                    sql.Append(cmd.CommandText);

                    sql.AppendLine("  FOR JSON PATH, WITHOUT_ARRAY_WRAPPER");
                    sql.AppendLine(");");
                    sql.AppendLine("SELECT COMPRESS(@json) AS CompressedData;");
                }
                else
                {
                    // 無圧縮
                    sql.Append(cmd.CommandText);
                    sql.AppendLine("   FOR JSON PATH ");
                }
                cmd.CommandText = sql.ToString();


                using ( SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Type fieldType = reader.GetFieldType(0);

                        if (fieldType == typeof(byte[]))
                        {
                            // 圧縮

                            // バイナリデータで送られてくるので、バイナリで受け取る
                            byte[] compressedData = new byte[0];

                            // 0レコードの場合抜ける
                            if (!reader.IsDBNull(reader.GetOrdinal("CompressedData")))
                            {

                                // Get the VARBINARY data
                                compressedData = (byte[])reader["CompressedData"];
                                Debug.Print(BitConverter.ToString(compressedData).Replace("-", ""));

                                // 圧縮データを解凍
                                json = Decompress(compressedData);

                            }

                        }
                        else if (fieldType == typeof(string))
                        {
                            // 無圧縮

                            StringBuilder sb = new StringBuilder();
                            sb.Append(reader.GetValue(0).ToString());
                            while (reader.Read())
                            {
                                sb.Append(reader.GetValue(0).ToString());
                            }
                            json = sb.ToString();
                        }

                    }
                }

                return json;

            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// 圧縮データの解凍処理（GZIP）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Decompress(byte[] data)
        {
            using (var decompressedStream = new MemoryStream())
            using (var compressedStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(decompressedStream);
                decompressedStream.Position = 0;
                return new StreamReader(decompressedStream, Encoding.Unicode).ReadToEnd();
            }
        }


    }
}
