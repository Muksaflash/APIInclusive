using System.ComponentModel.DataAnnotations;

namespace HashtagHelp.Domain.RequestModels
{
    public class FunnelRequestModel
    {
        [StringLength(50, ErrorMessage = "The NickName field should not exceed 50 characters.")]
        public string NickName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Id field is required.")]
        public string Id { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "The RequestNickNames list must contain at least one item.")]
        public List<string> RequestNickNames { get; set; } = new List<string>();

        [Required(ErrorMessage = "The HashtagArea field is required.")]
        public string HashtagArea { get; set; } = string.Empty;
    }
}