using QuoTra.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using QuoTra.Models;

namespace QuoTra.DAO
{
    public class ApiQuery : BaseQuery
    {
        public DataTable getProfileDetail(Profile profile)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();

            sql.AppendLine("SELECT ");
            sql.AppendLine("TEST.Prefecture, TEST.Comment");
            sql.AppendLine("FROM [ERP_DB_UTC].[dbo].[TestTable] TEST ");
            sql.AppendLine("WHERE Name=@NAME and Age=@AGE");

            command.Parameters.Add("@NAME", SqlDbType.NVarChar, 50).Value = profile.name;
            command.Parameters.Add("@AGE", SqlDbType.Int).Value = profile.age;

            String Query = sql.ToString();
            command.CommandText = Query;
            DataTable dt = ExecuteQuery(command);
            return dt;
        }

       
        public DataTable GetLoginUser(HashedLoginUser hashedLoginUser)
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
        /// UserDataテーブルへのデータ保存
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public bool AddUserData(SendUserDetail hashedLoginUser)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("INSERT INTO [ERP_DB_UTC].[dbo].[T_QuoTra_UserData] VALUES (@UUIDHASH, @Name, @NickName,'0', @Icon,  GETDATE(), '', 0);");

            command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uid == null ? "" : hashedLoginUser.uid;
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = hashedLoginUser.name == null ? "" : hashedLoginUser.name;
            command.Parameters.Add("@NickName", SqlDbType.NVarChar, 50).Value = hashedLoginUser.nickName == null ? "" : hashedLoginUser.nickName;
            command.Parameters.Add("@Icon", SqlDbType.NVarChar).Value = hashedLoginUser.Icon == null ? "" : hashedLoginUser.Icon;
            command.CommandText = sql.ToString();
            int count = ExecuteNonQuery(command);
            if (count == 0) throw new Exception("対象のレコードが存在しませんでした");
            if (count != 1) throw new Exception("レコードの追加に失敗しました");
            return true;
        }

        public bool UpdateUserData(SendUserDetail hashedLoginUser)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                SqlCommand command = new SqlCommand();
                sql.AppendLine("    UPDATE [ERP_DB_UTC].[dbo].[T_QuoTra_UserData]");
                sql.AppendLine("    SET Name=@Name,NickName=@NickName,Icon=@Icon");
                sql.AppendLine("    Where UUIDHash=@UUIDHASH");

                command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uid == null ? "" : hashedLoginUser.uid;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = hashedLoginUser.name == null ? "" : hashedLoginUser.name;
                command.Parameters.Add("@NickName", SqlDbType.NVarChar, 50).Value = hashedLoginUser.nickName == null ? "" : hashedLoginUser.nickName;
                command.Parameters.Add("@Icon", SqlDbType.NVarChar).Value = hashedLoginUser.Icon == null ? "" : hashedLoginUser.Icon;
                command.CommandText = sql.ToString();
                int count = ExecuteNonQuery(command);
            }
            catch
            (Exception ex)
            {
            }

            return true;
        }

        public DataTable GetLoginUserData(string uuid)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("    SELECT TOP (1000) [UUIDHash],[Name],[NickName],[RoleId],[Icon],[CreateDate],[AccontCode],[DeleteFlag]");
            sql.AppendLine("    FROM [ERP_DB_UTC].[dbo].[T_QuoTra_UserData]");
            sql.AppendLine("    Where UUIDHash=@UUIDHASH");

            command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = uuid;
            command.CommandText = sql.ToString();
            return ExecuteQuery(command);
        }

        public int SetIV2(HashedRecordUser hashedRecordUser)
        {
            int count = 0;
            try
            {
                this.Begin();
                StringBuilder sql = new StringBuilder();
                SqlCommand command = new SqlCommand();

                sql.AppendLine("UPDATE [ERP_DB_UTC].[dbo].[M_QuoTra_Account] SET AppKeyIV2 = @APPKEYIV2");
                sql.AppendLine("  WHERE UUIDHash=@UUIDHASH AND DeviceIDHash=@DEVICEIDHASH;");
                command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedRecordUser.uuidHash;
                command.Parameters.Add("@DEVICEIDHASH", SqlDbType.NVarChar, 50).Value = hashedRecordUser.deviceIdHash;
                command.Parameters.Add("@APPKEYIV2", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeyIV2;
                command.CommandText = sql.ToString();
                count = ExecuteNonQuery(command);

                if (count == 0) throw new Exception("対象のレコードが存在しませんでした");
                if (count != 1) throw new Exception("レコードの追加に失敗しました");

                this.Commit();
            }
            catch (Exception)
            {
                if (this.IsTransaction())
                {
                    // ロールバックします。
                    this.Rollback();
                }
                this.Close();
                throw;
            }
            return count;
        }




        public int SetRefreashToken(HashedRecordUser user)
        {
            int count = 0;
            try
            {
                this.Begin();
                StringBuilder sql = new StringBuilder();
                SqlCommand command = new SqlCommand();

                sql.AppendLine("UPDATE [ERP_DB_UTC].[dbo].[M_QuoTra_Account] SET RefreshToken = @REFRESH_TOKEN, RefreshTokenExpiryTime = @EXPIREY, RefreshTimes = @REFRESH_TIMES");
                sql.AppendLine("  WHERE UUIDHash=@UUIDHASH AND DeviceIDHash=@DEVICEIDHASH;");
                command.Parameters.Add("@REFRESH_TOKEN", SqlDbType.NVarChar, 100).Value = user.refreshToken;
                command.Parameters.Add("@EXPIREY", SqlDbType.DateTime2).Value = user.refreshTokenExpiryTime;
                command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = user.uuidHash;
                command.Parameters.Add("@REFRESH_TIMES", SqlDbType.Int).Value = user.refreshTimes;
                command.Parameters.Add("@DEVICEIDHASH", SqlDbType.NVarChar, 50).Value = user.deviceIdHash;
                command.CommandText = sql.ToString();
                count = ExecuteNonQuery(command);

                if (count != 1) throw new Exception("レコードの追加に失敗しました");

                this.Commit();
            }
            catch (Exception)
            {
                if (this.IsTransaction())
                {
                    // ロールバックします。
                    this.Rollback();
                }
                this.Close();
                throw;
            }
            return count;
        }

        public DataTable GetRefreashToken(HashedLoginUser user)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("SELECT RefreshToken, RefreshTokenExpiryTime, TokenDurationMinutes, RefreshTimes");
            sql.AppendLine("FROM [ERP_DB_UTC].[dbo].[M_QuoTra_Account] CLU");
            sql.AppendLine("WHERE CLU.UUIDHash=@UUIDHASH and CLU.DeviceIDHash=@deviceidhash");

            command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = user.uuidHash;
            command.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = user.deviceIdHash;

            command.CommandText = sql.ToString();
            return ExecuteQuery(command);
        }

        public int AddLoginUser(HashedRecordUser hashedRecordUser)
        {
            StringBuilder sql = new StringBuilder();
            SqlCommand command = new SqlCommand();
            sql.AppendLine("INSERT INTO [ERP_DB_UTC].[dbo].[M_QuoTra_Account] VALUES (@UUIDHASH, @DeviceIDHash, @AppKeyHash, @AppKeySalt, @AppKeyIV1, null, @Role, null, null, @TokenDurationMinutes, @RefreshTimes);");

            command.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedRecordUser.uuidHash;
            command.Parameters.Add("@DeviceIDHash", SqlDbType.NVarChar, 50).Value = hashedRecordUser.deviceIdHash;
            command.Parameters.Add("@AppKeyHash", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeyHash;
            command.Parameters.Add("@AppKeySalt", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeySalt;
            command.Parameters.Add("@AppKeyIV1", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeyIV1;
            command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = hashedRecordUser.role;
            command.Parameters.Add("@TokenDurationMinutes", SqlDbType.Int).Value = hashedRecordUser.tokenDurationMinutes;
            command.Parameters.Add("@RefreshTimes", SqlDbType.Int).Value = hashedRecordUser.refreshTimes;
            command.CommandText = sql.ToString();
            return ExecuteNonQuery(command);
        }

    }
}
