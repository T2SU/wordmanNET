﻿using System;
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

        public List<RelatedWord> RelatedWords { get; set; }

        public List<RelatedString> RelatedStrings { get; set; }

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
