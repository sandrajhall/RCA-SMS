using Microsoft.Extensions.Caching.Memory;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Security.Cryptography;

namespace RCA_StudyManagementSystem.Client.Stores
{
    public sealed class UNCAffiliatesPdfStore
    {
        private readonly IMemoryCache _cache;

        public UNCAffiliatesPdfStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Store(UNCAffiliatesImportView record)
        {
            var token = "UNC" + record.HospitalName + record.PAT_MRN_ID;

            _cache.Set(
                GetKey(token),
                record,
                TimeSpan.FromMinutes(15));

            return token;
        }

        public UNCAffiliatesImportView? Get(string token)
        {
            return _cache.Get<UNCAffiliatesImportView>(GetKey(token));
        }

        private static string GetKey(string token)
        {
            return $"uncrex-pdf:{token}";
        }
    }
}