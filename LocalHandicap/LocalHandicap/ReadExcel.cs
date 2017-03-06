using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

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

            dynamic value = excelRange.get_Value(
                    Excel.XlRangeValueDataType.xlRangeValueDefault);

            Marshal.ReleaseComObject(excelRange);
            Marshal.ReleaseComObject(sheet);
            //wb.Close(Type.Missing, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(wb);
            Marshal.ReleaseComObject(wbs);
            //excelApp.Quit();
            Marshal.ReleaseComObject(excelApp);

            ExcelContents = new List<string>();

            if (value is object)
            {
                Type type = value.GetType();
                if (type.IsArray)
                {
                    object[,] valueArray = (object[,])value;
                    for (int i = 1; i < valueArray.GetLength(0); i++)
                    {
                        // Look for a line with 3 non-null
                        if ((valueArray.GetLength(1) >= 3) && (valueArray[i, 1] != null) &&
                            (valueArray[i, 2] != null) && (valueArray[i, 3] != null))
                        {
                            // If the line is more than 3 entries, then the 4th must be null
                            if ((valueArray.GetLength(1) >= 4) && (valueArray[i, 4] != null))
                            {
                                continue;
                            }

                            // If the content is double/string/double if it looks like this: 9079663	Albitz, Paul	2.3	
                            // If the content is double/string/string if it looks like this: 9079663	Albitz, Paul	2.3M	
                            string playerEntry = string.Empty;
                            if ((valueArray[i, 1] is double) && (valueArray[i, 2] is string))
                            {
                                // Create CSV line with name first, then GHIN, then index
                                if (valueArray[i, 3] is double)
                                {
                                    ExcelContents.Add("\"" + (string)valueArray[i, 2] + "\"," + ((double)valueArray[i, 1]).ToString() + "," +  ((double)valueArray[i, 3]).ToString());
                                }
                                else if (valueArray[i, 3] is string)
                                {
                                    ExcelContents.Add("\"" + (string)valueArray[i, 2] + "\"," + ((double)valueArray[i, 1]).ToString() + "," +  (string)valueArray[i, 3]);
                                }
                            }
                        }
                        //for (int j = 1; j < valueArray.GetLength(1); j++)
                        //{
                            
                            //if (valueArray[i, j] is string)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("String (" + i + "," + j + "): " + (string)valueArray[i, j]);
                            //}
                            //else if (valueArray[i, j] is float)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("Float (" + i + "," + j + "): " + ((float)valueArray[i, j]).ToString());
                            //}
                            //else if (valueArray[i, j] is int)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("Int (" + i + "," + j + "): " + (int)valueArray[i, j]);
                            //}
                            //else if (valueArray[i, j] is double)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("Double (" + i + "," + j + "): " + ((double)valueArray[i, j]).ToString());
                            //}
                            //else
                            //{
                            //    System.Diagnostics.Debug.WriteLine("??? (" + i + "," + j + "): " + valueArray[i, j].GetType());
                            //}
                        //}
                    }
                }
            }
        }
    }
}
