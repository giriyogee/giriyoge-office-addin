await Word.run(async (context) => {
    const cc = context.document.contentControls.getFirst();

    cc.load("id,tag,title");
    await context.sync();

    console.log("Content Control:", cc.tag);

    // Get the range contained by the CC
    const range = cc.getRange("Content");

    // Try to access charts from the range
    const charts = range.getCharts();

    charts.load("items");

    await context.sync();

    console.log("Charts found:", charts.items.length);

    for (const chart of charts.items) {

        chart.load("name,title,altText");
        await context.sync();

        console.log("Chart:", chart);

        const ooxml = chart.getOoxml();

        await context.sync();

        console.log("CHART OOXML:");
        console.log(ooxml.value);
    }
});