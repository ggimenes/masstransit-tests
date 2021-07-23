using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InMemoryOutboxTest.Api.Migrations
{
    public partial class RemovingSecondConsumer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObservedInMemoryOutboxDelay",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "SecondEventReceived",
                table: "SagaStates");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ObservedInMemoryOutboxDelay",
                table: "SagaStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SecondEventReceived",
                table: "SagaStates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
