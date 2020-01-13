using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LocalHandicap
{
    class ReadExcelFile
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public List<string> ExcelContents { get; private set; }

        public ReadExcelFile(string fileName)
        {
            var excelApp = new Excel.Application();

            Job job = new Job();
            uint pid = 0;
            GetWindowThreadProcessId(new IntPtr(excelApp.Hwnd), out pid);
            if (!job.AddProcess(Process.GetProcessById((int)pid).Handle))
            {
                throw new ApplicationException("Failed to save process ID for Excel process");
            }

            Excel.Workbooks wbs = excelApp.Workbooks;
            Excel.Workbook wb = wbs.Open(fileName,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing);

            // sheets are indexed starting at 1
            Excel.Worksheet sheet = (Excel.Worksheet)wb.Sheets[1];

            Excel.Range excelRange = sheet.UsedRange;

            dynamic spreadsheetValue = excelRange.get_Value(
                    Excel.XlRangeValueDataType.xlRangeValueDefault);

            Marshal.ReleaseComObject(excelRange);
            Marshal.ReleaseComObject(sheet);
            //wb.Close(Type.Missing, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(wb);
            Marshal.ReleaseComObject(wbs);
            //excelApp.Quit();
            Marshal.ReleaseComObject(excelApp);

            ExcelContents = new List<string>();

            List<int> ghinColumns = new List<int>();

            if (spreadsheetValue is object)
            {
                Type type = spreadsheetValue.GetType();
                if (type.IsArray)
                {
                    object[,] valueArray = (object[,])spreadsheetValue;
                    for (int i = 1; i < valueArray.GetLength(0); i++)
                    {
                        if (ghinColumns.Count == 0)
                        {
                            for (int j = 1; j < valueArray.GetLength(1); j++)
                            {
                                string value = valueArray[i, j] as string;
                                if ((value != null) && value.ToLower().Contains("ghin"))
                                {
                                    ghinColumns.Add(j);
                                }
                            }
                        }
                        else
                        {
                            foreach (var col in ghinColumns)
                            {
                                if ((valueArray.GetLength(1) >= (col + 2)) && (valueArray[i, col] != null) &&
                                    (valueArray[i, col + 1] != null) && (valueArray[i, col + 2] != null))
                                {
                                    AddCsvLine(valueArray, i, col);
                                }
                            }
                        }

                        //for (int j = 1; j < valueArray.GetLength(1); j++)
                        //{

                        //    if (valueArray[i, j] is string)
                        //    {
                        //        System.Diagnostics.Debug.WriteLine("String (" + i + "," + j + "): " + (string)valueArray[i, j]);
                        //    }
                        //    else if (valueArray[i, j] is float)
                        //    {
                        //        System.Diagnostics.Debug.WriteLine("Float (" + i + "," + j + "): " + ((float)valueArray[i, j]).ToString());
                        //    }
                        //    else if (valueArray[i, j] is int)
                        //    {
                        //        System.Diagnostics.Debug.WriteLine("Int (" + i + "," + j + "): " + (int)valueArray[i, j]);
                        //    }
                        //    else if (valueArray[i, j] is double)
                        //    {
                        //        System.Diagnostics.Debug.WriteLine("Double (" + i + "," + j + "): " + ((double)valueArray[i, j]).ToString());
                        //    }
                        //    else if (valueArray[i, j] == null)
                        //    {
                        //        // ignore
                        //    }
                        //    else
                        //    {
                        //        System.Diagnostics.Debug.WriteLine("??? (" + i + "," + j + "): " + valueArray[i, j].GetType());
                        //    }
                        //}
                    }
                }
            }
        }

        private bool AddCsvLine(object[,] valueArray, int row, int column)
        {
            if ((valueArray[row, column] == null) || (valueArray[row, column + 1] == null) || (valueArray[row, column + 2] == null))
            {
                return false;
            }

            // If the content is double/string/double if it looks like this: 9079663	Albitz, Paul	2.3	
            // If the content is double/string/string if it looks like this: 9079663	Albitz, Paul	2.3M	
            // 2020: newer report has 9079663   Paul Albitz  2.3
                string playerEntry = string.Empty;
            if ((valueArray[row, column] is double) && (valueArray[row, column + 1] is string))
            {
                string lastNameFirstName = (string)valueArray[row, column + 1];
                // If the format is not "last, first" then fix up the name to meet that format
                if (!lastNameFirstName.Contains(","))
                {
                    // Remove spaces at the end and any double spaces in the middle
                    lastNameFirstName = lastNameFirstName.Trim().Replace("  ", " ");
                    string[] components = lastNameFirstName.Split(' ');
                    if (components.Length > 1)
                    {
                        int lastIndex = components.Length - 1;
                        bool containsSuffix = false;
                        // Check for ending with "jr" or "jr."
                        if (components[lastIndex].ToLower().StartsWith("jr") && (components[lastIndex].Length <= 3))
                        {
                            containsSuffix = true;
                            lastIndex--;
                        }
                        lastNameFirstName = components[lastIndex] + ", ";
                        // Add in the first and middle names
                        for (int i = lastIndex - 1; i >= 0; i--)
                        {
                            // Sometimes there are extra spaces in the name
                            if (!string.IsNullOrEmpty(components[i]))
                            {
                                lastNameFirstName += components[i] + " ";
                            }
                        }

                        if (containsSuffix)
                        {
                            lastNameFirstName += components[components.Length - 1];
                        }
                        lastNameFirstName = lastNameFirstName.TrimEnd();
                    }
                }
                // Create CSV line with name first, then GHIN, then index
                if (valueArray[row, column + 2] is double)
                {
                    ExcelContents.Add("\"" + lastNameFirstName + "\"," + ((double)valueArray[row, column]).ToString() + "," + ((double)valueArray[row, column + 2]).ToString());
                    return true;
                }
                else if (valueArray[row, column + 2] is string)
                {
                    // The 3rd field can be "NH", "2.3R" or "+0.9" or "2.6" but listed as type string
                    string value = valueArray[row, column + 2] as string;
                    value = value.Trim();
                    if (value.Length > 0)
                    {
                        string valueWithoutLastChar = value.Remove(value.Length - 1, 1);
                        string valueWithoutFirstChar = value.Remove(0, 1);
                        float index;
                        if ((value.ToLower() == "nh") || float.TryParse(value, out index) || float.TryParse(valueWithoutLastChar, out index) || float.TryParse(valueWithoutFirstChar, out index))
                        {
                            ExcelContents.Add("\"" + lastNameFirstName + "\"," + ((double)valueArray[row, column]).ToString() + "," + (string)valueArray[row, column + 2]);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Ignoring this line because the index is not of the expected form: " +
                                ((double)valueArray[row, column]).ToString() + " " +
                                (string)valueArray[row, column + 1] + " " +
                                (string)valueArray[row, column + 2]);
                        }

                    }
                }
            }
            return false;
        }
    }
}
