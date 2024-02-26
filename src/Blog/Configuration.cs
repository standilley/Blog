namespace Blog
{
    public static class Configuration
    {  
        public static string JwtKey = "0D75965ECF1B4A27831F4805C2584205";
        public static string ApiKeyName = "api_key";
        public static string ApiKey { get; set; } = "curso_api_7A96A54A7EA1406F88DE5CF74E1A3465";
        public static SmtpConfiguration Smtp = new(); 
        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string UserName { get; set; }
            public string Password { get; set; }            
        }
    }
}
