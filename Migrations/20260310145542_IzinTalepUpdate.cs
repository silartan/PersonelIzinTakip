using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelIzinTakip.Migrations
{
    /// <inheritdoc />
    public partial class IzinTalepUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "IzinTalepleri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicTarihi",
                table: "IzinTalepleri",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BitisTarihi",
                table: "IzinTalepleri",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "IzinTalepleri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GunSayisi",
                table: "IzinTalepleri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "IzinTalepleri",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_UserId",
                table: "IzinTalepleri",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IzinTalepleri_AspNetUsers_UserId",
                table: "IzinTalepleri",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IzinTalepleri_AspNetUsers_UserId",
                table: "IzinTalepleri");

            migrationBuilder.DropIndex(
                name: "IX_IzinTalepleri_UserId",
                table: "IzinTalepleri");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "IzinTalepleri");

            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "IzinTalepleri");

            migrationBuilder.DropColumn(
                name: "BitisTarihi",
                table: "IzinTalepleri");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "IzinTalepleri");

            migrationBuilder.DropColumn(
                name: "GunSayisi",
                table: "IzinTalepleri");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IzinTalepleri");
        }
    }
}
