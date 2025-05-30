using System;

namespace GoogleOAuthDemo.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime DateTaken { get; set; }
    }
}
