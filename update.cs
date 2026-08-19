using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

internal static class EmfExporter
{
    private const uint CF_ENHMETAFILE = 14;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(
        IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool IsClipboardFormatAvailable(
        uint format);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(
        uint uFormat);

    [DllImport(
        "gdi32.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern IntPtr CopyEnhMetaFile(
        IntPtr hEnh,
        string lpFileName);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteEnhMetaFile(
        IntPtr hEnh);

    public static void ExportChartToEmf(
        Excel.ChartObject chartObject,
        string outputPath)
    {
        if (chartObject == null)
            throw new ArgumentNullException(nameof(chartObject));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException(
                "Output path is required.",
                nameof(outputPath));

        string directory =
            Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Excel -> clipboard as vector picture.
        chartObject.CopyPicture(
            Excel.XlPictureAppearance.xlPrinter,
            Excel.XlCopyPictureFormat.xlPicture);

        // Wait for EMF to appear.
        WaitForClipboardFormat(
            CF_ENHMETAFILE,
            2000);

        if (!OpenClipboard(IntPtr.Zero))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                "Unable to open clipboard.");
        }

        IntPtr sourceEmf = IntPtr.Zero;
        IntPtr savedEmf = IntPtr.Zero;

        try
        {
            sourceEmf =
                GetClipboardData(CF_ENHMETAFILE);

            if (sourceEmf == IntPtr.Zero)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "EMF was not available on clipboard.");
            }

            // Save the clipboard EMF directly to disk.
            savedEmf =
                CopyEnhMetaFile(
                    sourceEmf,
                    outputPath);

            if (savedEmf == IntPtr.Zero)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "CopyEnhMetaFile failed.");
            }
        }
        finally
        {
            // We own the copy returned by CopyEnhMetaFile.
            if (savedEmf != IntPtr.Zero)
            {
                DeleteEnhMetaFile(savedEmf);
            }

            CloseClipboard();
        }
    }

    private static void WaitForClipboardFormat(
        uint format,
        int timeoutMilliseconds)
    {
        DateTime start = DateTime.UtcNow;

        while (true)
        {
            if (IsClipboardFormatAvailable(format))
                return;

            if ((DateTime.UtcNow - start).TotalMilliseconds
                >= timeoutMilliseconds)
            {
                throw new TimeoutException(
                    "Timed out waiting for EMF clipboard data.");
            }

            System.Threading.Thread.Sleep(50);
        }
    }
}