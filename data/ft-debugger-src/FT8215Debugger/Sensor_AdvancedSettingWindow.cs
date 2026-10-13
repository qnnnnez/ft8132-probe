using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class Sensor_AdvancedSettingWindow : Window, IComponentConnector
{
	private MainWindow mainWindow;

	public Sensor_AdvancedSettingWindow()
	{
		InitializeComponent();
		mainWindow = (MainWindow)Application.Current.MainWindow;
		InitUIParam();
		SetBinding2MainWindowCtrl();
		DelegateMainWindowEvent();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		base.Visibility = Visibility.Hidden;
	}

	private void InitUIParam()
	{
		comboBox_Seg7En.ItemsSource = mainWindow.comboBox_Seg7En.Items;
		slider_SPDBw.Minimum = mainWindow.slider_SPDBw.Minimum;
		slider_SPDBw.Maximum = mainWindow.slider_SPDBw.Maximum;
		slider_SPDBw.Ticks = mainWindow.slider_SPDBw.Ticks;
		slider_TSMIN.Minimum = mainWindow.slider_TSMIN.Minimum;
		slider_TSMIN.Maximum = mainWindow.slider_TSMIN.Maximum;
		slider_TSMIN.Ticks = mainWindow.slider_TSMIN.Ticks;
		slider_EFreQMin.Minimum = mainWindow.slider_EFreQMin.Minimum;
		slider_EFreQMin.Maximum = mainWindow.slider_EFreQMin.Maximum;
		slider_EFreQMin.Ticks = mainWindow.slider_EFreQMin.Ticks;
		comboBox_FaultRestoreTimes.ItemsSource = mainWindow.comboBox_FaultRestoreTimes.Items;
		slider_UvpThr.Minimum = mainWindow.slider_UvpThr.Minimum;
		slider_UvpThr.Maximum = mainWindow.slider_UvpThr.Maximum;
		slider_UvpThr.Ticks = mainWindow.slider_UvpThr.Ticks;
		slider_OvpThr.Minimum = mainWindow.slider_OvpThr.Minimum;
		slider_OvpThr.Maximum = mainWindow.slider_OvpThr.Maximum;
		slider_OvpThr.Ticks = mainWindow.slider_OvpThr.Ticks;
		slider_TheCompSel.Minimum = mainWindow.slider_TheCompSel.Minimum;
		slider_TheCompSel.Maximum = mainWindow.slider_TheCompSel.Maximum;
		slider_TheCompSel.Ticks = mainWindow.slider_TheCompSel.Ticks;
		slider_LimitSel.Minimum = mainWindow.slider_LimitSel.Minimum;
		slider_LimitSel.Maximum = mainWindow.slider_LimitSel.Maximum;
		slider_LimitSel.Ticks = mainWindow.slider_LimitSel.Ticks;
	}

	private void SetBinding2MainWindowCtrl()
	{
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_Seg7En, comboBox_Seg7En);
		CommonFunc.SetBinding2Slider(mainWindow.slider_TSMIN, slider_TSMIN);
		CommonFunc.SetBinding2Slider(mainWindow.slider_SPDBw, slider_SPDBw);
		CommonFunc.SetBinding2Slider(mainWindow.slider_EFreQMin, slider_EFreQMin);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_0x, radioButton_CloSpeed_0x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_1x, radioButton_CloSpeed_1x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_2x, radioButton_CloSpeed_2x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_4x, radioButton_CloSpeed_4x);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FaultRestoreTimes, comboBox_FaultRestoreTimes);
		CommonFunc.SetBinding2Slider(mainWindow.slider_UvpThr, slider_UvpThr);
		CommonFunc.SetBinding2Slider(mainWindow.slider_OvpThr, slider_OvpThr);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_UvpThr, textBox_UvpThr);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_OvpThr, textBox_OvpThr);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_CarrierSel0, radioBtn_CarrierSel0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_CarrierSel1, radioBtn_CarrierSel1);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_CarrierSel2, radioBtn_CarrierSel2);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_CarrierSel3, radioBtn_CarrierSel3);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_McOffSel0, radioBtn_McOffSel0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_McOffSel1, radioBtn_McOffSel1);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_McOffSel2, radioBtn_McOffSel2);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_McOffSel3, radioBtn_McOffSel3);
		CommonFunc.SetBinding2Slider(mainWindow.slider_TheCompSel, slider_TheCompSel);
		CommonFunc.SetBinding2Slider(mainWindow.slider_LimitSel, slider_LimitSel);
	}

	private void DelegateMainWindowEvent()
	{
		comboBox_Seg7En.SelectionChanged += mainWindow.comboBox_Seg7En_SelectionChanged;
		comboBox_FaultRestoreTimes.SelectionChanged += mainWindow.comboBox_FaultRestoreTimes_SelectionChanged;
		slider_UvpThr.ValueChanged += mainWindow.slider_UvpThr_ValueChanged;
		slider_OvpThr.ValueChanged += mainWindow.slider_OvpThr_ValueChanged;
		slider_TheCompSel.ValueChanged += mainWindow.slider_TheCompSel_ValueChanged;
		slider_LimitSel.ValueChanged += mainWindow.slider_LimitSel_ValueChanged;
	}
}
