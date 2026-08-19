using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;

public static class SvgExporter
{
    private const string SvgMimeType = "image/svg+xml";

    // ============================================================
    // CHART
    // ============================================================

    public static string ExportChart(
        Excel.Chart chart,
        string outputPath)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return CopyAndExport(
            () => chart.Copy(),
            outputPath);
    }


    // ============================================================
    // GROUP CHART / GROUPED SHAPE
    // ============================================================

    public static string ExportGroupChart(
        Excel.Shape shape,
        string outputPath)
    {
        if (shape == null)
            throw new ArgumentNullException(nameof(shape));

        return CopyAndExport(
            () => shape.Copy(),
            outputPath);
    }


    // ============================================================
    // TABLE / RANGE
    // ============================================================

    public static string ExportTable(
        Excel.Range range,
        string outputPath)
    {
        if (range == null)
            throw new ArgumentNullException(nameof(range));

        return CopyAndExport(
            () => range.Copy(),
            outputPath);
    }


    // ============================================================
    // COMMON COPY / EXPORT
    // ============================================================

    private static string CopyAndExport(
        Action copyAction,
        string outputPath)
    {
        if (copyAction == null)
            throw new ArgumentNullException(nameof(copyAction));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentNullException(nameof(outputPath));

        outputPath = NormalizePath(outputPath);

        string directory =
            Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        DeleteIfExists(outputPath);

        ClearClipboard();

        // IMPORTANT:
        // Normal Excel Copy(), NOT CopyPicture().
        copyAction();

        IDataObject data =
            WaitForClipboardData(5000);

        if (data == null)
        {
            throw new InvalidOperationException(
                "Excel did not place the copied object on the clipboard.");
        }

        DebugClipboardFormats(data);

        // Try SVG exposed through .NET.
        if (TrySaveSvgFromManagedClipboard(
                data,
                outputPath))
        {
            return outputPath;
        }

        // Try SVG directly through native Windows clipboard.
        if (TrySaveSvgFromNativeClipboard(
                outputPath))
        {
            return outputPath;
        }

        throw new InvalidOperationException(
            "Excel copied the object, but SVG was not found " +
            "on the clipboard.");
    }


    // ============================================================
    // PATH
    // ============================================================

    private static string NormalizePath(
        string outputPath)
    {
        outputPath =
            Path.GetFullPath(outputPath);

        string directory =
            Path.GetDirectoryName(outputPath);

        string fileName =
            Path.GetFileNameWithoutExtension(outputPath);

        return Path.Combine(
            directory ?? string.Empty,
            fileName + ".svg");
    }


    // ============================================================
    // WAIT FOR CLIPBOARD
    // ============================================================

    private static IDataObject WaitForClipboardData(
        int timeoutMilliseconds)
    {
        DateTime start =
            DateTime.UtcNow;

        while (
            (DateTime.UtcNow - start).TotalMilliseconds
            < timeoutMilliseconds)
        {
            try
            {
                IDataObject data =
                    Clipboard.GetDataObject();

                if (data != null)
                {
                    string[] formats =
                        data.GetFormats();

                    if (formats != null &&
                        formats.Length > 0)
                    {
                        return data;
                    }
                }
            }
            catch
            {
                // Excel may temporarily lock clipboard.
            }

            Thread.Sleep(50);
        }

        return null;
    }


    // ============================================================
    // DEBUG
    // ============================================================

    private static void DebugClipboardFormats(
        IDataObject data)
    {
        Debug.WriteLine(
            "========== CLIPBOARD FORMATS ==========");

        foreach (string format in data.GetFormats())
        {
            Debug.WriteLine(
                "Clipboard format: " + format);
        }

        Debug.WriteLine(
            "========================================");
    }


    // ============================================================
    // SVG - MANAGED CLIPBOARD
    // ============================================================

    private static bool TrySaveSvgFromManagedClipboard(
        IDataObject data,
        string outputPath)
    {
        string svgFormat = null;

        foreach (string format in data.GetFormats())
        {
            if (string.Equals(
                    format,
                    SvgMimeType,
                    StringComparison.OrdinalIgnoreCase) ||
                format.IndexOf(
                    "svg",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                svgFormat = format;
                break;
            }
        }

        if (svgFormat == null)
        {
            Debug.WriteLine(
                "SVG not found in managed clipboard.");

            return false;
        }

        Debug.WriteLine(
            "SVG format found: " + svgFormat);

        object value =
            data.GetData(svgFormat);

        if (value is string text)
        {
            if (!LooksLikeSvg(text))
                return false;

            File.WriteAllText(
                outputPath,
                text,
                System.Text.Encoding.UTF8);

            return true;
        }

        if (value is byte[] bytes)
        {
            File.WriteAllBytes(
                outputPath,
                bytes);

            return true;
        }

        if (value is MemoryStream stream)
        {
            File.WriteAllBytes(
                outputPath,
                stream.ToArray());

            return true;
        }

        return false;
    }


    // ============================================================
    // SVG - NATIVE CLIPBOARD
    // ============================================================

    private static bool TrySaveSvgFromNativeClipboard(
        string outputPath)
    {
        uint svgFormat =
            RegisterClipboardFormat(
                SvgMimeType);

        if (svgFormat == 0)
            return false;

        if (!OpenClipboard(IntPtr.Zero))
            return false;

        try
        {
            IntPtr hData =
                GetClipboardData(svgFormat);

            if (hData == IntPtr.Zero)
            {
                Debug.WriteLine(
                    "SVG NOT FOUND on native clipboard.");

                return false;
            }

            Debug.WriteLine(
                "SVG FOUND on native clipboard.");

            IntPtr pData =
                GlobalLock(hData);

            if (pData == IntPtr.Zero)
                return false;

            try
            {
                UIntPtr size =
                    GlobalSize(hData);

                ulong length =
                    size.ToUInt64();

                if (length == 0 ||
                    length > int.MaxValue)
                {
                    return false;
                }

                byte[] bytes =
                    new byte[(int)length];

                Marshal.Copy(
                    pData,
                    bytes,
                    0,
                    bytes.Length);

                File.WriteAllBytes(
                    outputPath,
                    bytes);

                return File.Exists(outputPath);
            }
            finally
            {
                GlobalUnlock(hData);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }


    // ============================================================
    // HELPERS
    // ============================================================

    private static bool LooksLikeSvg(
        string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.IndexOf(
                   "<svg",
                   StringComparison.OrdinalIgnoreCase) >= 0;
    }


    private static void ClearClipboard()
    {
        try
        {
            Clipboard.Clear();
        }
        catch
        {
        }
    }


    private static void DeleteIfExists(
        string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }


    // ============================================================
    // WIN32
    // ============================================================

    [DllImport(
        "user32.dll",
        CharSet = CharSet.Unicode)]
    private static extern uint RegisterClipboardFormat(
        string lpszFormat);


    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(
        IntPtr hWndNewOwner);


    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();


    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(
        uint uFormat);


    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(
        IntPtr hMem);


    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(
        IntPtr hMem);


    [DllImport("kernel32.dll")]
    private static extern UIntPtr GlobalSize(
        IntPtr hMem);
}