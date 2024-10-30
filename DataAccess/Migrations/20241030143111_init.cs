using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "80375fe5-9bcc-4b1e-a44f-5a0a1539fe2f", "AQAAAAIAAYagAAAAEMV5R0yswD1LfBY6Hrn/qdp4MoB6XSTMfDHvWksA63U7SaUVbA9ucCOuYmliORlEAA==", "02e5c499-ca5e-4168-bc0a-d6230e8bab42" });

            migrationBuilder.UpdateData(
                table: "BasketItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTime(2024, 10, 30, 18, 1, 11, 299, DateTimeKind.Local).AddTicks(8265));

            migrationBuilder.UpdateData(
                table: "Baskets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Payed" },
                values: new object[] { new DateTime(2024, 10, 30, 18, 1, 11, 299, DateTimeKind.Local).AddTicks(8233), new DateTime(2024, 10, 31, 18, 1, 11, 299, DateTimeKind.Local).AddTicks(8233) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Img" },
                values: new object[] { new DateTime(2024, 10, 30, 18, 1, 11, 299, DateTimeKind.Local).AddTicks(8199), "wwwroot\\uploads\\Screenshot 2024-10-15 125514.jpg" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Img" },
                values: new object[] { new DateTime(2024, 10, 30, 18, 1, 11, 299, DateTimeKind.Local).AddTicks(8217), "wwwroot\\uploads\\Screenshot 2024-10-15 125514.jpg" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b9d2083c-6b83-44d7-ba3f-daf597fcddc6", "AQAAAAIAAYagAAAAECn2040/y3sCp/4SBABoP/ubHmPnhEBXB3NYVMXSTR69myR+Vm1mp0bry1zqAKooUg==", "003ddbfa-0db0-44b9-8841-430b281c66dd" });

            migrationBuilder.UpdateData(
                table: "BasketItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTime(2024, 10, 28, 20, 9, 6, 581, DateTimeKind.Local).AddTicks(3819));

            migrationBuilder.UpdateData(
                table: "Baskets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Payed" },
                values: new object[] { new DateTime(2024, 10, 28, 20, 9, 6, 581, DateTimeKind.Local).AddTicks(3783), new DateTime(2024, 10, 29, 20, 9, 6, 581, DateTimeKind.Local).AddTicks(3783) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Img" },
                values: new object[] { new DateTime(2024, 10, 28, 20, 9, 6, 581, DateTimeKind.Local).AddTicks(3753), "uploads/Screenshot 2024-10-15 125514" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Img" },
                values: new object[] { new DateTime(2024, 10, 28, 20, 9, 6, 581, DateTimeKind.Local).AddTicks(3764), "uploads/Screenshot 2024-10-15 125514" });
        }
    }
}
