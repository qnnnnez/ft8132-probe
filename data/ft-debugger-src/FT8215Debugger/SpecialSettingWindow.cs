using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.Win32;

namespace FT8215Debugger;

public partial class SpecialSettingWindow : Window, IComponentConnector
{
	public delegate void PassValuesHandler(object sender, PassValuesEventArgs e);

	private DateTime dtBeginTime;

	private DateTime dtEndTime;

	public ParamStructureDef m_sSpecialParamCfg { get; set; }

	public MainWindow mainWindow { get; set; }

	public event PassValuesHandler PassValuesEvent;

	public SpecialSettingWindow()
	{
		InitializeComponent();
		InitUserControl();
		mainWindow = (MainWindow)Application.Current.MainWindow;
	}

	private int InitUserControl()
	{
		return 0;
	}

	private int LoadUserConfigUI()
	{
		AddComboBoxItems();
		comboBox_IsdDirRev.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_IsdDirRev.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_IsdDirRev.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byIsdDirRev;
		comboBox_OvpSel.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_OvpSel.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_OvpSel.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpSel;
		comboBox_TsdSel.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdSel;
		comboBox_DpmDis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_DpmDis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_DpmDis.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byDpmDis;
		comboBox_Foc_Dis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_Foc_Dis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_Foc_Dis.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byFocDis;
		comboBox_BootMod.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_BootMod.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_BootMod.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byBootMod;
		comboBox_TsdDis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_TsdDis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_TsdDis.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdDis;
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq0"));
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq1"));
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq2"));
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq3"));
		comboBox_TsMin.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byTSMin;
		comboBox_RvsCurSel.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_RvsCurSel.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_RvsCurSel.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byRvsCutSel;
		byte byNewSingle = m_sSpecialParamCfg.m_eSpecialRegCfg.byNewSingle;
		if (byNewSingle == 1)
		{
			checkBox_SmpMode.IsChecked = true;
		}
		else
		{
			checkBox_SmpMode.IsChecked = false;
		}
		int num = 0;
		for (num = 0; num < 32; num++)
		{
			string newItem = num.ToString();
			comboBox_DigTest.Items.Add(newItem);
		}
		comboBox_DigTest.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byDigTst;
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt0"));
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt1"));
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt2"));
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt3"));
		comboBox_DigMoniCt.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byDigMoniCt;
		comboBox_TestMode.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_TestMode.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_TestMode.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byTestMode;
		comboBox_IrefDis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_IrefDis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_IrefDis.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byIrefDis;
		comboBox_HallOvMd.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byHallOvMd;
		comboBox_HallLttEn.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byHallLttEn;
		comboBox_StallTstQmax.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstQMax;
		comboBox_StallTstInc.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstInc;
		comboBox_StallTstEn.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstEn;
		comboBox_DrvDisMod.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byDrvDisMod;
		comboBox_Cmp0CkSel.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byCmp0CkSel;
		comboBox_UvloDis.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byUvloDis;
		comboBox_OvpDis.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpDis;
		comboBox_FocLdVoltKp.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp;
		comboBox_FocLqVoltKp.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp;
		comboBox_FocFluxVoltKp.SelectedIndex = m_sSpecialParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp;
		return 0;
	}

	private void AddComboBoxItems()
	{
		int num = 0;
		for (num = 0; num < 16; num++)
		{
			comboBox_TsdSel.Items.Add(num);
		}
		comboBox_HallOvMd.Items.Add((string)FindResource("Cfg_UserHallOvMd0"));
		comboBox_HallOvMd.Items.Add((string)FindResource("Cfg_UserHallOvMd1"));
		comboBox_HallLttEn.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_HallLttEn.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_StallTstQmax.Items.Add((string)FindResource("Cfg_UserStallTstQmax0"));
		comboBox_StallTstQmax.Items.Add((string)FindResource("Cfg_UserStallTstQmax1"));
		comboBox_StallTstInc.Items.Add((string)FindResource("Cfg_UserHallLttEn0"));
		comboBox_StallTstInc.Items.Add((string)FindResource("Cfg_UserHallLttEn1"));
		comboBox_StallTstEn.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_StallTstEn.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_DrvDisMod.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_DrvDisMod.Items.Add((string)FindResource("Cfg_UserEnable1"));
		for (num = 0; num < 4; num++)
		{
			comboBox_Cmp0CkSel.Items.Add(num.ToString());
		}
		for (num = 0; num < 32; num++)
		{
			comboBox_FocLdVoltKp.Items.Add(num.ToString());
			comboBox_FocLqVoltKp.Items.Add(num.ToString());
			comboBox_FocFluxVoltKp.Items.Add(num.ToString());
		}
		comboBox_UvloDis.Items.Add("Disable");
		comboBox_UvloDis.Items.Add("Enable");
		comboBox_OvpDis.Items.Add("Disable");
		comboBox_OvpDis.Items.Add("Enable");
	}

	private void LoadDefaultUI()
	{
		AddComboBoxItems();
		comboBox_IsdDirRev.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_IsdDirRev.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_IsdDirRev.SelectedIndex = 0;
		comboBox_OvpSel.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_OvpSel.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_OvpSel.SelectedIndex = 0;
		comboBox_TsdSel.SelectedIndex = 0;
		comboBox_DpmDis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_DpmDis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_DpmDis.SelectedIndex = 0;
		comboBox_Foc_Dis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_Foc_Dis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_Foc_Dis.SelectedIndex = 0;
		comboBox_BootMod.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_BootMod.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_BootMod.SelectedIndex = 0;
		comboBox_TsdDis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_TsdDis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_TsdDis.SelectedIndex = 0;
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq0"));
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq1"));
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq2"));
		comboBox_TsMin.Items.Add((string)FindResource("Cfg_CarrierFreq3"));
		comboBox_TsMin.SelectedIndex = 0;
		comboBox_RvsCurSel.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_RvsCurSel.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_RvsCurSel.SelectedIndex = 0;
		bool value = false;
		checkBox_SmpMode.IsChecked = value;
		int num = 0;
		for (num = 0; num < 32; num++)
		{
			string newItem = num.ToString();
			comboBox_DigTest.Items.Add(newItem);
		}
		comboBox_DigTest.SelectedIndex = 0;
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt0"));
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt1"));
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt2"));
		comboBox_DigMoniCt.Items.Add((string)FindResource("Cfg_SPDigMoniCt3"));
		comboBox_DigMoniCt.SelectedIndex = 0;
		comboBox_TestMode.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_TestMode.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_TestMode.SelectedIndex = 0;
		comboBox_IrefDis.Items.Add((string)FindResource("Cfg_UserEnable0"));
		comboBox_IrefDis.Items.Add((string)FindResource("Cfg_UserEnable1"));
		comboBox_IrefDis.SelectedIndex = 0;
		comboBox_HallOvMd.SelectedIndex = 0;
		comboBox_HallLttEn.SelectedIndex = 0;
		comboBox_StallTstQmax.SelectedIndex = 0;
		comboBox_StallTstInc.SelectedIndex = 0;
		comboBox_StallTstEn.SelectedIndex = 0;
		comboBox_DrvDisMod.SelectedIndex = 0;
		comboBox_Cmp0CkSel.SelectedIndex = 0;
		comboBox_UvloDis.SelectedIndex = 0;
		comboBox_OvpDis.SelectedIndex = 0;
		comboBox_FocLdVoltKp.SelectedIndex = 0;
		comboBox_FocLqVoltKp.SelectedIndex = 0;
		comboBox_FocFluxVoltKp.SelectedIndex = 0;
	}

	private void btn_OK_Click(object sender, RoutedEventArgs e)
	{
		string text = "test";
		int num = 1;
		UpdateUIDataToParam();
		PassValuesEventArgs e2 = new PassValuesEventArgs(new ParamStructureDef.S_SpecialRegCfg
		{
			byOcpSel = m_sSpecialParamCfg.m_eSpecialRegCfg.byOcpSel,
			byIsdDirRev = m_sSpecialParamCfg.m_eSpecialRegCfg.byIsdDirRev,
			byOvpSel = m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpSel,
			byTsdSel = m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdSel,
			byDzSet = m_sSpecialParamCfg.m_eSpecialRegCfg.byDzSet,
			byDpmDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byDpmDis,
			byOvpDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpDis,
			byOcpDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byOcpDis,
			byTsdDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdDis,
			byFocDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byFocDis,
			byBootMod = m_sSpecialParamCfg.m_eSpecialRegCfg.byBootMod,
			byIsdBrakeEn = m_sSpecialParamCfg.m_eSpecialRegCfg.byIsdBrakeEn,
			byIdleBrakeEn = m_sSpecialParamCfg.m_eSpecialRegCfg.byIdleBrakeEn,
			byTSMin = m_sSpecialParamCfg.m_eSpecialRegCfg.byTSMin,
			byRvsAlignEn = m_sSpecialParamCfg.m_eSpecialRegCfg.byRvsAlignEn,
			byRvsCutSel = m_sSpecialParamCfg.m_eSpecialRegCfg.byRvsCutSel,
			byNewSingle = m_sSpecialParamCfg.m_eSpecialRegCfg.byNewSingle,
			byDigTst = m_sSpecialParamCfg.m_eSpecialRegCfg.byDigTst,
			byDigMoniCt = m_sSpecialParamCfg.m_eSpecialRegCfg.byDigMoniCt,
			fRV = m_sSpecialParamCfg.m_eSpecialRegCfg.fRV,
			byTestMode = m_sSpecialParamCfg.m_eSpecialRegCfg.byTestMode,
			byIrefDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byIrefDis,
			byHallOvMd = m_sSpecialParamCfg.m_eSpecialRegCfg.byHallOvMd,
			byHallLttEn = m_sSpecialParamCfg.m_eSpecialRegCfg.byHallLttEn,
			byStallTstQMax = m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstQMax,
			byStallTstInc = m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstInc,
			byStallTstEn = m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstEn,
			byDrvDisMod = m_sSpecialParamCfg.m_eSpecialRegCfg.byDrvDisMod,
			byCmp0CkSel = m_sSpecialParamCfg.m_eSpecialRegCfg.byCmp0CkSel,
			byUvloDis = m_sSpecialParamCfg.m_eSpecialRegCfg.byUvloDis,
			byFlocLdVoltKp = m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp,
			byFlocLqVoltKp = m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp,
			byFocFluxVoltKp = m_sSpecialParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp,
			byRestaEn = m_sSpecialParamCfg.m_eSpecialRegCfg.byRestaEn,
			byIsdFwMd = m_sSpecialParamCfg.m_eSpecialRegCfg.byIsdFwMd,
			byLaMd = m_sSpecialParamCfg.m_eSpecialRegCfg.byLaMd,
			byNewFunc = m_sSpecialParamCfg.m_eSpecialRegCfg.byNewFunc
		});
		this.PassValuesEvent(this, e2);
		Close();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		LoadUserConfigUI();
	}

	private void btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		ClearUIContent();
		LoadDefaultUI();
	}

	private bool ClearUIContent()
	{
		comboBox_IsdDirRev.Items.Clear();
		comboBox_OvpSel.Items.Clear();
		comboBox_TsdSel.Items.Clear();
		comboBox_DpmDis.Items.Clear();
		comboBox_Foc_Dis.Items.Clear();
		comboBox_BootMod.Items.Clear();
		comboBox_TsdDis.Items.Clear();
		comboBox_TsMin.Items.Clear();
		comboBox_RvsCurSel.Items.Clear();
		comboBox_DigTest.Items.Clear();
		comboBox_DigMoniCt.Items.Clear();
		comboBox_TestMode.Items.Clear();
		comboBox_IrefDis.Items.Clear();
		comboBox_HallOvMd.Items.Clear();
		comboBox_HallLttEn.Items.Clear();
		comboBox_StallTstQmax.Items.Clear();
		comboBox_StallTstInc.Items.Clear();
		comboBox_StallTstEn.Items.Clear();
		comboBox_DrvDisMod.Items.Clear();
		comboBox_Cmp0CkSel.Items.Clear();
		comboBox_UvloDis.Items.Clear();
		comboBox_OvpDis.Items.Clear();
		comboBox_FocLdVoltKp.Items.Clear();
		comboBox_FocLqVoltKp.Items.Clear();
		comboBox_FocFluxVoltKp.Items.Clear();
		return true;
	}

	private int UpdateUIDataToParam()
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byIsdDirRev = (byte)comboBox_IsdDirRev.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpSel = (byte)comboBox_OvpSel.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdSel = (byte)comboBox_TsdSel.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDpmDis = (byte)comboBox_DpmDis.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFocDis = (byte)comboBox_Foc_Dis.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byBootMod = (byte)comboBox_BootMod.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdDis = (byte)comboBox_TsdDis.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTSMin = (byte)comboBox_TsMin.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byRvsCutSel = (byte)comboBox_RvsCurSel.SelectedIndex;
		if (checkBox_SmpMode.IsChecked == true)
		{
			m_sSpecialParamCfg.m_eSpecialRegCfg.byNewSingle = 1;
		}
		else
		{
			m_sSpecialParamCfg.m_eSpecialRegCfg.byNewSingle = 0;
		}
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDigTst = (byte)comboBox_DigTest.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDigMoniCt = (byte)comboBox_DigMoniCt.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTestMode = (byte)comboBox_TestMode.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byIrefDis = (byte)comboBox_IrefDis.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byHallOvMd = (byte)comboBox_HallOvMd.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byHallLttEn = (byte)comboBox_HallLttEn.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstQMax = (byte)comboBox_StallTstQmax.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstInc = (byte)comboBox_StallTstInc.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstEn = (byte)comboBox_StallTstEn.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDrvDisMod = (byte)comboBox_DrvDisMod.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byCmp0CkSel = (byte)comboBox_Cmp0CkSel.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byUvloDis = (byte)comboBox_UvloDis.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpDis = (byte)comboBox_OvpDis.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp = (byte)comboBox_FocLdVoltKp.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp = (byte)comboBox_FocLqVoltKp.SelectedIndex;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp = (byte)comboBox_FocFluxVoltKp.SelectedIndex;
		return 0;
	}

	private void btn_ReadReg_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		label_RegStatus.Content = "Status2: 0%";
		if (mainWindow.ReadRegister() == 0)
		{
			label_RegStatus.Content = "Status2:100%...read register successfully";
		}
		else
		{
			label_RegStatus.Content = "Status2:80%...read register fail";
		}
	}

	private void btn_OpenEfuse_Click(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Bin files(*.Bin,*.bin)|*.Bin;*bin";
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		SaveFileDialog saveFileDialog2 = saveFileDialog;
		bool? flag = true;
		if (saveFileDialog2.ShowDialog() == flag)
		{
			textBox_ReadEfuse.Text = saveFileDialog2.FileName;
		}
	}

	private void btn_ReadEfuse_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		string text = textBox_ReadEfuse.Text;
		if (text.Length == 0)
		{
			MessageBox.Show("file path is empty, please specify a bin file name to readback");
			return;
		}
		label_EfuseStatus.Content = "Status1:20%...";
		if (mainWindow.HandleVerifyFlow() != 0)
		{
			label_EfuseStatus.Content = "Status1:30%...Read efuse fail";
			return;
		}
		label_EfuseStatus.Content = "Status1:60%...";
		if (mainWindow.LogEfuse2Bin(text) != 0)
		{
			label_EfuseStatus.Content = "Status1:70%...Log efuse fail";
			return;
		}
		label_EfuseStatus.Content = "Status1:80%...";
		string content = $"0x{mainWindow.m_wTgtSrcVal:X4}";
		label_readEfuseAdmin.Content = content;
		label_EfuseStatus.Content = "Status1:100%.Read efuse successfully";
	}

	private void btn_ReadReg_Click_1(object sender, RoutedEventArgs e)
	{
		int num = 0;
		label_RegStatus.Content = "Status2: 0%";
		if (mainWindow.ReadRegister() == 0)
		{
			label_RegStatus.Content = "Status2:100%...read register successfully";
		}
		else
		{
			label_RegStatus.Content = "Status2:80%...read register fail";
		}
	}

	private void comboBox_IsdDirRev_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byIsdDirRev = (byte)comboBox_IsdDirRev.SelectedIndex;
	}

	private void comboBox_OvpSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpSel = (byte)comboBox_OvpSel.SelectedIndex;
	}

	private void comboBox_TsdSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdSel = (byte)comboBox_TsdSel.SelectedIndex;
	}

	private void comboBox_DpmDis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDpmDis = (byte)comboBox_DpmDis.SelectedIndex;
	}

	private void comboBox_Foc_Dis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFocDis = (byte)comboBox_Foc_Dis.SelectedIndex;
	}

	private void comboBox_BootMod_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byBootMod = (byte)comboBox_BootMod.SelectedIndex;
	}

	private void comboBox_TsdDis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTsdDis = (byte)comboBox_TsdDis.SelectedIndex;
	}

	private void comboBox_TsMin_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTSMin = (byte)comboBox_TsMin.SelectedIndex;
	}

	private void comboBox_RvsCurSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byRvsCutSel = (byte)comboBox_RvsCurSel.SelectedIndex;
	}

	private void checkBox_SmpMode_Click(object sender, RoutedEventArgs e)
	{
		if (checkBox_SmpMode.IsChecked == true)
		{
			m_sSpecialParamCfg.m_eSpecialRegCfg.byNewSingle = 1;
		}
		else
		{
			m_sSpecialParamCfg.m_eSpecialRegCfg.byNewSingle = 0;
		}
	}

	private void comboBox_DigTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDigTst = (byte)comboBox_DigTest.SelectedIndex;
		string text = "Do not test";
	}

	private void comboBox_TestMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byTestMode = (byte)comboBox_TestMode.SelectedIndex;
	}

	private void comboBox_DigMoniCt_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDigMoniCt = (byte)comboBox_DigMoniCt.SelectedIndex;
	}

	private void comboBox_IrefDis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ComboBox comboBox = sender as ComboBox;
		m_sSpecialParamCfg.m_eSpecialRegCfg.byIrefDis = (byte)comboBox.SelectedIndex;
	}

	private void comboBox_HallOvMd_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byHallOvMd = (byte)comboBox_HallOvMd.SelectedIndex;
	}

	private void comboBox_HallLttEn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byHallLttEn = (byte)comboBox_HallLttEn.SelectedIndex;
	}

	private void comboBox_StallTstQmax_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstQMax = (byte)comboBox_StallTstQmax.SelectedIndex;
	}

	private void comboBox_StallTstInc_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstInc = (byte)comboBox_StallTstInc.SelectedIndex;
	}

	private void comboBox_StallTstEn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byStallTstEn = (byte)comboBox_StallTstEn.SelectedIndex;
	}

	private void comboBox_DrvDisMod_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byDrvDisMod = (byte)comboBox_DrvDisMod.SelectedIndex;
	}

	private void comboBox_Cmp0CkSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byCmp0CkSel = (byte)comboBox_Cmp0CkSel.SelectedIndex;
	}

	private void comboBox_AdcVRef_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
	}

	private void comboBox_UvloDis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byUvloDis = (byte)comboBox_UvloDis.SelectedIndex;
	}

	private void comboBox_OvpDis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byOvpDis = (byte)comboBox_OvpDis.SelectedIndex;
	}

	private void comboBox_FocLdVoltKp_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp = (byte)comboBox_FocLdVoltKp.SelectedIndex;
	}

	private void comboBox_FocLqVoltKp_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp = (byte)comboBox_FocLqVoltKp.SelectedIndex;
	}

	private void comboBox_FocFluxVoltKp_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sSpecialParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp = (byte)comboBox_FocFluxVoltKp.SelectedIndex;
	}

	private void checkBox_ShowHallComMd_Click(object sender, RoutedEventArgs e)
	{
		mainWindow.ShowHallComMd(checkBox_ShowHallComMd.IsChecked == true);
	}
}
