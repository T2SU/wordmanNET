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
                .Select(getter)
                .SingleOrDefault();
            return word_data;
        }

        /// <summary>
        /// Change the value of a word stored in the database. The old_one value cannot be null. If the modifier value is null, the word will be deleted. On success, the string 'mod' or 'del' is returned.
        /// </summary>
        /// <param name="word">Word. This value cannot be null or empty.</param>
        /// <param name="modifier">An expression to change the data of a word. If this value is null, the word will be deleted.</param>
        /// <returns>Asynchronous operation to change word. On success, the string 'mod' or 'del' is returned.</returns>
        public static async Task<string> ChangeDetail(WordContext ctx, string word, Action<Word> modifier)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
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
            return ret;
        }

        public static async Task<Example> NewExample(WordContext ctx, string word, string sentence)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(sentence)) throw new ArgumentNullException(nameof(sentence));

            var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.Examples });
            var new_example = new Example() { WordID = target_word.WordID, Sentence = sentence };
            target_word.Examples.Add(new_example);
            await ctx.SaveChangesAsync();

            return new_example;
        }

        public static async Task<Antonym> NewAntonym(WordContext ctx, string word, string antonym)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(antonym)) throw new ArgumentNullException(nameof(antonym));

            var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.Antonyms });
            int antonymWordID = await GetWordID(ctx, antonym);
            if (antonymWordID == 0)
            {
                var newWord = await NewWord(ctx, antonym);
                antonymWordID = newWord.WordID;
            }
            var new_antonym = new Antonym() { WordID = target_word.WordID, AntonymWordID = antonymWordID };
            target_word.Antonyms.Add(new_antonym);
            await ctx.SaveChangesAsync();

            return new_antonym;
        }

        public static async Task<Synonym> NewSynonym(WordContext ctx, string word, string synonym)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (string.IsNullOrWhiteSpace(synonym)) throw new ArgumentNullException(nameof(synonym));

            var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.Synonyms });
            int synonymWordID = await GetWordID(ctx, synonym);
            if (synonymWordID == 0)
            {
                var newWord = await NewWord(ctx, synonym);
                synonymWordID = newWord.WordID;
            }
            var new_synonym = new Synonym() { WordID = target_word.WordID, SynonymWordID = synonymWordID };
            target_word.Synonyms.Add(new_synonym);
            await ctx.SaveChangesAsync();

            return new_synonym;
        }

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
