using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class AdvancedWindow : Window, IComponentConnector
{
	public MainWindow mainWindow { get; set; }

	public ParamStructureDef m_sMotorParamCfg { get; set; }

	public AdvancedWindow()
	{
		InitializeComponent();
		mainWindow = (MainWindow)Application.Current.MainWindow;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		base.Visibility = Visibility.Hidden;
	}

	public void comboBox_newFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		byte b = Convert.ToByte(mainWindow.textBox_SlvID.Text, 16);
		if (b == 127)
		{
			comboBox_newFunc.SelectedIndex = 1;
		}
		else
		{
			comboBox_newFunc.SelectedIndex = 0;
		}
		byte b2 = (byte)comboBox_newFunc.SelectedIndex;
		m_sMotorParamCfg.m_eSpecialRegCfg.byNewFunc = b2;
		if (b2 == 0)
		{
			comboBox_restaEn.IsEnabled = true;
			comboBox_IsdFwMd.IsEnabled = true;
			comboBox_lamd.IsEnabled = true;
		}
		else
		{
			comboBox_restaEn.IsEnabled = false;
			comboBox_IsdFwMd.IsEnabled = false;
			comboBox_lamd.IsEnabled = false;
		}
	}

	public void comboBox_restaEn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sMotorParamCfg.m_eSpecialRegCfg.byRestaEn = (byte)comboBox_restaEn.SelectedIndex;
	}

	public void comboBox_IsdFwMd_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sMotorParamCfg.m_eSpecialRegCfg.byIsdFwMd = (byte)comboBox_IsdFwMd.SelectedIndex;
	}

	public void comboBox_lamd_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		m_sMotorParamCfg.m_eSpecialRegCfg.byLaMd = (byte)comboBox_lamd.SelectedIndex;
	}
}
