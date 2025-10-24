using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMCSCopilot.Models
{



        public enum ClaimStatus { Pending, Approved, Rejected, Settled }

        public class Claim
        {
            public int Id { get; set; }

            [Required]
            public string LecturerId { get; set; }

            [Required]
            [Range(0.5, 1000)]
            public decimal HoursWorked { get; set; }

            [Required]
            [Range(0.01, 10000)]
            public decimal HourlyRate { get; set; }

            [Display(Name = "Calculated Amount")]
            public decimal Amount { get; set; }

            [StringLength(1000)]
            public string Notes { get; set; }

            public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

            public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

            public List<ClaimFile> Files { get; set; } = new();

            public string LastUpdatedBy { get; set; }

            public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        }

}
