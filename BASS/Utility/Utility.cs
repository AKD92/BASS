
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace BASS.Utilities
{
    public class Utility
    {
        public static string DeepestExceptionMessage(Exception Excp)
        {
            string Message;
            while (Excp.InnerException != null)
            {
                Excp = Excp.InnerException;
            }
            Message = Excp.Message;
            return Message;
        }

        public static void SetErrorLog(long? userId, string component, string desrciption)
        {
            object UserIdObj;
            UserIdObj = userId == null ? DBNull.Value : (object)userId;
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString);

            try
            {
                dbConnection.Open();
                SqlCommand cmd = new SqlCommand("SP_CreateErrorlog", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", UserIdObj);
                cmd.Parameters.AddWithValue("@Component", component);
                cmd.Parameters.AddWithValue("@Description", desrciption);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
        }


        public static bool SendEmail(string recepientEmail, string cc, string Bcc, string subject, string body, List<string> files, bool isHtmlBody)
        {
            bool emailSent = false;
            using (MailMessage mailMessage = new MailMessage())
            {
                string[] toEmails = null;
                string[] toCCs = null;
                string[] toBCCs = null;
                try
                {
                    mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SMTP_Server"]);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = isHtmlBody;
                    toEmails = recepientEmail.Split(',');
                    if (cc != null)
                        toCCs = cc.Split(',');
                    if (Bcc != null)
                    {
                        toBCCs = Bcc.Split(',');
                    }

                    if (toEmails != null && toEmails.ToList().Count > 0)
                    {
                        foreach (string t in toEmails)
                        {
                            if (t != string.Empty)
                            {
                                mailMessage.To.Add(new MailAddress(t));
                            }
                        }
                    }
                    else
                    {
                        if (recepientEmail != null && recepientEmail != string.Empty)
                            mailMessage.To.Add(new MailAddress(recepientEmail));
                    }

                    if (toCCs != null && toCCs.ToList().Count > 0)
                    {
                        foreach (string c in toCCs)
                        {
                            if (c != string.Empty)
                            {
                                mailMessage.CC.Add(new MailAddress(c));
                            }
                        }
                    }
                    else
                    {
                        if (cc != null && cc != string.Empty)
                            mailMessage.CC.Add(new MailAddress(cc));
                    }
                    if (toBCCs != null && toBCCs.ToList().Count > 0)
                    {
                        foreach (string c in toBCCs)
                        {
                            if (c != string.Empty)
                            {
                                mailMessage.Bcc.Add(new MailAddress(c));
                            }
                        }
                    }
                    else
                    {
                        if (Bcc != null && Bcc != string.Empty)
                            mailMessage.Bcc.Add(new MailAddress(Bcc));
                    }

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = ConfigurationManager.AppSettings["SMTP_Mail"];
                    smtp.Port = int.Parse(ConfigurationManager.AppSettings["SMTP_PortNo"]);
                    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);

                    NetworkCredential NetworkCred = new NetworkCredential();
                    NetworkCred.UserName = ConfigurationManager.AppSettings["SMTP_Server"];
                    NetworkCred.Password = ConfigurationManager.AppSettings["SMTP_Password"];
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;

                    if (files != null && files.Count > 0)
                    {
                        foreach (string file in files)
                        {
                            Attachment attachment;
                            attachment = new Attachment(file);
                            mailMessage.Attachments.Add(attachment);
                        }
                    }
                    smtp.Send(mailMessage);
                    emailSent = true;
                }
                catch (Exception ex)
                {
                    emailSent = false;
                    throw ex;
                }
            }
            return emailSent;
        }
    }
}
