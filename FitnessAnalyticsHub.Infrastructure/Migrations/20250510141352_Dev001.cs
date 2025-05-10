using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessAnalyticsHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Dev001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "PacePerKilometerInTicks",
                table: "Activities",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PacePerKilometerInTicks",
                table: "Activities");
        }
    }
}
