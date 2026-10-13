using System;
using System.IO;

namespace FT8215Debugger;

internal class LOG
{
	private static string m_fileName = AppDomain.CurrentDomain.BaseDirectory + "log\\info.log";

	public LOG()
	{
	}

	public LOG(string fileName)
	{
		m_fileName = fileName;
	}

	public void SetFileName(string fileName)
	{
		m_fileName = fileName;
	}

	public static int WriteMsg(string message)
	{
		if (m_fileName == "")
		{
			return 1;
		}
		try
		{
			StreamWriter streamWriter = new StreamWriter(m_fileName, append: true);
			streamWriter.WriteLine("Time: " + DateTime.Now.ToString());
			streamWriter.WriteLine(message);
			streamWriter.WriteLine();
			streamWriter.Close();
		}
		catch
		{
			return 2;
		}
		return 0;
	}

	public static void WriteExecuteResult(string text, bool append)
	{
		string path = AppDomain.CurrentDomain.BaseDirectory + "usercode\\output.txt";
		try
		{
			StreamWriter streamWriter = new StreamWriter(path, append);
			streamWriter.WriteLine(text);
			streamWriter.Close();
		}
		catch (Exception)
		{
		}
	}
}
