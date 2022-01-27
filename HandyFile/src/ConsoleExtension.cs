using System;

namespace HandyFile
{
    public static class ConsoleExtension
    {
        public static void Print(string text, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
        }
    }
}