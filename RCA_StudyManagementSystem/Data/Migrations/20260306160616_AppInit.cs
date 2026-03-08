using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCA_StudyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AppInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Batch",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfCases = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batch", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "Doctor",
                columns: table => new
                {
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigratedDoctorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimarySpecialty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondarySpecialty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoctorComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPathologist = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateOfDoctorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor", x => x.DoctorId);
                });

            migrationBuilder.CreateTable(
                name: "DoNotContact",
                columns: table => new
                {
                    DoNotContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SocialSecurityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLastContact = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoNotContact", x => x.DoNotContactId);
                });

            migrationBuilder.CreateTable(
                name: "Histology",
                columns: table => new
                {
                    HistologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistologyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistologyBehavior = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistologyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histology", x => x.HistologyId);
                });

            migrationBuilder.CreateTable(
                name: "Hospital",
                columns: table => new
                {
                    HospitalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigratedHospitalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospitalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HospitalShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospitalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospitalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReimbursementEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateOfHospitalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospital", x => x.HospitalId);
                });

            migrationBuilder.CreateTable(
                name: "Lookup",
                columns: table => new
                {
                    LookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LookupType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LookupCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup", x => x.LookupId);
                });

            migrationBuilder.CreateTable(
                name: "RCAContact",
                columns: table => new
                {
                    RCAContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credentials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RCAContact", x => x.RCAContactId);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementEntity",
                columns: table => new
                {
                    ReimbursementEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayableTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttentionTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoicePrefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementEntity", x => x.ReimbursementEntityId);
                });

            migrationBuilder.CreateTable(
                name: "Study",
                columns: table => new
                {
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrincipalInvestigator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CCR_F14_ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudySite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorLight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorDark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinAge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxAge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathMinAge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathMaxAge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathMaxNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Design = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    LastEnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Study", x => x.StudyId);
                });

            migrationBuilder.CreateTable(
                name: "StudyReportHeader",
                columns: table => new
                {
                    StudyReportHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeaderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExportTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyReportHeader", x => x.StudyReportHeaderId);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReimbursementEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceQuarter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "DECIMAL(19,2)", nullable: false),
                    DateEmailed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateSignedInvoiceReturned = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RASRSubmitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RASRId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RASRCompleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoice_ReimbursementEntity_ReimbursementEntityId",
                        column: x => x.ReimbursementEntityId,
                        principalTable: "ReimbursementEntity",
                        principalColumn: "ReimbursementEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RCAContactReimbursementEntity",
                columns: table => new
                {
                    RCAContactsRCAContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReimbursementEntitiesReimbursementEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RCAContactReimbursementEntity", x => new { x.RCAContactsRCAContactId, x.ReimbursementEntitiesReimbursementEntityId });
                    table.ForeignKey(
                        name: "FK_RCAContactReimbursementEntity_RCAContact_RCAContactsRCAContactId",
                        column: x => x.RCAContactsRCAContactId,
                        principalTable: "RCAContact",
                        principalColumn: "RCAContactId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RCAContactReimbursementEntity_ReimbursementEntity_ReimbursementEntitiesReimbursementEntityId",
                        column: x => x.ReimbursementEntitiesReimbursementEntityId,
                        principalTable: "ReimbursementEntity",
                        principalColumn: "ReimbursementEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementEntityRCAContact",
                columns: table => new
                {
                    ReimbursementEntityRCAContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReimbursementEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RCAContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimaryContact = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementEntityRCAContact", x => x.ReimbursementEntityRCAContactId);
                    table.ForeignKey(
                        name: "FK_ReimbursementEntityRCAContact_RCAContact_RCAContactId",
                        column: x => x.RCAContactId,
                        principalTable: "RCAContact",
                        principalColumn: "RCAContactId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReimbursementEntityRCAContact_ReimbursementEntity_ReimbursementEntityId",
                        column: x => x.ReimbursementEntityId,
                        principalTable: "ReimbursementEntity",
                        principalColumn: "ReimbursementEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyPathSubmission",
                columns: table => new
                {
                    DailyPathSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HospitalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPathSubmission", x => x.DailyPathSubmissionId);
                    table.ForeignKey(
                        name: "FK_DailyPathSubmission_Hospital_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospital",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyPathSubmission_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigratedCCRNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SocialSecurityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Race = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RaceCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ethnicity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EthnicityCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenderCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patient_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyContact",
                columns: table => new
                {
                    StudyContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOnTeamsChannel = table.Column<bool>(type: "bit", nullable: false),
                    IsExportEmailReceived = table.Column<bool>(type: "bit", nullable: false),
                    IsConfidentialAgreementSigned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyContact", x => x.StudyContactId);
                    table.ForeignKey(
                        name: "FK_StudyContact_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyHeader",
                columns: table => new
                {
                    StudyHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeaderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExportTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyHeader", x => x.StudyHeaderId);
                    table.ForeignKey(
                        name: "FK_StudyHeader_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyHistology",
                columns: table => new
                {
                    StudyHistologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyHistology", x => x.StudyHistologyId);
                    table.ForeignKey(
                        name: "FK_StudyHistology_Histology_HistologyId",
                        column: x => x.HistologyId,
                        principalTable: "Histology",
                        principalColumn: "HistologyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyHistology_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyLookup",
                columns: table => new
                {
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyLookup", x => new { x.StudyId, x.LookupId });
                    table.ForeignKey(
                        name: "FK_StudyLookup_Lookup_LookupId",
                        column: x => x.LookupId,
                        principalTable: "Lookup",
                        principalColumn: "LookupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyLookup_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItem",
                columns: table => new
                {
                    InvoiceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HospitalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumPathReports = table.Column<int>(type: "int", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItem", x => x.InvoiceItemId);
                    table.ForeignKey(
                        name: "FK_InvoiceItem_Hospital_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospital",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItem_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItem_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathReport",
                columns: table => new
                {
                    PathReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigratedCCRNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudyPrefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudyColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathIndex = table.Column<int>(type: "int", nullable: true),
                    SubmittingHospital = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HospitalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HospAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HospAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HospFaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittingHospitalPathReportNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginatingHospitalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigHospitalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrigHospAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrigHospAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigHospCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigHospState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigHospZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigHospPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigHospFaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginatingHospitalPathReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginatingHospitalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathReportObtained = table.Column<bool>(type: "bit", nullable: false),
                    StudySubmission = table.Column<bool>(type: "bit", nullable: false),
                    SlidesResideAtSubmittingHospital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfProcedure = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgeAtProcedure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DxAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxCountyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxCounty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DxPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExportStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RcaExportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAddendum = table.Column<bool>(type: "bit", nullable: false),
                    IsOutsidePathReport = table.Column<bool>(type: "bit", nullable: false),
                    IsOnHold = table.Column<bool>(type: "bit", nullable: false),
                    Reimbursement1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reimbursement2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoNotInvoice = table.Column<bool>(type: "bit", nullable: false),
                    Site = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathProcedure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologyDiagnosis1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistologyCode1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistologyBehavior1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistologyDiagnosisComments1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TumorSize = table.Column<double>(type: "float", nullable: true),
                    MarginStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ALNL_PositiveNodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ALNL_NodesExamined = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologicDiff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologicGrade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologicDiff2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologicGrade2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TumorSize2 = table.Column<double>(type: "float", nullable: true),
                    MarginStatus2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ALNL_PositiveNodes2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ALNL_NodesExamined2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PSA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PSA_ng_ml = table.Column<double>(type: "float", nullable: true),
                    PerineuralInvasion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumCoresBiopsy = table.Column<int>(type: "int", nullable: true),
                    NumCoresCancer = table.Column<int>(type: "int", nullable: true),
                    PrimGleason1 = table.Column<int>(type: "int", nullable: true),
                    SecGleason1 = table.Column<int>(type: "int", nullable: true),
                    GleasonSum1 = table.Column<int>(type: "int", nullable: true),
                    PrimGleason2 = table.Column<int>(type: "int", nullable: true),
                    SecGleason2 = table.Column<int>(type: "int", nullable: true),
                    GleasonSum2 = table.Column<int>(type: "int", nullable: true),
                    AuthorizingProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorizingProviderComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDAddress3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDCounty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDPhoneNumber1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MDPhoneNumber2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDFaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MDEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pathologist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathologistId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PathologistComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathAddress3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathCounty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathPhoneNumber1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathPhoneNumber2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathFaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Site2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteCode2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathProcedure2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathComments2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologyDiagnosis2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologyCode2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologyBehavior2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistologyDiagnosisComments2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorizingProvider2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Doctor2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorizingProviderComments2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2Address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2County = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2PhoneNumber1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD2PhoneNumber2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2FaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD2Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pathologist2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pathologist2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PathologistComments2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2Address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2County = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2PhoneNumber1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path2PhoneNumber2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2FaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path2Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathReport", x => x.PathReportId);
                    table.ForeignKey(
                        name: "FK_PathReport_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPhoneNumber",
                columns: table => new
                {
                    PatientPhoneNumberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumberComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPhoneNumber", x => x.PatientPhoneNumberId);
                    table.ForeignKey(
                        name: "FK_PatientPhoneNumber_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientStatus",
                columns: table => new
                {
                    PatientStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoContact = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfDeath = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateLastContact = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InformedOfCancerDiagnosis = table.Column<bool>(type: "bit", nullable: false),
                    StatedNoCancerDiagnosis = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientStatus", x => x.PatientStatusId);
                    table.ForeignKey(
                        name: "FK_PatientStatus_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientStatus_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "StudyId");
                });

            migrationBuilder.CreateTable(
                name: "PathReportExport",
                columns: table => new
                {
                    PathReportExportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathReportExport", x => x.PathReportExportId);
                    table.ForeignKey(
                        name: "FK_PathReportExport_Batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batch",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PathReportExport_PathReport_PathReportId",
                        column: x => x.PathReportId,
                        principalTable: "PathReport",
                        principalColumn: "PathReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPathSubmission_HospitalId",
                table: "DailyPathSubmission",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPathSubmission_StudyId",
                table: "DailyPathSubmission",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_ReimbursementEntityId",
                table: "Invoice",
                column: "ReimbursementEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItem_HospitalId",
                table: "InvoiceItem",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItem_InvoiceId",
                table: "InvoiceItem",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItem_StudyId",
                table: "InvoiceItem",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_PathReport_PatientId",
                table: "PathReport",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PathReport_SubmittingHospitalPathReportNumber",
                table: "PathReport",
                column: "SubmittingHospitalPathReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PathReportExport_BatchId",
                table: "PathReportExport",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PathReportExport_PathReportId",
                table: "PathReportExport",
                column: "PathReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_CaseNumber",
                table: "Patient",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patient_StudyId",
                table: "Patient",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPhoneNumber_PatientId",
                table: "PatientPhoneNumber",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientStatus_PatientId",
                table: "PatientStatus",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientStatus_StudyId",
                table: "PatientStatus",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_RCAContactReimbursementEntity_ReimbursementEntitiesReimbursementEntityId",
                table: "RCAContactReimbursementEntity",
                column: "ReimbursementEntitiesReimbursementEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementEntityRCAContact_RCAContactId",
                table: "ReimbursementEntityRCAContact",
                column: "RCAContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementEntityRCAContact_ReimbursementEntityId",
                table: "ReimbursementEntityRCAContact",
                column: "ReimbursementEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyContact_StudyId",
                table: "StudyContact",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyHeader_StudyId",
                table: "StudyHeader",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyHistology_HistologyId",
                table: "StudyHistology",
                column: "HistologyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyHistology_StudyId",
                table: "StudyHistology",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyLookup_LookupId",
                table: "StudyLookup",
                column: "LookupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPathSubmission");

            migrationBuilder.DropTable(
                name: "Doctor");

            migrationBuilder.DropTable(
                name: "DoNotContact");

            migrationBuilder.DropTable(
                name: "InvoiceItem");

            migrationBuilder.DropTable(
                name: "PathReportExport");

            migrationBuilder.DropTable(
                name: "PatientPhoneNumber");

            migrationBuilder.DropTable(
                name: "PatientStatus");

            migrationBuilder.DropTable(
                name: "RCAContactReimbursementEntity");

            migrationBuilder.DropTable(
                name: "ReimbursementEntityRCAContact");

            migrationBuilder.DropTable(
                name: "StudyContact");

            migrationBuilder.DropTable(
                name: "StudyHeader");

            migrationBuilder.DropTable(
                name: "StudyHistology");

            migrationBuilder.DropTable(
                name: "StudyLookup");

            migrationBuilder.DropTable(
                name: "StudyReportHeader");

            migrationBuilder.DropTable(
                name: "Hospital");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "Batch");

            migrationBuilder.DropTable(
                name: "PathReport");

            migrationBuilder.DropTable(
                name: "RCAContact");

            migrationBuilder.DropTable(
                name: "Histology");

            migrationBuilder.DropTable(
                name: "Lookup");

            migrationBuilder.DropTable(
                name: "ReimbursementEntity");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "Study");
        }
    }
}
