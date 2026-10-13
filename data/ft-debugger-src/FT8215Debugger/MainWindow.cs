using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using FT8215Debugger.Properties;
using Microsoft.Win32;

namespace FT8215Debugger;

public partial class MainWindow : Window, IComponentConnector
{
	public enum E_BtnOptType
	{
		eBtn_OPT_DEBUG = 1,
		eBtn_OPT_STOP = 2,
		eBtn_OPT_ON_OFF_TEST = 19
	}

	private const float CTRL_EN_OPACITY = 1f;

	private const float CTRL_DIS_OPACITY = 0.2f;

	public const int ONOFF_SIMULATE_MOTOR = 1;

	public const ushort CRC_SEED = ushort.MaxValue;

	public const ushort POLY16 = 4129;

	public const int MAX_OFFSET_CNT = 2;

	public const int WM_DEVICE_CHANGE = 537;

	private const int DBT_DEVICEARRIVAL = 32768;

	private const int DBT_DEVICEREMOVECOMPLETE = 32772;

	private const bool SW_SIMULATOR = false;

	private const ushort cst_wSampleFreq = 25000;

	private const float cst_fDOUTMAX = 0.051f;

	private const byte METHOD_PLL = 0;

	private const byte METHOD_SMO = 1;

	private const byte cst_Disable = 0;

	private const byte cst_Enable = 1;

	private const byte Voltage_LOOP = 0;

	private const byte Current_LOOP = 1;

	private const byte Speed_LOOP = 2;

	private const byte Power_LOOP = 3;

	private const byte SpdCtrlM_Anolog = 0;

	private const byte SpdCtrlM_DIGITAL = 1;

	private const byte SpdCtrlM_I2C = 2;

	private const float cst_2PI = 6.283185f;

	public const ushort MAX_SPEED_CNT = 5;

	public const ushort MAX_SPEED_PWM_CNT = 6;

	public const ushort MAX_VOL_CNT = 5;

	public BackgroundWorker bw;

	private XmlDocument document = null;

	private Storyboard myStoryboard;

	private Storyboard myStaustStoryboard;

	private DispatcherTimer timer;

	private DispatcherTimer Downloadtimer;

	private DispatcherTimer OnTimer;

	private DispatcherTimer OffTimer;

	private DispatcherTimer OnOffTimer;

	private DispatcherTimer readTimer;

	private DateTime dtBeginTime;

	private DateTime dtEndTime;

	private DateTime dtBeginOnTestTime;

	private DateTime dtInitTime;

	private DateTime dtFinalTime;

	private List<string> arrDeviceSN = new List<string>();

	private List<string> arrPulloutDeviceSN = new List<string>();

	public List<CDevListItemData> m_DevList = new List<CDevListItemData>();

	private List<DllDebuggerAPI.S_DevMapInfo> arrPortMapInfo = new List<DllDebuggerAPI.S_DevMapInfo>();

	private List<ParamStructureDef.S_OnOffTestRecordCfg> m_OnOffTestTotalList = new List<ParamStructureDef.S_OnOffTestRecordCfg>();

	private List<double> m_TimeList = new List<double>();

	private List<int> m_CountList = new List<int>();

	private List<float> m_fTableCurveXList = new List<float>();

	private List<float> m_fTableCurveYList = new List<float>();

	private ushort m_wHubIndex;

	private ushort m_wPortIndex;

	private uint m_dwOkFailFlag;

	public string FOLDER_ICON;

	public string TAG_ICON;

	public string EDITABLE_ICON;

	public byte[] m_byEfuseArr = new byte[128];

	public byte[] m_byREfuseArr = new byte[128];

	public byte[] m_byWEfuseBitArr = new byte[1024];

	public byte[] m_byREfuseBitArr = new byte[1024];

	public byte[] m_byAdminREfuseBitArr = new byte[1024];

	private DllDebuggerAPI.S_PreReg sPreReg = default(DllDebuggerAPI.S_PreReg);

	private DllDebuggerAPI.S_PreReg sPreRReg = default(DllDebuggerAPI.S_PreReg);

	private DllDebuggerAPI.S_PreReg sPreCcfgReg = default(DllDebuggerAPI.S_PreReg);

	private float m_fMotorSpeedBase;

	private float m_fMotorVoltageBase;

	private float m_fMotorCurrentBase;

	private byte m_byFlagEllipseStopCnt;

	public byte m_byIsLoadDefault;

	private Thread timeThread;

	private double m_ChartStep = 0.0;

	private string m_srcFilePath = string.Empty;

	private bool m_isDgv1ValueChangedEn = true;

	private bool m_isInitialized = false;

	private ParamStructureDef.S_OnOffTestRecordCfg m_OnOffTestRecordS = default(ParamStructureDef.S_OnOffTestRecordCfg);

	private bool m_isWindowClosing = false;

	private byte m_byPreFaultCode = 0;

	private DrawPolyLine m_hallLaCurveDpl;

	private PointCollection m_hallLaCurveDataSource = new PointCollection();

	private bool m_isAuthorize = false;

	public short m_i16SensorMode = -1;

	private byte m_byCircuitType = 0;

	private bool m_bReload = false;

	public bool m_blIsSave2HallUC = false;

	private Dictionary<string, int> m_nPreviousValueDict = new Dictionary<string, int>();

	public uc_2HallICCoolFan_s1 m_uc2HallICCoolFan_s1;

	public uc_2HallICCoolFan_s2 m_uc2HallICCoolFan_s2;

	public string m_strAppVersion = "1.0.0.83";

	private string m_strSwName = "FT8132";

	private SensorlessUC m_SensorlessUC;

	public UC_2Res_FOC_UVW_AS m_2Res_FOC_UVW_ASUC;

	public UC_2Hall_sensor_2Res_FOC_AS m_2Hall_sensor_2Res_FOC_ASUC;

	private bool m_isQuietMode;

	private bool m_bExeAuto = false;

	private bool m_bExitFlag;

	private bool m_isCmdOpera;

	public AdvancedWindow adWindow;

	private string TITLE_STRING = "ET8132Debugger V1.0.5";

	public int m_i32TotalCnt;

	public int m_i32SuccCnt;

	public int m_i32FailCnt;

	public TextBlock txtOldItem = new TextBlock();

	public TextBlock txtNewItem = new TextBlock();

	private byte m_byPreCtrlMode = 2;

	private int i32Test = 0;

	private byte m_byMotorStatus = 0;

	private byte m_byFaultStatus = 0;

	public ushort m_wRunningSpeed = 0;

	private byte m_byStartTimeDelay = 0;

	private ushort m_wTheta = 0;

	private ushort m_wOmega = 0;

	private ushort m_wUDCVal = 0;

	private ushort m_wWOUTVal = 0;

	private ushort m_wUQVal = 0;

	private ushort m_wUDVal = 0;

	private ushort m_wIA_MaxVal = 0;

	private ushort m_wIB_MaxVal = 0;

	private ushort m_wIC_MaxVal = 0;

	private ushort m_wIA_ARefVal = 0;

	private ushort m_wIB_ARefVal = 0;

	private ushort m_wIC_ARefVal = 0;

	private ushort m_wEmfVal = 0;

	private ushort m_wPwmReal = 0;

	private ushort m_wIQFDBVal = 0;

	private ushort m_wTimArrVal = 0;

	private ushort m_wTrip = 0;

	private ushort m_wPower = 0;

	private ushort m_wHallOmega = 0;

	private ushort m_wRsv = 0;

	private ushort m_wIPDPositionVal = 0;

	private ushort m_wSpdCtrl = 0;

	private bool m_isInCommand;

	private DoubleAnimation da;

	private DoubleAnimation daStatusLed;

	public int i32OptVal;

	private byte m_byDebugFlag = 0;

	private bool m_bFinishedTest = false;

	private bool m_bCanceled = false;

	private ushort m_wDownloadFlag = 0;

	private uint m_dwPassCnt = 0u;

	private uint m_dwFailCnt = 0u;

	private uint m_dwTotalCnt = 0u;

	public ushort m_wTgtSrcVal;

	private string m_strWriteBinFile;

	private byte m_byWriteBufSrcMode;

	private string _strCurPath;

	private DllDebuggerAPI.PFN_GPL32DownloaderCallback pfnStatusCallback = DllDebuggerAPI._DeviceStatusCallback;

	private int i32Flag = 0;

	private bool m_bFindDev = false;

	private ushort wTestIndex = 0;

	private string m_strFilePath;

	private string m_strFailRemark;

	private ushort m_wFailRealCnt = 0;

	private E_BtnOptType m_eBtnOptType;

	private byte m_byOldFaultVal = 0;

	private byte m_byFaultCnt = 0;

	private byte m_byCommunicationStatus = 0;

	private byte m_bySlaveId = 0;

	private ushort m_wMotorFlag = 0;

	private byte m_byTabIndex = 0;

	private byte m_byPowerSel = 0;

	private float m_fHW_RSHUNT;

	private float m_fHW_AMPGAIN;

	private float m_fHW_ADC_REF;

	private byte m_byPreRpdAlignSel = 0;

	private bool m_bStartFlag = true;

	private bool m_bStopFlag = false;

	private CSVHelper g_csvHelper;

	private bool m_blIsChangeVdc = false;

	public ParamStructureDef m_sMotorParamCfg { get; set; }

	public FileScriptParser commonConfigSetting { get; set; }

	public C_DlgItemSetting45 m_clsDlgItemSet45 { get; set; }

	public MainWindow()
	{
		InitializeComponent();
		InitControlEvent();
		int num = 0;
		for (num = 0; num < m_byEfuseArr.Length; num++)
		{
			m_byEfuseArr[num] = 0;
		}
		for (num = 0; num < m_byREfuseArr.Length; num++)
		{
			m_byREfuseArr[num] = 0;
		}
		for (num = 0; num < 1024; num++)
		{
			m_byWEfuseBitArr[num] = 0;
			m_byREfuseBitArr[num] = 0;
			m_byAdminREfuseBitArr[num] = 0;
		}
		m_fMotorSpeedBase = 0f;
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_byIsLoadDefault = 0;
		TAG_ICON = AppDomain.CurrentDomain.BaseDirectory + "\\UIConfigFile\\Images\\Icon\\1.ico";
		FOLDER_ICON = AppDomain.CurrentDomain.BaseDirectory + "\\UIConfigFile\\Images\\Icon\\ft.ico";
		EDITABLE_ICON = AppDomain.CurrentDomain.BaseDirectory + "\\UIConfigFile\\Images\\Icon\\favicon.ico";
		InitUserControl();
		InitOnOffConfig(checkBox_EnNewOnOffTest.IsChecked == true);
		m_dwOkFailFlag = 0u;
		m_wHubIndex = 0;
		m_wPortIndex = 0;
		m_byMotorStatus = 0;
		m_byFaultStatus = 0;
		m_wRunningSpeed = 0;
		m_byFlagEllipseStopCnt = 5;
		sPreReg.byRegisterNum = 32;
		sPreReg.adwRegAdd = new uint[384];
		sPreReg.adwRegVal = new uint[384];
		sPreRReg.byRegisterNum = 32;
		sPreRReg.adwRegAdd = new uint[384];
		sPreRReg.adwRegVal = new uint[384];
		sPreCcfgReg.adwRegAdd = new uint[384];
		sPreCcfgReg.adwRegVal = new uint[384];
		commonConfigSetting = new FileScriptParser();
		m_clsDlgItemSet45 = new C_DlgItemSetting45();
		i32Flag = 1;
		timer = new DispatcherTimer();
		timer.Interval = TimeSpan.FromMilliseconds(200.0);
		timer.Tick += timer_Tick;
		Downloadtimer = new DispatcherTimer();
		Downloadtimer.Interval = TimeSpan.FromMilliseconds(50.0);
		Downloadtimer.Tick += Downloadtimer_Tick;
		OnTimer = new DispatcherTimer();
		OnTimer.Interval = TimeSpan.FromSeconds(3.0);
		OnTimer.Tick += timer_OnTick;
		OutputDebugString("begin on test..\r\n");
		timeThread = new Thread(DispatcherThread);
		OffTimer = new DispatcherTimer();
		OffTimer.Tick += timer_OffTick;
		OnOffTimer = new DispatcherTimer();
		OnOffTimer.Tick += OnOffTimer_Tick;
		readTimer = new DispatcherTimer();
		readTimer.Tick += readTimer_Tick;
		EnumDevice();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		ShowTreeView();
		ShowContent();
		InitUIConfig();
		InitDownloadPage();
		HandleLACurveShow();
		InitStautsDisplay();
		InitDownLoadStautsDisplay(blVal: false);
		bw = new BackgroundWorker();
		bw.DoWork += bw_DoWork;
		bw.ProgressChanged += bw_ProgressChanged;
		bw.RunWorkerCompleted += CompleteWork;
		bw.WorkerReportsProgress = true;
		bw.WorkerSupportsCancellation = true;
		m_isInitialized = true;
		CircuitTypeParamDisplay(m_i16SensorMode, m_byCircuitType);
		if ((Application.Current as App).eArgsLen > 1)
		{
			int num = ExecuteAuto();
			if (num != 0)
			{
				LOG.WriteExecuteResult("Operation failed.", append: true);
			}
			string text = string.Format("Return value: 0x{0}.", (16711680 + num).ToString("X6"));
			Console.WriteLine(text);
			LOG.WriteExecuteResult(text, append: true);
			if (m_bExitFlag)
			{
				Downloadtimer.Stop();
				Exit(16711680 + num);
			}
		}
		SWTitle.Content = m_strSwName;
	}

	public void SetLossPhaseEn()
	{
		radioBtn_LossPhaseDis.IsChecked = true;
		radioBtn_LossPhaseDis.IsEnabled = false;
		radioBtn_LossPhaseEn.IsEnabled = false;
		m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn = 0;
		textBox_LPNorhCur.Text = "0.25";
		textBox_LPNorhCur.IsEnabled = false;
		m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent = float.Parse(textBox_LPNorhCur.Text);
	}

	public void InitControlEvent()
	{
		base.SourceInitialized += win_SourceInitialized;
	}

	public void InitUserControl()
	{
		btnDebug.IsEnabled = false;
		btnStartBw.IsEnabled = false;
		btn_ReadID.IsEnabled = false;
		btn_WriteID.IsEnabled = false;
		m_i32TotalCnt = 0;
		m_i32SuccCnt = 0;
		m_i32FailCnt = 0;
		statusBar1.Items.Clear();
		TextBlock textBlock = new TextBlock();
		textBlock.Text = "Device:";
		textBlock.Width = 50.0;
		statusBar1.Items.Add(textBlock);
		TextBlock textBlock2 = new TextBlock();
		textBlock2.Text = "Disconnected";
		textBlock2.Width = 80.0;
		statusBar1.Items.Add(textBlock2);
		TextBlock textBlock3 = new TextBlock();
		textBlock3.Text = "Elaps Time:";
		textBlock3.Width = 80.0;
		statusBar1.Items.Add(textBlock3);
		TextBlock textBlock4 = new TextBlock();
		textBlock4.Text = "00:00:00";
		textBlock4.Width = 80.0;
		statusBar1.Items.Add(textBlock4);
		Ellipse ellipse = new Ellipse();
		ellipse.HorizontalAlignment = HorizontalAlignment.Right;
		ellipse.VerticalAlignment = VerticalAlignment.Center;
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = Brushes.Gray;
		statusBar1.Items.Add(ellipse);
		TextBlock textBlock5 = new TextBlock();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = 30;
		GetMotorStatusByIndex(stringBuilder, 0);
		textBlock5.Width = 80.0;
		textBlock5.Text = stringBuilder.ToString();
		statusBar1.Items.Add(textBlock5);
		TextBlock textBlock6 = new TextBlock();
		textBlock6.Text = "All rights reserved. © 2021 Fortior Tech.";
		statusBar1.Items.Add(textBlock6);
	}

	private bool InitDownloadPage()
	{
		textBox_TotalCnt.Text = "0";
		textBox_PassedCnt.Text = "0";
		textBox_FailedCnt.Text = "0";
		checkBox_EnableWritingMode.IsChecked = false;
		InitDownloadHShow(checkBox_EnableWritingMode.IsChecked.Value);
		return true;
	}

	private bool InitUIConfig()
	{
		bool result = true;
		int num = 0;
		m_sMotorParamCfg = new ParamStructureDef();
		adWindow = new AdvancedWindow();
		adWindow.m_sMotorParamCfg = m_sMotorParamCfg;
		string currentConfig = Settings.Default.CurrentConfig;
		if (currentConfig.Length > 0)
		{
			if (OpenSpecifyConfig(currentConfig) != 0)
			{
				return LoadDefaultConfig();
			}
			UpdateUserConfigToUI();
		}
		else
		{
			result = LoadDefaultConfig();
		}
		return result;
	}

	private bool ClearUIContent()
	{
		comboBox_FanDaBeiShu.Items.Clear();
		comboBox_SMPMode.Items.Clear();
		comboBox_Seg7En.Items.Clear();
		comboBox_RPDAlign.Items.Clear();
		comboBox_RSDMode.Items.Clear();
		comboBox_ContrlMode.Items.Clear();
		comboBox_FaultRestoreTimes.Items.Clear();
		comboBox_SpdCtrlMD.Items.Clear();
		comboBox_StartClk.Items.Clear();
		comboBox_FocDebug12.Items.Clear();
		comboBox_RsdBrakeEn.Items.Clear();
		comboBox_FgDiv.Items.Clear();
		comboBox_FgMul.Items.Clear();
		comboBox_FgMul.Items.Clear();
		comboBox_FgRdMd.Items.Clear();
		comboBox_SpeedVSet.Items.Clear();
		comboBox_FocDebug34.Items.Clear();
		byte bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
		string text = $"0x{bySlaveId:X2}";
		bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1;
		text = $"0x{bySlaveId:X2}";
		m_clsDlgItemSet45.ClearUIContent();
		comboBox_IdleBrakeEn.Items.Clear();
		adWindow.comboBox_newFunc.Items.Clear();
		adWindow.comboBox_restaEn.Items.Clear();
		adWindow.comboBox_IsdFwMd.Items.Clear();
		adWindow.comboBox_lamd.Items.Clear();
		return true;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern void OutputDebugString(string message);

	public int ExecuteAuto()
	{
		int eArgsLen = (Application.Current as App).eArgsLen;
		IList<string> appArgsList = (Application.Current as App).appArgsList;
		OutputDebugString(eArgsLen.ToString());
		if (eArgsLen <= 1)
		{
			m_isCmdOpera = false;
			return 1;
		}
		m_isCmdOpera = true;
		m_isQuietMode = false;
		m_bExeAuto = false;
		m_bExitFlag = false;
		int num = 0;
		byte result = 2;
		string text = "";
		foreach (string item in appArgsList)
		{
			if (item.StartsWith("-act", StringComparison.OrdinalIgnoreCase))
			{
				string s = item.Replace("-act", "");
				if (!byte.TryParse(s, out result))
				{
					num = 4;
					LOG.WriteExecuteResult("[-act] parameter is incorrect.", append: false);
					return num;
				}
			}
			else if (item.StartsWith("-open", StringComparison.OrdinalIgnoreCase))
			{
				string s = item.Replace("-open", "");
				text = s.Replace("\"", "");
			}
			else if (item.Equals("-quiet", StringComparison.OrdinalIgnoreCase) || item.Equals("-q", StringComparison.OrdinalIgnoreCase))
			{
				m_isQuietMode = true;
				m_bExeAuto = true;
				m_bExitFlag = true;
			}
			else if (item.Equals("-auto", StringComparison.OrdinalIgnoreCase))
			{
				m_bExeAuto = true;
			}
			else if (item.Equals("-exit", StringComparison.OrdinalIgnoreCase))
			{
				m_bExitFlag = true;
			}
		}
		if (num == 0)
		{
			if (m_bExeAuto)
			{
				byte b = 3;
				LogCmdErrorInfo("command operation begin.");
				m_byCommunicationStatus = 1;
				while (b-- > 0)
				{
					btn_ReadID.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					if (m_byCommunicationStatus == 0)
					{
						break;
					}
					LogCmdErrorInfo("find device fail.");
					num = DeviceChangeCheck();
				}
				if (m_byCommunicationStatus == 1)
				{
					num = 5;
					Console.WriteLine("No device.");
					LOG.WriteExecuteResult("No device.", append: false);
					return num;
				}
				Console.WriteLine("Find device.");
				LOG.WriteExecuteResult("Find device.", append: false);
				m_wDownloadFlag = 0;
				switch (result)
				{
				case 0:
					btn_Erase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					if (m_wDownloadFlag == 1)
					{
						num = 0;
						Console.WriteLine("Erase successfully.");
						LOG.WriteExecuteResult("Erase successfully.", append: true);
					}
					else
					{
						num = 11;
						Console.WriteLine("Erase failed.");
						LOG.WriteExecuteResult("Erase failed.", append: true);
					}
					break;
				case 1:
					if (DoOpenOperation(text) == 0)
					{
						m_wTgtSrcVal = 0;
						btn_WriteEfuse.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
						string s = string.Format("FT8132Dbg_CRC: 0x{0}.", m_wTgtSrcVal.ToString("X4"));
						Console.WriteLine(s);
						LOG.WriteExecuteResult(s, append: true);
						if (m_wDownloadFlag == 1)
						{
							num = 0;
							Console.WriteLine("Write successfully.");
							LOG.WriteExecuteResult("Write successfully.", append: true);
						}
						else
						{
							num = 12;
							Console.WriteLine("Write failed.");
							LOG.WriteExecuteResult("Write failed.", append: true);
						}
					}
					else
					{
						num = 2;
						Console.WriteLine("Parse file failed.");
						LOG.WriteExecuteResult("Parse file failed.", append: true);
					}
					break;
				case 2:
					if (DoOpenOperation(text) == 0)
					{
						m_wTgtSrcVal = 0;
						btn_Verify.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
						string s = string.Format("FT8132Dbg_CRC: 0x{0}.", m_wTgtSrcVal.ToString("X4"));
						Console.WriteLine(s);
						LOG.WriteExecuteResult(s, append: true);
						if (m_wDownloadFlag == 1)
						{
							num = 0;
							Console.WriteLine("Verify successfully.");
							LOG.WriteExecuteResult("Verify successfully.", append: true);
						}
						else
						{
							num = 13;
							Console.WriteLine("Verify failed.");
							LOG.WriteExecuteResult("Verify failed.", append: true);
						}
					}
					else
					{
						num = 2;
						Console.WriteLine("Parse file failed.");
						LOG.WriteExecuteResult("Parse file failed.", append: true);
					}
					break;
				}
				if (num != 0)
				{
					return num;
				}
			}
			else
			{
				textBox_CodeFile.Text = text;
			}
		}
		return num;
	}

	private void Exit(int exitCode)
	{
		if (m_bExitFlag)
		{
			Application.Current.Shutdown();
			Environment.Exit(exitCode);
		}
	}

	private int LogCmdErrorInfo(string info)
	{
		int result = 0;
		if (m_isCmdOpera)
		{
			result = LOG.WriteMsg(info);
		}
		return result;
	}

	private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		PropertyNodeItem propertyNodeItem = e.NewValue as PropertyNodeItem;
		if (e.OldValue is PropertyNodeItem propertyNodeItem2)
		{
			txtOldItem.Text = $"'{((propertyNodeItem2 == null) ? string.Empty : propertyNodeItem2.Name)}'";
		}
		txtNewItem.Text = $"'{((propertyNodeItem == null) ? string.Empty : propertyNodeItem.Name)}'";
		if (propertyNodeItem == null || propertyNodeItem.IsGroup || propertyNodeItem.ArticleId >= 0)
		{
		}
		if (m_i16SensorMode == 3)
		{
			ShowContent_2HallICCoolFan();
		}
		else if ((Application.Current as App).eArgsLen > 1 && (m_i16SensorMode == 2 || m_i16SensorMode == 1))
		{
			ShowContent_2HallICCoolFan();
		}
		else
		{
			ShowContent();
		}
	}

	private void ShowContent()
	{
		PropertyNodeItem propertyNodeItem = (PropertyNodeItem)treeView1.SelectedItem;
		if (propertyNodeItem == null)
		{
			tabControl1.SelectedIndex = 0;
		}
		else if (propertyNodeItem.DisplayName == "Setting1" || propertyNodeItem.DisplayName == "Run" || propertyNodeItem.DisplayName == "Startup" || propertyNodeItem.DisplayName == "FOC Parameter" || propertyNodeItem.DisplayName == "Motor & Hardware" || propertyNodeItem.DisplayName == "SensorlessBasic")
		{
			tabControl1.SelectedIndex = 1;
			groupBox_mortorHwParam.Header.ToString();
		}
		else if (propertyNodeItem.DisplayName == "Function" || propertyNodeItem.DisplayName == "RSD" || propertyNodeItem.DisplayName == "RPD & Align" || propertyNodeItem.DisplayName == "FG & RD" || propertyNodeItem.DisplayName == "SpeedCtrl")
		{
			tabControl1.SelectedIndex = 2;
		}
		else if (propertyNodeItem.DisplayName == "Curve" || propertyNodeItem.DisplayName == "CurveCfg1" || propertyNodeItem.DisplayName == "CurveCfg2")
		{
			tabControl1.SelectedIndex = 3;
		}
		else if (propertyNodeItem.DisplayName == "SensorBasic" || propertyNodeItem.DisplayName == "HallParamCfg" || propertyNodeItem.DisplayName == "HallLeadAngleCfg" || propertyNodeItem.DisplayName == "HallLACurve")
		{
			tabControl1.SelectedIndex = 0;
		}
		else if (propertyNodeItem.DisplayName == "Protect&Other" || propertyNodeItem.DisplayName == "Protection" || propertyNodeItem.DisplayName == "Other Cfg")
		{
			tabControl1.SelectedIndex = 4;
		}
		else if (propertyNodeItem.DisplayName == "Display")
		{
			tabControl1.SelectedIndex = 5;
		}
		else if (propertyNodeItem.DisplayName == "OnOff")
		{
			tabControl1.SelectedIndex = 6;
		}
		else
		{
			tabControl1.SelectedIndex = 7;
		}
	}

	private void ShowContent_2HallICCoolFan()
	{
		PropertyNodeItem propertyNodeItem = (PropertyNodeItem)treeView1.SelectedItem;
		if (propertyNodeItem == null)
		{
			tabControl1.SelectedIndex = 0;
		}
		else
		{
			tabControl1.SelectedIndex = propertyNodeItem.ArticleId;
		}
	}

	public static void ExpandAll(TreeView treeView)
	{
		ExpandInternal(treeView);
	}

	private static void ExpandInternal(ItemsControl targetItemContainer)
	{
		if (targetItemContainer == null || targetItemContainer.ItemsSource == null)
		{
			return;
		}
		foreach (object item in targetItemContainer.ItemsSource)
		{
			TreeViewItem treeViewItem = targetItemContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
			if (treeViewItem != null)
			{
				treeViewItem.IsExpanded = true;
				if (treeViewItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
				{
					treeViewItem.UpdateLayout();
				}
			}
			ExpandInternal(treeViewItem);
		}
	}

	private void ShowTreeView()
	{
		List<PropertyNodeItem> list = new List<PropertyNodeItem>();
		PropertyNodeItem propertyNodeItem = new PropertyNodeItem();
		propertyNodeItem.DisplayName = "SensorBasic";
		propertyNodeItem.Name = "This is the setting of hall parameters";
		propertyNodeItem.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\a1.png";
		PropertyNodeItem propertyNodeItem2 = propertyNodeItem;
		PropertyNodeItem propertyNodeItem3 = new PropertyNodeItem();
		propertyNodeItem3.DisplayName = "HallParamCfg";
		propertyNodeItem3.Name = "It is the config of some hall parameters";
		propertyNodeItem3.Icon = TAG_ICON;
		PropertyNodeItem item = propertyNodeItem3;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem4 = new PropertyNodeItem();
		propertyNodeItem4.DisplayName = "HallLeadAngleCfg";
		propertyNodeItem4.Name = "It is the config of some hall parameters";
		propertyNodeItem4.Icon = TAG_ICON;
		PropertyNodeItem item2 = propertyNodeItem4;
		propertyNodeItem2.Children.Add(item2);
		PropertyNodeItem propertyNodeItem5 = new PropertyNodeItem();
		propertyNodeItem5.DisplayName = "HallLACurve";
		propertyNodeItem5.Name = "It is the config of some hall parameters";
		propertyNodeItem5.Icon = TAG_ICON;
		PropertyNodeItem item3 = propertyNodeItem5;
		propertyNodeItem2.Children.Add(item3);
		list.Add(propertyNodeItem2);
		PropertyNodeItem propertyNodeItem6 = new PropertyNodeItem();
		propertyNodeItem6.DisplayName = "SensorlessBasic";
		propertyNodeItem6.Name = "This is the setting of basic parameters";
		propertyNodeItem6.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\a1.png";
		PropertyNodeItem propertyNodeItem7 = propertyNodeItem6;
		PropertyNodeItem propertyNodeItem8 = new PropertyNodeItem();
		propertyNodeItem8.DisplayName = "Motor & Hardware";
		propertyNodeItem8.Name = "It is the config of motor parameters.";
		propertyNodeItem8.Icon = TAG_ICON;
		PropertyNodeItem item4 = propertyNodeItem8;
		propertyNodeItem7.Children.Add(item4);
		PropertyNodeItem propertyNodeItem9 = new PropertyNodeItem();
		propertyNodeItem9.DisplayName = "FOC Parameter";
		propertyNodeItem9.Name = "It is the config of FOC parameters";
		propertyNodeItem9.Icon = TAG_ICON;
		PropertyNodeItem item5 = propertyNodeItem9;
		propertyNodeItem7.Children.Add(item5);
		PropertyNodeItem propertyNodeItem10 = new PropertyNodeItem();
		propertyNodeItem10.DisplayName = "Startup";
		propertyNodeItem10.Name = "It is the config of start current";
		propertyNodeItem10.Icon = TAG_ICON;
		PropertyNodeItem item6 = propertyNodeItem10;
		propertyNodeItem7.Children.Add(item6);
		PropertyNodeItem propertyNodeItem11 = new PropertyNodeItem();
		propertyNodeItem11.DisplayName = "Run";
		propertyNodeItem11.Name = "It is the config of running";
		propertyNodeItem11.Icon = TAG_ICON;
		PropertyNodeItem item7 = propertyNodeItem11;
		propertyNodeItem7.Children.Add(item7);
		list.Add(propertyNodeItem7);
		PropertyNodeItem propertyNodeItem12 = new PropertyNodeItem();
		propertyNodeItem12.DisplayName = "Function";
		propertyNodeItem12.Name = "This is the setting of advanced parameters";
		propertyNodeItem12.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\b1.png";
		PropertyNodeItem propertyNodeItem13 = propertyNodeItem12;
		PropertyNodeItem propertyNodeItem14 = new PropertyNodeItem();
		propertyNodeItem14.DisplayName = "RPD & Align";
		propertyNodeItem14.Name = "It is the config of align";
		propertyNodeItem14.Icon = TAG_ICON;
		PropertyNodeItem item8 = propertyNodeItem14;
		propertyNodeItem13.Children.Add(item8);
		PropertyNodeItem propertyNodeItem15 = new PropertyNodeItem();
		propertyNodeItem15.DisplayName = "RSD";
		propertyNodeItem15.Name = "It is the config of Wind parameter";
		propertyNodeItem15.Icon = TAG_ICON;
		PropertyNodeItem item9 = propertyNodeItem15;
		propertyNodeItem13.Children.Add(item9);
		PropertyNodeItem propertyNodeItem16 = new PropertyNodeItem();
		propertyNodeItem16.DisplayName = "FG & RD";
		propertyNodeItem16.Name = "It is the config of protection parameters";
		propertyNodeItem16.Icon = TAG_ICON;
		PropertyNodeItem item10 = propertyNodeItem16;
		propertyNodeItem13.Children.Add(item10);
		PropertyNodeItem propertyNodeItem17 = new PropertyNodeItem();
		propertyNodeItem17.DisplayName = "SpeedCtrl";
		propertyNodeItem17.Name = "It is the config of others control info";
		propertyNodeItem17.Icon = TAG_ICON;
		PropertyNodeItem item11 = propertyNodeItem17;
		propertyNodeItem13.Children.Add(item11);
		list.Add(propertyNodeItem13);
		PropertyNodeItem propertyNodeItem18 = new PropertyNodeItem();
		propertyNodeItem18.DisplayName = "Curve";
		propertyNodeItem18.Name = "This is the setting of extend & curve parameters";
		propertyNodeItem18.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		PropertyNodeItem propertyNodeItem19 = propertyNodeItem18;
		PropertyNodeItem propertyNodeItem20 = new PropertyNodeItem();
		propertyNodeItem20.DisplayName = "CurveCfg1";
		propertyNodeItem20.Name = "It is the config of some extend parameters";
		propertyNodeItem20.Icon = TAG_ICON;
		PropertyNodeItem item12 = propertyNodeItem20;
		propertyNodeItem19.Children.Add(item12);
		PropertyNodeItem propertyNodeItem21 = new PropertyNodeItem();
		propertyNodeItem21.DisplayName = "CurveCfg2";
		propertyNodeItem21.Name = "It is the config of curve parameter";
		propertyNodeItem21.Icon = TAG_ICON;
		PropertyNodeItem item13 = propertyNodeItem21;
		propertyNodeItem19.Children.Add(item13);
		list.Add(propertyNodeItem19);
		PropertyNodeItem propertyNodeItem22 = new PropertyNodeItem();
		propertyNodeItem22.DisplayName = "Protect&Other";
		propertyNodeItem22.Name = "This is the setting of hall other parameters";
		propertyNodeItem22.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\c1.png";
		PropertyNodeItem item14 = propertyNodeItem22;
		list.Add(item14);
		PropertyNodeItem propertyNodeItem23 = new PropertyNodeItem();
		propertyNodeItem23.DisplayName = "Display";
		propertyNodeItem23.Name = "It shows motor status";
		propertyNodeItem23.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		PropertyNodeItem item15 = propertyNodeItem23;
		list.Add(item15);
		PropertyNodeItem propertyNodeItem24 = new PropertyNodeItem();
		propertyNodeItem24.DisplayName = "OnOff";
		propertyNodeItem24.Name = "It shows how to test motor performance";
		propertyNodeItem24.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\e1.png";
		PropertyNodeItem item16 = propertyNodeItem24;
		list.Add(item16);
		PropertyNodeItem propertyNodeItem25 = new PropertyNodeItem();
		propertyNodeItem25.DisplayName = "Download";
		propertyNodeItem25.Name = "It shows how to download and verify efuse";
		propertyNodeItem25.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\f1.png";
		PropertyNodeItem item17 = propertyNodeItem25;
		list.Add(item17);
		treeView1.ItemsSource = list;
	}

	private void ShowTreeView_2HIC_CoolFan()
	{
		List<PropertyNodeItem> list = new List<PropertyNodeItem>();
		PropertyNodeItem propertyNodeItem = new PropertyNodeItem();
		propertyNodeItem.DisplayName = "Sensor & Sensorless";
		propertyNodeItem.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\a1.png";
		propertyNodeItem.ArticleId = 0;
		PropertyNodeItem propertyNodeItem2 = propertyNodeItem;
		PropertyNodeItem propertyNodeItem3 = new PropertyNodeItem();
		propertyNodeItem3.DisplayName = "HallParamCfg";
		propertyNodeItem3.Icon = TAG_ICON;
		propertyNodeItem3.ArticleId = 0;
		PropertyNodeItem item = propertyNodeItem3;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem4 = new PropertyNodeItem();
		propertyNodeItem4.DisplayName = "Motor & Hardware";
		propertyNodeItem4.Icon = TAG_ICON;
		propertyNodeItem4.ArticleId = 0;
		item = propertyNodeItem4;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem5 = new PropertyNodeItem();
		propertyNodeItem5.DisplayName = "FOC Parameter";
		propertyNodeItem5.Icon = TAG_ICON;
		propertyNodeItem5.ArticleId = 0;
		item = propertyNodeItem5;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem6 = new PropertyNodeItem();
		propertyNodeItem6.DisplayName = "Startup";
		propertyNodeItem6.Icon = TAG_ICON;
		propertyNodeItem6.ArticleId = 0;
		item = propertyNodeItem6;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem7 = new PropertyNodeItem();
		propertyNodeItem7.DisplayName = "Run";
		propertyNodeItem7.Icon = TAG_ICON;
		propertyNodeItem7.ArticleId = 0;
		item = propertyNodeItem7;
		propertyNodeItem2.Children.Add(item);
		list.Add(propertyNodeItem2);
		PropertyNodeItem propertyNodeItem8 = new PropertyNodeItem();
		propertyNodeItem8.DisplayName = "Function & Protect & Other";
		propertyNodeItem8.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\b1.png";
		propertyNodeItem8.ArticleId = 1;
		PropertyNodeItem propertyNodeItem9 = propertyNodeItem8;
		PropertyNodeItem propertyNodeItem10 = new PropertyNodeItem();
		propertyNodeItem10.DisplayName = "RSD";
		propertyNodeItem10.Icon = TAG_ICON;
		propertyNodeItem10.ArticleId = 1;
		PropertyNodeItem item2 = propertyNodeItem10;
		propertyNodeItem9.Children.Add(item2);
		PropertyNodeItem propertyNodeItem11 = new PropertyNodeItem();
		propertyNodeItem11.DisplayName = "FG & RD";
		propertyNodeItem11.Icon = TAG_ICON;
		propertyNodeItem11.ArticleId = 1;
		item2 = propertyNodeItem11;
		propertyNodeItem9.Children.Add(item2);
		PropertyNodeItem propertyNodeItem12 = new PropertyNodeItem();
		propertyNodeItem12.DisplayName = "SpeedCtrl";
		propertyNodeItem12.Icon = TAG_ICON;
		propertyNodeItem12.ArticleId = 1;
		item2 = propertyNodeItem12;
		propertyNodeItem9.Children.Add(item2);
		PropertyNodeItem propertyNodeItem13 = new PropertyNodeItem();
		propertyNodeItem13.DisplayName = "Protection";
		propertyNodeItem13.Icon = TAG_ICON;
		propertyNodeItem13.ArticleId = 1;
		item2 = propertyNodeItem13;
		propertyNodeItem9.Children.Add(item2);
		PropertyNodeItem propertyNodeItem14 = new PropertyNodeItem();
		propertyNodeItem14.DisplayName = "Other Cfg";
		propertyNodeItem14.Icon = TAG_ICON;
		propertyNodeItem14.ArticleId = 1;
		item2 = propertyNodeItem14;
		propertyNodeItem9.Children.Add(item2);
		list.Add(propertyNodeItem9);
		PropertyNodeItem propertyNodeItem15 = new PropertyNodeItem();
		propertyNodeItem15.DisplayName = "Curve";
		propertyNodeItem15.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem15.ArticleId = 2;
		PropertyNodeItem item3 = propertyNodeItem15;
		list.Add(item3);
		PropertyNodeItem propertyNodeItem16 = new PropertyNodeItem();
		propertyNodeItem16.DisplayName = "Display";
		propertyNodeItem16.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem16.ArticleId = 3;
		PropertyNodeItem item4 = propertyNodeItem16;
		list.Add(item4);
		PropertyNodeItem propertyNodeItem17 = new PropertyNodeItem();
		propertyNodeItem17.DisplayName = "Download";
		propertyNodeItem17.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\f1.png";
		propertyNodeItem17.ArticleId = 4;
		PropertyNodeItem item5 = propertyNodeItem17;
		list.Add(item5);
		treeView1.ItemsSource = list;
	}

	private void ShowTreeView_RoboticVacuum()
	{
		List<PropertyNodeItem> list = new List<PropertyNodeItem>();
		PropertyNodeItem propertyNodeItem = new PropertyNodeItem();
		propertyNodeItem.DisplayName = "Sensorless Function";
		propertyNodeItem.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\a1.png";
		propertyNodeItem.ArticleId = 0;
		PropertyNodeItem propertyNodeItem2 = propertyNodeItem;
		PropertyNodeItem propertyNodeItem3 = new PropertyNodeItem();
		propertyNodeItem3.DisplayName = "Motor & Hardware";
		propertyNodeItem3.Icon = TAG_ICON;
		propertyNodeItem3.ArticleId = 0;
		PropertyNodeItem item = propertyNodeItem3;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem4 = new PropertyNodeItem();
		propertyNodeItem4.DisplayName = "FOC & FG & RSD";
		propertyNodeItem4.Icon = TAG_ICON;
		propertyNodeItem4.ArticleId = 0;
		item = propertyNodeItem4;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem5 = new PropertyNodeItem();
		propertyNodeItem5.DisplayName = "Startup";
		propertyNodeItem5.Icon = TAG_ICON;
		propertyNodeItem5.ArticleId = 0;
		item = propertyNodeItem5;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem6 = new PropertyNodeItem();
		propertyNodeItem6.DisplayName = "Run";
		propertyNodeItem6.Icon = TAG_ICON;
		propertyNodeItem6.ArticleId = 0;
		item = propertyNodeItem6;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem7 = new PropertyNodeItem();
		propertyNodeItem7.DisplayName = "Protection & Other";
		propertyNodeItem7.Icon = TAG_ICON;
		propertyNodeItem7.ArticleId = 0;
		item = propertyNodeItem7;
		propertyNodeItem2.Children.Add(item);
		list.Add(propertyNodeItem2);
		PropertyNodeItem propertyNodeItem8 = new PropertyNodeItem();
		propertyNodeItem8.DisplayName = "Curve";
		propertyNodeItem8.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem8.ArticleId = 1;
		PropertyNodeItem propertyNodeItem9 = propertyNodeItem8;
		PropertyNodeItem propertyNodeItem10 = new PropertyNodeItem();
		propertyNodeItem10.DisplayName = "CurveCfg1";
		propertyNodeItem10.Icon = TAG_ICON;
		propertyNodeItem10.ArticleId = 1;
		PropertyNodeItem item2 = propertyNodeItem10;
		propertyNodeItem9.Children.Add(item2);
		PropertyNodeItem propertyNodeItem11 = new PropertyNodeItem();
		propertyNodeItem11.DisplayName = "CurveCfg2";
		propertyNodeItem11.Icon = TAG_ICON;
		propertyNodeItem11.ArticleId = 1;
		item2 = propertyNodeItem11;
		propertyNodeItem9.Children.Add(item2);
		PropertyNodeItem propertyNodeItem12 = new PropertyNodeItem();
		propertyNodeItem12.DisplayName = "Curve";
		propertyNodeItem12.Icon = TAG_ICON;
		propertyNodeItem12.ArticleId = 1;
		item2 = propertyNodeItem12;
		propertyNodeItem9.Children.Add(item2);
		list.Add(propertyNodeItem9);
		PropertyNodeItem propertyNodeItem13 = new PropertyNodeItem();
		propertyNodeItem13.DisplayName = "Display";
		propertyNodeItem13.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem13.ArticleId = 2;
		PropertyNodeItem item3 = propertyNodeItem13;
		list.Add(item3);
		PropertyNodeItem propertyNodeItem14 = new PropertyNodeItem();
		propertyNodeItem14.DisplayName = "Download";
		propertyNodeItem14.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\f1.png";
		propertyNodeItem14.ArticleId = 3;
		PropertyNodeItem item4 = propertyNodeItem14;
		list.Add(item4);
		treeView1.ItemsSource = list;
	}

	private void ShowTreeView_2Res_Foc_UVW_AS()
	{
		List<PropertyNodeItem> list = new List<PropertyNodeItem>();
		PropertyNodeItem propertyNodeItem = new PropertyNodeItem();
		propertyNodeItem.DisplayName = "Sensorless Function";
		propertyNodeItem.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\a1.png";
		propertyNodeItem.ArticleId = 0;
		PropertyNodeItem propertyNodeItem2 = propertyNodeItem;
		PropertyNodeItem propertyNodeItem3 = new PropertyNodeItem();
		propertyNodeItem3.DisplayName = "Motor & Hardware";
		propertyNodeItem3.Icon = TAG_ICON;
		propertyNodeItem3.ArticleId = 0;
		PropertyNodeItem item = propertyNodeItem3;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem4 = new PropertyNodeItem();
		propertyNodeItem4.DisplayName = "FOC & RSD & FG";
		propertyNodeItem4.Icon = TAG_ICON;
		propertyNodeItem4.ArticleId = 0;
		item = propertyNodeItem4;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem5 = new PropertyNodeItem();
		propertyNodeItem5.DisplayName = "Protection";
		propertyNodeItem5.Icon = TAG_ICON;
		propertyNodeItem5.ArticleId = 0;
		item = propertyNodeItem5;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem6 = new PropertyNodeItem();
		propertyNodeItem6.DisplayName = "Startup";
		propertyNodeItem6.Icon = TAG_ICON;
		propertyNodeItem6.ArticleId = 0;
		item = propertyNodeItem6;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem7 = new PropertyNodeItem();
		propertyNodeItem7.DisplayName = "Run";
		propertyNodeItem7.Icon = TAG_ICON;
		propertyNodeItem7.ArticleId = 0;
		item = propertyNodeItem7;
		propertyNodeItem2.Children.Add(item);
		list.Add(propertyNodeItem2);
		PropertyNodeItem propertyNodeItem8 = new PropertyNodeItem();
		propertyNodeItem8.DisplayName = "Curve";
		propertyNodeItem8.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem8.ArticleId = 1;
		PropertyNodeItem item2 = propertyNodeItem8;
		list.Add(item2);
		PropertyNodeItem propertyNodeItem9 = new PropertyNodeItem();
		propertyNodeItem9.DisplayName = "Display";
		propertyNodeItem9.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem9.ArticleId = 2;
		PropertyNodeItem item3 = propertyNodeItem9;
		list.Add(item3);
		PropertyNodeItem propertyNodeItem10 = new PropertyNodeItem();
		propertyNodeItem10.DisplayName = "Download";
		propertyNodeItem10.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\f1.png";
		propertyNodeItem10.ArticleId = 3;
		PropertyNodeItem item4 = propertyNodeItem10;
		list.Add(item4);
		treeView1.ItemsSource = list;
	}

	private void ShowTreeView_2HallSensor_2Res_Foc_AS()
	{
		List<PropertyNodeItem> list = new List<PropertyNodeItem>();
		PropertyNodeItem propertyNodeItem = new PropertyNodeItem();
		propertyNodeItem.DisplayName = "Sensor Function";
		propertyNodeItem.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\a1.png";
		propertyNodeItem.ArticleId = 0;
		PropertyNodeItem propertyNodeItem2 = propertyNodeItem;
		PropertyNodeItem propertyNodeItem3 = new PropertyNodeItem();
		propertyNodeItem3.DisplayName = "HallParamCfg";
		propertyNodeItem3.Icon = TAG_ICON;
		propertyNodeItem3.ArticleId = 0;
		PropertyNodeItem item = propertyNodeItem3;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem4 = new PropertyNodeItem();
		propertyNodeItem4.DisplayName = "Motor &amp; Hardware";
		propertyNodeItem4.Icon = TAG_ICON;
		propertyNodeItem4.ArticleId = 0;
		item = propertyNodeItem4;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem5 = new PropertyNodeItem();
		propertyNodeItem5.DisplayName = "FOC &amp; RSD";
		propertyNodeItem5.Icon = TAG_ICON;
		propertyNodeItem5.ArticleId = 0;
		item = propertyNodeItem5;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem6 = new PropertyNodeItem();
		propertyNodeItem6.DisplayName = "Run &amp; Startup";
		propertyNodeItem6.Icon = TAG_ICON;
		propertyNodeItem6.ArticleId = 0;
		item = propertyNodeItem6;
		propertyNodeItem2.Children.Add(item);
		PropertyNodeItem propertyNodeItem7 = new PropertyNodeItem();
		propertyNodeItem7.DisplayName = "FG &amp; Protection";
		propertyNodeItem7.Icon = TAG_ICON;
		propertyNodeItem7.ArticleId = 0;
		item = propertyNodeItem7;
		propertyNodeItem2.Children.Add(item);
		list.Add(propertyNodeItem2);
		PropertyNodeItem propertyNodeItem8 = new PropertyNodeItem();
		propertyNodeItem8.DisplayName = "Curve";
		propertyNodeItem8.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem8.ArticleId = 1;
		PropertyNodeItem item2 = propertyNodeItem8;
		list.Add(item2);
		PropertyNodeItem propertyNodeItem9 = new PropertyNodeItem();
		propertyNodeItem9.DisplayName = "Display";
		propertyNodeItem9.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\d1.png";
		propertyNodeItem9.ArticleId = 2;
		PropertyNodeItem item3 = propertyNodeItem9;
		list.Add(item3);
		PropertyNodeItem propertyNodeItem10 = new PropertyNodeItem();
		propertyNodeItem10.DisplayName = "Download";
		propertyNodeItem10.Icon = Environment.CurrentDirectory + "\\UIConfigFile\\Images\\png\\f1.png";
		propertyNodeItem10.ArticleId = 3;
		PropertyNodeItem item4 = propertyNodeItem10;
		list.Add(item4);
		treeView1.ItemsSource = list;
	}

	public int Init2DefaultConfig()
	{
		m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu = 2;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu = 0.37183f;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan = 9.52E-05f;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi = 0.21435f;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu = 0.1f;
		m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu = 0;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fRV = 12f;
		m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj = 0;
		m_sMotorParamCfg.m_sFocParamCfg.bySMPMode = 3;
		m_sMotorParamCfg.m_sFocParamCfg.byStaTsam0En = 0;
		m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn = 1;
		m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn = 0;
		m_sMotorParamCfg.m_sFocParamCfg.bySeg7En = 1;
		m_sMotorParamCfg.m_sFocParamCfg.bySta7SegEn = 0;
		m_sMotorParamCfg.m_sFocParamCfg.byAtanEn = 1;
		m_sMotorParamCfg.m_sFocParamCfg.byStaSin = 0;
		m_sMotorParamCfg.m_sFocParamCfg.wFreqBase = 1600;
		m_sMotorParamCfg.m_sFocParamCfg.bySampleTime = 0;
		m_sMotorParamCfg.m_sFocParamCfg.bySpdBw = 2;
		m_sMotorParamCfg.m_sFocParamCfg.byEbwMode = 0;
		m_sMotorParamCfg.m_sFocParamCfg.fLeadAngleSet = 0f;
		m_sMotorParamCfg.m_sFocParamCfg.byEkNum = 0;
		m_sMotorParamCfg.m_sFocParamCfg.wEbw = 3200;
		m_sMotorParamCfg.m_sFocParamCfg.fAtoBw = 50f;
		m_sMotorParamCfg.m_sFocParamCfg.byTSMIN = 0;
		m_sMotorParamCfg.m_sFocParamCfg.fDTSEL = 0.5f;
		m_sMotorParamCfg.m_sFocParamCfg.byKSILDE = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn = 1;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10 = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11 = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD12 = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20 = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn = 0;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21 = 0;
		m_sMotorParamCfg.m_sStartCfg.byControlMode = 2;
		m_sMotorParamCfg.m_sStartCfg.fStartCurrent = 0.393f;
		m_sMotorParamCfg.m_sStartCfg.byFocEfreqAccx = 10;
		m_sMotorParamCfg.m_sStartCfg.fFocEfreqMinx = 0.6f;
		m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold = 1;
		m_sMotorParamCfg.m_sStartCfg.byCloSpeed = 0;
		m_sMotorParamCfg.m_sStartCfg.byStartDuty = 0;
		m_sMotorParamCfg.m_sRunCfg.byStaIncTime = 3;
		m_sMotorParamCfg.m_sRunCfg.byStbInc = 4;
		m_sMotorParamCfg.m_sRunCfg.byStaInc = 3;
		m_sMotorParamCfg.m_sRunCfg.fDqKp = 0.999f;
		m_sMotorParamCfg.m_sRunCfg.fDqKi = 0.0035f;
		m_sMotorParamCfg.m_sRunCfg.fWKp = 0.999f;
		m_sMotorParamCfg.m_sRunCfg.fWKi = 0.0035f;
		m_sMotorParamCfg.m_sRunCfg.byWoutMax = 40;
		m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys = 0;
		m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk = 0;
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdEn = 1;
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode = 0;
		m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed = 1.0;
		m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed = 7;
		m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed = 3;
		m_sMotorParamCfg.m_sWindHandleCfg.byRSD230 = 0;
		m_sMotorParamCfg.m_sWindHandleCfg.byRSD231 = 0;
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdHallFilter = 0;
		m_sMotorParamCfg.m_sWindHandleCfg.byIsdBrakeEn = 0;
		m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes = 0;
		m_sMotorParamCfg.m_sProtectCfg.byLock0 = 1;
		m_sMotorParamCfg.m_sProtectCfg.byLock1 = 1;
		m_sMotorParamCfg.m_sProtectCfg.byLock2 = 1;
		m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn = 0;
		m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed = 200u;
		m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed = 34000u;
		m_sMotorParamCfg.m_sProtectCfg.byLockEmfThd = 3;
		m_sMotorParamCfg.m_sProtectCfg.fLockEmf = 2f;
		m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent = 0.25f;
		m_sMotorParamCfg.m_sProtectCfg.byUvloDis = 0;
		m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd = 2;
		m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 0;
		m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 0;
		m_sMotorParamCfg.m_sOhterCfg.byQuickSta = 0;
		m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 0;
		m_sMotorParamCfg.m_sOhterCfg.byFgMul = 0;
		m_sMotorParamCfg.m_sOhterCfg.byFgRdMd = 0;
		m_sMotorParamCfg.m_sOhterCfg.byFgDiv = 0;
		m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet = 0;
		m_sMotorParamCfg.m_sOhterCfg.byFgSet = 1;
		m_sMotorParamCfg.m_sOhterCfg.byFgRd = 0;
		m_sMotorParamCfg.m_sOhterCfg.byStartClk = 0;
		m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet = 0;
		m_sMotorParamCfg.m_sExtendCfg.byVRefSel = 1;
		m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel = 0;
		m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed = 33000u;
		m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet = 0;
		string value = "0x00";
		byte b = Convert.ToByte(value, 16);
		m_sMotorParamCfg.m_sTargetSpdCurve.byXON0 = 0;
		m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON = 0;
		m_sMotorParamCfg.m_sTargetSpdCurve.byPreCtrlMode = 2;
		m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed = 500u;
		m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed = 32000u;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent = 0.1f;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent = 6f;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower = 0.1f;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower = 8f;
		m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel = 0;
		m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 0;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON = 0;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_25 = 8;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_50 = 16;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_75 = 16;
		string value2 = "0x00";
		byte bySlaveId = Convert.ToByte(value2, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = bySlaveId;
		value2 = "0xff";
		bySlaveId = Convert.ToByte(value2, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1 = bySlaveId;
		value2 = "50";
		bySlaveId = Convert.ToByte(value2, 10);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = bySlaveId;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpiDebug = 0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd = 0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = 0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn = 0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0 = 0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1 = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOcpSel = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdDirRev = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOvpSel = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTsdSel = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDzSet = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDpmDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byUvloDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOvpDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOcpDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTsdDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFocDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byBootMod = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdBrakeEn = 1;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTSMin = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRvsAlignEn = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRvsCutSel = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byNewSingle = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDigTst = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDigMoniCt = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.fRV = 10f;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTestMode = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIrefDis = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byHallOvMd = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byHallLttEn = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstQMax = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstInc = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstEn = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDrvDisMod = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byCmp0CkSel = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd = 0;
		m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = 1;
		m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn = 0;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty = 100;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration = 5;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty = 0;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration = 10;
		m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt = 100;
		m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt = 0;
		m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt = 0;
		m_clsDlgItemSet45.Init2DefaultConfig();
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Curr_Base = "0";
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Vol_Base2 = "0";
		m_sMotorParamCfg.m_sShowBASECfg.strMotor_Power_Base = "0";
		return 0;
	}

	public byte CompareVersion(string strFileVersion, string strCurVersion)
	{
		byte b = 0;
		strFileVersion = strFileVersion.Replace(".", "");
		strCurVersion = strCurVersion.Replace(".", "");
		int num = int.Parse(strFileVersion);
		int num2 = int.Parse(strCurVersion);
		int num3 = num - num2;
		if (num3 < 0)
		{
			return 1;
		}
		return 0;
	}

	public byte StartCurrentToDuty()
	{
		byte b = 0;
		byte b2 = 0;
		byte byFanDaBeiShu = m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu;
		float ampGain = GetAmpGain(byFanDaBeiShu);
		float fCaiYangDDianZu = m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu;
		byte byVRefSel = m_sMotorParamCfg.m_sExtendCfg.byVRefSel;
		float adcRefVoltage = GetAdcRefVoltage(byVRefSel);
		m_fHW_RSHUNT = fCaiYangDDianZu;
		m_fHW_AMPGAIN = ampGain;
		m_fHW_ADC_REF = adcRefVoltage;
		float fStartCurrent = m_sMotorParamCfg.m_sStartCfg.fStartCurrent;
		uint iValue = GetIValue(fStartCurrent, 15);
		b2 = (byte)((iValue >> 9) & 0x1F);
		if (b2 > 31)
		{
			b2 = 31;
		}
		double value = Math.Round((double)(int)b2 * 50.0 / 31.0, 0);
		b = Convert.ToByte(value);
		m_sMotorParamCfg.m_sStartCfg.byStartDuty = b;
		return b;
	}

	public int OpenSpecifyConfig(string filename)
	{
		int num = 0;
		num = commonConfigSetting.LoadConfig(filename);
		if (num != 0)
		{
			return num;
		}
		string strSwName = commonConfigSetting.m_strSwName;
		string sAppVersion = commonConfigSetting.m_sAppVersion;
		sAppVersion = sAppVersion.Replace(".", "");
		if (strSwName != "" && !m_strSwName.Equals(strSwName))
		{
			return 3;
		}
		if (m_i16SensorMode != commonConfigSetting.m_i16SensorMode || m_byCircuitType != commonConfigSetting.m_byCircuitType)
		{
			return 2;
		}
		m_sMotorParamCfg.m_sAppStartupCfg.i16SensorMode = commonConfigSetting.m_i16SensorMode;
		m_sMotorParamCfg.m_sAppStartupCfg.byCircuitType = commonConfigSetting.m_byCircuitType;
		m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu = commonConfigSetting.m_byPolePairs;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu = commonConfigSetting.m_fRS;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan = commonConfigSetting.m_fLQ;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi = commonConfigSetting.m_fKe;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu = commonConfigSetting.m_fHwRSHunt;
		m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu = commonConfigSetting.m_byHwAmpGain;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fRV = commonConfigSetting.m_fRV;
		m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj = commonConfigSetting.m_byVdcAdj;
		m_sMotorParamCfg.m_sFocParamCfg.bySMPMode = commonConfigSetting.m_bySMPMode;
		m_sMotorParamCfg.m_sFocParamCfg.byStaTsam0En = commonConfigSetting.m_byStaTsam0En;
		m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn = commonConfigSetting.m_byPllSmoEn;
		m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn = commonConfigSetting.m_byStaPllEn;
		m_sMotorParamCfg.m_sFocParamCfg.bySeg7En = commonConfigSetting.m_bySeg7En;
		m_sMotorParamCfg.m_sFocParamCfg.bySta7SegEn = commonConfigSetting.m_bySta7SegEn;
		m_sMotorParamCfg.m_sFocParamCfg.byAtanEn = commonConfigSetting.m_byAtanEn;
		m_sMotorParamCfg.m_sFocParamCfg.byStaSin = commonConfigSetting.m_byStaSin;
		m_sMotorParamCfg.m_sFocParamCfg.wFreqBase = commonConfigSetting.m_wFreqBase;
		m_sMotorParamCfg.m_sFocParamCfg.bySampleTime = commonConfigSetting.m_bySampleTime;
		m_sMotorParamCfg.m_sFocParamCfg.bySpdBw = commonConfigSetting.m_bySpdBw;
		m_sMotorParamCfg.m_sFocParamCfg.byEbwMode = commonConfigSetting.m_byEbwMode;
		m_sMotorParamCfg.m_sFocParamCfg.fLeadAngleSet = commonConfigSetting.m_fLeadAngleSet;
		m_sMotorParamCfg.m_sFocParamCfg.byEkNum = commonConfigSetting.m_byEkNum;
		m_sMotorParamCfg.m_sFocParamCfg.wEbw = commonConfigSetting.m_wEbw;
		m_sMotorParamCfg.m_sFocParamCfg.fAtoBw = commonConfigSetting.m_fAtoBw;
		m_sMotorParamCfg.m_sFocParamCfg.byTSMIN = commonConfigSetting.m_byTSMIN;
		m_sMotorParamCfg.m_sFocParamCfg.fDTSEL = commonConfigSetting.m_fDTSEL;
		m_sMotorParamCfg.m_sFocParamCfg.byKSILDE = commonConfigSetting.m_byKSILDE;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn = commonConfigSetting.m_byRpdAlignEn;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime = commonConfigSetting.m_byAlignTime;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty = commonConfigSetting.m_byAlignDuty;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10 = commonConfigSetting.m_byRPD10;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11 = commonConfigSetting.m_byRPD11;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD123_2 = commonConfigSetting.m_byRPD12;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20 = commonConfigSetting.m_byRPD20;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn = commonConfigSetting.m_byRPDRvsAlignEn;
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21 = commonConfigSetting.m_byRPD21;
		m_sMotorParamCfg.m_sStartCfg.byControlMode = commonConfigSetting.m_byControlMode;
		m_sMotorParamCfg.m_sStartCfg.fStartCurrent = commonConfigSetting.m_fStartCurrent;
		m_sMotorParamCfg.m_sStartCfg.byFocEfreqAccx = commonConfigSetting.m_byFocEfreqAcc;
		m_sMotorParamCfg.m_sStartCfg.fFocEfreqMinx = commonConfigSetting.m_fFocEfreqMin;
		m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold = commonConfigSetting.m_byFocEfreqHold;
		m_sMotorParamCfg.m_sStartCfg.byCloSpeed = commonConfigSetting.m_byCloSpeed;
		m_sMotorParamCfg.m_sRunCfg.byStaIncTime = commonConfigSetting.m_byStaIncTime;
		m_sMotorParamCfg.m_sRunCfg.byStbInc = commonConfigSetting.m_byStbInc;
		m_sMotorParamCfg.m_sRunCfg.byStaInc = commonConfigSetting.m_byStaInc;
		m_sMotorParamCfg.m_sRunCfg.fDqKp = commonConfigSetting.m_fDqKp;
		m_sMotorParamCfg.m_sRunCfg.fDqKi = commonConfigSetting.m_fDqKi;
		m_sMotorParamCfg.m_sRunCfg.fWKp = commonConfigSetting.m_fWKp;
		m_sMotorParamCfg.m_sRunCfg.fWKi = commonConfigSetting.m_fWKi;
		m_sMotorParamCfg.m_sRunCfg.byWoutMax = commonConfigSetting.m_byWoutMax;
		m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys = commonConfigSetting.m_byPwmOnoffhys;
		m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk = commonConfigSetting.m_byPwmOnoffhysClk;
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdEn = commonConfigSetting.m_byRsdEn;
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode = commonConfigSetting.m_byRsdMode;
		m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed = commonConfigSetting.m_dStaticSpeed;
		m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed = commonConfigSetting.m_byRevBrakeSpeed;
		m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed = commonConfigSetting.m_byForwardStaSpeed;
		m_sMotorParamCfg.m_sWindHandleCfg.byRSD230 = commonConfigSetting.m_byRSD230;
		m_sMotorParamCfg.m_sWindHandleCfg.byRSD231 = commonConfigSetting.m_byRSD231;
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdHallFilter = commonConfigSetting.m_byRsdHallFilter;
		m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd = commonConfigSetting.m_bySpdCtrlMd;
		m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = commonConfigSetting.m_byPwmOffDis;
		m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = commonConfigSetting.m_byPwmRevEn;
		m_sMotorParamCfg.m_sOhterCfg.byQuickSta = commonConfigSetting.m_byQuickSta;
		m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = commonConfigSetting.m_byCurveEnSpd;
		m_sMotorParamCfg.m_sOhterCfg.byFgMul = commonConfigSetting.m_byFgMul;
		m_sMotorParamCfg.m_sOhterCfg.byFgRdMd = commonConfigSetting.m_byFgRdMd;
		m_sMotorParamCfg.m_sOhterCfg.byFgDiv = commonConfigSetting.m_byFgDiv;
		m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet = commonConfigSetting.m_byFgIdleSet;
		m_sMotorParamCfg.m_sOhterCfg.byFgSet = commonConfigSetting.m_byFgSet;
		m_sMotorParamCfg.m_sOhterCfg.byFgRd = commonConfigSetting.m_byFgRd;
		m_sMotorParamCfg.m_sOhterCfg.byStartClk = commonConfigSetting.m_byStartClk;
		m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes = commonConfigSetting.m_byFaultRecovTimes;
		m_sMotorParamCfg.m_sProtectCfg.byLock0 = commonConfigSetting.m_byLock0;
		m_sMotorParamCfg.m_sProtectCfg.byLock1 = commonConfigSetting.m_byLock1;
		m_sMotorParamCfg.m_sProtectCfg.byLock2 = commonConfigSetting.m_byLock2;
		m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn = commonConfigSetting.m_byLossPhaseEn;
		m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed = commonConfigSetting.m_wLockMinSpeed;
		m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed = commonConfigSetting.m_wLocMaxSpeed;
		m_sMotorParamCfg.m_sProtectCfg.byLockEmfThd = commonConfigSetting.m_byLockEmfThd;
		m_sMotorParamCfg.m_sProtectCfg.fLockEmf = commonConfigSetting.m_fLockEmf;
		m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent = commonConfigSetting.m_fLpNorhCurrent;
		m_sMotorParamCfg.m_sProtectCfg.byUvloDis = commonConfigSetting.m_byUvloDis;
		m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet = commonConfigSetting.m_bySpeedVSet;
		m_sMotorParamCfg.m_sExtendCfg.byVRefSel = 1;
		m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel = commonConfigSetting.m_byFgTimeSel;
		m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed = commonConfigSetting.m_wLimitSpeed;
		if (sAppVersion == "")
		{
			StartCurrentToDuty();
		}
		else
		{
			int length = sAppVersion.Length;
			int length2 = m_strAppVersion.Replace(".", "").Length;
			int num2 = length2 - length;
			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					sAppVersion += "0";
				}
			}
			byte b = CompareVersion(sAppVersion, m_strAppVersion);
			if (b == 1)
			{
				StartCurrentToDuty();
			}
			else
			{
				m_sMotorParamCfg.m_sStartCfg.byStartDuty = commonConfigSetting.m_byStartDuty30;
			}
		}
		m_sMotorParamCfg.m_sTargetSpdCurve.byXON0 = commonConfigSetting.m_byXON0;
		m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON = commonConfigSetting.m_byX_ON;
		m_sMotorParamCfg.m_sTargetSpdCurve.byPreCtrlMode = commonConfigSetting.m_byPreCtrlMode;
		m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed = commonConfigSetting.m_dwMinSpeed;
		m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed = commonConfigSetting.m_dwMaxSpeed;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent = commonConfigSetting.m_fMinCurrent;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent = commonConfigSetting.m_fMaxCurrent;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower = commonConfigSetting.m_fMinPower;
		m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower = commonConfigSetting.m_fMaxPower;
		m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel = commonConfigSetting.m_byYMaxSel;
		m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = commonConfigSetting.m_byPwmX98En;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON = commonConfigSetting.m_byY_ON;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_25 = commonConfigSetting.m_byY_25;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_50 = commonConfigSetting.m_byY_50;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_75 = commonConfigSetting.m_byY_75;
		string strSlaveId = commonConfigSetting.m_strSlaveId;
		byte bySlaveId = Convert.ToByte(strSlaveId, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = bySlaveId;
		strSlaveId = commonConfigSetting.m_strUID1;
		bySlaveId = Convert.ToByte(strSlaveId, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1 = bySlaveId;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = commonConfigSetting.m_bySpdCtrl;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpiDebug = commonConfigSetting.m_bySpiDebug;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd = commonConfigSetting.m_byDbgMd;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = commonConfigSetting.m_byMotorDir;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn = commonConfigSetting.m_byPwmEn;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0 = commonConfigSetting.m_byFDS0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1 = commonConfigSetting.m_byFDS1;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOcpSel = commonConfigSetting.m_byOcpSel;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdDirRev = commonConfigSetting.m_byIsdDirRev;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOvpSel = commonConfigSetting.m_byOvpSel;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDzSet = commonConfigSetting.m_byDzSet;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDpmDis = commonConfigSetting.m_byDpmDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byUvloDis = commonConfigSetting.m_byUvloDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOvpDis = commonConfigSetting.m_byOvpDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byOcpDis = commonConfigSetting.m_byOcpDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTsdDis = commonConfigSetting.m_byTsdDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFocDis = commonConfigSetting.m_byFocDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byBootMod = commonConfigSetting.m_byBootMod;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdBrakeEn = commonConfigSetting.m_byIsdBrakeEn;
		m_sMotorParamCfg.m_sWindHandleCfg.byIsdBrakeEn = commonConfigSetting.m_byIsdBrakeEn;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn = commonConfigSetting.m_byIdleBrakeEn;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTSMin = commonConfigSetting.m_byTSMin;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRvsAlignEn = commonConfigSetting.m_byRvsAlignEn;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRvsCutSel = commonConfigSetting.m_byRvsCutSel;
		m_sMotorParamCfg.m_eSpecialRegCfg.byNewSingle = commonConfigSetting.m_byNewSingle;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDigTst = commonConfigSetting.m_byDigTst;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDigMoniCt = commonConfigSetting.m_byDigMoniCt;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTestMode = commonConfigSetting.m_byTestMode;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIrefDis = commonConfigSetting.m_byIrefDis;
		m_sMotorParamCfg.m_eSpecialRegCfg.byTsdSel = commonConfigSetting.m_byS5TsdSel;
		m_sMotorParamCfg.m_eSpecialRegCfg.byHallOvMd = commonConfigSetting.m_byHallOvMd;
		m_sMotorParamCfg.m_eSpecialRegCfg.byHallLttEn = commonConfigSetting.m_byHallLttEn;
		m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstQMax = commonConfigSetting.m_byStallTstQMax;
		m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstInc = commonConfigSetting.m_byStallTstInc;
		m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstEn = commonConfigSetting.m_byStallTstEn;
		m_sMotorParamCfg.m_eSpecialRegCfg.byDrvDisMod = commonConfigSetting.m_byDrvDisMod;
		m_sMotorParamCfg.m_eSpecialRegCfg.byCmp0CkSel = commonConfigSetting.m_byCmp0CkSel;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp = commonConfigSetting.m_byFlocLdVoltKp;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp = commonConfigSetting.m_byFlocLqVoltKp;
		m_sMotorParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp = commonConfigSetting.m_byFocFluxVoltKp;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn = commonConfigSetting.m_byRestaEn;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd = commonConfigSetting.m_byIsdFwMd;
		m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd = commonConfigSetting.m_byLaMd;
		m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = commonConfigSetting.m_byNewFunc;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty = commonConfigSetting.m_byStartOnOffDuty;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration = commonConfigSetting.m_byStartDurationTime;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty = commonConfigSetting.m_byStopDuty;
		m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration = commonConfigSetting.m_byStopDurationTime;
		m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt = commonConfigSetting.m_wTotalCnt;
		m_clsDlgItemSet45.OpenSpecifyConfig(filename);
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Curr_Base = commonConfigSetting.m_strHW_Board_Curr_Base;
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Vol_Base2 = commonConfigSetting.m_strHW_Board_Vol_Base2;
		m_sMotorParamCfg.m_sShowBASECfg.strMotor_Power_Base = commonConfigSetting.m_strMotor_Power_Base;
		return num;
	}

	private bool UpdateUIDataToUserCfg()
	{
		if (commonConfigSetting == null || m_sMotorParamCfg == null)
		{
			return false;
		}
		commonConfigSetting.m_strSwName = m_strSwName;
		commonConfigSetting.m_i16SensorMode = m_i16SensorMode;
		commonConfigSetting.m_byCircuitType = m_byCircuitType;
		commonConfigSetting.m_byPolePairs = m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		commonConfigSetting.m_fRS = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu;
		commonConfigSetting.m_fLQ = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan;
		commonConfigSetting.m_fKe = m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi;
		commonConfigSetting.m_fHwRSHunt = m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu;
		commonConfigSetting.m_byHwAmpGain = m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu;
		commonConfigSetting.m_fRV = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV;
		commonConfigSetting.m_byVdcAdj = m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj;
		commonConfigSetting.m_bySMPMode = m_sMotorParamCfg.m_sFocParamCfg.bySMPMode;
		commonConfigSetting.m_byStaTsam0En = m_sMotorParamCfg.m_sFocParamCfg.byStaTsam0En;
		commonConfigSetting.m_byPllSmoEn = m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn;
		commonConfigSetting.m_byStaPllEn = m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn;
		commonConfigSetting.m_bySeg7En = m_sMotorParamCfg.m_sFocParamCfg.bySeg7En;
		commonConfigSetting.m_bySta7SegEn = m_sMotorParamCfg.m_sFocParamCfg.bySta7SegEn;
		commonConfigSetting.m_byAtanEn = m_sMotorParamCfg.m_sFocParamCfg.byAtanEn;
		commonConfigSetting.m_byStaSin = m_sMotorParamCfg.m_sFocParamCfg.byStaSin;
		commonConfigSetting.m_wFreqBase = m_sMotorParamCfg.m_sFocParamCfg.wFreqBase;
		commonConfigSetting.m_bySampleTime = m_sMotorParamCfg.m_sFocParamCfg.bySampleTime;
		commonConfigSetting.m_bySpdBw = m_sMotorParamCfg.m_sFocParamCfg.bySpdBw;
		commonConfigSetting.m_byEbwMode = m_sMotorParamCfg.m_sFocParamCfg.byEbwMode;
		commonConfigSetting.m_fLeadAngleSet = m_sMotorParamCfg.m_sFocParamCfg.fLeadAngleSet;
		commonConfigSetting.m_byEkNum = m_sMotorParamCfg.m_sFocParamCfg.byEkNum;
		commonConfigSetting.m_wEbw = m_sMotorParamCfg.m_sFocParamCfg.wEbw;
		commonConfigSetting.m_fAtoBw = m_sMotorParamCfg.m_sFocParamCfg.fAtoBw;
		commonConfigSetting.m_byTSMIN = m_sMotorParamCfg.m_sFocParamCfg.byTSMIN;
		commonConfigSetting.m_fDTSEL = m_sMotorParamCfg.m_sFocParamCfg.fDTSEL;
		commonConfigSetting.m_byKSILDE = m_sMotorParamCfg.m_sFocParamCfg.byKSILDE;
		commonConfigSetting.m_byRpdAlignEn = m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn;
		commonConfigSetting.m_byAlignTime = m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime;
		commonConfigSetting.m_byAlignDuty = m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty;
		commonConfigSetting.m_byRPD10 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10;
		commonConfigSetting.m_byRPD11 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11;
		commonConfigSetting.m_byRPD12 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD123_2;
		commonConfigSetting.m_byRPD20 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20;
		commonConfigSetting.m_byRPDRvsAlignEn = m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn;
		commonConfigSetting.m_byRPD21 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21;
		commonConfigSetting.m_byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		commonConfigSetting.m_fStartCurrent = m_sMotorParamCfg.m_sStartCfg.fStartCurrent;
		commonConfigSetting.m_byFocEfreqAcc = m_sMotorParamCfg.m_sStartCfg.byFocEfreqAccx;
		commonConfigSetting.m_fFocEfreqMin = m_sMotorParamCfg.m_sStartCfg.fFocEfreqMinx;
		commonConfigSetting.m_byFocEfreqHold = m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold;
		commonConfigSetting.m_byCloSpeed = m_sMotorParamCfg.m_sStartCfg.byCloSpeed;
		commonConfigSetting.m_byStartDuty30 = m_sMotorParamCfg.m_sStartCfg.byStartDuty;
		commonConfigSetting.m_byStaIncTime = m_sMotorParamCfg.m_sRunCfg.byStaIncTime;
		commonConfigSetting.m_byStbInc = m_sMotorParamCfg.m_sRunCfg.byStbInc;
		commonConfigSetting.m_byStaInc = m_sMotorParamCfg.m_sRunCfg.byStaInc;
		commonConfigSetting.m_fDqKp = m_sMotorParamCfg.m_sRunCfg.fDqKp;
		commonConfigSetting.m_fDqKi = m_sMotorParamCfg.m_sRunCfg.fDqKi;
		commonConfigSetting.m_fWKp = m_sMotorParamCfg.m_sRunCfg.fWKp;
		commonConfigSetting.m_fWKi = m_sMotorParamCfg.m_sRunCfg.fWKi;
		commonConfigSetting.m_byWoutMax = m_sMotorParamCfg.m_sRunCfg.byWoutMax;
		commonConfigSetting.m_byPwmOnoffhys = m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys;
		commonConfigSetting.m_byPwmOnoffhysClk = m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk;
		commonConfigSetting.m_byRsdEn = m_sMotorParamCfg.m_sWindHandleCfg.byRsdEn;
		commonConfigSetting.m_byRsdMode = m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode;
		commonConfigSetting.m_dStaticSpeed = m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed;
		commonConfigSetting.m_byRevBrakeSpeed = m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed;
		commonConfigSetting.m_byForwardStaSpeed = m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed;
		commonConfigSetting.m_byRSD230 = m_sMotorParamCfg.m_sWindHandleCfg.byRSD230;
		commonConfigSetting.m_byRSD231 = m_sMotorParamCfg.m_sWindHandleCfg.byRSD231;
		commonConfigSetting.m_byRsdHallFilter = m_sMotorParamCfg.m_sWindHandleCfg.byRsdHallFilter;
		commonConfigSetting.m_bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
		commonConfigSetting.m_byPwmOffDis = m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis;
		commonConfigSetting.m_byPwmRevEn = m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn;
		commonConfigSetting.m_byQuickSta = m_sMotorParamCfg.m_sOhterCfg.byQuickSta;
		commonConfigSetting.m_byCurveEnSpd = m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd;
		commonConfigSetting.m_byFgMul = m_sMotorParamCfg.m_sOhterCfg.byFgMul;
		commonConfigSetting.m_byFgRdMd = m_sMotorParamCfg.m_sOhterCfg.byFgRdMd;
		commonConfigSetting.m_byFgDiv = m_sMotorParamCfg.m_sOhterCfg.byFgDiv;
		commonConfigSetting.m_byFgIdleSet = m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet;
		commonConfigSetting.m_byFgSet = m_sMotorParamCfg.m_sOhterCfg.byFgSet;
		commonConfigSetting.m_byFgRd = m_sMotorParamCfg.m_sOhterCfg.byFgRd;
		commonConfigSetting.m_byStartClk = m_sMotorParamCfg.m_sOhterCfg.byStartClk;
		commonConfigSetting.m_byFaultRecovTimes = m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes;
		commonConfigSetting.m_byLock0 = m_sMotorParamCfg.m_sProtectCfg.byLock0;
		commonConfigSetting.m_byLock1 = m_sMotorParamCfg.m_sProtectCfg.byLock1;
		commonConfigSetting.m_byLock2 = m_sMotorParamCfg.m_sProtectCfg.byLock2;
		commonConfigSetting.m_byLossPhaseEn = m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn;
		commonConfigSetting.m_wLockMinSpeed = m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed;
		commonConfigSetting.m_wLocMaxSpeed = m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed;
		commonConfigSetting.m_byLockEmfThd = m_sMotorParamCfg.m_sProtectCfg.byLockEmfThd;
		commonConfigSetting.m_fLockEmf = m_sMotorParamCfg.m_sProtectCfg.fLockEmf;
		commonConfigSetting.m_fLpNorhCurrent = m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent;
		commonConfigSetting.m_byUvloDis = m_sMotorParamCfg.m_sProtectCfg.byUvloDis;
		commonConfigSetting.m_bySpeedVSet = m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet;
		commonConfigSetting.m_byVRefSel = m_sMotorParamCfg.m_sExtendCfg.byVRefSel;
		commonConfigSetting.m_byFgTimeSel = m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel;
		commonConfigSetting.m_wLimitSpeed = m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed;
		commonConfigSetting.m_byXON0 = m_sMotorParamCfg.m_sTargetSpdCurve.byXON0;
		commonConfigSetting.m_byX_ON = m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON;
		commonConfigSetting.m_byPreCtrlMode = m_sMotorParamCfg.m_sTargetSpdCurve.byPreCtrlMode;
		commonConfigSetting.m_dwMinSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed;
		commonConfigSetting.m_dwMaxSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed;
		commonConfigSetting.m_fMinCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent;
		commonConfigSetting.m_fMaxCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent;
		commonConfigSetting.m_fMinPower = m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower;
		commonConfigSetting.m_fMaxPower = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower;
		commonConfigSetting.m_byYMaxSel = m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel;
		commonConfigSetting.m_byPwmX98En = m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En;
		commonConfigSetting.m_byY_ON = m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON;
		commonConfigSetting.m_byY_25 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_25;
		commonConfigSetting.m_byY_50 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_50;
		commonConfigSetting.m_byY_75 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_75;
		byte bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
		string strSlaveId = $"0x{bySlaveId:X2}";
		commonConfigSetting.m_strSlaveId = strSlaveId;
		bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1;
		strSlaveId = $"0x{bySlaveId:X2}";
		commonConfigSetting.m_strUID1 = strSlaveId;
		commonConfigSetting.m_bySpdCtrl = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl;
		commonConfigSetting.m_bySpiDebug = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpiDebug;
		commonConfigSetting.m_byDbgMd = m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd;
		commonConfigSetting.m_byMotorDir = m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir;
		commonConfigSetting.m_byPwmEn = m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn;
		commonConfigSetting.m_byFDS0 = m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0;
		commonConfigSetting.m_byFDS1 = m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1;
		commonConfigSetting.m_byOcpSel = m_sMotorParamCfg.m_eSpecialRegCfg.byOcpSel;
		commonConfigSetting.m_byIsdDirRev = m_sMotorParamCfg.m_eSpecialRegCfg.byIsdDirRev;
		commonConfigSetting.m_byOvpSel = m_sMotorParamCfg.m_eSpecialRegCfg.byOvpSel;
		commonConfigSetting.m_byDzSet = m_sMotorParamCfg.m_eSpecialRegCfg.byDzSet;
		commonConfigSetting.m_byDpmDis = m_sMotorParamCfg.m_eSpecialRegCfg.byDpmDis;
		commonConfigSetting.m_byUvloDis = m_sMotorParamCfg.m_eSpecialRegCfg.byUvloDis;
		commonConfigSetting.m_byOvpDis = m_sMotorParamCfg.m_eSpecialRegCfg.byOvpDis;
		commonConfigSetting.m_byOcpDis = m_sMotorParamCfg.m_eSpecialRegCfg.byOcpDis;
		commonConfigSetting.m_byTsdDis = m_sMotorParamCfg.m_eSpecialRegCfg.byTsdDis;
		commonConfigSetting.m_byFocDis = m_sMotorParamCfg.m_eSpecialRegCfg.byFocDis;
		commonConfigSetting.m_byBootMod = m_sMotorParamCfg.m_eSpecialRegCfg.byBootMod;
		commonConfigSetting.m_byIsdBrakeEn = m_sMotorParamCfg.m_eSpecialRegCfg.byIsdBrakeEn;
		commonConfigSetting.m_byIsdBrakeEn = m_sMotorParamCfg.m_sWindHandleCfg.byIsdBrakeEn;
		commonConfigSetting.m_byIdleBrakeEn = m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn;
		commonConfigSetting.m_byTSMin = m_sMotorParamCfg.m_eSpecialRegCfg.byTSMin;
		commonConfigSetting.m_byRvsAlignEn = m_sMotorParamCfg.m_eSpecialRegCfg.byRvsAlignEn;
		commonConfigSetting.m_byRvsCutSel = m_sMotorParamCfg.m_eSpecialRegCfg.byRvsCutSel;
		commonConfigSetting.m_byNewSingle = m_sMotorParamCfg.m_eSpecialRegCfg.byNewSingle;
		commonConfigSetting.m_byDigTst = m_sMotorParamCfg.m_eSpecialRegCfg.byDigTst;
		commonConfigSetting.m_byDigMoniCt = m_sMotorParamCfg.m_eSpecialRegCfg.byDigMoniCt;
		commonConfigSetting.m_byTestMode = m_sMotorParamCfg.m_eSpecialRegCfg.byTestMode;
		commonConfigSetting.m_byIrefDis = m_sMotorParamCfg.m_eSpecialRegCfg.byIrefDis;
		commonConfigSetting.m_byS5TsdSel = m_sMotorParamCfg.m_eSpecialRegCfg.byTsdSel;
		commonConfigSetting.m_byHallOvMd = m_sMotorParamCfg.m_eSpecialRegCfg.byHallOvMd;
		commonConfigSetting.m_byHallLttEn = m_sMotorParamCfg.m_eSpecialRegCfg.byHallLttEn;
		commonConfigSetting.m_byStallTstQMax = m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstQMax;
		commonConfigSetting.m_byStallTstInc = m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstInc;
		commonConfigSetting.m_byStallTstEn = m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstEn;
		commonConfigSetting.m_byDrvDisMod = m_sMotorParamCfg.m_eSpecialRegCfg.byDrvDisMod;
		commonConfigSetting.m_byCmp0CkSel = m_sMotorParamCfg.m_eSpecialRegCfg.byCmp0CkSel;
		commonConfigSetting.m_byFlocLdVoltKp = m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp;
		commonConfigSetting.m_byFlocLqVoltKp = m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp;
		commonConfigSetting.m_byFocFluxVoltKp = m_sMotorParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp;
		commonConfigSetting.m_byRestaEn = m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn;
		commonConfigSetting.m_byIsdFwMd = m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd;
		commonConfigSetting.m_byLaMd = m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd;
		commonConfigSetting.m_byNewFunc = m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc;
		commonConfigSetting.m_byStartOnOffDuty = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty;
		commonConfigSetting.m_byStartDurationTime = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration;
		commonConfigSetting.m_byStopDuty = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty;
		commonConfigSetting.m_byStopDurationTime = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration;
		commonConfigSetting.m_wTotalCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt;
		m_clsDlgItemSet45.UpdateUIDataToUserCfg();
		return true;
	}

	private bool ConverUIToParam()
	{
		if (m_sMotorParamCfg == null)
		{
			return false;
		}
		if (m_blIsSave2HallUC)
		{
			if (m_i16SensorMode == 3 && m_uc2HallICCoolFan_s1 != null && m_uc2HallICCoolFan_s2 != null)
			{
				m_uc2HallICCoolFan_s1.uc2hallIcCoolFans1ToMainWin();
				m_uc2HallICCoolFan_s2.uc2hallIcCoolFans2ToMainWin();
			}
			else if (m_i16SensorMode == 4 && m_SensorlessUC != null)
			{
				m_SensorlessUC.SaoDiBaoToMainWindow();
			}
		}
		m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu = byte.Parse(textBox_JiDuiShu.Text);
		m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan = float.Parse(textBox_DianGan.Text);
		m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu = float.Parse(textBox_DianZu.Text);
		m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi = float.Parse(textBox_FanDianDongShi.Text);
		m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu = float.Parse(textBox_CaiYandDianZu.Text);
		m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu = (byte)comboBox_FanDaBeiShu.SelectedIndex;
		m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj = ((radioButton_VdcAdj12.IsChecked != true) ? ((byte)1) : ((byte)0));
		switch (m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj)
		{
		case 1:
			m_sMotorParamCfg.m_sMotorParamHwCfg.fRV = 6.5f;
			break;
		default:
			m_sMotorParamCfg.m_sMotorParamHwCfg.fRV = 12f;
			break;
		}
		m_sMotorParamCfg.m_sFocParamCfg.bySMPMode = (byte)comboBox_SMPMode.SelectedIndex;
		if (radioButton_PllSmo_Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn = 1;
		}
		if (radioButton_StaPll_Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn = 1;
		}
		m_sMotorParamCfg.m_sFocParamCfg.bySeg7En = (byte)comboBox_Seg7En.SelectedIndex;
		if (radioButton_Atan_Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sFocParamCfg.byAtanEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sFocParamCfg.byAtanEn = 1;
		}
		if (radioButton_StaSin_Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sFocParamCfg.byStaSin = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sFocParamCfg.byStaSin = 1;
		}
		m_sMotorParamCfg.m_sFocParamCfg.bySampleTime = byte.Parse(txtBlock_SampleTime.Text);
		m_sMotorParamCfg.m_sFocParamCfg.bySpdBw = byte.Parse(txtBlock_SpdBw.Text);
		m_sMotorParamCfg.m_sFocParamCfg.byEbwMode = byte.Parse(txtBlock_EBwMode.Text);
		m_sMotorParamCfg.m_sFocParamCfg.fLeadAngleSet = float.Parse(txtBlock_LeadAngle.Text);
		m_sMotorParamCfg.m_sFocParamCfg.byEkNum = byte.Parse(txtBlock_EKNum.Text);
		m_sMotorParamCfg.m_sFocParamCfg.wEbw = ushort.Parse(textBox_EBw.Text);
		m_sMotorParamCfg.m_sFocParamCfg.fAtoBw = float.Parse(textBox_AtoBw.Text);
		m_sMotorParamCfg.m_sFocParamCfg.byTSMIN = byte.Parse(txtBlock_TSMIN.Text);
		m_sMotorParamCfg.m_sFocParamCfg.fDTSEL = float.Parse(slider_DTSEL.Value.ToString());
		m_sMotorParamCfg.m_sFocParamCfg.byKSILDE = (byte)float.Parse(txtBlock_KSILDE.Text);
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty = (byte)slider_AlignDuty.Value;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime = (byte)slider_AlignTime.Value;
		if (radioBtn_RPD10_6us.IsChecked == true)
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10 = 1;
		}
		if (radioBtn_RPD11_2us.IsChecked == true)
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11 = 1;
		}
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD123_2 = byte.Parse(textBlock_RPD12.Text);
		if (radioBtn_RPD20_6us.IsChecked == true)
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20 = 1;
		}
		if (radioBtn_RvsAlignEn0.IsChecked == true)
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn = 1;
		}
		m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21 = byte.Parse(textBlock_RPD21.Text);
		m_sMotorParamCfg.m_sStartCfg.fStartCurrent = float.Parse(textBox_StartCurrent.Text);
		m_sMotorParamCfg.m_sStartCfg.byStartDuty = byte.Parse(textBox_StartDuty30.Text);
		m_sMotorParamCfg.m_sStartCfg.byFocEfreqAccx = byte.Parse(textBlock_EFreQAcc.Text);
		m_sMotorParamCfg.m_sStartCfg.fFocEfreqMinx = float.Parse(textBlock_EFreQMin.Text);
		if (radioButton_EFreQHold_1x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold = 0;
		}
		if (radioButton_EFreQHold_2x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold = 1;
		}
		if (radioButton_EFreQHold_4x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold = 2;
		}
		if (radioButton_EFreQHold_8x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold = 3;
		}
		if (radioButton_CloSpeed_0x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byCloSpeed = 3;
		}
		if (radioButton_CloSpeed_1x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byCloSpeed = 0;
		}
		if (radioButton_CloSpeed_2x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byCloSpeed = 1;
		}
		if (radioButton_CloSpeed_4x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sStartCfg.byCloSpeed = 2;
		}
		if (radioButton_StaIncTime_1s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sRunCfg.byStaIncTime = 0;
		}
		if (radioButton_StaIncTime_2s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sRunCfg.byStaIncTime = 1;
		}
		if (radioButton_StaIncTime_4s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sRunCfg.byStaIncTime = 2;
		}
		if (radioButton_StaIncTime_6s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sRunCfg.byStaIncTime = 3;
		}
		m_sMotorParamCfg.m_sRunCfg.byStbInc = byte.Parse(textBlock_StbInc.Text);
		m_sMotorParamCfg.m_sRunCfg.byStaInc = byte.Parse(textBlock_StaInc.Text);
		m_sMotorParamCfg.m_sRunCfg.fDqKp = float.Parse(textBox_DQKp.Text);
		m_sMotorParamCfg.m_sRunCfg.fDqKi = float.Parse(textBox_DQKi.Text);
		m_sMotorParamCfg.m_sRunCfg.fWKp = float.Parse(textBox_WKp.Text);
		m_sMotorParamCfg.m_sRunCfg.fWKi = float.Parse(textBox_WKi.Text);
		m_sMotorParamCfg.m_sRunCfg.byWoutMax = byte.Parse(textBox_WOutMax.Text);
		byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
		if (bySpdCtrlMd == 3)
		{
			if (radioButton_PwmOnOffhys0.IsChecked == true)
			{
				m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk = 0;
			}
			else
			{
				m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk = 1;
			}
		}
		else if (radioButton_PwmOnOffhys0.IsChecked == true)
		{
			m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys = 1;
		}
		m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed = double.Parse(textBlock_StaticSpeed.Text);
		if (radioBtn_ForwardStaSpeed_1x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed = 0;
		}
		if (radioBtn_ForwardStaSpeed_2x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed = 1;
		}
		if (radioBtn_ForwardStaSpeed_4x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed = 2;
		}
		if (radioBtn_ForwardStaSpeed_8x.IsChecked == true)
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed = 3;
		}
		if (radioBtn_ThresholdL.IsChecked == true)
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byRSD230 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byRSD230 = 1;
		}
		m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes = (byte)comboBox_FaultRestoreTimes.SelectedIndex;
		if (radioBtn_Lock0Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sProtectCfg.byLock0 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sProtectCfg.byLock0 = 1;
		}
		if (radioBtn_Lock1Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sProtectCfg.byLock1 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sProtectCfg.byLock1 = 1;
		}
		if (radioBtn_Lock2Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sProtectCfg.byLock2 = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sProtectCfg.byLock2 = 1;
		}
		if (radioBtn_LossPhaseDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn = 1;
		}
		m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed = uint.Parse(textBox_LockMinSpeed.Text);
		m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed = uint.Parse(textBox_LockMaxSpeed.Text);
		m_sMotorParamCfg.m_sProtectCfg.fLockEmf = float.Parse(TextBox_LockEmf.Text);
		m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent = float.Parse(textBox_LPNorhCur.Text);
		if (radioBtn_PwmOffDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 1;
		}
		if (radioBtn_PwmRevDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 1;
		}
		if (radioBtn_QuickStaDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byQuickSta = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byQuickSta = 1;
		}
		if (radioBtn_CurveSpdDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 1;
		}
		m_sMotorParamCfg.m_sOhterCfg.byFgMul = (byte)comboBox_FgMul.SelectedIndex;
		m_sMotorParamCfg.m_sOhterCfg.byFgRdMd = (byte)comboBox_FgRdMd.SelectedIndex;
		m_sMotorParamCfg.m_sOhterCfg.byFgDiv = (byte)comboBox_FgDiv.SelectedIndex;
		if (radioBtn_FgIdleSetDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet = 1;
		}
		m_sMotorParamCfg.m_sOhterCfg.byStartClk = (byte)comboBox_StartClk.SelectedIndex;
		m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet = (byte)comboBox_SpeedVSet.SelectedIndex;
		m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed = uint.Parse(textBox_LimitSpeed.Text);
		if (radioBtn_04s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel = 0;
		}
		if (radioBtn_08s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel = 1;
		}
		if (radioBtn_12s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel = 2;
		}
		if (radioBtn_16s.IsChecked == true)
		{
			m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel = 3;
		}
		m_sMotorParamCfg.m_sTargetSpdCurve.byXON0 = byte.Parse(textBox_XON0.Text);
		m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON = byte.Parse(textBox_XON.Text);
		switch (m_sMotorParamCfg.m_sStartCfg.byControlMode)
		{
		case 2:
			m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed = uint.Parse(textBox_MinSpeed.Text);
			m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed = uint.Parse(textBox_MaxSpeed.Text);
			break;
		case 1:
			m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent = float.Parse(textBox_MinSpeed.Text);
			m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent = float.Parse(textBox_MaxSpeed.Text);
			break;
		case 3:
			m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower = float.Parse(textBox_MinSpeed.Text);
			m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower = float.Parse(textBox_MaxSpeed.Text);
			break;
		default:
			m_sMotorParamCfg.m_sTargetSpdCurve.byPreCtrlMode = m_byPreCtrlMode;
			if (m_byPreCtrlMode == 2)
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed = uint.Parse(textBox_MinSpeed.Text);
				m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed = uint.Parse(textBox_MaxSpeed.Text);
			}
			else if (m_byPreCtrlMode == 3)
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower = float.Parse(textBox_MinSpeed.Text);
				m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower = float.Parse(textBox_MaxSpeed.Text);
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent = float.Parse(textBox_MinSpeed.Text);
				m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent = float.Parse(textBox_MaxSpeed.Text);
			}
			break;
		}
		if (radioBtn_PwmX98Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 1;
		}
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON = byte.Parse(textBox_YON.Text);
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_25 = byte.Parse(textBox_Y25.Text);
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_50 = byte.Parse(textBox_Y50.Text);
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_75 = byte.Parse(textBox_Y75.Text);
		string text = textBox_SlvID.Text;
		byte bySlaveId = Convert.ToByte(text, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = bySlaveId;
		text = textBox_UID1.Text;
		bySlaveId = Convert.ToByte(text, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1 = bySlaveId;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = byte.Parse(textBox_SpdCtrl.Text);
		if (radioBtn_dbgmd0.IsChecked == true)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd = 0;
		}
		else if (radioBtn_dbgmd1.IsChecked == true)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd = 1;
		}
		else if (radioBtn_dbgmd2.IsChecked == true)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd = 2;
		}
		else
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd = 3;
		}
		if (radioBtn_MotorDirDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = 1;
		}
		if (radioBtn_PwmDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn = 1;
		}
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0 = (byte)comboBox_FocDebug12.SelectedIndex;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1 = (byte)comboBox_FocDebug34.SelectedIndex;
		if (checkBox_EnNewOnOffTest.IsChecked == true)
		{
			string text2 = textBox_StartDuty.Text;
			bySlaveId = Convert.ToByte(text2, 10);
			m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty = bySlaveId;
			text2 = textBox_StartDurationTime.Text;
			bySlaveId = Convert.ToByte(text2, 10);
			m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration = bySlaveId;
			text2 = textBox_StopDuty.Text;
			bySlaveId = Convert.ToByte(text2, 10);
			m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty = bySlaveId;
			text2 = textBox_StopDurationTime.Text;
			bySlaveId = Convert.ToByte(text2, 10);
			m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration = bySlaveId;
			text2 = textBox_TotalOnOffCnt.Text;
			ushort wTotalSetCnt = Convert.ToUInt16(text2, 10);
			m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt = wTotalSetCnt;
		}
		m_clsDlgItemSet45.ConverUIToParam();
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Curr_Base = textblock_BoardCurrBase.Text;
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Vol_Base2 = textblock_BoardVolBase2.Text;
		m_sMotorParamCfg.m_sShowBASECfg.strMotor_Power_Base = textblock_MotorPowerBase.Text;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn = (byte)comboBox_IdleBrakeEn.SelectedIndex;
		m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn = (byte)adWindow.comboBox_restaEn.SelectedIndex;
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd = (byte)adWindow.comboBox_IsdFwMd.SelectedIndex;
		m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd = (byte)adWindow.comboBox_lamd.SelectedIndex;
		m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = (byte)adWindow.comboBox_newFunc.SelectedIndex;
		return true;
	}

	public void InitSliderTicks(Slider slider, string strValPrefix)
	{
		slider.Ticks = null;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		ushort num4 = 0;
		DoubleCollection doubleCollection = new DoubleCollection();
		string resourceKey = strValPrefix + "Cnt";
		string s = (string)FindResource(resourceKey);
		ushort num5 = ushort.Parse(s);
		resourceKey = strValPrefix + num4;
		s = (string)FindResource(resourceKey);
		num = double.Parse(s);
		num4 = (ushort)(num5 - 1);
		resourceKey = strValPrefix + num4;
		s = (string)FindResource(resourceKey);
		num2 = double.Parse(s);
		slider.Minimum = num;
		slider.Maximum = num2;
		for (num4 = 0; num4 < num5; num4++)
		{
			if (strValPrefix.CompareTo("Cfg_RunWoutMax") == 0 || strValPrefix.CompareTo("Cfg_UserCfgSpdCtrl") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgXON0") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgXON") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgYON") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgY25") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgY50") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgY75") == 0 || strValPrefix.CompareTo("Cfg_WindRevBrakeSpeed") == 0 || strValPrefix.CompareTo("Cfg_HallSwiMd") == 0 || strValPrefix.CompareTo("Cfg_HallLA0") == 0 || strValPrefix.CompareTo("Cfg_S5TsdSel") == 0 || strValPrefix.CompareTo("Cfg_S5OvpThr") == 0 || strValPrefix.CompareTo("Cfg_PwrAmpSel") == 0 || strValPrefix.CompareTo("Cfg_S5LimitTrip") == 0 || strValPrefix.CompareTo("Cfg_LimitSel") == 0 || strValPrefix.CompareTo("Cfg_CurveCfgYMaxSel") == 0 || strValPrefix.CompareTo("Cfg_S5TheCompSel") == 0 || strValPrefix.CompareTo("Cfg_S5Cmp0CkSel") == 0)
			{
				s = num4.ToString();
			}
			else
			{
				resourceKey = strValPrefix + num4;
				s = (string)FindResource(resourceKey);
			}
			doubleCollection.Add(double.Parse(s));
		}
		slider.Ticks = doubleCollection;
		slider.Foreground = Brushes.Red;
	}

	public void InitSliderTicks(Slider slider, DoubleCollection tickMarks)
	{
		slider.Ticks = null;
		slider.Minimum = tickMarks.Min();
		slider.Maximum = tickMarks.Max();
		slider.Ticks = tickMarks;
		slider.Foreground = Brushes.Red;
	}

	private int HandleAlignDisplay(byte byAlignType)
	{
		bool isEnabled = false;
		if (byAlignType == 0)
		{
			radioBtn_RPD10_6us.IsEnabled = isEnabled;
			radioBtn_RPD10_4us.IsEnabled = isEnabled;
			radioBtn_RPD11_2us.IsEnabled = isEnabled;
			radioBtn_RPD11_4us.IsEnabled = isEnabled;
			textBlock_RPD12.IsEnabled = isEnabled;
			slider_RPD12.IsEnabled = isEnabled;
			radioBtn_RPD20_6us.IsEnabled = isEnabled;
			radioBtn_RPD20_4us.IsEnabled = isEnabled;
			textBlock_RPD21.IsEnabled = isEnabled;
			slider_RPD21.IsEnabled = isEnabled;
		}
		else if (byAlignType == 1)
		{
			isEnabled = true;
			isEnabled = false;
			radioBtn_RPD10_6us.IsEnabled = isEnabled;
			radioBtn_RPD10_4us.IsEnabled = isEnabled;
			radioBtn_RPD11_2us.IsEnabled = isEnabled;
			radioBtn_RPD11_4us.IsEnabled = isEnabled;
			textBlock_RPD12.IsEnabled = isEnabled;
			slider_RPD12.IsEnabled = isEnabled;
			radioBtn_RPD20_6us.IsEnabled = isEnabled;
			radioBtn_RPD20_4us.IsEnabled = isEnabled;
			textBlock_RPD21.IsEnabled = isEnabled;
			slider_RPD21.IsEnabled = isEnabled;
		}
		else if (byAlignType == 2 || byAlignType == 3)
		{
			isEnabled = false;
			isEnabled = true;
			radioBtn_RPD10_6us.IsEnabled = isEnabled;
			radioBtn_RPD10_4us.IsEnabled = isEnabled;
			radioBtn_RPD11_2us.IsEnabled = isEnabled;
			radioBtn_RPD11_4us.IsEnabled = isEnabled;
			textBlock_RPD12.IsEnabled = isEnabled;
			slider_RPD12.IsEnabled = isEnabled;
			isEnabled = false;
			radioBtn_RPD20_6us.IsEnabled = isEnabled;
			radioBtn_RPD20_4us.IsEnabled = isEnabled;
			textBlock_RPD21.IsEnabled = isEnabled;
			slider_RPD21.IsEnabled = isEnabled;
		}
		else
		{
			isEnabled = false;
			radioBtn_RPD10_6us.IsEnabled = isEnabled;
			radioBtn_RPD10_4us.IsEnabled = isEnabled;
			radioBtn_RPD11_2us.IsEnabled = isEnabled;
			radioBtn_RPD11_4us.IsEnabled = isEnabled;
			textBlock_RPD12.IsEnabled = isEnabled;
			slider_RPD12.IsEnabled = isEnabled;
			isEnabled = true;
			slider_RPD12.IsEnabled = isEnabled;
			textBlock_RPD12.IsEnabled = isEnabled;
			radioBtn_RPD20_6us.IsEnabled = isEnabled;
			radioBtn_RPD20_4us.IsEnabled = isEnabled;
			textBlock_RPD21.IsEnabled = isEnabled;
			slider_RPD21.IsEnabled = isEnabled;
		}
		byte byRvsAlignEn = m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn;
		isEnabled = ((byAlignType == 1 || byRvsAlignEn == 1) ? true : false);
		slider_AlignDuty.IsEnabled = isEnabled;
		textBlock_AlignDuty.IsEnabled = isEnabled;
		slider_AlignTime.IsEnabled = isEnabled;
		textBlock_AlignTime.IsEnabled = isEnabled;
		return 0;
	}

	private int HandleCurveCtrl(byte byCtrlMode, bool bEnabled)
	{
		bool flag = bEnabled;
		label_MinSpeed.IsEnabled = flag;
		label_MaxSpeed.IsEnabled = flag;
		textBox_MinSpeed.IsEnabled = flag;
		textBox_MaxSpeed.IsEnabled = flag;
		label_XON0.IsEnabled = flag;
		slider_XON0.IsEnabled = flag;
		textBox_XON0.IsEnabled = flag;
		textBlock_XON0.IsEnabled = flag;
		label_XON0_Percent.IsEnabled = flag;
		label_YON.IsEnabled = !flag;
		slider_YON.IsEnabled = !flag;
		textBox_YON.IsEnabled = !flag;
		textBlock_YON.IsEnabled = !flag;
		label_YON_Percent.IsEnabled = !flag;
		label_Y25.IsEnabled = !flag;
		slider_Y25.IsEnabled = !flag;
		textBox_Y25.IsEnabled = !flag;
		textBlock_Y25.IsEnabled = !flag;
		label_Y25_Percent.IsEnabled = !flag;
		label_Y50.IsEnabled = !flag;
		slider_Y50.IsEnabled = !flag;
		textBox_Y50.IsEnabled = !flag;
		textBlock_Y50.IsEnabled = !flag;
		label_Y50_Percent.IsEnabled = !flag;
		label_Y75.IsEnabled = !flag;
		slider_Y75.IsEnabled = !flag;
		textBox_Y75.IsEnabled = !flag;
		textBlock_Y75.IsEnabled = !flag;
		label_Y75_Percent.IsEnabled = !flag;
		return 0;
	}

	private int HandleCurveCfgSpeedDisplay(byte byCtrlMode)
	{
		switch (byCtrlMode)
		{
		case 2:
			m_byPreCtrlMode = 2;
			label_MinSpeed.Content = "MinSpeed";
			label_MaxSpeed.Content = "MaxSpeed";
			textBox_MinSpeed.ToolTip = "range:0-60000 integer";
			textBox_MaxSpeed.ToolTip = "range:0-60000 integer";
			textBox_MinSpeed.MaxLength = 7;
			textBox_MaxSpeed.MaxLength = 7;
			textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed.ToString();
			textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed.ToString();
			HandleCurveCtrl(byCtrlMode, bEnabled: true);
			break;
		case 1:
			m_byPreCtrlMode = 1;
			label_MinSpeed.Content = "MinCurrent";
			label_MaxSpeed.Content = "MaxCurrent";
			textBox_MinSpeed.ToolTip = null;
			textBox_MaxSpeed.ToolTip = null;
			textBox_MinSpeed.MaxLength = 6;
			textBox_MaxSpeed.MaxLength = 6;
			textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent.ToString("0.000");
			textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent.ToString("0.000");
			HandleCurveCtrl(byCtrlMode, bEnabled: true);
			break;
		case 3:
			m_byPreCtrlMode = 3;
			label_MinSpeed.Content = "MinPower";
			label_MaxSpeed.Content = "MaxPower";
			textBox_MinSpeed.ToolTip = "range:0.000-300.000 float";
			textBox_MaxSpeed.ToolTip = "range:0.000-300.000 float";
			textBox_MinSpeed.MaxLength = 7;
			textBox_MaxSpeed.MaxLength = 7;
			textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower.ToString("0.000");
			textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower.ToString("0.000");
			HandleCurveCtrl(byCtrlMode, bEnabled: true);
			break;
		default:
			if (m_byPreCtrlMode == 2)
			{
				label_MinSpeed.Content = "MinSpeed";
				label_MaxSpeed.Content = "MaxSpeed";
				textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed.ToString();
				textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed.ToString();
			}
			if (m_byPreCtrlMode == 1)
			{
				label_MinSpeed.Content = "MinCurrent";
				label_MaxSpeed.Content = "MaxCurrent";
				textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent.ToString("0.000");
				textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent.ToString("0.000");
			}
			if (m_byPreCtrlMode == 3)
			{
				label_MinSpeed.Content = "MinPower";
				label_MaxSpeed.Content = "MaxPower";
				textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower.ToString("0.000");
				textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower.ToString("0.000");
			}
			if (m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd == 0)
			{
				radioBtn_CurveSpdDis.IsChecked = true;
			}
			else
			{
				radioBtn_CurveSpdEn.IsChecked = true;
			}
			HandleCurveCtrl(byCtrlMode, bEnabled: false);
			break;
		}
		HandleCtrlModeCurveShow();
		return 0;
	}

	private int HandleParamDisplay()
	{
		bool flag = false;
		textBox_JiDuiShu.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu.ToString();
		textBox_DianZu.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu.ToString();
		textBox_DianGan.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan.ToString("F07");
		textBox_FanDianDongShi.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi.ToString("F07");
		textBox_CaiYandDianZu.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu.ToString();
		if (comboBox_FanDaBeiShu.Items.Count != 4)
		{
			comboBox_FanDaBeiShu.Items.Clear();
			comboBox_FanDaBeiShu.Items.Add((string)FindResource("FanDaBeiShu1"));
			comboBox_FanDaBeiShu.Items.Add((string)FindResource("FanDaBeiShu2"));
			comboBox_FanDaBeiShu.Items.Add((string)FindResource("FanDaBeiShu3"));
			comboBox_FanDaBeiShu.Items.Add((string)FindResource("FanDaBeiShu4"));
		}
		comboBox_FanDaBeiShu.SelectedIndex = m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu;
		textBox_RV.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV.ToString();
		if (m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj == 0)
		{
			radioButton_VdcAdj12.IsChecked = true;
		}
		else
		{
			radioButton_VdcAdj65.IsChecked = true;
		}
		if (comboBox_SMPMode.Items.Count != 4)
		{
			comboBox_SMPMode.Items.Clear();
			comboBox_SMPMode.Items.Add((string)FindResource("Cfg_FocSmpMode0"));
			comboBox_SMPMode.Items.Add((string)FindResource("Cfg_FocSmpMode1"));
			comboBox_SMPMode.Items.Add((string)FindResource("Cfg_FocSmpMode3"));
			comboBox_SMPMode.Items.Add((string)FindResource("Cfg_FocSmpMode2"));
		}
		comboBox_SMPMode.SelectedIndex = m_sMotorParamCfg.m_sFocParamCfg.bySMPMode;
		if (m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn == 0)
		{
			radioButton_PllSmo_Dis.IsChecked = true;
		}
		else
		{
			radioButton_PllSmo_En.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn == 0)
		{
			radioButton_StaPll_Dis.IsChecked = true;
		}
		else
		{
			radioButton_StaPll_En.IsChecked = true;
		}
		if (comboBox_Seg7En.Items.Count != 4)
		{
			comboBox_Seg7En.Items.Clear();
			comboBox_Seg7En.Items.Add((string)FindResource("Cfg_FocSeg7En0"));
			comboBox_Seg7En.Items.Add((string)FindResource("Cfg_FocSeg7En1"));
			comboBox_Seg7En.Items.Add((string)FindResource("Cfg_FocSeg7En2"));
			comboBox_Seg7En.Items.Add((string)FindResource("Cfg_FocSeg7En3"));
		}
		comboBox_Seg7En.SelectedIndex = m_sMotorParamCfg.m_sFocParamCfg.bySeg7En;
		if (m_sMotorParamCfg.m_sFocParamCfg.byAtanEn == 0)
		{
			radioButton_Atan_Dis.IsChecked = true;
		}
		else
		{
			radioButton_Atan_En.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sFocParamCfg.byStaSin == 0)
		{
			radioButton_StaSin_Dis.IsChecked = true;
		}
		else
		{
			radioButton_StaSin_En.IsChecked = true;
		}
		string strValPrefix = "Cfg_FocFreqBase";
		ushort wFreqBase = m_sMotorParamCfg.m_sFocParamCfg.wFreqBase;
		InitSliderTicks(slider_BaseFreq, strValPrefix);
		txtBlock_BaseFreq.Text = wFreqBase.ToString();
		strValPrefix = "Cfg_FocSampleTime";
		InitSliderTicks(slider_SampleTime, strValPrefix);
		txtBlock_SampleTime.Text = m_sMotorParamCfg.m_sFocParamCfg.bySampleTime.ToString();
		strValPrefix = "Cfg_FocSPDBw";
		InitSliderTicks(slider_SPDBw, strValPrefix);
		txtBlock_SpdBw.Text = m_sMotorParamCfg.m_sFocParamCfg.bySpdBw.ToString();
		strValPrefix = "Cfg_FocEBwMode";
		InitSliderTicks(slider_EBWMode, strValPrefix);
		txtBlock_EBwMode.Text = m_sMotorParamCfg.m_sFocParamCfg.byEbwMode.ToString();
		strValPrefix = "Cfg_FocLeadAngleSet";
		InitSliderTicks(slider_LeadAngle, strValPrefix);
		txtBlock_LeadAngle.Text = m_sMotorParamCfg.m_sFocParamCfg.fLeadAngleSet.ToString();
		strValPrefix = "Cfg_FocEknum";
		InitSliderTicks(slider_EKNum, strValPrefix);
		txtBlock_EKNum.Text = m_sMotorParamCfg.m_sFocParamCfg.byEkNum.ToString();
		txtBlock_TSMIN.Text = m_sMotorParamCfg.m_sFocParamCfg.byTSMIN.ToString();
		strValPrefix = "Cfg_FocDTSEL";
		InitSliderTicks(slider_DTSEL, strValPrefix);
		txtBlock_DTSEL.Text = m_sMotorParamCfg.m_sFocParamCfg.fDTSEL.ToString();
		txtBlock_KSILDE.Text = m_sMotorParamCfg.m_sFocParamCfg.byKSILDE.ToString();
		textBox_EBw.Text = m_sMotorParamCfg.m_sFocParamCfg.wEbw.ToString();
		textBox_AtoBw.Text = m_sMotorParamCfg.m_sFocParamCfg.fAtoBw.ToString();
		if (comboBox_RPDAlign.Items.Count != 5)
		{
			comboBox_RPDAlign.Items.Clear();
			comboBox_RPDAlign.Items.Add((string)FindResource("Cfg_AlignIPDEn0"));
			comboBox_RPDAlign.Items.Add((string)FindResource("Cfg_AlignIPDEn1"));
			comboBox_RPDAlign.Items.Add((string)FindResource("Cfg_AlignIPDEn2"));
			comboBox_RPDAlign.Items.Add((string)FindResource("Cfg_AlignIPDEn3"));
			comboBox_RPDAlign.Items.Add((string)FindResource("Cfg_AlignIPDEn4"));
		}
		SetRPDAlignSelectedIndex(m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn);
		byte byAlignDuty = m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty;
		slider_AlignDuty.Value = (int)byAlignDuty;
		HandleAlignDutyDisplay(byAlignDuty);
		byte byAlignTime = m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime;
		slider_AlignTime.Value = (int)byAlignTime;
		HandleAlignTimeDisplay(byAlignTime);
		if (m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10 == 0)
		{
			radioBtn_RPD10_6us.IsChecked = true;
		}
		else
		{
			radioBtn_RPD10_4us.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11 == 0)
		{
			radioBtn_RPD11_2us.IsChecked = true;
		}
		else
		{
			radioBtn_RPD11_4us.IsChecked = true;
		}
		textBlock_RPD12.Text = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD123_2.ToString();
		if (m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20 == 0)
		{
			radioBtn_RPD20_6us.IsChecked = true;
		}
		else
		{
			radioBtn_RPD20_4us.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn == 0)
		{
			radioBtn_RvsAlignEn0.IsChecked = true;
		}
		else
		{
			radioBtn_RvsAlignEn1.IsChecked = true;
		}
		textBlock_RPD21.Text = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21.ToString();
		HandleAlignDisplay(m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn);
		if (!AddControlModeAllItems())
		{
			SetControlModeSelectedIndex(m_sMotorParamCfg.m_sStartCfg.byControlMode);
		}
		strValPrefix = "Cfg_StartEfreqAcc";
		InitSliderTicks(slider_EFreQAcc, strValPrefix);
		textBlock_EFreQAcc.Text = m_sMotorParamCfg.m_sStartCfg.byFocEfreqAccx.ToString();
		strValPrefix = "Cfg_StartEfreqMin";
		InitSliderTicks(slider_EFreQMin, strValPrefix);
		textBlock_EFreQMin.Text = m_sMotorParamCfg.m_sStartCfg.fFocEfreqMinx.ToString();
		switch (m_sMotorParamCfg.m_sStartCfg.byCloSpeed)
		{
		case 0:
			radioButton_CloSpeed_1x.IsChecked = true;
			break;
		case 1:
			radioButton_CloSpeed_2x.IsChecked = true;
			break;
		case 2:
			radioButton_CloSpeed_4x.IsChecked = true;
			break;
		default:
			radioButton_CloSpeed_0x.IsChecked = true;
			break;
		}
		switch (m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold)
		{
		case 0:
			radioButton_EFreQHold_1x.IsChecked = true;
			break;
		case 1:
			radioButton_EFreQHold_2x.IsChecked = true;
			break;
		case 2:
			radioButton_EFreQHold_4x.IsChecked = true;
			break;
		default:
			radioButton_EFreQHold_8x.IsChecked = true;
			break;
		}
		switch (m_sMotorParamCfg.m_sRunCfg.byStaIncTime)
		{
		case 0:
			radioButton_StaIncTime_1s.IsChecked = true;
			break;
		case 1:
			radioButton_StaIncTime_2s.IsChecked = true;
			break;
		case 2:
			radioButton_StaIncTime_4s.IsChecked = true;
			break;
		default:
			radioButton_StaIncTime_6s.IsChecked = true;
			break;
		}
		strValPrefix = "Cfg_RunStbInc";
		InitSliderTicks(slider_StbInc, strValPrefix);
		textBlock_StbInc.Text = m_sMotorParamCfg.m_sRunCfg.byStbInc.ToString();
		strValPrefix = "Cfg_RunStaInc";
		InitSliderTicks(slider_StaInc, strValPrefix);
		textBlock_StaInc.Text = m_sMotorParamCfg.m_sRunCfg.byStaInc.ToString();
		textBox_DQKp.Text = m_sMotorParamCfg.m_sRunCfg.fDqKp.ToString();
		textBox_DQKi.Text = m_sMotorParamCfg.m_sRunCfg.fDqKi.ToString();
		textBox_WKp.Text = m_sMotorParamCfg.m_sRunCfg.fWKp.ToString();
		textBox_WKi.Text = m_sMotorParamCfg.m_sRunCfg.fWKi.ToString();
		byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
		if (((bySpdCtrlMd != 3) ? m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys : m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk) == 0)
		{
			radioButton_PwmOnOffhys0.IsChecked = true;
		}
		else
		{
			radioButton_PwmOnOffhys1.IsChecked = true;
		}
		strValPrefix = "Cfg_RunWoutMax";
		InitSliderTicks(slider_WOutMax, strValPrefix);
		textBox_WOutMax.Text = m_sMotorParamCfg.m_sRunCfg.byWoutMax.ToString();
		ushort byWoutMax = m_sMotorParamCfg.m_sRunCfg.byWoutMax;
		HandleWOutMaxDisplay(byWoutMax);
		if (comboBox_RSDMode.Items.Count != 4)
		{
			comboBox_RSDMode.Items.Clear();
			comboBox_RSDMode.Items.Add((string)FindResource("Cfg_WindRSDMode0"));
			comboBox_RSDMode.Items.Add((string)FindResource("Cfg_WindRSDMode1"));
			comboBox_RSDMode.Items.Add((string)FindResource("Cfg_WindRSDMode2"));
			comboBox_RSDMode.Items.Add((string)FindResource("Cfg_WindRSDMode3"));
		}
		SetRSDModeSelectedIndex(m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode);
		strValPrefix = "Cfg_WindStaticSpeed";
		DoubleCollection doubleCollection = new DoubleCollection();
		doubleCollection.Add(0.0);
		doubleCollection.Add(1.0);
		doubleCollection.Add(2.0);
		doubleCollection.Add(3.0);
		doubleCollection.Add(4.0);
		doubleCollection.Add(5.0);
		doubleCollection.Add(6.0);
		doubleCollection.Add(7.0);
		DoubleCollection tickMarks = doubleCollection;
		InitSliderTicks(slider_StaticSpeed, tickMarks);
		textBlock_StaticSpeed.Text = m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed.ToString();
		int freqBaseLevel = GetFreqBaseLevel();
		UpdateStatisSpeedSliderValue((byte)freqBaseLevel, m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed);
		switch (m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed)
		{
		case 0:
			radioBtn_ForwardStaSpeed_1x.IsChecked = true;
			break;
		case 1:
			radioBtn_ForwardStaSpeed_2x.IsChecked = true;
			break;
		case 2:
			radioBtn_ForwardStaSpeed_4x.IsChecked = true;
			break;
		default:
			radioBtn_ForwardStaSpeed_8x.IsChecked = true;
			break;
		}
		strValPrefix = "Cfg_WindRevBrakeSpeed";
		InitSliderTicks(slider_ReverseBrakeSpeed, strValPrefix);
		slider_ReverseBrakeSpeed.Value = (int)m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed;
		DisplayReverseBrakeSpeedText();
		if (m_sMotorParamCfg.m_sWindHandleCfg.byRSD230 == 0)
		{
			radioBtn_ThresholdL.IsChecked = true;
		}
		else
		{
			radioBtn_ThresholdH.IsChecked = true;
		}
		if (comboBox_FaultRestoreTimes.Items.Count != 4)
		{
			comboBox_FaultRestoreTimes.Items.Clear();
			comboBox_FaultRestoreTimes.Items.Add((string)FindResource("Cfg_PtFaultRestoreTimes0"));
			comboBox_FaultRestoreTimes.Items.Add((string)FindResource("Cfg_PtFaultRestoreTimes1"));
			comboBox_FaultRestoreTimes.Items.Add((string)FindResource("Cfg_PtFaultRestoreTimes2"));
			comboBox_FaultRestoreTimes.Items.Add((string)FindResource("Cfg_PtFaultRestoreTimes3"));
		}
		comboBox_FaultRestoreTimes.SelectedIndex = m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes;
		if (m_sMotorParamCfg.m_sProtectCfg.byLock0 == 0)
		{
			radioBtn_Lock0Dis.IsChecked = true;
		}
		else
		{
			radioBtn_Lock0En.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sProtectCfg.byLock1 == 0)
		{
			radioBtn_Lock1Dis.IsChecked = true;
		}
		else
		{
			radioBtn_Lock1En.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sProtectCfg.byLock2 == 0)
		{
			radioBtn_Lock2Dis.IsChecked = true;
		}
		else
		{
			radioBtn_Lock2En.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn == 0)
		{
			radioBtn_LossPhaseDis.IsChecked = true;
		}
		else
		{
			radioBtn_LossPhaseEn.IsChecked = true;
		}
		textBox_LockMinSpeed.Text = m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed.ToString();
		textBox_LockMaxSpeed.Text = m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed.ToString();
		textBox_LPNorhCur.Text = m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent.ToString();
		strValPrefix = "Cfg_PtLockEmfThd";
		strValPrefix = "Cfg_PtLockEmf";
		InitSliderTicks(slider_LockEmf, strValPrefix);
		TextBox_LockEmf.Text = m_sMotorParamCfg.m_sProtectCfg.fLockEmf.ToString();
		if (!AddSpdCtrlModeAllItems())
		{
			SetSpdCtrlModeSelectedIndex(m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd);
		}
		if (m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis == 0)
		{
			radioBtn_PwmOffDis.IsChecked = true;
		}
		else
		{
			radioBtn_PwmOffEn.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn == 0)
		{
			radioBtn_PwmRevDis.IsChecked = true;
		}
		else
		{
			radioBtn_PwmRevEn.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sOhterCfg.byQuickSta == 0)
		{
			radioBtn_QuickStaDis.IsChecked = true;
		}
		else
		{
			radioBtn_QuickStaEn.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd == 0)
		{
			radioBtn_CurveSpdDis.IsChecked = true;
		}
		else
		{
			radioBtn_CurveSpdEn.IsChecked = true;
		}
		if (comboBox_StartClk.Items.Count != 4)
		{
			comboBox_StartClk.Items.Clear();
			comboBox_StartClk.Items.Add((string)FindResource("Cfg_OtherStartClk0"));
			comboBox_StartClk.Items.Add((string)FindResource("Cfg_OtherStartClk1"));
			comboBox_StartClk.Items.Add((string)FindResource("Cfg_OtherStartClk2"));
			comboBox_StartClk.Items.Add((string)FindResource("Cfg_OtherStartClk3"));
		}
		comboBox_StartClk.SelectedIndex = m_sMotorParamCfg.m_sOhterCfg.byStartClk;
		if (comboBox_RsdBrakeEn.Items.Count != 2)
		{
			comboBox_RsdBrakeEn.Items.Clear();
			comboBox_RsdBrakeEn.Items.Add("Disable");
			comboBox_RsdBrakeEn.Items.Add("Enable");
		}
		comboBox_RsdBrakeEn.SelectedIndex = m_sMotorParamCfg.m_sWindHandleCfg.byIsdBrakeEn;
		if (comboBox_FgDiv.Items.Count != 4)
		{
			comboBox_FgDiv.Items.Clear();
			comboBox_FgDiv.Items.Add((string)FindResource("Cfg_OtherFGDiv0"));
			comboBox_FgDiv.Items.Add((string)FindResource("Cfg_OtherFGDiv1"));
			comboBox_FgDiv.Items.Add((string)FindResource("Cfg_OtherFGDiv2"));
			comboBox_FgDiv.Items.Add((string)FindResource("Cfg_OtherFGDiv3"));
		}
		comboBox_FgDiv.SelectedIndex = m_sMotorParamCfg.m_sOhterCfg.byFgDiv;
		if (comboBox_FgMul.Items.Count != 4)
		{
			comboBox_FgMul.Items.Clear();
			comboBox_FgMul.Items.Add((string)FindResource("Cfg_OtherFGMul0"));
			comboBox_FgMul.Items.Add((string)FindResource("Cfg_OtherFGMul1"));
			comboBox_FgMul.Items.Add((string)FindResource("Cfg_OtherFGMul2"));
			comboBox_FgMul.Items.Add((string)FindResource("Cfg_OtherFGMul3"));
		}
		comboBox_FgMul.SelectedIndex = m_sMotorParamCfg.m_sOhterCfg.byFgMul;
		if (comboBox_FgRdMd.Items.Count != 4)
		{
			comboBox_FgRdMd.Items.Clear();
			comboBox_FgRdMd.Items.Add((string)FindResource("Cfg_OtherFgRdMd0"));
			comboBox_FgRdMd.Items.Add((string)FindResource("Cfg_OtherFgRdMd1"));
			comboBox_FgRdMd.Items.Add((string)FindResource("Cfg_OtherFgRdMd2"));
			comboBox_FgRdMd.Items.Add((string)FindResource("Cfg_OtherFgRdMd3"));
		}
		comboBox_FgRdMd.SelectedIndex = m_sMotorParamCfg.m_sOhterCfg.byFgRdMd;
		if (m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet == 0)
		{
			radioBtn_FgIdleSetDis.IsChecked = true;
		}
		else
		{
			radioBtn_FgIdleSetEn.IsChecked = true;
		}
		if (comboBox_SpeedVSet.Items.Count != 4)
		{
			comboBox_SpeedVSet.Items.Clear();
			comboBox_SpeedVSet.Items.Add((string)FindResource("Cfg_ExtendCfgSpeedVSet0"));
			comboBox_SpeedVSet.Items.Add((string)FindResource("Cfg_ExtendCfgSpeedVSet1"));
			comboBox_SpeedVSet.Items.Add((string)FindResource("Cfg_ExtendCfgSpeedVSet2"));
			comboBox_SpeedVSet.Items.Add((string)FindResource("Cfg_ExtendCfgSpeedVSet3"));
		}
		comboBox_SpeedVSet.SelectedIndex = m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet;
		textBox_LimitSpeed.Text = m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed.ToString();
		switch (m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel)
		{
		case 0:
			radioBtn_04s.IsChecked = true;
			break;
		case 1:
			radioBtn_08s.IsChecked = true;
			break;
		case 2:
			radioBtn_12s.IsChecked = true;
			break;
		default:
			radioBtn_16s.IsChecked = true;
			break;
		}
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		m_byPreCtrlMode = m_sMotorParamCfg.m_sTargetSpdCurve.byPreCtrlMode;
		HandleCurveCfgSpeedDisplay(byControlMode);
		strValPrefix = "Cfg_CurveCfgYMaxSel";
		InitSliderTicks(slider_YMaxSel, strValPrefix);
		slider_YMaxSel.Value = (int)m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel;
		textBlock_YMaxSel.Text = 100.0 - 1.5 * (double)(int)m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel + "%";
		if (m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En == 0)
		{
			radioBtn_PwmX98Dis.IsChecked = true;
		}
		else
		{
			radioBtn_PwmX98En.IsChecked = true;
		}
		strValPrefix = "Cfg_CurveCfgXON0";
		InitSliderTicks(slider_XON0, strValPrefix);
		textBox_XON0.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byXON0.ToString();
		strValPrefix = "Cfg_CurveCfgXON";
		InitSliderTicks(slider_XON, strValPrefix);
		textBox_XON.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON.ToString();
		strValPrefix = "Cfg_CurveCfgYON";
		InitSliderTicks(slider_YON, strValPrefix);
		textBox_YON.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON.ToString();
		strValPrefix = "Cfg_CurveCfgY25";
		InitSliderTicks(slider_Y25, strValPrefix);
		textBox_Y25.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_25.ToString();
		strValPrefix = "Cfg_CurveCfgY50";
		InitSliderTicks(slider_Y50, strValPrefix);
		textBox_Y50.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_50.ToString();
		strValPrefix = "Cfg_CurveCfgY75";
		InitSliderTicks(slider_Y75, strValPrefix);
		textBox_Y75.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_75.ToString();
		HandleCtrlModeCurveShow();
		m_clsDlgItemSet45.ClearUIContent();
		m_clsDlgItemSet45.HandleParamDisplay();
		strValPrefix = "Cfg_UserCfgSpdCtrl";
		InitSliderTicks(slider_SpdCtrl, strValPrefix);
		textBox_SpdCtrl.Text = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl.ToString();
		if (m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd == 0)
		{
			radioBtn_dbgmd0.IsChecked = true;
		}
		else if (m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd == 1)
		{
			radioBtn_dbgmd1.IsChecked = true;
		}
		else if (m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd == 2)
		{
			radioBtn_dbgmd2.IsChecked = true;
		}
		else
		{
			radioBtn_dbgmd3.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir == 0)
		{
			radioBtn_MotorDirDis.IsChecked = true;
		}
		else
		{
			radioBtn_MotorDirEn.IsChecked = true;
		}
		if (m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn == 0)
		{
			radioBtn_PwmDis.IsChecked = true;
		}
		else
		{
			radioBtn_PwmEn.IsChecked = true;
		}
		if (comboBox_FocDebug12.Items.Count != 6)
		{
			comboBox_FocDebug12.Items.Clear();
			comboBox_FocDebug12.Items.Add((string)FindResource("Cfg_UserCfgFDS000"));
			comboBox_FocDebug12.Items.Add((string)FindResource("Cfg_UserCfgFDS001"));
			comboBox_FocDebug12.Items.Add((string)FindResource("Cfg_UserCfgFDS010"));
			comboBox_FocDebug12.Items.Add((string)FindResource("Cfg_UserCfgFDS011"));
			comboBox_FocDebug12.Items.Add((string)FindResource("Cfg_UserCfgFDS0100"));
			comboBox_FocDebug12.Items.Add((string)FindResource("Cfg_UserCfgFDS0101"));
		}
		comboBox_FocDebug12.SelectedIndex = m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0;
		if (comboBox_FocDebug34.Items.Count != 5)
		{
			comboBox_FocDebug34.Items.Clear();
			comboBox_FocDebug34.Items.Add((string)FindResource("Cfg_UserCfgFDS100"));
			comboBox_FocDebug34.Items.Add((string)FindResource("Cfg_UserCfgFDS101"));
			comboBox_FocDebug34.Items.Add((string)FindResource("Cfg_UserCfgFDS110"));
			comboBox_FocDebug34.Items.Add((string)FindResource("Cfg_UserCfgFDS111"));
			comboBox_FocDebug34.Items.Add((string)FindResource("Cfg_UserCfgFDS1100"));
		}
		comboBox_FocDebug34.SelectedIndex = m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1;
		byte bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
		string text = $"0x{bySlaveId:X2}";
		textBox_SlvID.Text = text;
		bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1;
		text = $"0x{bySlaveId:X2}";
		textBox_UID1.Text = text;
		textBox_SetChipID.Text = text;
		bySlaveId = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty;
		textBox_StartDuty.Text = bySlaveId.ToString();
		bySlaveId = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration;
		textBox_StartDurationTime.Text = bySlaveId.ToString();
		bySlaveId = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty;
		textBox_StopDuty.Text = bySlaveId.ToString();
		bySlaveId = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration;
		textBox_StopDurationTime.Text = bySlaveId.ToString();
		ushort wTotalSetCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt;
		textBox_TotalOnOffCnt.Text = wTotalSetCnt.ToString();
		textBox_DoneCnt.Text = ((ushort)0).ToString();
		textBox_FailCnt.Text = ((ushort)0).ToString();
		CacluCurrenAndSpeedBase();
		textBox_MinSpeed.ToolTip = $"range:0-{(int)(m_fMotorSpeedBase / 2f):d} integer";
		textBox_MaxSpeed.ToolTip = $"range:0-{(int)m_fMotorSpeedBase:d} integer";
		if ((radioBtn_HallSvMdEn.IsChecked == true) & (comboBox_ContrlMode.SelectedValue.ToString() == "VoltageLoop"))
		{
			EnableCurveSpdDis(isEnabled: true);
		}
		textblock_BoardCurrBase.Text = m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Curr_Base;
		textblock_BoardVolBase2.Text = m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Vol_Base2;
		textblock_MotorPowerBase.Text = m_sMotorParamCfg.m_sShowBASECfg.strMotor_Power_Base;
		if (comboBox_IdleBrakeEn.Items.Count != 2)
		{
			comboBox_IdleBrakeEn.Items.Clear();
			comboBox_IdleBrakeEn.Items.Add((string)FindResource("Cfg_UserEnable0"));
			comboBox_IdleBrakeEn.Items.Add((string)FindResource("Cfg_UserEnable1"));
		}
		comboBox_IdleBrakeEn.SelectedIndex = m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn;
		if (adWindow.comboBox_restaEn.Items.Count != 2)
		{
			adWindow.comboBox_restaEn.Items.Clear();
			adWindow.comboBox_restaEn.Items.Add((string)FindResource("Cfg_UserRestaEn0"));
			adWindow.comboBox_restaEn.Items.Add((string)FindResource("Cfg_UserRestaEn1"));
		}
		adWindow.comboBox_restaEn.SelectedIndex = m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn;
		if (adWindow.comboBox_IsdFwMd.Items.Count != 2)
		{
			adWindow.comboBox_IsdFwMd.Items.Clear();
			adWindow.comboBox_IsdFwMd.Items.Add((string)FindResource("Cfg_UserIsdFwMd0"));
			adWindow.comboBox_IsdFwMd.Items.Add((string)FindResource("Cfg_UserIsdFwMd1"));
		}
		adWindow.comboBox_IsdFwMd.SelectedIndex = m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd;
		if (adWindow.comboBox_lamd.Items.Count != 2)
		{
			adWindow.comboBox_lamd.Items.Clear();
			adWindow.comboBox_lamd.Items.Add((string)FindResource("Cfg_UserLaMd0"));
			adWindow.comboBox_lamd.Items.Add((string)FindResource("Cfg_UserLaMd1"));
		}
		adWindow.comboBox_lamd.SelectedIndex = m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd;
		if (adWindow.comboBox_newFunc.Items.Count != 2)
		{
			adWindow.comboBox_newFunc.Items.Clear();
			adWindow.comboBox_newFunc.Items.Add("Enable");
			adWindow.comboBox_newFunc.Items.Add("Disable");
		}
		adWindow.comboBox_newFunc.SelectedIndex = m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc;
		return 0;
	}

	private bool UpdateUserConfigToUI()
	{
		m_byIsLoadDefault = 1;
		HandleParamDisplay();
		InitStatusReg();
		return true;
	}

	private int ClearStatusReg()
	{
		m_byMotorStatus = 0;
		m_byFaultStatus = 0;
		m_wRunningSpeed = 0;
		m_wTheta = 0;
		m_wOmega = 0;
		m_wUDCVal = 0;
		m_wWOUTVal = 0;
		m_wUQVal = 0;
		m_wUDVal = 0;
		m_wIA_MaxVal = 0;
		m_wIB_MaxVal = 0;
		m_wIC_MaxVal = 0;
		m_wIA_ARefVal = 0;
		m_wIB_ARefVal = 0;
		m_wIC_ARefVal = 0;
		m_wEmfVal = 0;
		m_wPwmReal = 0;
		m_wIQFDBVal = 0;
		m_wTimArrVal = 0;
		m_wTrip = 0;
		m_wPower = 0;
		m_wHallOmega = 0;
		m_wRsv = 0;
		m_wIPDPositionVal = 0;
		return 0;
	}

	private void InitDownLoadStautsDisplay(bool blVal)
	{
		ellipse_Verify.IsEnabled = blVal;
	}

	private void InitStautsDisplay()
	{
		ellipse_DuZhan.IsEnabled = false;
		ellipse_LossPhase.IsEnabled = false;
		ellipse_UnderVoltage.IsEnabled = false;
		ellipse_OverVoltage.IsEnabled = false;
		ellipse_OverTemp.IsEnabled = false;
		ellipse_OverCurrent.IsEnabled = false;
		ellipse_IrefErr.IsEnabled = false;
		ellipse_HallAbn.IsEnabled = false;
		ellipse_DuZhan.Background = Brushes.LightGray;
		ellipse_DuZhan.BorderBrush = Brushes.LightGray;
		ellipse_LossPhase.Background = Brushes.LightGray;
		ellipse_LossPhase.BorderBrush = Brushes.LightGray;
		ellipse_UnderVoltage.Background = Brushes.LightGray;
		ellipse_UnderVoltage.BorderBrush = Brushes.LightGray;
		ellipse_OverVoltage.Background = Brushes.LightGray;
		ellipse_OverVoltage.BorderBrush = Brushes.LightGray;
		ellipse_OverTemp.Background = Brushes.LightGray;
		ellipse_OverTemp.BorderBrush = Brushes.LightGray;
		ellipse_OverCurrent.Background = Brushes.LightGray;
		ellipse_OverCurrent.BorderBrush = Brushes.LightGray;
		ellipse_IrefErr.Background = Brushes.LightGray;
		ellipse_IrefErr.BorderBrush = Brushes.LightGray;
		ellipse_HallAbn.Background = Brushes.LightGray;
		ellipse_HallAbn.BorderBrush = Brushes.LightGray;
	}

	private void FaultLEDEnable(int byVal)
	{
		switch (byVal)
		{
		case 0:
			ellipse_DuZhan.IsEnabled = true;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 1:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = true;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 2:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = true;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 3:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = true;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 4:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = true;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 5:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = true;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 6:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = true;
			ellipse_HallAbn.IsEnabled = false;
			break;
		case 7:
			ellipse_DuZhan.IsEnabled = false;
			ellipse_LossPhase.IsEnabled = false;
			ellipse_OverVoltage.IsEnabled = false;
			ellipse_UnderVoltage.IsEnabled = false;
			ellipse_OverTemp.IsEnabled = false;
			ellipse_OverCurrent.IsEnabled = false;
			ellipse_IrefErr.IsEnabled = false;
			ellipse_HallAbn.IsEnabled = true;
			break;
		}
	}

	private bool GetFaultStringByIndex(StringBuilder strBuildTmp, byte byIndex)
	{
		string text = strBuildTmp.ToString();
		int num = 0;
		byte b = 0;
		InitStautsDisplay();
		if (byIndex == 0)
		{
			return true;
		}
		for (num = 0; num < 8; num++)
		{
			b = (byte)(byIndex & (1 << num));
			if (b > 0)
			{
				FaultLEDEnable(num);
				switch (num)
				{
				case 0:
					text += "Stall";
					strBuildTmp.Append("Stall");
					ellipse_DuZhan.Background = Brushes.Red;
					ellipse_DuZhan.BorderBrush = Brushes.Red;
					ellipse_DuZhan.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 1:
					text += "LossPhase";
					strBuildTmp.Append("LossPhase");
					ellipse_LossPhase.Background = Brushes.Red;
					ellipse_LossPhase.BorderBrush = Brushes.Red;
					ellipse_LossPhase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 2:
					text += "OVOL";
					strBuildTmp.Append("OVOL");
					ellipse_OverVoltage.Background = Brushes.Red;
					ellipse_OverVoltage.BorderBrush = Brushes.Red;
					ellipse_OverVoltage.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 3:
					text += "UVOL";
					strBuildTmp.Append("UVOL");
					ellipse_UnderVoltage.Background = Brushes.Red;
					ellipse_UnderVoltage.BorderBrush = Brushes.Red;
					ellipse_UnderVoltage.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 4:
					text += "TSD";
					strBuildTmp.Append("TSD");
					ellipse_OverTemp.Background = Brushes.Red;
					ellipse_OverTemp.BorderBrush = Brushes.Red;
					ellipse_OverTemp.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 5:
					text += "OCP";
					strBuildTmp.Append("OCP");
					ellipse_OverCurrent.Background = Brushes.Red;
					ellipse_OverCurrent.BorderBrush = Brushes.Red;
					ellipse_OverCurrent.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 6:
					text += "IREF_ERR";
					strBuildTmp.Append("IREF_ERR");
					ellipse_IrefErr.Background = Brushes.Red;
					ellipse_IrefErr.BorderBrush = Brushes.Red;
					ellipse_IrefErr.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case 7:
					text += "hall_abn";
					strBuildTmp.Append("hall_abn");
					ellipse_HallAbn.Background = Brushes.Red;
					ellipse_HallAbn.BorderBrush = Brushes.Red;
					ellipse_HallAbn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				}
			}
		}
		return true;
	}

	private bool GetMotorStatusByIndex(StringBuilder strBuildTmp, byte byIndex)
	{
		string text = strBuildTmp.ToString();
		string value = strBuildTmp.ToString();
		byIndex &= 0xF;
		switch (byIndex)
		{
		case 0:
			value = "IDLE";
			break;
		case 1:
			value = "INITIAL";
			break;
		case 2:
			value = "RSD";
			break;
		case 3:
			value = "START";
			break;
		case 4:
			value = "RUN";
			break;
		case 5:
			value = "BRAKE";
			break;
		case 6:
			value = "ALIGN";
			break;
		case 7:
			value = "FAULT";
			break;
		case 8:
			value = "SLEEP";
			break;
		case 9:
			value = "RPD";
			break;
		}
		int length = text.Length;
		if (length > 0)
		{
			strBuildTmp.Remove(0, length);
		}
		strBuildTmp.Append(value);
		return true;
	}

	private void InitStatusReg()
	{
		textBox_MotorStatus.Text = "0x" + m_byMotorStatus.ToString("X2");
		textBox_FaultStatus.Text = m_byFaultStatus.ToString();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = 30;
		GetMotorStatusByIndex(stringBuilder, 0);
		lblMessage.Content = stringBuilder.ToString();
		textBox_TargetSpeed.Text = m_wRunningSpeed.ToString();
		string text = $"0x{m_wTheta:X4}";
		textBox_THETA.Text = text;
		text = $"0x{m_wOmega:X4}";
		textBox_OMEGA.Text = text;
		text = $"0x{m_wUDCVal:X4}";
		textBox_UDC.Text = text;
		text = $"0x{m_wWOUTVal:X4}";
		textBox_WOUT.Text = text;
		text = $"0x{m_wUQVal:X4}";
		textBox_UQ.Text = text;
		text = $"0x{m_wUDVal:X4}";
		textBox_UD.Text = text;
		text = $"0x{m_wIA_MaxVal:X4}";
		textBox_IA_MAX.Text = text;
		text = $"0x{m_wIB_MaxVal:X4}";
		textBox_IB_MAX.Text = text;
		text = $"0x{m_wIC_MaxVal:X4}";
		textBox_IC_MAX.Text = text;
		text = $"0x{m_wIA_ARefVal:X4}";
		textBox_IA_AREF.Text = text;
		text = $"0x{m_wIB_ARefVal:X4}";
		textBox_IB_AREF.Text = text;
		text = $"0x{m_wIC_ARefVal:X4}";
		textBox_IC_AREF.Text = text;
		text = $"0x{m_wEmfVal:X4}";
		textBox_EMF.Text = text;
		text = $"0x{m_wPwmReal:X4}";
		textBox_PWM_REAL.Text = text;
		text = $"0x{m_wIQFDBVal:X4}";
		textBox_IQFDB.Text = text;
		text = $"0x{m_wTimArrVal:X4}";
		textBox_tim_arr.Text = text;
		text = $"0x{m_wTrip:X4}";
		textBox_ITRIP.Text = text;
		text = $"0x{m_wPower:X4}";
		textBox_POWER.Text = text;
		text = $"0x{m_wHallOmega:X4}";
		textBox_HallOmega.Text = text;
		text = $"0x{m_wRsv:X4}";
		textBox_RSV.Text = text;
		text = $"0x{m_wIPDPositionVal:X4}";
		textBox_IPDPOSITION.Text = text;
		text = $"0x{m_wSpdCtrl:X4}";
		textBox_StatusReg_SpdCtrl.Text = text;
	}

	private bool LoadDefaultConfig()
	{
		int num = 0;
		m_byIsLoadDefault = 1;
		if (m_i16SensorMode == 1)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory;
			switch (m_byCircuitType)
			{
			case 0:
				text += "conf\\FT8132_S_No1_3HI_1R.pfd";
				break;
			case 1:
				text += "conf\\FT8132_S_No2_3HS_1R.pfd";
				break;
			case 2:
				text += "conf\\FT8132_S_No3_1HS_1R.pfd";
				break;
			case 3:
				text += "conf\\FT8132_S_No4_2HS_NTC.pfd";
				break;
			case 4:
				text += "conf\\FT8132_S_No5_3HI_2R.pfd";
				break;
			case 5:
				text = (((Application.Current as App).eArgsLen <= 1) ? (text + "conf\\FT8132_S_No6_2HS_2R.pfd") : (text + "conf\\FT8132_S_No6_2HS_2R_asic.pfd"));
				break;
			case 6:
				text += "conf\\FT8132_S_No7_3HI_3R.pfd";
				break;
			}
			if (File.Exists(text))
			{
				num = OpenSpecifyConfig(text);
				if (num != 0)
				{
					if (num == 3)
					{
						MessageBox.Show("Please open the configuration file of FT8132 Debugger!", "Tips", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
					}
					return false;
				}
				m_byIsLoadDefault = 0;
			}
		}
		else if (m_i16SensorMode == 2)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory;
			switch (m_byCircuitType)
			{
			case 0:
				text += "conf\\FT8132_SL_No1_1R.pfd";
				break;
			case 1:
				text += "conf\\FT8132_SL_No2_1R.pfd";
				break;
			case 2:
				text = (((Application.Current as App).eArgsLen <= 1) ? (text + "conf\\FT8132_SL_No3_2R.pfd") : (text + "conf\\FT8132_SL_No3_2R_asic.pfd"));
				break;
			case 3:
				text += "conf\\FT8132_SL_No4_3R.pfd";
				break;
			}
			if (File.Exists(text))
			{
				num = OpenSpecifyConfig(text);
				if (num != 0)
				{
					if (num == 3)
					{
						MessageBox.Show("Please open the configuration file of FT8132 Debugger!", "Tips", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
					}
					return false;
				}
				m_byIsLoadDefault = 0;
			}
		}
		else if (m_i16SensorMode == 3)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory;
			switch (m_byCircuitType)
			{
			case 0:
				text += "conf\\FT8132_2HIC_CoolFan_1R.pfd";
				break;
			case 1:
				text += "conf\\FT8132_2HIC_CoolFan_2R.pfd";
				break;
			}
			if (File.Exists(text))
			{
				num = OpenSpecifyConfig(text);
				if (num != 0)
				{
					if (num == 3)
					{
						MessageBox.Show("Please open the configuration file of FT8132 Debugger!", "Tips", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
					}
					return false;
				}
				m_byIsLoadDefault = 0;
			}
		}
		else if (m_i16SensorMode == 4)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "conf\\FT8132_RoboticVacuum.pfd";
			if (File.Exists(text))
			{
				num = OpenSpecifyConfig(text);
				if (num != 0)
				{
					if (num == 3)
					{
						MessageBox.Show("Please open the configuration file of FT8132 Debugger!", "Tips", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
					}
					return false;
				}
				m_byIsLoadDefault = 0;
			}
		}
		if (m_byIsLoadDefault != 0)
		{
			Init2DefaultConfig();
			m_byIsLoadDefault = 0;
		}
		HandleParamDisplay();
		InitStatusReg();
		m_blIsSave2HallUC = false;
		ConverUIToParam();
		return true;
	}

	public int SaveSpecifyConfig(string filename)
	{
		int num = 0;
		if (commonConfigSetting == null)
		{
			return 1;
		}
		commonConfigSetting.m_sAppVersion = base.Title.Substring(base.Title.IndexOf(".") - 1);
		num = commonConfigSetting.SaveConfig(filename);
		if (num != 0)
		{
			return num;
		}
		return num;
	}

	public void SetSensorModeCircuitType(short sensorMode, byte CircuitType)
	{
		m_i16SensorMode = sensorMode;
		m_byCircuitType = CircuitType;
	}

	public void SetSensorModeCircuitType(short sensorMode, byte CircuitType, bool isInCommand)
	{
		m_i16SensorMode = sensorMode;
		m_byCircuitType = CircuitType;
		m_isInCommand = isInCommand;
	}

	public void SetReloadFlag(bool reload)
	{
		m_bReload = reload;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		try
		{
			if (m_isInitialized)
			{
				int num = 0;
				string currentConfig = Settings.Default.CurrentConfig;
				m_blIsSave2HallUC = true;
				ConverUIToParam();
				UpdateUIDataToUserCfg();
				commonConfigSetting.m_byLogEn = 0;
				if (checkBox_SimulateComm.IsChecked == true)
				{
					commonConfigSetting.m_byLogEn = 1;
				}
				string text = AppDomain.CurrentDomain.BaseDirectory + "\\UserConfigFile\\FTClosedConfig.pfd";
				if (SaveSpecifyConfig(text) == 0)
				{
					Settings.Default.CurrentConfig = text;
					Settings.Default.Save();
				}
				OnTimer.Stop();
				OffTimer.Stop();
				if (timeThread.ThreadState == System.Threading.ThreadState.Suspended)
				{
					m_bFinishedTest = true;
					m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt = 0;
					timeThread.Resume();
				}
				timeThread.Abort();
				if (timeThread.IsAlive)
				{
					timeThread.Join();
				}
			}
		}
		catch (Exception ex)
		{
			LOG.WriteMsg(ex.Message);
		}
		m_isWindowClosing = true;
		Application.Current.Shutdown();
		Environment.Exit(16711780);
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.F) && Keyboard.IsKeyDown(Key.T))
		{
			LoginWindow loginWindow = new LoginWindow();
			loginWindow.ShowDialog();
			loginWindow.Close();
		}
	}

	private void menuOpen_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\UserConfigFile";
		openFileDialog.Filter = "pfd files(*.Pfd,*.pfd)|*.Pfd;*pfd|All files (*.*)|*.*";
		openFileDialog.FilterIndex = 1;
		openFileDialog.RestoreDirectory = true;
		bool? flag = true;
		if (openFileDialog.ShowDialog() != flag)
		{
			return;
		}
		int num2 = 0;
		switch (OpenSpecifyConfig(openFileDialog.FileName))
		{
		case 3:
			MessageBox.Show("Please open the configuration file of FT8132 Debugger!", "Tips", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
			break;
		default:
		{
			string sensorModeText = GetSensorModeText(m_i16SensorMode);
			string circuitTypeText = GetCircuitTypeText(m_i16SensorMode, m_byCircuitType);
			string sensorModeText2 = GetSensorModeText(commonConfigSetting.m_i16SensorMode);
			string circuitTypeText2 = GetCircuitTypeText(commonConfigSetting.m_i16SensorMode, commonConfigSetting.m_byCircuitType);
			string text = "The sensor mode or circuit type is not match!\r\n\r\n";
			text += $"Current mode is \"{sensorModeText:s}\", circuit type is \"{circuitTypeText:s}\".\r\n\r\n";
			text += $"But, in config file, the mode is \"{sensorModeText2:s}\", circuit type is \"{circuitTypeText2:s}\".";
			MessageBox.Show(text, "NOT MATCH", MessageBoxButton.OK, MessageBoxImage.Hand);
			break;
		}
		case 0:
			UpdateUserConfigToUI();
			checkBox_SimulateComm.IsChecked = false;
			if (commonConfigSetting.m_byLogEn == 1)
			{
				checkBox_SimulateComm.IsChecked = true;
			}
			CircuitTypeParamDisplay(m_i16SensorMode, m_byCircuitType);
			if (m_i16SensorMode == 3)
			{
				m_uc2HallICCoolFan_s1.MainWinTouc2hallIcCoolFans1();
				m_uc2HallICCoolFan_s2.MainWinTouc2hallIcCoolFans2();
			}
			else if (m_i16SensorMode == 4)
			{
				m_SensorlessUC.MainWindowToSaoDiBao();
			}
			else if (m_i16SensorMode == 1 && m_byCircuitType == 5 && m_2Hall_sensor_2Res_FOC_ASUC != null)
			{
				m_2Hall_sensor_2Res_FOC_ASUC.InitHallMosel();
			}
			break;
		}
	}

	private void MenuLoadDefaultCfg_Click(object sender, RoutedEventArgs e)
	{
		bool flag = true;
		ClearUIContent();
		if (LoadDefaultConfig())
		{
			if (m_i16SensorMode == 3)
			{
				m_uc2HallICCoolFan_s1.MainWinTouc2hallIcCoolFans1();
				m_uc2HallICCoolFan_s2.MainWinTouc2hallIcCoolFans2();
			}
			else if (m_i16SensorMode == 4)
			{
				m_SensorlessUC.MainWindowToSaoDiBao();
			}
			else if (m_i16SensorMode == 1 && m_byCircuitType == 5 && m_2Hall_sensor_2Res_FOC_ASUC != null)
			{
				m_2Hall_sensor_2Res_FOC_ASUC.InitHallMosel();
				comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
				comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode1"));
				comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
				SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			}
		}
	}

	private void MenuDebug_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		if (!btnDebug.IsEnabled)
		{
			MessageBox.Show("please connect device...", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
		}
		else
		{
			num = ExecuteDebugOperation();
		}
	}

	private void MenuStop_Click(object sender, RoutedEventArgs e)
	{
		if (!btnStartBw.IsEnabled)
		{
			MessageBox.Show("please connect device...", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
		}
		else
		{
			ExecuteStopOperation();
		}
	}

	private void menuAbout_Click(object sender, RoutedEventArgs e)
	{
		TestCode();
		string title = base.Title;
		MessageBox.Show(title + "\r\nwww.fortiortech.com");
	}

	private void StartExcelProcess()
	{
		string fileName = "E:\\Software\\Prod_FT8131Debugger\\Src\\ExeProject\\Prod_FT8131Debugger\\bin\\Debug\\temp\\test01.csv";
		Process.Start(fileName);
	}

	private void TestCode()
	{
		RecordOnOffError2CSV("usb communication fail.");
	}

	private void TestStringEmpty(string str)
	{
		if (str == null)
		{
			str = ((!(str == "")) ? "string is null" : "");
		}
		else if (str == "")
		{
			str = ((!(str == string.Empty)) ? "" : "string is empty");
		}
		else if (str == string.Empty)
		{
			str = ((!(str == "")) ? "string is empty" : "");
		}
		string text = str;
	}

	private void MenuSave_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		int num2 = 0;
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "pfd files(*.pfd)|*.pfd";
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		SaveFileDialog saveFileDialog2 = saveFileDialog;
		m_blIsSave2HallUC = true;
		ConverUIToParam();
		commonConfigSetting.m_byLogEn = 0;
		if (checkBox_SimulateComm.IsChecked == true)
		{
			commonConfigSetting.m_byLogEn = 1;
		}
		bool? flag = true;
		if (saveFileDialog2.ShowDialog() == flag)
		{
			UpdateUIDataToUserCfg();
			num2 = SaveSpecifyConfig(saveFileDialog2.FileName.ToString());
		}
	}

	private void textBox_SlvID_LostFocus(object sender, RoutedEventArgs e)
	{
		if (m_isWindowClosing)
		{
			return;
		}
		int num = 0;
		string text = textBox_SlvID.Text;
		try
		{
			byte b = Convert.ToByte(text, 16);
			if (b > 127)
			{
				MessageBox.Show("invalid value, please input hex format integer between 0x00 and 0x07.");
				num = 1;
			}
			else
			{
				adWindow.comboBox_newFunc.SelectedIndex = 0;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input hex format integer between 0x00 and 0x07.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				byte bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
				string text2 = $"0x{bySlaveId:X2}";
				textBox_SlvID.Text = text2;
			}
			else
			{
				textBox_SlvID.Text = (string)FindResource("Cfg_UserCfgSlvID");
			}
		}
	}

	private void textBox_UID1_LostFocus(object sender, RoutedEventArgs e)
	{
	}

	private void btnDebug_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		m_eBtnOptType = E_BtnOptType.eBtn_OPT_DEBUG;
		num = ExecuteDebugOperation();
	}

	private int ExecuteDebugOperation()
	{
		int num = 0;
		num = ExecuteDebugOpt();
		if (num == 0)
		{
			m_byDebugFlag = 1;
			if (checkBox_SimulateComm.IsChecked == true)
			{
				LogWriteDataToTxt();
			}
		}
		return num;
	}

	private bool LogWriteDataToTxt()
	{
		bool result = true;
		DllDebuggerAPI.S_PreReg sPregParm = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		int num = 0;
		sPregParm.byRegisterNum = sPreReg.byRegisterNum;
		for (num = 0; num < sPreReg.byRegisterNum; num++)
		{
			sPregParm.adwRegAdd[num] = sPreReg.adwRegAdd[num];
			sPregParm.adwRegVal[num] = sPreReg.adwRegVal[num];
		}
		string path = AppDomain.CurrentDomain.BaseDirectory + "\\log\\logWriteReg.txt";
		WriteArrayToTxt(sPregParm, path);
		return result;
	}

	public bool LogReadDataToTxt()
	{
		bool result = true;
		DllDebuggerAPI.S_PreReg sPregParm = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		int num = 0;
		sPregParm.byRegisterNum = sPreReg.byRegisterNum;
		for (num = 0; num < sPreReg.byRegisterNum; num++)
		{
			sPregParm.adwRegAdd[num] = 0u;
			sPregParm.adwRegVal[num] = 0u;
			sPregParm.adwRegAdd[num] = sPreReg.adwRegAdd[num];
			sPregParm.adwRegVal[num] = sPreReg.adwRegVal[num];
		}
		string path = AppDomain.CurrentDomain.BaseDirectory + "\\log\\logReadReg.txt";
		WriteArrayToTxt(sPregParm, path);
		return result;
	}

	private bool LogWriteEfuseToTxt()
	{
		bool result = true;
		DllDebuggerAPI.S_PreReg sPregParm = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		int num = 0;
		sPregParm.byRegisterNum = sPreRReg.byRegisterNum;
		for (num = 0; num < sPreRReg.byRegisterNum; num++)
		{
			sPregParm.adwRegAdd[num] = sPreRReg.adwRegAdd[num];
			sPregParm.adwRegVal[num] = sPreRReg.adwRegVal[num];
		}
		string path = AppDomain.CurrentDomain.BaseDirectory + "\\log\\logWriteEfuse.txt";
		WriteArrayToTxt(sPregParm, path);
		return result;
	}

	private bool LogReadEfueToTxt()
	{
		bool result = true;
		DllDebuggerAPI.S_PreReg sPregParm = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		int num = 0;
		sPregParm.byRegisterNum = sPreRReg.byRegisterNum;
		for (num = 0; num < sPreRReg.byRegisterNum; num++)
		{
			sPregParm.adwRegAdd[num] = sPreRReg.adwRegAdd[num];
			sPregParm.adwRegVal[num] = m_byREfuseArr[num];
		}
		string path = AppDomain.CurrentDomain.BaseDirectory + "\\log\\logReadEfuse.txt";
		WriteArrayToTxt(sPregParm, path);
		return result;
	}

	private bool ArrayToTxt(string[] array, string path)
	{
		if (path == null || array == null)
		{
			return false;
		}
		try
		{
			string text = "";
			foreach (string text2 in array)
			{
				text += text2;
				text += "|";
			}
			text = text.Substring(0, text.Length - 1);
			FileStream fileStream = new FileStream(path, FileMode.Create);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			streamWriter.Write(text);
			streamWriter.Flush();
			streamWriter.Close();
			fileStream.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private string[] TxtToArray(string path)
	{
		if (path == null)
		{
			return null;
		}
		try
		{
			string text = "";
			StreamReader streamReader = new StreamReader(path);
			text = streamReader.ReadToEnd();
			streamReader.Close();
			return text.Split('|');
		}
		catch (Exception)
		{
			return null;
		}
	}

	private bool WriteArrayToTxt(DllDebuggerAPI.S_PreReg sPregParm, string path)
	{
		if (path == null)
		{
			return false;
		}
		try
		{
			uint num = 0u;
			uint num2 = 0u;
			string text = "";
			string text2 = "";
			string text3 = "";
			if (true)
			{
				text += "        FortiorTech  FT8132  Debugger    \r\n";
				text += "----------------------------------------------------------------\r\n";
				text += "      Type      Value";
				text += "\r\n";
				text += "----------------------------------------------------------------\r\n";
				bool flag = false;
			}
			int num3 = 0;
			for (num3 = 0; num3 < sPregParm.byRegisterNum; num3++)
			{
				num = sPregParm.adwRegAdd[num3];
				num2 = sPregParm.adwRegVal[num3];
				byte b = (byte)num;
				string text4 = $"   Reg[0x{b:X2}]";
				text2 = text4;
				b = (byte)num2;
				text4 = $" 0x{b:X2}";
				text3 = text4;
				text += text2;
				text += " = ";
				text += text3;
				text += "\r\n";
			}
			text = text.Substring(0, text.Length - 1);
			FileStream fileStream = new FileStream(path, FileMode.Create);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			streamWriter.Write(text);
			streamWriter.Flush();
			streamWriter.Close();
			fileStream.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private string[] ReadTxtToArray(string path)
	{
		if (path == null)
		{
			return null;
		}
		try
		{
			string text = "";
			StreamReader streamReader = new StreamReader(path);
			text = streamReader.ReadToEnd();
			streamReader.Close();
			return text.Split('|');
		}
		catch (Exception)
		{
			return null;
		}
	}

	private int InitParamBeforeDebug()
	{
		byte bySpdCtrl = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl;
		byte byMotorDir = m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir;
		m_blIsSave2HallUC = true;
		ConverUIToParam();
		byte b = Convert.ToByte(textBox_SlvID.Text, 16);
		if (b == 127)
		{
			adWindow.comboBox_newFunc.SelectedIndex = 1;
			m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = 1;
		}
		else
		{
			adWindow.comboBox_newFunc.SelectedIndex = 0;
			m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = 0;
		}
		if (m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn == 1 || m_eBtnOptType == E_BtnOptType.eBtn_OPT_STOP)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = bySpdCtrl;
			if (checkBox_DirSwitch.IsChecked == true)
			{
				m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = byMotorDir;
			}
		}
		bool flag = false;
		if (!CalculateParam())
		{
			return 1;
		}
		int num = 0;
		sPreReg.byRegisterNum = 57;
		byte b2 = 0;
		for (num = 0; num < sPreReg.byRegisterNum; num++)
		{
			if (num > 51)
			{
				sPreReg.adwRegAdd[num] = num switch
				{
					52 => 4u, 
					53 => 6u, 
					54 => 2u, 
					55 => 3u, 
					_ => 242u, 
				};
				sPreReg.adwRegVal[num] = m_byEfuseArr[num];
			}
			else
			{
				sPreReg.adwRegAdd[num] = (uint)(134 + num);
				sPreReg.adwRegVal[num] = m_byEfuseArr[num];
			}
		}
		return 0;
	}

	private int ExecuteDebugOpt()
	{
		int num = 1;
		bool flag = false;
		string text = "";
		num = InitParamBeforeDebug();
		if (num != 0)
		{
			return num;
		}
		bool? isChecked = checkBox_SimulateComm.IsChecked;
		if (isChecked != true || isChecked.HasValue)
		{
		}
		bool flag2 = true;
		statusBar1.Items.Clear();
		TextBlock textBlock = new TextBlock();
		textBlock.Text = "Device:";
		textBlock.Width = 50.0;
		textBlock.HorizontalAlignment = HorizontalAlignment.Left;
		statusBar1.Items.Add(textBlock);
		TextBlock textBlock2 = new TextBlock();
		textBlock2.Text = "Debugging...";
		textBlock2.Width = 80.0;
		textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
		textBlock2.VerticalAlignment = VerticalAlignment.Stretch;
		statusBar1.Items.Add(textBlock2);
		TextBlock textBlock3 = new TextBlock();
		textBlock3.Text = "Elaps Time:";
		textBlock3.Width = 80.0;
		statusBar1.Items.Add(textBlock3);
		TextBlock textBlock4 = new TextBlock();
		textBlock4.Text = "00:00:00";
		textBlock4.Width = 80.0;
		statusBar1.Items.Add(textBlock4);
		Ellipse ellipse = new Ellipse();
		ellipse.HorizontalAlignment = HorizontalAlignment.Right;
		ellipse.VerticalAlignment = VerticalAlignment.Center;
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = Brushes.YellowGreen;
		statusBar1.Items.Add(ellipse);
		TextBlock textBlock5 = new TextBlock();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = 30;
		GetMotorStatusByIndex(stringBuilder, 4);
		textBlock5.Text = stringBuilder.ToString();
		textBlock5.Width = 80.0;
		statusBar1.Items.Add(textBlock5);
		ProgressBar progressBar = new ProgressBar();
		progressBar.Width = 600.0;
		progressBar.Height = 18.0;
		Duration duration = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation = new DoubleAnimation(20.0, duration);
		progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
		ToolTip toolTip = new ToolTip();
		toolTip.Content = "downloading: 20%";
		progressBar.ToolTip = toolTip;
		statusBar1.Items.Add(progressBar);
		m_byFaultStatus = 0;
		textBox_FaultStatus.Text = m_byFaultStatus.ToString();
		GetFaultStringByIndex(stringBuilder, m_byFaultStatus);
		SurspendReadThread();
		int num2 = 0;
		i32OptVal = 0;
		for (num2 = 0; num2 < m_DevList.Count; num2++)
		{
			string strDevSN = m_DevList.ElementAt(num2).m_strDevSN;
			if (m_DevList.ElementAt(num2).DevWorkState == CDevListItemData.WORKER_STATE.WS_ENDEDTASK)
			{
				m_DevList.ElementAt(num2).DevWorkState = CDevListItemData.WORKER_STATE.WS_READY;
			}
			if (m_DevList.ElementAt(num2).DevWorkState != CDevListItemData.WORKER_STATE.WS_READY)
			{
				return 1;
			}
			m_dwOkFailFlag = 0u;
			dtBeginTime = DateTime.Now;
			byte byOptType = 4;
			byte byAccType = 19;
			flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
			if (!flag)
			{
				num = 2;
				break;
			}
			sPreCcfgReg.byRegisterNum = 2;
			for (num2 = 0; num2 < sPreCcfgReg.byRegisterNum; num2++)
			{
				sPreCcfgReg.adwRegAdd[num2] = sPreReg.adwRegAdd[num2];
				sPreCcfgReg.adwRegVal[num2] = 0u;
			}
			num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref sPreCcfgReg);
			if (!flag)
			{
				num = 2;
				break;
			}
			byte b = (byte)sPreCcfgReg.adwRegVal[0];
			b &= 0xB6;
			sPreReg.adwRegVal[0] = (byte)((sPreReg.adwRegVal[0] & 0xFF) | b);
			b = (byte)sPreReg.adwRegVal[1];
			b &= 0x9F;
			sPreReg.adwRegVal[1] = (byte)((sPreReg.adwRegVal[1] & 0xFF) | b);
			num2 = 0;
			sPreReg.byRegisterNum = 52;
			for (num2 = 0; num2 < 3; num2++)
			{
				num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref sPreReg);
				num |= GetCommStatus();
				if (num != 0)
				{
					text = "usb communication fail";
					RecordOnOffError2CSV(text);
					LogCmdErrorInfo(text + " at ExecuteDebugOpt.");
					if (!m_isQuietMode)
					{
						MessageBox.Show(text, "EDO", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
					return num;
				}
				if (m_byCommunicationStatus != 1)
				{
					break;
				}
				LogCmdErrorInfo("get comm status fail after reading breg.");
				if (num2 != 1)
				{
					continue;
				}
				num = ScanI2CAddrDevID();
				if (num != 0)
				{
					LogCmdErrorInfo("scan i2c addr fail.");
					ReEnumDevSN();
					text = "usb communication fail at execute debug opt.";
					RecordOnOffError2CSV(text);
					if (!m_isCmdOpera && !m_isQuietMode)
					{
						MessageBox.Show(text, "EDO Scan", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
					return num;
				}
				if (m_byCommunicationStatus == 1)
				{
					return num;
				}
			}
			ProgressBar progressBar2 = new ProgressBar();
			progressBar2 = (ProgressBar)statusBar1.Items.GetItemAt(6);
			Duration duration2 = new Duration(TimeSpan.FromSeconds(1.0));
			DoubleAnimation animation2 = new DoubleAnimation(70.0, duration2);
			progressBar2.BeginAnimation(RangeBase.ValueProperty, animation2);
			ToolTip toolTip2 = new ToolTip();
			toolTip2.Content = "downloading:70%";
			progressBar2.ToolTip = toolTip2;
			statusBar1.Items.Refresh();
			sPreCcfgReg.byRegisterNum = 5;
			sPreReg.byRegisterNum = 52;
			sPreReg.byRegisterNum += sPreCcfgReg.byRegisterNum;
			byte b2 = 0;
			for (num2 = 0; num2 < sPreCcfgReg.byRegisterNum; num2++)
			{
				sPreCcfgReg.adwRegAdd[num2] = sPreReg.adwRegAdd[52 + num2];
				sPreCcfgReg.adwRegVal[num2] = sPreReg.adwRegVal[52 + num2];
			}
			byOptType = 1;
			byAccType = 2;
			if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
			{
				num = 2;
				break;
			}
			if (!bw.IsBusy)
			{
				num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref sPreCcfgReg);
			}
			else
			{
				DllDebuggerAPI._sTotalStepCnt = 0;
				Thread.Sleep(10);
				num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref sPreCcfgReg);
			}
			if (num != 0)
			{
				break;
			}
			ProgressBar progressBar3 = new ProgressBar();
			progressBar3 = (ProgressBar)statusBar1.Items.GetItemAt(6);
			Duration duration3 = new Duration(TimeSpan.FromSeconds(1.0));
			DoubleAnimation animation3 = new DoubleAnimation(90.0, duration3);
			progressBar3.BeginAnimation(RangeBase.ValueProperty, animation3);
			ToolTip toolTip3 = new ToolTip();
			toolTip3.Content = "downloading:90%";
			progressBar3.ToolTip = toolTip3;
			statusBar1.Items.Refresh();
			byte b3 = 1;
			byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
			if (bySpdCtrlMd != 2)
			{
				sPreCcfgReg.byRegisterNum = 1;
				sPreCcfgReg.adwRegAdd[0] = 4u;
				sPreCcfgReg.adwRegVal[0] = 0u;
				b3 = 0;
				byOptType = 1;
				byAccType = 2;
				flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
				num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref sPreCcfgReg);
				b3 = (byte)sPreCcfgReg.adwRegVal[0];
				sPreCcfgReg.adwRegVal[0] = (uint)(b3 & 0xFE);
				num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref sPreCcfgReg);
				if (num != 0)
				{
					text = "clear override fail..";
					LogCmdErrorInfo(text);
					if (!m_isQuietMode)
					{
						MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
					return 2;
				}
			}
			ProgressBar progressBar4 = new ProgressBar();
			progressBar4 = (ProgressBar)statusBar1.Items.GetItemAt(6);
			Duration duration4 = new Duration(TimeSpan.FromSeconds(1.0));
			DoubleAnimation animation4 = new DoubleAnimation(100.0, duration4);
			progressBar4.BeginAnimation(RangeBase.ValueProperty, animation4);
			ToolTip toolTip4 = new ToolTip();
			toolTip4.Content = "100%, write register completely";
			progressBar4.ToolTip = toolTip4;
			statusBar1.Items.Refresh();
			m_byFaultStatus = 0;
			DllDebuggerAPI._sTotalStepCnt = 1;
			if (!bw.IsBusy)
			{
				timer.Start();
				bw.RunWorkerAsync(i32OptVal);
			}
			Color color = Color.FromRgb(115, 190, 29);
			Brush brush = new SolidColorBrush(color);
			flagEllipse.Background = brush;
			flagEllipse.BorderBrush = brush;
			lblMessage.Content = "Running...";
			if (m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime != 0)
			{
				m_byStartTimeDelay = 1;
			}
			da = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.5)));
			da.Name = "ani";
			da.AutoReverse = true;
			da.RepeatBehavior = RepeatBehavior.Forever;
			myStoryboard = new Storyboard();
			myStoryboard.Children.Add(da);
			Storyboard.SetTargetName(da, flagEllipse.Name);
			Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
			myStoryboard.Begin(flagEllipse, HandoffBehavior.SnapshotAndReplace, isControllable: true);
			daStatusLed = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.5)));
			daStatusLed.Name = "ani";
			daStatusLed.AutoReverse = true;
			daStatusLed.RepeatBehavior = RepeatBehavior.Forever;
			myStaustStoryboard = new Storyboard();
			myStaustStoryboard.Children.Add(daStatusLed);
			Storyboard.SetTargetName(daStatusLed, StatusLed.Name);
			Storyboard.SetTargetProperty(daStatusLed, new PropertyPath(UIElement.OpacityProperty));
			myStaustStoryboard.Begin(ellipse, HandoffBehavior.SnapshotAndReplace, isControllable: true);
		}
		return num;
	}

	private void CancelBackWorker()
	{
		DllDebuggerAPI._sTotalStepCnt = 0;
		bw.CancelAsync();
		timer.Stop();
	}

	private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
	}

	private void bw_DoWork(object sender, DoWorkEventArgs e)
	{
		BackgroundWorker sender2 = sender as BackgroundWorker;
		e.Result = ComputeFibonacci(sender2, e);
	}

	private void btnStartBw_Click(object sender, RoutedEventArgs e)
	{
		m_eBtnOptType = E_BtnOptType.eBtn_OPT_STOP;
		ExecuteStopOperation();
	}

	private bool ExecuteStopOperation()
	{
		byte bySpdCtrl = 0;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = bySpdCtrl;
		int num = ExecuteDebugOperation();
		DllDebuggerAPI._sTotalStepCnt = 0;
		CancelBackWorker();
		timer.Stop();
		InitStautsDisplay();
		bool flag = true;
		bySpdCtrl = m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn;
		if (bySpdCtrl == 1)
		{
			HandleOnoffDisplay(flag);
			btnDebug.IsEnabled = !flag;
		}
		else
		{
			btn_OnOffTest.IsEnabled = !flag;
			btnDebug.IsEnabled = flag;
			btnStartBw.IsEnabled = flag;
		}
		m_bCanceled = true;
		m_bStartFlag = false;
		m_bStopFlag = false;
		return true;
	}

	private void checkBox_EnOnOffTest_Click(object sender, RoutedEventArgs e)
	{
		bool flag = checkBox_EnNewOnOffTest.IsChecked == true;
		InitOnOffConfig(flag);
		if (flag)
		{
			m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn = 1;
		}
		else
		{
			m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn = 0;
		}
	}

	private bool InitOnOffConfig(bool bChecked)
	{
		textBox_StartDuty.IsEnabled = bChecked;
		textBox_StopDuty.IsEnabled = bChecked;
		textBox_StartDurationTime.IsEnabled = bChecked;
		textBox_StopDurationTime.IsEnabled = bChecked;
		textBox_TotalOnOffCnt.IsEnabled = bChecked;
		if (m_bFindDev && bChecked)
		{
			btn_OnOffTest.IsEnabled = true;
			btnDebug.IsEnabled = false;
			btnStartBw.IsEnabled = false;
		}
		else
		{
			btn_OnOffTest.IsEnabled = false;
			btnDebug.IsEnabled = m_bFindDev;
			btnStartBw.IsEnabled = m_bFindDev;
		}
		return true;
	}

	private bool HandleOnoffDisplay(bool bCtrlShow)
	{
		if (checkBox_EnNewOnOffTest.IsChecked == true)
		{
			InitOnOffConfig(bCtrlShow);
		}
		checkBox_EnNewOnOffTest.IsEnabled = bCtrlShow;
		byte byOnOffTestEn = m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn;
		if (byOnOffTestEn == 1)
		{
			btnStartBw.IsEnabled = !bCtrlShow;
			btnDebug.IsEnabled = bCtrlShow;
		}
		return true;
	}

	private void btn_OnOffTest_Click(object sender, RoutedEventArgs e)
	{
		if (checkBox_EnNewOnOffTest.IsChecked == true)
		{
			btn_OnOffTest_Display_Click(sender, e);
		}
	}

	private void btn_OnOffTest_Display_Click(object sender, RoutedEventArgs e)
	{
		byte b = 0;
		b = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl;
		DateTime now = DateTime.Now;
		int num = 0;
		string text = now.ToString("yyyy_MM_dd_HH_mm_ss");
		m_strFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\log\\FT8132_" + text + ".xls";
		bool bCtrlShow = false;
		HandleOnoffDisplay(bCtrlShow);
		byte b2 = 0;
		m_eBtnOptType = E_BtnOptType.eBtn_OPT_ON_OFF_TEST;
		string text2 = textBox_StartDuty.Text;
		b2 = Convert.ToByte(text2, 10);
		m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty = b2;
		text2 = textBox_StopDuty.Text;
		b2 = Convert.ToByte(text2, 10);
		m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty = b2;
		text2 = textBox_StartDurationTime.Text;
		b2 = Convert.ToByte(text2, 10);
		m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration = b2;
		text2 = textBox_StopDurationTime.Text;
		b2 = Convert.ToByte(text2, 10);
		m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration = b2;
		text2 = textBox_TotalOnOffCnt.Text;
		ushort wTotalSetCnt = Convert.ToUInt16(text2, 10);
		m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt = wTotalSetCnt;
		m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt = 0;
		m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt = 0;
		wTotalSetCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt;
		textBox_DoneCnt.Text = wTotalSetCnt.ToString();
		wTotalSetCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt;
		textBox_FailCnt.Text = wTotalSetCnt.ToString();
		byte byStartDuration = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration;
		byte byStopDuration = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration;
		OnTimer.Interval = TimeSpan.FromSeconds((int)byStartDuration);
		OffTimer.Interval = TimeSpan.FromSeconds((int)byStopDuration);
		int num2 = 0;
		int num3 = 0;
		b2 = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = b2;
		bool flag = true;
		num3 = ExecuteDebugOperation();
		if (checkBox_SimulateComm.IsChecked == true)
		{
			LogWriteDataToTxt();
		}
		byte b3 = 0;
		byte b4 = 0;
		OutputDebugString("begin onoff test..\r\n");
		m_bFinishedTest = false;
		m_bCanceled = false;
		m_bStartFlag = true;
		m_bStopFlag = false;
		if (timeThread.ThreadState == System.Threading.ThreadState.Unstarted)
		{
			timeThread.Start();
		}
	}

	private int CheckJiDuiShuValidation()
	{
		int num = 0;
		string text = textBox_JiDuiShu.Text;
		try
		{
			byte b = byte.Parse(text);
			if (b < 1 || b > 20)
			{
				MessageBox.Show("invalid value, please input decimal integer between 1 and 20.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 1 and 20.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_JiDuiShu.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu.ToString();
			}
			else
			{
				textBox_JiDuiShu.Text = (string)FindResource("TextBox_JiDuiShu");
			}
		}
		return num;
	}

	public void textBox_JiDuiShu_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Return)
		{
			return;
		}
		int num = 0;
		if (CheckJiDuiShuValidation() == 0)
		{
			HandleJiDuiShuShow();
			if (!textBox_LockMinSpeed.IsEnabled)
			{
				ShowLockMinSpeedValue();
			}
			DisplayReverseBrakeSpeedText();
		}
	}

	public void textBox_JiDuiShu_LostFocus(object sender, RoutedEventArgs e)
	{
		uint num = 0u;
		int num2 = 0;
		if (CheckJiDuiShuValidation() == 0)
		{
			bool flag = false;
			if (m_byIsLoadDefault == 1)
			{
				flag = false;
			}
			else
			{
				flag = true;
			}
			byte byJiDuiShu = byte.Parse(textBox_JiDuiShu.Text);
			m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu = byJiDuiShu;
			if (!textBox_LockMinSpeed.IsEnabled)
			{
				ShowLockMinSpeedValue();
			}
			DisplayReverseBrakeSpeedText();
		}
	}

	private int HandleJiDuiShuShow()
	{
		uint num = 0u;
		bool flag = false;
		if (m_byIsLoadDefault == 1)
		{
			flag = false;
		}
		else
		{
			flag = true;
		}
		byte byJiDuiShu = byte.Parse(textBox_JiDuiShu.Text);
		m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu = byJiDuiShu;
		return 0;
	}

	public void textBox_DianZu_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_DianZu.Text;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 > 200.0)
			{
				MessageBox.Show("invalid value, please input float between 0.0 and 200.00");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float between 0.0 and 200.00.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_DianZu.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu.ToString();
			}
			else
			{
				textBox_DianZu.Text = (string)FindResource("TextBox_DianZu");
			}
		}
	}

	public void textBox_DianGan_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_DianGan.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.0 || (double)num2 > 1.0)
			{
				MessageBox.Show("invalid value, please input float data between 0.00000 and 1.00000.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.00000 and 1.00000.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_DianGan.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan.ToString();
			}
			else
			{
				textBox_DianGan.Text = (string)FindResource("TextBox_DianGan");
			}
		}
	}

	public void textBox_FanDianDongShi_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_FanDianDongShi.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.0 || (double)num2 > 100.0)
			{
				MessageBox.Show("invalid value, please input float data between 0.0000 and 100.0000.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.0000 and 100.0000.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_FanDianDongShi.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi.ToString();
			}
			else
			{
				textBox_FanDianDongShi.Text = (string)FindResource("TextBox_FanDianDongShi");
			}
		}
	}

	public void textBox_CaiYandDianZu_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_CaiYandDianZu.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.001 || (double)num2 > 2.0)
			{
				MessageBox.Show("invalid value, please input float data between 0.001 and 2.000.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.001 and 2.000.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_CaiYandDianZu.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu.ToString();
			}
			else
			{
				textBox_CaiYandDianZu.Text = (string)FindResource("TextBox_CaiYandDianZu");
			}
		}
	}

	private void textBox_RV_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_RV.Text;
		int num = 0;
		try
		{
			int num2 = int.Parse(text);
			if (num2 < 1 || num2 > 200)
			{
				MessageBox.Show("invalid value, please input integer data between 1 and 200.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input integer data between 1 and 200.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_RV.Text = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV.ToString();
			}
			else
			{
				textBox_RV.Text = (string)FindResource("TextBox_RV");
			}
		}
	}

	private void radioButton_VdcAdj12_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj = 0;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fRV = 12f;
		radioBtn_OuvpThr0.Content = (5.0 / 64.0 * (double)m_sMotorParamCfg.m_sMotorParamHwCfg.fRV).ToString("0.000 V");
		radioBtn_OuvpThr1.Content = (5.0 / 32.0 * (double)m_sMotorParamCfg.m_sMotorParamHwCfg.fRV).ToString("0.000 V");
		ShowOvpThrText();
		ShowUvpThrText();
	}

	private void radioButton_VdcAdj65_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj = 1;
		m_sMotorParamCfg.m_sMotorParamHwCfg.fRV = 6.5f;
		radioBtn_OuvpThr0.Content = (5.0 / 64.0 * (double)m_sMotorParamCfg.m_sMotorParamHwCfg.fRV).ToString("0.000 V");
		radioBtn_OuvpThr1.Content = (5.0 / 32.0 * (double)m_sMotorParamCfg.m_sMotorParamHwCfg.fRV).ToString("0.000 V");
		ShowOvpThrText();
		ShowUvpThrText();
	}

	private void checkBox_EnNewOnOffTest_Click(object sender, RoutedEventArgs e)
	{
		bool flag = checkBox_EnNewOnOffTest.IsChecked == true;
		if (flag)
		{
			m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn = 1;
		}
		else
		{
			m_sMotorParamCfg.m_sOnOffTestCfg.byOnOffTestEn = 0;
		}
		InitOnOffConfig(flag);
	}

	private void Downloadtimer_Tick(object sender, EventArgs e)
	{
		InitDownLoadStautsDisplay(blVal: true);
		if (m_wDownloadFlag == 1)
		{
			OutputDebugString("1");
			Color color = Color.FromRgb(115, 190, 29);
			BrushConverter brushConverter = new BrushConverter();
			Brush brush = (Brush)brushConverter.ConvertFromString("#FF73be1d");
			ellipse_Verify.Background = brush;
			ellipse_Verify.BorderBrush = brush;
			label_VerifyStatus.Content = "PASS";
			btn_Verify.IsEnabled = true;
		}
		else if (m_wDownloadFlag == 2)
		{
			OutputDebugString("2");
			ellipse_Verify.Background = Brushes.Red;
			ellipse_Verify.BorderBrush = Brushes.Red;
			label_VerifyStatus.Content = "FAIL";
			btn_Verify.IsEnabled = true;
		}
		ellipse_Verify.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
	}

	private int DeviceChangeCheck()
	{
		int num = 255;
		byte b = 3;
		StringBuilder stringBuilder = new StringBuilder();
		while (b-- > 0)
		{
			if (!DllDebuggerAPI.DLL_GPL32DownloaderEnumerateDevices())
			{
				continue;
			}
			int num2 = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceCount();
			if (num2 <= 0)
			{
				num = 1;
				i32Flag = 2;
				LogCmdErrorInfo("Device count is zero. Start enumerating device...");
				EnumDevice();
				Thread.Sleep(1000);
			}
			else
			{
				for (int i = 0; i < num2; i++)
				{
					num = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceSerial(i, stringBuilder);
					string text = arrDeviceSN[i];
					string text2 = stringBuilder.ToString();
					if (arrDeviceSN.Count <= i || string.Compare(text, text2, ignoreCase: true) != 0)
					{
						i32Flag = 1;
						LogCmdErrorInfo("old SN: " + text + ", new SN: " + text2);
						LogCmdErrorInfo("Device SN unmatch. Start enumerating device...");
						EnumDevice();
						num = ScanI2CAddrDevID();
						if (num != 0)
						{
							LogCmdErrorInfo("scan i2c addr fail at device change check. ret: " + num);
						}
						break;
					}
					num = 0;
					LogCmdErrorInfo("device not change.");
				}
			}
			if (num != 0)
			{
				continue;
			}
			break;
		}
		return num;
	}

	private void btn_WriteEfuse_Click(object sender, RoutedEventArgs e)
	{
		if (!btn_ReadID.IsEnabled)
		{
			string text = "Please connect device...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return;
		}
		LogCmdErrorInfo("check device change before prog.");
		int num = DeviceChangeCheck();
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byAccType = 2;
		byte byOptType = 49;
		bool flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = 0u;
		int num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(7, ref psPreReg);
		string text2 = textBox_CodeFile.Text;
		if (text2.Length == 0)
		{
			string text = "Code file is empty! Please generate or select a file first.";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return;
		}
		LogCmdErrorInfo("Write begin.");
		LogCmdErrorInfo("dwnflag1: " + m_wDownloadFlag);
		ushort num3 = 128;
		ushort num4 = 748;
		int num5 = 0;
		if (m_byWriteBufSrcMode == 1)
		{
			for (num5 = 0; num5 < num4; num5++)
			{
				bool flag2 = true;
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num5 = 0; num5 < num4; num5++)
			{
				bool flag2 = true;
			}
		}
		num2 = 0;
		SurspendReadThread();
		if (!m_isCmdOpera && StopMotorFlow() != 0)
		{
			RestoreReadThread();
			return;
		}
		ClearWriteEfuseStatus();
		m_wDownloadFlag = 0;
		statusBar1.Items.Clear();
		TextBlock textBlock = new TextBlock();
		textBlock.Text = "Device:";
		textBlock.Width = 50.0;
		textBlock.HorizontalAlignment = HorizontalAlignment.Left;
		statusBar1.Items.Add(textBlock);
		TextBlock textBlock2 = new TextBlock();
		textBlock2.Text = "downloading...";
		textBlock2.Width = 80.0;
		textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
		textBlock2.VerticalAlignment = VerticalAlignment.Stretch;
		statusBar1.Items.Add(textBlock2);
		TextBlock textBlock3 = new TextBlock();
		textBlock3.Text = "Elaps Time:";
		textBlock3.Width = 80.0;
		statusBar1.Items.Add(textBlock3);
		TextBlock textBlock4 = new TextBlock();
		textBlock4.Text = "00:00:00";
		textBlock4.Width = 80.0;
		statusBar1.Items.Add(textBlock4);
		Ellipse ellipse = new Ellipse();
		ellipse.HorizontalAlignment = HorizontalAlignment.Right;
		ellipse.VerticalAlignment = VerticalAlignment.Center;
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = flagEllipse.Background;
		statusBar1.Items.Add(ellipse);
		TextBlock textBlock5 = new TextBlock();
		textBlock5.Text = lblMessage.Content.ToString();
		textBlock5.Width = 80.0;
		statusBar1.Items.Add(textBlock5);
		ProgressBar progressBar = new ProgressBar();
		progressBar.Width = 600.0;
		progressBar.Height = 18.0;
		Duration duration = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation = new DoubleAnimation(20.0, duration);
		progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
		ToolTip toolTip = new ToolTip();
		toolTip.Content = "write efs: 20%";
		progressBar.ToolTip = toolTip;
		statusBar1.Items.Add(progressBar);
		dtBeginTime = DateTime.Now;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		sPreReg.byRegisterNum = 34;
		for (num7 = 0; num7 < sPreReg.byRegisterNum; num7++)
		{
			psPreReg.adwRegAdd[num7] = 0u;
			psPreReg.adwRegVal[num7] = 0u;
		}
		ProgressBar progressBar2 = new ProgressBar();
		progressBar2 = (ProgressBar)statusBar1.Items.GetItemAt(6);
		Duration duration2 = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation2 = new DoubleAnimation(60.0, duration2);
		progressBar2.BeginAnimation(RangeBase.ValueProperty, animation2);
		ToolTip toolTip2 = new ToolTip();
		toolTip2.Content = "writing efs:60%";
		progressBar2.ToolTip = toolTip2;
		statusBar1.Items.Refresh();
		LogCmdErrorInfo("comm status before prog: " + m_byCommunicationStatus);
		byAccType = 2;
		byOptType = 1;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 244u;
		psPreReg.adwRegVal[0] = 0u;
		num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		byte b = (byte)psPreReg.adwRegVal[0];
		byOptType = 5;
		byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			num2 = 2;
			return;
		}
		ushort num9 = 110;
		ushort num10 = 0;
		ushort num11 = 896;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = num9;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg) != 0)
		{
			string text = "Read bflash Fail...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		if (m_byWriteBufSrcMode == 1)
		{
			for (num7 = 0; num7 < num9; num7++)
			{
				m_byWEfuseBitArr[num7] = (byte)psPreReg.adwRegVal[num7 + 1];
				m_byWEfuseBitArr[num7 + num11] = (byte)psPreReg.adwRegVal[num7 + 1];
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num7 = 0; num7 < num9; num7++)
			{
				m_byREfuseBitArr[num7] = (byte)psPreReg.adwRegVal[num7 + 1];
				m_byREfuseBitArr[num7 + num11] = (byte)psPreReg.adwRegVal[num7 + 1];
			}
		}
		byAccType = 2;
		byOptType = 1;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 244u;
		psPreReg.adwRegVal[0] = 0u;
		num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		b = (byte)psPreReg.adwRegVal[0];
		if ((b & 0x20) == 32)
		{
			byAccType = 2;
			byOptType = 1;
			flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
			psPreReg.byRegisterNum = 1;
			psPreReg.adwRegAdd[0] = 244u;
			psPreReg.adwRegVal[0] = (byte)(0xDF & b);
			num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg);
		}
		byOptType = 40;
		byAccType = 5;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = num9;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg) != 0)
		{
			string text = "Chip erase flash Fail...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		byAccType = 2;
		byOptType = 1;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 244u;
		psPreReg.adwRegVal[0] = 0u;
		num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		b = (byte)psPreReg.adwRegVal[0];
		if ((b & 0x20) == 32)
		{
			byAccType = 2;
			byOptType = 1;
			flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
			psPreReg.byRegisterNum = 1;
			psPreReg.adwRegAdd[0] = 244u;
			psPreReg.adwRegVal[0] = (byte)(0xDF & b);
			num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg);
		}
		byOptType = 133;
		byAccType = 5;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.adwRegAdd[0] = num11;
		psPreReg.adwRegVal[0] = num9;
		if (m_byWriteBufSrcMode == 1)
		{
			for (num7 = 0; num7 < num9; num7++)
			{
				psPreReg.adwRegVal[num7 + 1] = m_byWEfuseBitArr[num7];
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num7 = 0; num7 < num9; num7++)
			{
				psPreReg.adwRegVal[num7 + 1] = m_byREfuseBitArr[num7];
			}
		}
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg) != 0)
		{
			string text = "Write bkflash Fail...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		byAccType = 2;
		byOptType = 1;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 244u;
		psPreReg.adwRegVal[0] = 0u;
		num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		b = (byte)psPreReg.adwRegVal[0];
		byOptType = 133;
		byAccType = 5;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.adwRegAdd[0] = num3;
		ushort num12 = 374;
		psPreReg.adwRegVal[0] = num12;
		if (m_byWriteBufSrcMode == 1)
		{
			for (num7 = 0; num7 < num12; num7++)
			{
				psPreReg.adwRegVal[num7 + 1] = m_byWEfuseBitArr[num7 + num3];
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num7 = 0; num7 < num12; num7++)
			{
				psPreReg.adwRegVal[num7 + 1] = m_byREfuseBitArr[num7 + num3];
			}
		}
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg) != 0)
		{
			string text = "Write cflash1 Fail...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		psPreReg.adwRegAdd[0] = (uint)(num3 + num12);
		psPreReg.adwRegVal[0] = num12;
		if (m_byWriteBufSrcMode == 1)
		{
			for (num7 = 0; num7 < num12; num7++)
			{
				psPreReg.adwRegVal[num7 + 1] = m_byWEfuseBitArr[num7 + num3 + num12];
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num7 = 0; num7 < num12; num7++)
			{
				psPreReg.adwRegVal[num7 + 1] = m_byREfuseBitArr[num7 + num3 + num12];
			}
		}
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg) != 0)
		{
			string text = "Write cflash1 Fail...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		Thread.Sleep(10);
		if (checkBox_SimulateComm.IsChecked == true)
		{
			sPreRReg.byRegisterNum = 58;
			if (m_byWriteBufSrcMode == 1)
			{
				Bit2Byte(sPreRReg.byRegisterNum, m_byWEfuseBitArr);
			}
			else if (m_byWriteBufSrcMode == 2)
			{
				Bit2Byte(sPreRReg.byRegisterNum, m_byREfuseBitArr);
			}
			LogWriteEfuseToTxt();
		}
		dtEndTime = DateTime.Now;
		string text3 = dtEndTime.Subtract(dtBeginTime).ToString();
		string text4 = text3.Substring(0, 11);
		TextBlock textBlock6 = new TextBlock();
		textBlock6.Text = text4;
		textBlock6 = (TextBlock)statusBar1.Items.GetItemAt(3);
		textBlock6.Text = text4;
		statusBar1.Items.Refresh();
		ProgressBar progressBar3 = new ProgressBar();
		progressBar3 = (ProgressBar)statusBar1.Items.GetItemAt(6);
		Duration duration3 = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation3 = new DoubleAnimation(80.0, duration3);
		progressBar3.BeginAnimation(RangeBase.ValueProperty, animation3);
		ToolTip toolTip3 = new ToolTip();
		toolTip3.Content = "downloading:80%";
		progressBar3.ToolTip = toolTip3;
		statusBar1.Items.Refresh();
		if (HandleVerifyFlow() != 0)
		{
			string text = "Read efuse fail";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		if (CompreCrcVal(m_wTgtSrcVal) != 0)
		{
			m_dwFailCnt++;
			textBox_FailedCnt.Text = m_dwFailCnt.ToString();
			m_wDownloadFlag = 2;
		}
		else
		{
			m_dwPassCnt++;
			m_wDownloadFlag = 1;
			textBox_PassedCnt.Text = m_dwPassCnt.ToString();
			ProgressBar progressBar4 = new ProgressBar();
			progressBar4 = (ProgressBar)statusBar1.Items.GetItemAt(6);
			Duration duration4 = new Duration(TimeSpan.FromSeconds(1.0));
			DoubleAnimation animation4 = new DoubleAnimation(100.0, duration4);
			progressBar4.BeginAnimation(RangeBase.ValueProperty, animation4);
			ToolTip toolTip4 = new ToolTip();
			toolTip4.Content = "downloading:100%";
			progressBar4.ToolTip = toolTip4;
			statusBar1.Items.Refresh();
		}
		m_dwTotalCnt = m_dwPassCnt + m_dwFailCnt;
		textBox_TotalCnt.Text = m_dwTotalCnt.ToString();
		byOptType = 1;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = 0u;
		LogCmdErrorInfo("dwnflag2: " + m_wDownloadFlag);
		LogCmdErrorInfo("Write end.");
	}

	private int Bit2Byte(ushort wByteNum, byte[] byDataBuf)
	{
		byte b = 8;
		byte b2 = 32;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (num = 0; num < b; num++)
		{
			sPreRReg.adwRegVal[num] = 0u;
			sPreRReg.adwRegAdd[num] = 0u;
			num2 = num * 8 + 6 * (num / 2);
			for (num3 = 0; num3 < 8; num3++)
			{
				sPreRReg.adwRegVal[num] |= (byte)(byDataBuf[num2 + num3] << num3);
			}
			sPreRReg.adwRegAdd[num] = (byte)(128 + num);
		}
		num = 0;
		num2 = 0;
		num3 = 0;
		for (num = 0; num < b2; num++)
		{
			sPreRReg.adwRegVal[num + b] = 0u;
			sPreRReg.adwRegAdd[num + b] = 0u;
			num2 = num * 8 + 6 * (num / 2);
			for (num3 = 0; num3 < 8; num3++)
			{
				sPreRReg.adwRegVal[num + b] |= (byte)(byDataBuf[128 + num2 + num3] << num3);
			}
			sPreRReg.adwRegAdd[num + b] = (byte)(136 + num);
		}
		return 0;
	}

	private int Byte2Bit(ushort wBitNum, byte[] byDataBuf)
	{
		for (int i = 0; i < wBitNum; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				m_byAdminREfuseBitArr[i * 8 + j] = (byte)((byDataBuf[i] >> j) & 1);
			}
		}
		return 0;
	}

	private void btn_Verify_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		if (!btn_ReadID.IsEnabled)
		{
			string text = "Please connect device...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return;
		}
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byAccType = 2;
		byte byOptType = 49;
		bool flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = 0u;
		int num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(7, ref psPreReg);
		string text2 = textBox_CodeFile.Text;
		if (text2.Length == 0)
		{
			string text = "Code file is empty. Please generate or open a file first.";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return;
		}
		LogCmdErrorInfo("Verify begin.");
		LogCmdErrorInfo("dwnflag1: " + m_wDownloadFlag);
		num2 = 0;
		OutputDebugString("38");
		SurspendReadThread();
		if (StopMotorFlow() != 0)
		{
			return;
		}
		ClearWriteEfuseStatus();
		m_wDownloadFlag = 0;
		Thread.Sleep(50);
		statusBar1.Items.Clear();
		TextBlock textBlock = new TextBlock();
		textBlock.Text = "Device:";
		textBlock.Width = 50.0;
		textBlock.HorizontalAlignment = HorizontalAlignment.Left;
		statusBar1.Items.Add(textBlock);
		TextBlock textBlock2 = new TextBlock();
		textBlock2.Text = "reading efuse...";
		textBlock2.Width = 80.0;
		textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
		textBlock2.VerticalAlignment = VerticalAlignment.Stretch;
		statusBar1.Items.Add(textBlock2);
		TextBlock textBlock3 = new TextBlock();
		textBlock3.Text = "Elaps Time:";
		textBlock3.Width = 80.0;
		statusBar1.Items.Add(textBlock3);
		TextBlock textBlock4 = new TextBlock();
		textBlock4.Text = "00:00:00";
		textBlock4.Width = 80.0;
		statusBar1.Items.Add(textBlock4);
		Ellipse ellipse = new Ellipse();
		ellipse.HorizontalAlignment = HorizontalAlignment.Right;
		ellipse.VerticalAlignment = VerticalAlignment.Center;
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = flagEllipse.Background;
		statusBar1.Items.Add(ellipse);
		TextBlock textBlock5 = new TextBlock();
		textBlock5.Text = lblMessage.Content.ToString();
		textBlock5.Width = 80.0;
		statusBar1.Items.Add(textBlock5);
		ProgressBar progressBar = new ProgressBar();
		progressBar.Width = 600.0;
		progressBar.Height = 18.0;
		Duration duration = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation = new DoubleAnimation(20.0, duration);
		progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
		ToolTip toolTip = new ToolTip();
		toolTip.Content = "reading: 20%";
		progressBar.ToolTip = toolTip;
		statusBar1.Items.Add(progressBar);
		dtBeginTime = DateTime.Now;
		ProgressBar progressBar2 = new ProgressBar();
		progressBar2 = (ProgressBar)statusBar1.Items.GetItemAt(6);
		Duration duration2 = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation2 = new DoubleAnimation(40.0, duration2);
		progressBar2.BeginAnimation(RangeBase.ValueProperty, animation2);
		ToolTip toolTip2 = new ToolTip();
		toolTip2.Content = "reading efs:60%";
		progressBar2.ToolTip = toolTip2;
		statusBar1.Items.Refresh();
		sPreReg.byRegisterNum = 50;
		for (num = 0; num < sPreReg.byRegisterNum; num++)
		{
			psPreReg.adwRegAdd[num] = 0u;
			psPreReg.adwRegVal[num] = 0u;
		}
		byAccType = 2;
		byOptType = 1;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 244u;
		psPreReg.adwRegVal[0] = 0u;
		num2 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		byte b = (byte)psPreReg.adwRegVal[0];
		byOptType = 5;
		byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			num2 = 2;
			return;
		}
		ushort num3 = 110;
		ushort num4 = 0;
		ushort num5 = 896;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = num3;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg) != 0)
		{
			string text = "Read bflash Fail...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		if (m_byWriteBufSrcMode == 1)
		{
			for (num = 0; num < num3; num++)
			{
				m_byWEfuseBitArr[num] = (byte)psPreReg.adwRegVal[num + 1];
				m_byWEfuseBitArr[num + num5] = (byte)psPreReg.adwRegVal[num + 1];
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num = 0; num < num3; num++)
			{
				m_byREfuseBitArr[num] = (byte)psPreReg.adwRegVal[num + 1];
				m_byREfuseBitArr[num + num5] = (byte)psPreReg.adwRegVal[num + 1];
			}
		}
		if (HandleVerifyFlow() != 0)
		{
			string text = "Read efuse fail";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		if (CompreCrcVal(m_wTgtSrcVal) != 0)
		{
			m_wDownloadFlag = 2;
			LogCmdErrorInfo("dwnflag1-2: " + m_wDownloadFlag);
			LogCmdErrorInfo("Verify end.");
			return;
		}
		m_wDownloadFlag = 1;
		Thread.Sleep(10);
		dtEndTime = DateTime.Now;
		string text3 = dtEndTime.Subtract(dtBeginTime).ToString();
		string text4 = text3.Substring(0, 11);
		TextBlock textBlock6 = new TextBlock();
		textBlock6.Text = text4;
		textBlock6 = (TextBlock)statusBar1.Items.GetItemAt(3);
		textBlock6.Text = text4;
		statusBar1.Items.Refresh();
		ProgressBar progressBar3 = new ProgressBar();
		progressBar3 = (ProgressBar)statusBar1.Items.GetItemAt(6);
		Duration duration3 = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation3 = new DoubleAnimation(100.0, duration3);
		progressBar3.BeginAnimation(RangeBase.ValueProperty, animation3);
		ToolTip toolTip3 = new ToolTip();
		toolTip3.Content = "100%, read efuse completely";
		progressBar3.ToolTip = toolTip3;
		statusBar1.Items.Refresh();
		Thread.Sleep(100);
		LogCmdErrorInfo("dwnflag2: " + m_wDownloadFlag);
		LogCmdErrorInfo("Verify end.");
	}

	public int HandleVerifyFlow()
	{
		int num = 0;
		m_wTgtSrcVal = 0;
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384],
			byRegisterNum = 68
		};
		for (int i = 0; i < psPreReg.byRegisterNum; i++)
		{
			psPreReg.adwRegAdd[i] = 0u;
			psPreReg.adwRegVal[i] = 0u;
		}
		for (int i = 0; i < 1024; i++)
		{
			m_byAdminREfuseBitArr[i] = 0;
		}
		ushort num2 = 0;
		ushort u16shift = ushort.MaxValue;
		ushort num3 = 128;
		ushort num4 = 748;
		ushort num5 = 110;
		ushort num6 = 0;
		ushort num7 = 896;
		int num8 = 0;
		byte byOptType = 5;
		byte byAccType = 5;
		bool flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = num5;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		if (num != 0)
		{
			MessageBox.Show("read abflash fail...");
			LogCmdErrorInfo("read abflash fail...");
			return num;
		}
		for (num8 = 0; num8 < num5; num8++)
		{
			m_byAdminREfuseBitArr[num8] = (byte)psPreReg.adwRegVal[num8 + 1];
		}
		psPreReg.adwRegAdd[0] = num7;
		psPreReg.adwRegVal[0] = num5;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		if (num != 0)
		{
			MessageBox.Show("read bkflash Fail...");
			LogCmdErrorInfo("read bkflash Fail...");
			return num;
		}
		if (m_byWriteBufSrcMode == 1)
		{
			for (num8 = 0; num8 < num5; num8++)
			{
				if (m_byWEfuseBitArr[num6 + num8] != psPreReg.adwRegVal[1 + num8])
				{
					string text = "rbkgloc:" + num8 + "gsrc:" + m_byWEfuseBitArr[num6 + num8] + "gtgt:" + psPreReg.adwRegVal[1 + num8];
				}
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num8 = 0; num8 < num5; num8++)
			{
				if (m_byREfuseBitArr[num6 + num8] != psPreReg.adwRegVal[1 + num8])
				{
					string text = "rbkoloc:" + num8 + "osrc:" + m_byREfuseBitArr[num6 + num8] + "otgt:" + psPreReg.adwRegVal[1 + num8];
				}
			}
		}
		for (num8 = 0; num8 < num5; num8++)
		{
			m_byAdminREfuseBitArr[num7 + num8] = (byte)psPreReg.adwRegVal[num8 + 1];
		}
		byOptType = 6;
		byAccType = 5;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.adwRegAdd[0] = num3;
		ushort num9 = 374;
		psPreReg.adwRegVal[0] = num9;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		if (num != 0)
		{
			MessageBox.Show("read flash fail...");
			LogCmdErrorInfo("read flash fail...");
			return num;
		}
		for (num8 = 0; num8 < num9; num8++)
		{
			m_byAdminREfuseBitArr[num3 + num8] = (byte)psPreReg.adwRegVal[num8 + 1];
		}
		if (m_byWriteBufSrcMode == 1)
		{
			for (num8 = 0; num8 < num9; num8++)
			{
				if (m_byWEfuseBitArr[num3 + num8] != psPreReg.adwRegVal[1 + num8])
				{
					string text = "rcgloc:" + num8 + "gsrc:" + m_byWEfuseBitArr[num3 + num8] + "gtgt:" + psPreReg.adwRegVal[1 + num8];
				}
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num8 = 0; num8 < num9; num8++)
			{
				if (m_byREfuseBitArr[num3 + num8] != psPreReg.adwRegVal[1 + num8])
				{
					string text = "roloc:" + num8 + "osrc:" + m_byREfuseBitArr[num3 + num8] + "otgt:" + psPreReg.adwRegVal[1 + num8];
				}
			}
		}
		psPreReg.adwRegAdd[0] = (uint)(num3 + num9);
		ushort num10 = (ushort)(num4 - num9);
		psPreReg.adwRegVal[0] = num10;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg);
		if (num != 0)
		{
			MessageBox.Show("read flash Fail...");
			LogCmdErrorInfo("read flash Fail...");
			return num;
		}
		for (num8 = 0; num8 < num10; num8++)
		{
			m_byAdminREfuseBitArr[num3 + num9 + num8] = (byte)psPreReg.adwRegVal[num8 + 1];
		}
		if (m_byWriteBufSrcMode == 1)
		{
			for (num8 = 0; num8 < num10; num8++)
			{
				if (m_byWEfuseBitArr[num3 + num9 + num8] != psPreReg.adwRegVal[1 + num8])
				{
					string text = "rcgloc:" + num8 + "gsrc:" + m_byWEfuseBitArr[num3 + num8] + "gtgt:" + psPreReg.adwRegVal[1 + num8];
				}
			}
		}
		else if (m_byWriteBufSrcMode == 2)
		{
			for (num8 = 0; num8 < num10; num8++)
			{
				if (m_byREfuseBitArr[num3 + num9 + num8] != psPreReg.adwRegVal[1 + num8])
				{
					string text = "roloc:" + num8 + "osrc:" + m_byREfuseBitArr[num3 + num8] + "otgt:" + psPreReg.adwRegVal[1 + num8];
				}
			}
		}
		num2 = crc16(m_byAdminREfuseBitArr, num3, num4, u16shift);
		m_wTgtSrcVal = num2;
		return num;
	}

	private void btn_Erase_Click(object sender, RoutedEventArgs e)
	{
		m_wDownloadFlag = 0;
		if (!btn_ReadID.IsEnabled)
		{
			string text = "Please connect device.";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return;
		}
		CancelBackWorker();
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byAccType = 2;
		byte byOptType = 49;
		bool flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = 0u;
		int num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(7, ref psPreReg);
		LogCmdErrorInfo("Erase begin.");
		LogCmdErrorInfo("dwnflag1: " + m_wDownloadFlag);
		ClearWriteEfuseStatus();
		dtBeginTime = DateTime.Now;
		int num2 = ChipErase1();
		InitDownLoadStautsDisplay(blVal: true);
		if (num2 == 0)
		{
			m_wDownloadFlag = 1;
			BrushConverter brushConverter = new BrushConverter();
			Brush brush = (Brush)brushConverter.ConvertFromString("#FF73be1d");
			ellipse_Verify.Background = brush;
			ellipse_Verify.BorderBrush = brush;
			StatusBarInfo statusBarInfo = new StatusBarInfo();
			statusBarInfo.DeviceStatus = "Erase";
			statusBarInfo.Time = DateTime.Now.Subtract(dtBeginTime).ToString();
			statusBarInfo.StatusLight = Brushes.Green;
			statusBarInfo.MotorStatus = "Success";
			statusBarInfo.Progress = 100.0;
			StatusBarInfo info = statusBarInfo;
			UpdateStatusBar(info);
		}
		else
		{
			m_wDownloadFlag = 2;
			ellipse_Verify.Background = Brushes.Red;
			ellipse_Verify.BorderBrush = Brushes.Red;
			StatusBarInfo statusBarInfo2 = new StatusBarInfo();
			statusBarInfo2.DeviceStatus = "Erase";
			statusBarInfo2.Time = DateTime.Now.Subtract(dtBeginTime).ToString();
			statusBarInfo2.StatusLight = Brushes.Red;
			statusBarInfo2.MotorStatus = "Fail";
			statusBarInfo2.Progress = 50.0;
			StatusBarInfo info = statusBarInfo2;
			UpdateStatusBar(info);
		}
		LogCmdErrorInfo("dwnflag2: " + m_wDownloadFlag);
		LogCmdErrorInfo("Erase end.");
	}

	private int ChipErase1()
	{
		int num = 0;
		bool flag = false;
		byte b = 0;
		byte b2 = 0;
		ushort num2 = 748;
		DllDebuggerAPI.S_PreReg psPreReg = default(DllDebuggerAPI.S_PreReg);
		SurspendReadThread();
		if (StopMotorFlow() != 0)
		{
			return 1;
		}
		psPreReg.adwRegAdd = new uint[384];
		psPreReg.adwRegVal = new uint[384];
		b = 40;
		b2 = 5;
		flag = DllDebuggerAPI.DLL_GPLB32SetOptType(b, b2);
		psPreReg.adwRegAdd[0] = 128u;
		psPreReg.adwRegVal[0] = num2;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg) != 0)
		{
			MessageBox.Show("chip erase flash Fail...");
			LogCmdErrorInfo("chip erase flash fail...");
			return 2;
		}
		return 0;
	}

	private void UpdateStatusBar(StatusBarInfo info)
	{
		statusBar1.Items.Clear();
		TextBlock textBlock = new TextBlock();
		textBlock.Text = "Device: " + info.DeviceStatus;
		TextBlock newItem = textBlock;
		statusBar1.Items.Add(newItem);
		TextBlock textBlock2 = new TextBlock();
		textBlock2.Text = "Elaps Time: " + info.Time;
		textBlock2.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
		TextBlock newItem2 = textBlock2;
		statusBar1.Items.Add(newItem2);
		Ellipse ellipse = new Ellipse();
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = info.StatusLight;
		ellipse.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
		Ellipse newItem3 = ellipse;
		statusBar1.Items.Add(newItem3);
		TextBlock textBlock3 = new TextBlock();
		textBlock3.Text = info.MotorStatus;
		TextBlock newItem4 = textBlock3;
		statusBar1.Items.Add(newItem4);
		DoubleAnimation animation = new DoubleAnimation(duration: new Duration(TimeSpan.FromSeconds(1.0)), toValue: info.Progress);
		ToolTip toolTip = new ToolTip();
		toolTip.Content = info.DeviceStatus + ": " + info.Progress + "%";
		ToolTip toolTip2 = toolTip;
		ProgressBar progressBar = new ProgressBar();
		progressBar.Width = 600.0;
		progressBar.Height = 18.0;
		progressBar.ToolTip = toolTip2;
		progressBar.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
		ProgressBar progressBar2 = progressBar;
		progressBar2.BeginAnimation(RangeBase.ValueProperty, animation);
		statusBar1.Items.Add(progressBar2);
		statusBar1.Items.Refresh();
	}

	public int LogEfuse2Bin(string strUserFilePath)
	{
		int num = 0;
		ushort num2 = 0;
		num2 = m_wTgtSrcVal;
		num = WriteEfuse2Bin(bWriteFlag: true, m_byAdminREfuseBitArr, strUserFilePath);
		LogReadEfueToTxt();
		return num;
	}

	private int ClearWriteEfuseStatus()
	{
		ellipse_Verify.IsEnabled = false;
		label_VerifyStatus.Content = "";
		ushort num = 0;
		string content = $"0x{num:X4}";
		label_TargetCrc.Content = content;
		return 0;
	}

	private int CompreCrcVal(ushort wCrcVal)
	{
		int num = 0;
		string value = label_SrcCrc.Content.ToString();
		ushort num2 = Convert.ToUInt16(value, 16);
		if (num2 == wCrcVal)
		{
			num = 0;
		}
		else
		{
			num = 1;
			LogCmdErrorInfo("srccrc: " + num2.ToString("X") + ", tgtcrc: " + wCrcVal.ToString("X"));
		}
		string content = $"0x{wCrcVal:X4}";
		label_TargetCrc.Content = content;
		return num;
	}

	private void btn_Open_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\UserConfigFile";
		openFileDialog.Filter = "Bin files(*.Bin,*.bin)|*.Bin;*bin|All files (*.*)|*.*";
		openFileDialog.FilterIndex = 1;
		openFileDialog.RestoreDirectory = true;
		bool? flag = true;
		if (openFileDialog.ShowDialog() == flag)
		{
			string fileName = openFileDialog.FileName;
			DoOpenOperation(fileName);
		}
	}

	private int DoOpenOperation(string strTmp)
	{
		ushort num = 0;
		int num2 = 0;
		num2 = WriteEfuse2Bin(bWriteFlag: false, m_byREfuseBitArr, strTmp);
		switch (num2)
		{
		case 0:
		{
			textBox_CodeFile.Text = strTmp;
			textBox_CodeFile.ToolTip = strTmp;
			m_strWriteBinFile = strTmp;
			byte b = 7;
			if (b == 7)
			{
				byte b2 = 128;
				ushort length = 748;
				ushort u16shift = ushort.MaxValue;
				num = crc16(m_byREfuseBitArr, b2, length, u16shift);
				byte b3 = b2;
				byte b4 = (byte)((m_byREfuseBitArr[b3 + 3] << 3) + (m_byREfuseBitArr[b3 + 2] << 2) + (m_byREfuseBitArr[b3 + 1] << 1));
				string content = $"0x{b4 >> 1:X2}";
				label_Bin_I2CAddr.Content = content;
			}
			string content2 = $"0x{num:X4}";
			label_SrcCrc.Content = content2;
			m_byWriteBufSrcMode = 2;
			Downloadtimer.Start();
			break;
		}
		case 2:
		{
			Downloadtimer.Stop();
			string text = "Invalid file length.";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "FILE IO ERROR", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			break;
		}
		}
		return num2;
	}

	private void textBox_FailedCnt_KeyDown(object sender, KeyEventArgs e)
	{
	}

	private void textBox_PassedCnt_KeyDown(object sender, KeyEventArgs e)
	{
	}

	private void checkBox_EnableWritingMode_Click(object sender, RoutedEventArgs e)
	{
		bool? isChecked = checkBox_EnableWritingMode.IsChecked;
		if (isChecked.HasValue)
		{
			bool value = isChecked.Value;
			InitDownloadHShow(value);
		}
	}

	private int InitDownloadHShow(bool bChecked)
	{
		btn_WriteEfuse.IsEnabled = bChecked;
		btn_Erase.IsEnabled = bChecked;
		btn_Verify.IsEnabled = bChecked;
		textBox_TotalCnt.IsEnabled = bChecked;
		textBox_PassedCnt.IsEnabled = bChecked;
		textBox_FailedCnt.IsEnabled = bChecked;
		return 0;
	}

	private ushort crc16(byte[] buf, ushort wStartAddr, ushort length, ushort u16shift)
	{
		ushort num = 0;
		int num2 = wStartAddr;
		ushort num3 = u16shift;
		for (int i = 0; i < length * 8; i++)
		{
			if (i % 8 == 0)
			{
				num = (ushort)(buf[num2++] << 8);
			}
			ushort num4 = (ushort)(num3 ^ num);
			num3 <<= 1;
			num <<= 1;
			if ((ushort)(num4 & 0x8000) != 0)
			{
				num3 ^= 0x1021;
			}
		}
		return num3;
	}

	private int WriteEfuse2Bin(bool bWriteFlag, byte[] pbyDataBuf, string strFilePath)
	{
		int num = 0;
		string text = "";
		if (bWriteFlag)
		{
			try
			{
				FileStream fileStream = new FileStream(strFilePath, FileMode.OpenOrCreate);
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				if (pbyDataBuf == null)
				{
					MessageBox.Show("write buffer invalid.");
					return 1;
				}
				binaryWriter.Write(pbyDataBuf);
				binaryWriter.Close();
				fileStream.Close();
				num = 0;
			}
			catch (Exception ex)
			{
				text = ex.Message;
				LogCmdErrorInfo(text);
				if (!m_isQuietMode)
				{
					MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
				num = 1;
			}
		}
		else
		{
			try
			{
				string text2 = File.GetAttributes(strFilePath).ToString();
				if (text2.IndexOf("ReadOnly") != -1)
				{
					File.SetAttributes(strFilePath, FileAttributes.Normal);
				}
				FileStream fileStream2 = new FileStream(strFilePath, FileMode.Open);
				BinaryReader binaryReader = new BinaryReader(fileStream2);
				if (fileStream2.Length == 1024)
				{
					binaryReader.Read(pbyDataBuf, 0, (int)fileStream2.Length);
					num = 0;
				}
				else
				{
					num = 2;
				}
				binaryReader.Close();
				fileStream2.Close();
			}
			catch (Exception ex)
			{
				text = ex.Message;
				LogCmdErrorInfo(text);
				if (!m_isQuietMode)
				{
					MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
				num = 1;
			}
		}
		return num;
	}

	private int CreateDebugOrDwnFile(byte byDebugMode)
	{
		int num = 0;
		num = InitParamBeforeDebug();
		string text = textBox_SlvID.Text;
		byte bySlaveId = Convert.ToByte(text, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = bySlaveId;
		byte bySlaveId2 = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
		byte byUvloDis = m_sMotorParamCfg.m_sProtectCfg.byUvloDis;
		text = textBox_UID1.Text;
		bySlaveId = Convert.ToByte(text, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.UID1 = bySlaveId;
		int num2 = 0;
		int num3 = 0;
		num2 = 32;
		if (num != 0)
		{
			return num;
		}
		return num;
	}

	private ushort calcECC(byte[] dwBuf, ushort i32Len)
	{
		byte b = 128;
		for (int i = b; i < i32Len + b; i += 22)
		{
			dwBuf[16 + i] = (byte)((dwBuf[1 + i] ^ dwBuf[2 + i] ^ dwBuf[3 + i] ^ dwBuf[5 + i] ^ dwBuf[8 + i] ^ dwBuf[9 + i] ^ dwBuf[11 + i] ^ dwBuf[14 + i]) & 1);
			dwBuf[17 + i] = (byte)((dwBuf[i] ^ dwBuf[1 + i] ^ dwBuf[2 + i] ^ dwBuf[4 + i] ^ dwBuf[6 + i] ^ dwBuf[8 + i] ^ dwBuf[10 + i] ^ dwBuf[12 + i]) & 1);
			dwBuf[18 + i] = (byte)(~(dwBuf[i] ^ dwBuf[3 + i] ^ dwBuf[4 + i] ^ dwBuf[7 + i] ^ dwBuf[9 + i] ^ dwBuf[10 + i] ^ dwBuf[13 + i] ^ dwBuf[15 + i]) & 1);
			dwBuf[19 + i] = (byte)(~(dwBuf[i] ^ dwBuf[1 + i] ^ dwBuf[5 + i] ^ dwBuf[6 + i] ^ dwBuf[7 + i] ^ dwBuf[11 + i] ^ dwBuf[12 + i] ^ dwBuf[13 + i]) & 1);
			dwBuf[20 + i] = (byte)((dwBuf[2 + i] ^ dwBuf[3 + i] ^ dwBuf[4 + i] ^ dwBuf[5 + i] ^ dwBuf[6 + i] ^ dwBuf[7 + i] ^ dwBuf[14 + i] ^ dwBuf[15 + i]) & 1);
			dwBuf[21 + i] = (byte)((dwBuf[8 + i] ^ dwBuf[9 + i] ^ dwBuf[10 + i] ^ dwBuf[11 + i] ^ dwBuf[12 + i] ^ dwBuf[13 + i] ^ dwBuf[14 + i] ^ dwBuf[15 + i]) & 1);
		}
		return 0;
	}

	private int ConvertRegByte2MtpBitBuf(byte byByteNum, DllDebuggerAPI.S_PreReg psPreReg)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		byte b = 128;
		byte b2 = 2;
		for (num = 0; num < byByteNum; num++)
		{
			num2 = num * 8 + 6 * (num / 2);
			for (num3 = 0; num3 < 8; num3++)
			{
				m_byWEfuseBitArr[b + num2 + num3] = (byte)((psPreReg.adwRegVal[b2 + num] >> num3) & 1);
			}
		}
		return 0;
	}

	private void btn_Generate_Click(object sender, RoutedEventArgs e)
	{
		byte b = 7;
		string text = $"{DateTime.Now:yyyyMMdd_HHmmss}";
		if (b == 7)
		{
			if (!m_isInCommand)
			{
				text = AppDomain.CurrentDomain.BaseDirectory + "\\UserConfigFile\\FT8132_" + text + ".bin";
			}
			else
			{
				string text2 = AppDomain.CurrentDomain.BaseDirectory + "\\GenerateBIN";
				for (int i = 0; i < 2; i++)
				{
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
					}
					text2 += "\\FT8132";
				}
				text = text2 + "_" + text + ".bin";
			}
		}
		m_strWriteBinFile = text;
		textBox_CodeFile.Text = m_strWriteBinFile;
		byte byDebugMode = 0;
		int num = 0;
		Array.Clear(m_byWEfuseBitArr, 0, m_byWEfuseBitArr.Length);
		if (CreateDebugOrDwnFile(byDebugMode) != 0)
		{
			MessageBox.Show("create bin file failed..");
			return;
		}
		int num2 = 0;
		for (num2 = 0; num2 < m_byEfuseArr.Length; num2++)
		{
			m_byEfuseArr[num2] = 0;
		}
		byte b2 = 50;
		for (num2 = 0; num2 < b2; num2++)
		{
			m_byEfuseArr[num2] = (byte)sPreReg.adwRegVal[2 + num2];
		}
		ushort num3 = 0;
		ushort u16shift = ushort.MaxValue;
		num3 = crc16(m_byEfuseArr, 0, 66, u16shift);
		byte b3 = 128;
		ConvertRegByte2MtpBitBuf(b2, sPreReg);
		for (num2 = 0; num2 < 16; num2++)
		{
			m_byWEfuseBitArr[b3 + 726 + num2] = (byte)((num3 >> num2) & 1);
		}
		ushort num4 = 748;
		calcECC(m_byWEfuseBitArr, num4);
		ushort num5 = 0;
		num5 = crc16(m_byWEfuseBitArr, b3, num4, u16shift);
		num3 = num5;
		string content = $"0x{num3:X4}";
		string text3 = textBox_CodeFile.Text;
		if (text3.Length > 0)
		{
			label_SrcCrc.Content = content;
		}
		bool bWriteFlag = true;
		string strWriteBinFile = m_strWriteBinFile;
		if (WriteEfuse2Bin(bWriteFlag, m_byWEfuseBitArr, strWriteBinFile) == 0)
		{
			byte b4 = b3;
			byte b5 = (byte)((m_byWEfuseBitArr[b4 + 3] << 3) + (m_byWEfuseBitArr[b4 + 2] << 2) + (m_byWEfuseBitArr[b4 + 1] << 1));
			content = $"0x{b5 >> 1:X2}";
			label_Bin_I2CAddr.Content = content;
			if (checkBox_SimulateComm.IsChecked == true)
			{
				sPreReg.byRegisterNum = 57;
				LogWriteDataToTxt();
			}
			Downloadtimer.Start();
			m_byWriteBufSrcMode = 1;
		}
	}

	private void textBox_FailedCnt_LostFocus(object sender, RoutedEventArgs e)
	{
	}

	private void textBox_PassedCnt_LostFocus(object sender, RoutedEventArgs e)
	{
	}

	public void slider_WOutMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_WOutMax.Value;
		HandleWOutMaxDisplay(num);
		UpdateWOutMaxDict((byte)num);
	}

	private void UpdateWOutMaxDict(byte value)
	{
		if (m_i16SensorMode != 1)
		{
			return;
		}
		if (m_nPreviousValueDict.Keys.Contains("WOutMax_FOC"))
		{
			if (radioBtn_HallSvMdDis.IsChecked == true)
			{
				m_nPreviousValueDict["WOutMax_FOC"] = value;
			}
		}
		else
		{
			m_nPreviousValueDict.Add("WOutMax_FOC", m_sMotorParamCfg.m_sRunCfg.byWoutMax);
		}
	}

	private int HandleWOutMaxDisplay(ushort wWOutMaxVal)
	{
		double num = 50.0 / 63.0;
		double num2 = num * (double)(int)wWOutMaxVal;
		textBlock_WOutMax.Text = $"{num2:F3}";
		return 0;
	}

	private int HandleAlignDutyDisplay(byte byAlignDuty)
	{
		textBlock_AlignDuty.Text = ((float)(1.5 * (double)(byAlignDuty + 1))).ToString();
		return 0;
	}

	private int HandleAlignTimeDisplay(byte byAlignTime)
	{
		textBlock_AlignTime.Text = ((float)(0.1 * (double)(byAlignTime + 1))).ToString();
		return 0;
	}

	private void slider_YMaxSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_YMaxSel.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel = (byte)num;
		textBlock_YMaxSel.Text = 100.0 - 1.5 * (double)(int)m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel + "%";
		HandleCtrlModeCurveShow();
	}

	private void slider_XON0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_XON0.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byXON0 = (byte)num;
		HandleTxtBlkCurveDisplay(textBlock_XON0, num, 25, 32);
	}

	private void slider_XON_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_XON.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON = (byte)num;
		HandleTxtBlkCurveDisplay(textBlock_XON, num, 50, 64);
	}

	private void slider_YON_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_YON.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON = (byte)num;
		HandleTxtBlkCurveDisplay(textBlock_YON, num, 100, 127);
	}

	private void slider_Y25_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_Y25.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_25 = (byte)num;
		HandleTxtBlkCurveDisplay(textBlock_Y25, num, 100, 127);
	}

	private void slider_Y50_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_Y50.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_50 = (byte)num;
		HandleTxtBlkCurveDisplay(textBlock_Y50, num, 100, 127);
	}

	private void slider_Y75_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_Y75.Value;
		m_sMotorParamCfg.m_sTargetSpdCurve.byY_75 = (byte)num;
		HandleTxtBlkCurveDisplay(textBlock_Y75, num, 100, 127);
	}

	private int HandleTxtBlkCurveDisplay(TextBlock txbBlk, ushort wWOutMaxVal, ushort wFactor1, ushort wFactor2)
	{
		float num = (int)wFactor2;
		float num2 = (float)(int)wFactor1 / num;
		float num3 = num2 * (float)(int)wWOutMaxVal;
		txbBlk.Text = $"{num3:F3}";
		HandleCtrlModeCurveShow();
		return 0;
	}

	public void comboBox_FanDaBeiShu_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FanDaBeiShu.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu = (byte)comboBox_FanDaBeiShu.SelectedIndex;
		}
	}

	private void comboBox_SMPMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		byte b = 0;
		if (comboBox_SMPMode.SelectedValue == null)
		{
			return;
		}
		string text = comboBox_SMPMode.SelectedValue.ToString();
		if (m_i16SensorMode == 1 || m_i16SensorMode == 2)
		{
			for (int i = 0; i < 4; i++)
			{
				string text2 = (string)FindResource("Cfg_FocSmpMode" + i);
				if (text == text2)
				{
					b = (byte)i;
					break;
				}
			}
			if (b == 0 || b == 1)
			{
				radioBtn_OcpMod2.Visibility = Visibility.Hidden;
			}
			else
			{
				radioBtn_OcpMod2.Visibility = Visibility.Visible;
			}
		}
		else
		{
			b = (byte)comboBox_SMPMode.SelectedIndex;
		}
		if (b == 2)
		{
			m_sMotorParamCfg.m_sFocParamCfg.bySMPMode = (byte)(b + 1);
		}
		else
		{
			m_sMotorParamCfg.m_sFocParamCfg.bySMPMode = b;
		}
		if (b == 0)
		{
			radioBtn_AmpFs0.IsChecked = true;
			EnableAmpFs(isEnabled: false);
		}
		else
		{
			EnableAmpFs(isEnabled: true);
		}
	}

	public void comboBox_Seg7En_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sMotorParamCfg.m_sFocParamCfg.bySeg7En = (byte)comboBox_Seg7En.SelectedIndex;
	}

	public void slider_BaseFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		string text = txtBlock_BaseFreq.Text;
		m_sMotorParamCfg.m_sFocParamCfg.wFreqBase = (ushort)slider_BaseFreq.Value;
		if (!textBox_LockMinSpeed.IsEnabled)
		{
			ShowLockMinSpeedValue();
		}
		DisplayReverseBrakeSpeedText();
		byte freqBaseLevel = GetFreqBaseLevel();
		UpdateStatisSpeedTextBlock(freqBaseLevel, (byte)slider_StaticSpeed.Value);
	}

	public void comboBox_ContrlMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_ContrlMode.SelectedValue == null)
		{
			return;
		}
		string text = comboBox_ContrlMode.SelectedValue.ToString();
		if (m_i16SensorMode == 1 && radioBtn_HallSvMdEn.IsChecked == true)
		{
			for (int i = 0; i < 4; i++)
			{
				string text2 = (string)FindResource("Cfg_RunControlMode" + i);
				if (text == text2)
				{
					m_sMotorParamCfg.m_sStartCfg.byControlMode = (byte)i;
					break;
				}
			}
		}
		else
		{
			m_sMotorParamCfg.m_sStartCfg.byControlMode = (byte)comboBox_ContrlMode.SelectedIndex;
		}
		if (m_sMotorParamCfg.m_sStartCfg.byControlMode == 3)
		{
			comboBox_SpdCtrlMD.Items.Remove((string)FindResource("Cfg_OtherSpdCtrlMD0"));
			SetSpdCtrlModeSelectedIndex(m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd);
		}
		else
		{
			AddSpdCtrlModeAllItems();
		}
		HandleSpdCtrlShow(m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd);
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		HandleCurveCfgSpeedDisplay(byControlMode);
		if (text == "SpeedLoop" && radioBtn_HallSvMdEn.IsChecked == true)
		{
			CheckedeCurveSpdDis(0);
			EnableCurveSpdDis(isEnabled: false);
		}
		else
		{
			EnableCurveSpdDis(isEnabled: true);
		}
	}

	public void comboBox_RPDAlign_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_RPDAlign.SelectedValue == null)
		{
			return;
		}
		if (m_i16SensorMode == 1 || m_i16SensorMode == 2)
		{
			string text = comboBox_RPDAlign.SelectedValue.ToString();
			for (int i = 0; i < 5; i++)
			{
				string text2 = (string)FindResource("Cfg_AlignIPDEn" + i);
				if (text == text2)
				{
					m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn = (byte)i;
					break;
				}
			}
		}
		else
		{
			m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn = (byte)comboBox_RPDAlign.SelectedIndex;
		}
		HandleAlignDisplay(m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn);
	}

	public void comboBox_FaultRestoreTimes_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FaultRestoreTimes.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes = (byte)comboBox_FaultRestoreTimes.SelectedIndex;
		}
	}

	public void comboBox_SpdCtrlMD_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_SpdCtrlMD.SelectedIndex < 0)
		{
			return;
		}
		if (comboBox_SpdCtrlMD.Items.Count != 4)
		{
			if (comboBox_SpdCtrlMD.SelectedValue == null)
			{
				return;
			}
			string text = comboBox_SpdCtrlMD.SelectedValue.ToString();
			for (int i = 0; i < 4; i++)
			{
				string text2 = (string)FindResource("Cfg_OtherSpdCtrlMD" + i);
				if (text == text2)
				{
					m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd = (byte)i;
					break;
				}
			}
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd = (byte)comboBox_SpdCtrlMD.SelectedIndex;
		}
		byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
		HandleSpdCtrlShow(bySpdCtrlMd);
	}

	private void comboBox_StartClk_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_StartClk.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sOhterCfg.byStartClk = (byte)comboBox_StartClk.SelectedIndex;
		}
	}

	private void comboBox_RsdBrakeEn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_RsdBrakeEn.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sWindHandleCfg.byIsdBrakeEn = (byte)comboBox_RsdBrakeEn.SelectedIndex;
		}
	}

	public void comboBox_FgDiv_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FgDiv.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sOhterCfg.byFgDiv = (byte)comboBox_FgDiv.SelectedIndex;
		}
	}

	public void comboBox_FgMul_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FgMul.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sOhterCfg.byFgMul = (byte)comboBox_FgMul.SelectedIndex;
		}
	}

	public void comboBox_FgRdMd_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FgRdMd.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sOhterCfg.byFgRdMd = (byte)comboBox_FgRdMd.SelectedIndex;
		}
	}

	private void comboBox_FocDebug12_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FocDebug12.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0 = (byte)comboBox_FocDebug12.SelectedIndex;
		}
	}

	private void comboBox_FocDebug34_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_FocDebug34.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1 = (byte)comboBox_FocDebug34.SelectedIndex;
		}
	}

	private int HandleSpdCtrlShow(byte bySpdCtrlMd)
	{
		string text = "";
		string content = "";
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		if (bySpdCtrlMd == 0 || bySpdCtrlMd == 1)
		{
			radioButton_PwmOnOffhys0.Content = "0.8%";
			radioButton_PwmOnOffhys1.Content = "1.6%";
			text = "PWM:";
		}
		else if (bySpdCtrlMd == 3)
		{
			radioButton_PwmOnOffhys0.Content = "8 Hz";
			radioButton_PwmOnOffhys1.Content = "20 Hz";
			text = "CLOCK:";
		}
		else
		{
			radioBtn_MotorDirDis.IsEnabled = true;
			radioBtn_MotorDirEn.IsEnabled = true;
			radioButton_PwmOnOffhys0.Content = "0.8%";
			radioButton_PwmOnOffhys1.Content = "1.6%";
			text = "PWM:";
		}
		switch (byControlMode)
		{
		case 0:
			content = "OutDuty:";
			break;
		case 1:
			content = "CUR:";
			break;
		case 2:
			content = "RPM:";
			break;
		case 3:
			content = "POWER:";
			break;
		}
		labelPWM.Content = text;
		labelRPM.Content = content;
		return 0;
	}

	private void radioBtn_SPIDebugDis_Checked(object sender, RoutedEventArgs e)
	{
		if (new bool?(false) == true)
		{
			HandleSpiDebug(1);
		}
	}

	private void radioBtn_SPIDebugEn_Checked(object sender, RoutedEventArgs e)
	{
		if (new bool?(false) == true)
		{
			HandleSpiDebug(0);
		}
	}

	private int HandleSpiDebug(byte bySpiDebug)
	{
		return 0;
	}

	public bool InitUserConfig(string strConfigFile)
	{
		try
		{
			if (_strCurPath != null)
			{
			}
			string ptszConfigFile = AppDomain.CurrentDomain.BaseDirectory + "\\conf\\FT8132Debugger.ini";
			m_strFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\log\\FT8132OnoffTest.xls";
			string ptszScriptFile = AppDomain.CurrentDomain.BaseDirectory + strConfigFile;
			if (!DllDebuggerAPI.DLL_GPL32DownloaderInitialize(ptszConfigFile, ptszScriptFile, pfnStatusCallback))
			{
				return false;
			}
			_strCurPath = strConfigFile;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			MessageBox.Show("load dll failed.");
		}
		finally
		{
		}
		return true;
	}

	private void win_SourceInitialized(object sender, EventArgs e)
	{
		if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
		{
			hwndSource.AddHook(WndProc);
		}
	}

	protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		try
		{
			if (537 == msg)
			{
				LogCmdErrorInfo("Device change.");
				int num = wParam.ToInt32();
				OutputDebugString(num.ToString());
				i32Flag = 0;
				switch (num)
				{
				case 32768:
				{
					DriveInfo[] drives = DriveInfo.GetDrives();
					DriveInfo[] array = drives;
					foreach (DriveInfo driveInfo in array)
					{
						if (driveInfo.DriveType == DriveType.Removable)
						{
							i32Flag = 1;
							break;
						}
					}
					break;
				}
				case 32772:
					i32Flag = 2;
					break;
				}
				if (!DllDebuggerAPI.DLL_GPL32DownloaderEnumerateDevices())
				{
					return IntPtr.Zero;
				}
				int num2 = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceCount();
				if (num2 > 0)
				{
					i32Flag = 1;
				}
				else
				{
					i32Flag = 2;
				}
				Thread.Sleep(40);
				EnumDevice();
				i32Flag = 0;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
		return IntPtr.Zero;
	}

	public void WriteEnumDeviceLog(string strMsg)
	{
		string text = AppDomain.CurrentDomain.BaseDirectory + "\\log";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text += "\\EnumDC.log";
		if (!File.Exists(text))
		{
			File.Create(text);
		}
		StreamWriter streamWriter = new StreamWriter(text, append: true);
		streamWriter.WriteLine("Time:" + DateTime.Now.ToString());
		streamWriter.WriteLine(strMsg);
		streamWriter.Close();
		streamWriter.Dispose();
	}

	public bool EnumDevice()
	{
		LogCmdErrorInfo("Enum device.");
		if (!DllDebuggerAPI.DLL_GPL32DownloaderEnumerateDevices())
		{
			return false;
		}
		int num = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceCount();
		int num2 = 0;
		int num3 = arrDeviceSN.Count();
		LogCmdErrorInfo("Device count: " + num);
		if (i32Flag == 1 && num3 > 0)
		{
			return true;
		}
		if (i32Flag == 2 && num3 <= 0)
		{
			return true;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = 50;
		if (num < num3)
		{
			for (int i = 0; i < num3; i++)
			{
				num2 = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceSerial(i, stringBuilder);
				int num4 = 0;
				string[] array = arrDeviceSN.ToArray();
				foreach (string text in array)
				{
					if (text.CompareTo(stringBuilder.ToString()) == 0)
					{
						num4++;
						continue;
					}
					arrPulloutDeviceSN.Add(text);
					arrDeviceSN.Remove(text);
					m_wHubIndex = arrPortMapInfo.ElementAt(num4).wHubIndex;
					m_wPortIndex = arrPortMapInfo.ElementAt(num4).wPortIndex;
					arrPortMapInfo.RemoveAt(num4);
					LogCmdErrorInfo("New SN: " + text);
				}
			}
		}
		if (i32Flag == 1)
		{
			for (int i = 0; i < num; i++)
			{
				if (m_DevList.Count > 0)
				{
					CDevListItemData cDevListItemData = m_DevList.ElementAt(i);
					if (cDevListItemData.DevWorkState != CDevListItemData.WORKER_STATE.WS_IDLE)
					{
						continue;
					}
				}
				CDevListItemData cDevListItemData2 = new CDevListItemData();
				try
				{
					num2 = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceSerial(i, stringBuilder);
					LogCmdErrorInfo("Device plugs in. SN: " + stringBuilder.ToString());
					if (stringBuilder == null || stringBuilder.Length == 0)
					{
						return false;
					}
				}
				catch (Exception)
				{
					stringBuilder.Append("fail");
				}
				cDevListItemData2.DevWorkState = CDevListItemData.WORKER_STATE.WS_READY;
				arrDeviceSN.Add(stringBuilder.ToString());
				cDevListItemData2.m_strDevSN = stringBuilder.ToString();
				m_DevList.Add(cDevListItemData2);
				DllDebuggerAPI.S_DevMapInfo psMapInfo = default(DllDebuggerAPI.S_DevMapInfo);
				if (!DllDebuggerAPI.DLL_GPDwnLibGetUSBPortInfoByIdx(i, ref psMapInfo))
				{
					return false;
				}
				arrPortMapInfo.Add(psMapInfo);
				string text2 = new string(psMapInfo.tszDevSN);
				if (!PluginUSBDevice(stringBuilder.ToString(), psMapInfo.wHubIndex, psMapInfo.wPortIndex))
				{
					return false;
				}
				statusBar1.Items.Clear();
				TextBlock textBlock = new TextBlock();
				textBlock.Text = "Device:";
				textBlock.Width = 50.0;
				textBlock.HorizontalAlignment = HorizontalAlignment.Left;
				statusBar1.Items.Add(textBlock);
				TextBlock textBlock2 = new TextBlock();
				textBlock2.Text = "Connected";
				textBlock2.Width = 70.0;
				textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
				textBlock2.VerticalAlignment = VerticalAlignment.Stretch;
				statusBar1.Items.Add(textBlock2);
				TextBlock textBlock3 = new TextBlock();
				textBlock3.Text = "Elaps Time:";
				textBlock3.Width = 80.0;
				statusBar1.Items.Add(textBlock3);
				TextBlock textBlock4 = new TextBlock();
				textBlock4.Text = "00:00:00";
				textBlock4.Width = 80.0;
				statusBar1.Items.Add(textBlock4);
				Ellipse ellipse = new Ellipse();
				ellipse.HorizontalAlignment = HorizontalAlignment.Right;
				ellipse.VerticalAlignment = VerticalAlignment.Center;
				ellipse.Width = 12.0;
				ellipse.Height = 12.0;
				ellipse.Fill = flagEllipse.Background;
				statusBar1.Items.Add(ellipse);
				TextBlock textBlock5 = new TextBlock();
				textBlock5.Width = 80.0;
				textBlock5.Text = lblMessage.Content.ToString();
				statusBar1.Items.Add(textBlock5);
				TextBlock textBlock6 = new TextBlock();
				textBlock6.Text = "All rights reserved. © 2021 Fortior Tech.";
				statusBar1.Items.Add(textBlock6);
				btn_ReadID.IsEnabled = true;
				btn_WriteID.IsEnabled = true;
				m_bFindDev = true;
			}
		}
		if (i32Flag == 2)
		{
			if (arrPulloutDeviceSN.Count > 0)
			{
				int i = 0;
				foreach (string item in arrPulloutDeviceSN)
				{
					for (i = 0; i < m_DevList.Count; i++)
					{
						string strDevSN = m_DevList.ElementAt(i).m_strDevSN;
						if (strDevSN.CompareTo(item) != 0)
						{
							continue;
						}
						if (m_DevList.ElementAt(i).DevWorkState == CDevListItemData.WORKER_STATE.WS_DOINGTASK)
						{
							do
							{
								bool flag = true;
							}
							while (m_DevList.ElementAt(i).DevWorkState != CDevListItemData.WORKER_STATE.WS_ENDEDTASK);
							PulloutUSBDevice(item);
							m_DevList.RemoveAt(i);
							LogCmdErrorInfo("Device pulls out.");
							continue;
						}
						PulloutUSBDevice(item);
						m_DevList.RemoveAt(i);
						statusBar1.Items.Clear();
						TextBlock textBlock = new TextBlock();
						textBlock.Text = "Device:";
						textBlock.Width = 50.0;
						statusBar1.Items.Add(textBlock);
						TextBlock textBlock2 = new TextBlock();
						textBlock2.Text = "Disconnected";
						textBlock2.Width = 80.0;
						statusBar1.Items.Add(textBlock2);
						TextBlock textBlock3 = new TextBlock();
						textBlock3.Text = "Elaps Time:";
						textBlock3.Width = 80.0;
						statusBar1.Items.Add(textBlock3);
						TextBlock textBlock4 = new TextBlock();
						textBlock4.Text = "00:00:00";
						textBlock4.Width = 80.0;
						statusBar1.Items.Add(textBlock4);
						Ellipse ellipse = new Ellipse();
						ellipse.HorizontalAlignment = HorizontalAlignment.Right;
						ellipse.VerticalAlignment = VerticalAlignment.Center;
						ellipse.Width = 12.0;
						ellipse.Height = 12.0;
						ellipse.Fill = Brushes.Gray;
						statusBar1.Items.Add(ellipse);
						m_byMotorStatus = 0;
						m_byFaultStatus = 0;
						m_wRunningSpeed = 0;
						BrushConverter brushConverter = new BrushConverter();
						Brush borderBrush = (Brush)brushConverter.ConvertFromString("#FFb2b5b7");
						flagEllipse.BorderBrush = borderBrush;
						flagEllipse.Background = Brushes.Gray;
						TextBlock textBlock5 = new TextBlock();
						textBlock5.Width = 80.0;
						textBox_MotorStatus.Text = "0x" + m_byMotorStatus.ToString("X2");
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.Capacity = 30;
						GetMotorStatusByIndex(stringBuilder2, m_byMotorStatus);
						lblMessage.Content = stringBuilder2.ToString();
						textBlock5.Text = lblMessage.Content.ToString();
						statusBar1.Items.Add(textBlock5);
						TextBlock textBlock6 = new TextBlock();
						textBlock6.Text = "All rights reserved. © 2021 Fortior Tech.";
						statusBar1.Items.Add(textBlock6);
					}
				}
			}
			btnDebug.IsEnabled = false;
			btnStartBw.IsEnabled = false;
			btn_ReadID.IsEnabled = false;
			btn_WriteID.IsEnabled = false;
			btn_OnOffTest.IsEnabled = false;
			m_bFindDev = false;
		}
		return true;
	}

	private bool PluginUSBDevice(string lpszDeviceSN, ushort wHubIndex, ushort wPortIndex)
	{
		bool flag = false;
		if (!DllDebuggerAPI.DLL_GPL32DownloaderSelectDevice(lpszDeviceSN))
		{
			return false;
		}
		string strConfigFile = "\\conf\\FT8132Debugger.conf";
		flag = InitUserConfig(strConfigFile);
		if (!flag)
		{
			return false;
		}
		return flag;
	}

	private bool PulloutUSBDevice(string lpszDeviceSN)
	{
		if (!DllDebuggerAPI.DLL_GPL32DownloadPlugOutDevice(m_wHubIndex, m_wPortIndex, lpszDeviceSN))
		{
			return false;
		}
		return true;
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		textBox_MotorStatus.Text = "0x" + m_byMotorStatus.ToString("X2");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = 30;
		GetMotorStatusByIndex(stringBuilder, m_byMotorStatus);
		lblMessage.Content = stringBuilder.ToString();
		if (statusBar1.Items.Count > 6)
		{
			TextBlock textBlock = new TextBlock();
			textBlock = (TextBlock)statusBar1.Items.GetItemAt(5);
			textBlock.Text = lblMessage.Content.ToString();
		}
		statusBar1.Items.Refresh();
		if (m_byMotorStatus == 7)
		{
			flagEllipse.Background = Brushes.Red;
			flagEllipse.BorderBrush = Brushes.Red;
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Capacity = 30;
			GetFaultStringByIndex(stringBuilder2, m_byFaultStatus);
			textBox_FaultStatus.Text = m_byFaultStatus.ToString();
			if (m_eBtnOptType == E_BtnOptType.eBtn_OPT_ON_OFF_TEST)
			{
				if (m_byFaultStatus != 0)
				{
					if (m_byOldFaultVal != m_byFaultStatus)
					{
						m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt++;
						ushort wFailCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt;
						textBox_FailCnt.Text = wFailCnt.ToString();
						m_wFailRealCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt;
						m_byOldFaultVal = m_byFaultStatus;
						OutputDebugString("old_val" + m_byOldFaultVal);
						byte bySpdCtrl = 0;
						m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = bySpdCtrl;
						int num = ExecuteDebugOperation();
						string strFilePath = m_strFilePath;
						m_strFailRemark = stringBuilder2.ToString();
						OutputDebugString(m_strFailRemark);
						SaveAsExcel(strFilePath);
					}
				}
				else
				{
					m_byOldFaultVal = 0;
					OutputDebugString("clear old_val");
				}
			}
		}
		else
		{
			if (m_wRunningSpeed != 0)
			{
				Color color = Color.FromRgb(115, 190, 29);
				Brush brush = new SolidColorBrush(color);
				flagEllipse.Background = brush;
				flagEllipse.BorderBrush = brush;
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Capacity = 30;
			GetFaultStringByIndex(stringBuilder2, m_byFaultStatus);
			textBox_FaultStatus.Text = m_byFaultStatus.ToString();
		}
		uint num2 = (uint)((float)(int)m_wRunningSpeed / 32768f * m_fMotorSpeedBase);
		if (num2 == 0 && m_byMotorStatus == 0)
		{
			OutputDebugString("timer_spd=0_motorSta=0\r\n");
			string text = "ms" + m_byMotorStatus;
			string text2 = "rs" + m_wRunningSpeed;
			string text3 = text + text2;
			if (myStoryboard != null)
			{
				if (m_byFlagEllipseStopCnt > 0 && 1 == m_byFlagEllipseStopCnt)
				{
					flagEllipse.BorderBrush = Brushes.Yellow;
					flagEllipse.Background = Brushes.Yellow;
					myStoryboard.Remove(flagEllipse);
					myStoryboard = null;
					m_byFlagEllipseStopCnt = 6;
					Ellipse ellipse = new Ellipse();
					ellipse = (Ellipse)statusBar1.Items.GetItemAt(4);
					ellipse.Fill = Brushes.Yellow;
					TextBlock textBlock2 = new TextBlock();
					textBlock2 = (TextBlock)statusBar1.Items.GetItemAt(5);
					textBlock2.Text = lblMessage.Content.ToString();
					statusBar1.Items.Refresh();
					if (m_byPreFaultCode != 4 && m_byPreFaultCode != 8)
					{
						if (m_byStartTimeDelay != 0)
						{
							m_byStartTimeDelay = 0;
							Thread.Sleep(1500);
						}
						else
						{
							DllDebuggerAPI._sTotalStepCnt = 0;
						}
						OutputDebugString("timer_totalCnt=0\r\n");
					}
				}
				m_byFlagEllipseStopCnt--;
			}
			else if (m_byMotorStatus == 0)
			{
				OutputDebugString("timer_motorIdle\r\n");
				flagEllipse.Background = Brushes.Yellow;
				flagEllipse.BorderBrush = Brushes.Yellow;
			}
		}
		Ellipse ellipse2 = new Ellipse();
		ellipse2 = (Ellipse)statusBar1.Items.GetItemAt(4);
		ellipse2.Fill = flagEllipse.Background;
		statusBar1.Items.Refresh();
		textBox_TargetSpeed.Text = num2.ToString();
		string text4 = $"0x{m_wTheta:X4}";
		textBox_THETA.Text = text4;
		text4 = $"0x{m_wOmega:X4}";
		textBox_OMEGA.Text = text4;
		text4 = $"0x{m_wUDCVal:X4}";
		textBox_UDC.Text = text4;
		text4 = $"0x{m_wIPDPositionVal:X4}";
		textBox_IPDPOSITION.Text = text4;
		text4 = $"0x{m_wWOUTVal:X4}";
		textBox_WOUT.Text = text4;
		text4 = $"0x{m_wUQVal:X4}";
		textBox_UQ.Text = text4;
		text4 = $"0x{m_wUDVal:X4}";
		textBox_UD.Text = text4;
		text4 = $"0x{m_wIA_MaxVal:X4}";
		textBox_IA_MAX.Text = text4;
		text4 = $"0x{m_wIB_MaxVal:X4}";
		textBox_IB_MAX.Text = text4;
		text4 = $"0x{m_wIC_MaxVal:X4}";
		textBox_IC_MAX.Text = text4;
		text4 = $"0x{m_wIA_ARefVal:X4}";
		textBox_IA_AREF.Text = text4;
		text4 = $"0x{m_wIB_ARefVal:X4}";
		textBox_IB_AREF.Text = text4;
		text4 = $"0x{m_wIC_ARefVal:X4}";
		textBox_IC_AREF.Text = text4;
		text4 = $"0x{m_wEmfVal:X4}";
		textBox_EMF.Text = text4;
		text4 = $"0x{m_wPwmReal:X4}";
		textBox_PWM_REAL.Text = text4;
		text4 = $"0x{m_wIQFDBVal:X4}";
		textBox_IQFDB.Text = text4;
		text4 = $"0x{m_wTimArrVal:X4}";
		textBox_tim_arr.Text = text4;
		text4 = $"0x{m_wTrip:X4}";
		textBox_ITRIP.Text = text4;
		text4 = $"0x{m_wPower:X4}";
		textBox_POWER.Text = text4;
		text4 = $"0x{m_wHallOmega:X4}";
		textBox_HallOmega.Text = text4;
		text4 = $"0x{m_wRsv:X4}";
		textBox_RSV.Text = text4;
		text4 = $"0x{m_wSpdCtrl:X4}";
		textBox_StatusReg_SpdCtrl.Text = text4;
		if (m_byFaultStatus > 0)
		{
		}
		for (int i = 0; i < m_DevList.Count; i++)
		{
		}
	}

	private int ComputeFibonacci(object sender, DoWorkEventArgs e)
	{
		int num = (int)e.Argument;
		int num2 = 0;
		byte b = 0;
		m_byFaultCnt = 0;
		DllDebuggerAPI.S_ByteArrayDef pbyNandID = new DllDebuggerAPI.S_ByteArrayDef
		{
			byStatusBuf = new byte[64]
		};
		byte byOptType = 1;
		byte byAccType = 2;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 2;
		}
		DllDebuggerAPI.S_PreReg s_PreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		ClearStatusReg();
		int num3 = 0;
		while (true)
		{
			bool flag = true;
			OutputDebugString("While true\r\n");
			try
			{
				if (bw.CancellationPending)
				{
					e.Cancel = true;
					return -1;
				}
				uint num4 = 64u;
				int count = arrDeviceSN.Count;
				string ptszDeviceSN = ((0 == count) ? "Fortior Device1" : arrDeviceSN.First());
				if (DllDebuggerAPI._sTotalStepCnt > 0)
				{
					num4 = 2u;
					num2 = DllDebuggerAPI.DLL_GPLB32DownloaderGetNandID(ptszDeviceSN, num4, ref pbyNandID, 2u);
					if (num2 == 0)
					{
						m_wSpdCtrl = (ushort)(pbyNandID.byStatusBuf[0] << 8);
						m_wSpdCtrl |= pbyNandID.byStatusBuf[1];
					}
					num4 = 64u;
					for (num3 = 0; num3 < 3; num3++)
					{
						num2 = DllDebuggerAPI.DLL_GPLB32DownloaderGetNandID(ptszDeviceSN, num4, ref pbyNandID, 44u);
						if (num2 != 0)
						{
							RestoreReadThread();
							MessageBox.Show("usb communication fail.", "CF");
							num2 = 1;
							break;
						}
						m_byCommunicationStatus = 1;
						s_PreReg.byRegisterNum = 2;
						s_PreReg.adwRegVal[0] = 0u;
						s_PreReg.adwRegVal[1] = 1u;
						num2 = GetCommStatusCmd();
						if (num2 != 0)
						{
							RestoreReadThread();
							OutputDebugString("While--ret!=0--2\r\n");
							MessageBox.Show("usb communication fail.", "CF Comm");
							num2 = 1;
							break;
						}
						if (m_byCommunicationStatus != 1)
						{
							break;
						}
						if (num3 != 1)
						{
							continue;
						}
						num2 = ScanI2CAddrDevID();
						if (num2 != 0)
						{
							if (!m_isCmdOpera)
							{
								MessageBox.Show("usb communication fail.", "CF Scan");
							}
							num2 = 1;
							break;
						}
						if (m_byCommunicationStatus == 1)
						{
							num2 = 1;
							break;
						}
					}
				}
				if (num2 == 0)
				{
					num2 = 0;
					i32Test = 1;
					m_byMotorStatus = pbyNandID.byStatusBuf[0];
					m_byFaultStatus = pbyNandID.byStatusBuf[1];
					m_wRunningSpeed = pbyNandID.byStatusBuf[4];
					m_wRunningSpeed = (ushort)((m_wRunningSpeed << 8) | pbyNandID.byStatusBuf[5]);
					m_wOmega = m_wRunningSpeed;
					m_wTheta = pbyNandID.byStatusBuf[2];
					m_wTheta = (ushort)((m_wTheta << 8) | pbyNandID.byStatusBuf[3]);
					m_wUDCVal = pbyNandID.byStatusBuf[6];
					m_wUDCVal = (ushort)((m_wUDCVal << 8) | pbyNandID.byStatusBuf[7]);
					m_wIPDPositionVal = pbyNandID.byStatusBuf[8];
					m_wIPDPositionVal = (ushort)((m_wIPDPositionVal << 8) | pbyNandID.byStatusBuf[9]);
					m_wWOUTVal = pbyNandID.byStatusBuf[10];
					m_wWOUTVal = (ushort)((m_wWOUTVal << 8) | pbyNandID.byStatusBuf[11]);
					m_wUQVal = pbyNandID.byStatusBuf[12];
					m_wUQVal = (ushort)((m_wUQVal << 8) | pbyNandID.byStatusBuf[13]);
					m_wUDVal = pbyNandID.byStatusBuf[14];
					m_wUDVal = (ushort)((m_wUDVal << 8) | pbyNandID.byStatusBuf[15]);
					m_wIA_MaxVal = pbyNandID.byStatusBuf[16];
					m_wIA_MaxVal = (ushort)((m_wIA_MaxVal << 8) | pbyNandID.byStatusBuf[17]);
					m_wIB_MaxVal = pbyNandID.byStatusBuf[18];
					m_wIB_MaxVal = (ushort)((m_wIB_MaxVal << 8) | pbyNandID.byStatusBuf[19]);
					m_wIC_MaxVal = pbyNandID.byStatusBuf[20];
					m_wIC_MaxVal = (ushort)((m_wIC_MaxVal << 8) | pbyNandID.byStatusBuf[21]);
					m_wIA_ARefVal = pbyNandID.byStatusBuf[22];
					m_wIA_ARefVal = (ushort)((m_wIA_ARefVal << 8) | pbyNandID.byStatusBuf[23]);
					m_wIB_ARefVal = pbyNandID.byStatusBuf[24];
					m_wIB_ARefVal = (ushort)((m_wIB_ARefVal << 8) | pbyNandID.byStatusBuf[25]);
					m_wIC_ARefVal = pbyNandID.byStatusBuf[26];
					m_wIC_ARefVal = (ushort)((m_wIC_ARefVal << 8) | pbyNandID.byStatusBuf[27]);
					m_wEmfVal = pbyNandID.byStatusBuf[28];
					m_wEmfVal = (ushort)((m_wEmfVal << 8) | pbyNandID.byStatusBuf[29]);
					m_wPwmReal = pbyNandID.byStatusBuf[30];
					m_wPwmReal = (ushort)((m_wPwmReal << 8) | pbyNandID.byStatusBuf[31]);
					m_wIQFDBVal = pbyNandID.byStatusBuf[32];
					m_wIQFDBVal = (ushort)((m_wIQFDBVal << 8) | pbyNandID.byStatusBuf[33]);
					m_wTimArrVal = pbyNandID.byStatusBuf[34];
					m_wTimArrVal = (ushort)((m_wTimArrVal << 8) | pbyNandID.byStatusBuf[35]);
					m_wTrip = pbyNandID.byStatusBuf[36];
					m_wTrip = (ushort)((m_wTrip << 8) | pbyNandID.byStatusBuf[37]);
					m_wPower = pbyNandID.byStatusBuf[38];
					m_wPower = (ushort)((m_wPower << 8) | pbyNandID.byStatusBuf[39]);
					m_wHallOmega = pbyNandID.byStatusBuf[40];
					m_wHallOmega = (ushort)((m_wHallOmega << 8) | pbyNandID.byStatusBuf[41]);
					m_wRsv = pbyNandID.byStatusBuf[42];
					m_wRsv = (ushort)((m_wRsv << 8) | pbyNandID.byStatusBuf[43]);
					if (m_byFaultCnt == 1 && m_wRunningSpeed == 0)
					{
						OutputDebugString("While--ret!=0--4\r\n");
						num2 = 1;
					}
					if (m_byMotorStatus == 7)
					{
						OutputDebugString("While--ms fault\r\n");
						num2 = 0;
						if (m_byFaultStatus == 4 || m_byFaultStatus == 8)
						{
							OutputDebugString("While--U/O vol\r\n");
							Thread.Sleep(800);
						}
						else
						{
							Thread.Sleep(5000);
						}
						m_byFaultCnt = 1;
						m_byPreFaultCode = m_byFaultStatus;
					}
					if (m_wRunningSpeed != 0)
					{
					}
					continue;
				}
				OutputDebugString("While--ret!=0--3\r\n");
				num2 = 1;
				i32Test = 0;
			}
			catch (Exception)
			{
				continue;
			}
			finally
			{
			}
			break;
		}
		return num2;
	}

	public void CompleteWork(object sender, RunWorkerCompletedEventArgs e)
	{
		dtEndTime = DateTime.Now;
		string text = dtEndTime.Subtract(dtBeginTime).ToString();
		string text2 = text.Substring(0, 11);
		string messageBoxText;
		if (e.Error != null)
		{
			lblMessage.Content = "ERROR";
			flagEllipse.Background = Brushes.Red;
			flagEllipse.BorderBrush = Brushes.Red;
			messageBoxText = $"An error occurred: {e.Error.Message}";
			MessageBox.Show(messageBoxText);
			return;
		}
		TextBlock textBlock;
		TextBlock textBlock2;
		TextBlock textBlock3;
		TextBlock textBlock4;
		Ellipse ellipse;
		TextBlock textBlock5;
		if (e.Cancelled)
		{
			lblMessage.Content = "PAUSE";
			Brush brush = new SolidColorBrush(Color.FromRgb(1, 121, 186));
			flagEllipse.Background = brush;
			flagEllipse.BorderBrush = brush;
			if (myStoryboard != null)
			{
				myStoryboard.Remove(flagEllipse);
				myStoryboard = null;
			}
			statusBar1.Items.Clear();
			textBlock = new TextBlock();
			textBlock.Text = "Device:";
			textBlock.Width = 50.0;
			statusBar1.Items.Add(textBlock);
			textBlock2 = new TextBlock();
			textBlock2.Text = "Pause...";
			textBlock2.Width = 70.0;
			statusBar1.Items.Add(textBlock2);
			textBlock3 = new TextBlock();
			textBlock3.Text = "Elaps Time:";
			textBlock3.Width = 80.0;
			statusBar1.Items.Add(textBlock3);
			textBlock4 = new TextBlock();
			textBlock4.Text = text2;
			textBlock4.Width = 80.0;
			statusBar1.Items.Add(textBlock4);
			ellipse = new Ellipse();
			ellipse.HorizontalAlignment = HorizontalAlignment.Right;
			ellipse.VerticalAlignment = VerticalAlignment.Center;
			ellipse.Width = 12.0;
			ellipse.Height = 12.0;
			ellipse.Fill = flagEllipse.Background;
			da.AutoReverse = false;
			statusBar1.Items.Add(ellipse);
			textBlock5 = new TextBlock();
			textBlock5.Text = lblMessage.Content.ToString();
			statusBar1.Items.Add(textBlock5);
			return;
		}
		int num = 0;
		int num2 = 0;
		string text3 = "";
		messageBoxText = $"Result = {e.Result}";
		if (0 == (int)e.Result)
		{
			da.AutoReverse = false;
			Color color = Color.FromRgb(115, 190, 29);
			Brush brush = new SolidColorBrush(color);
			flagEllipse.Background = brush;
			flagEllipse.BorderBrush = brush;
			m_i32SuccCnt++;
			m_dwOkFailFlag = 0u;
		}
		else
		{
			flagEllipse.BorderBrush = Brushes.Red;
			flagEllipse.Background = Brushes.Red;
			m_i32FailCnt++;
			m_dwOkFailFlag = 1u;
		}
		statusBar1.Items.Clear();
		textBlock = new TextBlock();
		textBlock.Text = "Device:";
		textBlock.Width = 50.0;
		textBlock.HorizontalAlignment = HorizontalAlignment.Left;
		statusBar1.Items.Add(textBlock);
		textBlock2 = new TextBlock();
		textBlock2.Text = "Stop...";
		textBlock2.Width = 70.0;
		textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
		textBlock2.VerticalAlignment = VerticalAlignment.Stretch;
		statusBar1.Items.Add(textBlock2);
		textBlock3 = new TextBlock();
		textBlock3.Text = "Elaps Time:";
		textBlock3.Width = 80.0;
		statusBar1.Items.Add(textBlock3);
		textBlock4 = new TextBlock();
		textBlock4.Text = text2;
		textBlock4.Width = 80.0;
		statusBar1.Items.Add(textBlock4);
		ellipse = new Ellipse();
		ellipse.HorizontalAlignment = HorizontalAlignment.Right;
		ellipse.VerticalAlignment = VerticalAlignment.Center;
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = flagEllipse.Background;
		da.AutoReverse = false;
		statusBar1.Items.Add(ellipse);
		textBlock5 = new TextBlock();
		if ((int)e.Result == 0)
		{
			textBlock5.Text = lblMessage.Content.ToString();
		}
		else
		{
			textBlock5.Text = " ";
		}
		textBlock5.Width = 80.0;
		textBlock5.Text = lblMessage.Content.ToString();
		statusBar1.Items.Add(textBlock5);
		if (myStoryboard != null)
		{
			myStoryboard.Remove(flagEllipse);
			myStoryboard = null;
		}
		myStaustStoryboard.Remove(StatusLed);
		myStaustStoryboard = null;
	}

	private int WriteChipIDReg()
	{
		int num = 0;
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384],
			byRegisterNum = 1
		};
		string text = textBox_SlvID.Text;
		byte bySlaveId = Convert.ToByte(text, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = bySlaveId;
		byte bySlaveId2 = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
		byte byUvloDis = m_sMotorParamCfg.m_sProtectCfg.byUvloDis;
		psPreReg.adwRegVal[1] = (byte)((bySlaveId2 << 1) + (byUvloDis & 1));
		psPreReg.adwRegAdd[1] = 136u;
		m_byREfuseArr[38] = (byte)psPreReg.adwRegVal[1];
		num = DllDebuggerAPI.DLL_GPLB32CmdPassThrough(1, ref psPreReg);
		if (num != 0)
		{
			return num;
		}
		return num;
	}

	private int SetCurrentDevID()
	{
		int num = 0;
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384],
			byRegisterNum = 2
		};
		psPreReg.adwRegAdd[0] = 54u;
		string text = textBox_SetI2CAddr.Text;
		if (text == "")
		{
			LogCmdErrorInfo("slvid empty.");
			return 1;
		}
		LogCmdErrorInfo("slvid: " + text);
		byte b = Convert.ToByte(text, 16);
		psPreReg.adwRegVal[0] = b;
		psPreReg.adwRegAdd[1] = 55u;
		psPreReg.adwRegVal[1] = 5u;
		byte byOptType = 4;
		byte byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 2;
		}
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(4, ref psPreReg);
		if (num != 0)
		{
			return num;
		}
		return num;
	}

	private int GetCommStatusCmd()
	{
		int num = 0;
		m_byCommunicationStatus = 1;
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384],
			byRegisterNum = 2
		};
		byte byOptType = 4;
		byte byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 2;
		}
		psPreReg.adwRegVal[0] = 4u;
		psPreReg.adwRegVal[1] = 1u;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(3, ref psPreReg);
		if (num != 0)
		{
			return num;
		}
		m_bySlaveId = (byte)psPreReg.adwRegVal[0];
		m_byCommunicationStatus = (byte)psPreReg.adwRegVal[1];
		return num;
	}

	private int GetCommStatus()
	{
		int num = 0;
		m_byCommunicationStatus = 1;
		num = GetCommStatusCmd();
		if (num != 0)
		{
			LogCmdErrorInfo("get comm status: " + m_byCommunicationStatus);
			return num;
		}
		LogCmdErrorInfo("get comm status fail: " + m_byCommunicationStatus);
		if (m_byCommunicationStatus == 0)
		{
			byte bySlaveId = m_bySlaveId;
			m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = (byte)(bySlaveId & 0xFF);
			string text = $"0x{m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId:X2}";
			textBox_SetI2CAddr.Text = text;
		}
		return num;
	}

	private int SetVoltage55V(byte byVal)
	{
		int num = 0;
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			byRegisterNum = 2,
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byOptType = byVal;
		byte byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 2;
		}
		psPreReg.adwRegVal[0] = 4u;
		psPreReg.adwRegVal[1] = 0u;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(4, ref psPreReg);
		if (num != 0)
		{
			return num;
		}
		return num;
	}

	private int ScanI2CAddrDevID()
	{
		int num = 0;
		LogCmdErrorInfo("scan i2c addr and id.");
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			byRegisterNum = 2,
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byOptType = 4;
		byte byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 1;
		}
		byte b = 0;
		byte b2 = 2;
		while (b2-- > 0)
		{
			psPreReg.adwRegVal[0] = 4u;
			psPreReg.adwRegVal[1] = 0u;
			num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(3, ref psPreReg);
			if (num == 0)
			{
				m_byCommunicationStatus = (byte)psPreReg.adwRegVal[1];
				if (m_byCommunicationStatus == 0)
				{
					break;
				}
			}
			Thread.Sleep(1000);
		}
		if (num != 0)
		{
			return 2;
		}
		if (m_byCommunicationStatus == 1)
		{
			num = 3;
			if (!m_isCmdOpera)
			{
				MessageBox.Show("Communication fail, Are you sure to connect the target?");
			}
			return num;
		}
		b = (byte)psPreReg.adwRegVal[0];
		return num;
	}

	private void ReEnumDevSN()
	{
		if (DllDebuggerAPI.DLL_GPL32DownloaderEnumerateDevices())
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceCount();
			for (int i = 0; i < num; i++)
			{
				int num2 = DllDebuggerAPI.DLL_GPL32DownloaderGetDeviceSerial(i, stringBuilder);
				LogCmdErrorInfo("reenum device SN: " + stringBuilder.ToString());
			}
		}
	}

	private void btn_WriteID_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			byRegisterNum = 1,
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byOptType = 5;
		byte byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			num = 2;
			return;
		}
		m_byCommunicationStatus = 0;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(4, ref psPreReg) != 0)
		{
			m_byCommunicationStatus = 1;
			return;
		}
		byte b = 0;
		if (m_byCommunicationStatus == 1)
		{
			MessageBox.Show("Communication fail, Are you sure to connect the target?");
			return;
		}
		b = byte.MaxValue;
		InitAllBtnDisplay(bParamEn: false);
		byOptType = 1;
		bool flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = 0u;
		num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(7, ref psPreReg);
	}

	private int SurspendReadThread()
	{
		if (DllDebuggerAPI._sTotalStepCnt == 1)
		{
			DllDebuggerAPI._sTotalStepCnt = 0;
			m_wMotorFlag = 1;
			Thread.Sleep(20);
		}
		return 0;
	}

	private int RestoreReadThread()
	{
		if (m_wMotorFlag != 0)
		{
			DllDebuggerAPI._sTotalStepCnt = 1;
		}
		return 0;
	}

	private void btn_ReadID_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = "";
		bool flag = true;
		if (!btn_ReadID.IsEnabled)
		{
			text = "Please connect device...";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return;
		}
		SurspendReadThread();
		DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
		{
			adwRegAdd = new uint[384],
			adwRegVal = new uint[384]
		};
		byte byAccType = 2;
		byte byOptType = 49;
		bool flag2 = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = 0u;
		num3 = DllDebuggerAPI.DLL_FT8215CmdPassThrough(7, ref psPreReg);
		Thread.Sleep(TimeSpan.FromMilliseconds(50.0));
		num3 = ScanI2CAddrDevID();
		if (num3 != 0)
		{
			RestoreReadThread();
			text = "USB communication fail at readid_click. Ret: " + num3;
			LogCmdErrorInfo(text);
			ReEnumDevSN();
			if (!m_isCmdOpera && !m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			return;
		}
		if (m_byCommunicationStatus == 1)
		{
			RestoreReadThread();
			return;
		}
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 1u;
		psPreReg.adwRegVal[0] = 0u;
		byOptType = 19;
		byAccType = 19;
		flag2 = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg) != 0 || (byte)psPreReg.adwRegVal[0] != 2)
		{
			RestoreReadThread();
			return;
		}
		m_byREfuseArr[39] = (byte)psPreReg.adwRegVal[0];
		byOptType = 19;
		byAccType = 19;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			RestoreReadThread();
			num3 = 2;
			return;
		}
		psPreReg.byRegisterNum = 1;
		psPreReg.adwRegAdd[0] = 136u;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg) != 0)
		{
			text = "USB communication fail.";
			LogCmdErrorInfo(text);
			if (!m_isQuietMode)
			{
				MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			RestoreReadThread();
			return;
		}
		byte b = (byte)psPreReg.adwRegVal[0];
		byOptType = 5;
		byAccType = 5;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			num3 = 2;
			return;
		}
		ushort num4 = 128;
		psPreReg.adwRegAdd[0] = 0u;
		psPreReg.adwRegVal[0] = num4;
		if (DllDebuggerAPI.DLL_FT8215CmdPassThrough(0, ref psPreReg) != 0)
		{
			MessageBox.Show("read bflash Fail...");
			return;
		}
		if ((byte)psPreReg.adwRegVal[0] == 128 && (byte)psPreReg.adwRegVal[111] != 0)
		{
			text = $"Failed to enter I2C! len:{psPreReg.adwRegVal[0]}, snid:{psPreReg.adwRegVal[111]}.";
			MessageBox.Show(text);
			return;
		}
		uint num5 = 0u;
		adWindow.comboBox_newFunc.SelectedIndex = 0;
		adWindow.comboBox_restaEn.SelectedIndex = 0;
		adWindow.comboBox_lamd.SelectedIndex = 0;
		adWindow.comboBox_IsdFwMd.SelectedIndex = 0;
		b &= 0xE;
		m_byREfuseArr[38] = b;
		ShowChipIDInfo(m_byREfuseArr[38], m_byREfuseArr[39]);
		InitAllBtnDisplay(m_bFindDev);
		RestoreReadThread();
		if (!m_isCmdOpera)
		{
			byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
			m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd = 2;
			m_eBtnOptType = E_BtnOptType.eBtn_OPT_STOP;
			ExecuteStopOperation();
			m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd = bySpdCtrlMd;
		}
	}

	private int InitAllBtnDisplay(bool bParamEn)
	{
		bool isEnabled = bParamEn;
		btnDebug.IsEnabled = isEnabled;
		btnStartBw.IsEnabled = isEnabled;
		return 0;
	}

	private int ShowChipIDInfo(byte bySlaveId, byte byChipId)
	{
		int result = 0;
		byte b = bySlaveId;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = (byte)((b >> 1) & 0xFF);
		string text = $"0x{m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId:X2}";
		textBox_SetI2CAddr.Text = text;
		textBox_SlvID.Text = text;
		if (byChipId != 0)
		{
			string text2 = $"0x{byChipId:X2}";
			textBox_SetChipID.Text = text2;
		}
		return result;
	}

	private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.Source.GetType() == typeof(TabControl))
		{
			int selectedIndex = tabControl1.SelectedIndex;
			InitTabControl((byte)selectedIndex);
		}
		else
		{
			e.Handled = true;
		}
	}

	private int InitTabControl(byte byTabIndex)
	{
		int result = 0;
		btnDebug.Visibility = Visibility.Visible;
		btnStartBw.Visibility = Visibility.Visible;
		textBox_SlvID.IsEnabled = true;
		textBox_UID1.IsEnabled = true;
		btn_WriteID.IsEnabled = btn_ReadID.IsEnabled;
		m_byTabIndex = 0;
		if (byTabIndex == 6)
		{
			btnDebug.Visibility = Visibility.Hidden;
			btnStartBw.Visibility = Visibility.Hidden;
			textBox_SlvID.IsEnabled = false;
			textBox_UID1.IsEnabled = false;
			btn_WriteID.IsEnabled = false;
			m_byTabIndex = 5;
		}
		return result;
	}

	private int StopMotor()
	{
		int num = 0;
		bool flag = false;
		if (m_wRunningSpeed != 0)
		{
			DllDebuggerAPI.S_PreReg psPreReg = new DllDebuggerAPI.S_PreReg
			{
				adwRegAdd = new uint[384],
				adwRegVal = new uint[384],
				byRegisterNum = 2
			};
			psPreReg.adwRegAdd[0] = 2u;
			psPreReg.adwRegVal[0] = 0u;
			psPreReg.adwRegAdd[1] = 3u;
			psPreReg.adwRegVal[1] = 0u;
			byte byOptType = 1;
			byte byAccType = 2;
			if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
			{
				return 2;
			}
			SurspendReadThread();
			num = DllDebuggerAPI.DLL_FT8215CmdPassThrough(1, ref psPreReg);
			if (num != 0)
			{
				RestoreReadThread();
				return num;
			}
			RestoreReadThread();
			while (m_wRunningSpeed != 0)
			{
				Thread.Sleep(20);
				if (m_wRunningSpeed == 0)
				{
					SurspendReadThread();
					Thread.Sleep(150);
					break;
				}
			}
			Thread.Sleep(50);
			SurspendReadThread();
		}
		return num;
	}

	private int StopMotorFlow()
	{
		int num = 0;
		DllDebuggerAPI.S_ByteArrayDef pbyNandID = new DllDebuggerAPI.S_ByteArrayDef
		{
			byStatusBuf = new byte[64]
		};
		byte byOptType = 4;
		byte byAccType = 19;
		bool flag = DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType);
		uint dwStartAddr = 64u;
		string strDevSN = m_DevList.ElementAt(0).m_strDevSN;
		int num2 = 0;
		for (num2 = 0; num2 < 3; num2++)
		{
			num = DllDebuggerAPI.DLL_GPLB32DownloaderGetNandID(strDevSN, dwStartAddr, ref pbyNandID, 1u);
			LogCmdErrorInfo("scani2caddr: " + num);
			num = GetCommStatus();
			if (num != 0)
			{
				LogCmdErrorInfo("get comm status fail: " + num);
				if (!m_isCmdOpera)
				{
					MessageBox.Show("usb communication fail.", "SMF");
				}
				return num;
			}
			if (m_byCommunicationStatus == 1)
			{
				LogCmdErrorInfo("comm status fail: " + m_byCommunicationStatus);
				if (num2 != 1)
				{
					continue;
				}
				LogCmdErrorInfo("scan i2c addr at StopMotorFlow.");
				num = ScanI2CAddrDevID();
				if (num != 0)
				{
					ReEnumDevSN();
					if (!m_isCmdOpera)
					{
						MessageBox.Show("usb communication fail at stop motor flow.", "SMF Scan");
					}
					return num;
				}
				if (m_byCommunicationStatus == 1)
				{
					LogCmdErrorInfo("scan i2c addr fail again: " + m_byCommunicationStatus);
					return num;
				}
				LogCmdErrorInfo("scan i2c addr success: " + m_byCommunicationStatus);
				continue;
			}
			LogCmdErrorInfo("scan i2c addr success: " + m_byCommunicationStatus);
			break;
		}
		m_byMotorStatus = pbyNandID.byStatusBuf[0];
		num2 = 0;
		if (m_byMotorStatus == 0 || m_byMotorStatus == 7 || m_byMotorStatus == 8)
		{
			num = 0;
		}
		else
		{
			byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
			if (bySpdCtrlMd != 2)
			{
				MessageBoxResult messageBoxResult = MessageBox.Show("Please stop motor first and try again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
				return num;
			}
			if (m_eBtnOptType != E_BtnOptType.eBtn_OPT_ON_OFF_TEST)
			{
				MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to execute this operation? It will stop motor if you do", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK);
				if (messageBoxResult == MessageBoxResult.Cancel)
				{
					return num;
				}
			}
			num = 0;
			num = StopMotor();
			if (num != 0)
			{
				return num;
			}
			CancelBackWorker();
			Thread.Sleep(20);
		}
		return num;
	}

	private uint GetQFamatVal(double fVal, ushort wFormatCnt)
	{
		uint result = 0u;
		switch (wFormatCnt)
		{
		case 8:
			result = (uint)(fVal * 256.0);
			break;
		case 10:
			result = (uint)(fVal * 1024.0);
			break;
		case 11:
			result = (uint)(fVal * 2048.0);
			break;
		case 12:
			result = (uint)(fVal * 4096.0);
			break;
		case 13:
			result = (uint)(fVal * 8192.0);
			break;
		case 15:
			result = (uint)(fVal * 32768.0);
			break;
		case 16:
			result = 65535u;
			break;
		case 17:
			result = (uint)(fVal * 65535.0);
			break;
		}
		return result;
	}

	private ushort GetPowerExponent(ushort wPowerTmp)
	{
		ushort result = 0;
		if (wPowerTmp <= 2)
		{
			m_byPowerSel = 0;
			MessageBox.Show("PowerExponent err");
		}
		else if (wPowerTmp <= 4)
		{
			result = 2;
			m_byPowerSel = 0;
		}
		else if (wPowerTmp <= 16)
		{
			result = 4;
			m_byPowerSel = 1;
		}
		else if (wPowerTmp <= 64)
		{
			result = 16;
			m_byPowerSel = 2;
		}
		else if (wPowerTmp <= 256)
		{
			result = 64;
			m_byPowerSel = 3;
		}
		else if (wPowerTmp <= 1024)
		{
			result = 256;
			m_byPowerSel = 4;
		}
		else if (wPowerTmp <= 4096)
		{
			result = 1024;
			m_byPowerSel = 5;
		}
		else if (wPowerTmp <= 16384)
		{
			result = 4096;
			m_byPowerSel = 6;
		}
		else
		{
			result = 16384;
			m_byPowerSel = 7;
		}
		return result;
	}

	private uint GetIValue(float fCurr_Value, ushort wFormatCnt)
	{
		uint num = 0u;
		float num2 = fCurr_Value * m_fHW_RSHUNT * m_fHW_AMPGAIN / m_fHW_ADC_REF;
		return GetQFamatVal(num2, wFormatCnt);
	}

	private float GetAdcRefVoltage(byte byAdcIndex)
	{
		float result = 4.5f;
		switch (byAdcIndex)
		{
		case 0:
			result = 4.5f;
			break;
		case 1:
			result = 5f;
			break;
		case 2:
			result = 3f;
			break;
		case 3:
			result = 4f;
			break;
		}
		return result;
	}

	private float GetAmpGain(byte byApmIndex)
	{
		float result = 2f;
		switch (byApmIndex)
		{
		case 0:
			result = 4f;
			break;
		case 1:
			result = 8f;
			break;
		case 2:
			result = 12f;
			break;
		case 3:
			result = 16f;
			break;
		}
		return result;
	}

	private ushort GetFreqBaseIdx(ushort wFreqBaseIdx)
	{
		ushort result = 11;
		switch (wFreqBaseIdx)
		{
		case 100:
			result = 0;
			break;
		case 160:
			result = 1;
			break;
		case 200:
			result = 2;
			break;
		case 250:
			result = 3;
			break;
		case 320:
			result = 4;
			break;
		case 400:
			result = 5;
			break;
		case 500:
			result = 6;
			break;
		case 640:
			result = 7;
			break;
		case 800:
			result = 8;
			break;
		case 1000:
			result = 9;
			break;
		case 1280:
			result = 10;
			break;
		case 1600:
			result = 11;
			break;
		case 2000:
			result = 12;
			break;
		case 2560:
			result = 13;
			break;
		case 3200:
			result = 14;
			break;
		case 4000:
			result = 15;
			break;
		}
		return result;
	}

	private int CacluCurrenAndSpeedBase()
	{
		byte byVRefSel = m_sMotorParamCfg.m_sExtendCfg.byVRefSel;
		float adcRefVoltage = GetAdcRefVoltage(byVRefSel);
		float fRV = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV;
		float fMotorVoltageBase = (float)((double)(adcRefVoltage * fRV) / 1.732);
		m_fMotorVoltageBase = fMotorVoltageBase;
		byte byFanDaBeiShu = m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu;
		float ampGain = GetAmpGain(byFanDaBeiShu);
		float fCaiYangDDianZu = m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu;
		float fMotorCurrentBase = adcRefVoltage / ampGain / fCaiYangDDianZu;
		m_fMotorCurrentBase = fMotorCurrentBase;
		ushort wFreqBase = m_sMotorParamCfg.m_sFocParamCfg.wFreqBase;
		byte byJiDuiShu = m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		float fMotorSpeedBase = 60 * wFreqBase / byJiDuiShu;
		m_fMotorSpeedBase = fMotorSpeedBase;
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		if (byControlMode == 2)
		{
			textBox_MinSpeed.ToolTip = $"range:0-{(int)(m_fMotorSpeedBase / 2f):d} integer";
			textBox_MaxSpeed.ToolTip = $"range:0-{(int)m_fMotorSpeedBase:d} integer";
		}
		return 0;
	}

	private byte GetLeadAnglebyValue(float fLeadAngle)
	{
		byte b = 0;
		if (fLeadAngle == 0f)
		{
			return 0;
		}
		if (fLeadAngle == 2.82f)
		{
			return 1;
		}
		if (fLeadAngle == 5.64f)
		{
			return 2;
		}
		if (fLeadAngle == 8.47f)
		{
			return 3;
		}
		if (fLeadAngle == 11.3f)
		{
			return 4;
		}
		if (fLeadAngle == 14.1f)
		{
			return 5;
		}
		if (fLeadAngle == 16.9f)
		{
			return 6;
		}
		return 7;
	}

	private byte GetRevBrakeSpeed(byte byRevBrakeSpeed)
	{
		byte b = 0;
		return byRevBrakeSpeed;
	}

	private byte GetStaticSpeed(double isdStaTime)
	{
		byte result = 0;
		if (isdStaTime == 28.0 || isdStaTime == 100.0 || isdStaTime == 0.5)
		{
			result = 0;
		}
		else if (isdStaTime == 24.0 || isdStaTime == 64.0 || isdStaTime == 0.25)
		{
			result = 1;
		}
		else if (isdStaTime == 20.0 || isdStaTime == 32.0 || isdStaTime == 0.167)
		{
			result = 2;
		}
		else if (isdStaTime == 16.0)
		{
			result = 3;
		}
		else if (isdStaTime == 8.0)
		{
			result = 4;
		}
		else if (isdStaTime == 4.0)
		{
			result = 5;
		}
		else if (isdStaTime == 2.0)
		{
			result = 6;
		}
		else if (isdStaTime == 1.0)
		{
			result = 7;
		}
		return result;
	}

	private byte GetFocEFreqMinVal(float fFocEFreqMin)
	{
		byte b = 0;
		return (byte)Math.Round((double)fFocEFreqMin / 0.2, 0);
	}

	private byte GetLockEmfVal(float fLockEmf)
	{
		byte b = 0;
		if ((double)fLockEmf == 16.0)
		{
			return 0;
		}
		if ((double)fLockEmf == 8.0)
		{
			return 1;
		}
		if ((double)fLockEmf == 4.0)
		{
			return 2;
		}
		if ((double)fLockEmf == 2.0)
		{
			return 3;
		}
		if ((double)fLockEmf == 1.0)
		{
			return 4;
		}
		if ((double)fLockEmf == 0.5)
		{
			return 5;
		}
		if ((double)fLockEmf == 0.25)
		{
			return 6;
		}
		return 7;
	}

	private byte GetDtSelByValue(float fDtVal)
	{
		byte b = 0;
		if (fDtVal == 0.5f)
		{
			return 0;
		}
		if (fDtVal == 0.67f)
		{
			return 1;
		}
		if (fDtVal == 0.83f)
		{
			return 2;
		}
		if (fDtVal == 1f)
		{
			return 3;
		}
		if (fDtVal == 1.17f)
		{
			return 4;
		}
		if (fDtVal == 1.33f)
		{
			return 5;
		}
		if (fDtVal == 1.5f)
		{
			return 6;
		}
		return 7;
	}

	private ushort GetLockMinSpeedValue()
	{
		return 0;
	}

	private float GetMotorSpeedBase()
	{
		ushort wFreqBase = m_sMotorParamCfg.m_sFocParamCfg.wFreqBase;
		byte byJiDuiShu = m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		return 60 * wFreqBase / byJiDuiShu;
	}

	private bool CalculateParam()
	{
		byte b = m_sMotorParamCfg.m_sFocParamCfg.bySeg7En;
		byte b2 = m_sMotorParamCfg.m_sFocParamCfg.bySta7SegEn;
		byte byStaPllEn = m_sMotorParamCfg.m_sFocParamCfg.byStaPllEn;
		byte byStaTsam0En = m_sMotorParamCfg.m_sFocParamCfg.byStaTsam0En;
		byte byPllSmoEn = m_sMotorParamCfg.m_sFocParamCfg.byPllSmoEn;
		byte b3 = m_sMotorParamCfg.m_sFocParamCfg.bySMPMode;
		byte byRpdAlignEn = m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn;
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
		byte byFaultRecovTimes = m_sMotorParamCfg.m_sProtectCfg.byFaultRecovTimes;
		byte byLossPhaseEn = m_sMotorParamCfg.m_sProtectCfg.byLossPhaseEn;
		byte byLock = m_sMotorParamCfg.m_sProtectCfg.byLock0;
		byte byLock2 = m_sMotorParamCfg.m_sProtectCfg.byLock1;
		byte byLock3 = m_sMotorParamCfg.m_sProtectCfg.byLock2;
		byte byRsdEn = m_sMotorParamCfg.m_sWindHandleCfg.byRsdEn;
		byte byOcpSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel;
		byte bySlpDis = m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis;
		byte byOvpSel = m_sMotorParamCfg.m_eSpecialRegCfg.byOvpSel;
		byte byTsdSel = m_sMotorParamCfg.m_eSpecialRegCfg.byTsdSel;
		byte byDzSet = m_sMotorParamCfg.m_eSpecialRegCfg.byDzSet;
		byte byDpmDis = m_sMotorParamCfg.m_eSpecialRegCfg.byDpmDis;
		byte byOvpDis = m_sMotorParamCfg.m_eSpecialRegCfg.byOvpDis;
		byte byOcpDis = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis;
		byte byTsdDis = m_sMotorParamCfg.m_eSpecialRegCfg.byTsdDis;
		byte byFocDis = m_sMotorParamCfg.m_eSpecialRegCfg.byFocDis;
		byte byBootMod = m_sMotorParamCfg.m_eSpecialRegCfg.byBootMod;
		byte byUvloDis = m_sMotorParamCfg.m_eSpecialRegCfg.byUvloDis;
		byte byNewFunc = m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc;
		byte byLaMd = m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd;
		byte byIsdFwMd = m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd;
		byte byRestaEn = m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn;
		byte byIdleBrakeEn = m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn;
		byte byIsdBrakeEn = m_sMotorParamCfg.m_sWindHandleCfg.byIsdBrakeEn;
		byte byTSMIN = m_sMotorParamCfg.m_sFocParamCfg.byTSMIN;
		ushort num = (ushort)m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel switch
		{
			0 => 25000, 
			1 => 42000, 
			2 => 16000, 
			_ => 20000, 
		};
		byte byRvsAlignEn = m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn;
		byte byRvsCutSel = m_sMotorParamCfg.m_eSpecialRegCfg.byRvsCutSel;
		byte byNewSingle = m_sMotorParamCfg.m_eSpecialRegCfg.byNewSingle;
		byte byDigTst = m_sMotorParamCfg.m_eSpecialRegCfg.byDigTst;
		float fRV = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV;
		byte byCurveEnSpd = m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd;
		byte byFgMul = m_sMotorParamCfg.m_sOhterCfg.byFgMul;
		byte byFgDiv = m_sMotorParamCfg.m_sOhterCfg.byFgDiv;
		byte byFgIdleSet = m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet;
		byte byFgSet = m_sMotorParamCfg.m_sOhterCfg.byFgSet;
		byte byFgRd = m_sMotorParamCfg.m_sOhterCfg.byFgRd;
		byte byVRefSel = m_sMotorParamCfg.m_sExtendCfg.byVRefSel;
		float adcRefVoltage = GetAdcRefVoltage(byVRefSel);
		float num2 = (float)((double)(adcRefVoltage * fRV) / 1.732);
		byte byFanDaBeiShu = m_sMotorParamCfg.m_sMotorParamHwCfg.byFanDaBeiShu;
		float ampGain = GetAmpGain(byFanDaBeiShu);
		float fCaiYangDDianZu = m_sMotorParamCfg.m_sMotorParamHwCfg.fCaiYangDDianZu;
		float num3 = adcRefVoltage / ampGain / fCaiYangDDianZu;
		m_fHW_RSHUNT = fCaiYangDDianZu;
		m_fHW_AMPGAIN = ampGain;
		m_fHW_ADC_REF = adcRefVoltage;
		m_byEfuseArr[0] = (byte)((byOvpSel << 6) + (byDpmDis << 3) + (byTsdDis & 1));
		m_byEfuseArr[1] = (byte)((byFocDis << 6) + (byBootMod << 5));
		byte b4 = 2;
		string text = textBox_SlvID.Text;
		byte bySlaveId = Convert.ToByte(text, 16);
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId = bySlaveId;
		bySlaveId = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySlaveId;
		m_byEfuseArr[b4] = (byte)((byRestaEn << 7) + (byIsdFwMd << 6) + (byLaMd << 5) + ((bySlaveId & 7) << 1) + (byUvloDis & 1));
		byte byStaSin = m_sMotorParamCfg.m_sFocParamCfg.byStaSin;
		byte byAtanEn = m_sMotorParamCfg.m_sFocParamCfg.byAtanEn;
		byte bySpeedVSet = m_sMotorParamCfg.m_sExtendCfg.bySpeedVSet;
		byte byIrefDis = m_sMotorParamCfg.m_eSpecialRegCfg.byIrefDis;
		m_byEfuseArr[b4 + 1] = (byte)((byStaSin << 7) + (byAtanEn << 6) + ((bySpeedVSet & 3) << 4) + ((byFanDaBeiShu & 3) << 2) + (bySlpDis << 1) + byIrefDis);
		byte byCarrierSel = m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel;
		byte byRsdMode = m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode;
		m_byEfuseArr[b4 + 2] = (byte)((byCarrierSel << 6) + ((byRsdMode & 3) << 4) + ((byControlMode & 3) << 2) + (bySpdCtrlMd & 3));
		if (byNewSingle == 1)
		{
			b3 = 2;
		}
		switch (b)
		{
		case 0:
			b2 = 0;
			b = 0;
			break;
		case 1:
			b2 = 0;
			b = 1;
			break;
		case 2:
			b2 = 1;
			b = 0;
			break;
		case 3:
			b2 = 1;
			b = 1;
			break;
		}
		m_byEfuseArr[b4 + 3] = (byte)((b2 << 7) + (byStaPllEn << 6) + (byIsdBrakeEn << 5) + (byIdleBrakeEn << 4) + (b << 3) + (byPllSmoEn << 2) + (b3 & 3));
		float fLeadAngleSet = m_sMotorParamCfg.m_sFocParamCfg.fLeadAngleSet;
		byte leadAnglebyValue = GetLeadAnglebyValue(fLeadAngleSet);
		m_byEfuseArr[b4 + 4] = (byte)(((leadAnglebyValue & 3) << 6) + ((byFaultRecovTimes & 3) << 4) + (byLossPhaseEn << 3) + (byLock3 << 2) + (byLock2 << 1) + byLock);
		byte byFgRdMd = m_sMotorParamCfg.m_sOhterCfg.byFgRdMd;
		byte byFgIdleSet2 = m_sMotorParamCfg.m_sOhterCfg.byFgIdleSet;
		m_byEfuseArr[b4 + 5] = (byte)(((byFgDiv & 3) << 6) + ((byFgMul & 3) << 4) + (byFgIdleSet2 << 3) + (byFgRdMd << 1) + ((leadAnglebyValue & 7) >> 2));
		byte byPwmRevEn = m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn;
		byte byPwmOffDis = m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis;
		byte byQuickSta = m_sMotorParamCfg.m_sOhterCfg.byQuickSta;
		byte byStaIncTime = m_sMotorParamCfg.m_sRunCfg.byStaIncTime;
		byte byFgTimeSel = m_sMotorParamCfg.m_sExtendCfg.byFgTimeSel;
		m_byEfuseArr[b4 + 6] = (byte)((byCurveEnSpd << 7) + (byPwmRevEn << 6) + (byPwmOffDis << 5) + (byQuickSta << 4) + (byStaIncTime << 2) + (byFgTimeSel & 3));
		byte byForwardStaSpeed = m_sMotorParamCfg.m_sWindHandleCfg.byForwardStaSpeed;
		byte byRevBrakeSpeed = m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed;
		byte staticSpeed = GetStaticSpeed(m_sMotorParamCfg.m_sWindHandleCfg.dStaticSpeed);
		byte byIsdMd2Time = m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time;
		m_byEfuseArr[b4 + 7] = (byte)(((byForwardStaSpeed & 3) << 6) + ((byIsdMd2Time & 7) << 3) + (staticSpeed & 7));
		byte b5 = 0;
		if (radioBtn_HallStaOpEn.IsChecked == true || radioBtn_HallSvMdEn.IsChecked == true)
		{
			byte byStartDuty = m_sMotorParamCfg.m_sStartCfg.byStartDuty;
			double value = Math.Round((double)(int)byStartDuty / 50.0 * 31.0, 0);
			int num4 = Convert.ToInt16(value);
			b5 = (byte)(num4 & 0x1F);
		}
		else
		{
			float fStartCurrent = m_sMotorParamCfg.m_sStartCfg.fStartCurrent;
			float num5 = fStartCurrent * fCaiYangDDianZu * ampGain / (adcRefVoltage / 2f);
			uint qFamatVal = GetQFamatVal(num5, 15);
			uint iValue = GetIValue(fStartCurrent, 15);
			b5 = (byte)((iValue >> 9) & 0x1F);
		}
		if (b5 > 31)
		{
			b5 = 31;
		}
		byte byAlignDuty = m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty;
		byte byAlignTime = m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime;
		byte byRPD = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10;
		byte byRPD2 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11;
		byte byRPD3 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD12;
		byte byRPD4 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD20;
		byte byRPD5 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21;
		byte b6 = 0;
		if (m_byPreRpdAlignSel == 0 && byRpdAlignEn == 0)
		{
			b6 = 0;
		}
		else if (m_byPreRpdAlignSel != 0 && byRpdAlignEn == 0)
		{
			b6 = m_byPreRpdAlignSel;
		}
		else
		{
			b6 = byRpdAlignEn;
		}
		m_byEfuseArr[b4 + 8] = (byte)(((b5 & 0xF) << 4) + (byRevBrakeSpeed & 0xF));
		byte byFocEfreqAccx = m_sMotorParamCfg.m_sStartCfg.byFocEfreqAccx;
		byte byStbInc = m_sMotorParamCfg.m_sRunCfg.byStbInc;
		byte byStaInc = m_sMotorParamCfg.m_sRunCfg.byStaInc;
		m_byEfuseArr[b4 + 9] = (byte)(((byFocEfreqAccx & 1) << 7) + ((byStbInc & 7) << 4) + ((byStaInc & 7) << 1) + ((b5 & 0x10) >> 4));
		float fFocEfreqMinx = m_sMotorParamCfg.m_sStartCfg.fFocEfreqMinx;
		byte focEFreqMinVal = GetFocEFreqMinVal(fFocEfreqMinx);
		m_byEfuseArr[b4 + 10] = (byte)(((focEFreqMinVal & 0xF) << 4) + ((byFocEfreqAccx >> 1) & 0xF));
		ushort wFreqBase = m_sMotorParamCfg.m_sFocParamCfg.wFreqBase;
		byte b7 = (byte)GetFreqBaseIdx(wFreqBase);
		byte byCloSpeed = m_sMotorParamCfg.m_sStartCfg.byCloSpeed;
		byte byFocEfreqHold = m_sMotorParamCfg.m_sStartCfg.byFocEfreqHold;
		m_byEfuseArr[b4 + 11] = (byte)(((b7 & 0xF) << 4) + ((byCloSpeed & 3) << 2) + (byFocEfreqHold & 3));
		byte byJiDuiShu = m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		float num6 = (m_fMotorSpeedBase = 60 * wFreqBase / byJiDuiShu);
		float num7 = (float)m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed / m_fMotorSpeedBase;
		uint qFamatVal2 = GetQFamatVal(num7, 15);
		byte b8 = (byte)((qFamatVal2 >> 9) & 0x3F);
		if (b8 >= 15)
		{
			b8 = 15;
		}
		float num8 = (float)m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed / m_fMotorSpeedBase;
		uint qFamatVal3 = GetQFamatVal(num8, 15);
		byte b9 = (byte)((qFamatVal3 >> 8) & 0x7F);
		if (b9 >= 127)
		{
			b9 = 127;
		}
		byte byHallMd = m_sMotorParamCfg.m_sHallParamCfg.byHallMd;
		byHallMd &= 7;
		byte byAddr_ = m_sMotorParamCfg.m_sExtendCfg.byAddr_94;
		byte byHallFgMd = m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd;
		byte byHallForMd = m_sMotorParamCfg.m_sHallParamCfg.byHallForMd;
		byte byHallStaMd = m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd;
		byte byHallCorMd = m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd;
		byte hallLockSelByIdx = m_clsDlgItemSet45.GetHallLockSelByIdx(m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel);
		if (byHallMd == 0)
		{
			m_byEfuseArr[b4 + 12] = (byte)((((b9 >> 2) & 0xF) << 4) + (b8 & 0xF));
		}
		else
		{
			m_byEfuseArr[b4 + 12] = (byte)(((byHallFgMd & 3) << 6) + ((byHallForMd & 1) << 5) + ((byHallStaMd & 1) << 4) + ((byHallCorMd & 1) << 3) + (hallLockSelByIdx & 7));
		}
		float fLpNorhCurrent = m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent;
		uint iValue2 = GetIValue(fLpNorhCurrent, 15);
		byte b10 = 0;
		b10 = (byte)((iValue2 >= 8192) ? 15 : ((iValue2 < 1024) ? ((byte)((iValue2 >> 7) & 7)) : ((byte)(8 + (byte)((iValue2 >> 10) & 7)))));
		uint num9 = iValue2;
		byte byLockEmfThd = m_sMotorParamCfg.m_sProtectCfg.byLockEmfThd;
		float fLockEmf = m_sMotorParamCfg.m_sProtectCfg.fLockEmf;
		byte lockEmfVal = GetLockEmfVal(fLockEmf);
		byte byHallOvMd = m_sMotorParamCfg.m_eSpecialRegCfg.byHallOvMd;
		byte byHallPu = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu;
		byte byHallPllEn = m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn;
		byte byHallLockSwi = m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi;
		if (byHallMd == 0)
		{
			m_byEfuseArr[b4 + 13] = (byte)((b10 << 4) + (((b9 >> 6) & 1) << 3) + ((b9 & 3) << 1) + (lockEmfVal >> 2));
		}
		else
		{
			m_byEfuseArr[b4 + 13] = (byte)((b10 << 4) + ((byHallOvMd & 1) << 3) + ((byHallPu & 1) << 2) + ((byHallPllEn & 1) << 1) + (byHallLockSwi & 1));
		}
		byte bySpdBw = m_sMotorParamCfg.m_sFocParamCfg.bySpdBw;
		byte byEbwMode = m_sMotorParamCfg.m_sFocParamCfg.byEbwMode;
		byte byEkNum = m_sMotorParamCfg.m_sFocParamCfg.byEkNum;
		byte byHallLockExt = m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt;
		byte byHallLttEn = m_sMotorParamCfg.m_eSpecialRegCfg.byHallLttEn;
		if (byHallMd == 0)
		{
			m_byEfuseArr[b4 + 14] = (byte)((bySpdBw << 6) + (byEbwMode << 4) + (byEkNum << 2) + (lockEmfVal & 3));
		}
		else
		{
			m_byEfuseArr[b4 + 14] = (byte)((bySpdBw << 6) + (byEbwMode << 4) + (byEkNum << 2) + ((byHallLockExt & 1) << 1) + (byHallLttEn & 1));
		}
		byte byMfpEn = m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn;
		byte byHallSvMd = m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd;
		uint num10 = 0u;
		float fDianGan = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianGan;
		float fDianZu = m_sMotorParamCfg.m_sMotorParamHwCfg.fDianZu;
		float num11 = (float)(1.0 / (double)(int)num);
		if (byMfpEn == 0)
		{
			float num12 = fDianGan / (fDianGan + fDianZu * num11);
			num10 = GetQFamatVal(num12, 15);
		}
		else
		{
			float num12 = (float)(0.024 * (double)(fDianGan / num11) * (double)(num3 / num2));
			num10 = GetQFamatVal(num12, 13);
		}
		if (num10 < 16)
		{
			num10 = 16u;
		}
		else if (num10 > 32767)
		{
			num10 = 32767u;
		}
		uint num13 = 0u;
		if (byMfpEn == 0)
		{
			float num14 = num11 / (fDianGan + fDianZu * num11) * (num2 / num3);
			num13 = GetQFamatVal(num14, 13);
		}
		else
		{
			float num14 = fDianZu * num3 / num2;
			num13 = GetQFamatVal(num14, 11);
		}
		if (num13 < 16)
		{
			num13 = 16u;
		}
		else if (num13 > 32767)
		{
			num13 = 32767u;
		}
		float fFanDianDongShi = m_sMotorParamCfg.m_sMotorParamHwCfg.fFanDianDongShi;
		float num15 = (float)((double)(num6 * fFanDianDongShi) / 1000.0);
		uint num16 = 0u;
		if (byMfpEn == 0)
		{
			float num17 = num11 / (fDianGan + fDianZu * num11) * (num15 / num3);
			num16 = GetQFamatVal(num17, 15);
		}
		else
		{
			float fMfpEk = m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3;
			float num17 = fMfpEk;
			num16 = GetQFamatVal(num17, 8);
		}
		if (num16 < 16)
		{
			num16 = 16u;
		}
		else if (num16 > 32767)
		{
			num16 = 32767u;
		}
		byte bySampleTime = m_sMotorParamCfg.m_sFocParamCfg.bySampleTime;
		byte b11 = 0;
		b11 = ((bySpdCtrlMd != 3) ? m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhys : m_sMotorParamCfg.m_sRunCfg.byPwmOnoffhysClk);
		byte byRdsPu = m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu;
		byte byHallLA = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0;
		byte byHallLA2 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125;
		byte byHallLA3 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250;
		byte byHallLA4 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375;
		if (byHallSvMd == 1)
		{
			m_byEfuseArr[b4 + 15] = (byte)((b11 << 4) + ((bySampleTime & 7) << 1) + byRdsPu);
			m_byEfuseArr[b4 + 16] = (byte)(((byHallLA2 & 0xF) << 4) + (byHallLA & 0xF));
			m_byEfuseArr[b4 + 17] = (byte)(((byHallLA4 & 0xF) << 4) + (byHallLA3 & 0xF));
		}
		else if (byMfpEn == 0)
		{
			m_byEfuseArr[b4 + 15] = (byte)((((num10 >> 8) & 7) << 5) + (b11 << 4) + ((bySampleTime & 7) << 1) + byRdsPu);
			m_byEfuseArr[b4 + 16] = (byte)((((num13 >> 6) & 0xF) << 4) + ((num10 >> 11) & 0xF));
			m_byEfuseArr[b4 + 17] = (byte)((((num16 >> 6) & 7) << 5) + ((num13 >> 10) & 0x1F));
		}
		else
		{
			m_byEfuseArr[b4 + 15] = (byte)((((num10 >> 8) & 7) << 5) + (b11 << 4) + ((bySampleTime & 7) << 1) + byRdsPu);
			m_byEfuseArr[b4 + 16] = (byte)((((num13 >> 6) & 0xF) << 4) + ((num10 >> 11) & 0xF));
			m_byEfuseArr[b4 + 17] = (byte)((((num16 >> 6) & 7) << 5) + ((num13 >> 10) & 0x1F));
		}
		float fDqKp = m_sMotorParamCfg.m_sRunCfg.fDqKp;
		uint qFamatVal4 = GetQFamatVal(fDqKp, 12);
		byte b12 = 0;
		b12 = (byte)((qFamatVal4 > 64) ? ((qFamatVal4 < 1024) ? ((byte)((qFamatVal4 >> 6) & 0xF)) : ((qFamatVal4 == 1024) ? 16 : ((qFamatVal4 >= 16384) ? 31 : ((byte)(16 + ((qFamatVal4 >> 10) & 0xF)))))) : 0);
		float fDqKi = m_sMotorParamCfg.m_sRunCfg.fDqKi;
		uint qFamatVal5 = GetQFamatVal(fDqKi, 15);
		byte b13 = 0;
		b13 = (byte)((qFamatVal5 > 1) ? ((qFamatVal5 < 16) ? ((byte)((qFamatVal5 >> 1) & 7)) : ((qFamatVal5 < 128) ? ((byte)(8 + ((qFamatVal5 >> 4) & 7))) : ((qFamatVal5 < 1024) ? ((byte)(16 + ((qFamatVal5 >> 7) & 7))) : ((qFamatVal5 >= 8192) ? 31 : ((byte)(24 + ((qFamatVal5 >> 10) & 7))))))) : 0);
		byte byHallLimSel = m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel;
		byte byHallRsv = m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1;
		byte byHallLA5 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500;
		if (byHallSvMd == 1)
		{
			m_byEfuseArr[b4 + 18] = (byte)(((b12 & 3) << 6) + (byHallRsv << 5) + (byHallLimSel << 4) + (byHallLA5 & 0xF));
		}
		else
		{
			m_byEfuseArr[b4 + 18] = (byte)(((b12 & 3) << 6) + ((num16 >> 9) & 0x3F));
		}
		m_byEfuseArr[b4 + 19] = (byte)(((b13 & 0x1F) << 3) + ((b12 >> 2) & 7));
		float fWKp = m_sMotorParamCfg.m_sRunCfg.fWKp;
		uint qFamatVal6 = GetQFamatVal(fWKp, 12);
		byte b14 = 0;
		float fWKi = m_sMotorParamCfg.m_sRunCfg.fWKi;
		uint qFamatVal7 = GetQFamatVal(fWKi, 15);
		byte b15 = 0;
		b14 = (byte)((qFamatVal6 > 64) ? ((qFamatVal6 < 1024) ? ((byte)((qFamatVal6 >> 6) & 0xF)) : ((qFamatVal6 == 1024) ? 16 : ((qFamatVal6 >= 16384) ? 31 : ((byte)(16 + ((qFamatVal6 >> 10) & 0xF)))))) : 0);
		b15 = (byte)((qFamatVal7 > 1) ? ((qFamatVal7 < 16) ? ((byte)((qFamatVal7 >> 1) & 7)) : ((qFamatVal7 < 128) ? ((byte)(8 + ((qFamatVal7 >> 4) & 7))) : ((qFamatVal7 < 1024) ? ((byte)(16 + ((qFamatVal7 >> 7) & 7))) : ((qFamatVal7 >= 8192) ? 31 : ((byte)(24 + ((qFamatVal7 >> 10) & 7))))))) : 0);
		m_byEfuseArr[b4 + 20] = (byte)(((b15 & 7) << 5) + (b14 & 0x1F));
		byte byWoutMax = m_sMotorParamCfg.m_sRunCfg.byWoutMax;
		m_byEfuseArr[b4 + 21] = (byte)(((byWoutMax & 0x3F) << 2) + ((b15 >> 3) & 3));
		uint num18 = m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed;
		if (num18 >= (uint)num6)
		{
			num18 = (uint)(num6 - 1f);
		}
		byte b16 = (byte)(256f * ((float)num18 / num6));
		m_byEfuseArr[b4 + 22] = b16;
		float fAtoBw = m_sMotorParamCfg.m_sFocParamCfg.fAtoBw;
		uint num19 = (uint)(43751.073828125 * (double)fAtoBw / (double)(int)wFreqBase);
		byte b17 = 0;
		b17 = (byte)((num19 < 1) ? 1 : ((num19 < 64) ? ((byte)(num19 & 0x3F)) : ((num19 < 512) ? ((byte)(64 + ((num19 >> 3) & 0x3F))) : ((num19 < 4096) ? ((byte)(128 + ((num19 >> 6) & 0x3F))) : ((num19 >= 32768) ? byte.MaxValue : ((byte)(192 + ((num19 >> 9) & 0x3F))))))));
		float num20 = 6.283185f * fAtoBw * fAtoBw * num11 / (float)(int)wFreqBase;
		uint qFamatVal8 = GetQFamatVal(num20, 15);
		byte b18 = 0;
		b18 = (byte)((qFamatVal8 < 1) ? 1 : ((qFamatVal8 < 64) ? ((byte)(qFamatVal8 & 0x3F)) : ((qFamatVal8 < 512) ? ((byte)(64 + ((qFamatVal8 >> 3) & 0x3F))) : ((qFamatVal8 < 4096) ? ((byte)(128 + ((qFamatVal8 >> 6) & 0x3F))) : ((qFamatVal8 >= 32768) ? byte.MaxValue : ((byte)(192 + ((qFamatVal8 >> 9) & 0x3F))))))));
		m_byEfuseArr[b4 + 23] = b17;
		m_byEfuseArr[b4 + 24] = b18;
		ushort wEbw = m_sMotorParamCfg.m_sFocParamCfg.wEbw;
		float num21 = (float)((10.68141450881958 * (double)(int)wEbw * (double)fDianGan - (double)fDianZu) * (double)num3 / (double)num2);
		uint qFamatVal9 = GetQFamatVal(num21, 11);
		float num22 = 6.283185f * (float)(int)wEbw * 6.283185f * (float)(int)wEbw * fDianGan * num11 * num3 / num2;
		uint qFamatVal10 = GetQFamatVal(num22, 11);
		byte b19 = 0;
		byte b20 = 0;
		b19 = (byte)((qFamatVal9 < 1) ? 1 : ((qFamatVal9 < 64) ? ((byte)(qFamatVal9 & 0x3F)) : ((qFamatVal9 < 512) ? ((byte)(64 + ((qFamatVal9 >> 3) & 0x3F))) : ((qFamatVal9 < 4096) ? ((byte)(128 + ((qFamatVal9 >> 6) & 0x3F))) : ((qFamatVal9 >= 32768) ? byte.MaxValue : ((byte)(192 + ((qFamatVal9 >> 9) & 0x3F))))))));
		b20 = (byte)((qFamatVal10 < 1) ? 1 : ((qFamatVal10 < 64) ? ((byte)(qFamatVal10 & 0x3F)) : ((qFamatVal10 < 512) ? ((byte)(64 + ((qFamatVal10 >> 3) & 0x3F))) : ((qFamatVal10 < 4096) ? ((byte)(128 + ((qFamatVal10 >> 6) & 0x3F))) : ((qFamatVal10 >= 32768) ? byte.MaxValue : ((byte)(192 + ((qFamatVal10 >> 9) & 0x3F))))))));
		byte byHallLA6 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625;
		byte byHallLA7 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750;
		byte byHallLA8 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875;
		byte byHallLA9 = m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999;
		if (byHallSvMd == 1)
		{
			m_byEfuseArr[b4 + 25] = (byte)(((byHallLA7 & 0xF) << 4) + (byHallLA6 & 0xF));
			m_byEfuseArr[b4 + 26] = (byte)(((byHallLA9 & 0xF) << 4) + (byHallLA8 & 0xF));
		}
		else if (byMfpEn == 0)
		{
			m_byEfuseArr[b4 + 25] = b19;
			m_byEfuseArr[b4 + 26] = b20;
		}
		else
		{
			m_byEfuseArr[b4 + 25] = (byte)(num10 & 0xFF);
			m_byEfuseArr[b4 + 26] = (byte)((((num16 >> 1) & 3) << 6) + (num13 & 0x3F));
		}
		byte byX_ON = m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON;
		byte byXON = m_sMotorParamCfg.m_sTargetSpdCurve.byXON0;
		uint dwMinSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed;
		uint dwMaxSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed;
		float fMinCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent;
		uint iValue3 = GetIValue(fMinCurrent, 17);
		float fMaxCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent;
		uint iValue4 = GetIValue(fMaxCurrent, 17);
		float num23 = 0f;
		float num24 = 0f;
		float num25 = 0f;
		ushort num26 = 0;
		ushort num27 = 0;
		float num28 = 0f;
		float num29 = 0f;
		if (byControlMode == 3)
		{
			num23 = m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower;
			num24 = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower;
			num29 = adcRefVoltage * fRV;
			num25 = num3 * 2f * num29;
			num26 = (ushort)(num25 / num24);
			num27 = GetPowerExponent(num26);
			num28 = num25 / (float)(int)num27;
		}
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Curr_Base = num3.ToString();
		m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Vol_Base2 = num29.ToString();
		m_sMotorParamCfg.m_sShowBASECfg.strMotor_Power_Base = num28.ToString();
		textblock_BoardCurrBase.Text = m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Curr_Base;
		textblock_BoardVolBase2.Text = m_sMotorParamCfg.m_sShowBASECfg.strHW_Board_Vol_Base2;
		textblock_MotorPowerBase.Text = m_sMotorParamCfg.m_sShowBASECfg.strMotor_Power_Base;
		byte byYMaxSel = m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel;
		byte byPwmX98En = m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En;
		byte byY_ON = m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON;
		byte byY_ = m_sMotorParamCfg.m_sTargetSpdCurve.byY_25;
		byte byY_2 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_50;
		byte byY_3 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_75;
		m_byEfuseArr[b4 + 27] = (byte)(((byX_ON & 3) << 6) + ((byYMaxSel & 7) << 3) + (byPwmX98En << 2) + (byRvsAlignEn << 1) + byRvsCutSel);
		ushort num30 = 0;
		ushort num31 = 0;
		if (byControlMode == 0)
		{
			m_byEfuseArr[b4 + 28] = (byte)(((byY_ON & 0xF) << 4) + ((byX_ON >> 2) & 0xF));
			m_byEfuseArr[b4 + 29] = (byte)(((byY_ & 0x1F) << 3) + ((byY_ON >> 4) & 7));
			m_byEfuseArr[b4 + 30] = (byte)(((byY_2 & 0x3F) << 2) + ((byY_ >> 5) & 3));
			m_byEfuseArr[b4 + 31] = (byte)(((byY_3 & 0x7F) << 1) + ((byY_2 >> 6) & 1));
		}
		else if (byControlMode == 2 || byControlMode == 3)
		{
			if (byControlMode == 2)
			{
				num30 = (ushort)(4095f * ((float)dwMaxSpeed / num6));
				if (num30 > 4095)
				{
					num30 = 4095;
				}
				num31 = (ushort)(4095f * ((float)dwMinSpeed / num6));
				if (num31 > 2047)
				{
					num31 = 2047;
				}
			}
			else
			{
				num30 = (ushort)(4095f * (num24 / num28));
				if (num30 > 4095)
				{
					num30 = 4095;
				}
				num31 = (ushort)(4095f * (num23 / num28));
				if (num31 > 2047)
				{
					num31 = 2047;
				}
			}
			byte byStartClk = m_sMotorParamCfg.m_sOhterCfg.byStartClk;
			byte b21 = 10;
			if (bySpdCtrlMd == 3)
			{
				m_byEfuseArr[b4 + 28] = (byte)(((byStartClk & 3) << 6) + (b21 & 0x3F));
			}
			else
			{
				m_byEfuseArr[b4 + 28] = (byte)(((byXON & 0xF) << 4) + ((byX_ON >> 2) & 0xF));
			}
			m_byEfuseArr[b4 + 29] = (byte)(((num31 & 0x7F) << 1) + (byte)((byXON >> 4) & 1));
			m_byEfuseArr[b4 + 30] = (byte)(((num30 & 0xF) << 4) + (byte)((num31 >> 7) & 0xF));
			m_byEfuseArr[b4 + 31] = (byte)((num30 >> 4) & 0xFF);
		}
		else if (byControlMode == 1)
		{
			num30 = (ushort)(4095f * (float)((double)iValue4 / 32767.0));
			if (num30 > 4095)
			{
				num30 = 4095;
			}
			num31 = (ushort)(4094f * (float)((double)iValue3 / 32767.0));
			if (num31 > 2047)
			{
				num31 = 2047;
			}
			m_byEfuseArr[b4 + 28] = (byte)(((byXON & 0xF) << 4) + ((byX_ON >> 2) & 0xF));
			m_byEfuseArr[b4 + 29] = (byte)(((num31 & 0x7F) << 1) + (byte)((byXON >> 4) & 1));
			m_byEfuseArr[b4 + 30] = (byte)(((num30 & 0xF) << 4) + (byte)((num31 >> 7) & 0xF));
			m_byEfuseArr[b4 + 31] = (byte)((num30 >> 4) & 0xFF);
		}
		byte byMcOffSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel;
		byte b22 = m_sMotorParamCfg.m_sMotorParamHwCfg.byVdcAdj;
		byte byOvpThr = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr;
		if (m_blIsChangeVdc)
		{
			b22 = ((b22 == 0) ? ((byte)1) : ((byte)0));
		}
		m_byEfuseArr[b4 + 32] = (byte)(((byMcOffSel & 3) << 6) + ((b22 & 1) << 5) + (byOvpThr & 0x1F));
		byte byIdleTime = m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime;
		byte dtSelByValue = GetDtSelByValue(m_sMotorParamCfg.m_sFocParamCfg.fDTSEL);
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrAmpSel = m_byPowerSel;
		byte byPwrAmpSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrAmpSel;
		m_byEfuseArr[b4 + 33] = (byte)(((byIdleTime & 3) << 6) + ((dtSelByValue & 7) << 3) + (byPwrAmpSel & 7));
		byte byITripEn = m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn;
		byte byLimitITrip = m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip;
		m_byEfuseArr[b4 + 34] = (byte)((byITripEn << 7) + ((byMfpEn & 1) << 6) + (byLimitITrip & 0x3F));
		byte byIsdFltSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel;
		byte byITripRe = m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe;
		byte byRSD = m_sMotorParamCfg.m_sWindHandleCfg.byRSD230;
		m_byEfuseArr[b4 + 35] = (byte)((byTSMIN << 4) + ((byIsdFltSel & 3) << 2) + ((byITripRe & 1) << 1) + (byRSD & 1));
		byte byKSILDE = m_sMotorParamCfg.m_sFocParamCfg.byKSILDE;
		byte byUvpThr = m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr;
		if (byMfpEn == 0)
		{
			m_byEfuseArr[b4 + 36] = (byte)((byKSILDE << 5) + (byUvpThr & 0x1F));
		}
		else
		{
			m_byEfuseArr[b4 + 36] = (byte)((((num16 >> 3) & 7) << 5) + (byUvpThr & 0x1F));
		}
		byte byPwrFltSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel;
		if ((byHallMd & 7) == 0)
		{
			if (byRpdAlignEn == 2 || byRpdAlignEn == 3)
			{
				byte byRPD6 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD10;
				byte byRPD7 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD11;
				m_byEfuseArr[b4 + 37] = (byte)(((byRPD6 & 1) << 7) + ((byRPD7 & 1) << 6) + ((byRpdAlignEn & 7) << 3) + (byHallMd & 7));
			}
			else if (byRpdAlignEn == 4)
			{
				byte byRPD8 = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD21;
				m_byEfuseArr[b4 + 37] = (byte)(((byRPD8 & 3) << 6) + ((byRpdAlignEn & 7) << 3) + (byHallMd & 7));
			}
			else
			{
				m_byEfuseArr[b4 + 37] = (byte)(((byAlignTime & 3) << 6) + ((byRpdAlignEn & 7) << 3) + (byHallMd & 7));
			}
			if (byRpdAlignEn == 2 || byRpdAlignEn == 3 || byRpdAlignEn == 4)
			{
				byte byRPD123_ = m_sMotorParamCfg.m_sIpdAlignCfg.byRPD123_2;
				m_byEfuseArr[b4 + 38] = (byte)((byPwrFltSel << 6) + (byRPD123_ & 0x3F));
			}
			else
			{
				m_byEfuseArr[b4 + 38] = (byte)((byPwrFltSel << 6) + ((byAlignDuty & 0xF) << 2) + ((byAlignTime >> 2) & 3));
			}
		}
		else
		{
			byte byHallComMd = m_sMotorParamCfg.m_sHallParamCfg.byHallComMd;
			byte byHall2RevEn = m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn;
			byte byHall1RevEn = m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn;
			byte byHall0RevEn = m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn;
			m_byEfuseArr[b4 + 37] = (byte)(((byHallComMd & 3) << 6) + ((byHall2RevEn & 1) << 5) + ((byHall1RevEn & 1) << 4) + ((byHall0RevEn & 1) << 3) + (byHallMd & 7));
			byte byHallStaOp = m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp;
			byte byHallSwiMd = m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd;
			byte byHallCalMd = m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd;
			byte byHallSvMd2 = m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd;
			m_byEfuseArr[b4 + 38] = (byte)((byPwrFltSel << 6) + ((byHallStaOp & 1) << 5) + ((byHallSwiMd & 7) << 2) + ((byHallCalMd & 1) << 1) + (byHallSvMd2 & 1));
		}
		byte byFlocLdVoltKp = m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp;
		byte byFlocLqVoltKp = m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp;
		byte byFocFluxVoltKp = m_sMotorParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp;
		byte byDrvDisMod = m_sMotorParamCfg.m_eSpecialRegCfg.byDrvDisMod;
		if (byHallSvMd == 1)
		{
			byte byHaDu = m_sMotorParamCfg.m_sHallParamCfg.byHaDu;
			byte byHaTi = m_sMotorParamCfg.m_sHallParamCfg.byHaTi;
			byte byHallAlignSpeed = m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed;
			m_byEfuseArr[b4 + 39] = (byte)(((byHaTi & 7) << 5) + (byHaDu & 0x1F));
			m_byEfuseArr[b4 + 40] = (byte)(((byDrvDisMod & 1) << 7) + ((byHallAlignSpeed & 0x1F) << 2) + ((byHaTi >> 3) & 3));
		}
		else
		{
			byte b23 = byFlocLqVoltKp;
			byte b24 = byFlocLdVoltKp;
			m_byEfuseArr[b4 + 39] = (byte)(((b23 & 7) << 5) + (b24 & 0x1F));
			byte b25 = byFocFluxVoltKp;
			m_byEfuseArr[b4 + 40] = (byte)(((byDrvDisMod & 1) << 7) + ((b25 & 0x1F) << 2) + ((b23 >> 3) & 3));
		}
		m_byEfuseArr[b4 + 41] = (byte)m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0;
		m_byEfuseArr[b4 + 42] = (byte)m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1;
		m_byEfuseArr[b4 + 43] = (byte)m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2;
		byte byTheCompSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel;
		byte b26 = (byte)m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid;
		m_byEfuseArr[b4 + 44] = (byte)(((byTheCompSel & 0xF) << 4) + (b26 & 0xF));
		byte b27 = (byte)(m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel & 3);
		byte b28 = (byte)((m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel & 0xC) >> 2);
		byte byCmp0Hys = m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys;
		byte byCmp0CkSel = m_sMotorParamCfg.m_eSpecialRegCfg.byCmp0CkSel;
		m_byEfuseArr[b4 + 45] = (byte)(((b27 & 3) << 6) + ((b28 & 3) << 4) + ((byCmp0Hys & 3) << 2) + (byCmp0CkSel & 3));
		m_byEfuseArr[b4 + 46] = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa;
		byte byAmpSingle = m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle;
		byte byAmpFs = m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs;
		byte byOcpMod = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod;
		m_byEfuseArr[b4 + 47] = (byte)(((byAmpSingle & 1) << 7) + ((byAmpFs & 1) << 6) + ((byOvpDis & 1) << 5) + ((byOcpDis & 1) << 4) + ((byOcpSel & 3) << 2) + (byOcpMod & 3));
		byte byEtcEn = m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn;
		byte byOuvpThr = m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr;
		byte byStallTstQMax = m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstQMax;
		byte byStallTstInc = m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstInc;
		byte byStallTstEn = m_sMotorParamCfg.m_eSpecialRegCfg.byStallTstEn;
		byte byDirInv = m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv;
		byte byDirEn = m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1;
		byte byDirEn2 = m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0;
		m_byEfuseArr[b4 + 48] = (byte)((byEtcEn << 7) + ((byOuvpThr & 1) << 6) + ((byStallTstQMax & 1) << 5) + ((byStallTstInc & 1) << 4) + ((byStallTstEn & 1) << 3) + ((byDirInv & 1) << 2) + ((byDirEn & 1) << 1) + (byDirEn2 & 1));
		byte byLimitSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel;
		byte byTsdSel2 = m_sMotorParamCfg.m_eSpecialRegCfg.byTsdSel;
		m_byEfuseArr[b4 + 49] = (byte)((((byYMaxSel >> 3) & 3) << 6) + ((byLimitSel & 3) << 4) + (byTsdSel2 & 0xF));
		byte b29 = 0;
		byte byMotorDir = m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir;
		byte byPwmEn = m_sMotorParamCfg.m_sUserStatusInfoCfg.byPwmEn;
		byte byTestMode = m_sMotorParamCfg.m_eSpecialRegCfg.byTestMode;
		byte byDbgMd = m_sMotorParamCfg.m_sUserStatusInfoCfg.byDbgMd;
		byte b30 = 1;
		m_byEfuseArr[b4 + 50] = (byte)(((byDbgMd & 3) << 6) + ((byPwmEn & 1) << 4) + ((byTestMode & 1) << 3) + ((b29 & 1) << 2) + ((byMotorDir & 1) << 1) + (b30 & 1));
		byte byFDS = m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS0;
		byte byFDS2 = m_sMotorParamCfg.m_sUserStatusInfoCfg.byFDS1;
		byte b31 = (byte)(((byFDS2 & 0xF) << 4) + (byFDS & 0xF));
		m_byEfuseArr[b4 + 51] = b31;
		byte bySpdCtrl = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl;
		float num32 = 40.96f;
		ushort num33 = (ushort)(num32 * (float)(int)bySpdCtrl);
		num33 <<= 4;
		m_byEfuseArr[b4 + 52] = (byte)(num33 >> 8);
		m_byEfuseArr[b4 + 53] = (byte)(num33 & 0xFF);
		byte byDigMoniCt = m_sMotorParamCfg.m_eSpecialRegCfg.byDigMoniCt;
		m_byEfuseArr[b4 + 54] = (byte)(((byDigMoniCt & 3) << 6) + (byDigTst & 0x1F));
		return true;
	}

	public void DispatcherThread()
	{
		while (true)
		{
			bool flag = true;
			byte b = 0;
			int num = 0;
			int num2 = 0;
			byte b2 = 0;
			byte b3 = 0;
			ushort wTotalSetCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt;
			for (num = 0; num < wTotalSetCnt; num++)
			{
				if (m_bFinishedTest)
				{
					break;
				}
				if (m_bCanceled)
				{
					break;
				}
				OutputDebugString("begin onoff..\r\n");
				do
				{
					flag = true;
					m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt = (ushort)(num + 1);
					if (b2 == 0)
					{
						b2 = 1;
						OnTimer.Start();
						b3 = 0;
					}
				}
				while (m_bStartFlag);
				OnTimer.Stop();
				if (m_bFinishedTest || m_bCanceled)
				{
					break;
				}
				do
				{
					flag = true;
					if (b3 == 0)
					{
						b3 = 1;
						OffTimer.Start();
						b2 = 0;
					}
				}
				while (m_bStopFlag);
				OffTimer.Stop();
				textBox_DoneCnt.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateCnt));
				Thread.Sleep(100);
			}
			if (wTotalSetCnt == num)
			{
				m_bFinishedTest = true;
				OutputDebugString("finished\r\n");
			}
			Thread.Sleep(100);
		}
	}

	private void UpdateCnt()
	{
		ushort wExecutedCnt = m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt;
		if (wExecutedCnt == m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt || m_bFinishedTest)
		{
			bool flag = true;
			HandleOnoffDisplay(flag);
			btnDebug.IsEnabled = !flag;
			byte byStopDuty = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty;
			m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = byStopDuty;
			int num = ExecuteDebugOperation();
			bool flag2 = true;
		}
		textBox_DoneCnt.Text = wExecutedCnt.ToString();
	}

	private void timer_OnTick(object sender, EventArgs e)
	{
		byte byStopDuty = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = byStopDuty;
		int num = 0;
		bool flag = true;
		if (!m_bCanceled)
		{
			num = ExecuteDebugOperation();
		}
		byte byDirSwitch = m_sMotorParamCfg.m_sOnOffTestCfg.byDirSwitch;
		byte byMotorDir = m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir;
		bool flag2 = checkBox_DirSwitch.IsChecked == true;
		if (byDirSwitch == 1 && flag2)
		{
			if (byMotorDir == 1)
			{
				m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = 0;
			}
			else
			{
				m_sMotorParamCfg.m_sUserStatusInfoCfg.byMotorDir = 1;
			}
		}
		m_bStartFlag = false;
		m_bStopFlag = true;
		OutputDebugString("start");
	}

	private void timer_OffTick(object sender, EventArgs e)
	{
		int num = 0;
		byte byStartDuty = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty;
		m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = byStartDuty;
		bool flag = true;
		if (!m_bCanceled && m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt != m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt)
		{
			num = ExecuteDebugOperation();
		}
		m_bStartFlag = true;
		m_bStopFlag = false;
		OutputDebugString("stop");
		OutputDebugString(textBox_DoneCnt.Text);
	}

	private void OnOffTimer_Tick(object sender, EventArgs e)
	{
		try
		{
			if (m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt >= m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt || m_bStopFlag || m_bCanceled)
			{
				OutputDebugString("OnOff test stop------");
				dtFinalTime = DateTime.Now;
				btn_OnOffTest.Content = "Test";
				HandleOnoffDisplay(bCtrlShow: true);
				OnOffTimer.Stop();
				if (m_bCanceled)
				{
					MessageBox.Show("On off test stopped!", "Stop", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				}
				else
				{
					MessageBox.Show("On off test finished!", "Finish", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				}
				return;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString(), "Exception");
		}
		int num = 0;
		switch ((!m_bStartFlag) ? 1 : 0)
		{
		case 0:
		{
			OutputDebugString("Off time start------");
			DateTime now = DateTime.Now;
			dtFinalTime = now.AddSeconds((int)m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuration);
			m_bStartFlag = false;
			if (readTimer.IsEnabled)
			{
				readTimer.Stop();
				readTimer.IsEnabled = false;
				TimeSpan timeSpan = now.Subtract(dtBeginOnTestTime);
				double value = (float)(int)m_wRunningSpeed / 32767f * m_fMotorSpeedBase;
				value = Math.Round(value, 2);
				m_OnOffTestRecordS.dtBeginTime = dtBeginOnTestTime;
				m_OnOffTestRecordS.dtEndTime = now;
				m_OnOffTestRecordS.dCurSpeed = value;
				m_OnOffTestRecordS.dTimeInterval = Math.Round(timeSpan.TotalSeconds, 2);
				m_OnOffTestTotalList.Add(m_OnOffTestRecordS);
			}
			m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = m_sMotorParamCfg.m_sOnOffTestCfg.byStopDuty;
			m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt++;
			m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt = (ushort)(m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt - m_sMotorParamCfg.m_sOnOffTestCfg.wPassCnt);
			OutputDebugString("exeCnt:" + m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt);
			break;
		}
		case 1:
		{
			OutputDebugString("On time start------");
			dtBeginOnTestTime = DateTime.Now;
			m_bStartFlag = true;
			m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl = m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuty;
			OnOffTimer.Interval = TimeSpan.FromSeconds((int)m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration);
			int tickCount = Environment.TickCount;
			readTimer.IsEnabled = true;
			readTimer.Start();
			break;
		}
		}
		num = ExecuteDebugOperation();
	}

	private void readTimer_Tick(object sender, EventArgs e)
	{
		DateTime now = DateTime.Now;
		TimeSpan timeSpan = now.Subtract(dtBeginOnTestTime);
		double value = (float)(int)m_wRunningSpeed / 32767f * m_fMotorSpeedBase;
		value = Math.Round(value, 2);
		if (value >= m_sMotorParamCfg.m_sOnOffTestCfg.dTgtSpeed)
		{
			ParamStructureDef.S_OnOffTestRecordCfg s_OnOffTestRecordCfg = new ParamStructureDef.S_OnOffTestRecordCfg
			{
				dtBeginTime = dtBeginOnTestTime,
				dtEndTime = now,
				dCurSpeed = value,
				dTimeInterval = Math.Round(timeSpan.TotalSeconds, 2)
			};
			OutputDebugString("speed:" + value + " time:" + timeSpan.TotalSeconds);
			if (timeSpan.TotalSeconds >= m_sMotorParamCfg.m_sOnOffTestCfg.dMinDetectTime && timeSpan.TotalSeconds <= m_sMotorParamCfg.m_sOnOffTestCfg.dMaxDetectTime)
			{
				m_sMotorParamCfg.m_sOnOffTestCfg.wPassCnt++;
				s_OnOffTestRecordCfg.strStatus = "PASS";
				s_OnOffTestRecordCfg.byPassStatus = 1;
				m_OnOffTestTotalList.Add(s_OnOffTestRecordCfg);
				m_OnOffTestRecordS = s_OnOffTestRecordCfg;
				OutputDebugString("current speed " + value + " >= " + m_sMotorParamCfg.m_sOnOffTestCfg.dTgtSpeed + ", pass cnt:" + m_sMotorParamCfg.m_sOnOffTestCfg.wPassCnt);
			}
			else
			{
				s_OnOffTestRecordCfg.strStatus = "FAIL";
				s_OnOffTestRecordCfg.byPassStatus = 0;
				m_OnOffTestTotalList.Add(s_OnOffTestRecordCfg);
				m_OnOffTestRecordS = s_OnOffTestRecordCfg;
			}
			readTimer.Stop();
			readTimer.IsEnabled = false;
		}
		else if (timeSpan.TotalSeconds >= (double)(int)m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration)
		{
			OutputDebugString("speed:" + value + " time:" + timeSpan.TotalSeconds);
			ParamStructureDef.S_OnOffTestRecordCfg s_OnOffTestRecordCfg = new ParamStructureDef.S_OnOffTestRecordCfg
			{
				dtBeginTime = dtBeginOnTestTime,
				dtEndTime = now,
				dTimeInterval = Math.Round(timeSpan.TotalSeconds, 2)
			};
			if (s_OnOffTestRecordCfg.dTimeInterval == (double)(int)m_sMotorParamCfg.m_sOnOffTestCfg.byStartDuration)
			{
				s_OnOffTestRecordCfg.dTimeInterval += 0.01;
			}
			s_OnOffTestRecordCfg.dCurSpeed = value;
			s_OnOffTestRecordCfg.strStatus = "FAIL";
			s_OnOffTestRecordCfg.byPassStatus = 0;
			m_OnOffTestTotalList.Add(s_OnOffTestRecordCfg);
			m_OnOffTestRecordS = s_OnOffTestRecordCfg;
			readTimer.Stop();
			readTimer.IsEnabled = false;
		}
	}

	public void SaveAsExcel(string strFileName)
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("Date");
		dataTable.Columns.Add("Time");
		dataTable.Columns.Add("Body");
		dataTable.Columns.Add("FailIdx");
		dataTable.Columns.Add("RealCnt");
		dataTable.Columns.Add("FaultCode");
		dataTable.Columns.Add("Status");
		dataTable.Columns.Add("Remark");
		int num = 0;
		DateTime now = DateTime.Now;
		int num2 = 0;
		DataRow dataRow = dataTable.NewRow();
		dataRow["Date"] = now.ToString("yyyy/MM/dd");
		dataRow["Time"] = now.ToString("HH:mm:ss");
		dataRow["Body"] = "FT8132";
		num2 = m_sMotorParamCfg.m_sOnOffTestCfg.wFailCnt;
		dataRow["FailIdx"] = num2;
		num2 = m_wFailRealCnt;
		dataRow["RealCnt"] = num2;
		dataRow["FaultCode"] = m_byOldFaultVal;
		if (m_byOldFaultVal != 0)
		{
			dataRow["Status"] = "FAIL";
		}
		else
		{
			dataRow["Status"] = "/";
		}
		dataRow["Remark"] = m_strFailRemark;
		string message = m_byOldFaultVal + "_" + m_strFailRemark;
		OutputDebugString(message);
		dataTable.Rows.Add(dataRow);
		using ExcelHelper excelHelper = new ExcelHelper(strFileName);
		bool isAppendToExcel = true;
		excelHelper.DataTableToExcel(dataTable, 0, 1, "Sheet1", isColumnWritten: true, isAppendToExcel);
	}

	private int HandleCtrlModeCurveShow()
	{
		NormalDrawingElement8215ClsLp normalDrawingElement8215ClsLp = new NormalDrawingElement8215ClsLp();
		gridCLCurve.Children.Clear();
		gridCLCurve.Children.Add(normalDrawingElement8215ClsLp);
		ushort num = 1440;
		ushort num2 = 360;
		byte b = 1;
		float num3 = 12f;
		byte b2 = 1;
		ushort num4 = (ushort)(num2 + num);
		byte b3 = (byte)(num3 + (float)(int)b);
		float num5 = (float)((double)(int)num4 * 1.25);
		int num6 = 0;
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		byte byPwmOffDis = m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis;
		byte byCurveEnSpd = m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd;
		byte byPwmRevEn = m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn;
		byte byXON = m_sMotorParamCfg.m_sTargetSpdCurve.byXON0;
		byte byX_ON = m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON;
		uint dwMinSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed;
		uint dwMaxSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed;
		float fMinPower = m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower;
		float fMaxPower = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower;
		float fMinCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent;
		float fMaxCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent;
		byte byYMaxSel = m_sMotorParamCfg.m_sTargetSpdCurve.byYMaxSel;
		byte byPwmX98En = m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En;
		byte byY_ON = m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON;
		byte byY_ = m_sMotorParamCfg.m_sTargetSpdCurve.byY_25;
		byte byY_2 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_50;
		byte byY_3 = m_sMotorParamCfg.m_sTargetSpdCurve.byY_75;
		float num7 = float.Parse(textBlock_XON0.Text);
		float num8 = float.Parse(textBlock_XON.Text);
		float num9 = float.Parse(textBlock_YON.Text);
		float num10 = float.Parse(textBlock_Y25.Text);
		float num11 = float.Parse(textBlock_Y50.Text);
		float num12 = float.Parse(textBlock_Y75.Text);
		float num13 = (float)((double)(int)byYMaxSel * 1.7857142857142858);
		string s = $"{num13:F3}";
		num13 = float.Parse(s);
		num13 = (float)(100.0 - (double)num13);
		float num14 = 100f;
		float num15 = (float)((double)fMaxCurrent * 0.025);
		float num16 = (float)(int)byYMaxSel * num15;
		float num17 = (float)((double)fMaxCurrent * 0.1);
		float num18 = fMaxCurrent + num17;
		num16 = num18 - num16;
		ushort num19 = (ushort)((double)dwMaxSpeed * (1.0 / 64.0));
		ushort num20 = (ushort)(byYMaxSel * num19);
		ushort num21 = (ushort)((double)dwMaxSpeed * 0.0625);
		ushort num22 = (ushort)(dwMaxSpeed + num21);
		num20 = (ushort)(num22 - num20);
		normalDrawingElement8215ClsLp.SetCtrlCurveParam(byCurveEnSpd, byPwmX98En, byPwmRevEn);
		normalDrawingElement8215ClsLp.InitEfuseBuffer(byPwmOffDis, byControlMode);
		switch (byControlMode)
		{
		case 0:
		{
			int num25 = 0;
			if (byPwmOffDis == 0)
			{
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num14 - num8;
				}
			}
			else
			{
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num9;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num14;
				}
			}
			num25++;
			if (byCurveEnSpd == 1)
			{
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num9;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num14 - num8;
				}
				num25++;
				if (num8 <= 25f)
				{
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num10;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 25f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 25.0);
					}
					num25++;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num11;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 50f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 50.0);
				}
				num25++;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num12;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 75f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 75.0);
				}
				num25++;
				if (byPwmX98En == 1)
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num13;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 98.0);
					}
					num25++;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num13;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 100.0);
					}
				}
				else
				{
					num6 = 5;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num13;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 100.0);
					}
				}
				num6 = num25 + 1;
			}
			else
			{
				num6 = 5;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num9;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = num14 - num8;
				}
				if (byPwmX98En == 1)
				{
					num25++;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num13;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 98.0);
					}
				}
				num25++;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25] = num13;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25] = (float)((double)num14 - 100.0);
				}
				num6 = num25 + 1;
			}
			normalDrawingElement8215ClsLp.wStopVLow = num2;
			normalDrawingElement8215ClsLp.wStopVRange = num;
			normalDrawingElement8215ClsLp.wStopVLowVRange = num4;
			normalDrawingElement8215ClsLp.fStopSminDutyHyster = num3;
			normalDrawingElement8215ClsLp.fStopVLowVRangeMax = 110f;
			normalDrawingElement8215ClsLp.SetPointCount(num6);
			break;
		}
		case 1:
		{
			if (byCurveEnSpd == 1)
			{
				if (byPwmX98En == 1)
				{
					if (byPwmOffDis == 1)
					{
						num6 = 7;
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = num16;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = num16;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 100f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 0f;
						}
					}
					else
					{
						num6 = 8;
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxCurrent;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = num16;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[7] = num16;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[7] = 100f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[7] = 0f;
						}
					}
					normalDrawingElement8215ClsLp.SetPointCount(num6);
				}
				else if (byPwmOffDis == 1)
				{
					num6 = 5;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 0f;
					}
				}
				else
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
					}
				}
				normalDrawingElement8215ClsLp.SetYMaxSel(byYMaxSel);
			}
			else if (byPwmX98En == 1)
			{
				if (byPwmOffDis == 1)
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = (float)((double)num14 - 98.0);
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
					}
				}
				else
				{
					num6 = 7;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = fMaxCurrent;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 0f;
					}
				}
			}
			else if (byPwmOffDis == 1)
			{
				num6 = 5;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 0f;
				}
			}
			else
			{
				num6 = 6;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxCurrent;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
				}
			}
			normalDrawingElement8215ClsLp.wStopVLow = num2;
			normalDrawingElement8215ClsLp.wStopVRange = num;
			normalDrawingElement8215ClsLp.wStopVLowVRange = num4;
			normalDrawingElement8215ClsLp.fStopSminDutyHyster = num3;
			ushort num24 = (ushort)((double)fMaxCurrent * 0.2);
			normalDrawingElement8215ClsLp.fStopVLowVRangeMax = fMaxCurrent + (float)(int)num24;
			break;
		}
		case 3:
		{
			if (byCurveEnSpd == 1)
			{
				if (byPwmX98En == 1)
				{
					if (byPwmOffDis == 1)
					{
						num6 = 7;
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 100f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 0f;
						}
					}
					else
					{
						num6 = 8;
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[7] = fMaxPower;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[7] = 100f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[7] = 0f;
						}
					}
					normalDrawingElement8215ClsLp.SetPointCount(num6);
				}
				else if (byPwmOffDis == 1)
				{
					num6 = 5;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 0f;
					}
				}
				else
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
					}
				}
				normalDrawingElement8215ClsLp.SetYMaxSel(byYMaxSel);
			}
			else if (byPwmX98En == 1)
			{
				if (byPwmOffDis == 1)
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = (float)((double)num14 - 98.0);
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
					}
				}
				else
				{
					num6 = 7;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = fMaxPower;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 0f;
					}
				}
			}
			else if (byPwmOffDis == 1)
			{
				num6 = 5;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = fMinPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = fMinPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMaxPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 0f;
				}
			}
			else
			{
				num6 = 6;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = fMaxPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = fMaxPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = fMinPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = fMaxPower;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
				}
			}
			normalDrawingElement8215ClsLp.wStopVLow = num2;
			normalDrawingElement8215ClsLp.wStopVRange = num;
			normalDrawingElement8215ClsLp.wStopVLowVRange = num4;
			normalDrawingElement8215ClsLp.fStopSminDutyHyster = num3;
			ushort num24 = 1;
			normalDrawingElement8215ClsLp.fStopVLowVRangeMax = fMaxPower + (float)(int)num24;
			break;
		}
		default:
		{
			if (byCurveEnSpd == 1)
			{
				if (byPwmX98En == 1)
				{
					if (byPwmOffDis == 1)
					{
						num6 = 7;
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = dwMinSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = dwMinSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMaxSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = (int)num20;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = (int)num20;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 100f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 0f;
						}
					}
					else
					{
						num6 = 8;
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMinSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = dwMaxSpeed;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = (int)num20;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 98f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = (float)((double)num14 - 98.0);
						}
						normalDrawingElement8215ClsLp.m_fTableCurvesRPM[7] = (int)num20;
						if (byPwmRevEn == 0)
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[7] = 100f;
						}
						else
						{
							normalDrawingElement8215ClsLp.m_fTableCurveDuty[7] = 0f;
						}
					}
					normalDrawingElement8215ClsLp.SetPointCount(num6);
				}
				else if (byPwmOffDis == 1)
				{
					num6 = 5;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = dwMinSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = dwMinSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 0f;
					}
				}
				else
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMinSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
					}
				}
				normalDrawingElement8215ClsLp.SetYMaxSel(byYMaxSel);
			}
			else if (byPwmX98En == 1)
			{
				if (byPwmOffDis == 1)
				{
					num6 = 6;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = dwMinSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = dwMinSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = (float)((double)num14 - 98.0);
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
					}
				}
				else
				{
					num6 = 7;
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMinSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 98f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = (float)((double)num14 - 98.0);
					}
					normalDrawingElement8215ClsLp.m_fTableCurvesRPM[6] = dwMaxSpeed;
					if (byPwmRevEn == 0)
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 100f;
					}
					else
					{
						normalDrawingElement8215ClsLp.m_fTableCurveDuty[6] = 0f;
					}
				}
			}
			else if (byPwmOffDis == 1)
			{
				num6 = 5;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = dwMinSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = dwMinSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMaxSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = 0f;
				}
			}
			else
			{
				num6 = 6;
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[0] = dwMaxSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = 0f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[0] = num14;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[1] = dwMaxSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[1] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[2] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num7;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[2] = num14 - num7;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[3] = 0f;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[3] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[4] = dwMinSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num8;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[4] = num14 - num8;
				}
				normalDrawingElement8215ClsLp.m_fTableCurvesRPM[5] = dwMaxSpeed;
				if (byPwmRevEn == 0)
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 100f;
				}
				else
				{
					normalDrawingElement8215ClsLp.m_fTableCurveDuty[5] = 0f;
				}
			}
			normalDrawingElement8215ClsLp.wStopVLow = num2;
			normalDrawingElement8215ClsLp.wStopVRange = num;
			normalDrawingElement8215ClsLp.wStopVLowVRange = num4;
			normalDrawingElement8215ClsLp.fStopSminDutyHyster = num3;
			uint num23 = (uint)((double)dwMaxSpeed * 0.125);
			normalDrawingElement8215ClsLp.fStopVLowVRangeMax = dwMaxSpeed + num23;
			break;
		}
		}
		m_fTableCurveXList.Clear();
		m_fTableCurveYList.Clear();
		for (int num25 = 0; num25 < normalDrawingElement8215ClsLp.m_fTableCurveDuty.Length; num25++)
		{
			m_fTableCurveXList.Add(normalDrawingElement8215ClsLp.m_fTableCurveDuty[num25]);
			m_fTableCurveYList.Add(normalDrawingElement8215ClsLp.m_fTableCurvesRPM[num25]);
		}
		return 0;
	}

	private void gridCLCurve_MouseMove(object sender, MouseEventArgs e)
	{
		ushort num = 0;
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		byte bySpdCtrlMd = m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd;
		byte byPwmOffDis = m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis;
		uint dwMaxSpeed = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed;
		float fMaxCurrent = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent;
		float fMaxPower = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower;
		if (byControlMode == 0)
		{
			num = 5;
		}
		else if (byPwmOffDis == 1)
		{
			num = 5;
		}
		else
		{
			num = 6;
		}
		NormalDrawingElement8215ClsLp normalDrawingElement8215ClsLp = new NormalDrawingElement8215ClsLp();
		Point position = e.GetPosition(gridCLCurve);
		double x = position.X;
		double num2 = position.Y;
		ushort num3 = 0;
		uint num4 = 0u;
		string text = "";
		string content = "";
		switch (byControlMode)
		{
		case 0:
			if (x >= 50.0 && x <= 435.0)
			{
				x = (x - 55.0) / 378.0 * 100.0;
				num3 = (ushort)x;
				if (num2 < 35.0)
				{
					num2 = 35.0;
				}
				if (num2 > 230.0)
				{
					num2 = 230.0;
				}
				num2 = (1.0 - (num2 - 35.0) / 195.0) * 100.0;
				num2 = Math.Round(ConvertToCurveY((int)num3, num2), 0);
				num4 = (uint)num2;
			}
			else
			{
				num3 = 0;
				num4 = 0u;
			}
			text = ((bySpdCtrlMd != 3) ? ("PWM: " + num3 + " %") : ("CLOCK: " + num3));
			content = "OutDuty: " + num4 + " %";
			break;
		case 1:
			if (x >= 50.0 && x <= 435.0)
			{
				x = (x - 50.0) / 383.0 * 100.0;
				num3 = (ushort)x;
				if (num2 < 35.0)
				{
					num2 = 35.0;
				}
				if (num2 > 230.0)
				{
					num2 = 230.0;
				}
				num2 = (1.0 - (num2 - 35.0) / 195.0) * (double)fMaxCurrent;
				content = "CUR: " + Math.Round(ConvertToCurveY((int)num3, num2), 3).ToString("0.000");
			}
			else
			{
				num3 = 0;
				num4 = 0u;
				if (byControlMode == 1)
				{
					content = "CUR: " + num4;
				}
			}
			text = ((bySpdCtrlMd != 3) ? ("PWM: " + num3 + " %") : ("CLOCK: " + num3));
			break;
		case 3:
			if (x >= 50.0 && x <= 435.0)
			{
				x = (x - 50.0) / 383.0 * 100.0;
				num3 = (ushort)x;
				if (num2 < 35.0)
				{
					num2 = 35.0;
				}
				if (num2 > 230.0)
				{
					num2 = 230.0;
				}
				num2 = (1.0 - (num2 - 35.0) / 195.0) * (double)fMaxPower;
				num2 = Math.Round(ConvertToCurveY((int)num3, num2), 3);
			}
			else
			{
				num3 = 0;
				num2 = 0.0;
			}
			text = ((bySpdCtrlMd != 3) ? ("PWM: " + num3 + " %") : ("CLOCK: " + num3));
			content = "POWER: " + num2.ToString("0.000") + " W";
			break;
		default:
			if (x >= 50.0 && x <= 435.0)
			{
				x = (x - 50.0) / 383.0 * 100.0;
				num3 = (ushort)x;
				if (num2 < 35.0)
				{
					num2 = 35.0;
				}
				if (num2 > 230.0)
				{
					num2 = 230.0;
				}
				num2 = (1.0 - (num2 - 35.0) / 195.0) * (double)dwMaxSpeed;
				num4 = (uint)Math.Round(ConvertToCurveY((int)num3, num2), 0);
			}
			else
			{
				num3 = 0;
				num4 = 0u;
			}
			text = ((bySpdCtrlMd != 3) ? ("PWM: " + num3 + " %") : ("CLOCK: " + num3));
			content = "RPM: " + num4;
			break;
		}
		labelPWM.Content = text;
		labelRPM.Content = content;
	}

	private double ConvertToCurveY(double wMouseX, double dbMouseY)
	{
		double result = 0.0;
		int num = 0;
		float[] array = new float[m_fTableCurveXList.Count];
		float[] array2 = new float[m_fTableCurveYList.Count];
		m_fTableCurveXList.CopyTo(array, 0);
		m_fTableCurveYList.CopyTo(array2, 0);
		for (num = 0; num < array.Length - 1; num++)
		{
			if ((float)wMouseX == array[num])
			{
				result = array2[num];
				break;
			}
			if ((float)wMouseX == array[num + 1])
			{
				result = array2[num + 1];
				break;
			}
			if (!((float)wMouseX > array[num + 1]))
			{
				result = PointYAtOneLine(new Point(array[num], array2[num]), new Point(array[num + 1], array2[num + 1]), wMouseX);
				break;
			}
		}
		return result;
	}

	public double PointYAtOneLine(Point point1, Point point2, double x)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		if (point1.X != point2.X)
		{
			num2 = (point2.Y - point1.Y) / (point2.X - point1.X);
			num3 = point1.Y - num2 * point1.X;
			return num2 * x + num3;
		}
		return point1.Y;
	}

	private void radioBtn_PwmX98Dis_Click(object sender, RoutedEventArgs e)
	{
		if (radioBtn_PwmX98Dis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 1;
		}
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_PwmX98En_Click(object sender, RoutedEventArgs e)
	{
		if (radioBtn_PwmX98En.IsChecked == true)
		{
			m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 1;
		}
		else
		{
			m_sMotorParamCfg.m_sTargetSpdCurve.byPwmX98En = 0;
		}
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_PwmOffDis_Click(object sender, RoutedEventArgs e)
	{
		if (radioBtn_PwmOffDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 1;
		}
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_PwmOffEn_Click(object sender, RoutedEventArgs e)
	{
		if (radioBtn_PwmOffEn.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 1;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmOffDis = 0;
		}
		HandleCtrlModeCurveShow();
	}

	public int CheckValueValid(double dMin, double dMax, double dCurrent)
	{
		if (dCurrent < dMin || dCurrent > dMax)
		{
			string text = "invalid value, please input floating number between " + dMin + " and " + dMax;
			return 1;
		}
		return 0;
	}

	public int CheckValueValid(int iMin, int iMax, ushort wCurrent)
	{
		if (wCurrent < iMin || wCurrent > iMax)
		{
			string text = "invalid value, please input decimal integer between " + iMin + " and " + iMax;
			return 1;
		}
		return 0;
	}

	private void textBox_MinSpeed_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			int num = 0;
			if (CheckSpeedMinValidationNew() == 0)
			{
				HandleCtrlModeCurveShow();
			}
		}
	}

	private void textBox_MinSpeed_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		if (CheckSpeedMinValidationNew() == 0)
		{
			HandleCtrlModeCurveShow();
		}
	}

	private int CheckSpeedMinValidationNew()
	{
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_fMotorSpeedBase = 0f;
		CacluCurrenAndSpeedBase();
		int num = 0;
		float num2 = m_fMotorCurrentBase / 2f;
		uint num3 = (uint)(m_fMotorSpeedBase / 2f);
		string text = textBox_MinSpeed.Text;
		switch (byControlMode)
		{
		case 1:
		{
			float fMinCurrent = 0f;
			try
			{
				fMinCurrent = float.Parse(text);
			}
			catch (Exception)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
				num = 1;
			}
			if (num != 0)
			{
				if (m_byIsLoadDefault == 1)
				{
					textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent.ToString();
				}
				else
				{
					textBox_MinSpeed.Text = (string)FindResource("Cfg_RunMinCurrent");
				}
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.fMinCurrent = fMinCurrent;
			}
			break;
		}
		case 2:
		{
			uint num5 = 0u;
			try
			{
				num5 = uint.Parse(text);
				if (num5 > num3 || num5 < 0)
				{
					MessageBox.Show("invalid value, please input decimal integer between 0 and " + num3);
					num = 1;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and " + num3);
				num = 1;
			}
			if (num != 0)
			{
				if (m_byIsLoadDefault == 1)
				{
					textBox_MinSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed.ToString();
				}
				else
				{
					textBox_MinSpeed.Text = (string)FindResource("Cfg_RunMinSpeed");
				}
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.dwMinSpeed = num5;
			}
			break;
		}
		default:
		{
			float num4 = 0f;
			try
			{
				num4 = float.Parse(text);
				num2 = 100f;
				if (num4 > num2 || (double)num4 < 0.0)
				{
					MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
					num = 1;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
				num = 1;
			}
			if (num != 0)
			{
				if (m_byIsLoadDefault == 1)
				{
					textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower.ToString("0.000");
				}
				else
				{
					textBox_MaxSpeed.Text = (string)FindResource("Cfg_RunMinPower");
				}
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.fMinPower = num4;
			}
			break;
		}
		case 0:
			break;
		}
		return num;
	}

	private int CheckSpeedMinValidation()
	{
		return 0;
	}

	private void textBox_MaxSpeed_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			int num = 0;
			if (CheckSpeedMaxValidationNew() == 0)
			{
				HandleCtrlModeCurveShow();
			}
		}
	}

	private void textBox_MaxSpeed_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		if (CheckSpeedMaxValidationNew() == 0)
		{
			HandleCtrlModeCurveShow();
		}
	}

	private int CheckSpeedMaxValidationNew()
	{
		byte byControlMode = m_sMotorParamCfg.m_sStartCfg.byControlMode;
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_fMotorSpeedBase = 0f;
		CacluCurrenAndSpeedBase();
		int num = 0;
		float num2 = m_fMotorCurrentBase;
		uint num3 = (uint)m_fMotorSpeedBase;
		string text = textBox_MaxSpeed.Text;
		switch (byControlMode)
		{
		case 1:
		{
			float fMaxCurrent = 0f;
			try
			{
				fMaxCurrent = float.Parse(text);
			}
			catch (Exception)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
				num = 1;
			}
			if (num != 0)
			{
				if (m_byIsLoadDefault == 1)
				{
					textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent.ToString("0.000");
				}
				else
				{
					textBox_MaxSpeed.Text = (string)FindResource("Cfg_RunMaxCurrent");
				}
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.fMaxCurrent = fMaxCurrent;
			}
			break;
		}
		case 2:
		{
			uint num5 = 0u;
			try
			{
				num5 = uint.Parse(text);
				if (num5 > num3 || num5 < 0)
				{
					MessageBox.Show("invalid value, please input decimal integer between 0 and " + num3);
					num = 1;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and " + num3);
				num = 1;
			}
			if (num != 0)
			{
				if (m_byIsLoadDefault == 1)
				{
					textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed.ToString();
				}
				else
				{
					textBox_MaxSpeed.Text = (string)FindResource("Cfg_RunMaxSpeed");
				}
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.dwMaxSpeed = num5;
			}
			break;
		}
		default:
		{
			float num4 = 0f;
			try
			{
				num4 = float.Parse(text);
				num2 = 300f;
				if (num4 > num2 || (double)num4 < 0.0)
				{
					MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
					num = 1;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
				num = 1;
			}
			if (num != 0)
			{
				if (m_byIsLoadDefault == 1)
				{
					textBox_MaxSpeed.Text = m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower.ToString("0.000");
				}
				else
				{
					textBox_MaxSpeed.Text = (string)FindResource("Cfg_RunMaxPower");
				}
			}
			else
			{
				m_sMotorParamCfg.m_sTargetSpdCurve.fMaxPower = num4;
			}
			break;
		}
		case 0:
			break;
		}
		return num;
	}

	private int CheckSpeedMaxValidation()
	{
		return 0;
	}

	private void radioBtn_CurveSpdDis_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 0;
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_PwmRevDis_Click(object sender, RoutedEventArgs e)
	{
		if (radioBtn_PwmRevDis.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 0;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 1;
		}
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_PwmRevEn_Click(object sender, RoutedEventArgs e)
	{
		if (radioBtn_PwmRevEn.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 1;
		}
		else
		{
			m_sMotorParamCfg.m_sOhterCfg.byPwmRevEn = 0;
		}
		HandleCtrlModeCurveShow();
	}

	public int ReadRegister()
	{
		int num = 0;
		SurspendReadThread();
		statusBar1.Items.Clear();
		TextBlock textBlock = new TextBlock();
		textBlock.Text = "Device:";
		textBlock.Width = 50.0;
		textBlock.HorizontalAlignment = HorizontalAlignment.Left;
		statusBar1.Items.Add(textBlock);
		TextBlock textBlock2 = new TextBlock();
		textBlock2.Text = "reading reg...";
		textBlock2.Width = 80.0;
		textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
		textBlock2.VerticalAlignment = VerticalAlignment.Stretch;
		statusBar1.Items.Add(textBlock2);
		TextBlock textBlock3 = new TextBlock();
		textBlock3.Text = "Elaps Time:";
		textBlock3.Width = 80.0;
		statusBar1.Items.Add(textBlock3);
		TextBlock textBlock4 = new TextBlock();
		textBlock4.Text = "00:00:00";
		textBlock4.Width = 80.0;
		statusBar1.Items.Add(textBlock4);
		Ellipse ellipse = new Ellipse();
		ellipse.HorizontalAlignment = HorizontalAlignment.Right;
		ellipse.VerticalAlignment = VerticalAlignment.Center;
		ellipse.Width = 12.0;
		ellipse.Height = 12.0;
		ellipse.Fill = flagEllipse.Background;
		statusBar1.Items.Add(ellipse);
		TextBlock textBlock5 = new TextBlock();
		textBlock5.Text = lblMessage.Content.ToString();
		textBlock5.Width = 80.0;
		statusBar1.Items.Add(textBlock5);
		ProgressBar progressBar = new ProgressBar();
		progressBar.Width = 640.0;
		progressBar.Height = 20.0;
		Duration duration = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation = new DoubleAnimation(20.0, duration);
		progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
		ToolTip toolTip = new ToolTip();
		toolTip.Content = "reading efs: 20%";
		progressBar.ToolTip = toolTip;
		statusBar1.Items.Add(progressBar);
		dtBeginTime = DateTime.Now;
		int num2 = 0;
		sPreReg.byRegisterNum = 35;
		for (num2 = 0; num2 < sPreReg.byRegisterNum; num2++)
		{
			sPreReg.adwRegAdd[num2] = (uint)(133 + num2);
			sPreReg.adwRegVal[num2] = 0u;
		}
		byte byOptType = 4;
		byte byAccType = 19;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 2;
		}
		num2 = 0;
		for (num2 = 0; num2 < 3; num2++)
		{
			num = DllDebuggerAPI.DLL_GPLB32CmdPassThrough(0, ref sPreReg);
			if (num != 0)
			{
				RestoreReadThread();
				return num;
			}
			num |= GetCommStatus();
			if (num != 0)
			{
				RestoreReadThread();
				MessageBox.Show("usb communication fail.", "RR");
				return num;
			}
			if (m_byCommunicationStatus != 1)
			{
				break;
			}
			if (num2 != 1)
			{
				continue;
			}
			num = ScanI2CAddrDevID();
			if (num != 0)
			{
				ReEnumDevSN();
				if (!m_isCmdOpera)
				{
					MessageBox.Show("usb communication fail at read register.", "RR Scan");
				}
				return num;
			}
			if (m_byCommunicationStatus == 1)
			{
				return num;
			}
		}
		uint num3 = 0u;
		sPreReg.byRegisterNum = 35;
		for (num2 = 0; num2 < sPreReg.byRegisterNum; num2++)
		{
			if (num2 > 34)
			{
				num3 = (byte)sPreReg.adwRegAdd[num2];
				m_byREfuseArr[num2] = (byte)sPreReg.adwRegVal[num2];
			}
			else
			{
				num3 = (byte)sPreReg.adwRegAdd[num2];
				m_byREfuseArr[num2] = (byte)sPreReg.adwRegVal[num2];
			}
		}
		ProgressBar progressBar2 = new ProgressBar();
		progressBar2 = (ProgressBar)statusBar1.Items.GetItemAt(6);
		Duration duration2 = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation2 = new DoubleAnimation(40.0, duration2);
		progressBar2.BeginAnimation(RangeBase.ValueProperty, animation2);
		ToolTip toolTip2 = new ToolTip();
		toolTip2.Content = "reading:40%";
		progressBar2.ToolTip = toolTip2;
		statusBar1.Items.Refresh();
		sPreCcfgReg.byRegisterNum = 4;
		sPreReg.byRegisterNum += sPreCcfgReg.byRegisterNum;
		for (num2 = 0; num2 < sPreCcfgReg.byRegisterNum; num2++)
		{
			switch (num2)
			{
			case 0:
				sPreCcfgReg.adwRegAdd[num2] = 4u;
				break;
			case 1:
				sPreCcfgReg.adwRegAdd[num2] = 6u;
				break;
			case 2:
				sPreCcfgReg.adwRegAdd[num2] = 2u;
				break;
			default:
				sPreCcfgReg.adwRegAdd[num2] = 3u;
				break;
			}
			sPreCcfgReg.adwRegVal[num2] = 0u;
		}
		byOptType = 4;
		byAccType = 19;
		if (!DllDebuggerAPI.DLL_GPLB32SetOptType(byOptType, byAccType))
		{
			return 2;
		}
		num = DllDebuggerAPI.DLL_GPLB32CmdPassThrough(0, ref sPreCcfgReg);
		if (num != 0)
		{
			return num;
		}
		num3 = 0u;
		for (num2 = 0; num2 < sPreCcfgReg.byRegisterNum; num2++)
		{
			num3 = (byte)sPreCcfgReg.adwRegAdd[num2];
			m_byREfuseArr[35 + num2] = (byte)sPreCcfgReg.adwRegVal[num2];
			sPreReg.adwRegAdd[35 + num2] = num3;
			sPreReg.adwRegVal[35 + num2] = (byte)sPreCcfgReg.adwRegVal[num2];
		}
		Thread.Sleep(10);
		dtEndTime = DateTime.Now;
		string text = dtEndTime.Subtract(dtBeginTime).ToString();
		string text2 = text.Substring(0, 11);
		TextBlock textBlock6 = new TextBlock();
		textBlock6.Text = text2;
		textBlock6 = (TextBlock)statusBar1.Items.GetItemAt(3);
		textBlock6.Text = text2;
		statusBar1.Items.Refresh();
		ProgressBar progressBar3 = new ProgressBar();
		progressBar3 = (ProgressBar)statusBar1.Items.GetItemAt(6);
		Duration duration3 = new Duration(TimeSpan.FromSeconds(1.0));
		DoubleAnimation animation3 = new DoubleAnimation(100.0, duration3);
		progressBar3.BeginAnimation(RangeBase.ValueProperty, animation3);
		ToolTip toolTip3 = new ToolTip();
		toolTip3.Content = "100%, read register completely";
		progressBar3.ToolTip = toolTip3;
		statusBar1.Items.Refresh();
		LogReadDataToTxt();
		RestoreReadThread();
		return num;
	}

	private void textBox_EBw_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_EBw.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 32768)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 32768.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 32768.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_EBw.Text = m_sMotorParamCfg.m_sFocParamCfg.wEbw.ToString();
			}
			else
			{
				textBox_EBw.Text = (string)FindResource("Cfg_FocEbwTxtBox");
			}
		}
	}

	public void textBox_AtoBw_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_AtoBw.Text;
		try
		{
			byte b = byte.Parse(text);
			if (b > 200)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 200.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 200.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_AtoBw.Text = m_sMotorParamCfg.m_sFocParamCfg.fAtoBw.ToString();
			}
			else
			{
				textBox_AtoBw.Text = (string)FindResource("Cfg_FocAtoBw");
			}
		}
	}

	public void textBox_StartCurrent_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_StartCurrent.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.001 || (double)num2 > 20.0)
			{
				MessageBox.Show("invalid value, please input float data between 0.001 and 20.000.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.001 and 20.000.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_StartCurrent.Text = m_sMotorParamCfg.m_sStartCfg.fStartCurrent.ToString();
			}
			else
			{
				textBox_StartCurrent.Text = (string)FindResource("Cfg_StartCurrent");
			}
		}
	}

	public void textBox_DQKp_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_DQKp.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.0 || (double)num2 > 4.0)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and 4.0");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.0 and 4.0");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_DQKp.Text = m_sMotorParamCfg.m_sRunCfg.fDqKp.ToString();
			}
			else
			{
				textBox_DQKp.Text = (string)FindResource("Cfg_RunDQKP");
			}
		}
	}

	public void textBox_DQKi_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_DQKi.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.0 || (double)num2 > 0.5)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and 0.500");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.0 and 0.500");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_DQKi.Text = m_sMotorParamCfg.m_sRunCfg.fDqKi.ToString();
			}
			else
			{
				textBox_DQKi.Text = (string)FindResource("Cfg_RunDQKI");
			}
		}
	}

	public void textBox_WKp_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_WKp.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.0 || (double)num2 > 4.0)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and 4.0");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.0 and 4.0");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_WKp.Text = m_sMotorParamCfg.m_sRunCfg.fWKp.ToString();
			}
			else
			{
				textBox_WKp.Text = (string)FindResource("Cfg_RunWKP");
			}
		}
	}

	public void textBox_WKi_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_WKi.Text;
		int num = 0;
		try
		{
			float num2 = float.Parse(text);
			if ((double)num2 < 0.0 || (double)num2 > 0.5)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and 0.500");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.0 and 0.500");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_WKi.Text = m_sMotorParamCfg.m_sRunCfg.fWKi.ToString();
			}
			else
			{
				textBox_WKi.Text = (string)FindResource("Cfg_RunWKI");
			}
		}
	}

	public void textBox_LockMinSpeed_LostFocus(object sender, RoutedEventArgs e)
	{
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_fMotorSpeedBase = 0f;
		CacluCurrenAndSpeedBase();
		int num = 0;
		uint num2 = (uint)(m_fMotorSpeedBase / 4f);
		string text = textBox_LockMinSpeed.Text;
		try
		{
			uint num3 = uint.Parse(text);
			if (num3 > num2)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and " + num2);
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and " + num2);
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_LockMinSpeed.Text = m_sMotorParamCfg.m_sProtectCfg.wLockMinSpeed.ToString();
			}
			else
			{
				textBox_LockMinSpeed.Text = (string)FindResource("Cfg_PtLockMinSpeed");
			}
		}
	}

	public void textBox_LockMaxSpeed_LostFocus(object sender, RoutedEventArgs e)
	{
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_fMotorSpeedBase = 0f;
		CacluCurrenAndSpeedBase();
		int num = 0;
		uint num2 = (uint)m_fMotorSpeedBase;
		string text = textBox_LockMaxSpeed.Text;
		try
		{
			uint num3 = uint.Parse(text);
			if (num3 > num2)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and " + num2);
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and " + num2);
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_LockMaxSpeed.Text = m_sMotorParamCfg.m_sProtectCfg.wLocMaxSpeed.ToString();
			}
			else
			{
				textBox_LockMaxSpeed.Text = (string)FindResource("Cfg_PtLockMaxSpeed");
			}
		}
	}

	private void textBox_LPNorhCur_LostFocus(object sender, RoutedEventArgs e)
	{
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_fMotorSpeedBase = 0f;
		CacluCurrenAndSpeedBase();
		int num = 0;
		float num2 = m_fMotorCurrentBase / 2f;
		string text = textBox_LPNorhCur.Text;
		try
		{
			float num3 = float.Parse(text);
			if (num3 > num2)
			{
				MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0.0 and " + num2);
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_LPNorhCur.Text = m_sMotorParamCfg.m_sProtectCfg.fLpNorhCurrent.ToString();
			}
			else
			{
				textBox_LPNorhCur.Text = (string)FindResource("Cfg_PtLPNorhCurrent");
			}
		}
	}

	private void textBox_LimitSpeed_LostFocus(object sender, RoutedEventArgs e)
	{
		m_fMotorVoltageBase = 0f;
		m_fMotorCurrentBase = 0f;
		m_fMotorSpeedBase = 0f;
		CacluCurrenAndSpeedBase();
		int num = 0;
		float fMotorCurrentBase = m_fMotorCurrentBase;
		uint num2 = (uint)m_fMotorSpeedBase;
		num2--;
		string text = textBox_LimitSpeed.Text;
		try
		{
			uint num3 = uint.Parse(text);
			if (num3 > num2)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and " + num2, (sender as TextBox).Text);
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and " + num2, (sender as TextBox).Text);
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_LimitSpeed.Text = m_sMotorParamCfg.m_sExtendCfg.wLimitSpeed.ToString();
			}
			else
			{
				textBox_LimitSpeed.Text = (string)FindResource("Cfg_RunLimitSpeed");
			}
		}
	}

	private void textBox_XON0_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_XON0.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 31)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_XON0.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byXON0.ToString();
			}
			else
			{
				textBox_XON0.Text = (string)FindResource("Cfg_CurveCfgXON0Def");
			}
		}
	}

	private void textBox_XON_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_XON.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 63)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 63.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 63.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_XON.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byX_ON.ToString();
			}
			else
			{
				textBox_XON.Text = (string)FindResource("Cfg_CurveCfgXONDef");
			}
		}
	}

	private void textBox_YON_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_YON.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 127)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_YON.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_ON.ToString();
			}
			else
			{
				textBox_YON.Text = (string)FindResource("Cfg_CurveCfgYONDef");
			}
		}
	}

	private void textBox_Y25_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_Y25.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 127)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_Y25.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_25.ToString();
			}
			else
			{
				textBox_Y25.Text = (string)FindResource("Cfg_CurveCfgY25Def");
			}
		}
	}

	private void textBox_Y50_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_Y50.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 127)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_Y50.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_50.ToString();
			}
			else
			{
				textBox_Y50.Text = (string)FindResource("Cfg_CurveCfgY50Def");
			}
		}
	}

	private void textBox_Y75_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_Y75.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 127)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 127.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_Y75.Text = m_sMotorParamCfg.m_sTargetSpdCurve.byY_75.ToString();
			}
			else
			{
				textBox_Y75.Text = (string)FindResource("Cfg_CurveCfgY75Def");
			}
		}
	}

	private void textBox_WOutMax_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_WOutMax.Text;
		try
		{
			ushort num2 = ushort.Parse(text);
			if (num2 > 63)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 63.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 63.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_WOutMax.Text = m_sMotorParamCfg.m_sRunCfg.byWoutMax.ToString();
			}
			else
			{
				textBox_WOutMax.Text = (string)FindResource("Cfg_RunWoutMaxDef");
			}
		}
	}

	private void textBox_SpdCtrl_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_SpdCtrl.Text;
		try
		{
			byte b = byte.Parse(text);
			if (b > 100)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 100.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 100.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_SpdCtrl.Text = m_sMotorParamCfg.m_sUserStatusInfoCfg.bySpdCtrl.ToString();
			}
			else
			{
				textBox_SpdCtrl.Text = (string)FindResource("Cfg_UserCfgSpdCtrlDef");
			}
		}
	}

	private void checkBox_DirSwitch_Checked(object sender, RoutedEventArgs e)
	{
		if (checkBox_DirSwitch.IsChecked == true)
		{
			m_sMotorParamCfg.m_sOnOffTestCfg.byDirSwitch = 1;
		}
		else
		{
			m_sMotorParamCfg.m_sOnOffTestCfg.byDirSwitch = 0;
		}
	}

	public void comboBox_RSDMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ComboBox comboBox = sender as ComboBox;
		byte b = 0;
		byte b2 = (byte)comboBox_HallMd.SelectedIndex;
		if (comboBox.SelectedIndex < 0)
		{
			return;
		}
		if (comboBox.SelectedValue != null)
		{
			string text = comboBox.SelectedValue.ToString();
			for (int i = 0; i < 4; i++)
			{
				string text2 = (string)FindResource("Cfg_WindRSDMode" + i);
				if (text == text2)
				{
					b = (byte)i;
					break;
				}
			}
		}
		else
		{
			b = (byte)comboBox.SelectedIndex;
		}
		m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode = b;
		if (b == 2 || b == 3)
		{
			radioBtn_ThresholdL.IsEnabled = true;
			radioBtn_ThresholdH.IsEnabled = true;
			slider_IsdMd2Time.IsEnabled = true;
			textBlock_IsdMd2Time.IsEnabled = true;
		}
		else
		{
			radioBtn_ThresholdL.IsEnabled = false;
			radioBtn_ThresholdH.IsEnabled = false;
			slider_IsdMd2Time.IsEnabled = false;
			textBlock_IsdMd2Time.IsEnabled = false;
			slider_IsdMd2Time.Value = 0.0;
		}
	}

	public void slider_AlignDuty_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		byte byAlignDuty = (byte)slider_AlignDuty.Value;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignDuty = byAlignDuty;
		HandleAlignDutyDisplay(byAlignDuty);
	}

	public void slider_AlignTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		byte byAlignTime = (byte)slider_AlignTime.Value;
		m_sMotorParamCfg.m_sIpdAlignCfg.byAlignTime = byAlignTime;
		HandleAlignTimeDisplay(byAlignTime);
	}

	private void radioBtn_HallCorMdDis_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd = 0;
		if (!textBox_LockMinSpeed.IsEnabled)
		{
			ShowLockMinSpeedValue();
		}
	}

	private void radioBtn_HallCorMdEn_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd = 1;
		if (!textBox_LockMinSpeed.IsEnabled)
		{
			ShowLockMinSpeedValue();
		}
	}

	public void slider_HallLockSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		float num = (float)slider_HallLockSel.Value;
		m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel = num;
		if (!textBox_LockMinSpeed.IsEnabled)
		{
			ShowLockMinSpeedValue();
		}
	}

	public void slider_HallSwiMd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallSwiMd.Value;
		m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd = (byte)num;
		textBlock_HallSwiMd.Text = m_clsDlgItemSet45.GetHallSwiStrByIdx(m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd);
	}

	private void slider_HallLa0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa0.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa125_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa125.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa250_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa250.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa375_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa375.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa500_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa500.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa625_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa625.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa750_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa750.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa875_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa875.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875 = (byte)num;
		HandleLACurveShow();
	}

	private void slider_HallLa999_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_HallLa999.Value;
		m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999 = (byte)num;
		HandleLACurveShow();
	}

	private void Grid_MouseMove(object sender, MouseEventArgs e)
	{
		if (m_hallLaCurveDpl == null)
		{
			return;
		}
		Point position = e.GetPosition(contentCtrl_HallLaCurve);
		PointCollection curvePoints = m_hallLaCurveDpl.GetCurvePoints();
		double num = 0.0;
		double num2 = 0.0;
		for (int i = 0; i < m_hallLaCurveDataSource.Count - 1; i++)
		{
			Point point = curvePoints[i];
			Point point2 = curvePoints[i + 1];
			if (position.X == point.X)
			{
				num = m_hallLaCurveDataSource[i].X;
				num2 = m_hallLaCurveDataSource[i].Y;
				break;
			}
			if (position.X == point2.X)
			{
				num = m_hallLaCurveDataSource[i + 1].X;
				num2 = m_hallLaCurveDataSource[i + 1].Y;
				break;
			}
			if (position.X > point.X && position.X < point2.X)
			{
				double num3 = Math.Sqrt(Math.Pow(point2.X - point.X, 2.0) + Math.Pow(point2.Y - point.Y, 2.0));
				double num4 = Math.Sqrt(Math.Pow(m_hallLaCurveDataSource[i + 1].X - m_hallLaCurveDataSource[i].X, 2.0) + Math.Pow(m_hallLaCurveDataSource[i + 1].Y - m_hallLaCurveDataSource[i].Y, 2.0));
				num = (position.X - point.X) / num3 * num4;
				num += m_hallLaCurveDataSource[i].X;
				num2 = PointYAtOneLine(m_hallLaCurveDataSource[i], m_hallLaCurveDataSource[i + 1], num);
				break;
			}
		}
		textBlock_HallLaCurvePosition.Text = $"X: {num:F1}, Y: {num2:F3}";
	}

	private void HandleLACurveShow()
	{
		if (m_sMotorParamCfg == null)
		{
			return;
		}
		int num = 9;
		float[] array = new float[num];
		float[] array2 = new float[num];
		float[] array3 = new float[num];
		array2[0] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0;
		array2[1] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125;
		array2[2] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250;
		array2[3] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375;
		array2[4] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500;
		array2[5] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625;
		array2[6] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750;
		array2[7] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875;
		array2[8] = (int)m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999;
		for (int i = 0; i < num; i++)
		{
			array[i] = (float)((double)i * 12.5);
			array3[i] = (float)((double)array2[i] / 16.0 * 11.25);
		}
		for (int i = 0; i < array2.Length; i++)
		{
			if (i == 0)
			{
				array2[i] = array3[i];
			}
			else
			{
				array2[i] = array2[i - 1] + array3[i];
			}
		}
		PointCollection pointCollection = new PointCollection();
		for (int i = 0; i < num; i++)
		{
			Point value = new Point(array[i], array2[i]);
			pointCollection.Add(value);
		}
		m_hallLaCurveDpl = new DrawPolyLine
		{
			DataSource = pointCollection,
			XUnit = "%",
			YUnit = "°",
			PolyPen = new Pen(Brushes.Yellow, 1.0),
			DashPen = new Pen(Brushes.LightGray, 1.0),
			AxisPen = new Pen(Brushes.Yellow, 1.0),
			TextColor = Brushes.LightBlue
		};
		contentCtrl_HallLaCurve.Content = m_hallLaCurveDpl;
		m_hallLaCurveDataSource = pointCollection.Clone();
	}

	public void textBox_MfpEk3_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		float num2 = 0f;
		string text = textBox_MfpEk3.Text;
		try
		{
			num2 = float.Parse(text);
			if ((double)num2 > 100.0 || (double)num2 < 0.01)
			{
				MessageBox.Show("invalid value, please input decimal float between 0.01 and 100.0.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal float between 0.01 and 100.0.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_MfpEk3.Text = m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3.ToString();
			}
			else
			{
				textBox_MfpEk3.Text = (string)FindResource("Cfg_FocMfpEk3");
			}
		}
		m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3 = num2;
	}

	private void slider_PwrAmpSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	public void slider_LimitSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel = (byte)slider_LimitSel.Value;
	}

	private void slider_IsdMd2Time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_IsdMd2Time.Value;
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time = (byte)num;
	}

	private void slider_TsdSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	public void slider_OvpThr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_OvpThr.Value;
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr = (byte)num;
		ShowOvpThrText();
	}

	private void ShowOvpThrText()
	{
		byte byOvpThr = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr;
		float fRV = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV;
		float adcRefVoltage = GetAdcRefVoltage(m_sMotorParamCfg.m_sExtendCfg.byVRefSel);
		float num = 0f;
		num = ((byOvpThr != 0) ? ((float)((double)((byOvpThr + 1) * 1024) / 32768.0 * (double)adcRefVoltage * (double)fRV)) : ((float)((double)(byOvpThr * 1024) / 32768.0 * (double)adcRefVoltage * (double)fRV)));
		textBox_OvpThr.Text = num.ToString("0.000");
	}

	public void slider_UvpThr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_UvpThr.Value;
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr = (byte)num;
		ShowUvpThrText();
	}

	private void ShowUvpThrText()
	{
		byte byUvpThr = m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr;
		float fRV = m_sMotorParamCfg.m_sMotorParamHwCfg.fRV;
		float adcRefVoltage = GetAdcRefVoltage(m_sMotorParamCfg.m_sExtendCfg.byVRefSel);
		float num = (float)((double)(byUvpThr * 1024) / 32768.0 * (double)adcRefVoltage * (double)fRV);
		textBox_UvpThr.Text = num.ToString("0.000");
	}

	private void slider_Cmp0CkSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	public void slider_TheCompSel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_TheCompSel.Value;
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel = (byte)num;
	}

	private void slider_LimitITrip_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		ushort num = (ushort)slider_LimitITrip.Value;
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip = (byte)num;
		UpdateLimitItrip((byte)num);
	}

	private void UpdateLimitItrip(byte value)
	{
		if (m_i16SensorMode != 1)
		{
			return;
		}
		if (m_nPreviousValueDict.Keys.Contains("LimtiItrip_SVPWM"))
		{
			if (radioBtn_HallSvMdEn.IsChecked == true)
			{
				m_nPreviousValueDict["LimtiItrip_SVPWM"] = value;
			}
		}
		else
		{
			m_nPreviousValueDict.Add("LimtiItrip_SVPWM", m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip);
		}
	}

	public void comboBox_HallMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		byte b = 0;
		if (comboBox_HallMode.SelectedValue == null)
		{
			return;
		}
		string text = comboBox_HallMode.SelectedValue.ToString();
		if (m_i16SensorMode == 1 || m_i16SensorMode == 2)
		{
			switch (text)
			{
			case "Disable":
				b = 0;
				break;
			case "3 hall 120":
				b = 1;
				break;
			case "1 hall":
				b = 2;
				break;
			case "2 hall 120":
				b = 3;
				break;
			case "2 hall 90":
				b = 4;
				break;
			}
		}
		else
		{
			b = (byte)comboBox_HallMode.SelectedIndex;
		}
		switch (text)
		{
		case "1 hall":
			radioBtn_HallPllEn.IsChecked = true;
			radioBtn_HallPllDis.IsEnabled = false;
			radioBtn_HallPllEn.IsEnabled = false;
			ShowLockMinSpeedValue();
			textBox_LockMinSpeed.IsEnabled = false;
			break;
		case "2 hall 120":
			radioBtn_HallPllEn.IsChecked = true;
			radioBtn_HallPllDis.IsEnabled = false;
			radioBtn_HallPllEn.IsEnabled = false;
			break;
		case "2 hall 90":
			radioBtn_HallPllEn.IsChecked = true;
			radioBtn_HallPllDis.IsEnabled = false;
			radioBtn_HallPllEn.IsEnabled = false;
			break;
		}
		if (b > 1)
		{
			m_sMotorParamCfg.m_sHallParamCfg.byHallMd = (byte)(b * 2 - 1);
		}
		else
		{
			m_sMotorParamCfg.m_sHallParamCfg.byHallMd = b;
		}
		if (m_sMotorParamCfg.m_sHallParamCfg.byHallMd == 0)
		{
			EnableLock0(isEnabled: true);
			EnableLock1(isEnabled: true);
			EnableLock2(isEnabled: true);
			EnableLockMinSpeed(isEnabled: true);
		}
		else
		{
			radioBtn_Lock1Dis.IsChecked = true;
			radioBtn_Lock2Dis.IsChecked = true;
			EnableLock1(isEnabled: false);
			EnableLock2(isEnabled: false);
			ShowLockMinSpeedValue();
			EnableLockMinSpeed(isEnabled: false);
		}
	}

	public void comboBox_HallFgMd_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_HallFgMd.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd = (byte)comboBox_HallFgMd.SelectedIndex;
			byte byHallFgMd = m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd;
		}
	}

	public void comboBox_CmpModeSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboBox_CmpModeSel.SelectedIndex >= 0)
		{
			m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel = (byte)comboBox_CmpModeSel.SelectedIndex;
			byte byCmp0ModSel = m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel;
		}
	}

	private void radioBtn_HallStaMdDis_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void radioBtn_HallStaMdEn_Checked(object sender, RoutedEventArgs e)
	{
		EnableLock0(isEnabled: true);
	}

	private void radioBtn_HallSvMdDis_Checked(object sender, RoutedEventArgs e)
	{
		textBox_HaDu.IsEnabled = false;
		textBox_HaTi.IsEnabled = false;
		textBox_HallAlignSpeed.IsEnabled = false;
		EnableHallStaMd(isEnabled: true);
		EnableCurveSpdDis(isEnabled: true);
		if (m_i16SensorMode == 1)
		{
			int value = 0;
			if (m_nPreviousValueDict.TryGetValue("WOutMax_FOC", out value))
			{
				slider_WOutMax.Value = value;
			}
			else
			{
				slider_WOutMax.Value = (int)m_sMotorParamCfg.m_sRunCfg.byWoutMax;
			}
			slider_WOutMax.IsEnabled = true;
			textBox_WOutMax.IsEnabled = true;
			slider_LimitITrip.Value = 0.0;
			slider_LimitITrip.IsEnabled = false;
			textBox_LimitITrip.IsEnabled = false;
			AddSpdCtrlModeAllItems();
			EnablePllSmo(isEnabled: true);
			EnableAoEn(isEnabled: true);
			textBox_MfpEk3.IsEnabled = true;
			EnableStaPll(isEnabled: true);
			EnableSinAtan(isEnabled: true);
			EnableStaSin(isEnabled: true);
			textBox_AtoBw.IsEnabled = true;
			slider_SampleTime.IsEnabled = true;
			slider_EBWMode.IsEnabled = true;
			slider_LeadAngle.IsEnabled = true;
			slider_EKNum.IsEnabled = true;
			slider_TSMIN.IsEnabled = true;
			radioBtn_LossPhaseDis.IsEnabled = true;
			radioBtn_LossPhaseEn.IsEnabled = true;
			slider_TheCompSel.IsEnabled = true;
			AddControlModeAllItems();
			slider_HallLa0.Value = 0.0;
			slider_HallLa0.IsEnabled = false;
			slider_HallLa125.Value = 0.0;
			slider_HallLa125.IsEnabled = false;
			slider_HallLa250.Value = 0.0;
			slider_HallLa250.IsEnabled = false;
			slider_HallLa375.Value = 0.0;
			slider_HallLa375.IsEnabled = false;
			slider_HallLa500.Value = 0.0;
			slider_HallLa500.IsEnabled = false;
			slider_HallLa625.Value = 0.0;
			slider_HallLa625.IsEnabled = false;
			slider_HallLa750.Value = 0.0;
			slider_HallLa750.IsEnabled = false;
			slider_HallLa875.Value = 0.0;
			slider_HallLa875.IsEnabled = false;
			slider_HallLa999.Value = 0.0;
			slider_HallLa999.IsEnabled = false;
		}
		else if (m_i16SensorMode == 2)
		{
			slider_LimitITrip.Value = 0.0;
			slider_LimitITrip.IsEnabled = false;
			textBox_LimitITrip.IsEnabled = false;
		}
		if (radioBtn_HallStaOpEn.IsChecked == true)
		{
			label25.Visibility = Visibility.Hidden;
			textBox_StartCurrent.Visibility = Visibility.Hidden;
			label_StartDuty30.Visibility = Visibility.Visible;
			textBox_StartDuty30.Visibility = Visibility.Visible;
			label_StartDutyUnit.Visibility = Visibility.Visible;
		}
		else
		{
			label25.Visibility = Visibility.Visible;
			textBox_StartCurrent.Visibility = Visibility.Visible;
			label_StartDuty30.Visibility = Visibility.Hidden;
			textBox_StartDuty30.Visibility = Visibility.Hidden;
			label_StartDutyUnit.Visibility = Visibility.Hidden;
		}
	}

	private void radioBtn_HallSvMdEn_Checked(object sender, RoutedEventArgs e)
	{
		textBox_HaDu.IsEnabled = true;
		textBox_HaTi.IsEnabled = true;
		textBox_HallAlignSpeed.IsEnabled = true;
		radioBtn_HallStaMdDis.IsChecked = true;
		EnableHallStaMd(isEnabled: false);
		if (comboBox_ContrlMode.SelectedValue.ToString() == "VoltageLoop")
		{
			EnableCurveSpdDis(isEnabled: true);
		}
		else
		{
			CheckedeCurveSpdDis(0);
			EnableCurveSpdDis(isEnabled: false);
		}
		if (radioBtn_HallStaOpEn.IsChecked == true || radioBtn_HallSvMdEn.IsChecked == true)
		{
			label25.Visibility = Visibility.Hidden;
			textBox_StartCurrent.Visibility = Visibility.Hidden;
			label_StartDuty30.Visibility = Visibility.Visible;
			textBox_StartDuty30.Visibility = Visibility.Visible;
			label_StartDutyUnit.Visibility = Visibility.Visible;
		}
		if (m_i16SensorMode == 1)
		{
			slider_WOutMax.Value = 0.0;
			slider_WOutMax.IsEnabled = false;
			textBox_WOutMax.IsEnabled = false;
			int value = 0;
			if (m_nPreviousValueDict.TryGetValue("LimtiItrip_SVPWM", out value))
			{
				slider_LimitITrip.Value = value;
			}
			else
			{
				slider_LimitITrip.Value = (int)m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip;
			}
			slider_LimitITrip.IsEnabled = true;
			textBox_LimitITrip.IsEnabled = true;
			SetSpdCtrlModeSelectedIndex(m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd);
			radioButton_PllSmo_En.IsChecked = true;
			EnablePllSmo(isEnabled: false);
			radioBtn_MfpDis.IsChecked = true;
			EnableAoEn(isEnabled: false);
			textBox_MfpEk3.Text = "2.5";
			textBox_MfpEk3.IsEnabled = false;
			radioButton_StaPll_Dis.IsChecked = true;
			EnableStaPll(isEnabled: false);
			radioButton_Atan_En.IsChecked = true;
			EnableSinAtan(isEnabled: false);
			radioButton_StaSin_Dis.IsChecked = true;
			EnableStaSin(isEnabled: false);
			textBox_EBw.Text = "3200";
			textBox_EBw.IsEnabled = false;
			textBox_AtoBw.Text = "50";
			textBox_AtoBw.IsEnabled = false;
			slider_SampleTime.Value = 0.0;
			slider_SampleTime.IsEnabled = false;
			slider_EBWMode.Value = 0.0;
			slider_EBWMode.IsEnabled = false;
			slider_LeadAngle.Value = 0.0;
			slider_LeadAngle.IsEnabled = false;
			slider_EKNum.Value = 0.0;
			slider_EKNum.IsEnabled = false;
			slider_TSMIN.Value = 0.0;
			slider_TSMIN.IsEnabled = false;
			radioBtn_LossPhaseDis.IsChecked = true;
			radioBtn_LossPhaseDis.IsEnabled = false;
			radioBtn_LossPhaseEn.IsEnabled = false;
			slider_TheCompSel.Value = 0.0;
			slider_TheCompSel.IsEnabled = false;
			comboBox_ContrlMode.Items.Remove((string)FindResource("Cfg_RunControlMode1"));
			comboBox_ContrlMode.Items.Remove((string)FindResource("Cfg_RunControlMode3"));
			SetControlModeSelectedIndex(m_sMotorParamCfg.m_sStartCfg.byControlMode);
			slider_HallLa0.IsEnabled = true;
			slider_HallLa125.IsEnabled = true;
			slider_HallLa250.IsEnabled = true;
			slider_HallLa375.IsEnabled = true;
			slider_HallLa500.IsEnabled = true;
			slider_HallLa625.IsEnabled = true;
			slider_HallLa750.IsEnabled = true;
			slider_HallLa875.IsEnabled = true;
			slider_HallLa999.IsEnabled = true;
		}
	}

	private void radioBtn_RvsAlignEn0_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn = 0;
		HandleAlignDisplay(m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn);
	}

	private void radioBtn_RvsAlignEn1_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sIpdAlignCfg.byRvsAlignEn = 1;
		HandleAlignDisplay(m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn);
	}

	private void menuItem_CRCAddrStartAt86_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void menuItem_CRCAddrStartAt88_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void menuItem_CRCAddr_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (!m_isAuthorize)
		{
			AuthorizeWindow authorizeWindow = new AuthorizeWindow();
			if (authorizeWindow.ShowDialog() == true)
			{
				m_isAuthorize = true;
			}
		}
	}

	private void textBox_HaDu_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		byte b = 0;
		string text = textBox_HaDu.Text;
		try
		{
			b = byte.Parse(text);
			if (b > 31)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_HaDu.Text = m_sMotorParamCfg.m_sHallParamCfg.byHaDu.ToString();
			}
			else
			{
				textBox_HaDu.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sHallParamCfg.byHaDu = b;
	}

	private void textBox_HaTi_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		byte b = 0;
		string text = textBox_HaTi.Text;
		try
		{
			b = byte.Parse(text);
			if (b > 31)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_HaTi.Text = m_sMotorParamCfg.m_sHallParamCfg.byHaTi.ToString();
			}
			else
			{
				textBox_HaTi.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sHallParamCfg.byHaTi = b;
	}

	private void textBox_HallAlignSpeed_LostFocus(object sender, RoutedEventArgs e)
	{
		int num = 0;
		byte b = 0;
		string text = textBox_HallAlignSpeed.Text;
		try
		{
			b = byte.Parse(text);
			if (b > 31)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 31.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_HallAlignSpeed.Text = m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed.ToString();
			}
			else
			{
				textBox_HallAlignSpeed.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed = b;
	}

	public void textBox_OcpDa_LostFocus(object sender, RoutedEventArgs e)
	{
		CheckOcpDaInputValid();
	}

	public void textBox_OcpDa_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			CheckOcpDaInputValid();
		}
	}

	public void CheckOcpDaInputValid()
	{
		int num = 0;
		byte b = 0;
		string text = textBox_OcpDa.Text;
		try
		{
			b = byte.Parse(text);
			if (b > byte.MaxValue)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_OcpDa.Text = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa.ToString();
			}
			else
			{
				textBox_OcpDa.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa = b;
		ShowOcpDaVoltage();
	}

	public void ShowOcpDaVoltage()
	{
		byte byOcpDa = m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa;
		double num = (double)(int)byOcpDa / 255.0 * 5.0;
		Span span = new Span();
		span.FontSize = 10.0;
		Span span2 = span;
		span2.Inlines.Add(new Run("voltage: ")
		{
			Foreground = Brushes.Gray
		});
		span2.Inlines.Add(new Run(num.ToString("0.000"))
		{
			Foreground = Brushes.Chocolate
		});
		span2.Inlines.Add(new Run(" v")
		{
			Foreground = Brushes.Gray
		});
		textBlock_OcpDa_Voltage.Inlines.Clear();
		textBlock_OcpDa_Voltage.Inlines.Add(span2);
		if (m_SensorlessUC != null)
		{
			m_SensorlessUC.textBlock_OcpDa_Voltage.Inlines.Clear();
			m_SensorlessUC.textBlock_OcpDa_Voltage.Inlines.Add(span2);
		}
	}

	private void textBox_SpdBlk0_LostFocus(object sender, RoutedEventArgs e)
	{
		CheckSpdBlk0InputValid();
	}

	private void textBox_SpdBlk0_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			CheckSpdBlk0InputValid();
		}
	}

	private void CheckSpdBlk0InputValid()
	{
		int num = 0;
		byte b = 0;
		string text = textBox_SpdBlk0.Text;
		try
		{
			b = byte.Parse(text);
			if (b > byte.MaxValue)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_SpdBlk0.Text = m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0.ToString();
			}
			else
			{
				textBox_SpdBlk0.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0 = b;
		ShowSpdBlk0Speed();
	}

	public void ShowSpdBlk0Speed()
	{
		double num = (double)((m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0 + 1) * 64 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		double num2 = (double)(m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0 * 64 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		double num3 = num * 60.0 / (double)(int)m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		double num4 = num2 * 60.0 / (double)(int)m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		Span span = new Span();
		span.FontSize = 10.0;
		Span span2 = span;
		Span span3 = new Span();
		span3.FontSize = 10.0;
		Span span4 = span3;
		Run run = new Run(num3.ToString("0"));
		Run run2 = new Run(num4.ToString("0"));
		run.Foreground = Brushes.Chocolate;
		run2.Foreground = Brushes.Chocolate;
		span2.Inlines.Add(run);
		span2.Inlines.Add(new Run(" rpm, ")
		{
			Foreground = Brushes.Gray
		});
		span4.Inlines.Add(run2);
		span4.Inlines.Add(new Run(" rpm")
		{
			Foreground = Brushes.Gray
		});
		textBlock_SpdBlk0_InSpeed.Inlines.Clear();
		textBlock_SpdBlk0_InSpeed.Inlines.Add(span2);
		textBlock_SpdBlk0_OutSpeed.Inlines.Clear();
		textBlock_SpdBlk0_OutSpeed.Inlines.Add(span4);
	}

	private void textBox_SpdBlk1_LostFocus(object sender, RoutedEventArgs e)
	{
		CheckSpdBlk1InputValid();
	}

	private void textBox_SpdBlk1_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			CheckSpdBlk1InputValid();
		}
	}

	private void CheckSpdBlk1InputValid()
	{
		int num = 0;
		byte b = 0;
		string text = textBox_SpdBlk1.Text;
		try
		{
			b = byte.Parse(text);
			if (b > byte.MaxValue)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_SpdBlk1.Text = m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1.ToString();
			}
			else
			{
				textBox_SpdBlk1.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1 = b;
		ShowSpdBlk1Speed();
	}

	public void ShowSpdBlk1Speed()
	{
		double num = (double)((m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1 + 1) * 64 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		double num2 = (double)(m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1 * 64 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		double num3 = num * 60.0 / (double)(int)m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		double num4 = num2 * 60.0 / (double)(int)m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		Span span = new Span();
		span.FontSize = 10.0;
		Span span2 = span;
		Span span3 = new Span();
		span3.FontSize = 10.0;
		Span span4 = span3;
		Run run = new Run(num3.ToString("0"));
		Run run2 = new Run(num4.ToString("0"));
		run.Foreground = Brushes.Chocolate;
		run2.Foreground = Brushes.Chocolate;
		span2.Inlines.Add(run);
		span2.Inlines.Add(new Run(" rpm, ")
		{
			Foreground = Brushes.Gray
		});
		span4.Inlines.Add(run2);
		span4.Inlines.Add(new Run(" rpm")
		{
			Foreground = Brushes.Gray
		});
		textBlock_SpdBlk1_InSpeed.Inlines.Clear();
		textBlock_SpdBlk1_InSpeed.Inlines.Add(span2);
		textBlock_SpdBlk1_OutSpeed.Inlines.Clear();
		textBlock_SpdBlk1_OutSpeed.Inlines.Add(span4);
	}

	private void textBox_SpdBlk2_LostFocus(object sender, RoutedEventArgs e)
	{
		CheckSpdBlk2InputValid();
	}

	private void textBox_SpdBlk2_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			CheckSpdBlk2InputValid();
		}
	}

	private void CheckSpdBlk2InputValid()
	{
		int num = 0;
		byte b = 0;
		string text = textBox_SpdBlk2.Text;
		try
		{
			b = byte.Parse(text);
			if (b > byte.MaxValue)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 255.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_SpdBlk2.Text = m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2.ToString();
			}
			else
			{
				textBox_SpdBlk2.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2 = b;
		ShowSpdBlk2Speed();
	}

	public void ShowSpdBlk2Speed()
	{
		double num = (double)((m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2 + 1) * 64 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		double num2 = (double)(m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2 * 64 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		double num3 = num * 60.0 / (double)(int)m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		double num4 = num2 * 60.0 / (double)(int)m_sMotorParamCfg.m_sMotorParamHwCfg.byJiDuiShu;
		Span span = new Span();
		span.FontSize = 10.0;
		Span span2 = span;
		Span span3 = new Span();
		span3.FontSize = 10.0;
		Span span4 = span3;
		Run run = new Run(num3.ToString("0"));
		Run run2 = new Run(num4.ToString("0"));
		run.Foreground = Brushes.Chocolate;
		run2.Foreground = Brushes.Chocolate;
		span2.Inlines.Add(run);
		span2.Inlines.Add(new Run(" rpm, ")
		{
			Foreground = Brushes.Gray
		});
		span4.Inlines.Add(run2);
		span4.Inlines.Add(new Run(" rpm")
		{
			Foreground = Brushes.Gray
		});
		textBlock_SpdBlk2_InSpeed.Inlines.Clear();
		textBlock_SpdBlk2_InSpeed.Inlines.Add(span2);
		textBlock_SpdBlk2_OutSpeed.Inlines.Clear();
		textBlock_SpdBlk2_OutSpeed.Inlines.Add(span4);
	}

	private void textBox_SpdBlkWid_LostFocus(object sender, RoutedEventArgs e)
	{
		CheckSpdBlkWidInputValid();
	}

	private void textBox_SpdBlkWid_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			CheckSpdBlkWidInputValid();
		}
	}

	private void CheckSpdBlkWidInputValid()
	{
		int num = 0;
		byte b = 0;
		string text = textBox_SpdBlkWid.Text;
		try
		{
			b = byte.Parse(text);
			if (b > 15)
			{
				MessageBox.Show("invalid value, please input decimal integer between 0 and 15.");
				num = 1;
			}
		}
		catch
		{
			MessageBox.Show("invalid value, please input decimal integer between 0 and 15.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_SpdBlkWid.Text = m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid.ToString();
			}
			else
			{
				textBox_SpdBlkWid.Text = "0";
			}
		}
		m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid = b;
		ShowSpdBlkWidSpeed();
	}

	public void ShowSpdBlkWidSpeed()
	{
		double num = (double)(m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid * 32 * m_sMotorParamCfg.m_sFocParamCfg.wFreqBase) / 32768.0;
		Span span = new Span();
		span.FontSize = 10.0;
		Span span2 = span;
		Run run = new Run(num.ToString("0"));
		run.Foreground = Brushes.Chocolate;
		span2.Inlines.Add(run);
		span2.Inlines.Add(new Run(" rpm")
		{
			Foreground = Brushes.Gray
		});
		textBlock_SpdBlkWid_Speed.Inlines.Clear();
		textBlock_SpdBlkWid_Speed.Inlines.Add(span2);
	}

	private void ShowLockMinSpeedValue()
	{
		float fHallLockSel = m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel;
		byte byHallCorMd = m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd;
		float motorSpeedBase = GetMotorSpeedBase();
		textBox_LockMinSpeed.Text = ((double)(fHallLockSel + (float)(int)byHallCorMd) * 0.937 * (double)motorSpeedBase).ToString("0");
	}

	private bool AddSpdCtrlModeAllItems()
	{
		if (comboBox_SpdCtrlMD.Items.Count == 4)
		{
			return false;
		}
		comboBox_SpdCtrlMD.Items.Clear();
		comboBox_SpdCtrlMD.Items.Add((string)FindResource("Cfg_OtherSpdCtrlMD0"));
		comboBox_SpdCtrlMD.Items.Add((string)FindResource("Cfg_OtherSpdCtrlMD1"));
		comboBox_SpdCtrlMD.Items.Add((string)FindResource("Cfg_OtherSpdCtrlMD2"));
		comboBox_SpdCtrlMD.Items.Add((string)FindResource("Cfg_OtherSpdCtrlMD3"));
		SetSpdCtrlModeSelectedIndex(m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd);
		return true;
	}

	private bool AddControlModeAllItems()
	{
		if (comboBox_ContrlMode.Items.Count == 4)
		{
			return false;
		}
		comboBox_ContrlMode.Items.Clear();
		comboBox_ContrlMode.Items.Add((string)FindResource("Cfg_RunControlMode0"));
		comboBox_ContrlMode.Items.Add((string)FindResource("Cfg_RunControlMode1"));
		comboBox_ContrlMode.Items.Add((string)FindResource("Cfg_RunControlMode2"));
		comboBox_ContrlMode.Items.Add((string)FindResource("Cfg_RunControlMode3"));
		SetControlModeSelectedIndex(m_sMotorParamCfg.m_sStartCfg.byControlMode);
		return true;
	}

	private void CircuitTypeParamDisplay(short sensorMode, byte CircuitType)
	{
		if (m_i16SensorMode == 4)
		{
			tabControl1.Items.Clear();
			TabItem tabItem = new TabItem();
			tabItem.Header = "Sensorless Function";
			tabItem.Background = Brushes.LightSeaGreen;
			tabItem.Width = 150.0;
			TabItem tabItem2 = tabItem;
			m_SensorlessUC = new SensorlessUC();
			tabItem2.Content = m_SensorlessUC;
			tabControl1.Items.Insert(0, tabItem2);
			tabControl1.SelectedIndex = 0;
			tabItem_Curve.Width = 150.0;
			tabControl1.Items.Insert(1, tabItem_Curve);
			tabItem_Display.Width = 150.0;
			tabControl1.Items.Insert(2, tabItem_Display);
			tabItem_Download.Width = 150.0;
			tabControl1.Items.Insert(3, tabItem_Download);
			ShowTreeView_RoboticVacuum();
		}
		else if (m_i16SensorMode == 3)
		{
			tabControl1.Items.Clear();
			TabItem tabItem3 = new TabItem();
			tabItem3.Header = "Sensor & Sensorless";
			tabItem3.Background = Brushes.LightSeaGreen;
			tabItem3.Width = 160.0;
			TabItem tabItem2 = tabItem3;
			m_uc2HallICCoolFan_s1 = new uc_2HallICCoolFan_s1(this);
			tabItem2.Content = m_uc2HallICCoolFan_s1;
			tabControl1.Items.Insert(0, tabItem2);
			TabItem tabItem4 = new TabItem();
			tabItem4.Header = "Function & Protect & Other";
			tabItem4.Background = Brushes.LightSeaGreen;
			TabItem tabItem5 = tabItem4;
			m_uc2HallICCoolFan_s2 = new uc_2HallICCoolFan_s2(this);
			m_uc2HallICCoolFan_s2.adWindow = adWindow;
			tabItem5.Content = m_uc2HallICCoolFan_s2;
			tabControl1.Items.Insert(1, tabItem5);
			tabControl1.SelectedIndex = 0;
			tabItem_Curve.Width = 160.0;
			tabControl1.Items.Insert(2, tabItem_Curve);
			tabItem_Display.Width = 160.0;
			tabControl1.Items.Insert(3, tabItem_Display);
			tabItem_Download.Width = 160.0;
			tabControl1.Items.Insert(4, tabItem_Download);
			ShowTreeView_2HIC_CoolFan();
			ShowHallComMd(isVisible: true);
			m_uc2HallICCoolFan_s2.comboBox_CmpModeSel.IsEnabled = false;
			if (m_byCircuitType == 0)
			{
				comboBox_SMPMode.SelectedIndex = 0;
			}
			else
			{
				comboBox_SMPMode.SelectedIndex = 1;
			}
		}
		else if ((Application.Current as App).eArgsLen > 1 && m_i16SensorMode == 2)
		{
			tabControl1.Items.Clear();
			TabItem tabItem6 = new TabItem();
			tabItem6.Header = "Sensorless Function";
			tabItem6.Background = Brushes.LightSeaGreen;
			tabItem6.Width = 150.0;
			TabItem tabItem2 = tabItem6;
			m_2Res_FOC_UVW_ASUC = new UC_2Res_FOC_UVW_AS();
			tabItem2.Content = m_2Res_FOC_UVW_ASUC;
			tabControl1.Items.Insert(0, tabItem2);
			tabControl1.SelectedIndex = 0;
			tabItem_Curve.Width = 150.0;
			tabControl1.Items.Insert(1, tabItem_Curve);
			tabItem_Display.Width = 150.0;
			tabControl1.Items.Insert(2, tabItem_Display);
			tabItem_Download.Width = 150.0;
			tabControl1.Items.Insert(3, tabItem_Download);
			ShowTreeView_2Res_Foc_UVW_AS();
			label_XON0.Visibility = Visibility.Hidden;
			slider_XON0.Visibility = Visibility.Hidden;
			slider_XON0.IsEnabled = false;
			textBox_XON0.Visibility = Visibility.Hidden;
			textBox_XON0.IsEnabled = false;
			textBlock_XON0.Visibility = Visibility.Hidden;
			textBlock_XON0.IsEnabled = false;
			label_XON0_Percent.Visibility = Visibility.Hidden;
		}
		else if ((Application.Current as App).eArgsLen > 1)
		{
			tabControl1.Items.Clear();
			TabItem tabItem7 = new TabItem();
			tabItem7.Header = "Sensor Function";
			tabItem7.Background = Brushes.LightSeaGreen;
			tabItem7.Width = 150.0;
			TabItem tabItem2 = tabItem7;
			m_2Hall_sensor_2Res_FOC_ASUC = new UC_2Hall_sensor_2Res_FOC_AS();
			tabItem2.Content = m_2Hall_sensor_2Res_FOC_ASUC;
			tabControl1.Items.Insert(0, tabItem2);
			tabControl1.SelectedIndex = 0;
			tabItem_Curve.Width = 150.0;
			tabControl1.Items.Insert(1, tabItem_Curve);
			tabItem_Display.Width = 150.0;
			tabControl1.Items.Insert(2, tabItem_Display);
			tabItem_Download.Width = 150.0;
			tabControl1.Items.Insert(3, tabItem_Download);
			ShowTreeView_2HallSensor_2Res_Foc_AS();
			label_XON0.Visibility = Visibility.Hidden;
			slider_XON0.Visibility = Visibility.Hidden;
			slider_XON0.IsEnabled = false;
			textBox_XON0.Visibility = Visibility.Hidden;
			textBox_XON0.IsEnabled = false;
			textBlock_XON0.Visibility = Visibility.Hidden;
			textBlock_XON0.IsEnabled = false;
			label_XON0_Percent.Visibility = Visibility.Hidden;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode1"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
		}
		else
		{
			if (m_bReload)
			{
				ClearUIContent();
				LoadDefaultConfig();
				m_bReload = false;
			}
			if (m_sMotorParamCfg.m_sStartCfg.byControlMode == 3)
			{
				comboBox_SpdCtrlMD.Items.Remove((string)FindResource("Cfg_OtherSpdCtrlMD0"));
				SetSpdCtrlModeSelectedIndex(m_sMotorParamCfg.m_sOhterCfg.bySpdCtrlMd);
			}
			switch (sensorMode)
			{
			case 1:
				ParamDisplayInSensorMode(CircuitType);
				break;
			case 2:
				ParamDisplayInSensorlessMode(CircuitType);
				break;
			}
		}
	}

	private void ParamDisplayInSensorMode(byte CircuitType)
	{
		comboBox_RPDAlign.SelectedIndex = 0;
		comboBox_RPDAlign.IsEnabled = false;
		comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode2"));
		comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode3"));
		switch (CircuitType)
		{
		case 0:
			comboBox_SMPMode.SelectedIndex = 0;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 0;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			CheckedHallForMd(0);
			EnableHallForMd(isEnabled: false);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			break;
		case 1:
			comboBox_SMPMode.SelectedIndex = 0;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 8;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			CheckedHallForMd(0);
			EnableHallForMd(isEnabled: false);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			break;
		case 2:
			comboBox_SMPMode.SelectedIndex = 0;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 8;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.SelectedIndex = 2;
			comboBox_HallMode.IsEnabled = false;
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			break;
		case 3:
			comboBox_SMPMode.SelectedIndex = 0;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 8;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode1"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			CheckedHallForMd(0);
			EnableHallForMd(isEnabled: false);
			break;
		case 4:
			comboBox_SMPMode.SelectedIndex = 1;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 0;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			CheckedStaSin(0);
			EnableStaSin(isEnabled: false);
			CheckedHallForMd(0);
			EnableHallForMd(isEnabled: false);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			break;
		case 5:
			comboBox_SMPMode.SelectedIndex = 1;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 8;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode1"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			CheckedStaSin(0);
			EnableStaSin(isEnabled: false);
			CheckedHallForMd(0);
			EnableHallForMd(isEnabled: false);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			CheckedAmpFs(1);
			EnableAmpFs(isEnabled: false);
			CheckedDirEn0(1);
			EnableDirEn0(isEnabled: false);
			CheckedDirEn1(0);
			EnableDirEn1(isEnabled: false);
			break;
		case 6:
			comboBox_SMPMode.SelectedIndex = 3;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 0;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode0"));
			comboBox_HallMode.Items.Remove((string)FindResource("Cfg_HallMode3"));
			SetHallModeSelectedIndex(m_sMotorParamCfg.m_sHallParamCfg.byHallMd);
			CheckedStaSin(0);
			EnableStaSin(isEnabled: false);
			CheckedHallForMd(0);
			EnableHallForMd(isEnabled: false);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			break;
		}
		if (comboBox_ContrlMode.SelectedValue.ToString() == "SpeedLoop" && radioBtn_HallSvMdEn.IsChecked == true)
		{
			CheckedeCurveSpdDis(0);
			EnableCurveSpdDis(isEnabled: false);
		}
		else
		{
			EnableCurveSpdDis(isEnabled: true);
		}
	}

	private void ParamDisplayInSensorlessMode(byte CircuitType)
	{
		tabItem_SensorBasic.Width = 0.0;
		if (tabControl1.SelectedIndex == 0)
		{
			tabControl1.SelectedIndex = 1;
		}
		switch (CircuitType)
		{
		case 0:
			comboBox_SMPMode.SelectedIndex = 0;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 12;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode2"));
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode3"));
			SetRSDModeSelectedIndex(m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			break;
		case 1:
			comboBox_SMPMode.SelectedIndex = 0;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 8;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode2"));
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode3"));
			SetRSDModeSelectedIndex(m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode);
			comboBox_RPDAlign.Items.Remove((string)FindResource("Cfg_AlignIPDEn2"));
			comboBox_RPDAlign.Items.Remove((string)FindResource("Cfg_AlignIPDEn3"));
			SetRPDAlignSelectedIndex(m_sMotorParamCfg.m_sIpdAlignCfg.byRpdAlignEn);
			CheckedDirEn0(1);
			EnableDirEn0(isEnabled: false);
			CheckedDirEn1(0);
			EnableDirEn1(isEnabled: false);
			break;
		case 2:
			comboBox_SMPMode.SelectedIndex = 1;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 12;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode2"));
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode3"));
			SetRSDModeSelectedIndex(m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			CheckedStaSin(0);
			EnableStaSin(isEnabled: false);
			CheckedDirEn0(1);
			EnableDirEn0(isEnabled: false);
			CheckedDirEn1(0);
			EnableDirEn1(isEnabled: false);
			break;
		case 3:
			comboBox_SMPMode.SelectedIndex = 3;
			comboBox_SMPMode.IsEnabled = false;
			comboBox_CmpModeSel.SelectedIndex = 12;
			comboBox_CmpModeSel.IsEnabled = false;
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode2"));
			comboBox_RSDMode.Items.Remove((string)FindResource("Cfg_WindRSDMode3"));
			SetRSDModeSelectedIndex(m_sMotorParamCfg.m_sWindHandleCfg.byRsdMode);
			CheckedEtcEn(0);
			EnableEtcEn(isEnabled: false);
			CheckedStaSin(0);
			EnableStaSin(isEnabled: false);
			CheckedDirEn0(1);
			EnableDirEn0(isEnabled: false);
			CheckedDirEn1(0);
			EnableDirEn1(isEnabled: false);
			break;
		}
	}

	private void EnableLock0(bool isEnabled)
	{
		radioBtn_Lock0Dis.IsEnabled = isEnabled;
		radioBtn_Lock0En.IsEnabled = isEnabled;
	}

	private void EnableLock1(bool isEnabled)
	{
		radioBtn_Lock1Dis.IsEnabled = isEnabled;
		radioBtn_Lock1En.IsEnabled = isEnabled;
	}

	private void EnableLock2(bool isEnabled)
	{
		radioBtn_Lock2Dis.IsEnabled = isEnabled;
		radioBtn_Lock2En.IsEnabled = isEnabled;
	}

	private void EnableLockMinSpeed(bool isEnabled)
	{
		textBox_LockMinSpeed.IsEnabled = isEnabled;
	}

	private void EnableHallForMd(bool isEnabled)
	{
		radioBtn_HallForMdDis.IsEnabled = isEnabled;
		radioBtn_HallForMDEn.IsEnabled = isEnabled;
	}

	private void CheckedHallForMd(byte value)
	{
		if (value == 1)
		{
			radioBtn_HallForMDEn.IsChecked = true;
		}
		else
		{
			radioBtn_HallForMdDis.IsChecked = true;
		}
	}

	private void EnableHallStaMd(bool isEnabled)
	{
		radioBtn_HallStaMdDis.IsEnabled = isEnabled;
		radioBtn_HallStaMdEn.IsEnabled = isEnabled;
	}

	private void EnablePllSmo(bool isEnabled)
	{
		radioButton_PllSmo_Dis.IsEnabled = isEnabled;
		radioButton_PllSmo_En.IsEnabled = isEnabled;
		double opacity = (isEnabled ? 1f : 0.2f);
		radioButton_PllSmo_Dis.Opacity = opacity;
		radioButton_PllSmo_En.Opacity = opacity;
		label_PllSmo.Opacity = opacity;
	}

	private void EnableAoEn(bool isEnabled)
	{
		radioBtn_MfpDis.IsEnabled = isEnabled;
		radioBtn_MfpEn.IsEnabled = isEnabled;
		double opacity = (isEnabled ? 1f : 0.2f);
		radioBtn_MfpDis.Opacity = opacity;
		radioBtn_MfpEn.Opacity = opacity;
		label_AOEN.Opacity = opacity;
	}

	private void EnableAOEK3(bool isEnabled)
	{
		textBox_MfpEk3.IsEnabled = isEnabled;
		double opacity = (isEnabled ? 1f : 0.2f);
		textBox_MfpEk3.Opacity = opacity;
		label_AOEK3.Opacity = opacity;
	}

	private void EnableStaPll(bool isEnabled)
	{
		radioButton_StaPll_Dis.IsEnabled = isEnabled;
		radioButton_StaPll_En.IsEnabled = isEnabled;
	}

	private void EnableSinAtan(bool isEnabled)
	{
		radioButton_Atan_Dis.IsEnabled = isEnabled;
		radioButton_Atan_En.IsEnabled = isEnabled;
		double opacity = (isEnabled ? 1f : 0.2f);
		radioButton_Atan_Dis.Opacity = opacity;
		radioButton_Atan_En.Opacity = opacity;
		label_SinAtan.Opacity = opacity;
	}

	private void EnableStaSin(bool isEnabled)
	{
		radioButton_StaSin_Dis.IsEnabled = isEnabled;
		radioButton_StaSin_En.IsEnabled = isEnabled;
	}

	private void CheckedStaSin(byte value)
	{
		if (value == 1)
		{
			radioButton_StaSin_En.IsChecked = true;
		}
		else
		{
			radioButton_StaSin_Dis.IsChecked = true;
		}
	}

	private void EnableEBW(bool isEnabled)
	{
		textBox_EBw.IsEnabled = isEnabled;
		label_EBW.IsEnabled = isEnabled;
		double opacity = (isEnabled ? 1f : 0.2f);
		label_EBW.Opacity = opacity;
		textBox_EBw.Opacity = opacity;
	}

	private void EnableEBWMode(bool isEnabled)
	{
		slider_EBWMode.IsEnabled = isEnabled;
		double opacity = (isEnabled ? 1f : 0.2f);
		slider_EBWMode.Opacity = opacity;
		label_EBWMode.Opacity = opacity;
		txtBlock_EBwMode.Opacity = opacity;
	}

	private void EnableEtcEn(bool isEnabled)
	{
		radioBtn_EtcEn0.IsEnabled = isEnabled;
		radioBtn_EtcEn1.IsEnabled = isEnabled;
	}

	private void CheckedEtcEn(byte value)
	{
		if (value == 1)
		{
			radioBtn_EtcEn1.IsChecked = true;
		}
		else
		{
			radioBtn_EtcEn0.IsChecked = true;
		}
	}

	private void EnableAmpFs(bool isEnabled)
	{
		radioBtn_AmpFs0.IsEnabled = isEnabled;
		radioBtn_AmpFs1.IsEnabled = isEnabled;
	}

	private void CheckedAmpFs(byte value)
	{
		if (value == 1)
		{
			radioBtn_AmpFs1.IsChecked = true;
		}
		else
		{
			radioBtn_AmpFs0.IsChecked = true;
		}
	}

	private void EnableDirInv(bool isEnabled)
	{
		radioBtn_DirInv0.IsEnabled = isEnabled;
		radioBtn_DirInv1.IsEnabled = isEnabled;
	}

	private void EnableDirEn0(bool isEnabled)
	{
		radioBtn_DirEn00.IsEnabled = isEnabled;
		radioBtn_DirEn01.IsEnabled = isEnabled;
	}

	private void CheckedDirEn0(byte value)
	{
		if (value == 1)
		{
			radioBtn_DirEn01.IsChecked = true;
		}
		else
		{
			radioBtn_DirEn00.IsChecked = true;
		}
	}

	private void EnableDirEn1(bool isEnabled)
	{
		radioBtn_DirEn10.IsEnabled = isEnabled;
		radioBtn_DirEn11.IsEnabled = isEnabled;
	}

	private void CheckedDirEn1(byte value)
	{
		if (value == 1)
		{
			radioBtn_DirEn11.IsChecked = true;
		}
		else
		{
			radioBtn_DirEn10.IsChecked = true;
		}
	}

	private void EnableCurveSpdDis(bool isEnabled)
	{
		radioBtn_CurveSpdDis.IsEnabled = isEnabled;
		radioBtn_CurveSpdEn.IsEnabled = isEnabled;
	}

	private void CheckedeCurveSpdDis(byte value)
	{
		if (value == 1)
		{
			radioBtn_CurveSpdEn.IsChecked = true;
		}
		else
		{
			radioBtn_CurveSpdDis.IsChecked = true;
		}
	}

	private void EnableAbnDis(bool isEnabled)
	{
		radioBtn_HallPllDis.IsEnabled = isEnabled;
		radioBtn_HallPllEn.IsEnabled = isEnabled;
	}

	private void CheckedAbnDis(byte value)
	{
		if (value == 1)
		{
			radioBtn_HallPllEn.IsChecked = true;
		}
		else
		{
			radioBtn_HallPllDis.IsChecked = true;
		}
	}

	private void SetHallModeSelectedIndex(byte value)
	{
		int num = 0;
		string strB = (string)FindResource("Cfg_HallMode" + value);
		foreach (string item in (IEnumerable)comboBox_HallMode.Items)
		{
			if (string.Compare(item, strB) == 0)
			{
				break;
			}
			num++;
		}
		if (num >= comboBox_HallMode.Items.Count)
		{
			num = 0;
		}
		if (comboBox_HallMode.SelectedIndex != num)
		{
			comboBox_HallMode.SelectedIndex = num;
		}
	}

	private void SetRSDModeSelectedIndex(byte value)
	{
		int num = 0;
		string strB = (string)FindResource("Cfg_WindRSDMode" + value);
		foreach (string item in (IEnumerable)comboBox_RSDMode.Items)
		{
			if (string.Compare(item, strB) == 0)
			{
				break;
			}
			num++;
		}
		if (num >= comboBox_RSDMode.Items.Count)
		{
			num = 0;
		}
		if (comboBox_RSDMode.SelectedIndex != num)
		{
			comboBox_RSDMode.SelectedIndex = num;
		}
	}

	private void SetRPDAlignSelectedIndex(byte value)
	{
		int num = 0;
		string strB = (string)FindResource("Cfg_AlignIPDEn" + value);
		foreach (string item in (IEnumerable)comboBox_RPDAlign.Items)
		{
			if (string.Compare(item, strB) == 0)
			{
				break;
			}
			num++;
		}
		if (num >= comboBox_RPDAlign.Items.Count)
		{
			num = 0;
		}
		if (comboBox_RPDAlign.SelectedIndex != num)
		{
			comboBox_RPDAlign.SelectedIndex = num;
		}
	}

	private void SetControlModeSelectedIndex(byte value)
	{
		int num = 0;
		string strB = (string)FindResource("Cfg_RunControlMode" + value);
		foreach (string item in (IEnumerable)comboBox_ContrlMode.Items)
		{
			if (string.Compare(item, strB) == 0)
			{
				break;
			}
			num++;
		}
		if (num >= comboBox_ContrlMode.Items.Count)
		{
			num = 0;
		}
		if (comboBox_ContrlMode.SelectedIndex != num)
		{
			comboBox_ContrlMode.SelectedIndex = num;
		}
	}

	private void SetSpdCtrlModeSelectedIndex(byte value)
	{
		int num = 0;
		string strB = (string)FindResource("Cfg_OtherSpdCtrlMD" + value);
		foreach (string item in (IEnumerable)comboBox_SpdCtrlMD.Items)
		{
			if (string.Compare(item, strB) == 0)
			{
				break;
			}
			num++;
		}
		if (num >= comboBox_SpdCtrlMD.Items.Count)
		{
			num = 0;
		}
		if (comboBox_SpdCtrlMD.SelectedIndex != num)
		{
			comboBox_SpdCtrlMD.SelectedIndex = num;
		}
	}

	private string GetSensorModeText(short sensorMode)
	{
		string text = "";
		return sensorMode switch
		{
			1 => "Sensor", 
			2 => "Sensorless", 
			_ => "Full", 
		};
	}

	private string GetCircuitTypeText(short sensorMode, byte CircuitType)
	{
		string result = "";
		switch (sensorMode)
		{
		case 1:
			result = (string)FindResource("SensorCircuitType" + CircuitType);
			break;
		case 2:
			result = (string)FindResource("SensorlessCircuitType" + CircuitType);
			break;
		}
		return result;
	}

	public void SetHallRevEnValue(byte isEnabled)
	{
		if (isEnabled == 0)
		{
			radioBtn_Hall0RevDis.IsChecked = true;
			radioBtn_Hall1RevDis.IsChecked = true;
			radioBtn_Hall2RevDis.IsChecked = true;
		}
		else
		{
			radioBtn_Hall0RevEn.IsChecked = true;
			radioBtn_Hall1RevEn.IsChecked = true;
			radioBtn_Hall2RevEn.IsChecked = true;
		}
	}

	public void RecordOnOffError2CSV(string errorInfo)
	{
		string fileName = AppDomain.CurrentDomain.BaseDirectory + "log\\OnOffErrorInfo.csv";
		string[] array = new string[4] { "Time", "ExeCnt", "Desc", "Fault Code" };
		DataTable dataTable = new DataTable();
		DataRow dataRow = dataTable.NewRow();
		string[] array2 = array;
		foreach (string columnName in array2)
		{
			dataTable.Columns.Add(columnName);
		}
		dataRow["Time"] = DateTime.Now.ToString("MM/dd HH:mm:ss");
		dataRow["ExeCnt"] = m_sMotorParamCfg.m_sOnOffTestCfg.wExecutedCnt;
		dataRow["Desc"] = errorInfo;
		dataRow["Fault Code"] = m_byFaultStatus;
		dataTable.Rows.Add(dataRow);
		if (g_csvHelper == null)
		{
			g_csvHelper = new CSVHelper(fileName, array);
		}
		g_csvHelper.ExportDataTable2CSV(dataTable);
	}

	public void radioBtn_Hall0RevDis_Checked(object sender, RoutedEventArgs e)
	{
		bool? isChecked = radioBtn_Hall1RevEn.IsChecked;
		if (isChecked != true && isChecked.HasValue && radioBtn_Hall2RevEn.IsChecked == false)
		{
			EnableAbnDis(isEnabled: true);
		}
	}

	public void radioBtn_Hall0RevEn_Checked(object sender, RoutedEventArgs e)
	{
		CheckedAbnDis(1);
		EnableAbnDis(isEnabled: false);
	}

	public void radioBtn_Hall1RevDis_Checked(object sender, RoutedEventArgs e)
	{
		bool? isChecked = radioBtn_Hall0RevEn.IsChecked;
		if (isChecked != true && isChecked.HasValue && radioBtn_Hall2RevEn.IsChecked == false)
		{
			EnableAbnDis(isEnabled: true);
		}
	}

	public void radioBtn_Hall1RevEn_Checked(object sender, RoutedEventArgs e)
	{
		CheckedAbnDis(1);
		EnableAbnDis(isEnabled: false);
	}

	private void radioBtn_Hall2RevDis_Checked(object sender, RoutedEventArgs e)
	{
		bool? isChecked = radioBtn_Hall0RevEn.IsChecked;
		if (isChecked != true && isChecked.HasValue && radioBtn_Hall1RevEn.IsChecked == false)
		{
			EnableAbnDis(isEnabled: true);
		}
	}

	private void radioBtn_Hall2RevEn_Checked(object sender, RoutedEventArgs e)
	{
		CheckedAbnDis(1);
		EnableAbnDis(isEnabled: false);
	}

	private void slider_ReverseBrakeSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed = (byte)slider_ReverseBrakeSpeed.Value;
		DisplayReverseBrakeSpeedText();
	}

	private void DisplayReverseBrakeSpeedText()
	{
		float num = (int)m_sMotorParamCfg.m_sWindHandleCfg.byRevBrakeSpeed;
		float num2 = (int)m_sMotorParamCfg.m_sFocParamCfg.wFreqBase;
		double value = (double)(num + 1f) / 16.0 * ((double)num2 / 4.0);
		if (num == 15f)
		{
			textBlock_ReverseBrakeSpeed.Text = "Brake";
		}
		else
		{
			textBlock_ReverseBrakeSpeed.Text = Math.Round(value, 2) + "Hz";
		}
	}

	private void UpdateStatisSpeedSliderValue(byte level, double textBlockValue)
	{
		double num = 0.0;
		DoubleCollection doubleCollection = new DoubleCollection();
		doubleCollection.Add(1.0);
		doubleCollection.Add(2.0);
		doubleCollection.Add(4.0);
		doubleCollection.Add(8.0);
		doubleCollection.Add(16.0);
		doubleCollection.Add(20.0);
		doubleCollection.Add(24.0);
		doubleCollection.Add(28.0);
		DoubleCollection doubleCollection2 = doubleCollection;
		DoubleCollection doubleCollection3 = new DoubleCollection();
		doubleCollection3.Add(1.0);
		doubleCollection3.Add(2.0);
		doubleCollection3.Add(4.0);
		doubleCollection3.Add(8.0);
		doubleCollection3.Add(16.0);
		doubleCollection3.Add(32.0);
		doubleCollection3.Add(64.0);
		doubleCollection3.Add(100.0);
		DoubleCollection doubleCollection4 = doubleCollection3;
		DoubleCollection doubleCollection5 = new DoubleCollection();
		doubleCollection5.Add(1.0);
		doubleCollection5.Add(2.0);
		doubleCollection5.Add(4.0);
		doubleCollection5.Add(8.0);
		doubleCollection5.Add(16.0);
		doubleCollection5.Add(0.167);
		doubleCollection5.Add(0.25);
		doubleCollection5.Add(0.5);
		DoubleCollection doubleCollection6 = doubleCollection5;
		num = level switch
		{
			1 => doubleCollection2.IndexOf(textBlockValue), 
			2 => doubleCollection4.IndexOf(textBlockValue), 
			_ => doubleCollection6.IndexOf(textBlockValue), 
		};
		slider_StaticSpeed.Value = num;
	}

	private void UpdateStatisSpeedTextBlock(byte level, byte index)
	{
		double num = 0.0;
		DoubleCollection doubleCollection = new DoubleCollection();
		doubleCollection.Add(1.0);
		doubleCollection.Add(2.0);
		doubleCollection.Add(4.0);
		doubleCollection.Add(8.0);
		doubleCollection.Add(16.0);
		doubleCollection.Add(20.0);
		doubleCollection.Add(24.0);
		doubleCollection.Add(28.0);
		DoubleCollection doubleCollection2 = doubleCollection;
		DoubleCollection doubleCollection3 = new DoubleCollection();
		doubleCollection3.Add(1.0);
		doubleCollection3.Add(2.0);
		doubleCollection3.Add(4.0);
		doubleCollection3.Add(8.0);
		doubleCollection3.Add(16.0);
		doubleCollection3.Add(32.0);
		doubleCollection3.Add(64.0);
		doubleCollection3.Add(100.0);
		DoubleCollection doubleCollection4 = doubleCollection3;
		DoubleCollection doubleCollection5 = new DoubleCollection();
		doubleCollection5.Add(1.0);
		doubleCollection5.Add(2.0);
		doubleCollection5.Add(4.0);
		doubleCollection5.Add(8.0);
		doubleCollection5.Add(16.0);
		doubleCollection5.Add(0.167);
		doubleCollection5.Add(0.25);
		doubleCollection5.Add(0.5);
		DoubleCollection doubleCollection6 = doubleCollection5;
		num = level switch
		{
			1 => doubleCollection2[index], 
			2 => doubleCollection4[index], 
			_ => doubleCollection6[index], 
		};
		textBlock_StaticSpeed.Text = num.ToString();
	}

	private void radioButton_PllSmo_Dis_Checked(object sender, RoutedEventArgs e)
	{
		if (radioBtn_MfpEn.IsChecked == false)
		{
			EnableEBW(isEnabled: true);
		}
		EnableEBWMode(isEnabled: true);
	}

	private void radioButton_PllSmo_En_Checked(object sender, RoutedEventArgs e)
	{
		if (radioButton_StaPll_En.IsChecked != true)
		{
			EnableEBW(isEnabled: false);
		}
		EnableEBWMode(isEnabled: false);
	}

	private void radioBtn_MfpDis_Checked(object sender, RoutedEventArgs e)
	{
		if (radioButton_PllSmo_En.IsChecked != true)
		{
			EnableEBW(isEnabled: true);
			EnableEBWMode(isEnabled: true);
		}
		EnableAOEK3(isEnabled: false);
		EnablePllSmo(isEnabled: true);
		EnableSinAtan(isEnabled: true);
	}

	private void radioBtn_MfpEn_Checked(object sender, RoutedEventArgs e)
	{
		EnableEBW(isEnabled: false);
		EnableEBWMode(isEnabled: false);
		EnableAOEK3(isEnabled: true);
		radioButton_PllSmo_En.IsChecked = true;
		EnablePllSmo(isEnabled: false);
		radioButton_Atan_En.IsChecked = true;
		EnableSinAtan(isEnabled: false);
	}

	private void radioButton_StaPll_Dis_Checked(object sender, RoutedEventArgs e)
	{
		EnableAoEn(isEnabled: true);
		EnablePllSmo(isEnabled: true);
	}

	private void radioButton_StaPll_En_Checked(object sender, RoutedEventArgs e)
	{
		EnableEBW(isEnabled: true);
		radioBtn_MfpDis.IsChecked = true;
		EnableAoEn(isEnabled: false);
		radioButton_PllSmo_En.IsChecked = true;
		EnablePllSmo(isEnabled: false);
	}

	public void slider_StaticSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		byte freqBaseLevel = GetFreqBaseLevel();
		UpdateStatisSpeedTextBlock(freqBaseLevel, (byte)slider_StaticSpeed.Value);
	}

	private byte GetFreqBaseLevel()
	{
		int num = ((m_sMotorParamCfg.m_sFocParamCfg.wFreqBase >= 2000) ? 2 : ((m_sMotorParamCfg.m_sFocParamCfg.wFreqBase >= 640) ? 1 : 0));
		return (byte)num;
	}

	private void menuItem_ShowHallComMd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (!m_isAuthorize)
		{
			AuthorizeWindow authorizeWindow = new AuthorizeWindow();
			if (authorizeWindow.ShowDialog() == true)
			{
				m_isAuthorize = true;
			}
			else
			{
				e.Handled = false;
			}
		}
	}

	private void menuItem_ShowHallComMd_Checked(object sender, RoutedEventArgs e)
	{
		if (m_isAuthorize)
		{
			ShowHallComMd(isVisible: true);
		}
	}

	private void menuItem_ShowHallComMd_Unchecked(object sender, RoutedEventArgs e)
	{
		ShowHallComMd(isVisible: false);
	}

	public void ShowHallComMd(bool isVisible)
	{
		if (isVisible)
		{
			label_HallComMd.Opacity = 1.0;
			radioBtn_HallComMd0.Opacity = 1.0;
			radioBtn_HallComMd1.Opacity = 1.0;
			radioBtn_HallComMd3.Opacity = 1.0;
			radioBtn_HallComMd2.Opacity = 1.0;
			label_HallComMd.IsEnabled = true;
			radioBtn_HallComMd0.IsEnabled = true;
			radioBtn_HallComMd1.IsEnabled = true;
			radioBtn_HallComMd3.IsEnabled = true;
			radioBtn_HallComMd2.IsEnabled = true;
		}
		else
		{
			label_HallComMd.Opacity = 0.0;
			radioBtn_HallComMd0.Opacity = 0.0;
			radioBtn_HallComMd1.Opacity = 0.0;
			radioBtn_HallComMd3.Opacity = 0.0;
			radioBtn_HallComMd2.Opacity = 0.0;
			label_HallComMd.IsEnabled = false;
			radioBtn_HallComMd0.IsEnabled = false;
			radioBtn_HallComMd1.IsEnabled = false;
			radioBtn_HallComMd3.IsEnabled = false;
			radioBtn_HallComMd2.IsEnabled = false;
		}
	}

	private void radioBtn_CurveSpdEn_Checked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 1;
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_CurveSpdEn_Unchecked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 0;
		HandleCtrlModeCurveShow();
	}

	private void radioBtn_CurveSpdDis_Unchecked(object sender, RoutedEventArgs e)
	{
		m_sMotorParamCfg.m_sOhterCfg.byCurveEnSpd = 1;
		HandleCtrlModeCurveShow();
	}

	public void textBox_StartDuty30_LostFocus(object sender, RoutedEventArgs e)
	{
		string text = textBox_StartDuty30.Text;
		int num = 0;
		try
		{
			byte b = Convert.ToByte(text);
			if (b < 0 || b > 50)
			{
				MessageBox.Show("invalid value, please input int data between 0 and 50.");
				num = 1;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("invalid value, please input float data between 0 and 50.");
			num = 1;
		}
		if (num != 0)
		{
			if (m_byIsLoadDefault == 1)
			{
				textBox_StartDuty30.Text = m_sMotorParamCfg.m_sStartCfg.byStartDuty.ToString();
			}
			else
			{
				textBox_StartDuty30.Text = (string)FindResource("Cfg_StartDuty30");
			}
		}
	}

	private void radioBtn_HallStaOpDis_Checked(object sender, RoutedEventArgs e)
	{
		if (radioBtn_HallSvMdEn.IsChecked == true)
		{
			label25.Visibility = Visibility.Hidden;
			textBox_StartCurrent.Visibility = Visibility.Hidden;
			label_StartDuty30.Visibility = Visibility.Visible;
			textBox_StartDuty30.Visibility = Visibility.Visible;
			label_StartDutyUnit.Visibility = Visibility.Visible;
		}
		else
		{
			label25.Visibility = Visibility.Visible;
			textBox_StartCurrent.Visibility = Visibility.Visible;
			label_StartDuty30.Visibility = Visibility.Hidden;
			textBox_StartDuty30.Visibility = Visibility.Hidden;
			label_StartDutyUnit.Visibility = Visibility.Hidden;
		}
	}

	private void radioBtn_HallStaOpEn_Checked(object sender, RoutedEventArgs e)
	{
		if (radioBtn_HallStaOpEn.IsChecked == true || radioBtn_HallSvMdEn.IsChecked == true)
		{
			label25.Visibility = Visibility.Hidden;
			textBox_StartCurrent.Visibility = Visibility.Hidden;
			label_StartDuty30.Visibility = Visibility.Visible;
			textBox_StartDuty30.Visibility = Visibility.Visible;
			label_StartDutyUnit.Visibility = Visibility.Visible;
		}
	}

	private void comboBox_IdleBrakeEn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn = (byte)comboBox_IdleBrakeEn.SelectedIndex;
	}

	private void checkBox_EnableChangeVdc_Click(object sender, RoutedEventArgs e)
	{
		if (checkBox_EnableChangeVdc.IsChecked == true)
		{
			m_blIsChangeVdc = true;
		}
		else
		{
			m_blIsChangeVdc = false;
		}
	}

	private void btn_WinMin_Click(object sender, RoutedEventArgs e)
	{
		base.WindowState = WindowState.Minimized;
	}

	private void btn_WinMax_Click(object sender, RoutedEventArgs e)
	{
		if (base.WindowState == WindowState.Minimized)
		{
			base.WindowState = WindowState.Normal;
		}
		else
		{
			base.WindowState = WindowState.Maximized;
		}
	}

	private void btn_WinClose_Click(object sender, RoutedEventArgs e)
	{
		CloseWindowFun();
	}

	private void TitleBar_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private void CloseWindowFun()
	{
		try
		{
			if (m_isInitialized)
			{
				int num = 0;
				string currentConfig = Settings.Default.CurrentConfig;
				m_blIsSave2HallUC = true;
				ConverUIToParam();
				UpdateUIDataToUserCfg();
				commonConfigSetting.m_byLogEn = 0;
				if (checkBox_SimulateComm.IsChecked == true)
				{
					commonConfigSetting.m_byLogEn = 1;
				}
				string text = AppDomain.CurrentDomain.BaseDirectory + "\\UserConfigFile\\FTClosedConfig.pfd";
				if (SaveSpecifyConfig(text) == 0)
				{
					Settings.Default.CurrentConfig = text;
					Settings.Default.Save();
				}
				OnTimer.Stop();
				OffTimer.Stop();
				if (timeThread.ThreadState == System.Threading.ThreadState.Suspended)
				{
					m_bFinishedTest = true;
					m_sMotorParamCfg.m_sOnOffTestCfg.wTotalSetCnt = 0;
					timeThread.Resume();
				}
				timeThread.Abort();
				if (timeThread.IsAlive)
				{
					timeThread.Join();
				}
			}
		}
		catch (Exception ex)
		{
			LOG.WriteMsg(ex.Message);
		}
		m_isWindowClosing = true;
		Application.Current.Shutdown();
		Environment.Exit(16711780);
	}
}
