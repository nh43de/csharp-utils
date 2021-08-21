using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataPowerTools
{
    /// <summary>
    ///     Tries to get things by repeating the action until successful, or number of tries has been met.
    /// </summary>
    public static class TrySimple
    {
        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            Func<Exception, T> replacement)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return replacement(ex);
            }
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            T replacement)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return replacement;
            }
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(
            this Func<Task<T>> action,
            Func<Exception, T> replacement)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                return replacement(ex);
            }
        }
        
        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(
            this Func<CancellationToken, Task<T>> action,
            Func<Exception, T> replacement,
            CancellationToken token = default(CancellationToken))
        {
            try
            {
                return await action(token);
            }
            catch (Exception ex)
            {
                return replacement(ex);
            }
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(
            this Func<CancellationToken, Task<T>> action,
            T replacement,
            CancellationToken token = default(CancellationToken))
        {
            try
            {
                return await action(token);
            }
            catch (Exception ex)
            {
                return replacement;
            }
        }
    }
}