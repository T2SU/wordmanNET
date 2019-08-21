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
            ViewBag.AppVer = App.Version;
            ViewBag.Data = await WordManipulator.LoadWordsUsingDefault(page);
            return View();
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add(string word)
        {
            try
            {
                var new_word = await WordManipulator.NewWord(word);
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

        [HttpPost]
        [Route("get_detail")]
        public async Task<IActionResult> GetDetail(string word, string detail_type)
        {
            switch (detail_type)
            {
                case "example":
                    return new JsonResult(await WordManipulator.GetWordDetail(word, w => w.Examples));
                case "synonym":
                    return new JsonResult(await WordManipulator.GetWordDetail(word, w => w.Synonyms));
                case "antonym":
                    return new JsonResult(await WordManipulator.GetWordDetail(word, w => w.Antonyms));
                default:
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
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