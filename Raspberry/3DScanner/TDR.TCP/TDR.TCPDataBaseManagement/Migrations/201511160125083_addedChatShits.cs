namespace TDR.TCPDataBaseManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedChatShits : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessage",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        SenderId = c.Guid(nullable: false),
                        RecipientId = c.Guid(nullable: false),
                        Message = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.RecipientId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.SenderId, cascadeDelete: true)
                .Index(t => t.RecipientId)
                .Index(t => t.SenderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessage", "SenderId", "dbo.User");
            DropForeignKey("dbo.ChatMessage", "RecipientId", "dbo.User");
            DropIndex("dbo.ChatMessage", new[] { "SenderId" });
            DropIndex("dbo.ChatMessage", new[] { "RecipientId" });
            DropTable("dbo.ChatMessage");
        }
    }
}
