using System.ComponentModel.DataAnnotations;


namespace CMCSCopilot.Models
{
    public class ClaimFile
    {
        public int Id { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string StoredFileName { get; set; }

        public long Size { get; set; }

        public Claim Claim { get; set; }
    }
}
