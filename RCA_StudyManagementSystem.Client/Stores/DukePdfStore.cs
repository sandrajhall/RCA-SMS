using Microsoft.Extensions.Caching.Memory;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Security.Cryptography;

namespace RCA_StudyManagementSystem.Client.Stores
{
    public sealed class DukePdfStore
    {
        private readonly IMemoryCache _cache;

        public DukePdfStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Store(DukeImportView record)
        {
            var token = "Duke" + record.MedicalRecordNumber;

            _cache.Set(
                GetKey(token),
                record,
                TimeSpan.FromMinutes(15));

            return token;
        }

        public DukeImportView? Get(string token)
        {
            return _cache.Get<DukeImportView>(GetKey(token));
        }

        private static string GetKey(string token)
        {
            return $"duke-pdf:{token}";
        }
    }
}