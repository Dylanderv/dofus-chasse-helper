using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DofusChasseHelper.Domain.External;
using ScaleHQ.DotScreen;

namespace DofusChasseHelper.Infrastructure;

public class ScreenshotProvider : IScreenshotProvider
{
    public Bitmap ScreenShot()
    {
        var screen = Screen.PrimaryScreen;

        var deviceName = screen.DeviceName;
        var screenSize = screen.Bounds;
        var workingArea = screen.WorkingArea;

        Console.WriteLine($"{deviceName} | {workingArea.Width}x{workingArea.Height}");

        var target = new Bitmap(screenSize.Width, screenSize.Height);
        using (var g = Graphics.FromImage(target))
        {
            g.CopyFromScreen(0, 0, 0, 0, new Size(screenSize.Width, screenSize.Height));
            // target.Save($".\\base\\{deviceName}.png", ImageFormat.Png);
        }

        return target;
    }
    
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

    public Bitmap PrintWindow(string characterName)    
    {       
        Process proc;

        // Cater for cases when the process can't be located.
        try
        {
            proc = Process
                .GetProcesses()
                .Where(x => x.ProcessName.Equals("Dofus", StringComparison.OrdinalIgnoreCase))
                .Single(x => x.MainWindowTitle.Contains(characterName));
        }
        catch (IndexOutOfRangeException e)
        {
            return null;
        }

        
        RECT rc;        
        GetWindowRect(proc.MainWindowHandle, out rc);
   
        Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);        
        Graphics gfxBmp = Graphics.FromImage(bmp);        
        IntPtr hdcBitmap = gfxBmp.GetHdc();        

        PrintWindow(proc.MainWindowHandle, hdcBitmap, 2);  
  
        gfxBmp.ReleaseHdc(hdcBitmap);               
        gfxBmp.Dispose(); 
   
        return bmp;   
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    private int _Left;
    private int _Top;
    private int _Right;
    private int _Bottom;

    public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
    {
    }
    public RECT(int Left, int Top, int Right, int Bottom)
    {
        _Left = Left;
        _Top = Top;
        _Right = Right;
        _Bottom = Bottom;
    }

    public int X {
        get { return _Left; }
        set { _Left = value; }
    }
    public int Y {
        get { return _Top; }
        set { _Top = value; }
    }
    public int Left {
        get { return _Left; }
        set { _Left = value; }
    }
    public int Top {
        get { return _Top; }
        set { _Top = value; }
    }
    public int Right {
        get { return _Right; }
        set { _Right = value; }
    }
    public int Bottom {
        get { return _Bottom; }
        set { _Bottom = value; }
    }
    public int Height {
        get { return _Bottom - _Top; }
        set { _Bottom = value + _Top; }
    }
    public int Width {
        get { return _Right - _Left; }
        set { _Right = value + _Left; }
    }
    public Point Location {
        get { return new Point(Left, Top); }
        set {
            _Left = value.X;
            _Top = value.Y;
        }
    }
    public Size Size {
        get { return new Size(Width, Height); }
        set {
            _Right = value.Width + _Left;
            _Bottom = value.Height + _Top;
        }
    }

    public static implicit operator Rectangle(RECT Rectangle)
    {
        return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
    }
    public static implicit operator RECT(Rectangle Rectangle)
    {
        return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
    }
    public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
    {
        return Rectangle1.Equals(Rectangle2);
    }
    public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
    {
        return !Rectangle1.Equals(Rectangle2);
    }

    public override string ToString()
    {
        return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public bool Equals(RECT Rectangle)
    {
        return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
    }

    public override bool Equals(object Object)
    {
        if (Object is RECT) {
            return Equals((RECT)Object);
        } else if (Object is Rectangle) {
            return Equals(new RECT((Rectangle)Object));
        }

        return false;
    }
}