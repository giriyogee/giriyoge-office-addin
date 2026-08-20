// ============================================================
// TABLE / RANGE
// ============================================================

public static string ExportTable(
    Excel.Range range,
    string outputPath)
{
    if (range == null)
        throw new ArgumentNullException(nameof(range));

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentNullException(nameof(outputPath));

    Excel.Worksheet sourceSheet = null;
    Excel.Worksheet tempSheet = null;
    Excel.Shape drawingGroup = null;

    try
    {
        sourceSheet = (Excel.Worksheet)range.Worksheet;

        Excel.Application app =
            sourceSheet.Application;

        // ----------------------------------------------------
        // 1. Copy range as EMF
        // ----------------------------------------------------

        range.CopyPicture(
            Excel.XlPictureAppearance.xlScreen,
            Excel.XlCopyPictureFormat.xlPicture);

        // ----------------------------------------------------
        // 2. Temporary worksheet
        // ----------------------------------------------------

        tempSheet =
            (Excel.Worksheet)sourceSheet.Parent.Worksheets.Add(
                After: sourceSheet);

        int shapeCountBefore =
            tempSheet.Shapes.Count;

        // ----------------------------------------------------
        // 3. Paste EMF
        // ----------------------------------------------------

        tempSheet.PasteSpecial(
            Format: "Picture (Enhanced Metafile)",
            Link: false,
            DisplayAsIcon: false);

        if (tempSheet.Shapes.Count <= shapeCountBefore)
        {
            throw new InvalidOperationException(
                "Excel did not paste the EMF picture.");
        }

        // The newly pasted EMF picture.
        Excel.Shape picture =
            tempSheet.Shapes[tempSheet.Shapes.Count];

        // ----------------------------------------------------
        // 4. Select picture
        // ----------------------------------------------------

        picture.Select();

        // ----------------------------------------------------
        // 5. Queue ENTER BEFORE PictureEdit
        //
        // PictureEdit displays:
        //
        // "This is a picture. Do you want to convert
        //  it to a Microsoft Drawing Object?"
        //
        // ENTER = Yes
        // ----------------------------------------------------

        System.Windows.Forms.SendKeys.Send("~");

        // ----------------------------------------------------
        // 6. Excel converts EMF -> Microsoft Drawing Object
        // ----------------------------------------------------

        app.CommandBars.ExecuteMso("PictureEdit");

        // ----------------------------------------------------
        // 7. The resulting object should now be a Group
        // ----------------------------------------------------

        drawingGroup =
            tempSheet.Shapes[tempSheet.Shapes.Count];

        Debug.WriteLine(
            "Converted shape: " + drawingGroup.Name);

        Debug.WriteLine(
            "Converted shape type: " + drawingGroup.Type);

        Debug.WriteLine(
            "Group items: " +
            drawingGroup.GroupItems.Count);

        // ----------------------------------------------------
        // 8. THIS IS THE IMPORTANT PART:
        //
        // Use the existing SVG clipboard/export mechanism.
        // ----------------------------------------------------

        return CopyAndExport(
            () => drawingGroup.Copy(),
            outputPath);
    }
    finally
    {
        // ----------------------------------------------------
        // 9. Delete temporary worksheet
        // ----------------------------------------------------

        if (tempSheet != null)
        {
            try
            {
                tempSheet.Delete();
            }
            catch
            {
            }

            Marshal.ReleaseComObject(tempSheet);
        }

        if (drawingGroup != null)
        {
            Marshal.ReleaseComObject(drawingGroup);
        }

        if (sourceSheet != null)
        {
            Marshal.ReleaseComObject(sourceSheet);
        }
    }
}