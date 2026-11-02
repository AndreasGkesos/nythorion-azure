using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nythorion.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameDocumentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentChunks_Documents_DocumentId",
                table: "DocumentChunks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentChunks",
                table: "DocumentChunks");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "documents");

            migrationBuilder.RenameTable(
                name: "DocumentChunks",
                newName: "document_chunks");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentChunks_DocumentId",
                table: "document_chunks",
                newName: "IX_document_chunks_DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_documents",
                table: "documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_document_chunks",
                table: "document_chunks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_chunks_documents_DocumentId",
                table: "document_chunks",
                column: "DocumentId",
                principalTable: "documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_chunks_documents_DocumentId",
                table: "document_chunks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_documents",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document_chunks",
                table: "document_chunks");

            migrationBuilder.RenameTable(
                name: "documents",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "document_chunks",
                newName: "DocumentChunks");

            migrationBuilder.RenameIndex(
                name: "IX_document_chunks_DocumentId",
                table: "DocumentChunks",
                newName: "IX_DocumentChunks_DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentChunks",
                table: "DocumentChunks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentChunks_Documents_DocumentId",
                table: "DocumentChunks",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
