using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{

        public interface IBatchData
        {
            Task<IEnumerable<Batch>> ListBatchesAsync(CancellationToken token);

            Task<IEnumerable<Batch>> ListBatchesByStudyAsync(Guid studyId);

            Task<Batch> GetBatchAsync(Guid id);

            Task<Guid> CreateBatchAsync(string userId, Batch batch);

            Task UpdateBatchAsync(Guid id, string userId, Batch batch);

            Task DeleteBatchAsync(Guid id);

            Task<string> GetLastBatchNumberAsync(string prefix);

            Task<Guid> GetBatchIdAsync(string batchNumber);
        }
    
}
