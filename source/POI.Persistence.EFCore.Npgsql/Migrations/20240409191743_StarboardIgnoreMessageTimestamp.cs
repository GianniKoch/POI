using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POI.Persistence.EFCore.Npgsql.Migrations
{
    /// <inheritdoc />
    public partial class StarboardIgnoreMessageTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StarboardMessageIgnoreAfter",
                table: "ServerSettings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StarboardMessageIgnoreAfter",
                table: "ServerSettings");
        }
    }
}
