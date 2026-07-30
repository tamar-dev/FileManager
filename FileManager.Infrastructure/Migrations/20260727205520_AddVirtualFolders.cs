using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtualFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VirtualFolderFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VirtualFolderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualFolderFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VirtualFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualFolders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VirtualFolderFiles_VirtualFolderId_FileEntryId",
                table: "VirtualFolderFiles",
                columns: new[] { "VirtualFolderId", "FileEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualFolders_ParentId_Name",
                table: "VirtualFolders",
                columns: new[] { "ParentId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VirtualFolderFiles");

            migrationBuilder.DropTable(
                name: "VirtualFolders");
        }
    }
}
