using QuoTra.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using QuoTra.Models;
using System.Drawing.Imaging;
using System.Drawing;

namespace QuoTra.DAO
{
    public class ApiQuery
    {
        readonly SqlConnection cn;      // SQL接続文字列
        readonly SqlTransaction? tran;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sqlConnection"></param>
        public ApiQuery(HttpContext httpContext, SqlConnection sqlConnection, SqlTransaction? sqlTransaction = null)
        {
            cn = sqlConnection;
            tran = sqlTransaction;
        }
        // 接続テスト用
        public DataTable getProfileDetail(Profile profile)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT ");
                sql.AppendLine("TEST.Prefecture, TEST.Comment");
                sql.AppendLine("FROM [dbo].[TestTable] TEST ");
                sql.AppendLine("WHERE Name=@NAME and Age=@AGE");

                cmd.Parameters.Add("@NAME", SqlDbType.NVarChar, 50).Value = profile.name;
                cmd.Parameters.Add("@AGE", SqlDbType.Int).Value = profile.age;
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
        /// ログインユーザーデータ取得
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public DataTable GetLoginUser(HashedLoginUser hashedLoginUser)
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
        /// UserDataテーブルへのデータ保存
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public bool AddUserData(SendUserDetail hashedLoginUser)
        {
            int count = 0;
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("INSERT INTO [dbo].[QuoTra_T_UserProfiles] VALUES (@UUIDHASH, @Name, @NickName,'0', @Icon, '','',GETDATE(), '', 0);");

                cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uid == null ? "" : hashedLoginUser.uid;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = hashedLoginUser.name == null ? "" : hashedLoginUser.name;
                cmd.Parameters.Add("@NickName", SqlDbType.NVarChar, 50).Value = hashedLoginUser.nickName == null ? "" : hashedLoginUser.nickName;
                cmd.Parameters.Add("@Icon", SqlDbType.Binary, 4000).Value = string.IsNullOrEmpty(hashedLoginUser.Icon) ? DBNull.Value : CreateThumbnail(ConvertBase64ToBinary(hashedLoginUser.Icon));
                cmd.CommandText = sql.ToString();
                count = cmd.ExecuteNonQuery();
                if (count == 0) throw new Exception("対象のレコードが存在しませんでした");
                if (count != 1) throw new Exception("レコードの追加に失敗しました");
                return true;


            }


            return true;
        }

        /// <summary>
        /// ユーザーデータ更新
        /// </summary>
        /// <param name="hashedLoginUser"></param>
        /// <returns></returns>
        public bool UpdateUserData(SendUserDetail hashedLoginUser)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("    UPDATE [dbo].[QuoTra_T_UserProfiles]");
                    sql.AppendLine("    SET Name=@Name,NickName=@NickName,Icon=@Icon,MailAddress=@mailAddress");
                    sql.AppendLine("    Where UUIDHash=@UUIDHASH");

                    cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedLoginUser.uid == null ? "" : hashedLoginUser.uid;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = hashedLoginUser.name == null ? "" : hashedLoginUser.name;
                    cmd.Parameters.Add("@NickName", SqlDbType.NVarChar, 50).Value = hashedLoginUser.nickName == null ? "" : hashedLoginUser.nickName;
                    cmd.Parameters.Add("@Icon", SqlDbType.Binary).Value = string.IsNullOrEmpty(hashedLoginUser.Icon) ? (object)DBNull.Value : CreateThumbnail(ConvertBase64ToBinary(hashedLoginUser.Icon));
                    cmd.Parameters.Add("@mailAddress", SqlDbType.NVarChar, 100).Value = hashedLoginUser.mailAddress == null ? "" : hashedLoginUser.mailAddress;

                    cmd.CommandText = sql.ToString();
                    DataTable dt = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        return true;

                    }
                }
                catch
                (Exception ex)
                {
                }

            }

            return true;
        }
        public static byte[] ConvertBase64ToBinary(string base64Data)
        {
            // Base64文字列をバイト配列にデコード
            byte[] binaryData = Convert.FromBase64String(base64Data);
            return binaryData;
        }
        /// <summary>
        /// 画像サムネイル作成
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public byte[] CreateThumbnail(byte[] imageBytes)
        {
            using (MemoryStream inputStream = new MemoryStream(imageBytes))
            using (Image image = Image.FromStream(inputStream))
            using (Image thumbnail = image.GetThumbnailImage(150, 150, () => false, IntPtr.Zero))
            using (MemoryStream outputStream = new MemoryStream())
            {
                thumbnail.Save(outputStream, ImageFormat.Jpeg);
                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// ログインユーザーデータ詳細情報取得
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public DataTable GetLoginUserData(string uuid)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("    SELECT  [UUIDHash],[Name],[NickName],[RoleId],[Icon],[MailAddress],[SalesArea],[CreateDate],[AccontCode],[DeleteFlag]");
                sql.AppendLine("    FROM [dbo].[QuoTra_T_UserProfiles]");
                sql.AppendLine("    Where UUIDHash=@UUIDHASH");
                cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = uuid;
                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    Logger.WriteLog("INFO", "Query" + sql.ToString());
                    dt.Load(reader);
                    return dt;

                }
            }

        }

        public int SetIV2(HashedRecordUser hashedRecordUser)
        {
            using (var cmd = new SqlCommand(string.Empty, cn, tran))
            {
                int count = 0;
                try
                {
                    StringBuilder sql = new StringBuilder();

                    sql.AppendLine("UPDATE [dbo].[QuoTra_M_AuthAccount] SET AppKeyIV2 = @APPKEYIV2");
                    sql.AppendLine("  WHERE UUIDHash=@UUIDHASH AND DeviceIDHash=@DEVICEIDHASH;");
                    cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedRecordUser.uuidHash;
                    cmd.Parameters.Add("@DEVICEIDHASH", SqlDbType.NVarChar, 50).Value = hashedRecordUser.deviceIdHash;
                    cmd.Parameters.Add("@APPKEYIV2", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeyIV2;
                    cmd.CommandText = sql.ToString();
                    Logger.WriteLog("SQL", "SendIV2 " + cmd.CommandText + "," + hashedRecordUser.uuidHash + "," + hashedRecordUser.deviceIdHash + "," + hashedRecordUser.appKeyIV2);

                    count = cmd.ExecuteNonQuery();
                    return count;


                }
                catch (Exception)
                {
                    throw;
                }

            }

        }

        public int SetRefreashToken(HashedRecordUser user)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                int count = 0;
                try
                {
                    StringBuilder sql = new StringBuilder();

                    sql.AppendLine("UPDATE [dbo].[QuoTra_M_AuthAccount] SET RefreshToken = @REFRESH_TOKEN, RefreshTokenExpiryTime = @EXPIREY, RefreshTimes = @REFRESH_TIMES");
                    sql.AppendLine("  WHERE UUIDHash=@UUIDHASH AND DeviceIDHash=@DEVICEIDHASH;");
                    cmd.Parameters.Add("@REFRESH_TOKEN", SqlDbType.NVarChar, 100).Value = user.refreshToken;
                    cmd.Parameters.Add("@EXPIREY", SqlDbType.DateTime2).Value = user.refreshTokenExpiryTime;
                    cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = user.uuidHash;
                    cmd.Parameters.Add("@REFRESH_TIMES", SqlDbType.Int).Value = user.refreshTimes;
                    cmd.Parameters.Add("@DEVICEIDHASH", SqlDbType.NVarChar, 50).Value = user.deviceIdHash;
                    cmd.CommandText = sql.ToString();

                    count = cmd.ExecuteNonQuery();
                    if (count != 1) throw new Exception("レコードの追加に失敗しました");
                    return count;
                }
                catch (Exception)
                {

                    throw;
                }
            }

        }

        public DataTable GetRefreashToken(HashedLoginUser user)
        {
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT RefreshToken, RefreshTokenExpiryTime, TokenDurationMinutes, RefreshTimes");
                sql.AppendLine("FROM [dbo].[QuoTra_M_AuthAccount] CLU");
                sql.AppendLine("WHERE CLU.UUIDHash=@UUIDHASH and CLU.DeviceIDHash=@deviceidhash");

                cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = user.uuidHash;
                cmd.Parameters.Add("@deviceidhash", SqlDbType.NVarChar, 50).Value = user.deviceIdHash;

                cmd.CommandText = sql.ToString();
                DataTable dt = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return dt;

                }
            }

        }

        public int AddLoginUser(HashedRecordUser hashedRecordUser)
        {
            int count = 0;
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                // DELETE文を追加
                sql.AppendLine("DELETE FROM [dbo].[QuoTra_M_AuthAccount] WHERE UUIDHASH = @UUIDHASH AND DeviceIDHash = @DeviceIDHash;");
                // INSERT文を続けて追加
                sql.AppendLine("INSERT INTO [dbo].[QuoTra_M_AuthAccount] (UUIDHASH, DeviceIDHash, AppKeyHash, AppKeySalt, AppKeyIV1, Role, TokenDurationMinutes, RefreshTimes) VALUES (@UUIDHASH, @DeviceIDHash, @AppKeyHash, @AppKeySalt, @AppKeyIV1, @Role, @TokenDurationMinutes, @RefreshTimes);");

                // パラメータを追加
                cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = hashedRecordUser.uuidHash;
                cmd.Parameters.Add("@DeviceIDHash", SqlDbType.NVarChar, 50).Value = hashedRecordUser.deviceIdHash;
                cmd.Parameters.Add("@AppKeyHash", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeyHash;
                cmd.Parameters.Add("@AppKeySalt", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeySalt;
                cmd.Parameters.Add("@AppKeyIV1", SqlDbType.NVarChar, 50).Value = hashedRecordUser.appKeyIV1;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = hashedRecordUser.role;
                cmd.Parameters.Add("@TokenDurationMinutes", SqlDbType.Int).Value = hashedRecordUser.tokenDurationMinutes;
                cmd.Parameters.Add("@RefreshTimes", SqlDbType.Int).Value = hashedRecordUser.refreshTimes;

                cmd.CommandText = sql.ToString();

                count = cmd.ExecuteNonQuery();

                if (count < 1) throw new Exception("レコードの追加に失敗しました");
                return count;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetUserProfilesCount(string uuidHash)
        {
            int rowCount = 0;
            using (var cmd = new SqlCommand(string.Empty, cn))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT [UUIDHash]");
                sql.AppendLine("FROM [dbo].[QuoTra_T_UserProfiles]");
                sql.AppendLine("WHERE UUIDHash=@UUIDHASH");

                cmd.CommandText = sql.ToString();
                cmd.Parameters.Add("@UUIDHASH", SqlDbType.NVarChar, 50).Value = uuidHash;

                DataTable dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                rowCount = dt.Rows.Count;
            }

            return rowCount;
        }

    }
}
