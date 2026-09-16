using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMetroService.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Update_Journey_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Tickets_TicketId",
                table: "Journeys");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "Journeys",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "RapidPassId",
                table: "Journeys",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_RapidPassId",
                table: "Journeys",
                column: "RapidPassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_RapidPasses_RapidPassId",
                table: "Journeys",
                column: "RapidPassId",
                principalTable: "RapidPasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Tickets_TicketId",
                table: "Journeys",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_RapidPasses_RapidPassId",
                table: "Journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Tickets_TicketId",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_RapidPassId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "RapidPassId",
                table: "Journeys");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "Journeys",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Tickets_TicketId",
                table: "Journeys",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
