using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;

namespace Majorsoft.CQRS.Repositories.Tests.TestDb
{
	public partial class TestDbContext : DbContext
	{
		public virtual DbSet<Event> Events { get; set; }

		public virtual DbSet<Link> Links { get; set; }

		public virtual DbSet<Message> Messages { get; set; }

		public virtual DbSet<Category> Categories { get; set; }

		public virtual DbSet<InformationCategory> InformationCategories { get; set; }

		public TestDbContext(DbContextOptions<TestDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Link>(entity =>
			{
				entity.ToTable("Link");

				entity.Property(e => e.LinkId).ValueGeneratedNever();

				entity.Property(e => e.CountryOrgCode)
					.HasMaxLength(2)
					.IsUnicode(false)
					.IsFixedLength(true);

				entity.Property(e => e.CreatedBy)
					.IsUnicode(false);

				entity.Property(e => e.CreatedDate).HasColumnType("datetime");

				entity.Property(e => e.LastChanged)
					.IsRowVersion()
					.IsConcurrencyToken();

				entity.Property(e => e.LastModifiedBy)
					.IsUnicode(false)
					.IsRequired(false);

				entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");

				entity.Property(e => e.Organization).HasMaxLength(255);

				entity.Property(e => e.PublishedDate).HasColumnType("datetime");

				entity.Property(e => e.Title)
					.HasMaxLength(255);

				entity.Property(e => e.Url);

				entity.HasOne(d => d.Event)
					.WithMany(p => p.Links)
					.HasForeignKey(d => d.EventId)
					.HasConstraintName("FK_Link_Event")
					.IsRequired(false);

				entity.HasOne(d => d.Message)
					.WithMany(p => p.Links)
					.HasForeignKey(d => d.MessageId)
					.HasConstraintName("FK_Link_Message")
					.IsRequired(false);

				entity.HasOne(d => d.InformationCategory)
					.WithMany(p => p.Links)
					.HasForeignKey(d => d.CategoryId)
					.HasConstraintName("FK_Link_InformationCategory");
			});

			modelBuilder.Entity<InformationCategory>(entity =>
			{
				entity.HasKey(e => e.CategoryId);

				entity.ToTable("InformationCategory");

				entity.Property(e => e.CategoryText)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(e => e.IsSmrelated).HasColumnName("IsSMRelated");
			});
		}
	}

	public partial class Link
	{
		public Link()
		{
		}

		public Guid LinkId { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public string? Url { get; set; }
		public int CategoryId { get; set; }
		public Guid? EventId { get; set; }
		public Guid? MessageId { get; set; }
		public int? WorkflowStatusId { get; set; }
		public DateTime CreatedDate { get; set; }
		public string? CreatedBy { get; set; }
		public DateTime LastModifiedDate { get; set; }
		public string LastModifiedBy { get; set; }
		public byte[]? LastChanged { get; set; }
		public DateTime? PublishedDate { get; set; }
		public string? CountryOrgCode { get; set; }
		public string? Organization { get; set; }

		public virtual Event Event { get; set; }
		public virtual Message Message { get; set; }

		public virtual InformationCategory InformationCategory { get; set; }
	}

	public partial class Event
	{
		public Event()
		{
			Documents = new HashSet<Document>();
			EventAccessControls = new HashSet<EventAccessControl>();
			Links = new HashSet<Link>();
		}

		public Guid EventId { get; set; }
		public int EventCategoryId { get; set; }
		public string ExpertTitle { get; set; }
		public string PublicTitle { get; set; }
		public string CountryCode { get; set; }
		public string Location { get; set; }
		public string EmergencyStatus { get; set; }
		public string EmergencyClass { get; set; }
		public string ExpertDescription { get; set; }
		public string PublicDescription { get; set; }
		public DateTime? EventDate { get; set; }
		public int? InesRatingLevelId { get; set; }
		public int? InesRatingStatusId { get; set; }
		public int ConfidentialityId { get; set; }
		public bool OfficialNotificationFlag { get; set; }
		public string ExpertReportingCountryCode { get; set; }
		public string ExpertReportingOrganization { get; set; }
		public DateTime? ExpertReportedDate { get; set; }
		public Guid? PublicLastMessageId { get; set; }
		public string PublicReportingCountryCode { get; set; }
		public string PublicReportingOrganization { get; set; }
		public DateTime? PublicReportedDate { get; set; }
		public bool? PublicFlag { get; set; }
		public string ReferenceNumber { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? CreatedDate { get; set; }
		public string LastModifiedBy { get; set; }
		public DateTime? LastModifiedDate { get; set; }
		public string PublicLastModifiedBy { get; set; }
		public DateTime? PublicLastModifiedDate { get; set; }
		public bool? AutomaticUpdateFlag { get; set; }
		public byte[] LastChanged { get; set; }

		public virtual ICollection<Document> Documents { get; set; }
		public virtual ICollection<EventAccessControl> EventAccessControls { get; set; }
		public virtual ICollection<Link> Links { get; set; }
	}

	public partial class Message
	{
		public Message()
		{
			Links = new HashSet<Link>();
		}

		public Guid MessageId { get; set; }
		public Guid? EventId { get; set; }
		public string MessageType { get; set; }
		public bool EmerconFlag { get; set; }
		public bool ReportFlag { get; set; }
		public int MessageNumber { get; set; }
		public string IaeaReferenceNumber { get; set; }
		public int MessageStatusId { get; set; }
		public short? BookNo { get; set; }
		public int? InesNo { get; set; }
		public int? ReleaseCharacteristicId { get; set; }
		public bool? ProtectiveActionPlannedTakenFlag { get; set; }
		public string? ActionsTaken { get; set; }
		public string? EventSummary { get; set; }
		public bool? PublishOnNewsFlag { get; set; }
		public string CountryOrgCode { get; set; }
		public bool ReportedByIecFlag { get; set; }
		public bool ExerciseFlag { get; set; }
		public string CreatedBy { get; set; }
		public string LastModifiedBy { get; set; }
		public DateTime LastModifiedDate { get; set; }
		public DateTime CreatedDate { get; set; }
		public byte[] LastChanged { get; set; }
		public DateTime? SubmittedDate { get; set; }
		public DateTime? PublishedDate { get; set; }
		public int ConfidentialityId { get; set; }
		public short CorrectionVersion { get; set; }
		public bool? ConfirmReadActionDismissFlag { get; set; }
		public DateTime? ConfirmReadActionDismissDate { get; set; }
		public Guid? ParentMessageId { get; set; }
		public DateTime? CorrectionPublishedDate { get; set; }
		public string SubmittedBy { get; set; }
		public bool ReportedByNuclearSecurityRoleUserFlag { get; set; }

		public virtual ICollection<Link> Links { get; set; }
	}

	public partial class EventAccessControl
	{
		public Guid EventId { get; set; }
		public string CountryOrgCode { get; set; }
		public int RoleMask { get; set; }
		public Guid EventAccessControlId { get; set; }

		public virtual Event Event { get; set; }
	}

	public partial class Document
	{
		public Guid DocumentId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int CategoryId { get; set; }
		public string ReferenceNumber { get; set; }
		public DateTime? Date { get; set; }
		public Guid? MessageId { get; set; }
		public Guid? EventId { get; set; }
		public int? WorkflowStatusId { get; set; }
		public Guid FileId { get; set; }
		public int? PublicDistributionTypeId { get; set; }
		public string Language { get; set; }
		public DateTime? PublishedDate { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public string LastModifiedBy { get; set; }
		public DateTime LastModifiedDate { get; set; }
		public byte[] LastChanged { get; set; }
		public int? Confidentiality { get; set; }
		public string CountryOrgCode { get; set; }
		public string Organization { get; set; }
		public Guid? RanetRegistrationId { get; set; }

		public virtual Event Event { get; set; }
		public virtual Message Message { get; set; }
	}

	public partial class Category
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }

	}

	public partial class InformationCategory
	{
		public InformationCategory()
		{
			Links = new HashSet<Link>();
		}

		public ICollection<Link> Links { get; set; }
		public int CategoryId { get; set; }
		public string CategoryText { get; set; }
		public bool IsGeneralInfo { get; set; }
		public bool? IsEmerconRelated { get; set; }
		public int? DisplayOrder { get; set; }
		public bool? IsSmrelated { get; set; }
		public int InformationTypeMask { get; set; }
	}
}