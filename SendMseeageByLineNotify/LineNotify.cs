using System;
using System.IO;
using System.Net;
using System.Text;

namespace SendMseeageByLineNotify
{
    class LineNotify
    {
        private string LineNotifyApiUrl = "https://notify-api.line.me/api/notify";
        /// <summary>
        /// 傳訊息/圖片
        /// </summary>
        /// <param name="sLineNotifyToken">Line Notify Token</param>
        /// <param name="sMessageString">訊息</param>
        /// <param name="sImgUrl">圖片網址(http/https)</param>
        /// <returns></returns>
        public bool SendMessage(string sLineNotifyToken, string sMessageString, string sImgUrl)
        {
            if (sMessageString.Length == 0 && sImgUrl.Length == 0)//皆空則直接終止
                return false;

            GC.Collect();

            bool isSuccess = true;
            string Url = LineNotifyApiUrl;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = null;
            request.Method = "POST";
            request.KeepAlive = false; //是否保持連線
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Set("Authorization", "Bearer " + sLineNotifyToken);
            StringBuilder content = new StringBuilder();
            //if (sMessageString.Length > 0)空的就給它空的字串，必須要給API參數message，但不用有值，否則400
            content.Append("message=\r\n" + sMessageString);//發送的文字訊息內容

            if (sImgUrl.Length > 0)
            {
                content.Append("&imageThumbnail=" + sImgUrl);
                content.Append("&imageFullsize=" + sImgUrl);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(content.ToString());
            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                using (Stream data = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        MakeLog ml = new MakeLog();
                        ml.ErrorLog(Program.sStartupPath + "," + text);
                        Console.WriteLine(text);
                    }
                    data.Close();
                }

                isSuccess = false;
            }

            response.Close();
            request.Abort();

            response = null;
            request = null;
            GC.Collect();

            return isSuccess;
        }
        /// <summary>
        /// 傳訊息/圖片
        /// </summary>
        /// <param name="sLineNotifyToken">Line Notify Token</param>
        /// <param name="sMessageString">訊息</param>
        /// <param name="sImgUrl">圖片網址(http/https)</param>
        /// <param name="sSmallImgUrl">縮圖圖片網址(http/https)</param>
        /// <returns></returns>
        public bool SendMessage(string sLineNotifyToken, string sMessageString, string sImgUrl, string sSmallImgUrl)
        {
            if (sMessageString.Length == 0 && sImgUrl.Length == 0)//皆空則直接終止
                return false;

            GC.Collect();

            bool isSuccess = true;
            string Url = LineNotifyApiUrl;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = null;
            request.Method = "POST";
            request.KeepAlive = false; //是否保持連線
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Set("Authorization", "Bearer " + sLineNotifyToken);
            StringBuilder content = new StringBuilder();
            //if (sMessageString.Length > 0)空的就給它空的字串，必須要給API參數message，但不用有值，否則400
            if (sMessageString.Length > 0)
                sMessageString = "\r\n" + sMessageString;
            else
                sMessageString = "\r\n" + sMessageString;
            content.Append("message=" + sMessageString);//發送的文字訊息內容

            if (sImgUrl.Length > 0)
            {
                content.Append("&imageThumbnail=" + sSmallImgUrl);
                content.Append("&imageFullsize=" + sImgUrl);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(content.ToString());
            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                /*using (Stream data = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        MakeLog ml = new MakeLog();
                        ml.ErrorLog(Program.sStartupPath + "," + text);
                        Console.WriteLine(text);
                    }
                    data.Close();
                }*/

                isSuccess = false;
                throw e;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    request.Abort();
                }

                response = null;
                request = null;
                GC.Collect();
            }

            return isSuccess;
        }
    }
}
