using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Net;

namespace SendMseeageByLineNotify
{
    class Program
    {
        public static string sStartupPath = Application.StartupPath;
        public static List<string> ErrorList = new List<string>();
        static void Main(string[] args)
        {
            /*ImageTools ittt = new ImageTools();
            ittt.GenerateHighThumbnail(@"D:\Yves\projects\20190814_Excel截圖程式\20190829100501.jpg", @"D:\Yves\projects\20190814_Excel截圖程式\20190829100501_s.jpg", 240, 240);*/

            string sDirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];//來源圖檔資料夾路徑
            string sAppName = ConfigurationManager.AppSettings["AppName"];//使用此程式的單位名稱(簡寫，可自定義)
            string sUploadPath = Path.Combine(ConfigurationManager.AppSettings["UploadPath"], sAppName);//上傳輸出資料夾路徑
            string sSendMsg = ConfigurationManager.AppSettings["SendMsg"];//需傳送的Message
            string sDomainUrl = ConfigurationManager.AppSettings["DomainUrl"];//域名網址Url
            string sLineNotifyToken = ConfigurationManager.AppSettings["LineNotifyToken"];//LineNotify的Token
            bool FieldNameHaveTitle = ConfigurationManager.AppSettings["FieldNameHaveTitle"].ToString() == "1" ? true : false;//圖檔名是否含有標題

            /*LineNotify lnn = new LineNotify();
            lnn.SendMsg(sLineNotifyToken, "", "http://webservice.ytec.com.tw/picturetest/Raze/havesend/20190826104154_0.jpg");*/
            //測試用

            bool bNotEmpty = sDirectoryPath != "" || sSendMsg != "";
            //sDirectoryPath、sSendMsg皆非空
            bool bHaveToken = sLineNotifyToken != "";
            //sLineNotifyToken非空

            int PictureCount = 0;//計數合法的圖片數量(應該要傳輸的)
            bool NeedCheckEmptyAndAlert = ConfigurationManager.AppSettings["CheckEmptyAndAlert"] == "1" ? true : false;//是否需要因沒有該傳的圖片而提醒
            List<string> AlertMailAddress = new List<string>(ConfigurationManager.AppSettings["AlertMailAddress"].ToString().Split(';'));

            try
            {
                if (bNotEmpty && bHaveToken)//若皆為空則不呼叫Line Notify API
                {
                    if (!Directory.Exists(sUploadPath))
                        Directory.CreateDirectory(sUploadPath);
                    string sUploadBackupPath = Path.Combine(sUploadPath, "HaveSend");//上傳備份路徑，每經過Line Notify API傳過的檔案都會備份到這
                    if (!Directory.Exists(sUploadBackupPath))
                        Directory.CreateDirectory(sUploadBackupPath);
                    string sBackupDirectoryPath = Path.Combine(sDirectoryPath, "HaveSend");//來源備份檔，每上傳過的檔案都會備份到這
                    if (!Directory.Exists(sBackupDirectoryPath))
                        Directory.CreateDirectory(sBackupDirectoryPath);

                    CheckBackupDirectory(sUploadBackupPath);
                    CheckBackupDirectory(sBackupDirectoryPath);

                    string sNow = DateTime.Now.ToString("yyyMMddHHmmss");

                    List<string> FilsList = new List<string>(Directory.GetFiles(sDirectoryPath));//資料夾底下所有檔案路徑串列
                    List<string> ImgPathList = new List<string>();//來源圖檔路徑串列
                    List<string> ImgUrlList = new List<string>();//要傳送到LINE的圖檔URL路徑
                    List<string> SmallImgUrlList = new List<string>();//要傳送到LINE的縮圖圖檔URL路徑
                    List<string> BackupFileList = new List<string>();//來源圖檔備份路徑串列
                    List<string> UploadFileList = new List<string>();//上傳圖檔路徑串列
                    List<string> UploadSmallFileList = new List<string>();//上傳縮圖圖檔路徑串列
                    List<string> BackupUploadFileList = new List<string>();//上傳圖檔備份路徑串列
                    for (int i = 0; i < FilsList.Count; i++)
                    {
                        string sFileName = Path.GetFileName(FilsList[i]);//檔名
                        string sExtName = Path.GetExtension(FilsList[i]);//副檔名
                        string sFileNameWithoutExt = Path.GetFileNameWithoutExtension(FilsList[i]);//檔名不包含副檔名
                        if (File.Exists(FilsList[i]) && sFileName.IndexOf("tmpImg") < 0 && File.Exists(FilsList[i]) && sFileName.IndexOf("result") < 0 && (sExtName.ToLower().Contains("jpg") || sExtName.ToLower().Contains("png")))
                        {
                            PictureCount++;
                            string SmallImgName = sFileNameWithoutExt + "_small" + sExtName;
                            string UploadOutPath = Path.Combine(sUploadPath, sFileName);//上傳完整路徑
                            string UploadSmallOutPath = Path.Combine(sUploadPath, SmallImgName);//上傳完整路徑
                            string fileName = Path.GetFileName(FilsList[i]);
                            //File.Copy(FilsList[i], UploadOutPath);
                            //string BackupPath = Path.Combine(sBackupDirectoryPath, sNow + "_" + i.ToString() + sExtName);//來源檔案備份路徑
                            string BackupPath = Path.Combine(sBackupDirectoryPath, sNow + "_" + i.ToString() + "_" + fileName);//來源檔案備份路徑
                            //File.Move(FilsList[i], BackupPath);
                            string sImgUrl = UrlCombine(new List<string> { sDomainUrl, sAppName, sFileName });//生成網址URL
                            string sSmallImgUrl = UrlCombine(new List<string> { sDomainUrl, sAppName, SmallImgName });//生成網址URL
                            string BackupUploadPath = Path.Combine(sUploadBackupPath, sNow + "_" + i.ToString() + sExtName);//已傳輸備份路徑
                            //先取得之後要複製移動檔案的路徑
                            ImgPathList.Add(FilsList[i]);
                            BackupFileList.Add(BackupPath);
                            BackupUploadFileList.Add(BackupUploadPath);
                            UploadFileList.Add(UploadOutPath);
                            UploadSmallFileList.Add(UploadSmallOutPath);
                            ImgUrlList.Add(sImgUrl);
                            SmallImgUrlList.Add(sSmallImgUrl);
                        }
                    }

                    bool[] isSendSuccessful = new bool[ImgUrlList.Count];
                    LineNotify ln = new LineNotify();
                    ImageTools it = new ImageTools();
                    for (int i = 0; i < ImgUrlList.Count; i++)
                    {//先傳輸
                        try
                        {
                            if (File.Exists(UploadFileList[i]))
                                File.Delete(UploadFileList[i]);//避免重名
                            if (File.Exists(UploadSmallFileList[i]))
                                File.Delete(UploadSmallFileList[i]);//避免重名
                            File.Copy(ImgPathList[i], UploadFileList[i]);
                            it.GenerateHighThumbnail(ImgPathList[i], UploadSmallFileList[i], 200, 200);
                            Thread.Sleep(3000);
                            if (FieldNameHaveTitle)
                            {
                                string tmpFileName = Path.GetFileNameWithoutExtension(ImgUrlList[i]);
                                sSendMsg = tmpFileName.Substring(0, tmpFileName.IndexOf("__"));
                            }
                            isSendSuccessful[i] = ln.SendMessage(sLineNotifyToken, sSendMsg, ImgUrlList[i], SmallImgUrlList[i]);//紀錄傳輸是否成功
                        }
                        catch (Exception e)
                        {
                            string aaa = e.ToString();
                            MakeLog ml = new MakeLog();
                            ml.ErrorLog(Program.sStartupPath + " " + ImgUrlList[i] + " " + SmallImgUrlList[i] + ":" + aaa.ToString());
                        }
                    }
                    for (int i = 0; i < UploadFileList.Count; i++)
                    {//傳輸成功後才將檔案移至備份路徑
                        if (isSendSuccessful[i])
                        {
                            string tNow = DateTime.Now.ToString("yyyMMddHHmmss");
                            if (File.Exists(BackupFileList[i]))
                            {
                                string tmpName = Path.GetFileNameWithoutExtension(BackupFileList[i]);
                                File.Move(ImgPathList[i], BackupFileList[i].Replace(tmpName, tmpName + "_over_" + tNow));
                            }
                            else
                                File.Move(ImgPathList[i], BackupFileList[i]);
                            if (File.Exists(BackupUploadFileList[i]))
                            {
                                string tmpName = Path.GetFileNameWithoutExtension(BackupUploadFileList[i]);
                                File.Move(UploadFileList[i], BackupUploadFileList[i].Replace(tmpName, tmpName + "_over_" + tNow));
                            }
                            else
                                File.Move(UploadFileList[i], BackupUploadFileList[i]);
                            File.Delete(UploadSmallFileList[i]);
                        }
                    }

                    if (NeedCheckEmptyAndAlert && PictureCount == 0)
                    {
                        for (int i = 0; i < AlertMailAddress.Count; i++)
                            if (AlertMailAddress[i] != "")
                                SendMail.SendMsgToMail(new List<string> { sAppName + ":" + sDirectoryPath + "，路徑上沒有圖片" }, AlertMailAddress[i]);
                        SendMail.SendMsgToMail(new List<string> { sAppName + ":" + sDirectoryPath + "，路徑上沒有圖片" }, "mail@ytec.com.tw");
                    }
                }
            }
            catch (Exception e)
            {
                string aaa = e.ToString();
                //string aaa = we.ToString();
                MakeLog ml = new MakeLog();
                ml.ErrorLog(Program.sStartupPath + "," + aaa.ToString());
            }

            if (ErrorList.Count > 0)
            {
                SendMail.SendMsgToMail(ErrorList, "mail@ytec.com.tw");
                for (int i = 0; i < AlertMailAddress.Count; i++)
                    if (AlertMailAddress[i] != "")
                        SendMail.SendMsgToMail(ErrorList, AlertMailAddress[i]);
            }

            Console.WriteLine("over");
            //Console.ReadKey();
        }
        /// <summary>
        /// 合成URL路徑
        /// </summary>
        /// <param name="UrlList"></param>
        /// <returns></returns>
        public static string UrlCombine(List<string> UrlList)
        {
            StringBuilder reStr = new StringBuilder();

            for (int i = 0; i < UrlList.Count; i++)
            {//檢查尾部有無"/"
                if (UrlList[i].Last() != '/')
                    reStr.Append(UrlList[i] + "/");
                else
                    reStr.Append(UrlList[i]);
            }

            string reUrl = reStr.ToString();
            int UrlLength = reUrl.Length;
            if (UrlLength > 0)
                reUrl = reUrl.Substring(0, UrlLength - 1);
            return reUrl;
        }
        /// <summary>
        /// 刪除備份路徑裡N天前的檔案
        /// </summary>
        /// <param name="BackupPath"></param>
        private static void CheckBackupDirectory(string BackupPath)
        {
            if (!Directory.Exists(BackupPath))
                return;

            int BackupLimitTime = -14;//備份資料將保存幾天，預設14天
            try
            {
                BackupLimitTime = Convert.ToInt32(ConfigurationManager.AppSettings["BackupTimeLimit"]);
                if (BackupLimitTime > 0)
                    BackupLimitTime = BackupLimitTime * -1;
            }
            catch { }
            string time = DateTime.Now.AddDays(BackupLimitTime).ToString("yyyMMddHHmmss");//取得14天前的時間
            List<string> PictureList = new List<string>(Directory.GetFiles(BackupPath));
            Tools t = new Tools();

            for (int i = 0; i < PictureList.Count; i++)
            {
                string FileName = Path.GetFileNameWithoutExtension(PictureList[i]);
                if (FileName.Length >= 14)//防呆
                {
                    List<string> tmpList = new List<string>(FileName.Split('_'));
                    for (int j = 0; j < tmpList.Count; j++)
                    {
                        if (tmpList[j].Length >= 14 && t.IsNumeric(tmpList[j]))
                        {
                            string tmpStr = tmpList[j].Substring(0, 14);
                            if (string.Compare(time, tmpStr) >= 0)
                            {
                                File.Delete(PictureList[i]);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
