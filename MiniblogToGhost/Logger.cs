﻿namespace MiniblogToGhost
{
    using System;

    public static class Logger
    {
        public static void Log(string format, params object[] args) => Console.WriteLine(format, args);
    }
}
