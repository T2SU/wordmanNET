using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wordman.SQLite
{
    public class Antonym
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AntonymID { get; set; }

        public int WordID { get; set; }

        public int AntonymWordID { get; set; }

        public Word Word { get; set; }
    }
}
