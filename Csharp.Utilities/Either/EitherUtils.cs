using System;

namespace BotFramework.Either
{
    public static class EitherUtils
    {
        public static Func<Either<TA, TC>, Either<TB, TC>>
            Bind<TA, TB, TC>(
                Func<TA, Either<TB, TC>> func
            )
        {
            return e => e.Match(
                a => func(a),
                c => new Either<TB, TC>(c)
            );
        }

        public static Func<T, T> ReturnParam<T>(Action<T> func)
        {
            return arg => {
                func(arg);
                return arg;
            };
        }

        public static Func<TA, Either<TA, TB>> ReturnEitherLeft<TA, TB>(Func<TA, TA> func)
        {
            return arg => new Either<TA, TB>(func(arg));
        }

        public static Func<TA, TB>
            Combine<TA, TB>(
                Func<TA, TB> firstFunc,
                params Func<TB, TB>[] moreFuncs
            )
        {
            return arg =>
            {
                var ret = firstFunc(arg);
                foreach (var func in moreFuncs)
                {
                    ret = func(ret);
                }
                return ret;
            };
        }

        public static Func<TA, Either<TB, TC>> Identity<TA, TB, TC>(Func<TA, Either<TB, TC>> func)
        {
            return func;
        }
    }
}