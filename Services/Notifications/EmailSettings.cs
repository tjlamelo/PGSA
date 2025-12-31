namespace PGSA_Licence3.Services.Notifications
{
    public class EmailSettings
    {
        public required string Host { get; set; }
        public required int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string From { get; set; }
    }
}
