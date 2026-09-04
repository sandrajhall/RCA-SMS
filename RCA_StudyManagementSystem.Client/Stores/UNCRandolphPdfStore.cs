using Microsoft.Extensions.Caching.Memory;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Security.Cryptography;

namespace RCA_StudyManagementSystem.Client.Stores
{
    public sealed class UNCRandolphPdfStore
    {
        private readonly IMemoryCache _cache;

        public UNCRandolphPdfStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Store(UNCRandolphImportView record)
        {
            var token = "UNCRandolph" + record.MRN;

            _cache.Set(
                GetKey(token),
                record,
                TimeSpan.FromMinutes(15));

            return token;
        }

        public UNCRandolphImportView? Get(string token)
        {
            return _cache.Get<UNCRandolphImportView>(GetKey(token));
        }

        private static string GetKey(string token)
        {
            return $"uncrandolph-pdf:{token}";
        }
    }
}