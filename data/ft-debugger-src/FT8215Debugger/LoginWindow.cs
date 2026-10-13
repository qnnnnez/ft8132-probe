using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class LoginWindow : Window, IComponentConnector
{
	public MainWindow mainWindow { get; set; }

	public LoginWindow()
	{
		InitializeComponent();
		mainWindow = (MainWindow)Application.Current.MainWindow;
		FocusManager.SetFocusedElement(this, passwordBox_Super);
		passwordBox_Super.SelectAll();
		passwordBox_Super.Focus();
	}

	private void btn_OK_Click(object sender, RoutedEventArgs e)
	{
		HandleOK();
	}

	private int HandleOK()
	{
		int result = 0;
		passwordBox_Super.Password = passwordBox_Super.Password.Trim();
		string text = "fT8102";
		if (passwordBox_Super.Password.Length > 0)
		{
			if (passwordBox_Super.Password == text)
			{
				SpecialSettingWindow specialSettingWindow = new SpecialSettingWindow();
				specialSettingWindow.m_sSpecialParamCfg = mainWindow.m_sMotorParamCfg;
				SpecialSettingWindow specialSettingWindow2 = specialSettingWindow;
				specialSettingWindow2.PassValuesEvent += ReceiveValues;
				base.Visibility = Visibility.Hidden;
				specialSettingWindow2.ShowDialog();
				Close();
			}
			else
			{
				MessageBox.Show("Error password，please input correct password！");
				passwordBox_Super.Password = "";
				result = 1;
			}
		}
		else
		{
			MessageBox.Show("The input string should not be empty！");
			result = 2;
		}
		return result;
	}

	private void ReceiveValues(object sender, PassValuesEventArgs e)
	{
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byOcpSel = e.SpecialParamCfg.byOcpSel;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byIsdDirRev = e.SpecialParamCfg.byIsdDirRev;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byOvpSel = e.SpecialParamCfg.byOvpSel;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byTsdSel = e.SpecialParamCfg.byTsdSel;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byDzSet = e.SpecialParamCfg.byDzSet;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byDpmDis = e.SpecialParamCfg.byDpmDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byUvloDis = e.SpecialParamCfg.byUvloDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byOvpDis = e.SpecialParamCfg.byOvpDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byOcpDis = e.SpecialParamCfg.byOcpDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byTsdDis = e.SpecialParamCfg.byTsdDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byFocDis = e.SpecialParamCfg.byFocDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byBootMod = e.SpecialParamCfg.byBootMod;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byIsdBrakeEn = e.SpecialParamCfg.byIsdBrakeEn;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byIdleBrakeEn = e.SpecialParamCfg.byIdleBrakeEn;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byTSMin = e.SpecialParamCfg.byTSMin;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byRvsAlignEn = e.SpecialParamCfg.byRvsAlignEn;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byRvsCutSel = e.SpecialParamCfg.byRvsCutSel;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byNewSingle = e.SpecialParamCfg.byNewSingle;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byDigTst = e.SpecialParamCfg.byDigTst;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byDigMoniCt = e.SpecialParamCfg.byDigMoniCt;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.fRV = e.SpecialParamCfg.fRV;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byTestMode = e.SpecialParamCfg.byTestMode;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byIrefDis = e.SpecialParamCfg.byIrefDis;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLdVoltKp = e.SpecialParamCfg.byFlocLdVoltKp;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byFlocLqVoltKp = e.SpecialParamCfg.byFlocLqVoltKp;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byFocFluxVoltKp = e.SpecialParamCfg.byFocFluxVoltKp;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn = e.SpecialParamCfg.byRestaEn;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd = e.SpecialParamCfg.byIsdFwMd;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd = e.SpecialParamCfg.byLaMd;
		mainWindow.m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = e.SpecialParamCfg.byNewFunc;
	}

	private void btn_Cancel_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ShowModalDialog(bool bShow)
	{
	}

	private void passwordBox_Super_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			HandleOK();
		}
	}
}
