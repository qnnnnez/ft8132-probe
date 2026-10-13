using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace FT8215Debugger;

public partial class UC_2Res_FOC_UVW_AS : UserControl, IComponentConnector
{
	private MainWindow mainWindow;

	private AdvancedSettingWindow m_AdvancedSettingWin;

	public UC_2Res_FOC_UVW_AS()
	{
		InitializeComponent();
		mainWindow = (MainWindow)Application.Current.MainWindow;
		InitUIParam();
		SetBinding2MainWindowCtrl();
		DelegateMainWindowEvent();
		m_AdvancedSettingWin = new AdvancedSettingWindow();
	}

	private void InitUIParam()
	{
		comboBox_FanDaBeiShu.ItemsSource = mainWindow.comboBox_FanDaBeiShu.Items;
		comboBox_RSDMode.ItemsSource = mainWindow.comboBox_RSDMode.Items;
		comboBox_FgDiv.ItemsSource = mainWindow.comboBox_FgDiv.Items;
		comboBox_FgMul.ItemsSource = mainWindow.comboBox_FgMul.Items;
		comboBox_SpdCtrlMD.ItemsSource = mainWindow.comboBox_SpdCtrlMD.Items;
		slider_BaseFreq.Minimum = mainWindow.slider_BaseFreq.Minimum;
		slider_BaseFreq.Maximum = mainWindow.slider_BaseFreq.Maximum;
		slider_BaseFreq.Ticks = mainWindow.slider_BaseFreq.Ticks;
		slider_SPDBw.Minimum = mainWindow.slider_SPDBw.Minimum;
		slider_SPDBw.Maximum = mainWindow.slider_SPDBw.Maximum;
		slider_SPDBw.Ticks = mainWindow.slider_SPDBw.Ticks;
		slider_StaticSpeed.Minimum = mainWindow.slider_StaticSpeed.Minimum;
		slider_StaticSpeed.Maximum = mainWindow.slider_StaticSpeed.Maximum;
		slider_StaticSpeed.Ticks = mainWindow.slider_StaticSpeed.Ticks;
		comboBox_ContrlMode.ItemsSource = mainWindow.comboBox_ContrlMode.Items;
		slider_EFreQAcc.Minimum = mainWindow.slider_EFreQAcc.Minimum;
		slider_EFreQAcc.Maximum = mainWindow.slider_EFreQAcc.Maximum;
		slider_EFreQAcc.Ticks = mainWindow.slider_EFreQAcc.Ticks;
		slider_EFreQMin.Minimum = mainWindow.slider_EFreQMin.Minimum;
		slider_EFreQMin.Maximum = mainWindow.slider_EFreQMin.Maximum;
		slider_EFreQMin.Ticks = mainWindow.slider_EFreQMin.Ticks;
		slider_WOutMax.Minimum = mainWindow.slider_WOutMax.Minimum;
		slider_WOutMax.Maximum = mainWindow.slider_WOutMax.Maximum;
		slider_WOutMax.Ticks = mainWindow.slider_WOutMax.Ticks;
		slider_StbInc.Minimum = mainWindow.slider_StbInc.Minimum;
		slider_StbInc.Maximum = mainWindow.slider_StbInc.Maximum;
		slider_StbInc.Ticks = mainWindow.slider_StbInc.Ticks;
		slider_StaInc.Minimum = mainWindow.slider_StaInc.Minimum;
		slider_StaInc.Maximum = mainWindow.slider_StaInc.Maximum;
		slider_StaInc.Ticks = mainWindow.slider_StaInc.Ticks;
		comboBox_FaultRestoreTimes.ItemsSource = mainWindow.comboBox_FaultRestoreTimes.Items;
		slider_LockEmf.Minimum = mainWindow.slider_LockEmf.Minimum;
		slider_LockEmf.Maximum = mainWindow.slider_LockEmf.Maximum;
		slider_LockEmf.Ticks = mainWindow.slider_LockEmf.Ticks;
	}

	private void SetBinding2MainWindowCtrl()
	{
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_JiDuiShu, textBox_JiDuiShu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DianZu, textBox_DianZu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DianGan, textBox_DianGan);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_FanDianDongShi, textBox_FanDianDongShi);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_CaiYandDianZu, textBox_CaiYandDianZu);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FanDaBeiShu, comboBox_FanDaBeiShu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_AtoBw, textBox_AtoBw);
		CommonFunc.SetBinding2Slider(mainWindow.slider_BaseFreq, slider_BaseFreq);
		CommonFunc.SetBinding2Slider(mainWindow.slider_SPDBw, slider_SPDBw);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_StaticSpeed, textBlock_StaticSpeed);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_RSDMode, comboBox_RSDMode);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StaticSpeed, slider_StaticSpeed);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgDiv, comboBox_FgDiv);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgMul, comboBox_FgMul);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_SpdCtrlMD, comboBox_SpdCtrlMD);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_ContrlMode, comboBox_ContrlMode);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_StartCurrent, textBox_StartCurrent);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_StartDuty30, textBox_StartDuty30);
		CommonFunc.SetBinding2Slider(mainWindow.slider_EFreQAcc, slider_EFreQAcc);
		CommonFunc.SetBinding2Slider(mainWindow.slider_EFreQMin, slider_EFreQMin);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_0x, radioButton_CloSpeed_0x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_1x, radioButton_CloSpeed_1x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_2x, radioButton_CloSpeed_2x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_CloSpeed_4x, radioButton_CloSpeed_4x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_EFreQHold_1x, radioButton_EFreQHold_1x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_EFreQHold_2x, radioButton_EFreQHold_2x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_EFreQHold_4x, radioButton_EFreQHold_4x);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioButton_EFreQHold_8x, radioButton_EFreQHold_8x);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DQKp, textBox_DQKp);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DQKi, textBox_DQKi);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_WKp, textBox_WKp);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_WKi, textBox_WKi);
		CommonFunc.SetBinding2Slider(mainWindow.slider_WOutMax, slider_WOutMax);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_WOutMax, textBlock_WOutMax);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StbInc, slider_StbInc);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StaInc, slider_StaInc);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FaultRestoreTimes, comboBox_FaultRestoreTimes);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Lock0Dis, radioBtn_Lock0Dis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Lock0En, radioBtn_Lock0En);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Lock1Dis, radioBtn_Lock1Dis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Lock1En, radioBtn_Lock1En);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Lock2Dis, radioBtn_Lock2Dis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Lock2En, radioBtn_Lock2En);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_LockMinSpeed, textBox_LockMinSpeed);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_LockMaxSpeed, textBox_LockMaxSpeed);
		CommonFunc.SetBinding2Slider(mainWindow.slider_LockEmf, slider_LockEmf);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_OcpDis0, radioBtn_OcpDis0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_OcpDis1, radioBtn_OcpDis1);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_OcpDa, textBox_OcpDa);
		ShowOcpDaVoltage(textBox_OcpDa.Text);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_DirInv0, radioBtn_DirInv0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_DirInv1, radioBtn_DirInv1);
	}

	private void DelegateMainWindowEvent()
	{
		textBox_JiDuiShu.LostFocus += mainWindow.textBox_JiDuiShu_LostFocus;
		textBox_JiDuiShu.KeyDown += mainWindow.textBox_JiDuiShu_KeyDown;
		textBox_DianZu.LostFocus += mainWindow.textBox_DianZu_LostFocus;
		textBox_DianGan.LostFocus += mainWindow.textBox_DianGan_LostFocus;
		textBox_FanDianDongShi.LostFocus += mainWindow.textBox_FanDianDongShi_LostFocus;
		textBox_CaiYandDianZu.LostFocus += mainWindow.textBox_CaiYandDianZu_LostFocus;
		comboBox_FanDaBeiShu.SelectionChanged += mainWindow.comboBox_FanDaBeiShu_SelectionChanged;
		textBox_AtoBw.LostFocus += mainWindow.textBox_AtoBw_LostFocus;
		slider_BaseFreq.ValueChanged += mainWindow.slider_BaseFreq_ValueChanged;
		comboBox_RSDMode.SelectionChanged += mainWindow.comboBox_RSDMode_SelectionChanged;
		slider_StaticSpeed.ValueChanged += mainWindow.slider_StaticSpeed_ValueChanged;
		comboBox_FgDiv.SelectionChanged += mainWindow.comboBox_FgDiv_SelectionChanged;
		comboBox_FgMul.SelectionChanged += mainWindow.comboBox_FgMul_SelectionChanged;
		comboBox_SpdCtrlMD.SelectionChanged += mainWindow.comboBox_SpdCtrlMD_SelectionChanged;
		comboBox_ContrlMode.SelectionChanged += mainWindow.comboBox_ContrlMode_SelectionChanged;
		textBox_StartCurrent.LostFocus += mainWindow.textBox_StartCurrent_LostFocus;
		textBox_StartDuty30.LostFocus += mainWindow.textBox_StartDuty30_LostFocus;
		textBox_DQKp.LostFocus += mainWindow.textBox_DQKp_LostFocus;
		textBox_DQKi.LostFocus += mainWindow.textBox_DQKi_LostFocus;
		textBox_WKp.LostFocus += mainWindow.textBox_WKp_LostFocus;
		textBox_WKi.LostFocus += mainWindow.textBox_WKi_LostFocus;
		slider_WOutMax.ValueChanged += mainWindow.slider_WOutMax_ValueChanged;
		comboBox_FaultRestoreTimes.SelectionChanged += mainWindow.comboBox_FaultRestoreTimes_SelectionChanged;
		textBox_LockMinSpeed.LostFocus += mainWindow.textBox_LockMinSpeed_LostFocus;
		textBox_LockMaxSpeed.LostFocus += mainWindow.textBox_LockMaxSpeed_LostFocus;
		textBox_OcpDa.LostFocus += mainWindow.textBox_OcpDa_LostFocus;
		textBox_OcpDa.KeyDown += mainWindow.textBox_OcpDa_KeyDown;
	}

	public void ShowOcpDaVoltage(string ocpDaText)
	{
		byte result = 0;
		byte.TryParse(ocpDaText, out result);
		double num = (double)(int)result / 255.0 * 5.0;
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
	}

	private void btn_Advanced_Click(object sender, RoutedEventArgs e)
	{
		m_AdvancedSettingWin.ShowDialog();
	}

	private void textBox_OcpDa_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			ShowOcpDaVoltage(textBox_OcpDa.Text);
		}
	}

	private void textBox_OcpDa_LostFocus(object sender, RoutedEventArgs e)
	{
		CheckOcpDaInputValid();
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
			if (mainWindow.m_byIsLoadDefault == 1)
			{
				textBox_OcpDa.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa.ToString();
			}
			else
			{
				textBox_OcpDa.Text = "0";
			}
		}
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa = b;
		ShowOcpDaVoltage(textBox_OcpDa.Text);
	}
}
