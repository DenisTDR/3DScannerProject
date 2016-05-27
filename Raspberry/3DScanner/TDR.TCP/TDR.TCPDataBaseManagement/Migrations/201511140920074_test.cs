namespace TDR.TCPDataBaseManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Account",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Username = c.String(unicode: false),
                        Password = c.String(unicode: false),
                        UserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Username = c.String(unicode: false),
                        FirstName = c.String(unicode: false),
                        LastName = c.String(unicode: false),
                        Email = c.String(unicode: false),
                        Nick = c.String(unicode: false),
                        AccountId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Account", "UserId", "dbo.User");
            DropIndex("dbo.Account", new[] { "UserId" });
            DropTable("dbo.User");
            DropTable("dbo.Account");
        }
    }
}
