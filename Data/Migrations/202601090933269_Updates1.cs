// <copyright file="202601090933269_Updates1.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// .
    /// </summary>
    public partial class Updates1 : DbMigration
    {
        /// <summary>
        /// .
        /// </summary>
        public override void Up()
        {
            this.AddColumn("dbo.Borrowing", "StaffId", c => c.Int());
            this.CreateIndex("dbo.Borrowing", "StaffId");
            this.AddForeignKey("dbo.Borrowing", "StaffId", "dbo.Reader", "Id");
        }

        /// <summary>
        /// .
        /// </summary>
        public override void Down()
        {
            this.DropForeignKey("dbo.Borrowing", "StaffId", "dbo.Reader");
            this.DropIndex("dbo.Borrowing", new[] { "StaffId" });
            this.DropColumn("dbo.Borrowing", "StaffId");
        }
    }
}
