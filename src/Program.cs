using System.Drawing;
using System.Drawing.Imaging;

string[] cmdArgs = Environment.GetCommandLineArgs();

if (cmdArgs.Length == 3)
{
    if (!File.Exists(cmdArgs[1]))
    {
        Console.WriteLine("File does not exist");
        return;
    }
    byte[] data = File.ReadAllBytes(cmdArgs[1]);
    GenerateHilbert_1bpp(data, cmdArgs[2]);
}
else if (cmdArgs.Length == 4)
{
    if (!File.Exists(cmdArgs[1]))
    {
        Console.WriteLine("File does not exist");
        return;
    }
    byte[] data = File.ReadAllBytes(cmdArgs[1]);
    switch (cmdArgs[3])
    {
        case "bit":
            GenerateHilbert_Bit(data, cmdArgs[2]);
            break;

        case "1bpp":
            GenerateHilbert_1bpp(data, cmdArgs[2]);
            break;

        case "2bpp":
            GenerateHilbert_2bpp(data, cmdArgs[2]);
            break;

        default:
            Console.WriteLine("Invalid mode");
            break;
    }
}
else
    Console.WriteLine("Invalid parameters");

static void GenerateHilbert_2bpp(byte[] data, string outfile)
{
    uint n = (uint)Math.Ceiling(Math.Sqrt(data.Length / 2));
    Bitmap bmp = new Bitmap((int)n, (int)n);
    for (uint i = 0; i < Math.Pow(n, 2) * 2; i += 2)
    {
        Point p = HCPoint(i / 2, n);
        if (i < data.Length)
        {
            bmp.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, data[i], data[i + 1]));
        }
        else
            bmp.SetPixel(p.X, p.Y, Color.Red);
    }
    bmp.Save(outfile, ImageFormat.Png);
}

static void GenerateHilbert_1bpp(byte[] data, string outfile)
{
    uint n = (uint)Math.Ceiling(Math.Sqrt(data.Length));
    Bitmap bmp = new Bitmap((int)n, (int)n);
    for (uint i = 0; i < Math.Pow(n, 2); i++)
    {
        Point p = HCPoint(i, n);
        if (i < data.Length)
        {
            bmp.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, data[i], 0));
        }
        else
            bmp.SetPixel(p.X, p.Y, Color.Red);
    }
    bmp.Save(outfile, ImageFormat.Png);
}

static void GenerateHilbert_Bit(byte[] data, string outfile)
{
    uint n = (uint)Math.Ceiling(Math.Sqrt(data.Length * 8));
    Bitmap bmp = new Bitmap((int)n, (int)n);
    for (uint i = 0; i < Math.Pow(n, 2); i++)
    {
        Point p = HCPoint(i, n);
        if (i < data.Length * 8)
        {
            byte b = data[i / 8];
            bool bit = BitOperations.IsBitSet(b, (int)i % 8);
            bmp.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, bit ? 255 : 0, 0));
        }
        else
            bmp.SetPixel(p.X, p.Y, Color.Red);
    }
    bmp.Save(outfile, ImageFormat.Png);
}

static uint Last2Bits(uint x) => x & 0b11;

static Point HCPoint(uint x, uint n)
{
    uint[][] positions = 
    { 
        new uint[] { 0, 0 }, 
        new uint[] { 0, 1 }, 
        new uint[] { 1, 1 }, 
        new uint[] { 1, 0 } 
    };

    uint[] tmp = positions[Last2Bits(x)];
    x = x >> 2;

    uint cX = tmp[0];
    uint cY = tmp[1];

    for (uint i = 4; i <= n; i *= 2)
    {
        uint i2 = i / 2;

        switch (Last2Bits(x))
        {
            case 0:
                {
                    uint tmp2 = cX;
                    cX = cY;
                    cY = tmp2;
                    break;
                }

            case 1:
                {
                    cY += i2;
                    break;
                }

            case 2:
                {
                    cX += i2;
                    cY += i2;
                    break;
                }

            case 3:
                {
                    uint tmp2 = cY;
                    cY = (i2 - 1) - cX;
                    cX = (i2 - 1) - tmp2;
                    cX += i2;
                    break;
                }
        }

        x = x >> 2;
    }

    return new Point((int)cX, (int)cY);
}
static class BitOperations
{
    public static bool IsBitSet(ushort val, int pos)
    {
        return (val & (1 << pos)) != 0;
    }

    public static void SetBit(ref ushort val, int pos, bool value)
    {
        if (value)
        {
            val = (ushort)(val | (1 << pos));
        }
        else
        {
            val = (ushort)(val & ~(1 << pos));
        }
    }
}
