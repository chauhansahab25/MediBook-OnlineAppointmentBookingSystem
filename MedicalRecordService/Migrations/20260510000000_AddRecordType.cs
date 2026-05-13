using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalRecordService.Migrations
{
    public partial class AddRecordType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecordType",
                table: "MedicalRecords",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Consultation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordType",
                table: "MedicalRecords");
        }
    }
}
