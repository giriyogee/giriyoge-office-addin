using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

public static class OfficeVectorExporter
{
    private const uint CF_ENHMETAFILE = 14;

    private const string SvgClipboardFormat = "image/svg+xml";


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
            () =>
            {
                chart.CopyPicture(
                    Excel.XlPictureAppearance.xlScreen,
                    Excel.XlCopyPictureFormat.xlPicture,
                    Excel.XlPictureAppearance.xlScreen);
            },
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
            () =>
            {
                shape.CopyPicture(
                    Excel.XlPictureAppearance.xlScreen,
                    Excel.XlCopyPictureFormat.xlPicture);
            },
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
            () =>
            {
                range.CopyPicture(
                    Excel.XlPictureAppearance.xlScreen,
                    Excel.XlCopyPictureFormat.xlPicture);
            },
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

        outputPath = Path.GetFullPath(outputPath);

        string directory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        DeleteIfExists(outputPath);

        // Make sure an old clipboard value is not accidentally used.
        try
        {
            Clipboard.Clear();
        }
        catch
        {
            // Clipboard may be temporarily unavailable.
            // We will still attempt the Excel copy.
        }

        // --------------------------------------------------------
        // Excel native copy
        // --------------------------------------------------------

        copyAction();

        // Wait for Excel to populate clipboard.
        IDataObject data = WaitForClipboardData(5000);

        if (data == null)
        {
            throw new InvalidOperationException(
                "Excel did not place the copied object on the clipboard.");
        }

        // --------------------------------------------------------
        // First preference: SVG
        // --------------------------------------------------------

        if (TrySaveSvg(data, outputPath))
        {
            return outputPath;
        }

        // --------------------------------------------------------
        // Second preference: EMF
        // --------------------------------------------------------

        if (TrySaveEmfFromClipboard(outputPath))
        {
            return outputPath;
        }

        throw new InvalidOperationException(
            "Excel copied the object, but neither SVG nor EMF " +
            "was available on the clipboard.");
    }


    // ============================================================
    // WAIT FOR CLIPBOARD
    // ============================================================

    private static IDataObject WaitForClipboardData(
        int timeoutMilliseconds)
    {
        DateTime start = DateTime.UtcNow;

        while (
            (DateTime.UtcNow - start).TotalMilliseconds
            < timeoutMilliseconds)
        {
            try
            {
                IDataObject data = Clipboard.GetDataObject();

                if (data != null)
                {
                    string[] formats = data.GetFormats();

                    if (formats != null && formats.Length > 0)
                    {
                        return data;
                    }
                }
            }
            catch
            {
                // Clipboard can be locked temporarily by Excel.
            }

            Thread.Sleep(50);
        }

        return null;
    }


    // ============================================================
    // SVG
    // ============================================================

    private static bool TrySaveSvg(
        IDataObject data,
        string outputPath)
    {
        if (data == null)
            return false;

        try
        {
            string[] formats = data.GetFormats();

            if (formats == null)
                return false;

            string svgFormat = FindSvgFormat(formats);

            if (svgFormat == null)
                return false;

            object svgData = data.GetData(svgFormat);

            if (svgData == null)
                return false;

            // ----------------------------------------------------
            // Case 1: SVG comes back as string
            // ----------------------------------------------------

            if (svgData is string svgString)
            {
                if (!LooksLikeSvg(svgString))
                    return false;

                File.WriteAllText(
                    outputPath,
                    svgString,
                    System.Text.Encoding.UTF8);

                return File.Exists(outputPath);
            }

            // ----------------------------------------------------
            // Case 2: SVG comes back as byte[]
            // ----------------------------------------------------

            if (svgData is byte[] svgBytes)
            {
                if (svgBytes.Length == 0)
                    return false;

                File.WriteAllBytes(
                    outputPath,
                    svgBytes);

                return File.Exists(outputPath);
            }

            // ----------------------------------------------------
            // Case 3: MemoryStream
            // ----------------------------------------------------

            if (svgData is MemoryStream memoryStream)
            {
                byte[] bytes = memoryStream.ToArray();

                if (bytes.Length == 0)
                    return false;

                File.WriteAllBytes(
                    outputPath,
                    bytes);

                return File.Exists(outputPath);
            }
        }
        catch
        {
            // SVG extraction failed.
            // Caller will try EMF.
        }

        return false;
    }


    private static string FindSvgFormat(
        string[] formats)
    {
        foreach (string format in formats)
        {
            if (string.IsNullOrEmpty(format))
                continue;

            if (string.Equals(
                    format,
                    SvgClipboardFormat,
                    StringComparison.OrdinalIgnoreCase))
            {
                return format;
            }

            // Some applications expose SVG with slightly
            // different naming.
            if (format.IndexOf(
                    "svg",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return format;
            }
        }

        return null;
    }


    private static bool LooksLikeSvg(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.IndexOf(
                   "<svg",
                   StringComparison.OrdinalIgnoreCase) >= 0;
    }


    // ============================================================
    // EMF
    // ============================================================

    private static bool TrySaveEmfFromClipboard(
        string outputPath)
    {
        IntPtr hEmf = IntPtr.Zero;

        if (!OpenClipboard(IntPtr.Zero))
            return false;

        try
        {
            hEmf = GetClipboardData(CF_ENHMETAFILE);

            if (hEmf == IntPtr.Zero)
                return false;

            uint size = GetEnhMetaFileBits(
                hEmf,
                0,
                null);

            if (size == 0)
                return false;

            byte[] emfBytes = new byte[size];

            uint written = GetEnhMetaFileBits(
                hEmf,
                size,
                emfBytes);

            if (written == 0)
                return false;

            File.WriteAllBytes(
                outputPath,
                emfBytes);

            return File.Exists(outputPath);
        }
        finally
        {
            CloseClipboard();
        }
    }


    // ============================================================
    // FILE HELPERS
    // ============================================================

    private static void DeleteIfExists(
        string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }


    // ============================================================
    // WIN32 CLIPBOARD
    // ============================================================

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(
        IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(
        uint uFormat);


    // ============================================================
    // WIN32 EMF
    // ============================================================

    [DllImport("gdi32.dll")]
    private static extern uint GetEnhMetaFileBits(
        IntPtr hemf,
        uint cbBuffer,
        [Out] byte[] lpbBuffer);
}