using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class AdvancedSettingWindow : Window, IComponentConnector
{
	private MainWindow mainWindow;

	public AdvancedSettingWindow()
	{
		InitializeComponent();
		mainWindow = (MainWindow)Application.Current.MainWindow;
		InitUIParam();
		SetBinding2MainWindowCtrl();
		DelegateMainWindowEvent();
	}

	private void InitUIParam()
	{
		comboBox_Seg7En.ItemsSource = mainWindow.comboBox_Seg7En.Items;
		slider_LeadAngle.Minimum = mainWindow.slider_LeadAngle.Minimum;
		slider_LeadAngle.Maximum = mainWindow.slider_LeadAngle.Maximum;
		slider_LeadAngle.Ticks = mainWindow.slider_LeadAngle.Ticks;
		slider_EKNum.Minimum = mainWindow.slider_EKNum.Minimum;
		slider_EKNum.Maximum = mainWindow.slider_EKNum.Maximum;
		slider_EKNum.Ticks = mainWindow.slider_EKNum.Ticks;
		slider_TSMIN.Minimum = mainWindow.slider_TSMIN.Minimum;
		slider_TSMIN.Maximum = mainWindow.slider_TSMIN.Maximum;
		slider_TSMIN.Ticks = mainWindow.slider_TSMIN.Ticks;
		slider_DTSEL.Minimum = mainWindow.slider_DTSEL.Minimum;
		slider_DTSEL.Maximum = mainWindow.slider_DTSEL.Maximum;
		slider_DTSEL.Ticks = mainWindow.slider_DTSEL.Ticks;
		comboBox_RPDAlign.ItemsSource = mainWindow.comboBox_RPDAlign.Items;
		slider_RPD12.Minimum = mainWindow.slider_RPD12.Minimum;
		slider_RPD12.Maximum = mainWindow.slider_RPD12.Maximum;
		slider_RPD12.Ticks = mainWindow.slider_RPD12.Ticks;
		slider_AlignDuty.Minimum = mainWindow.slider_AlignDuty.Minimum;
		slider_AlignDuty.Maximum = mainWindow.slider_AlignDuty.Maximum;
		slider_AlignDuty.Ticks = mainWindow.slider_AlignDuty.Ticks;
		slider_AlignTime.Minimum = mainWindow.slider_AlignTime.Minimum;
		slider_AlignTime.Maximum = mainWindow.slider_AlignTime.Maximum;
		slider_AlignTime.Ticks = mainWindow.slider_AlignTime.Ticks;
		slider_RPD21.Minimum = mainWindow.slider_RPD21.Minimum;
		slider_RPD21.Maximum = mainWindow.slider_RPD21.Maximum;
		slider_RPD21.Ticks = mainWindow.slider_RPD21.Ticks;
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
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_MfpEk3, textBox_MfpEk3);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_Seg7En, comboBox_Seg7En);
		CommonFunc.SetBinding2Slider(mainWindow.slider_TSMIN, slider_TSMIN);
		CommonFunc.SetBinding2Slider(mainWindow.slider_LeadAngle, slider_LeadAngle);
		CommonFunc.SetBinding2Slider(mainWindow.slider_EKNum, slider_EKNum);
		CommonFunc.SetBinding2Slider(mainWindow.slider_DTSEL, slider_DTSEL);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_StaIncTime_1s, radioButton_StaIncTime_1s);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_StaIncTime_2s, radioButton_StaIncTime_2s);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_StaIncTime_4s, radioButton_StaIncTime_4s);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_StaIncTime_6s, radioButton_StaIncTime_6s);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_RPDAlign, comboBox_RPDAlign);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_RPD10_6us, radioBtn_RPD10_6us);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_RPD10_4us, radioBtn_RPD10_4us);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_RPD11_2us, radioBtn_RPD11_2us);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_RPD11_4us, radioBtn_RPD11_4us);
		CommonFunc.SetBinding2Slider(mainWindow.slider_RPD12, slider_RPD12);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_RPD12, textBlock_RPD12);
		CommonFunc.SetBinding2Slider(mainWindow.slider_AlignDuty, slider_AlignDuty);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_AlignDuty, textBlock_AlignDuty);
		CommonFunc.SetBinding2Slider(mainWindow.slider_AlignTime, slider_AlignTime);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_AlignTime, textBlock_AlignTime);
		CommonFunc.SetBinding2Slider(mainWindow.slider_RPD21, slider_RPD21);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_RPD21, textBlock_RPD21);
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
		textBox_MfpEk3.LostFocus += mainWindow.textBox_MfpEk3_LostFocus;
		comboBox_Seg7En.SelectionChanged += mainWindow.comboBox_Seg7En_SelectionChanged;
		comboBox_RPDAlign.SelectionChanged += mainWindow.comboBox_RPDAlign_SelectionChanged;
		slider_AlignDuty.ValueChanged += mainWindow.slider_AlignDuty_ValueChanged;
		slider_AlignTime.ValueChanged += mainWindow.slider_AlignTime_ValueChanged;
		slider_UvpThr.ValueChanged += mainWindow.slider_UvpThr_ValueChanged;
		slider_OvpThr.ValueChanged += mainWindow.slider_OvpThr_ValueChanged;
		slider_TheCompSel.ValueChanged += mainWindow.slider_TheCompSel_ValueChanged;
		slider_LimitSel.ValueChanged += mainWindow.slider_LimitSel_ValueChanged;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		mainWindow.textBox_MfpEk3.Text = textBox_MfpEk3.Text;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3 = float.Parse(textBox_MfpEk3.Text);
		e.Cancel = true;
		base.Visibility = Visibility.Hidden;
	}
}
