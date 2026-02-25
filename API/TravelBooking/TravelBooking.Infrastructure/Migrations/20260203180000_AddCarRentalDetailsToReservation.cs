using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCarRentalDetailsToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CarPickUpDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true,
                comment: "Arac kiralama alis tarihi");

            migrationBuilder.AddColumn<DateTime>(
                name: "CarDropOffDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true,
                comment: "Arac kiralama birakis tarihi");

            migrationBuilder.AddColumn<string>(
                name: "CarPickUpLocation",
                table: "Reservations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                comment: "Arac kiralama alis yeri");

            migrationBuilder.AddColumn<string>(
                name: "CarDropOffLocation",
                table: "Reservations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                comment: "Arac kiralama birakis yeri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CarPickUpDate", table: "Reservations");
            migrationBuilder.DropColumn(name: "CarDropOffDate", table: "Reservations");
            migrationBuilder.DropColumn(name: "CarPickUpLocation", table: "Reservations");
            migrationBuilder.DropColumn(name: "CarDropOffLocation", table: "Reservations");
        }
    }
}
