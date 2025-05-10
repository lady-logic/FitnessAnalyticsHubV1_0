using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessAnalyticsHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Dev002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "PacePerKilometerInTicks",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "PacePerKilometerInTicks",
                table: "Activities",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");
        }
    }
}
