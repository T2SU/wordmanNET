using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wordman.SQLite
{
    public class Synonym
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SynonymID { get; set; }

        public int WordID { get; set; }

        public int SynonymWordID { get; set; }

        public Word Word { get; set; }
    }
}
