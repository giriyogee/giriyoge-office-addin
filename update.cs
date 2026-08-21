private static void ExecutePictureEdit(
    Excel.Application app)
{
    Thread enterThread = new Thread(() =>
    {
        Thread.Sleep(300);

        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
    });

    enterThread.SetApartmentState(ApartmentState.STA);
    enterThread.IsBackground = true;
    enterThread.Start();

    // This blocks while the confirmation dialog is open.
    app.CommandBars.ExecuteMso("PictureEdit");

    enterThread.Join();
}