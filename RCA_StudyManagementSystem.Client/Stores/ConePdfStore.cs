using Microsoft.Extensions.Caching.Memory;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Security.Cryptography;

namespace RCA_StudyManagementSystem.Client.Stores
{
    public sealed class ConePdfStore
    {
        private readonly IMemoryCache _cache;

        public ConePdfStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Store(ConeImportView record)
        {
            var token = record.HospitalName + record.MRN;

            _cache.Set(
                GetKey(token),
                record,
                TimeSpan.FromMinutes(15));

            return token;
        }

        public ConeImportView? Get(string token)
        {
            return _cache.Get<ConeImportView>(GetKey(token));
        }

        private static string GetKey(string token)
        {
            return $"cone-pdf:{token}";
        }
    }
}