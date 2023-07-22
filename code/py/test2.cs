private void buttonPDF_Click(object sender, EventArgs e)
{
    YZDataGrid dgvInfo = FindDataGrid();

    DataTable dt = (DataTable)dgvInfo.DataSource;

    if (dt == null || dt.Rows.Count == 0)
        return;

    SaveFileDialog dlg = new SaveFileDialog();
    dlg.FileName = this.title;
    dlg.DefaultExt = ".pdf";
    dlg.Filter = "Text documents (.pdf)|*.pdf";

    if (dlg.ShowDialog() == DialogResult.OK)
    {
        string fileName = dlg.FileName;
        int columnCount = dgvInfo.Columns.Count;

        if (dgvInfo.Group != null)
        {
            columnCount--;
        }
        float[] columnWidth = this.exportPDFCalcWidth(dgvInfo, columnCount);
        float width = 0;
        foreach (float v in columnWidth)
        {
            width += v;
        }
        iTextSharp.text.Rectangle pageSize = new iTextSharp.text.Rectangle(width, 842f);
        iTextSharp.text.Document document = new iTextSharp.text.Document(pageSize);
        document.SetMargins(0, 0, 0, 0);

        iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
        document.Open();

        iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont("C:\\WINDOWS\\FONTS\\STSONG.TTF", iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);
        iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, (float)this.Font.Size);

        iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(columnCount);
        table.SetTotalWidth(columnWidth);
        table.LockedWidth = true;

        this.exportPDFTitle(dgvInfo, table, font);
        this.exportPDFRows(dgvInfo, table, font, dt);

        document.Add(table);
        document.Close();

        MessageBox.Show("完成");

    }//end if
}

private float[] exportPDFCalcWidth(YZDataGrid dgvInfo, int columnCount)
{
    float[] columnWidth = new float[columnCount];
    int start = 0;
    int j = 0;

    if (dgvInfo.Group != null)
    {
        start = 1;
    }
    Graphics g = this.CreateGraphics();

    for (int i = start; i < dgvInfo.Columns.Count; i++)
    {
        float width = (dgvInfo.Columns[i].Width * 25.4f / g.DpiX) * (595 / 210);
        columnWidth[j] = width + 6f;
        j++;
    }
    return columnWidth;
}

private void exportPDFTitle(YZDataGrid dgvInfo, iTextSharp.text.pdf.PdfPTable table, iTextSharp.text.Font font)
{
    //列标题                
    int start = 0;
    if (dgvInfo.Group != null)
    {
        start = 1;
    }
    for (int i = start; i < dgvInfo.Columns.Count; i++)
    {
        string title = dgvInfo.Columns[i].HeaderText;
        iTextSharp.text.Phrase phrase = new iTextSharp.text.Phrase(title, font);
        iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(phrase);
        cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
        cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        table.AddCell(cell);
    }

}
private void exportPDFRows(YZDataGrid dgvInfo, iTextSharp.text.pdf.PdfPTable table, iTextSharp.text.Font font, DataTable dt)
{
    int start = 0;
    if (dgvInfo.Group != null)
    {
        start = 1;
    }
    int k = 0;
    foreach (DataRow row in dt.Rows)
    {
        k++;
        for (int i = start; i < dgvInfo.Columns.Count; i++)
        {
            DataGridViewColumn column = dgvInfo.Columns[i];
            string value = row[column.DataPropertyName].ToString();
            iTextSharp.text.Phrase phrase = new iTextSharp.text.Phrase(value, font);
            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(phrase);
            table.AddCell(cell);
        }
    }

}