using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Branchid",
                table: "branch_services",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    device_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    device_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: true),
                    counter_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last_seen_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                    table.ForeignKey(
                        name: "FK_devices_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_branch_services_Branchid",
                table: "branch_services",
                column: "Branchid");

            migrationBuilder.CreateIndex(
                name: "IX_devices_branch_id",
                table: "devices",
                column: "branch_id");

            migrationBuilder.AddForeignKey(
                name: "FK_branch_services_branches_Branchid",
                table: "branch_services",
                column: "Branchid",
                principalTable: "branches",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_branch_services_branches_Branchid",
                table: "branch_services");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropIndex(
                name: "IX_branch_services_Branchid",
                table: "branch_services");

            migrationBuilder.DropColumn(
                name: "Branchid",
                table: "branch_services");
        }
    }
}
