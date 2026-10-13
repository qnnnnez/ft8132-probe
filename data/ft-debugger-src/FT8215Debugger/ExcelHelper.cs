using System;
using System.Data;
using System.IO;
using System.Windows;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace FT8215Debugger;

public class ExcelHelper : IDisposable
{
	private string fileName = null;

	private Workbook workbook = null;

	private FileStream fs = null;

	private bool disposed;

	public ExcelHelper(string fileName)
	{
		this.fileName = fileName;
		disposed = false;
	}

	public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
	{
		Sheet sheet = null;
		DataTable dataTable = new DataTable();
		int num = 0;
		try
		{
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			workbook = new HSSFWorkbook(fs);
			if (sheetName != null)
			{
				sheet = workbook.GetSheet(sheetName);
				if (sheet == null)
				{
					sheet = workbook.GetSheetAt(0);
				}
			}
			else
			{
				sheet = workbook.GetSheetAt(0);
			}
			if (sheet != null)
			{
				Row row = sheet.GetRow(0);
				int lastCellNum = row.LastCellNum;
				int num2 = 0;
				if (isFirstRowColumn)
				{
					for (int i = row.FirstCellNum; i < lastCellNum; i++)
					{
						Cell cell = row.GetCell(i);
						if (cell != null)
						{
							string text = cell.ToString();
							if (text != null)
							{
								DataColumn column = new DataColumn(text);
								dataTable.Columns.Add(column);
							}
						}
					}
				}
				else
				{
					for (int i = 0; i < 10; i++)
					{
						switch (i)
						{
						case 0:
							dataTable.Columns.Add("Number");
							break;
						case 1:
							dataTable.Columns.Add("Time");
							break;
						default:
							dataTable.Columns.Add("CH" + (i - 1));
							break;
						}
					}
				}
				num = sheet.FirstRowNum + 1;
				num2 = sheet.LastRowNum;
				for (int i = num; i <= num2; i++)
				{
					Row row2 = sheet.GetRow(i);
					if (row2 == null)
					{
						continue;
					}
					lastCellNum = row2.LastCellNum;
					DataRow dataRow = dataTable.NewRow();
					for (int j = row2.FirstCellNum; j < lastCellNum; j++)
					{
						if (row2.GetCell(j) != null)
						{
							dataRow[j] = row2.GetCell(j).ToString();
						}
					}
					dataTable.Rows.Add(dataRow);
				}
			}
			return dataTable;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception: " + ex.Message);
			return null;
		}
	}

	public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Sheet sheet = null;
		fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		workbook = new HSSFWorkbook();
		try
		{
			if (workbook != null)
			{
				sheet = workbook.CreateSheet(sheetName);
				if (isColumnWritten)
				{
					Row row = sheet.CreateRow(0);
					for (num2 = 0; num2 < data.Columns.Count; num2++)
					{
						row.CreateCell(num2).SetCellValue(data.Columns[num2].ColumnName);
					}
					num3 = 1;
				}
				else
				{
					num3 = 0;
				}
				for (num = 0; num < data.Rows.Count; num++)
				{
					Row row = sheet.CreateRow(num3);
					for (num2 = 0; num2 < data.Columns.Count; num2++)
					{
						row.CreateCell(num2).SetCellValue(data.Rows[num][num2].ToString());
					}
					num3++;
				}
				workbook.Write(fs);
				return num3;
			}
			return -1;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception: " + ex.Message);
			return -1;
		}
	}

	public int DataTableToExcel(DataTable data, int startIndexOfData, int rowCount, string sheetName, bool isColumnWritten, bool isAppendToExcel)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Sheet sheet = null;
		try
		{
			fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			if (fs.Length > 0 && isAppendToExcel)
			{
				workbook = new HSSFWorkbook(fs);
			}
			else
			{
				workbook = new HSSFWorkbook();
			}
			if (workbook != null)
			{
				sheet = workbook.GetSheet(sheetName);
				if (sheet == null)
				{
					sheet = workbook.CreateSheet(sheetName);
				}
				if (sheet.LastRowNum <= 0)
				{
					if (isColumnWritten)
					{
						Row row = sheet.CreateRow(0);
						for (num2 = 0; num2 < data.Columns.Count; num2++)
						{
							row.CreateCell(num2).SetCellValue(data.Columns[num2].ColumnName);
						}
						num3 = 1;
						int num4 = 0;
						if (data.Rows.Count >= 3)
						{
							num4 = 3;
						}
						for (num = 0; num < num4; num++)
						{
							row = sheet.CreateRow(num + 1);
							for (num2 = 0; num2 < data.Columns.Count; num2++)
							{
								row.CreateCell(num2).SetCellValue(data.Rows[num][num2].ToString());
							}
							num3++;
						}
					}
					else
					{
						num3 = 0;
					}
				}
				else
				{
					num3 = sheet.LastRowNum + 1;
				}
				for (num = startIndexOfData; num < startIndexOfData + rowCount; num++)
				{
					Row row = sheet.CreateRow(num3);
					for (num2 = 0; num2 < data.Columns.Count; num2++)
					{
						row.CreateCell(num2).SetCellValue(data.Rows[num][num2].ToString());
					}
					num3++;
				}
				if (!fs.CanWrite)
				{
					fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
				}
				workbook.Write(fs);
				return num3;
			}
			return -1;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Hand);
			return -1;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing && fs != null)
			{
				fs.Close();
			}
			fs = null;
			disposed = true;
		}
	}
}
