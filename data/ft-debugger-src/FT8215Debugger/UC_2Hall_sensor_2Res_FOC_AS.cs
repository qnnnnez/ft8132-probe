using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace FT8215Debugger;

public partial class UC_2Hall_sensor_2Res_FOC_AS : UserControl, IComponentConnector
{
	private MainWindow mainWindow;

	private Sensor_AdvancedSettingWindow m_Sensor_AdvancedSettingWin;

	public UC_2Hall_sensor_2Res_FOC_AS()
	{
		InitializeComponent();
		mainWindow = (MainWindow)Application.Current.MainWindow;
		InitUIParam();
		SetBinding2MainWindowCtrl();
		DelegateMainWindowEvent();
		InitHallMosel();
		m_Sensor_AdvancedSettingWin = new Sensor_AdvancedSettingWindow();
	}

	private void InitUIParam()
	{
		comboBox_HallMode.ItemsSource = mainWindow.comboBox_HallMode.Items;
		slider_HallLockSel.Minimum = mainWindow.slider_HallLockSel.Minimum;
		slider_HallLockSel.Maximum = mainWindow.slider_HallLockSel.Maximum;
		slider_HallLockSel.Ticks = mainWindow.slider_HallLockSel.Ticks;
		slider_HallSwiMd.Minimum = mainWindow.slider_HallSwiMd.Minimum;
		slider_HallSwiMd.Maximum = mainWindow.slider_HallSwiMd.Maximum;
		slider_HallSwiMd.Ticks = mainWindow.slider_HallSwiMd.Ticks;
		comboBox_FanDaBeiShu.ItemsSource = mainWindow.comboBox_FanDaBeiShu.Items;
		comboBox_RSDMode.ItemsSource = mainWindow.comboBox_RSDMode.Items;
		comboBox_SpdCtrlMD.ItemsSource = mainWindow.comboBox_SpdCtrlMD.Items;
		slider_BaseFreq.Minimum = mainWindow.slider_BaseFreq.Minimum;
		slider_BaseFreq.Maximum = mainWindow.slider_BaseFreq.Maximum;
		slider_BaseFreq.Ticks = mainWindow.slider_BaseFreq.Ticks;
		slider_DTSEL.Minimum = mainWindow.slider_DTSEL.Minimum;
		slider_DTSEL.Maximum = mainWindow.slider_DTSEL.Maximum;
		slider_DTSEL.Ticks = mainWindow.slider_DTSEL.Ticks;
		slider_StaticSpeed.Minimum = mainWindow.slider_StaticSpeed.Minimum;
		slider_StaticSpeed.Maximum = mainWindow.slider_StaticSpeed.Maximum;
		slider_StaticSpeed.Ticks = mainWindow.slider_StaticSpeed.Ticks;
		comboBox_ContrlMode.ItemsSource = mainWindow.comboBox_ContrlMode.Items;
		slider_WOutMax.Minimum = mainWindow.slider_WOutMax.Minimum;
		slider_WOutMax.Maximum = mainWindow.slider_WOutMax.Maximum;
		slider_WOutMax.Ticks = mainWindow.slider_WOutMax.Ticks;
		slider_StbInc.Minimum = mainWindow.slider_StbInc.Minimum;
		slider_StbInc.Maximum = mainWindow.slider_StbInc.Maximum;
		slider_StbInc.Ticks = mainWindow.slider_StbInc.Ticks;
		slider_StaInc.Minimum = mainWindow.slider_StaInc.Minimum;
		slider_StaInc.Maximum = mainWindow.slider_StaInc.Maximum;
		slider_StaInc.Ticks = mainWindow.slider_StaInc.Ticks;
		comboBox_FgDiv.ItemsSource = mainWindow.comboBox_FgDiv.Items;
		comboBox_FgMul.ItemsSource = mainWindow.comboBox_FgMul.Items;
	}

	private void SetBinding2MainWindowCtrl()
	{
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_HallMode, comboBox_HallMode);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd0, radioBtn_HallComMd0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd1, radioBtn_HallComMd1);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd0, radioBtn_HallComMd0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd1, radioBtn_HallComMd1);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd3, radioBtn_HallComMd3);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd2, radioBtn_HallComMd2);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall0RevDis, radioBtn_Hall0RevDis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall0RevEn, radioBtn_Hall0RevEn);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall1RevDis, radioBtn_Hall1RevDis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall1RevEn, radioBtn_Hall1RevEn);
		CommonFunc.SetBinding2Slider(mainWindow.slider_HallLockSel, slider_HallLockSel);
		CommonFunc.SetBinding2Slider(mainWindow.slider_HallSwiMd, slider_HallSwiMd);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_HallSwiMd, textBlock_HallSwiMd);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_JiDuiShu, textBox_JiDuiShu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DianZu, textBox_DianZu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DianGan, textBox_DianGan);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_FanDianDongShi, textBox_FanDianDongShi);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_CaiYandDianZu, textBox_CaiYandDianZu);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FanDaBeiShu, comboBox_FanDaBeiShu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_AtoBw, textBox_AtoBw);
		CommonFunc.SetBinding2Slider(mainWindow.slider_BaseFreq, slider_BaseFreq);
		CommonFunc.SetBinding2Slider(mainWindow.slider_DTSEL, slider_DTSEL);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_RSDMode, comboBox_RSDMode);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StaticSpeed, slider_StaticSpeed);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_StaticSpeed, textBlock_StaticSpeed);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_SpdCtrlMD, comboBox_SpdCtrlMD);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_ContrlMode, comboBox_ContrlMode);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_StartCurrent, textBox_StartCurrent);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_StartDuty30, textBox_StartDuty30);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DQKp, textBox_DQKp);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DQKi, textBox_DQKi);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_WKp, textBox_WKp);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_WKi, textBox_WKi);
		CommonFunc.SetBinding2Slider(mainWindow.slider_WOutMax, slider_WOutMax);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_WOutMax, textBlock_WOutMax);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StbInc, slider_StbInc);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StaInc, slider_StaInc);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgDiv, comboBox_FgDiv);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgMul, comboBox_FgMul);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_OcpDis0, radioBtn_OcpDis0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_OcpDis1, radioBtn_OcpDis1);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_OcpDa, textBox_OcpDa);
		ShowOcpDaVoltage(textBox_OcpDa.Text);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_DirInv0, radioBtn_DirInv0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_DirInv1, radioBtn_DirInv1);
	}

	private void DelegateMainWindowEvent()
	{
		comboBox_HallMode.SelectionChanged += mainWindow.comboBox_HallMode_SelectionChanged;
		radioBtn_Hall0RevDis.Checked += mainWindow.radioBtn_Hall0RevDis_Checked;
		radioBtn_Hall0RevEn.Checked += mainWindow.radioBtn_Hall0RevEn_Checked;
		radioBtn_Hall1RevDis.Checked += mainWindow.radioBtn_Hall1RevDis_Checked;
		radioBtn_Hall1RevEn.Checked += mainWindow.radioBtn_Hall1RevEn_Checked;
		slider_HallLockSel.ValueChanged += mainWindow.slider_HallLockSel_ValueChanged;
		slider_HallSwiMd.ValueChanged += mainWindow.slider_HallSwiMd_ValueChanged;
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

	private void btn_Advanced_Click(object sender, RoutedEventArgs e)
	{
		m_Sensor_AdvancedSettingWin.ShowDialog();
	}

	private void radioBtn_HallIC_Checked(object sender, RoutedEventArgs e)
	{
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallModsel = 0;
		mainWindow.comboBox_CmpModeSel.SelectedValue = (string)FindResource("Cfg_Com0MdSel0");
	}

	private void radioBtn_HallSensor_Checked(object sender, RoutedEventArgs e)
	{
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallModsel = 1;
		mainWindow.comboBox_CmpModeSel.SelectedValue = (string)FindResource("Cfg_Com0MdSel8");
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

	public void InitHallMosel()
	{
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallModsel == 0)
		{
			radioBtn_HallIC.IsChecked = true;
		}
		else
		{
			radioBtn_HallSensor.IsChecked = true;
		}
	}
}
