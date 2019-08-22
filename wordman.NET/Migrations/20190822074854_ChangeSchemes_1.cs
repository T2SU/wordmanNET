using Microsoft.EntityFrameworkCore.Migrations;

namespace wordman.Migrations
{
    public partial class ChangeSchemes_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Antonyms");

            migrationBuilder.DropTable(
                name: "Example");

            migrationBuilder.DropTable(
                name: "Synonyms");

            migrationBuilder.CreateTable(
                name: "RelatedString",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordID = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedString", x => x.ID);
                    table.ForeignKey(
                        name: "foreignKey_Word_RelatedStrings",
                        column: x => x.WordID,
                        principalTable: "Words",
                        principalColumn: "WordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatedWord",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordID = table.Column<int>(nullable: false),
                    RelatedWordID = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedWord", x => x.ID);
                    table.ForeignKey(
                        name: "foreignKey_Word_RelatedWords",
                        column: x => x.WordID,
                        principalTable: "Words",
                        principalColumn: "WordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelatedString_WordID",
                table: "RelatedString",
                column: "WordID");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedString_Type_WordID_Content",
                table: "RelatedString",
                columns: new[] { "Type", "WordID", "Content" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedWord_WordID",
                table: "RelatedWord",
                column: "WordID");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedWord_Type_WordID_RelatedWordID",
                table: "RelatedWord",
                columns: new[] { "Type", "WordID", "RelatedWordID" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelatedString");

            migrationBuilder.DropTable(
                name: "RelatedWord");

            migrationBuilder.CreateTable(
                name: "Antonyms",
                columns: table => new
                {
                    AntonymID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AntonymWordID = table.Column<int>(nullable: false),
                    WordID = table.Column<int>(nullable: false)
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
                    Sentence = table.Column<string>(nullable: true),
                    WordID = table.Column<int>(nullable: false)
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
                    SynonymWordID = table.Column<int>(nullable: false),
                    WordID = table.Column<int>(nullable: false)
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
        }
    }
}
