using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

public static class EmfExporter
{
    // Windows clipboard format for Enhanced Metafile.
    private const uint CF_ENHMETAFILE = 14;

    // --------------------------------------------------------------------
    // CHART
    // --------------------------------------------------------------------

    public static string ExportChartToEmf(
        Excel.Chart chart,
        string outputPath)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return ExportUsingClipboard(
            () =>
            {
                chart.CopyPicture(
                    Excel.XlPictureAppearance.xlScreen,
                    Excel.XlCopyPictureFormat.xlPicture,
                    Excel.XlPictureAppearance.xlScreen);
            },
            outputPath);
    }


    // --------------------------------------------------------------------
    // GROUP CHART / GROUPED SHAPE
    // --------------------------------------------------------------------

    public static string ExportGroupChartToEmf(
        Excel.Shape groupShape,
        string outputPath)
    {
        if (groupShape == null)
            throw new ArgumentNullException(nameof(groupShape));

        return ExportUsingClipboard(
            () =>
            {
                /*
                 * xlPicture is the important part.
                 *
                 * xlPicture -> vector / EMF
                 * xlBitmap  -> raster / bitmap
                 */
                groupShape.CopyPicture(
                    Excel.XlPictureAppearance.xlScreen,
                    Excel.XlCopyPictureFormat.xlPicture);
            },
            outputPath);
    }


    // --------------------------------------------------------------------
    // TABLE / RANGE
    // --------------------------------------------------------------------

    public static string ExportTableToEmf(
        Excel.Range range,
        string outputPath)
    {
        if (range == null)
            throw new ArgumentNullException(nameof(range));

        return ExportUsingClipboard(
            () =>
            {
                /*
                 * Excel generates the picture representation of the range.
                 *
                 * xlPicture is required to get the vector representation.
                 */
                range.CopyPicture(
                    Excel.XlPictureAppearance.xlScreen,
                    Excel.XlCopyPictureFormat.xlPicture);
            },
            outputPath);
    }


    // --------------------------------------------------------------------
    // COMMON EXPORT PIPELINE
    // --------------------------------------------------------------------

    private static string ExportUsingClipboard(
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
        {
            Directory.CreateDirectory(directory);
        }

        DeleteExistingFile(outputPath);

        ClearClipboard();

        // Execute Excel's native copy operation.
        copyAction();

        // Excel may populate the clipboard asynchronously.
        IntPtr hEmf = WaitForEnhancedMetafile(5000);

        if (hEmf == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Excel did not place an Enhanced Metafile (EMF) on the clipboard.");
        }

        try
        {
            SaveEnhancedMetafile(hEmf, outputPath);
        }
        finally
        {
            // Clipboard owns the handle. Do NOT DeleteEnhMetaFile here.
        }

        if (!File.Exists(outputPath))
        {
            throw new IOException(
                "Failed to create EMF file: " + outputPath);
        }

        return outputPath;
    }


    // --------------------------------------------------------------------
    // CLIPBOARD
    // --------------------------------------------------------------------

    private static void ClearClipboard()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return;

        try
        {
            EmptyClipboard();
        }
        finally
        {
            CloseClipboard();
        }
    }


    private static IntPtr WaitForEnhancedMetafile(int timeoutMs)
    {
        DateTime start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            IntPtr hEmf = GetEnhancedMetafileFromClipboard();

            if (hEmf != IntPtr.Zero)
                return hEmf;

            Thread.Sleep(50);
        }

        return IntPtr.Zero;
    }


    private static IntPtr GetEnhancedMetafileFromClipboard()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return IntPtr.Zero;

        try
        {
            return GetClipboardData(CF_ENHMETAFILE);
        }
        finally
        {
            CloseClipboard();
        }
    }


    // --------------------------------------------------------------------
    // SAVE EMF
    // --------------------------------------------------------------------

    private static void SaveEnhancedMetafile(
        IntPtr hEmf,
        string outputPath)
    {
        if (hEmf == IntPtr.Zero)
            throw new ArgumentNullException(nameof(hEmf));

        uint size = GetEnhMetaFileBits(
            hEmf,
            0,
            null);

        if (size == 0)
        {
            throw new InvalidOperationException(
                "Unable to read Enhanced Metafile data.");
        }

        byte[] emfData = new byte[size];

        uint written = GetEnhMetaFileBits(
            hEmf,
            size,
            emfData);

        if (written == 0)
        {
            throw new InvalidOperationException(
                "Unable to extract Enhanced Metafile data.");
        }

        File.WriteAllBytes(outputPath, emfData);
    }


    private static void DeleteExistingFile(string path)
    {
        if (!File.Exists(path))
            return;

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Sometimes Excel/Word still has the previous EMF open.
            // Let the caller receive a useful error.
            throw;
        }
    }


    // --------------------------------------------------------------------
    // WIN32
    // --------------------------------------------------------------------

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("gdi32.dll")]
    private static extern uint GetEnhMetaFileBits(
        IntPtr hemf,
        uint cbBuffer,
        [Out] byte[] lpbBuffer);
}