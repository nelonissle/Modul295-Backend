using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modul295PraxisArbeit.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndAssignedUserIdToServiceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Users_AssignedUserUserId",
                table: "ServiceOrders");

            migrationBuilder.RenameColumn(
                name: "AssignedUserUserId",
                table: "ServiceOrders",
                newName: "AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceOrders_AssignedUserUserId",
                table: "ServiceOrders",
                newName: "IX_ServiceOrders_AssignedUserId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ServiceOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Users_AssignedUserId",
                table: "ServiceOrders",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Users_AssignedUserId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ServiceOrders");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                table: "ServiceOrders",
                newName: "AssignedUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceOrders_AssignedUserId",
                table: "ServiceOrders",
                newName: "IX_ServiceOrders_AssignedUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Users_AssignedUserUserId",
                table: "ServiceOrders",
                column: "AssignedUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
