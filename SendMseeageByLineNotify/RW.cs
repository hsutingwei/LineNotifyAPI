using System.Text;
using System.IO;

namespace SendMseeageByLineNotify
{
    class RW
    {
        public void WriteFile(string sFilePath, string sOutStr, bool isAppend)
        {
            using (StreamWriter sw = new StreamWriter(sFilePath, isAppend, Encoding.UTF8))
            {
                sw.WriteLine(sOutStr);
                sw.Close();
            }
        }
    }
}
