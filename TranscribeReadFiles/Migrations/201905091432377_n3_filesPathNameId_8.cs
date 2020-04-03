namespace TranscribeReadFiles.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class n3_filesPathNameId_8 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OriginData",
                c => new
                    {
                        FilesId = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                        CaseHistoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        Sex = c.Int(nullable: false),
                        Years = c.Int(nullable: false),
                        MajorDiagnosisCoding = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        MajorDiagnosis = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        HistoryOfPastIllness = c.String(unicode: false),
                        Temperature = c.Double(),
                        Pulse = c.Double(),
                        Breath = c.Double(),
                        Blood_Pressure_Systolic = c.Double(),
                        Blood_Pressure_Diastolic = c.Double(),
                        Width = c.Double(),
                        Height = c.Double(),
                        Corneal_Endothelium_Right = c.Double(),
                        Corneal_Endothelium_Left = c.Double(),
                        Right_Intraocular_Pressure = c.Double(),
                        Left_Intraocular_Pressure = c.Double(),
                    })
                .PrimaryKey(t => t.FilesId)
                .Index(t => t.CaseHistoryId, clustered: true, name: "INDEX_REGNUM");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.OriginData", "INDEX_REGNUM");
            DropTable("dbo.OriginData");
        }
    }
}
