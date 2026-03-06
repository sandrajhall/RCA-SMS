using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManager.Shared.Domain;

namespace RCA_StudyManagementSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // Define DbSets for your entities here
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Study> Studies { get; set; }
        public DbSet<PathReport> PathReports { get; set; }
        public DbSet<PatientPhoneNumber> PatientPhoneNumbers { get; set; }
        public DbSet<Lookup> Lookups { get; set; }
        public DbSet<StudyLookup> StudyLookups { get; set; }
        public DbSet<Histology> Histologies { get; set; }
        public DbSet<StudyHistology> StudyHistologies { get; set; }
        public DbSet<StudyHeader> StudyHeaders { get; set; }
        public DbSet<StudyReportHeader> StudyReportHeaders { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<PathReportExport> PathReportExports { get; set; }
        public DbSet<DoNotContact> DoNotContacts { get; set; }
        public DbSet<StudyContact> StudyContacts { get; set; }
        public DbSet<ReimbursementEntity> ReimbursementEntities { get; set; }
        public DbSet<RCAContact> RCAContacts { get; set; }
        public DbSet<ReimbursementEntityRCAContact> ReimbursementEntityRCAContacts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<DailyPathSubmission> DailyPathSubmissions { get; set; }
        public DbSet<PatientStatus> PatientStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entity mappings here

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.CaseNumber)
                .IsUnique();

            modelBuilder.Entity<PathReport>()
                .HasIndex(p => p.SubmittingHospitalPathReportNumber)
                .IsUnique();

            modelBuilder.Entity<StudyLookup>()
                .HasKey(sl => new { sl.StudyId, sl.LookupId }); // Define a composite primary key for the join table

            modelBuilder.Entity<StudyLookup>()
                .HasOne(sl => sl.Study)
                .WithMany(s => s.StudyLookups)
                .HasForeignKey(sl => sl.StudyId); // Configure the one-to-many relationship from join table to Study

            modelBuilder.Entity<StudyLookup>()
                .HasOne(sl => sl.Lookup)
                .WithMany(s => s.StudyLookups)
                .HasForeignKey(sl => sl.LookupId); // Configure the one-to-many relationship from join table to Lookup

            modelBuilder.Entity<Batch>()
                .HasMany(b => b.PathReportExports);

            modelBuilder.Entity<PathReportExport>()
                .HasOne(pre => pre.Batch)
                .WithMany(b => b.PathReportExports)
                .HasForeignKey(pre => pre.BatchId);

            modelBuilder.Entity<Invoice>()
                .Property(p => p.TotalAmount)
                .HasColumnType("DECIMAL(19, 2)"); // Set the exact SQL type

            modelBuilder.Entity<ReimbursementEntityRCAContact>()
                .HasOne(re => re.ReimbursementEntity)
                .WithMany(r => r.ReimbursementEntityRCAContacts)
                .HasForeignKey(re => re.ReimbursementEntityId);

            modelBuilder.Entity<ReimbursementEntityRCAContact>()
                .HasOne(rc => rc.RCAContact)
                .WithMany(a => a.ReimbursementEntityRCAContacts)
                .HasForeignKey(rc => rc.RCAContactId);

            modelBuilder.Entity<PatientStatus>()
               .HasOne(ps => ps.Study)
               .WithMany(s => s.PatientStatuses)
               .HasForeignKey(ps => ps.StudyId)
               .OnDelete(DeleteBehavior.NoAction); // Fixes error when trying to run migrations related to PatientStatus and Study entities

        }
    }


}
