// <copyright file="LibraryDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data
{
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Domain.Models;

    /// <summary>
    /// Entity Framework DbContext for the library management system.
    /// Configures all entities and their relationships with the database.
    /// </summary>
    public class LibraryDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryDbContext"/> class.
        /// Uses the connection string named "LibraryConnection" from App.config.
        /// </summary>
        public LibraryDbContext()
            : base("name=LibraryConnection")
        {
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;
        }

        /// <summary>
        /// <para>Gets or sets the Authors DbSet.</para>
        /// </summary>
        public DbSet<Author> Authors { get; set; }

        /// <summary>
        /// <para>Gets or sets the Books DbSet.</para>
        /// </summary>
        public DbSet<Book> Books { get; set; }

        /// <summary>
        /// <para>Gets or sets the Domains DbSet.</para>
        /// </summary>
        public DbSet<BookDomain> Domains { get; set; }

        /// <summary>
        /// <para>Gets or sets the Editions DbSet.</para>
        /// </summary>
        public DbSet<Edition> Editions { get; set; }

        /// <summary>
        /// <para>Gets or sets the Readers DbSet.</para>
        /// </summary>
        public DbSet<Reader> Readers { get; set; }

        /// <summary>
        /// <para>Gets or sets the Borrowings DbSet.</para>
        /// </summary>
        public DbSet<Borrowing> Borrowings { get; set; }

        /// <summary>
        /// <para>Gets or sets the LoanExtensions DbSet.</para>
        /// </summary>
        public DbSet<LoanExtension> LoanExtensions { get; set; }

        /// <summary>
        /// Configures the model on context creation.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Author
            modelBuilder.Entity<Author>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Author>()
                .Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Author>()
                .Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(100);

            // BookDomain
            modelBuilder.Entity<BookDomain>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<BookDomain>()
                .Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<BookDomain>()
                .HasOptional(d => d.ParentDomain)
                .WithMany(d => d.Subdomains)
                .HasForeignKey(d => d.ParentDomainId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Domains)
                .WithMany(d => d.Books)
                .Map(m =>
                {
                    m.ToTable("BookBookDomain");
                    m.MapLeftKey("BookId");
                    m.MapRightKey("DomainId");
                });

            // Book
            modelBuilder.Entity<Book>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Book>()
                .Property(b => b.ISBN)
                .HasMaxLength(20);

            modelBuilder.Entity<Book>()
                .Property(b => b.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .Map(m =>
                {
                    m.ToTable("BookAuthor");
                    m.MapLeftKey("BookId");
                    m.MapRightKey("AuthorId");
                });

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Editions)
                .WithRequired(e => e.Book)
                .HasForeignKey(e => e.BookId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.BorrowingRecords)
                .WithRequired(br => br.Book)
                .HasForeignKey(br => br.BookId)
                .WillCascadeOnDelete(false);

            // Edition
            modelBuilder.Entity<Edition>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Edition>()
                .Property(e => e.Publisher)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Edition>()
                .Property(e => e.BookType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Edition>()
                .Property(e => e.Year)
                .IsRequired();

            modelBuilder.Entity<Edition>()
                .Property(e => e.EditionNumber)
                .IsRequired();

            modelBuilder.Entity<Edition>()
                .Property(e => e.PageCount)
                .IsRequired();

            // Reader
            modelBuilder.Entity<Reader>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Reader>()
                .Property(r => r.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Reader>()
                .Property(r => r.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Reader>()
                .Property(r => r.Address)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Reader>()
                .Property(r => r.Email)
                .HasMaxLength(150);

            modelBuilder.Entity<Reader>()
                .Property(r => r.PhoneNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<Reader>()
                .Property(r => r.IsStaff)
                .IsRequired();

            modelBuilder.Entity<Reader>()
                .Property(r => r.RegistrationDate)
                .IsRequired();

            // Reader -> Borrowing (as borrower)
            modelBuilder.Entity<Reader>()
                .HasMany(r => r.BorrowingRecords)
                .WithRequired(br => br.Reader)
                .HasForeignKey(br => br.ReaderId)
                .WillCascadeOnDelete(false);

            // Reader -> Borrowing (as staff who gave the borrowing)
            modelBuilder.Entity<Reader>()
                .HasMany(r => r.BorrowingGiven)
                .WithOptional(br => br.Staff)
                .HasForeignKey(br => br.StaffId)
                .WillCascadeOnDelete(false);

            // Borrowing
            modelBuilder.Entity<Borrowing>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.BorrowingDate)
                .HasColumnType("datetime2")
                .IsRequired();

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.DueDate)
                .HasColumnType("datetime2")
                .IsRequired();

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.ReturnDate)
                .HasColumnType("datetime2")
                .IsOptional();

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.IsActive)
                .IsRequired();

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.InitialBorrowingDays)
                .IsRequired();

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.TotalExtensionDays)
                .IsRequired();

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.LastExtensionDate)
                .HasColumnType("datetime2")
                .IsOptional();

            // One-to-Many: Borrowing -> LoanExtension
            modelBuilder.Entity<Borrowing>()
                .HasMany(b => b.Extensions)
                .WithRequired(le => le.Borrowing)
                .HasForeignKey(le => le.BorrowingId)
                .WillCascadeOnDelete(true);

            // LoanExtension
            modelBuilder.Entity<LoanExtension>()
                .HasKey(le => le.Id);

            modelBuilder.Entity<LoanExtension>()
                .Property(le => le.BorrowingId)
                .IsRequired();

            modelBuilder.Entity<LoanExtension>()
                .Property(le => le.ExtensionDate)
                .IsRequired();

            modelBuilder.Entity<LoanExtension>()
                .Property(le => le.ExtensionDays)
                .IsRequired();
        }
    }
}
