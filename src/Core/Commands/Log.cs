﻿using Acklann.GlobN;
using Acklann.Traneleon.Compilation;
using Akka.Event;
using System;

namespace Acklann.Traneleon.Commands
{
    internal static class Log
    {
        internal static string WorkingDirectory = "";
        internal static LogLevel Level = LogLevel.DebugLevel;

        public static void Debug(string message)
        {
            if (Level >= LogLevel.DebugLevel)
            {
                Write(message, ConsoleColor.DarkGray);
            }
        }

        public static void Info(string message)
        {
            if (Level >= LogLevel.InfoLevel)
            {
                Write(message, ConsoleColor.White);
            }
        }

        public static void Warn(string message)
        {
            if (Level >= LogLevel.WarningLevel)
            {
                Write(message, ConsoleColor.Yellow);
            }
        }

        public static void Error(string message)
        {
            if (Level >= LogLevel.ErrorLevel)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
            }
        }

        public static void Item(ICompilierResult value)
        {
            string path = value.OutputFile.GetRelativePath(WorkingDirectory);
            if (value.Succeeded)
            {
                string elapse = TimeSpan.FromTicks(value.ExecutionTime).ToString(@"mm\:ss\.fff");
                Write($"=> generated '{path}' in ({elapse}).", ConsoleColor.White);
            }
            else
            {
                foreach (var error in value.ErrorList)
                {
                    Write($"{error.Message} at {error.File}:{error.Line}", ConsoleColor.Red);
                }
            }
        }

        public static void Write(string message, ConsoleColor fg)
        {
            if (Console.ForegroundColor != fg) Console.ForegroundColor = fg;
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($" {message}");
        }
    }
}