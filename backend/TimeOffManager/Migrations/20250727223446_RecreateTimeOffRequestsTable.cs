using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeOffManager.Migrations
{
    public partial class RecreateTimeOffRequestsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_UserId",
                table: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Requests",
                table: "Requests");

            migrationBuilder.RenameTable(
                name: "Requests",
                newName: "TimeOffRequests");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_UserId",
                table: "TimeOffRequests",
                newName: "IX_TimeOffRequests_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeOffRequests",
                table: "TimeOffRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_Users_UserId",
                table: "TimeOffRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_Users_UserId",
                table: "TimeOffRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeOffRequests",
                table: "TimeOffRequests");

            migrationBuilder.RenameTable(
                name: "TimeOffRequests",
                newName: "Requests");

            migrationBuilder.RenameIndex(
                name: "IX_TimeOffRequests_UserId",
                table: "Requests",
                newName: "IX_Requests_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Requests",
                table: "Requests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
