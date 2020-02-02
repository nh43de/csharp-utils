// http://jacksondunstan.com/articles/3349

using System;

namespace BotFramework.Either
{
    public struct Either<TLeft, TRight>
    {
        private bool isLeft;
        private Union<TLeft, TRight> union;

        public Either(TLeft left)
        {
            isLeft = true;
            union.Right = default(TRight);
            union.Left = left;
        }

        public Either(TRight right)
        {
            isLeft = false;
            union.Left = default(TLeft);
            union.Right = right;
        }

        public TLeft Left
        {
            get
            {
                if (isLeft == false)
                {
                    throw new Exception("Either doesn't hold Left");
                }
                return union.Left;
            }
            set
            {
                union.Left = value;
                isLeft = true;
            }
        }

        public TRight Right
        {
            get
            {
                if (isLeft)
                {
                    throw new Exception("Either doesn't hold Right");
                }
                return union.Right;
            }
            set
            {
                union.Right = value;
                isLeft = false;
            }
        }

        public bool IsLeft
        {
            get { return isLeft; }
        }
    }
}