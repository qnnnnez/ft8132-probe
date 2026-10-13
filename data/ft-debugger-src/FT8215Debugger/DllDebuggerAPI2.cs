using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace FT8215Debugger;

internal class DllDebuggerAPI2
{
	public MainWindow mainWindow { get; set; }

	public DllDebuggerAPI2()
	{
		mainWindow = (MainWindow)Application.Current.MainWindow;
	}

	[DllImport("LRunas.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern int RunasUserW(string lpwszUserName, string lpwszUserPsw, string lpwszCmdLine, string lpwszParam, bool bAdminPrivilege, bool bRemember, bool bForceDisplayInterface);

	[DllImport("LRunas.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int RunasUserA(string lpwszUserName, string lpwszUserPsw, string lpwszCmdLine, string lpwszParam, bool bAdminPrivilege, bool bRemember, bool bForceDisplayInterface);

	[DllImport("LRunas.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern bool IsUserAdminPrivilege();

	[DllImport("kernel32")]
	public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

	[DllImport("kernel32")]
	public static extern int GetPrivateProfileString(string section, string key, string defVal, byte[] retVal, int size, string filePath);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern int GetPrivateProfileSection(string lpAppName, byte[] lpReturnedString, uint nSize, string lpFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
}
