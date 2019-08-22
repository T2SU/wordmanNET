using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using wordman.Words;

namespace wordman.Models
{
    public class WordViewModel
    {
        public int Page { get; set; } = 1;

        [StringLength(2000)]
        public string Keyword { get; set; }

        public string Column { get; set; }

        public Order Order { get; set; } = Order.Desc;

        private WordViewModel Copy()
        {
            return new WordViewModel() { Keyword = Keyword, Page = Page, Order = Order, Column = Column };
        }

        public WordViewModel CopyByPage(int page)
        {
            var copied = Copy();
            copied.Page = page;
            return copied;
        }

        public WordViewModel CopyByOrder(string column)
        {
            var copied = Copy();
            if (copied.Column == column)
            {
                if (copied.Order == Order.Asc) copied.Order = Order.Desc;
                else copied.Order = Order.Asc;
            }
            else
            {
                copied.Order = Order.Desc;
                copied.Column = column;
            }
            return copied;
        }
    }
}
