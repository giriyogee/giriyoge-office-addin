public static string ExportTable(
    Excel.Range range,
    string outputPath)
{
    if (range == null)
        throw new ArgumentNullException(nameof(range));

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentNullException(nameof(outputPath));

    Excel.Worksheet sourceSheet = null;
    Excel.Workbook workbook = null;
    Excel.Worksheet tempSheet = null;
    Excel.Shape picture = null;
    Excel.Shape drawingGroup = null;

    try
    {
        sourceSheet = (Excel.Worksheet)range.Worksheet;
        workbook = (Excel.Workbook)sourceSheet.Parent;

        Excel.Application app =
            sourceSheet.Application;

        // ====================================================
        // 1. Copy range as EMF
        // ====================================================

        range.CopyPicture(
            Excel.XlPictureAppearance.xlScreen,
            Excel.XlCopyPictureFormat.xlPicture);

        // ====================================================
        // 2. Create temporary worksheet
        // ====================================================

        tempSheet =
            (Excel.Worksheet)workbook.Worksheets.Add(
                After: sourceSheet);

        // ====================================================
        // 3. Paste as EMF
        // ====================================================

        int shapeCountBefore =
            tempSheet.Shapes.Count;

        tempSheet.PasteSpecial(
            Format: "Picture (Enhanced Metafile)",
            Link: false,
            DisplayAsIcon: false);

        if (tempSheet.Shapes.Count <= shapeCountBefore)
        {
            throw new InvalidOperationException(
                "Excel did not paste the EMF picture.");
        }

        // ====================================================
        // 4. Get newly pasted shape
        // ====================================================

        object shapeIndex =
            tempSheet.Shapes.Count;

        picture =
            tempSheet.Shapes.Item(ref shapeIndex);

        // ====================================================
        // 5. Select picture
        // ====================================================

        picture.Select();

        // ====================================================
        // 6. Queue ENTER BEFORE PictureEdit
        // ====================================================

        System.Windows.Forms.SendKeys.Send("~");

        // ====================================================
        // 7. EMF -> Microsoft Drawing Object
        // ====================================================

        app.CommandBars.ExecuteMso("PictureEdit");

        // ====================================================
        // 8. Get resulting drawing group
        // ====================================================

        object groupIndex =
            tempSheet.Shapes.Count;

        drawingGroup =
            tempSheet.Shapes.Item(ref groupIndex);

        Debug.WriteLine(
            "Converted shape: " +
            drawingGroup.Name);

        Debug.WriteLine(
            "Converted shape type: " +
            drawingGroup.Type);

        Debug.WriteLine(
            "Group items: " +
            drawingGroup.GroupItems.Count);

        // ====================================================
        // 9. Validate conversion
        // ====================================================

        if (drawingGroup.Type !=
            Microsoft.Office.Core.MsoShapeType.msoGroup)
        {
            throw new InvalidOperationException(
                "PictureEdit did not produce a Microsoft Drawing Object group.");
        }

        // ====================================================
        // 10. Use YOUR EXISTING SVG exporter
        // ====================================================

        return CopyAndExport(
            () => drawingGroup.Copy(),
            outputPath);
    }
    finally
    {
        // Delete temporary sheet after SVG has been obtained
        if (tempSheet != null)
        {
            try
            {
                tempSheet.Delete();
            }
            catch
            {
            }
        }

        if (drawingGroup != null)
            Marshal.ReleaseComObject(drawingGroup);

        if (picture != null)
            Marshal.ReleaseComObject(picture);

        if (tempSheet != null)
            Marshal.ReleaseComObject(tempSheet);

        if (workbook != null)
            Marshal.ReleaseComObject(workbook);

        if (sourceSheet != null)
            Marshal.ReleaseComObject(sourceSheet);
    }
}