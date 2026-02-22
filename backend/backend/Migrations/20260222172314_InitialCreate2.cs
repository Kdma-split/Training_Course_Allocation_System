using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentApprovals",
                columns: table => new
                {
                    AssignmentApprovalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentChannelUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChannelUserAssignmentChannelUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentApprovals", x => x.AssignmentApprovalId);
                    table.ForeignKey(
                        name: "FK_AssignmentApprovals_AssignmentChannelUsers_ChannelUserAssignmentChannelUserId",
                        column: x => x.ChannelUserAssignmentChannelUserId,
                        principalTable: "AssignmentChannelUsers",
                        principalColumn: "AssignmentChannelUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelApprovals",
                columns: table => new
                {
                    ChannelApprovalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isActived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelApprovals", x => x.ChannelApprovalId);
                    table.ForeignKey(
                        name: "FK_ChannelApprovals_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "ChannelUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseApprovals",
                columns: table => new
                {
                    CourseApprovalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseChannelUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseChannelUserId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseApprovals", x => x.CourseApprovalId);
                    table.ForeignKey(
                        name: "FK_CourseApprovals_CourseChannelUsers_CourseChannelUserId1",
                        column: x => x.CourseChannelUserId1,
                        principalTable: "CourseChannelUsers",
                        principalColumn: "CourseChannelUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentApprovals_ChannelUserAssignmentChannelUserId",
                table: "AssignmentApprovals",
                column: "ChannelUserAssignmentChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelApprovals_ChannelUserId",
                table: "ChannelApprovals",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseApprovals_CourseChannelUserId1",
                table: "CourseApprovals",
                column: "CourseChannelUserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentApprovals");

            migrationBuilder.DropTable(
                name: "ChannelApprovals");

            migrationBuilder.DropTable(
                name: "CourseApprovals");
        }
    }
}
