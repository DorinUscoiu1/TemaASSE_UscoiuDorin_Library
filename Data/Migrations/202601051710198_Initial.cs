// <copyright file="202601051710198_Initial.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// .
    /// </summary>
    public partial class Initial : DbMigration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            this.CreateTable(
                "dbo.Author",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.Book",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 1000),
                        ISBN = c.String(maxLength: 20),
                        TotalCopies = c.Int(nullable: false),
                        ReadingRoomOnlyCopies = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            this.CreateTable(
                "dbo.Borrowing",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReaderId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        BorrowingDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        ReturnDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        TotalExtensionDays = c.Int(nullable: false),
                        LastExtensionDate = c.DateTime(),
                        InitialBorrowingDays = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reader", t => t.ReaderId)
                .ForeignKey("dbo.Book", t => t.BookId)
                .Index(t => t.ReaderId)
                .Index(t => t.BookId);
            this.CreateTable(
                "dbo.LoanExtension",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BorrowingId = c.Int(nullable: false),
                        ExtensionDate = c.DateTime(nullable: false),
                        ExtensionDays = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Borrowing", t => t.BorrowingId, cascadeDelete: true)
                .Index(t => t.BorrowingId);

            this.CreateTable(
                "dbo.Reader",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        Address = c.String(nullable: false, maxLength: 255),
                        PhoneNumber = c.String(maxLength: 20),
                        Email = c.String(maxLength: 150),
                        RegistrationDate = c.DateTime(nullable: false),
                        IsStaff = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.BookDomain",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        ParentDomainId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookDomain", t => t.ParentDomainId)
                .Index(t => t.ParentDomainId);

            this.CreateTable(
                "dbo.Edition",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BookId = c.Int(nullable: false),
                        Publisher = c.String(nullable: false, maxLength: 150),
                        Year = c.Int(nullable: false),
                        EditionNumber = c.Int(nullable: false),
                        PageCount = c.Int(nullable: false),
                        BookType = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .Index(t => t.BookId);

            this.CreateTable(
                "dbo.BookAuthor",
                c => new
                    {
                        BookId = c.Int(nullable: false),
                        AuthorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookId, t.AuthorId })
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Author", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.AuthorId);

            this.CreateTable(
                "dbo.BookBookDomain",
                c => new
                    {
                        BookId = c.Int(nullable: false),
                        DomainId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookId, t.DomainId })
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.BookDomain", t => t.DomainId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.DomainId);
        }

        /// <inheritdoc/>
        public override void Down()
        {
            this.DropForeignKey("dbo.Edition", "BookId", "dbo.Book");
            this.DropForeignKey("dbo.BookBookDomain", "DomainId", "dbo.BookDomain");
            this.DropForeignKey("dbo.BookBookDomain", "BookId", "dbo.Book");
            this.DropForeignKey("dbo.BookDomain", "ParentDomainId", "dbo.BookDomain");
            this.DropForeignKey("dbo.Borrowing", "BookId", "dbo.Book");
            this.DropForeignKey("dbo.Borrowing", "ReaderId", "dbo.Reader");
            this.DropForeignKey("dbo.LoanExtension", "BorrowingId", "dbo.Borrowing");
            this.DropForeignKey("dbo.BookAuthor", "AuthorId", "dbo.Author");
            this.DropForeignKey("dbo.BookAuthor", "BookId", "dbo.Book");
            this.DropIndex("dbo.BookBookDomain", new[] { "DomainId" });
            this.DropIndex("dbo.BookBookDomain", new[] { "BookId" });
            this.DropIndex("dbo.BookAuthor", new[] { "AuthorId" });
            this.DropIndex("dbo.BookAuthor", new[] { "BookId" });
            this.DropIndex("dbo.Edition", new[] { "BookId" });
            this.DropIndex("dbo.BookDomain", new[] { "ParentDomainId" });
            this.DropIndex("dbo.LoanExtension", new[] { "BorrowingId" });
            this.DropIndex("dbo.Borrowing", new[] { "BookId" });
            this.DropIndex("dbo.Borrowing", new[] { "ReaderId" });
            this.DropTable("dbo.BookBookDomain");
            this.DropTable("dbo.BookAuthor");
            this.DropTable("dbo.Edition");
            this.DropTable("dbo.BookDomain");
            this.DropTable("dbo.Reader");
            this.DropTable("dbo.LoanExtension");
            this.DropTable("dbo.Borrowing");
            this.DropTable("dbo.Book");
            this.DropTable("dbo.Author");
        }
    }
}
