namespace Utils;

public static class Ansi
{
    public static string Bell => "\x07";
    public static string Backspace => "\x08";
    public static string HorizontalTab => "\x09";
    public static string Newline => "\x0A";
    public static string VerticalTab => "\x0B";
    public static string FormFeed => "\x0C";
    public static string CarriageReturn => "\x0D";
    public static string Escape => "\x1B";
    public static string Delete => "\x7F";

    public static string Set(int target) => $"{Escape}[{target}m";
    public static string ResetAll => Set(0);

    public static string WrapBold(string s) => $"{SetBold}{s}{ResetBold}";
    public static string SetBold => Set(1);
    public static string ResetBold => Set(22);

    public static string WrapDim(string s) => $"{SetDim}{s}{ResetDim}";
    public static string SetDim => Set(2);
    public static string ResetDim => Set(22);

    public static string WrapItalic(string s) => $"{SetItalic}{s}{ResetItalic}";
    public static string SetItalic => Set(3);
    public static string ResetItalic => Set(23);
    
    public static string WrapUnderline(string s) => $"{SetUnderline}{s}{ResetUnderline}";
    public static string SetUnderline => Set(4);
    public static string ResetUnderline => Set(24);
    
    public static string WrapBlinking(string s) => $"{SetBlinking}{s}{ResetBlinking}";
    public static string SetBlinking => Set(5);
    public static string ResetBlinking => Set(25);
    
    public static string WrapInverse(string s) => $"{SetInverse}{s}{ResetInverse}";
    public static string SetInverse => Set(6);
    public static string ResetInverse => Set(26);
    
    public static string WrapHidden(string s) => $"{SetHidden}{s}{ResetHidden}";
    public static string SetHidden => Set(7);
    public static string ResetHidden => Set(27);
    
    public static string WrapStrikethrough(string s) => $"{SetStrikethrough}{s}{ResetStrikethrough}";
    public static string SetStrikethrough => Set(7);
    public static string ResetStrikethrough => Set(27);

    
    public static string WrapColor(int color, string s) => $"{Set(color)}{s}{Set(color >= 40 ? Background.Default : Foreground.Default)}";
    public static string WrapForeground(int color, string s) => $"{Set(color)}{s}{Set(Foreground.Default)}";
    public static string WrapBackground(int color, string s) => $"{Set(color)}{s}{Set(Background.Default)}";

    public static string WrapYellow(string s) => WrapColor(Foreground.Yellow, s);
    public static string WrapRed(string s) => WrapColor(Foreground.Red, s);
    public static string WrapGreen(string s) => WrapColor(Foreground.Green, s);
    public static string WrapBlue(string s) => WrapColor(Foreground.Blue, s);
    public static string WrapWhite(string s) => WrapColor(Foreground.White, s);
    public static string WrapBlack(string s) => WrapColor(Foreground.Black, s);
    
    public static class Foreground
    {
        public static int Black => 30;
        public static int Red => 31;
        public static int Green => 32;
        public static int Yellow => 33;
        public static int Blue => 34;
        public static int Magenta => 35;
        public static int Cyan => 36;
        public static int White => 37;
        public static int Default => 39;
    }
    
    public static class Background
    {
        public static int Black => 40;
        public static int Red => 41;
        public static int Green => 42;
        public static int Yellow => 43;
        public static int Blue => 44;
        public static int Magenta => 45;
        public static int Cyan => 46;
        public static int White => 47;
        public static int Default => 49;
    }
}
