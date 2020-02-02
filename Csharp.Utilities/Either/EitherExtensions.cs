using System;

namespace BotFramework.Either
{
    public static class EitherExtensions
    {
        public static TResult Match<TLeft, TRight, TResult>(
            this Either<TLeft, TRight> either,
            Func<TLeft, TResult> leftMatcher,
            Func<TRight, TResult> rightMatcher
        )
        {
            if (either.IsLeft)
            {
                return leftMatcher(either.Left);
            }
            else
            {
                return rightMatcher(either.Right);
            }
        }
    }
}