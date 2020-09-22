using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DBMemThresholdMonitorTask.Entity
{
    public class MonitorLog
    {
        [Description("Id")]
        public int Id { get; set; }
        [Description("ServerName")]
        public string ServerName { get; set; }

        [Description("Usage")]
        public double Usage { get; set; }
        [Description("CreateDate")]
        public DateTime CreateDate { get; set; }

        [Description("Threshold")]
        public int Threshold { get; set; }
    }

    public class ThresholdInfo
    {
        [Description("Id")]
        public int Id { get; set; }
        [Description("ServerName")]
        public string ServerName { get; set; }
        [Description("DbType")]
        public int DbType { get; set; }

        [Description("Threshold")]
        public int Threshold { get; set; }

        [Description("ServiceName")]
        public string ServiceName { get; set; }
    }
    public class ThresholdInfoDto
    {

        public ThresholdInfo ThresholdInfo { get; set; }

        public MonitorLog MonitorLog { get; set; }
    }

    public class Contact
    {
        [Description("Id")]
        public int Id { get; set; }
        [Description("Name")]
        public string Name { get; set; }

        [Description("Email")]
        public string Email { get; set; }

        [Description("Cellphone")]
        public string Cellphone { get; set; }

        [Description("Enabled")]
        public bool Enabled { get; set; }
    }

    public class ContactThresholdMap
    {
        [Description("Id")]
        public int Id { get; set; }
        [Description("ContactId")]
        public int ContactId { get; set; }

        [Description("ThresholdInfoId")]
        public int ThresholdInfoId { get; set; }



    }
    public class Template
    {
        [Description("Id")]
        public int Id { get; set; }
        [Description("Subject")]
        public string Subject { get; set; }

        [Description("Content")]
        public string Content { get; set; }

        [Description("TemplateName")]
        public string TemplateName { get; set; }


    }
    public class SharedPoolSize
    {
        [Description("NAME")]
        public string Name { get; set; }
        [Description("SIZEBYMB")]
        public int SizeByMB { get; set; }



    }
    public class SharedPoolRemainingSize
    {
        [Description("POOL")]
        public string Pool { get; set; }

        [Description("NAME")]
        public string Name { get; set; }
        [Description("SIZEBYMB")]
        public int SizeByMB { get; set; }



    }
}
