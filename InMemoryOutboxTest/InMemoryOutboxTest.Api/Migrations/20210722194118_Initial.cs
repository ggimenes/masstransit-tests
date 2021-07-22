using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InMemoryOutboxTest.Api.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaStates",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedDelayBeforeSaveChanges = table.Column<int>(type: "int", nullable: false),
                    ObservedInMemoryOutboxDelay = table.Column<int>(type: "int", nullable: false),
                    SecondEventPublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SecondEventReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaStates", x => x.CorrelationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaStates");
        }
    }
}
