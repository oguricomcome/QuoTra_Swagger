using Microsoft.Data.SqlClient;

namespace QuoTra.DAO.Exceptions
{
    public class SQLException : Exception
    {
        private string sql;
        private string parameter;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="sql">例外が発生したSQL</param>
        /// <param name="parameter">例外が発生したパラメータ</param>
        /// <param name="ex">例外の詳細</param>
        public SQLException(string sql, SqlParameterCollection parameter, Exception ex)
            : this(sql, parameter, ex.Message, ex)
        {
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="sql">例外が発生したSQL</param>
        /// <param name="parameter">例外が発生したパラメータ</param>
        /// <param name="message">例外のメッセージ</param>
        /// <param name="ex">例外の詳細</param>
        public SQLException(string sql, SqlParameterCollection parameter, string message, Exception ex)
            : base(message, ex)
        {
            this.sql = sql;
            this.parameter = this.SetParameter(parameter);
        }

        /// <summary>
        /// 例外が発生したパラメータを設定
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private string SetParameter(SqlParameterCollection parameter)
        {
            if (parameter == null || parameter.Count < 1) return String.Empty;

            List<string> data = new List<string>();
            foreach (SqlParameter p in parameter)
            {
                data.Add(p.ParameterName + "=" + (p.Value != null ? p.Value.ToString() : ""));
            }
            return "[" + string.Join(",", data) + "]";
        }


    }
}
