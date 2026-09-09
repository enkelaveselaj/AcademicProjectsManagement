using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicProjects.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentAuthorAndDocumentUploader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UploadedById",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Comments");
        }
    }
}
