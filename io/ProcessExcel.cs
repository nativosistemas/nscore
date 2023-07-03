
using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace nscore;
public class ProcessExcel : IDisposable
{
    private bool disposedValue = false;
    public static DataTable readExcel(string pPath, List<int> pIdColumn)
    {
        DataTable result = null;
        if (!string.IsNullOrWhiteSpace(pPath))
        {
            if (System.IO.File.Exists(pPath))
            {
                try
                {
                    using (var document = SpreadsheetDocument.Open(pPath, false))
                    {
                        WorkbookPart workbookPart = document.WorkbookPart;
                        IEnumerable<Sheet> sheets = workbookPart.Workbook.Descendants<Sheet>();
                        Sheet hojaDeseada = sheets.FirstOrDefault();

                        WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(hojaDeseada.Id);
                        Worksheet worksheet = worksheetPart.Worksheet;
                        if (worksheet != null)
                        {
                            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                            Row? oFirstOrDefault = sheetData.Elements<Row>().FirstOrDefault();
                            if (oFirstOrDefault != null)
                            {
                                var cellValues = oFirstOrDefault.Elements<Cell>().Select(c => GetCellValue(c, workbookPart)).ToList();
                                DataTable oDataTable = new DataTable();
                                for (int iType = 0; iType <= pIdColumn.Count; iType++)
                                {
                                    oDataTable.Columns.Add(new DataColumn(iType.ToString(), cellValues[iType].GetType()));
                                }
                                foreach (Row excelRow in sheetData.Elements<Row>())
                                {
                                    var cellValues_row = excelRow.Elements<Cell>().Select(c => GetCellValue(c, workbookPart)).ToList();
                                    DataRow fila = oDataTable.NewRow();
                                    for (int i = 0; i <= pIdColumn.Count; i++)
                                    {
                                        fila[i] = cellValues_row[i];
                                    }
                                    oDataTable.Rows.Add(fila);
                                }
                                result = oDataTable;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.log(ex);
                }
            }
        }
        return result;
    }
    public static string ConvertDataTableToHTML(DataTable dt)
    {
        string html = "<table>";
        /*
        //add header row
        html += "<tr>";
        for (int i = 0; i < dt.Columns.Count; i++)
            html += "<td>" + dt.Columns[i].ColumnName + "</td>";
        html += "</tr>";
        //add rows
        */
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            html += "<tr>";
            for (int j = 0; j < dt.Columns.Count; j++)
                html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
            html += "</tr>";
        }
        html += "</table>";
        return html;
    }
    public static string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        string result = string.Empty;
        if (cell.DataType == null)
        {
            result = cell.InnerText;
        }
        else if (cell.DataType.Value == CellValues.Number)
        {
            result = cell.InnerText;
        }
        else if (cell.DataType.Value == CellValues.SharedString)
        {
            int index = int.Parse(cell.InnerText);
            SharedStringTablePart sharedStringPart = workbookPart.SharedStringTablePart;
            result = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ElementAt(index).InnerText;
        }

        return result;
    }
    public static DataTable GetDataTable(List<int> pColumnName)
    {
        DataTable result = new DataTable();
        foreach (int c in pColumnName)//System.Type.GetType("System.String"))
        {
            result.Columns.Add(new DataColumn(c.ToString(), System.Type.GetType("System.String")));
        }
        return result;
    }
    public static DataTable GetDataTableAstronomy()
    {
        List<int> l_column = new List<int> { 1, 2, 3, 4, 5 };
        DataTable result = new DataTable();
        //
        string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "Estrellas más brillantes_tabla.xlsx");
        result = readExcel(pathAstronomy, l_column);
        //
        return result;
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // _PoolProcess.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}