namespace RCA_StudyManagementSystem.Services
{
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Secure { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string SubjectPrefix { get; set; }
        public string ToName { get; set; }
        public string ToEmail { get; set; }
    }
}
