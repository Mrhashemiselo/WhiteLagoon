using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhiteLagoon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataInVillasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Villas",
                columns: new[] { "Id", "CreatedDate", "Description", "ImageUrl", "Name", "Occupancy", "Price", "Sqft", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, null, "Hotel Royal is a hotel located in many cities around the world. It is a hotel brand that operates multiple properties in different locations.", "~/Images/royal.jpg", "Royal", 4, 200.0, 250, null },
                    { 2, null, "The term \"poor hotel\" doesn't have a widely recognized or standard definition. It's a somewhat subjective and vague term that could mean different things to different people. ", "~/Images/poor.jpg", "Poor", 1, 50.0, 50, null },
                    { 3, null, "A KingsHotel typically refers to a hotel that has the word Kings in its name, implying it is a hotel that aims to provide an upscale, high-end, or luxurious experience for its guests.", "~/Images/kings.jpg", "Kings", 6, 400.0, 500, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
