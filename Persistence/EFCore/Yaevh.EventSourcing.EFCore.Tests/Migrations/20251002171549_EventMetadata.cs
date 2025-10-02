using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaevh.EventSourcing.EFCore.Tests.Migrations
{
    /// <inheritdoc />
    public partial class EventMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataType",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_AggregateId_EventIndex",
                table: "Events",
                columns: new[] { "AggregateId", "EventIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_AggregateId_EventIndex",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MetadataType",
                table: "Events");
        }
    }
}
