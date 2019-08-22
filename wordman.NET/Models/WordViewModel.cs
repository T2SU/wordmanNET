using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using wordman.Words;

namespace wordman.Models
{
    public class WordViewModel
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        [FromQuery(Name = "keyword")]
        [StringLength(2000)]
        public string Keyword { get; set; }

        [FromQuery(Name = "order")]
        public Order Order { get; set; } = Order.Desc;

        public WordViewModel CopyByPage(int page)
        {
            return new WordViewModel() { Keyword = Keyword, Page = page, Order = Order };
        }
    }
}
