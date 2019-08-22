using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using wordman.Words;

namespace wordman.SQLite
{
    public class RelatedWord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int WordID { get; set; }

        public int RelatedWordID { get; set; }

        public RelatedWordType Type { get; set; }

        public Word Word { get; set; }
    }
}
