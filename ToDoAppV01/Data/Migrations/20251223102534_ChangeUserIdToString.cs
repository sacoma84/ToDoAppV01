using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAppV01.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Falls eine FK existiert, zuerst entfernen (Name ggf. anpassen)
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoLists_AspNetUsers_UserId",
                table: "ToDoLists");

            // Spaltentyp ändern: int -> nvarchar(450)
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ToDoLists",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // FK wieder hinzufügen (auf AspNetUsers(Id))
            migrationBuilder.AddForeignKey(
                name: "FK_ToDoLists_AspNetUsers_UserId",
                table: "ToDoLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "UserId",
            //    table: "ToDoLists",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDoLists_AspNetUsers_UserId",
                table: "ToDoLists");

             migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ToDoLists",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

             migrationBuilder.AddForeignKey(
                name: "FK_ToDoLists_AspNetUsers_UserId",
                table: "ToDoLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
