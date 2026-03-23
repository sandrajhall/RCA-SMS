using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RCA_StudyManagementSystem.Shared.Interfaces;

namespace RCA_StudyManagementSystem.Services
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserContext _userContext;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor, UserContext userContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _userContext = userContext;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            SetAuditProperties(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            SetAuditProperties(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void SetAuditProperties(DbContext? context)
        {
            if (context == null) return;

            var userId = Guid.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : Guid.Empty;
            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is ITrackable auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedDate = now;
                        auditable.CreatedUserId = userId;
                        auditable.ModifiedDate = now;
                        auditable.ModifiedUserId = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditable.ModifiedDate = now;
                        auditable.ModifiedUserId = userId;
                    }
                }
            }
        }
    }
}
