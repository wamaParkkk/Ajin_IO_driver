using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ajin_IO_driver
{
    public enum DigitalValue
    {
        Off = 0,
        On = 1
    };

    class Global
    {
        static string logfilePath = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, @"..\..\AjinIOLog\"));

        #region 이벤트로그 파일 폴더 및 파일 생성
        public static void EventLog(string Msg)
        {
            string sDate = DateTime.Today.ToShortDateString();
            string sTime = DateTime.Now.ToString("HH:mm:ss");
            string sDateTime;
            sDateTime = "[" + sDate + ", " + sTime + "] ";

            WriteFile(sDateTime + Msg);
        }

        private static void WriteFile(string Msg)
        {
            string sDate = DateTime.Today.ToShortDateString();
            string FileName = sDate + ".txt";

            if (File.Exists(logfilePath + FileName))
            {
                StreamWriter writer;
                writer = File.AppendText(logfilePath + FileName);
                writer.WriteLine(Msg);
                writer.Close();
            }
            else
            {
                CreateFile(Msg);
            }
        }

        private static void CreateFile(string Msg)
        {
            string sDate = DateTime.Today.ToShortDateString();
            string FileName = sDate + ".txt";

            if (!File.Exists(logfilePath + FileName))
            {
                using (File.Create(logfilePath + FileName)) ;                
            }

            StreamWriter writer;
            writer = File.AppendText(logfilePath + FileName);
            writer.WriteLine(Msg);
            writer.Close();
        }
        #endregion
    }
}
