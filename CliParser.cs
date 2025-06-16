using System.Diagnostics.CodeAnalysis;

namespace Utils;

public abstract record ParseResult
{
    public static string Describe<T>(T type)
        where T: Type
    {
        return type.Name;
    }
}

public sealed record Flag(string Name) : ParseResult
{
    public override string ToString() => $"flag '{Name}'";
}

public sealed record ArgumentString(string Name, string Value) : ParseResult
{
    public override string ToString() => $"string '{Name} = {Value}'";
}

public sealed record ArgumentList(string Name, List<string> Values) : ParseResult
{
    public override string ToString() => $"list '{Name} = [{string.Join(", ", Values)}]'";
}

public sealed record ArgumentInt(string Name, int Value) : ParseResult
{
    public override string ToString() => $"int '{Name} = {Value}'";
}

public sealed record Raw(string Name) : ParseResult
{
    public override string ToString() => $"'{Name}'";
}


public abstract record Maybe
{
    public static MaybeArgument Argument(
        string? longName = null,
        string? shortName = null,
        ArgumentKind kind = ArgumentKind.String,
        string description = "Arg"
    ) => new(longName, shortName, kind, description);
    
    public static MaybeFlag Flag(string? name = null, string description = "Flag") => new(name, description);
    public static MaybeRaw Raw(string? name = null, string description = "Raw") => new(name, description);

    public abstract bool Is(ParseResult? other);
}

public enum ArgumentKind
{
    String,
    Integer,
    List
}

public sealed record MaybeArgument(
    string? LongName, 
    string? ShortName,
    ArgumentKind ArgumentKind,
    string Description) : Maybe
{
    public override string ToString() => LongName is not null
        ? ShortName is not null
            ? $"<--{LongName}:-{ShortName}:{ArgumentKind}:{Description}>"
            : $"<--{LongName}:{ArgumentKind}:{Description}>"
        : ShortName is not null
            ? $"<-{ShortName}:{ArgumentKind}:{Description}>"
            : $"<{ArgumentKind}:{Description}>";

    public override bool Is(ParseResult? other)
    {
        if (other is ArgumentString argString)
        {
            return ArgumentKind is ArgumentKind.String
               && ((LongName is null && ShortName is null)
                   || ShortName == argString.Name 
                   || LongName == argString.Name);
        }

        if (other is ArgumentList argList)
        {
            return ArgumentKind == ArgumentKind.List 
               && ((LongName is null && ShortName is null)
                   || ShortName == argList.Name 
                   || LongName == argList.Name);
        }

        if (other is ArgumentInt argInt)
        {
            return ArgumentKind == ArgumentKind.Integer 
               && ((LongName is null && ShortName is null)
                   || ShortName == argInt.Name 
                   || LongName == argInt.Name);
        }

        return ArgumentKind is ArgumentKind.String && other is Raw;
    }
}

public sealed record MaybeFlag(string? Name, string Description) : Maybe
{
    public override string ToString() => $"<--{Name}:{Description}>";

    public override bool Is(ParseResult? other)
    {
        return other is Flag flag && (Name is null || Name == flag.Name);
    }
}

public sealed record MaybeRaw(string? Name, string Description) : Maybe
{
    public override string ToString() => $"<{Name}:{Description}>";

    public override bool Is(ParseResult? other)
    {
        return other is Raw cmd && (Name is null || Name == cmd.Name);
    }
}

public sealed class PossibilitySet(IEnumerable<Maybe> maybes)
{
    public List<Maybe> Possibilities { get; init; } = maybes.ToList();

    public bool Has(ParseResult? result)
    {
        return Possibilities.Any(maybe => maybe.Is(result));
    }
}

public sealed class Parser(string[] args, string? usage = null)
{
    private string[] Arguments { get; init; } = args;
    private PossibilitySet Persistent { get; init; } = new([]);
    private List<int> ParsedArgs { get; init; } = [];
    private string? Usage { get; init; } = usage;
    private int Index { get; set; } = 0;

    private string? Peek()
    {
        if (Index >= Arguments.Length-1) return null;
        return Arguments[Index + 1];
    }

    private string? TryNext()
    {
        while (ParsedArgs.Contains(Index + 1))
        {
            Index++;
        }

        if (Index >= Arguments.Length) return null;
        return Arguments[Index++];
    }

    private void Back() => Index--;

    private static void Assert([DoesNotReturnIf(false)] bool condition, string? message)
    {
        if (!condition)
        {
            (message ?? "assertion").Log(Ansi.WrapRed("error"));
            Environment.Exit(1);
        }
    }

    private ParseResult? Next()
    {
        if (TryNext() is { } next)
        {
            if (next.StartsWith("--") && next.Length > 2)
            {
                return new Flag(next.TrimStart('-').Trim());
            }

            if (next.StartsWith('-') && next.Length > 1)
            {
                var name = next.TrimStart('-');
                var value = TryNext();
                Assert(value is not null, $"expected value for argument {Ansi.WrapYellow(name)}");

                var peeked = Peek();
                if (value.EndsWith(',') || peeked != null && peeked.StartsWith(','))
                {
                    var values = new List<string> { value.Trim(',') };
                    var val = TryNext();
                    while (val is not null)
                    {
                        values.Add(val.Trim(','));
                        if (!val.EndsWith(',') && peeked is null)
                        {
                            break;
                        }

                        if (peeked is not null)
                        {
                            var maybeNext = TryNext();
                            if (maybeNext != null && !string.IsNullOrEmpty(maybeNext.Trim(',').Trim()))
                            {
                                values.Add(maybeNext.Trim(',').Trim());
                            }
                        }

                        val = TryNext();
                    }

                    values.PrintEach("Value");
                    return new ArgumentList(name, values);
                }

                if (int.TryParse(value.Trim().Trim('"'), out var intVal))
                {
                    return new ArgumentInt(name, intVal);
                }

                return new ArgumentString(name, value.Trim().Trim('"'));
            }

            return new Raw(next);
        }

        return null;
    }

    public ParseResult? Any()
    {
        return Next();
    }

    /// <summary>
    /// Checks if the next argument matches one of the provided possibilities.
    ///
    /// If none is found, the parser provides an error message and exits the entire program.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <returns></returns>
    public ParseResult Expect(params Maybe[] possibilities)
    {
        var set = new PossibilitySet(possibilities);
        var next = Next();
        ParseResult? result = next switch
        {
            Flag flag when set.Has(flag) => flag,
            ArgumentString arg when set.Has(arg) => arg,
            ArgumentList list when set.Has(list) => list,
            ArgumentInt intArg when set.Has(intArg) => intArg,
            Raw cmd when set.Has(cmd) => cmd,
            _ => null,
        };
        if (result is null)
        {
            if (next is not null)
            {
                $"invalid {next}".Log(Ansi.WrapRed("error"));
            }

            $"one of\n{"- ",11}{string.Join($"{"- ",11}", possibilities.Select(m => m + "\n"))}".Log(Ansi.WrapRed("expected"));
            Usage?.Log(Ansi.WrapRed("error"));
            Environment.Exit(1);
        }

        return result;
    }

    /// <summary>
    /// As the name suggests, Expect checks the next argument
    /// and tries to convert it to the provided ParseResult type.
    ///
    /// It uses the dynamic Expect under the hood, which fails
    /// and exits if the next argument does not match the expected one.
    ///
    /// As mentioned above, this expect adds a type conversion operation.
    /// So instead of this
    ///
    /// <code>var cmd = (Cmd) Parser.Expect(Maybe.Raw("run"));</code>
    ///
    /// we can do
    ///
    /// <code>var cmd = Parser.Expect&lt;Cmd&gt;(Maybe.Raw("run"));</code>
    /// 
    /// If that operation fails, it is because programmer tried to convert the
    /// result to an invalid one. The only difference between the two is, that
    /// the second one provides a cleaner api that allows for easier chaining
    /// without parenthesis hell like this
    ///
    /// <code>
    /// (Parser.Expect(Maybe.Argument("name")) as ArgumentString).Value
    /// or
    /// ((ArgumentString)Parser.Expect(Maybe.Argument("name")))!.Value
    /// </code>
    ///
    /// vs
    /// 
    /// <code>Parser.Expect&lt;ArgumentString&gt;(Maybe.Argument("name")).Value</code>
    /// </summary>
    /// <param name="possibilities"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Expect<T>(params Maybe[] possibilities)
        where T: ParseResult
    {
        var result = Expect(possibilities);
        if (result is not T)
        {
            "faulty parser, tried parsing an possibility into invalid type.".Log(Ansi.WrapRed("error"));
            $"{typeof(T)}, {Ansi.WrapRed("found")} {result.GetType().Name}".Log(Ansi.WrapRed("expected"));
            Environment.Exit(1);
        }
    
        return (result as T)!;
    }

    /// <summary>
    /// Checks if the next argument is of one of the provided possibilities.
    ///
    /// If not, it backtracks and returns null.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <returns></returns>
    public ParseResult? Optional(params Maybe[] possibilities)
    {
        var set = new PossibilitySet(possibilities);
        ParseResult? result = Next() switch
        {
            Flag flag when set.Has(flag) => flag,
            ArgumentString arg when set.Has(arg) => arg,
            ArgumentList list when set.Has(list) => list,
            ArgumentInt intArg when set.Has(intArg) => intArg,
            Raw cmd when set.Has(cmd) => cmd,
            _ => null,
        };
        if (result is null) Back();
        return result;
    }

    /// <summary>
    /// Checks if the *rest* of the arguments contain the optional.
    ///
    /// This means it checks from the current position onwards, so
    /// if the argument searched for appears at a position before,
    /// it will not be found.
    ///
    /// This allows for nesting commands with arguments that can be set
    /// regardless of their position, which makes commands more flexible.
    ///
    /// For example, if a program has a number of flags that can
    /// be set at any point, we can simply check for them in the beginning,
    /// which marks them as parsed and prevents them from being double parsed.
    /// </summary>
    /// <param name="maybe"></param>
    /// <returns></returns>
    public ParseResult? PersistentOptional(Maybe maybe)
    {
        var checkpoint = Index;
        // Index = 0;
        var result = null as ParseResult;
        var startIndex = Index;

        while (Next() is { } someArg)
        {
            result = someArg switch
            {
                Flag flag when maybe.Is(flag) => flag,
                ArgumentString arg when maybe.Is(arg) => arg,
                ArgumentList list when maybe.Is(list) => list,
                ArgumentInt intArg when maybe.Is(intArg) => intArg,
                Raw cmd when maybe.Is(cmd) => cmd,
                _ => null,
            };

            if (result is not null)
            {
                foreach (var index in (startIndex + 1).ToInc(Index))
                {
                    ParsedArgs.Add(index);
                }

                break;
            }

            startIndex = Index;
        }

        Index = checkpoint;
        return result;
    }

    public string? Command(params string[] names)
    {
        if (TryNext() is { } possibleCmd && names.Contains(possibleCmd))
        {
            return possibleCmd;
        }

        Back();
        return null;
    }

    public string? Argument(string? longName = null, string? shortName = null)
    {
        var checkpoint = Index;
        var next = TryNext();
        if (next is { } possibleArg && (possibleArg.StartsWith('-') || (longName == null && shortName == null)))
        {
            var name = possibleArg.TrimStart('-');
            var value = TryNext();
            Assert(value is not null, $"expected value for argument {Ansi.WrapYellow(name)}");
            if (name == longName || name == shortName)
            {
                return value;
            }
        }

        Index = checkpoint;
        return null;
    }

    public List<string>? ArgumentList(string longName, string? shortName)
    {
        var checkpoint = Index;
        if (TryNext() is { } possibleArg && possibleArg.StartsWith('-'))
        {
            var name = possibleArg.TrimStart('-');
            var value = TryNext();
            Assert(value is not null, $"expected value for argument {Ansi.WrapYellow(name)}");

            if (value.EndsWith(','))
            {
                var values = new List<string>();
                var val = TryNext();
                while (val is not null && val.EndsWith(','))
                {
                    values.Add(val);
                    val = TryNext();
                }

                if (name == longName || name == shortName)
                {
                    return values;
                }
            }

            if (name == longName || name == shortName)
            {
                return [value.Trim().Trim('"')];
            }
        }

        Index = checkpoint;
        return null;
    }

    public bool Flag(string name)
    {
        if (TryNext() is { } possibleFlag 
            && possibleFlag.StartsWith("--") 
            && possibleFlag[2..] == name)
        {
            return true;
        }

        Back();
        return false;
    }
}