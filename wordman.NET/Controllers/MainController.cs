using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wordman.Models;
using wordman.Mvc;
using wordman.SQLite;
using wordman.Utils;
using wordman.Words;

namespace wordman.Controllers
{
    public class MainController : Controller
    {
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(WordViewModel model)
        {
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                var state = new ListState();
                var filters = new List<Func<Word, bool>>();

                ViewBag.AppVer = App.Version;

                if (model.Keyword != null)
                {
                    filters.Add(w => w.Content.Contains(model.Keyword));
                }
                ViewBag.Data = await WordManipulator.LoadWords(ctx, state,
                    w => w.WordID,
                    w => w.Compact(),
                    model,
                    PageUtils.LimitPerPage,
                    filters.ToArray());
                ViewBag.State = state;

                return View(model);
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
                    t.Commit();
                    return new WordResult(new_word);
                }
                catch (DbUpdateException)
                {
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
        }

        [HttpPost]
        [Route("get_detail")]
        public async Task<IActionResult> GetDetail(string word, RelatedType detail_type)
        {
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                switch (detail_type)
                {
                    case RelatedType.Example:
                        return new JsonResult(await WordManipulator.GetRelatedStrings(ctx, RelatedType.Example, word));
                    case RelatedType.Synonym:
                        return new JsonResult(await WordManipulator.GetRelatedWords(ctx, RelatedType.Synonym, word));
                    case RelatedType.Antonym:
                        return new JsonResult(await WordManipulator.GetRelatedWords(ctx, RelatedType.Antonym, word));
                    default:
                        return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
        }

        [HttpPost]
        [Route("change_detail")]
        public async Task<IActionResult> ChangeDetail(string word, RelatedType detail_type, string oldone, string newone)
        {
            using (var ctx = new WordContext())
            {
                switch (detail_type)
                {
                    case RelatedType.Example:
                        return new EnumResult(await WordManipulator.ChangeRelatedString(ctx, RelatedType.Example, word, oldone, newone));
                    case RelatedType.Synonym:
                        return new EnumResult(await WordManipulator.ChangeRelatedWord(ctx, RelatedType.Synonym, word, oldone, newone));
                    case RelatedType.Antonym:
                        return new EnumResult(await WordManipulator.ChangeRelatedWord(ctx, RelatedType.Antonym, word, oldone, newone));
                    default:
                        return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
        }

        [HttpPost]
        [Route("update_ref")]
        public async Task<IActionResult> UpdateRef(string word)
        {
            using (var ctx = new WordContext())
            {
                return new WordResult(await WordManipulator.UpdateRef(ctx, word));
            }
        }
    }
}