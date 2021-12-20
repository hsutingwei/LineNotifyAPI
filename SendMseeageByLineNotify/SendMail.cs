using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SendMseeageByLineNotify
{
    class SendMail
    {
        public static void SendMsgToMail(List<string> msg, string ToAddress)
        {
            try
            {
                using (MailMessage mail = new MailMessage("mail@ytec.com.tw", ToAddress))
                {
                    mail.Subject = "公司Line帳號傳訊息 Error Log";
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.IsBodyHtml = true;

                    StringBuilder contentStr = new StringBuilder();
                    for (int i = 0; i < msg.Count; i++)
                        contentStr.Append(msg[i] + "<br>");

                    mail.Body = contentStr.ToString();

                    SmtpClient client = new SmtpClient("smtp.ytec.com.tw");
                    client.UseDefaultCredentials = true;
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                MakeLog ml = new MakeLog();
                ml.ErrorLog(Program.sStartupPath + ",SendMail Function :" + ex.ToString());
            }
        }
    }
}
