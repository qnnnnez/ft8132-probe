using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using FT8215Debugger.Properties;

namespace FT8215Debugger;

public partial class App : Application
{
	private const int GWL_STYLE = -16;

	private const int SW_SHOWNOACTIVATE = 4;

	private const long WS_MINIMIZE = 536870912L;

	private const bool DEBUG_INFO = false;

	public int eArgsLen = 0;

	public List<string> appArgsList = new List<string>();

	protected override void OnStartup(StartupEventArgs e)
	{
		try
		{
			Process[] processesByName = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length <= 2)
			{
				bool flag = false;
				short num = 0;
				byte b = 0;
				bool isInCommand = false;
				eArgsLen = e.Args.Length;
				for (int i = 0; i < e.Args.Length; i++)
				{
					appArgsList.Add(e.Args[i]);
					if (e.Args[i] == "-q" || e.Args[i] == "-quiet")
					{
						flag = true;
					}
					else if (e.Args[i] == "-senmode")
					{
						num = short.Parse(e.Args[i + 1]);
						appArgsList.Add(e.Args[i + 1]);
						i++;
					}
					else if (e.Args[i] == "-curtp")
					{
						b = byte.Parse(e.Args[i + 1]);
						appArgsList.Add(e.Args[i + 1]);
						i++;
					}
					else if (e.Args[i] == "-incmd")
					{
						isInCommand = bool.Parse(e.Args[i + 1]);
						appArgsList.Add(e.Args[i + 1]);
						i++;
					}
				}
				OutputDebugString(string.Format("{0}", string.Join(" ", appArgsList.ToArray())));
				OutputDebugString("argslen: " + eArgsLen);
				if (0 == 0)
				{
					if (eArgsLen > 1)
					{
						MainWindow mainWindow = new MainWindow();
						if (flag)
						{
							mainWindow.Visibility = Visibility.Hidden;
							mainWindow.ShowInTaskbar = false;
							mainWindow.WindowState = WindowState.Minimized;
						}
						else
						{
							MyStartupWindow myStartupWindow = new MyStartupWindow();
							short sensorMode = myStartupWindow.GetSensorMode();
							byte circuitType = myStartupWindow.GetCircuitType();
							bool reloadFlag = false;
							if (sensorMode != num || circuitType != b)
							{
								reloadFlag = true;
							}
							myStartupWindow.Close();
							mainWindow.SetSensorModeCircuitType(num, b, isInCommand);
							mainWindow.SetReloadFlag(reloadFlag);
						}
						mainWindow.Show();
					}
					else
					{
						MyStartupWindow myStartupWindow = new MyStartupWindow();
						if (myStartupWindow.ShowDialog() == true)
						{
							num = myStartupWindow.GetSensorMode();
							b = myStartupWindow.GetCircuitType();
							MainWindow mainWindow = new MainWindow();
							mainWindow.SetSensorModeCircuitType(num, b);
							mainWindow.SetReloadFlag(myStartupWindow.GetReloadFlag());
							mainWindow.Show();
						}
						else
						{
							SetEvent(CreateEvent(IntPtr.Zero, bManualReset: false, binitialState: false, "FT8132Debugger"));
							Environment.Exit(0);
							Application.Current.Shutdown();
						}
					}
					base.OnStartup(e);
				}
				else
				{
					SetEvent(CreateEvent(IntPtr.Zero, bManualReset: false, binitialState: false, "FT8132Debugger"));
					Environment.Exit(0);
					Application.Current.Shutdown();
				}
			}
			else
			{
				Console.WriteLine("The same process is running.\nReturn value: 0xFF0064.");
				LOG.WriteExecuteResult("The same process is running.\nReturn value: 0xFF0064.", append: false);
				SetEvent(CreateEvent(IntPtr.Zero, bManualReset: false, binitialState: false, "FT8132Debugger"));
				Environment.Exit(0);
				Application.Current.Shutdown();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + "\nReturn value: 0xFF0065.");
			LOG.WriteExecuteResult(ex.Message + "\nReturn value: 0xFF0065.", append: false);
			MessageBoxResult messageBoxResult;
			do
			{
				bool flag2 = true;
				messageBoxResult = MessageBox.Show("OnStartUp: " + ex.Message);
			}
			while (messageBoxResult != MessageBoxResult.OK && messageBoxResult != MessageBoxResult.Yes);
		}
	}

	private int RunasUser()
	{
		int num = 0;
		Thread.Sleep(80);
		bool flag;
		if (DllDebuggerAPI2.IsUserAdminPrivilege())
		{
			OutputDebugString("is admin");
			flag = true;
			Settings.Default.RestartCount = 1;
			Settings.Default.Save();
			return 0;
		}
		OutputDebugString("is not admin, runasu");
		flag = true;
		if (Settings.Default.RestartCount <= 0)
		{
			Settings.Default.RestartCount = 1;
			Settings.Default.Save();
			DeleteExePathFromIniFile();
			MessageBox.Show("Administrator permission required!", "Administrator permission required!", MessageBoxButton.OK);
		}
		else
		{
			Settings.Default.RestartCount--;
			Settings.Default.Save();
			string fileName = Process.GetCurrentProcess().MainModule.FileName;
			fileName = $"\"{fileName}\"";
			DllDebuggerAPI2.RunasUserW(null, null, fileName, string.Join(" ", appArgsList.ToArray()), bAdminPrivilege: true, bRemember: true, bForceDisplayInterface: false);
		}
		return 1;
	}

	private void DeleteExePathFromIniFile()
	{
		string text = "C:\\Users\\Public\\Documents\\FortiorTechSw\\FT8132Debugger\\FTProvisConf.ini";
		if (!File.Exists(text))
		{
			return;
		}
		string text2 = "";
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		StringBuilder stringBuilder = new StringBuilder(10);
		StringBuilder stringBuilder2 = new StringBuilder(260);
		DllDebuggerAPI2.GetPrivateProfileString("Basic", "SectionCount", "", stringBuilder, 10u, text);
		int.TryParse(stringBuilder.ToString(), out var result);
		for (int i = 0; i < result; i++)
		{
			text2 = "Section" + (i + 1);
			stringBuilder2.Remove(0, stringBuilder2.Length);
			DllDebuggerAPI2.GetPrivateProfileString(text2, "ExePath", "", stringBuilder2, 260u, text);
			if (string.Compare(stringBuilder2.ToString(), baseDirectory) == 0)
			{
				DllDebuggerAPI2.WritePrivateProfileString(text2, "ExePath", "", text);
			}
		}
	}

	public void Log(string text)
	{
		string path = AppDomain.CurrentDomain.BaseDirectory + "log\\SInfo.log";
		StreamWriter streamWriter = new StreamWriter(path, append: true);
		streamWriter.WriteLine("------------------------");
		streamWriter.WriteLine("Time:" + DateTime.Now.ToString());
		streamWriter.WriteLine(text);
		streamWriter.WriteLine("");
		streamWriter.Close();
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool binitialState, string lpName);

	[DllImport("user32.dll")]
	private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern long SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

	[DllImport("kernel32.dll")]
	private static extern bool SetEvent(IntPtr lpHandle);

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern void OutputDebugString(string message);

	private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		string path = AppDomain.CurrentDomain.BaseDirectory + "log\\ErrInfo.log";
		StreamWriter streamWriter = new StreamWriter(path, append: true);
		streamWriter.WriteLine("Time:" + DateTime.Now.ToString());
		streamWriter.WriteLine(e.Exception.ToString());
		streamWriter.Close();
		OutputDebugString("Exception:" + e.Exception.Message);
		MessageBox.Show(e.Exception.ToString(), "Exception");
		e.Handled = true;
	}

	[STAThread]
	[DebuggerNonUserCode]
	public static void Main()
	{
		App app = new App();
		app.InitializeComponent();
		app.Run();
	}
}
