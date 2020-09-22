using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DBMemThresholdMonitorTask.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NavyBule.Core;
using NavyBule.Core.Domain;
using NavyBule.Core.Events;
using NavyBule.Core.Messages;

namespace DBMemThresholdMonitorTask
{
    public interface IMonitorService
    {
        Task RunMonitor(int thresholdValue, string jobName);

    }
    public class MonitorService : IMonitorService
    {
        private readonly IDBTargetRepository _dbTargetRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly INotifyService _notifyService;
        public MonitorService(INotifyService notifyService, IEventPublisher eventPublisher, IDBTargetRepository dbTargetRepository, IConfiguration configuration, ILogger<MonitorService> logger)
        {
            _eventPublisher = eventPublisher;
            _dbTargetRepository = dbTargetRepository;
            _configuration = configuration;
            _logger = logger;
            _notifyService = notifyService;
        }

        public async Task RunMonitor(int thresholdValue, string jobName)
        {

            var thresholdInfo = await _dbTargetRepository.GetThreshold();
            if (jobName == null)
                return;
            var info = thresholdInfo.ToList().FirstOrDefault(x => x.ServerName.ToLower() == jobName.ToLower());
            if (!string.IsNullOrEmpty(info.ServerName) && Enum.IsDefined(typeof(DatabaseType), info.DbType))
            {
                var oracleServer = new OracleServer(info.ServerName, _configuration, _logger);

                var dto = new ThresholdInfoDto { ThresholdInfo = info, MonitorLog = new MonitorLog() };
                if (!oracleServer.IsConnected)
                {
                    _eventPublisher.Publish(info);
                    return;
                }
                var sharedPoolSize = await oracleServer.GetSharedPoolSize();
                dto.MonitorLog.ServerName = info.ServerName;
                dto.MonitorLog.CreateDate = DateTime.Now;
                dto.MonitorLog.Threshold = thresholdValue;
                dto.MonitorLog.Usage = sharedPoolSize.SizeByMB;
                if (sharedPoolSize.SizeByMB > thresholdValue)
                {
                    _eventPublisher.Publish(dto);
                }
            }


        }

    }
    public class EventMemoryWarning : IConsumer<ThresholdInfoDto>
    {
        private readonly INotifyService _notifyService;
        private readonly IDBTargetRepository _dbTargetRepository;
        public EventMemoryWarning(IDBTargetRepository dbTargetRepository, INotifyService notifyService)
        {
            _dbTargetRepository = dbTargetRepository;
            _notifyService = notifyService;
        }


        public void HandleEvent(ThresholdInfoDto eventMessage)
        {
            _notifyService.SendSMS(eventMessage, "DB.Warning").Wait();
            _notifyService.SendMail(eventMessage, "DB.Warning").Wait();
            _dbTargetRepository.Log(eventMessage.MonitorLog);
        }
    }
    public class EventDbConnectionFailed : IConsumer<ThresholdInfo>
    {
        private readonly INotifyService _notifyService;
        public EventDbConnectionFailed(INotifyService notifyService)
        {
            _notifyService = notifyService;
        }


        public void HandleEvent(ThresholdInfo eventMessage)
        {
            _notifyService.SendMail(eventMessage, "DB.Status").Wait();
        }
    }
    public interface INotifyService
    {
        Task SendSMS(ThresholdInfoDto eventMessage, string templateName = "");
        Task SendMail(ThresholdInfoDto eventMessage, string templateName = "");
        Task SendMail(ThresholdInfo eventMessage, string templateName = "");
    }

    public class NotifyService : INotifyService
    {
        private readonly IDBTargetRepository _dbTargetRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IOptions<RMAConfig> _mailConfig;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailSender _emailSender;
        private readonly ISmtpBuilder _smtpBuilder;
        public NotifyService(ISmtpBuilder smtpBuilder, ITokenizer tokenizer, IDBTargetRepository dbTargetRepository, IContactRepository contactRepository, IOptions<RMAConfig> mailConfig)
        {
            _dbTargetRepository = dbTargetRepository;
            _contactRepository = contactRepository;

            _mailConfig = mailConfig;
            _tokenizer = tokenizer;
            _emailSender = new EmailSender(null, smtpBuilder);
        }


        public async Task SendSMS(ThresholdInfoDto eventMessage, string templateName = "")
        {
            var messageTemplate = (await _dbTargetRepository.GetTemplate()).Where(x => x.TemplateName == templateName).ToList().FirstOrDefault();
            if (messageTemplate == null)
            {
                return;
            }
            var contacts = await GetContactListByServerId(eventMessage.ThresholdInfo.Id);
            var message = GetTemplateWithTkn(messageTemplate, eventMessage);
            var smsService = new SMSHttp();
            foreach (var contact in contacts)
            {
                if (!smsService.GetCredit())
                    return;
                var result = smsService.SendSMS(message.Subject, message.Content, contact.Cellphone, "");//DateTime.Now.ToString("yyyyMMddHHmmss")
                if (!result)
                {
                    //  await Log();
                }

            }
        }
        private IList<Token> GenerateTokens(Template messageTemplate, ThresholdInfoDto model)
        {
            IList<Token> list = new List<Token>();
            switch (messageTemplate.TemplateName)
            {
                case "DB.Warning":
                    list.Add(new Token("ServerName", model.ThresholdInfo.ServerName));
                    list.Add(new Token("Threshold", model.MonitorLog.Usage));
                    return list;
                case "DB.Status":
                    list.Add(new Token("ServerName", model.ThresholdInfo.ServerName));
                    break;

            }

            return list;


        }
        private Template GetTemplateWithTkn(Template messageTemplate, ThresholdInfoDto eventMessage)
        {
            var tokens = GenerateTokens(messageTemplate, eventMessage);
            string subject = _tokenizer.Replace(messageTemplate.Subject, tokens, htmlEncode: false);
            string body = _tokenizer.Replace(messageTemplate.Content, tokens, htmlEncode: true);
            messageTemplate.Subject = subject;
            messageTemplate.Content = body;
            return messageTemplate;
        }

        private async Task<List<Contact>> GetContactListByServerId(int serverId)
        {
            var contacts = await _contactRepository.GetContacts();
            var contactsMap = await _contactRepository.GetContactsMapByThresholdInfoId(serverId);
            var contactIds = contactsMap.Select(x => x.ContactId).ToList();
            return contacts.Where(x => contactIds.Contains(x.Id)).ToList();
        }

        public async Task SendMail(ThresholdInfoDto eventMessage, string templateName = "")
        {
            var messageTemplate = (await _dbTargetRepository.GetTemplate()).Where(x => x.TemplateName == templateName).ToList().FirstOrDefault();
            if (messageTemplate == null)
            {
                return;
            }
            var emailAccount = _mailConfig.Value.EmailAccount;
            var contacts = await GetContactListByServerId(eventMessage.ThresholdInfo.Id);

            var message = GetTemplateWithTkn(messageTemplate, eventMessage);
            foreach (var contact in contacts)
            {
                if (!string.IsNullOrEmpty(contact.Email))
                {
                    _emailSender.SendEmail(emailAccount,
                        message.Subject,
                        message.Content,
                        emailAccount.Email, emailAccount.DisplayName,
                        contact.Email,
                        contact.Name);
                }
            }

            //  await Log();
        }

        public async Task SendMail(ThresholdInfo eventMessage, string templateName = "")
        {
            var messageTemplate = (await _dbTargetRepository.GetTemplate()).Where(x => x.TemplateName == templateName).ToList().FirstOrDefault();
            if (messageTemplate == null)
            {
                return;
            }
            var emailAccount = _mailConfig.Value.EmailAccount;
            var contacts = await GetContactListByServerId(eventMessage.Id);
            var dto = new ThresholdInfoDto { ThresholdInfo = eventMessage, MonitorLog = null };
            var message = GetTemplateWithTkn(messageTemplate, dto);
            foreach (var contact in contacts)
            {
                if (!string.IsNullOrEmpty(contact.Email))
                {
                    _emailSender.SendEmail(emailAccount,
                        message.Subject,
                        message.Content,
                        emailAccount.Email, emailAccount.DisplayName,
                        contact.Email,
                        contact.Name);
                }
            }

        }
    }
    /// <summary>
    /// Class SMSHttp.下面class來自e8d demo
    /// </summary>
    public class SMSHttp
    {
        private string sendSMSUrl = "http://api.every8d.com/API21/HTTP/sendSMS.ashx";
        private string getCreditUrl = "http://api.every8d.com/API21/HTTP/getCredit.ashx";
        private string processMsg = "";

        /// <summary>
        /// 傳送簡訊
        /// </summary>
        /// <param name="userID">帳號</param>
        /// <param name="password">密碼</param>
        /// <param name="subject">簡訊主旨，主旨不會隨著簡訊內容發送出去。用以註記本次發送之用途。可傳入空字串。</param>
        /// <param name="content">簡訊發送內容</param>
        /// <param name="mobile">接收人之手機號碼。格式為: +886912345678或09123456789。多筆接收人時，請以半形逗點隔開( , )，如0912345678,0922333444。</param>
        /// <param name="sendTime">簡訊預定發送時間。-立即發送：請傳入空字串。-預約發送：請傳入預計發送時間，若傳送時間小於系統接單時間，將不予傳送。格式為YYYYMMDDhhmnss；例如:預約2009/01/31 15:30:00發送，則傳入20090131153000。若傳遞時間已逾現在之時間，將立即發送。</param>
        /// <returns>true:傳送成功；false:傳送失敗</returns>
        public bool SendSMS(string subject, string content, string mobile, string sendTime, string userID = "", string password = "")
        {
            bool success = false;
            try
            {
                if (!string.IsNullOrEmpty(sendTime))
                {
                    try
                    {
                        //檢查傳送時間格式是否正確
                        DateTime checkDt = DateTime.ParseExact(sendTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        if (!sendTime.Equals(checkDt.ToString("yyyyMMddHHmmss")))
                        {
                            this.processMsg = "傳送時間格式錯誤";
                            return false;
                        }
                    }
                    catch
                    {
                        this.processMsg = "傳送時間格式錯誤";
                        return false;
                    }
                }
                StringBuilder postDataSb = new StringBuilder();
                postDataSb.Append("UID=").Append(userID);
                postDataSb.Append("&PWD=").Append(password);
                postDataSb.Append("&SB=").Append(subject);
                postDataSb.Append("&MSG=").Append(content);
                postDataSb.Append("&DEST=").Append(mobile);
                postDataSb.Append("&ST=").Append(sendTime);

                string resultString = this.HttpPost(this.sendSMSUrl, postDataSb.ToString());
                if (!resultString.StartsWith("-"))
                {
                    /* 
					 * 傳送成功 回傳字串內容格式為：CREDIT,SENDED,COST,UNSEND,BATCH_ID，各值中間以逗號分隔。
					 * CREDIT：發送後剩餘點數。負值代表發送失敗，系統無法處理該命令
					 * SENDED：發送通數。
					 * COST：本次發送扣除點數
					 * UNSEND：無額度時發送的通數，當該值大於0而剩餘點數等於0時表示有部份的簡訊因無額度而無法被發送。
					 * BATCH_ID：批次識別代碼。為一唯一識別碼，可藉由本識別碼查詢發送狀態。格式範例：220478cc-8506-49b2-93b7-2505f651c12e
					 */
                    string[] split = resultString.Split(',');
                    this.Credit = Convert.ToDouble(split[0]);
                    this.BatchID = split[4];
                    success = true;
                }
                else
                {
                    //傳送失敗
                    this.processMsg = resultString;
                }

            }
            catch (Exception ex)
            {
                this.processMsg = ex.ToString();
            }
            return success;
        }

        /// <summary>
        /// 取得帳號餘額
        /// </summary>
        /// <returns>true:取得成功；false:取得失敗</returns>
        public bool GetCredit(string userID = "", string password = "")
        {
            bool success = false;
            try
            {
                StringBuilder postDataSb = new StringBuilder();
                postDataSb.Append("UID=").Append(userID);
                postDataSb.Append("&PWD=").Append(password);

                string resultString = this.HttpPost(this.getCreditUrl, postDataSb.ToString());
                if (!resultString.StartsWith("-"))
                {
                    this.Credit = Convert.ToDouble(resultString);
                    success = true;
                }
                else
                {
                    this.processMsg = resultString;
                }
            }
            catch (Exception ex)
            {
                this.processMsg = ex.ToString();
            }
            return success;
        }

        private string HttpPost(string url, string postData)
        {
            string result = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bs.Length;
                request.GetRequestStream().Write(bs, 0, bs.Length);
                //取得 WebResponse 的物件 然後把回傳的資料讀出
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                this.processMsg = ex.ToString();
            }
            return result;
        }

        public string ProcessMsg
        {
            get
            {
                return this.processMsg;
            }
        }

        public string BatchID { get; private set; } = "";

        public double Credit { get; private set; } = 0;
    }

}
