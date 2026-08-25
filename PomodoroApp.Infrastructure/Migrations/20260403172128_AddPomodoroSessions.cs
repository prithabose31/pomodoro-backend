using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomodoroApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPomodoroSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkMinutes = table.Column<int>(type: "int", nullable: false),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false),
                    CyclesCompleted = table.Column<int>(type: "int", nullable: false),
                    TotalMinutesLogged = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PomodoroSessions_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PomodoroSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_TaskId",
                table: "PomodoroSessions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId",
                table: "PomodoroSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PomodoroSessions");
        }
    }
}
