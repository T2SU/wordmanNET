using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace wordman.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    WordID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: true),
                    Referenced = table.Column<int>(nullable: false),
                    LastReferenced = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.WordID);
                });

            migrationBuilder.CreateTable(
                name: "Antonyms",
                columns: table => new
                {
                    AntonymID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordID = table.Column<int>(nullable: false),
                    AntonymWordID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antonyms", x => x.AntonymID);
                    table.ForeignKey(
                        name: "foreignKey_Word_Antonyms",
                        column: x => x.WordID,
                        principalTable: "Words",
                        principalColumn: "WordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Example",
                columns: table => new
                {
                    ExampleID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordID = table.Column<int>(nullable: false),
                    Sentence = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Example", x => x.ExampleID);
                    table.ForeignKey(
                        name: "foreignKey_Word_Examples",
                        column: x => x.WordID,
                        principalTable: "Words",
                        principalColumn: "WordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Synonyms",
                columns: table => new
                {
                    SynonymID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordID = table.Column<int>(nullable: false),
                    SynonymWordID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Synonyms", x => x.SynonymID);
                    table.ForeignKey(
                        name: "foreignKey_Word_Synonyms",
                        column: x => x.WordID,
                        principalTable: "Words",
                        principalColumn: "WordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Antonyms_WordID",
                table: "Antonyms",
                column: "WordID");

            migrationBuilder.CreateIndex(
                name: "IX_Example_WordID",
                table: "Example",
                column: "WordID");

            migrationBuilder.CreateIndex(
                name: "IX_Synonyms_WordID",
                table: "Synonyms",
                column: "WordID");

            migrationBuilder.CreateIndex(
                name: "IX_Words_Content",
                table: "Words",
                column: "Content",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Antonyms");

            migrationBuilder.DropTable(
                name: "Example");

            migrationBuilder.DropTable(
                name: "Synonyms");

            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
