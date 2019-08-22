using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wordman.SQLite;
using wordman.Words;

namespace wordman.Controllers
{
    public class MainController : Controller
    {
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                ViewBag.AppVer = App.Version;
                ViewBag.Data = await WordManipulator.LoadWordsUsingDefault(ctx, page);
                return View();
            }
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add(string word)
        {
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                try
                {
                    var new_word = await WordManipulator.NewWord(ctx, word);
                    return new JsonResult(new
                    {
                        word = new_word.Content,
                        @ref = new_word.Referenced,
                        time = new_word.LastReferenced.ToString()
                    });
                }
                catch (DbUpdateException)
                {
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
        }

        [HttpPost]
        [Route("get_detail")]
        public async Task<IActionResult> GetDetail(string word, string detail_type)
        {
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                switch (detail_type)
                {
                    case "example":
                        return new JsonResult(await WordManipulator.GetWordDetail(ctx, word, w => w.Examples));
                    case "synonym":
                        return new JsonResult(await WordManipulator.GetWordDetail(ctx, word, w => w.Synonyms));
                    case "antonym":
                        return new JsonResult(await WordManipulator.GetWordDetail(ctx, word, w => w.Antonyms));
                    default:
                        return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
        }

        [HttpPost]
        [Route("change_detail")]
        public async Task<IActionResult> ChangeDetail(string word, string detail_type, string oldone, string newone)
        {
            // 뇌정지 ㅜㅜ
        }
    }
}