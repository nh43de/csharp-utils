using System.Runtime.InteropServices;

namespace BotFramework.Either
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Union<TLeft, TRight>
    {
        [FieldOffset(0)] public TLeft Left;
        [FieldOffset(0)] public TRight Right;
    }
}