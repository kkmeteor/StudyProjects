using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStudy1
{
    public class FileLogger
    {
        // 该类保留写入日志文件的条款

        FileStream fs;
        StreamWriter sw;
        public FileLogger(string filename)
        {
            fs = new FileStream(filename, FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(fs);
        }
        public void Logger(string info)
        {
            sw.WriteLine(info);
        }
        public void Close()
        {
            sw.Close();
            fs.Close();

        }
    }
}
