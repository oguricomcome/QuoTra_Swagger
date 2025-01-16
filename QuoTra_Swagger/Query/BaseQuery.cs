using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace QuoTra.DAO
{
    public class BaseQuery : IDisposable
    {
        public string connectionString { get; set; }

        public BaseQuery()
        {
#if DEBUG
            //   connectionString = @"Data Source=FR-TA-HYDROGEN\sqlexpress2017;Initial Catalog=ICS_RGMN_Logs;User ID=azrRegimenUser;Password=ke9T5mD2ya5#;";
            connectionString = "Server=VMDEVELOP\\SQL2016;Database=ERP_DB_UTC;Integrated Security=True;TrustServerCertificate=Yes";

#else
            // Windows版
            // connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=RegimenCloudDB;User ID=azrRegimenUser;Password=ke9T5mD2ya5#;";
            // Linux版
            //connectionString = "Server=VMDEVELOP\\SQL2016;Database=ERP_DB_UTC;Integrated Security=True;TrustServerCertificate=Yes";
            // 本番　検証環境
            //connectionString = @"Server=tcp:ueno-thailand.database.windows.net,1433;Initial Catalog=ERP_DB_UTC_Try;Persist Security Info=True;User ID=ERP_QuoTra;Password=Zq4MHXRhzcDtPQWyLSuA6m9Jvi3SLTcFZxPeecK3NCTrdEHc;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=15;Max Pool Size=600;MultipleActiveResultSets=True";
            // 本番環境
            connectionString = @"Server=tcp:ueno-thailand.database.windows.net,1433;Initial Catalog=ERP_DB_UTC;Persist Security Info=True;User ID=ERP_QuoTra;Password=Zq4MHXRhzcDtPQWyLSuA6m9Jvi3SLTcFZxPeecK3NCTrdEHc;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=15;Max Pool Size=600;MultipleActiveResultSets=True";
#endif
            this.Open();

        }
        public void Dispose()
        {
            this.Close();
        }

        private const int retryCount = 3;


        /// <summary>
        /// DB接続
        /// </summary>
        public SqlConnection conn { get; set; }

        /// <summary>
        /// トランザクション
        /// </summary>
        protected SqlTransaction transaction = null;

        /// <summary>
        /// データベースに接続
        /// </summary>
        protected void Open()
        {
            if (this.conn != null) throw new Exception("データベースに接続できません。既に接続済みです。");
            try
            {
                this.conn = new SqlConnection(connectionString);
                this.conn.Open();
                return;
            }
            catch (Exception e)
            {
                conn = null;
                throw new Exception("データベースに接続できません。", e);
            }
        }

        /// <summary>
        /// データベースを閉じる
        /// </summary>
        public void Close()
        {
            if (this.conn != null && this.transaction != null)
            {
                try { this.transaction.Rollback(); }
                catch (Exception) { }
                this.transaction = null;
            }
            if (this.conn != null)
            {
                try { this.conn.Close(); }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                try { this.conn.Dispose(); }
                catch (Exception) { }
                this.conn = null;
            }
        }

        /// トランザクション中はTRUEを返す
        /// </summary>
        /// <returns></returns>
        public bool IsTransaction()
        {
            return (transaction != null);
        }

        /// <summary>
        /// トランザクション処理の開始
        /// </summary>
        /// <returns></returns>
        public bool Begin()
        {
            if (this.transaction != null) throw new Exception("トランザクションの入れ子をサポートしていません。");
            try
            {
                this.transaction = conn.BeginTransaction();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("トランザクションを開始できません。", e);
            }
        }

        /// <summary>
        /// コミット
        /// </summary>
        public void Commit()
        {
            if (this.transaction == null) throw new Exception("トランザクションは開始されていません。");
            try
            {
                this.transaction.Commit();
                this.transaction = null;
            }
            catch (Exception e)
            {
                throw new Exception("コミットに失敗しました。", e);
            }
        }

        /// <summary>
        /// ロールバック
        /// </summary>
        public void Rollback()
        {
            try
            {
                if (this.transaction != null)
                {
                    this.transaction.Rollback();
                    this.transaction = null;
                }
            }
            catch (Exception e)
            {
                throw new Exception("ロールバックに失敗しました。", e);
            }
        }

        /// <summary>
        /// 検索系SQLを実行します。
        /// </summary>
        /// <param name="command">SQLコマンド</param>
        /// <returns>検索結果</returns>
        protected DataTable ExecuteQuery(SqlCommand command)
        {
            DataTable dt = new DataTable();
            int rc = retryCount;
            while (rc > 0)
            {
                try
                {
                    command.CommandTimeout = 300;
                    command.Connection = conn;
                    if (IsTransaction()) command.Transaction = this.transaction;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    break;
                }
                catch (SqlException e)
                {
                    if (e.Number == -2)
                    {
                        rc--;
                        Thread.Sleep(3000);
                    }
                    else throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return dt;
        }


        /// <summary>
        /// 更新系SQLを実行します。
        /// </summary>
        /// <param name="command">SQLコマンド</param>
        /// <returns>更新レコード数</returns>
        protected int ExecuteNonQuery(SqlCommand command)
        {
            int rc = retryCount;
            int ret = 0;

            while (rc > 0)
            {
                try
                {
                    command.CommandTimeout = 300;
                    command.Connection = conn;
                    if (IsTransaction()) command.Transaction = this.transaction;
                    ret = command.ExecuteNonQuery();
                    break;
                }
                catch (SqlException e)
                {
                    if (e.Number == -2)
                    {
                        rc--;
                        Thread.Sleep(3000);
                    }
                    else throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return ret;
        }
    }
}


