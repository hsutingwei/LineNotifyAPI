using System;
using System.Text.RegularExpressions;

namespace SendMseeageByLineNotify
{
    class Tools
    {
        /// <summary>
        /// 是否為整數
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns>true:是整數;false:非整數</returns>
        public bool IsNumeric(String strNumber)
        {
            Regex NumberPattern = new Regex("[^-?\\d+$]");
            return !NumberPattern.IsMatch(strNumber);
        }
    }
}
