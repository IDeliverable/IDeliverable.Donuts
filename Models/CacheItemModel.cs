using System;

namespace IDeliverable.Donuts.Models
{
    public class CacheItemModel
    {
        public DateTime CachedOnUtc { get; set; }
        public int Duration { get; set; }
        public int GraceTime { get; set; }
        public byte[] Output { get; set; }
        public string ContentType { get; set; }
        public string QueryString { get; set; }
        public string CacheKey { get; set; }
        public string InvariantCacheKey { get; set; }
        public string Url { get; set; }
        public string Tenant { get; set; }
        public int StatusCode { get; set; }
        public string[] Tags { get; set; } 

        public int ValidFor => Duration;

        public DateTime ValidUntilUtc => CachedOnUtc.AddSeconds(ValidFor);

        public bool IsValid(DateTime utcNow)
        {
            return utcNow < ValidUntilUtc;
        }

        public int StoredFor => Duration + GraceTime;

        public DateTime StoredUntilUtc => CachedOnUtc.AddSeconds(StoredFor);

        public bool IsInGracePeriod(DateTime utcNow)
        {
            return utcNow > ValidUntilUtc && utcNow < StoredUntilUtc;
        }
    }
}