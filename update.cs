private static string CopyAndExport(
    Action copyAction,
    string outputPath)
{
    if (copyAction == null)
        throw new ArgumentNullException(nameof(copyAction));

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentNullException(nameof(outputPath));

    // Treat supplied path as a BASE path.
    outputPath = Path.GetFullPath(outputPath);

    // Remove any extension supplied by caller.
    outputPath = Path.Combine(
        Path.GetDirectoryName(outputPath),
        Path.GetFileNameWithoutExtension(outputPath));

    string directory = Path.GetDirectoryName(outputPath);

    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);

    // Remove old output files.
    DeleteIfExists(outputPath + ".svg");
    DeleteIfExists(outputPath + ".emf");

    Clipboard.Clear();

    // Excel native copy.
    copyAction();

    IDataObject data = WaitForClipboardData(5000);

    if (data == null)
    {
        throw new InvalidOperationException(
            "Excel did not place the copied object on the clipboard.");
    }

    // ------------------------------------------------------------
    // Try SVG first
    // ------------------------------------------------------------

    string svgPath = outputPath + ".svg";

    if (TrySaveSvg(data, svgPath))
    {
        return svgPath;
    }

    // ------------------------------------------------------------
    // Fall back to EMF
    // ------------------------------------------------------------

    string emfPath = outputPath + ".emf";

    if (TrySaveEmfFromClipboard(emfPath))
    {
        return emfPath;
    }

    throw new InvalidOperationException(
        "Excel copied the object, but neither SVG nor EMF " +
        "was available on the clipboard.");
}