using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace FT8215Debugger;

public class CSVHelper
{
	private string m_fileName;

	public CSVHelper()
	{
	}

	public CSVHelper(string fileName, string[] columnHeaders)
	{
		FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		StreamWriter streamWriter = new StreamWriter(fileStream);
		string value = string.Join(",", columnHeaders);
		streamWriter.WriteLine(value);
		streamWriter.Flush();
		streamWriter.Close();
		fileStream.Close();
		m_fileName = fileName;
	}

	public int ExportDataTable2CSV(DataTable table)
	{
		if (m_fileName == null || m_fileName == "")
		{
			MessageBox.Show("The *.csv file name is empty!", "FileNameIsNull", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return 1;
		}
		FileStream fileStream = new FileStream(m_fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
		StreamWriter streamWriter = new StreamWriter(fileStream);
		int count = table.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			DataRow dataRow = table.Rows[i];
			int num = dataRow.ItemArray.Count();
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < num; j++)
			{
				if (j == 0)
				{
					stringBuilder.Append(dataRow[j]);
				}
				else
				{
					stringBuilder.AppendFormat(",{0}", dataRow[j]);
				}
			}
			streamWriter.WriteLine(stringBuilder.ToString());
		}
		streamWriter.Flush();
		streamWriter.Close();
		fileStream.Close();
		return 0;
	}
}
