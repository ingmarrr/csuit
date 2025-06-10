namespace Dcs.Utils;

public static class Table
{
    public static char TopLeftCorner => '\u250C';        // ┌
    public static char TopRightCorner => '\u2510';       // ┐
    public static char BottomLeftCorner => '\u2514';     // └ 
    public static char BottomRightCorner => '\u2518';    // ┘
    public static char VerticalToLeft => '\u2524';       // ┤
    public static char VerticalToRight => '\u251C';      // ├
    public static char HorizontalDownwards => '\u252C';  // ┬
    public static char HorizontalUpwards => '\u2534';    // ┴
    public static char Cross => '\u253C';                // ┼
    public static char Horizontal => '\u2500';           // ─
    public static char Vertical => '\u2502';             // │

    public static string TopRow(int cols)
    {
        return TopLeftCorner + new string(Horizontal, cols - 2) + TopRightCorner + '\n';
    }
    
    public static string BottomRow(int cols)
    {
        return BottomLeftCorner + new string(Horizontal, cols - 2) + BottomRightCorner + '\n';
    }
}

