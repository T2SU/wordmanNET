﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using wordman.Models;
using wordman.SQLite;
using wordman.Utils;

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
        public static async Task<List<TRetType>> LoadWords<TInType, TRetType>(WordContext ctx, ListState state,
                Func<TInType, TRetType> selector,
                WordViewModel model,
                int limit = PageUtils.LimitPerPage,
                params Func<TInType, bool>[] filters)
            where TInType : class where TRetType : class
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (limit <= 0 || limit > 2000) throw new ArgumentOutOfRangeException(nameof(limit));

            // Generate a query dynamically.

            string queryOrder = "";
            switch (model.Order)
            {
                case Order.Asc:
                    queryOrder = "OrderBy";
                    break;
                case Order.Desc:
                    queryOrder = "OrderByDescending";
                    break;
                default:
                    break;
            }

            IEnumerable<TInType> query = ctx.Set<TInType>();

            if (!string.IsNullOrEmpty(queryOrder))
            {
                var inType = typeof(TInType);
                PropertyInfo pInfo;

                var param = Expression.Parameter(typeof(TInType), "w");
                LambdaExpression keyExpr;
                if (model.Column == null)
                {
                    keyExpr = Expression.Lambda(Expression.PropertyOrField(param, nameof(Word.WordID)), param);
                    pInfo = inType.GetProperty(nameof(Word.WordID));
                }
                else
                {
                    keyExpr = Expression.Lambda(Expression.PropertyOrField(param, model.Column), param);
                    pInfo = inType.GetProperty(model.Column);
                }

                var queryableType = typeof(Queryable);
                var method = queryableType.GetMethods()
                 .Where(m => m.Name == queryOrder && m.IsGenericMethodDefinition)
                 .Where(m => m.GetParameters().Count() == 2)
                 .Single();

                MethodInfo order = method.MakeGenericMethod(typeof(TInType), pInfo.PropertyType);
                query = (IOrderedQueryable<TInType>)order.Invoke(order, new object[] { query, keyExpr });
            }

            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }

            state.TotalCount = query.Count();

            query = query.Skip((model.Page - 1) * limit);
            query = query.Take(limit);

            return await query
                .ToAsyncEnumerable()
                .Select(selector)
                .ToList();
        }

        public static async Task<List<string>> GetRelatedWords(WordContext ctx, RelatedType type, string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (!Enum.IsDefined(typeof(RelatedType), type) || type == RelatedType.None) throw new ArgumentException(nameof(type));
            var word_data = await ctx.Words
                .Where(w => w.Content == word)
                .Include(w => w.RelatedWords)
                .ToAsyncEnumerable()
                .SingleOrDefault();
            return await word_data.RelatedWords
                .Where(rw => rw.Type == type)
                .Join(ctx.Words, a => a.RelatedWordID, i => i.WordID, (a, i) => i.Content)
                .ToAsyncEnumerable()
                .ToList();
        }

        public static async Task<List<string>> GetRelatedStrings(WordContext ctx, RelatedType type, string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (!Enum.IsDefined(typeof(RelatedType), type) || type == RelatedType.None) throw new ArgumentException(nameof(type));
            var word_data = await ctx.Words
                .Where(w => w.Content == word)
                .Include(w => w.RelatedStrings)
                .ToAsyncEnumerable()
                .SingleOrDefault();
            return await word_data.RelatedStrings
                .Where(rs => rs.Type == type)
                .Select(rs => rs.Content)
                .ToAsyncEnumerable()
                .ToList();
        }

        /// <summary>
        /// Returns the value of a single word, including antonyms, synonyms, and examples.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="F">Type</typeparam>
        /// <param name="word">word</param>
        /// <param name="getter">getter</param>
        /// <returns>Asynchronous operation to get word data</returns>
        private static async Task<T> GetWordDetail<T, F>(WordContext ctx, string word, Func<Word, T> getter, Expression<Func<Word, ICollection<F>>> includer)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            var word_data_query = ctx.Words
                .Where(w => w.Content == word);
            if (includer != null)
            {
                word_data_query = word_data_query.Include(includer);
            }
            var word_data = await word_data_query.ToAsyncEnumerable()
                .SingleOrDefault();
            if (word_data == default(Word))
            {
                return default;
            }
            return getter(word_data);
        }

        public static async Task<ChangeMod> ChangeRelatedString(WordContext ctx, RelatedType type, string word, string old_string, string new_string)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (!Enum.IsDefined(typeof(RelatedType), type) || type == RelatedType.None) throw new ArgumentException(nameof(type));

            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                ChangeMod mod = 0;

                var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.RelatedStrings }, w => w.RelatedStrings);
                if (target_word != null)
                {
                    if (old_string != null)
                    {
                        var existing_example = await target_word.RelatedStrings
                            .Where(e => e.Content == old_string)
                            .Where(e => e.Type == type)
                            .ToAsyncEnumerable()
                            .SingleOrDefault();
                        if (existing_example != null)
                        {
                            target_word.RelatedStrings.Remove(existing_example);
                            await ctx.SaveChangesAsync();
                            mod |= ChangeMod.Del;
                        }
                    }
                    if (new_string != null)
                    {
                        var created_example = new RelatedString() { Type = type, WordID = target_word.WordID, Content = new_string };
                        target_word.RelatedStrings.Add(created_example);
                        await ctx.SaveChangesAsync();
                        mod |= ChangeMod.New;
                    }

                    t.Commit();
                }

                return mod;
            }
        }

        public static async Task<ChangeMod> ChangeRelatedWord(WordContext ctx, RelatedType type, string word, string old_one, string new_one)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            if (!Enum.IsDefined(typeof(RelatedType), type) || type == RelatedType.None) throw new ArgumentException(nameof(type));

            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                ChangeMod mod = 0;

                var target_word = await GetWordDetail(ctx, word, a => new { a.WordID, a.RelatedWords }, w => w.RelatedWords);
                if (target_word != null)
                {
                    if (old_one != null)
                    {
                        var existing_related_word = await target_word.RelatedWords
                            .Where(rw => rw.Type == type)
                            .Join(ctx.Words, s => s.RelatedWordID, i => i.WordID, (s, i) => new { RelatedWord = s, i.Content, Word = i })
                            .Where(e => e.Content == old_one)
                            .ToAsyncEnumerable()
                            .SingleOrDefault();

                        if (existing_related_word != null)
                        {
                            target_word.RelatedWords.Remove(existing_related_word.RelatedWord);

                            await ctx.Entry(existing_related_word.Word)
                                .Collection(w => w.RelatedWords)
                                .LoadAsync();
                            var related = existing_related_word.Word;
                            var related_target_word = await related.RelatedWords
                                .Where(a => a.Type == type && a.RelatedWordID == target_word.WordID)
                                .ToAsyncEnumerable()
                                .SingleOrDefault();
                            if (related_target_word != null)
                            {
                                related.RelatedWords.Remove(related_target_word);
                            }
                            mod |= ChangeMod.Del;
                        }

                        await ctx.SaveChangesAsync();
                    }
                    if (new_one != null)
                    {
                        var related_word = await GetWordDetail(ctx, new_one, a => a, w => w.RelatedWords);
                        if (related_word == null)
                        {
                            related_word = await NewWord(ctx, new_one);
                        }
                        var created_related_word = new RelatedWord() { Type = type, WordID = target_word.WordID, RelatedWordID = related_word.WordID };
                        target_word.RelatedWords.Add(created_related_word);
                        await ctx.SaveChangesAsync();

                        var created_related_word_pair = new RelatedWord() { Type = type, WordID = related_word.WordID, RelatedWordID = target_word.WordID };
                        related_word.RelatedWords.Add(created_related_word_pair);
                        await ctx.SaveChangesAsync();

                        mod |= ChangeMod.New;
                    }

                    t.Commit();
                }

                return mod;
            }
        }

        public static async Task<Word> UpdateRef(WordContext ctx, string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            using (var t = await ctx.Database.BeginTransactionAsync())
            {
                var target_word = await GetWordDetail(ctx, 
                    word, 
                    a => a, 
                    (Expression<Func<Word,ICollection<object>>>)null);
                if (target_word != null)
                {
                    target_word.Referenced++;
                    target_word.LastReferenced = DateTime.Now;
                    await ctx.SaveChangesAsync();
                    t.Commit();
                    return target_word;
                }
                return null;
            }
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
