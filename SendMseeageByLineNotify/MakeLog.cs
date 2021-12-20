using System;
using System.IO;

namespace SendMseeageByLineNotify
{
    class MakeLog
    {
        private string LogPath = Path.Combine(Program.sStartupPath, "Log.idx");
        public void ErrorLog(string ErrorMsg)
        {
            string Now = DateTime.Now.ToString("yyyMMddHHmmss");
            ErrorMsg = ErrorMsg.Replace("\r", "").Replace("\n", "").Replace(",", "，");
            string outStr = Now + "," + ErrorMsg;
            RW rw = new RW();
            rw.WriteFile(LogPath, outStr, true);
            Program.ErrorList.Add(ErrorMsg);
        }
    }
}
