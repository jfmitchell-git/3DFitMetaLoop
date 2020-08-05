using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaLoop.Common.DataEngine
{
    public class ExcelReader
    {

        public DataSet ReadExcelFile(string filename)
        {
            DataSet spreadheet = new DataSet();

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileStream, false))
                {
                    WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                    IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    foreach (Sheet sheet in spreadSheetDocument.WorkbookPart.Workbook.Sheets)
                    {
                        DataTable dt = new DataTable();

                        string relationshipId = sheet.Id.Value;
                        WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                        Worksheet workSheet = worksheetPart.Worksheet;
                        SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                        IEnumerable<Row> rows = sheetData.Descendants<Row>();

                        dt.TableName = sheet.Name.Value;

                        foreach (Cell cell in rows.ElementAt(0))
                        {
                            string colName = GetCellValue(spreadSheetDocument, cell);

                            if (string.IsNullOrEmpty(colName)) break;
                            if (colName.IndexOf(':') > 0) colName = colName.Split(':')[0];
                            dt.Columns.Add(colName);
                        }


                        int t = rows.Count();
                        foreach (Row row in rows) //this will also include your header row...
                        {
                            DataRow tempRow = dt.NewRow();

                            for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                            {
                                Cell cell = row.Descendants<Cell>().ElementAt(i);
                                int actualCellIndex = CellReferenceToIndex(cell);
                                if (actualCellIndex < dt.Columns.Count)
                                    tempRow[actualCellIndex] = GetCellValue(spreadSheetDocument, cell);
                            }


                            bool emptyRow = true;
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(tempRow[i].ToString()))
                                {
                                    emptyRow = false;
                                    break;
                                }
                            }

                            if (!emptyRow)
                            {
                                dt.Rows.Add(tempRow);
                            }
                            else
                            {
                                break;
                            }


                        }

                        dt.Rows.RemoveAt(0); //...so i'm taking it out here.
                        spreadheet.Tables.Add(dt);
                    }

                }

                return spreadheet;
            }
        }

        private static int CellReferenceToIndex(Cell cell)
        {
            string cellReference = cell.CellReference.ToString().ToUpper();

            //remove digits
            string columnReference = Regex.Replace(cellReference.ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            //working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
            //then multiply that number by our multiplier (which starts at 1)
            //multiply our multiplier by 26 as there are 26 letters
            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier = mulitplier * 26;
            }

            //the result is zero based so return columnnumber + 1 for a 1 based answer
            //this will match Excel's COLUMN function
            return columnNumber;


            //string reference = cell.CellReference.ToString().ToUpper();
            //int ci = 0;
            //reference = reference.ToUpper();
            //for (int ix = 0; ix < reference.Length && reference[ix] >= 'A'; ix++)
            //    ci = (ci * 26) + ((int)reference[ix] - 64);
            //return ci;



            //int index = 0;
            //string reference = cell.CellReference.ToString().ToUpper();
            //foreach (char ch in reference)
            //{
            //    if (Char.IsLetter(ch))
            //    {
            //        int value = (int)ch - (int)'A';
            //        index = (index == 0) ? value : ((index + 1) * 26) + value;
            //    }
            //    else
            //    {
            //        return index;
            //    }

            //}
            //return index;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell == null || cell.CellValue == null) return string.Empty;

            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }
    }
}
