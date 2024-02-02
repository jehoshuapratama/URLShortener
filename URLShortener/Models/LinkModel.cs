using Newtonsoft.Json;

namespace URLShortener.Models
{
    public class LinkModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "shortKey")]
        public string ShortKey { get; set; }

        [JsonProperty(PropertyName = "originalUrl")]
        public string OriginalUrl { get; set; }

        [JsonProperty(PropertyName = "expiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [JsonProperty(PropertyName = "usedId")]
        public int? UserId { get; set; }

        [JsonProperty(PropertyName = "hitCount")]
        public int? HitCount { get; set; }
    }
}
