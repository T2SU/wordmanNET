using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wordman.SQLite;

namespace wordman.Mvc
{
    public class WordResult : JsonResult
    {
        public WordResult(Word word) : base(Serialize(word))
        {
            Word = word;
        }

        public Word Word { get; }

        private static object Serialize(Word new_word)
        {
            return new
            {
                word = new_word.Content,
                @ref = new_word.Referenced,
                time = new_word.LastReferenced.ToString()
            };
        }
    }
}
