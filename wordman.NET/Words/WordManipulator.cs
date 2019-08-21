using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using wordman.SQLite;

namespace wordman.Words
{
    public static class WordManipulator
    {
        /// <summary>
        /// Add a new word entry to the database.
        /// </summary>
        /// <param name="word">A word to add</param>
        /// <returns>Asynchronous operation to add a word</returns>
        public static async Task<Word> NewWord(string word, WordContext ctx = null, IDbContextTransaction t = null)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(word);

            bool d = false;
            if (ctx == null && t == null)
            {
                d = true;
                ctx = new WordContext();
                t = await ctx.Database.BeginTransactionAsync();
            }
            try
            {
                var new_word = new Word() { Content = word, LastReferenced = DateTime.Now };
                ctx.Words.Add(new_word);
                await ctx.SaveChangesAsync();
                t.Commit();
                return new_word;
            }
            finally
            {
                if (d)
                {
                    try
                    {
                        t.Dispose();
                    }
                    catch { }
                    try
                    {
                        ctx.Dispose();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Word>> LoadWordsUsingDefault(int page)
        {
            return await LoadWords<Word, int, Word>(w => w.WordID, w => w.Compact(), Order.Desc, page);
        }

        /// <summary>
        /// Retrieve a word list using the specified criteria.
        /// </summary>
        /// <typeparam name="TInType">An object type mapped to words stored in the database</typeparam>
        /// <typeparam name="TOrderingType">A type of value to use for ordering</typeparam>
        /// <typeparam name="TRetType">The type to be returned by creating a loaded word with selector</typeparam>
        /// <param name="keyExpr">An expression to get the values needed for ordering</param>
        /// <param name="selector">A selector to convert the return values</param>
        /// <param name="order">Sorting order</param>
        /// <param name="page">The page to load. It works by skipping <code>(page - 1) * limit</code>.</param>
        /// <param name="limit">Number of words per page. Default value is 50.</param>
        /// <returns>Asynchronous operation to return converted words</returns>
        public static async Task<List<TRetType>> LoadWords<TInType, TOrderingType, TRetType>(Func<TInType, TOrderingType> keyExpr,
                Func<TInType, TRetType> selector,
                Order order,
                int page,
                int limit = 50)
            where TInType : class where TRetType : class
        {
            if (keyExpr == null) throw new ArgumentNullException(nameof(keyExpr));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (limit <= 0 || limit > 2000) throw new ArgumentOutOfRangeException(nameof(limit));
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                // Generate a query dynamically.

                var param = Expression.Parameter(typeof(TInType), "w");

                IEnumerable<TInType> query = ctx.Set<TInType>();
                switch (order)
                {
                    case Order.Asc:
                        query = query.OrderBy(keyExpr);
                        break;
                    case Order.Desc:
                        query = query.OrderByDescending(keyExpr);
                        break;
                    default:
                        break;
                }
                query = query.Skip((page - 1) * limit);
                query = query.Take(limit);

                return await query
                    .ToAsyncEnumerable()
                    .Select(selector)
                    .ToList();
            }
        }

        /// <summary>
        /// Returns the value of a single word, including antonyms, synonyms, and examples.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="word">word</param>
        /// <param name="getter">getter</param>
        /// <returns>Asynchronous operation to get word data</returns>
        public static async Task<T> GetWordDetail<T>(string word, Func<Word, T> getter)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                var word_data = await ctx.Words
                    .Where(w => w.Content == word)
                    .ToAsyncEnumerable()
                    .SingleOrDefault();
                return getter(word_data);
            }
        }

        /// <summary>
        /// Change the value of a word stored in the database. The old_one value cannot be null. If the modifier value is null, the word will be deleted. On success, the string 'mod' or 'del' is returned.
        /// </summary>
        /// <param name="word">Word. This value cannot be null or empty.</param>
        /// <param name="modifier">An expression to change the data of a word. If this value is null, the word will be deleted.</param>
        /// <returns>Asynchronous operation to change word. On success, the string 'mod' or 'del' is returned.</returns>
        public static async Task<string> ChangeDetail(string word, Action<Word> modifier)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                var w = await ctx.Words
                    .Where(wd => wd.Content == word)
                    .ToAsyncEnumerable()
                    .SingleOrDefault();
                if (word == null)
                {
                    return null;
                }
                string ret;
                if (modifier == null)
                {
                    ctx.Words.Remove(w);
                    ret = "del";
                }
                else
                {
                    modifier(w);
                    ret = "mod";
                }
                await ctx.SaveChangesAsync();
                t.Commit();
                return ret;
            }
        }

        /// <summary>
        /// Insert a new word detail into the database. On success, return 'new' string.
        /// </summary>
        /// <typeparam name="T">Details type</typeparam>
        /// <param name="word">Word to fill in the details</param>
        /// <param name="f">Details object constructor</param>
        /// <returns>On success, return 'new' string.</returns>
        public static async Task<string> NewDetail<T>(string word, Func<T> f) where T : class
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                var sets = ctx.Set<T>();
                var @new = f();
                sets.Add(@new);
                await ctx.SaveChangesAsync();
                t.Commit();
                return "new";
            }
        }

        public static async Task<Example> ProduceExample(string word, string sentence)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(sentence));
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                int wordID = await GetWordID(ctx, word);
                return new Example() { WordID = wordID, Sentence = sentence };
            }
        }

        public static async Task<Antonym> ProduceAntonym(string word, string antonym)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(antonym));
            using (var ctx = new WordContext())
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                int wordID = await GetWordID(ctx, word);
                int antonymWordID = await GetWordID(ctx, antonym);
                if (antonymWordID == 0)
                {
                    var newWord = await NewWord(antonym, ctx, t);
                    antonymWordID = newWord.WordID;
                }
                return new Antonym() { WordID = wordID, AntonymWordID = antonymWordID };
            }
        }

        // 반의어 등록시 A, B 서로 등록시키는게 필요..

        private static async Task<int> GetWordID(WordContext ctx, string word)
        {
            var words = await ctx.Words
                .Where(a => a.Content == word)
                .ToAsyncEnumerable()
                .Select(a => a.WordID)
                .ToList();
            foreach (var wid in words)
            {
                return wid;
            }
            return 0;
        }
    }
}
