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
        public static async Task<Word> NewWord(WordContext ctx, string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(word);

            var new_word = new Word() { Content = word, LastReferenced = DateTime.Now };
            ctx.Words.Add(new_word);
            await ctx.SaveChangesAsync();

            return new_word;
        }

        public static async Task<int> NewWordAndID(WordContext ctx, string word)
        {
            return (await NewWord(ctx, word)).WordID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Word>> LoadWordsUsingDefault(WordContext ctx, int page)
        {
            return await LoadWords<Word, int, Word>(ctx, w => w.WordID, w => w.Compact(), Order.Desc, page);
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
        public static async Task<List<TRetType>> LoadWords<TInType, TOrderingType, TRetType>(WordContext ctx, 
            Func<TInType, TOrderingType> keyExpr,
                Func<TInType, TRetType> selector,
                Order order,
                int page,
                int limit = 50)
            where TInType : class where TRetType : class
        {
            if (keyExpr == null) throw new ArgumentNullException(nameof(keyExpr));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (limit <= 0 || limit > 2000) throw new ArgumentOutOfRangeException(nameof(limit));

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

        public static async Task<List<RelatedWord>> GetRelatedWords(WordContext ctx, RelatedWordType type, string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            var word_data = await ctx.Words
                .Where(w => w.Content == word)
                .ToAsyncEnumerable()
                .SingleOrDefault();
            return await word_data.RelatedWords
                .Where(rw => rw.Type == type)
                .ToAsyncEnumerable()
                .ToList();
        }

        public static async Task<List<RelatedString>> GetRelatedStrings(WordContext ctx, RelatedStringType type, string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            var word_data = await ctx.Words
                .Where(w => w.Content == word)
                .ToAsyncEnumerable()
                .SingleOrDefault();
            return await word_data.RelatedStrings
                .Where(rw => rw.Type == type)
                .ToAsyncEnumerable()
                .ToList();
        }
        
        /// <summary>
         /// Returns the value of a single word, including antonyms, synonyms, and examples.
         /// </summary>
         /// <typeparam name="T">Type</typeparam>
         /// <param name="word">word</param>
         /// <param name="getter">getter</param>
         /// <returns>Asynchronous operation to get word data</returns>
        public static async Task<T> GetWordDetail<T>(WordContext ctx, string word, Func<Word, T> getter)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            var word_data = await ctx.Words
                .Where(w => w.Content == word)
                .ToAsyncEnumerable()
                .SingleOrDefault();
            return getter(word_data);
        }

        public static async Task<ChangeMod> ChangeRelatedString(WordContext ctx, RelatedStringType type, string word, string old_string, string new_string)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(new_string)) throw new ArgumentNullException(nameof(new_string));

            ChangeMod mod = 0;

            var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.RelatedStrings });
            if (old_string != null)
            {
                var existing_example = await target_word.RelatedStrings
                    .Where(e => e.Content == old_string)
                    .Where(e => e.Type == type)
                    .ToAsyncEnumerable()
                    .SingleOrDefault();
                target_word.RelatedStrings.Remove(existing_example);
                await ctx.SaveChangesAsync();
                mod |= ChangeMod.Del;
            }
            if (new_string != null)
            {
                var created_example = new RelatedString() { Type = type, WordID = target_word.WordID, Content = new_string };
                target_word.RelatedStrings.Add(created_example);
                await ctx.SaveChangesAsync();
                mod |= ChangeMod.Add;
            }

            return mod;
        }

        public static async Task<ChangeMod> ChangeRelatedWord(WordContext ctx, RelatedWordType type, string word, string old_one, string new_one)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(new_one)) throw new ArgumentNullException(nameof(new_one));

            ChangeMod mod = 0;

            var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.RelatedWords });
            if (old_one != null)
            {
                var existing_words = await target_word.RelatedWords
                    .Where(rw => rw.Type == type)
                    .Join(ctx.Words, s => s.RelatedWordID, i => i.WordID, (s, i) => new { RelatedWord = s, i.Content })
                    .Where(e => e.Content == old_one)
                    .ToAsyncEnumerable()
                    .SingleOrDefault();
                target_word.RelatedWords.Remove(existing_words.RelatedWord);
                await ctx.SaveChangesAsync();
                mod |= ChangeMod.Del;
            }
            if (new_one != null)
            {
                int relatedWordID = await GetWordID(ctx, new_one);
                if (relatedWordID == 0)
                {
                    relatedWordID = await NewWordAndID(ctx, new_one);
                }
                var created_related_word = new RelatedWord() { Type = type, WordID = target_word.WordID, RelatedWordID = relatedWordID };
                target_word.RelatedWords.Add(created_related_word);
                await ctx.SaveChangesAsync();
                mod |= ChangeMod.Add;
            }

            return mod;
        }

        private static async Task<int> GetWordID(WordContext ctx, string word)
        {
            return await ctx.Words
                .Where(a => a.Content == word)
                .ToAsyncEnumerable()
                .Select(a => a.WordID)
                .SingleOrDefault();
        }
    }
}
