using QuoTra.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace QuoTra.DAO
{
    public class CalendarQuery : BaseQuery
    {
        
        /// <summary>
        /// Calendar-スケジュール取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetCalendarData(HashedLoginUser hashedLoginUser)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("SELECT CLU.*");
            sql.AppendLine("FROM [ERP_DB_UTC].[dbo].[M_QuoTra_Account] CLU");
            sql.AppendLine("WHERE CLU.UUIDHash=@UUIDhash and CLU.DeviceIDHash=@deviceidhash");

            command.Parameters.Add("@UUIDhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uuidHash;
            command.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.deviceIdHash;
            command.CommandText = sql.ToString();

            return ExecuteQuery(command);
        }

        /// <summary>
        /// Calendar-スケジュール登録
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable SetCalendarData(HashedLoginUser hashedLoginUser)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("SELECT CLU.*");
            sql.AppendLine("FROM [ERP_DB_UTC].[dbo].[M_QuoTra_Account] CLU");
            sql.AppendLine("WHERE CLU.UUIDHash=@UUIDhash and CLU.DeviceIDHash=@deviceidhash");

            command.Parameters.Add("@UUIDhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uuidHash;
            command.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.deviceIdHash;
            command.CommandText = sql.ToString();

            return ExecuteQuery(command);
        }

        /// <summary>
        /// Calendar-スケジュールアップデート
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable UpdateCalendarData(HashedLoginUser hashedLoginUser)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("SELECT CLU.*");
            sql.AppendLine("FROM [ERP_DB_UTC].[dbo].[M_QuoTra_Account] CLU");
            sql.AppendLine("WHERE CLU.UUIDHash=@UUIDhash and CLU.DeviceIDHash=@deviceidhash");

            command.Parameters.Add("@UUIDhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uuidHash;
            command.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.deviceIdHash;
            command.CommandText = sql.ToString();

            return ExecuteQuery(command);
        }

        /// <summary>
        /// Calendar-スケジュール　削除
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable DeleteCalendarData(HashedLoginUser hashedLoginUser)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("SELECT CLU.*");
            sql.AppendLine("FROM [ERP_DB_UTC].[dbo].[M_QuoTra_Account] CLU");
            sql.AppendLine("WHERE CLU.UUIDHash=@UUIDhash and CLU.DeviceIDHash=@deviceidhash");

            command.Parameters.Add("@UUIDhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uuidHash;
            command.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = hashedLoginUser.deviceIdHash;
            command.CommandText = sql.ToString();

            return ExecuteQuery(command);
        }
        /// <summary>
        /// 会社情報取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetCompanyData()
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("SELECT MCP.CompanyCode,MCP.CompanyName,MCP.CompanyEnglishName,MCP.CountryCode,MCM.CommonName,MCP.Address1");
            sql.AppendLine("  FROM [ERP_DB_UTC].[dbo].[M_Sales_Company] AS MCP");
            sql.AppendLine("  LEFT JOIN [ERP_DB_UTC].[dbo].[M_Sales_Common] AS MCM ON MCM.CommonCode=MCP.CountryCode;");

            command.CommandText = sql.ToString();

            return ExecuteQuery(command);
        }
    }
}
