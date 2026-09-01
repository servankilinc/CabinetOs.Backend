using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinetOs.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameCameraCaptureStorageKeyToRelativePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StorageKey",
                table: "CameraCapture",
                newName: "RelativePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RelativePath",
                table: "CameraCapture",
                newName: "StorageKey");
        }
    }
}
