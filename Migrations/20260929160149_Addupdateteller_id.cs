using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class Addupdateteller_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Services_serviceid",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_branches_branchid",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_users_user_idid",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_branchid",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_serviceid",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_user_idid",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "branchid",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "serviceid",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "user_idid",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_branch_id",
                table: "Tickets",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_service_id",
                table: "Tickets",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_teller_id",
                table: "Tickets",
                column: "teller_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Services_service_id",
                table: "Tickets",
                column: "service_id",
                principalTable: "Services",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_branches_branch_id",
                table: "Tickets",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_users_teller_id",
                table: "Tickets",
                column: "teller_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Services_service_id",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_branches_branch_id",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_users_teller_id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_branch_id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_service_id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_teller_id",
                table: "Tickets");

            migrationBuilder.AddColumn<int>(
                name: "branchid",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "serviceid",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "user_idid",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_branchid",
                table: "Tickets",
                column: "branchid");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_serviceid",
                table: "Tickets",
                column: "serviceid");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_user_idid",
                table: "Tickets",
                column: "user_idid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Services_serviceid",
                table: "Tickets",
                column: "serviceid",
                principalTable: "Services",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_branches_branchid",
                table: "Tickets",
                column: "branchid",
                principalTable: "branches",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_users_user_idid",
                table: "Tickets",
                column: "user_idid",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
