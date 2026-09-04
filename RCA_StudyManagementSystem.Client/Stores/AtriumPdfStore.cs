using Microsoft.Extensions.Caching.Memory;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Security.Cryptography;

namespace RCA_StudyManagementSystem.Client.Stores
{
    public sealed class AtriumPdfStore
    {
        private readonly IMemoryCache _cache;

        public AtriumPdfStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Store(AtriumImportView record)
        {
            var token = record.HospitalName + record.MRN;

            _cache.Set(
                GetKey(token),
                record,
                TimeSpan.FromMinutes(15));

            return token;
        }

        public AtriumImportView ? Get(string token)
        {
            return _cache.Get<AtriumImportView>(GetKey(token));
        }

        private static string GetKey(string token)
        {
            return $"atrium-pdf:{token}";
        }
    }
}