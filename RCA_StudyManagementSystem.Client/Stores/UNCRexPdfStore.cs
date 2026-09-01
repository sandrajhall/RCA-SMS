using Microsoft.Extensions.Caching.Memory;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Security.Cryptography;

namespace RCA_StudyManagementSystem.Client.Stores
{
    public sealed class UNCRexPdfStore
    {
        private readonly IMemoryCache _cache;

        public UNCRexPdfStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Store(UNCRexImportView record)
        {
            var token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            _cache.Set(
                GetKey(token),
                record,
                TimeSpan.FromMinutes(15));

            return token;
        }

        public UNCRexImportView? Get(string token)
        {
            return _cache.Get<UNCRexImportView>(GetKey(token));
        }

        private static string GetKey(string token)
        {
            return $"uncrex-pdf:{token}";
        }
    }
}