using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class uc_2HallICCoolFan_s1 : UserControl, IComponentConnector
{
	private uc_2HallICCoolFan_s2 m_uc_2HallICCoolFan_s2Win;

	private MainWindow mainWindow { get; set; }

	public uc_2HallICCoolFan_s1(Window owner)
	{
		mainWindow = owner as MainWindow;
		InitializeComponent();
		SetBinding2MainWindowCtrl(owner);
		Delegate2MainWindowEvent(owner);
		InitUICtrl(owner);
		ShowHallComMd(isVisible: false);
	}

	private void InitUICtrl(Window owner)
	{
		slider_HallSwiMd.Minimum = mainWindow.slider_HallSwiMd.Minimum;
		slider_HallSwiMd.Maximum = mainWindow.slider_HallSwiMd.Maximum;
		slider_HallSwiMd.Ticks = mainWindow.slider_HallSwiMd.Ticks;
		LoadHallModel();
		comboBox_FanDaBeiShu.ItemsSource = mainWindow.comboBox_FanDaBeiShu.Items;
		slider_BaseFreq.Minimum = mainWindow.slider_BaseFreq.Minimum;
		slider_BaseFreq.Maximum = mainWindow.slider_BaseFreq.Maximum;
		slider_BaseFreq.Ticks = mainWindow.slider_BaseFreq.Ticks;
		slider_SampleTime.Minimum = mainWindow.slider_SampleTime.Minimum;
		slider_SampleTime.Maximum = mainWindow.slider_SampleTime.Maximum;
		slider_SampleTime.Ticks = mainWindow.slider_SampleTime.Ticks;
		slider_SPDBw.Minimum = mainWindow.slider_SPDBw.Minimum;
		slider_SPDBw.Maximum = mainWindow.slider_SPDBw.Maximum;
		slider_SPDBw.Ticks = mainWindow.slider_SPDBw.Ticks;
		slider_LeadAngle.Minimum = mainWindow.slider_LeadAngle.Minimum;
		slider_LeadAngle.Maximum = mainWindow.slider_LeadAngle.Maximum;
		slider_LeadAngle.Ticks = mainWindow.slider_LeadAngle.Ticks;
		slider_TSMIN.Minimum = mainWindow.slider_TSMIN.Minimum;
		slider_TSMIN.Maximum = mainWindow.slider_TSMIN.Maximum;
		slider_TSMIN.Ticks = mainWindow.slider_TSMIN.Ticks;
		slider_DTSEL.Minimum = mainWindow.slider_DTSEL.Minimum;
		slider_DTSEL.Maximum = mainWindow.slider_DTSEL.Maximum;
		slider_DTSEL.Ticks = mainWindow.slider_DTSEL.Ticks;
		comboBox_ContrlMode.ItemsSource = mainWindow.comboBox_ContrlMode.Items;
		slider_EFreQMin.Minimum = mainWindow.slider_EFreQMin.Minimum;
		slider_EFreQMin.Maximum = mainWindow.slider_EFreQMin.Maximum;
		slider_EFreQMin.Ticks = mainWindow.slider_EFreQMin.Ticks;
		slider_StbInc.Minimum = mainWindow.slider_StbInc.Minimum;
		slider_StbInc.Maximum = mainWindow.slider_StbInc.Maximum;
		slider_StbInc.Ticks = mainWindow.slider_StbInc.Ticks;
		slider_StaInc.Minimum = mainWindow.slider_StaInc.Minimum;
		slider_StaInc.Maximum = mainWindow.slider_StaInc.Maximum;
		slider_StaInc.Ticks = mainWindow.slider_StaInc.Ticks;
		slider_WOutMax.Minimum = mainWindow.slider_WOutMax.Minimum;
		slider_WOutMax.Maximum = mainWindow.slider_WOutMax.Maximum;
		slider_WOutMax.Ticks = mainWindow.slider_WOutMax.Ticks;
	}

	private void SetBinding2MainWindowCtrl(Window owner)
	{
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall0RevDis, radioBtn_Hall0RevDis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall0RevEn, radioBtn_Hall0RevEn);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall1RevDis, radioBtn_Hall1RevDis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_Hall1RevEn, radioBtn_Hall1RevEn);
		CommonFunc.SetBinding2Slider(mainWindow.slider_HallSwiMd, slider_HallSwiMd);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_HallSwiMd, textBlock_HallSwiMd);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallStaOpDis, radioBtn_HallStaOpDis);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallStaOpEn, radioBtn_HallStaOpEn);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd0, radioBtn_HallComMd0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd1, radioBtn_HallComMd1);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd3, radioBtn_HallComMd3);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd2, radioBtn_HallComMd2);
		CommonFunc.SetBinding2Label(mainWindow.label_HallComMd, label_HallComMd, UIElement.OpacityProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd0, radioBtn_HallComMd0, UIElement.OpacityProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd1, radioBtn_HallComMd1, UIElement.OpacityProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd3, radioBtn_HallComMd3, UIElement.OpacityProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd2, radioBtn_HallComMd2, UIElement.OpacityProperty);
		CommonFunc.SetBinding2Label(mainWindow.label_HallComMd, label_HallComMd, UIElement.IsEnabledProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd0, radioBtn_HallComMd0, UIElement.IsEnabledProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd1, radioBtn_HallComMd1, UIElement.IsEnabledProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd3, radioBtn_HallComMd3, UIElement.IsEnabledProperty);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_HallComMd2, radioBtn_HallComMd2, UIElement.IsEnabledProperty);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_JiDuiShu, textBox_JiDuiShu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DianZu, textBox_DianZu);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_DianGan, textBox_DianGan);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_FanDianDongShi, textBox_FanDianDongShi);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_CaiYandDianZu, textBox_CaiYandDianZu);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FanDaBeiShu, comboBox_FanDaBeiShu);
		CommonFunc.SetBinding2Slider(mainWindow.slider_BaseFreq, slider_BaseFreq);
		CommonFunc.SetBinding2Slider(mainWindow.slider_SampleTime, slider_SampleTime);
		CommonFunc.SetBinding2Slider(mainWindow.slider_SPDBw, slider_SPDBw);
		CommonFunc.SetBinding2Slider(mainWindow.slider_LeadAngle, slider_LeadAngle);
		CommonFunc.SetBinding2Slider(mainWindow.slider_TSMIN, slider_TSMIN);
		CommonFunc.SetBinding2Slider(mainWindow.slider_DTSEL, slider_DTSEL);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_AtoBw, textBox_AtoBw);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_ContrlMode, comboBox_ContrlMode);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_StartCurrent, textBox_StartCurrent);
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
		CommonFunc.SetBinding2Slider(mainWindow.slider_StbInc, slider_StbInc);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StaInc, slider_StaInc);
		CommonFunc.SetBinding2Slider(mainWindow.slider_WOutMax, slider_WOutMax);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_WOutMax, textBlock_WOutMax);
	}

	private void Delegate2MainWindowEvent(Window owner)
	{
		radioBtn_Hall0RevDis.Checked += mainWindow.radioBtn_Hall0RevDis_Checked;
		radioBtn_Hall0RevEn.Checked += mainWindow.radioBtn_Hall0RevEn_Checked;
		radioBtn_Hall1RevDis.Checked += mainWindow.radioBtn_Hall1RevDis_Checked;
		radioBtn_Hall1RevEn.Checked += mainWindow.radioBtn_Hall1RevEn_Checked;
		slider_HallSwiMd.ValueChanged += mainWindow.slider_HallSwiMd_ValueChanged;
		textBox_JiDuiShu.LostFocus += mainWindow.textBox_JiDuiShu_LostFocus;
		textBox_JiDuiShu.KeyDown += mainWindow.textBox_JiDuiShu_KeyDown;
		textBox_DianZu.LostFocus += mainWindow.textBox_DianZu_LostFocus;
		textBox_DianGan.LostFocus += mainWindow.textBox_DianGan_LostFocus;
		textBox_FanDianDongShi.LostFocus += mainWindow.textBox_FanDianDongShi_LostFocus;
		textBox_CaiYandDianZu.LostFocus += mainWindow.textBox_CaiYandDianZu_LostFocus;
		comboBox_FanDaBeiShu.SelectionChanged += mainWindow.comboBox_FanDaBeiShu_SelectionChanged;
		slider_BaseFreq.ValueChanged += mainWindow.slider_BaseFreq_ValueChanged;
		textBox_AtoBw.LostFocus += mainWindow.textBox_AtoBw_LostFocus;
		comboBox_ContrlMode.SelectionChanged += mainWindow.comboBox_ContrlMode_SelectionChanged;
		textBox_StartCurrent.LostFocus += mainWindow.textBox_StartCurrent_LostFocus;
		textBox_DQKp.LostFocus += mainWindow.textBox_DQKp_LostFocus;
		textBox_DQKi.LostFocus += mainWindow.textBox_DQKi_LostFocus;
		textBox_WKp.LostFocus += mainWindow.textBox_WKp_LostFocus;
		textBox_WKi.LostFocus += mainWindow.textBox_WKi_LostFocus;
		slider_WOutMax.ValueChanged += mainWindow.slider_WOutMax_ValueChanged;
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

	public void uc2hallIcCoolFans1ToMainWin()
	{
		mainWindow.radioBtn_Hall0RevDis.IsChecked = radioBtn_Hall0RevDis.IsChecked;
		mainWindow.radioBtn_Hall0RevEn.IsChecked = radioBtn_Hall0RevEn.IsChecked;
		mainWindow.radioBtn_Hall1RevDis.IsChecked = radioBtn_Hall1RevDis.IsChecked;
		mainWindow.radioBtn_Hall1RevEn.IsChecked = radioBtn_Hall1RevEn.IsChecked;
		mainWindow.slider_HallSwiMd.Value = slider_HallSwiMd.Value;
		mainWindow.radioBtn_HallStaOpDis.IsChecked = radioBtn_HallStaOpDis.IsChecked;
		mainWindow.radioBtn_HallStaOpEn.IsChecked = radioBtn_HallStaOpEn.IsChecked;
		mainWindow.radioBtn_HallComMd0.IsChecked = radioBtn_HallComMd0.IsChecked;
		mainWindow.radioBtn_HallComMd1.IsChecked = radioBtn_HallComMd1.IsChecked;
		mainWindow.radioBtn_HallComMd3.IsChecked = radioBtn_HallComMd3.IsChecked;
		mainWindow.radioBtn_HallComMd2.IsChecked = radioBtn_HallComMd2.IsChecked;
		mainWindow.textBox_JiDuiShu.Text = textBox_JiDuiShu.Text;
		mainWindow.textBox_DianZu.Text = textBox_DianZu.Text;
		mainWindow.textBox_DianGan.Text = textBox_DianGan.Text;
		mainWindow.textBox_FanDianDongShi.Text = textBox_FanDianDongShi.Text;
		mainWindow.textBox_CaiYandDianZu.Text = textBox_CaiYandDianZu.Text;
		mainWindow.comboBox_FanDaBeiShu.SelectedIndex = comboBox_FanDaBeiShu.SelectedIndex;
		mainWindow.slider_BaseFreq.Value = slider_BaseFreq.Value;
		mainWindow.slider_SampleTime.Value = slider_SampleTime.Value;
		mainWindow.slider_SPDBw.Value = slider_SPDBw.Value;
		mainWindow.slider_LeadAngle.Value = slider_LeadAngle.Value;
		mainWindow.slider_TSMIN.Value = slider_TSMIN.Value;
		mainWindow.slider_DTSEL.Value = slider_DTSEL.Value;
		mainWindow.textBox_AtoBw.Text = textBox_AtoBw.Text;
		mainWindow.comboBox_ContrlMode.SelectedIndex = comboBox_ContrlMode.SelectedIndex;
		mainWindow.textBox_StartCurrent.Text = textBox_StartCurrent.Text;
		mainWindow.slider_EFreQMin.Value = slider_EFreQMin.Value;
		mainWindow.radioButton_CloSpeed_0x.IsChecked = radioButton_CloSpeed_0x.IsChecked;
		mainWindow.radioButton_CloSpeed_1x.IsChecked = radioButton_CloSpeed_1x.IsChecked;
		mainWindow.radioButton_CloSpeed_2x.IsChecked = radioButton_CloSpeed_2x.IsChecked;
		mainWindow.radioButton_CloSpeed_4x.IsChecked = radioButton_CloSpeed_4x.IsChecked;
		mainWindow.radioButton_EFreQHold_1x.IsChecked = radioButton_EFreQHold_1x.IsChecked;
		mainWindow.radioButton_EFreQHold_2x.IsChecked = radioButton_EFreQHold_2x.IsChecked;
		mainWindow.radioButton_EFreQHold_4x.IsChecked = radioButton_EFreQHold_4x.IsChecked;
		mainWindow.radioButton_EFreQHold_8x.IsChecked = radioButton_EFreQHold_8x.IsChecked;
		mainWindow.textBox_DQKp.Text = textBox_DQKp.Text;
		mainWindow.textBox_DQKi.Text = textBox_DQKi.Text;
		mainWindow.textBox_WKp.Text = textBox_WKp.Text;
		mainWindow.textBox_WKi.Text = textBox_WKi.Text;
		mainWindow.slider_StbInc.Value = slider_StbInc.Value;
		mainWindow.slider_StaInc.Value = slider_StaInc.Value;
		mainWindow.slider_WOutMax.Value = slider_WOutMax.Value;
	}

	public void MainWinTouc2hallIcCoolFans1()
	{
		radioBtn_Hall0RevDis.IsChecked = mainWindow.radioBtn_Hall0RevDis.IsChecked;
		radioBtn_Hall0RevEn.IsChecked = mainWindow.radioBtn_Hall0RevEn.IsChecked;
		radioBtn_Hall1RevDis.IsChecked = mainWindow.radioBtn_Hall1RevDis.IsChecked;
		radioBtn_Hall1RevEn.IsChecked = mainWindow.radioBtn_Hall1RevEn.IsChecked;
		slider_HallSwiMd.Value = mainWindow.slider_HallSwiMd.Value;
		radioBtn_HallStaOpDis.IsChecked = mainWindow.radioBtn_HallStaOpDis.IsChecked;
		radioBtn_HallStaOpEn.IsChecked = mainWindow.radioBtn_HallStaOpEn.IsChecked;
		LoadHallModel();
		radioBtn_HallComMd0.IsChecked = mainWindow.radioBtn_HallComMd0.IsChecked;
		radioBtn_HallComMd1.IsChecked = mainWindow.radioBtn_HallComMd1.IsChecked;
		radioBtn_HallComMd3.IsChecked = mainWindow.radioBtn_HallComMd3.IsChecked;
		radioBtn_HallComMd2.IsChecked = mainWindow.radioBtn_HallComMd2.IsChecked;
		textBox_JiDuiShu.Text = mainWindow.textBox_JiDuiShu.Text;
		textBox_DianZu.Text = mainWindow.textBox_DianZu.Text;
		textBox_DianGan.Text = mainWindow.textBox_DianGan.Text;
		textBox_FanDianDongShi.Text = mainWindow.textBox_FanDianDongShi.Text;
		textBox_CaiYandDianZu.Text = mainWindow.textBox_CaiYandDianZu.Text;
		comboBox_FanDaBeiShu.SelectedIndex = mainWindow.comboBox_FanDaBeiShu.SelectedIndex;
		slider_BaseFreq.Value = mainWindow.slider_BaseFreq.Value;
		slider_SampleTime.Value = mainWindow.slider_SampleTime.Value;
		slider_SPDBw.Value = mainWindow.slider_SPDBw.Value;
		slider_LeadAngle.Value = mainWindow.slider_LeadAngle.Value;
		slider_TSMIN.Value = mainWindow.slider_TSMIN.Value;
		slider_DTSEL.Value = mainWindow.slider_DTSEL.Value;
		textBox_AtoBw.Text = mainWindow.textBox_AtoBw.Text;
		comboBox_ContrlMode.SelectedIndex = mainWindow.comboBox_ContrlMode.SelectedIndex;
		textBox_StartCurrent.Text = mainWindow.textBox_StartCurrent.Text;
		slider_EFreQMin.Value = mainWindow.slider_EFreQMin.Value;
		radioButton_CloSpeed_0x.IsChecked = mainWindow.radioButton_CloSpeed_0x.IsChecked;
		radioButton_CloSpeed_1x.IsChecked = mainWindow.radioButton_CloSpeed_1x.IsChecked;
		radioButton_CloSpeed_2x.IsChecked = mainWindow.radioButton_CloSpeed_2x.IsChecked;
		radioButton_CloSpeed_4x.IsChecked = mainWindow.radioButton_CloSpeed_4x.IsChecked;
		radioButton_EFreQHold_1x.IsChecked = mainWindow.radioButton_EFreQHold_1x.IsChecked;
		radioButton_EFreQHold_2x.IsChecked = mainWindow.radioButton_EFreQHold_2x.IsChecked;
		radioButton_EFreQHold_4x.IsChecked = mainWindow.radioButton_EFreQHold_4x.IsChecked;
		radioButton_EFreQHold_8x.IsChecked = mainWindow.radioButton_EFreQHold_8x.IsChecked;
		textBox_DQKp.Text = mainWindow.textBox_DQKp.Text;
		textBox_DQKi.Text = mainWindow.textBox_DQKi.Text;
		textBox_WKp.Text = mainWindow.textBox_WKp.Text;
		textBox_WKi.Text = mainWindow.textBox_WKi.Text;
		slider_StbInc.Value = mainWindow.slider_StbInc.Value;
		slider_StaInc.Value = mainWindow.slider_StaInc.Value;
		slider_WOutMax.Value = mainWindow.slider_WOutMax.Value;
	}

	public void LoadHallModel()
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
