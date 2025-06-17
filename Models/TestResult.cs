using System;

namespace GoogleOAuthDemo.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;      // Display name
        public string UserEmail { get; set; } = string.Empty;     // For filtering results
        public string Subject { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime DateTaken { get; set; }
    }
}
