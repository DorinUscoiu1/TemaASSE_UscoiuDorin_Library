namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updates1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Borrowing", "StaffId", c => c.Int());
            CreateIndex("dbo.Borrowing", "StaffId");
            AddForeignKey("dbo.Borrowing", "StaffId", "dbo.Reader", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Borrowing", "StaffId", "dbo.Reader");
            DropIndex("dbo.Borrowing", new[] { "StaffId" });
            DropColumn("dbo.Borrowing", "StaffId");
        }
    }
}
