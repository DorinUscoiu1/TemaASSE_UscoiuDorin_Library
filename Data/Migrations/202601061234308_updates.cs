// <copyright file="202601061234308_updates.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>df.</summary>
    public partial class Updates : DbMigration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            this.AlterColumn("dbo.Borrowing", "BorrowingDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            this.AlterColumn("dbo.Borrowing", "DueDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            this.AlterColumn("dbo.Borrowing", "ReturnDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            this.AlterColumn("dbo.Borrowing", "LastExtensionDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }

        /// <inheritdoc/>
        public override void Down()
        {
            this.AlterColumn("dbo.Borrowing", "LastExtensionDate", c => c.DateTime());
            this.AlterColumn("dbo.Borrowing", "ReturnDate", c => c.DateTime());
            this.AlterColumn("dbo.Borrowing", "DueDate", c => c.DateTime(nullable: false));
            this.AlterColumn("dbo.Borrowing", "BorrowingDate", c => c.DateTime(nullable: false));
        }
    }
}
