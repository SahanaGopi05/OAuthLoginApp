using System;
using System.ComponentModel.DataAnnotations;

namespace GoogleOAuthDemo.Models
{
    public class UploadedMaterial
    {
        public int Id { get; set; }

        [Required]
        public string? UserEmail { get; set; }

        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? FileName { get; set; }

        [Required]
        public string? FilePath { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
