using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using wordman.Words;

namespace wordman.SQLite
{
    public class RelatedString
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int WordID { get; set; }

        public string Content { get; set; }

        public RelatedStringType Type { get; set; }

        public Word Word { get; set; }
    }
}
