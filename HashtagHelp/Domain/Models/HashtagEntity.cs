﻿namespace HashtagHelp.Domain.Models
{
    public class HashtagEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string InstagramId { get; set; } = string.Empty;

        public string MediaCount { get; set; } = "0";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
