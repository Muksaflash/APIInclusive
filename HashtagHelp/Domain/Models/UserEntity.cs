using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HashtagHelp.Domain.Models
{
    public class UserEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SocialId { get; set; } = string.Empty;

        public string NickName { get; set; } = string.Empty;
    }
}
