﻿using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Utils;

public static class IoExtensions
{
    private const int INDENT = 25;

    [Pure] public static bool WriteTo(this string text, string path)
    {
        using (var writer = new StreamWriter(path))
        {
            return text.WriteTo(writer);
        }
    }

    [Pure] public static bool WriteTo(this string text, TextWriter writer)
    {
        try
        {
            writer.Write(text);
            return true;
        }
        catch (ObjectDisposedException)
        {
            "output stream closed".LogErr();
            return false;
        }
        catch (IOException)
        {
            "io exception occured".LogErr();
            return false;
        }
    }

    [Pure]
    public static string ReadToEnd(this string path)
    {
        try
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception)
        {
            $"failed reading file: {Path.GetFullPath(path)}".LogErr();
            throw;
        }
    }

    public static void LogStep(this object o, string step)
    {
        Console.Write($"{Ansi.WrapBlue(step),INDENT}");
        Console.WriteLine(" " + o);
    
        Console.ResetColor();
    }

    public static void LogErr(this object o)
    {
        o.Log(Ansi.WrapRed("error"));
    }

    public static void Log(this object o, params object[] args)
    {
        foreach (var arg in args) Console.Write($"{arg,INDENT}");
        Console.WriteLine(" " + o);
    
        Console.ResetColor();
    }

    public static void LogEach<T>(this IEnumerable<T> items, params object[] args)
    {
        foreach (var item in items)
        {
            item?.Log(args);
        }
    }

    public static void LogEach<T>(this IEnumerator<T> items, params object[] args)
    {
        while (items.MoveNext())
        {
            items.Current?.Log(args);
        }
    }    
    
    public static void PrintNextLine(this object o, params object[] args)
    {
        var caller = new CallerInfo(new StackFrame(1, true));
        Console.Write(caller.ToString());
        foreach (var arg in args) Console.Write("[" + arg + "]");
        Console.WriteLine(Environment.NewLine + o);

        Console.ResetColor();
    }

    public static void PrintNextLine(this IEnumerable<object> items, params object[] args)
    {
        var caller = new CallerInfo(new StackFrame(1, true));
        foreach (var item in items)
        {
            Console.Write(caller.ToString());
            foreach (var arg in args) Console.Write("[" + arg + "]");
            Console.WriteLine(Environment.NewLine + item);
        }

        Console.ResetColor();
    }

    public static void Print(this object o, params object[] args)
    {
        var caller = new CallerInfo(new StackFrame(1, true));
        Console.Write(caller.ToString());
        foreach (var arg in args) Console.Write("[" + arg + "]");
        Console.WriteLine(" " + o);
    
        Console.ResetColor();
    }

    public static void PrintEach<T>(this IEnumerable<T> items, params object[] args)
    {
        var caller = new CallerInfo(new StackFrame(1, true));
        foreach (var item in items)
        {
            Console.Write(caller.ToString());
            foreach (var arg in args) Console.Write("[" + arg + "]");
            Console.WriteLine(" " + item);
        }

        Console.ResetColor();
    }

    private sealed record CallerInfo(int Line, int Col, string Name, string File)
    {
        public CallerInfo(StackFrame frame) : this(
            frame.GetFileLineNumber(),
            frame.GetFileColumnNumber(),
            frame.GetMethod()!.Name.Normalize(), "")
        {
            var fileParts = frame.GetFileName()!.Split('/').ToList();
            File = string.Join('/', fileParts.Count > 3 ? fileParts[^3..] : fileParts);
        }

        public override string ToString() => $"[{File}:{Line}:{Col}] {Name} ";
    }
}