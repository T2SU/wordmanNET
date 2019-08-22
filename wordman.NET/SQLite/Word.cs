using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace wordman.SQLite
{
    public class Word
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WordID { get; set; }

        public string Content { get; set; }

        public int Referenced { get; set; }

        public DateTime LastReferenced { get; set; }

        [JsonIgnore]
        public virtual ICollection<RelatedWord> RelatedWords { get; set; } = new List<RelatedWord>();

        [JsonIgnore]
        public virtual ICollection<RelatedString> RelatedStrings { get; set; } = new List<RelatedString>();

        public Word Compact()
        {
            return new Word()
            {
                WordID = WordID,
                Content = Content,
                Referenced = Referenced,
                LastReferenced = LastReferenced
            };
        }
    }
}
