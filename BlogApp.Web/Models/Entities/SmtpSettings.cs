namespace BlogApp.Web.Models.Entities
{
    public class SmtpSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public bool UseSsl { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
    }
}
