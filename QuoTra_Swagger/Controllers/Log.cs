using System;
using System.Diagnostics;
using System.IO;

public class Logger
{
    private static readonly object lockObject = new object();
    private static readonly string logDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public static void WriteLog(string status, string message)
    {
        //// 古いログファイルを削除
        //CleanUpOldLogs();

        //// ログファイルの名前を作成
        //string logFileName = DateTime.Now.ToString("yyyy.MM.dd") + "_logs.txt";
        //string logFilePath = Path.Combine(logDirectory, logFileName);

        //// ログの内容を作成
        //string logTime = DateTime.Now.ToString("HH:mm:ss.fff");
        //string logContent = $"{logTime} {status}: {message}";

        //// ロックを使用して同時アクセスを防ぐ
        //lock (lockObject)
        //{
        //    using (StreamWriter writer = new StreamWriter(logFilePath, true))
        //    {
        //        writer.WriteLine(logContent);
        //    }
        //}
    }

    private static void CleanUpOldLogs()
    {
        try
        {
            // 現在の日時を取得
            DateTime now = DateTime.Now;

            // ログディレクトリ内のすべてのファイルを取得
            string[] logFiles = Directory.GetFiles(logDirectory, "*_logs.txt");

            foreach (string logFile in logFiles)
            {
                // ファイル名から日付部分を抽出
                string fileName = Path.GetFileName(logFile);
                string datePart = fileName.Substring(0, 10);

                // 日付部分をDateTimeに変換
                if (DateTime.TryParseExact(datePart, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out DateTime fileDate))
                {
                    // ファイルが1週間以上前のものであれば削除
                    if ((now - fileDate).TotalDays > 7)
                    {
                        File.Delete(logFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // エラーが発生した場合の処理（必要に応じてログ出力など）
            Debug.WriteLine($"Error cleaning up old logs: {ex.Message}");
        }
    }
}
