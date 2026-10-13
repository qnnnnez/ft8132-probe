using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace FT8215Debugger;

public partial class uc_2HallICCoolFan_s2 : UserControl, IComponentConnector
{
	public AdvancedWindow adWindow;

	private MainWindow mainWindow { get; set; }

	public uc_2HallICCoolFan_s2(Window owner)
	{
		mainWindow = owner as MainWindow;
		InitializeComponent();
		InitUICtrl(owner);
		SetBinding2MainWindowCtrl(owner);
		Delegate2MainWindowEvent(owner);
	}

	private void InitUICtrl(Window owner)
	{
		comboBox_FgDiv.ItemsSource = mainWindow.comboBox_FgDiv.Items;
		comboBox_FgMul.ItemsSource = mainWindow.comboBox_FgMul.Items;
		comboBox_FgRdMd.ItemsSource = mainWindow.comboBox_FgRdMd.Items;
		slider_StaticSpeed.Minimum = mainWindow.slider_StaticSpeed.Minimum;
		slider_StaticSpeed.Maximum = mainWindow.slider_StaticSpeed.Maximum;
		slider_StaticSpeed.Ticks = mainWindow.slider_StaticSpeed.Ticks;
		comboBox_SpdCtrlMD.ItemsSource = mainWindow.comboBox_SpdCtrlMD.Items;
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
		comboBox_CmpModeSel.ItemsSource = mainWindow.comboBox_CmpModeSel.Items;
		slider_HallLockSel.Minimum = mainWindow.slider_HallLockSel.Minimum;
		slider_HallLockSel.Maximum = mainWindow.slider_HallLockSel.Maximum;
		slider_HallLockSel.Ticks = mainWindow.slider_HallLockSel.Ticks;
	}

	private void SetBinding2MainWindowCtrl(Window owner)
	{
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgDiv, comboBox_FgDiv);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgMul, comboBox_FgMul);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FgRdMd, comboBox_FgRdMd);
		CommonFunc.SetBinding2Slider(mainWindow.slider_StaticSpeed, slider_StaticSpeed);
		CommonFunc.SetBinding2TextBlock(mainWindow.textBlock_StaticSpeed, textBlock_StaticSpeed);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_SpdCtrlMD, comboBox_SpdCtrlMD);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_FaultRestoreTimes, comboBox_FaultRestoreTimes);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_OcpDa, textBox_OcpDa);
		CommonFunc.SetBinding2Slider(mainWindow.slider_UvpThr, slider_UvpThr);
		CommonFunc.SetBinding2TextBox(mainWindow.textBox_UvpThr, textBox_UvpThr);
		CommonFunc.SetBinding2Slider(mainWindow.slider_OvpThr, slider_OvpThr);
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
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_DirInv0, radioBtn_DirInv0);
		CommonFunc.SetBinding2RadioBtn(mainWindow.radioBtn_DirInv1, radioBtn_DirInv1);
		CommonFunc.SetBinding2Slider(mainWindow.slider_LimitSel, slider_LimitSel);
		CommonFunc.SetBinding2ComboBox(mainWindow.comboBox_CmpModeSel, comboBox_CmpModeSel);
		CommonFunc.SetBinding2Slider(mainWindow.slider_HallLockSel, slider_HallLockSel);
	}

	private void Delegate2MainWindowEvent(Window owner)
	{
		comboBox_FgDiv.SelectionChanged += mainWindow.comboBox_FgDiv_SelectionChanged;
		comboBox_FgMul.SelectionChanged += mainWindow.comboBox_FgMul_SelectionChanged;
		comboBox_FgRdMd.SelectionChanged += mainWindow.comboBox_FgRdMd_SelectionChanged;
		slider_StaticSpeed.ValueChanged += mainWindow.slider_StaticSpeed_ValueChanged;
		comboBox_SpdCtrlMD.SelectionChanged += mainWindow.comboBox_SpdCtrlMD_SelectionChanged;
		comboBox_FaultRestoreTimes.SelectionChanged += mainWindow.comboBox_FaultRestoreTimes_SelectionChanged;
		slider_UvpThr.ValueChanged += mainWindow.slider_UvpThr_ValueChanged;
		slider_OvpThr.ValueChanged += mainWindow.slider_OvpThr_ValueChanged;
		slider_TheCompSel.ValueChanged += mainWindow.slider_TheCompSel_ValueChanged;
		slider_LimitSel.ValueChanged += mainWindow.slider_LimitSel_ValueChanged;
		comboBox_CmpModeSel.SelectionChanged += mainWindow.comboBox_CmpModeSel_SelectionChanged;
		slider_HallLockSel.ValueChanged += mainWindow.slider_HallLockSel_ValueChanged;
	}

	private void textBox_OcpDa_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			ShowOcpDaVoltage(textBox_OcpDa.Text);
		}
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

	public void uc2hallIcCoolFans2ToMainWin()
	{
		mainWindow.comboBox_FgDiv.SelectedIndex = comboBox_FgDiv.SelectedIndex;
		mainWindow.comboBox_FgMul.SelectedIndex = comboBox_FgMul.SelectedIndex;
		mainWindow.comboBox_FgRdMd.SelectedIndex = comboBox_FgRdMd.SelectedIndex;
		mainWindow.slider_StaticSpeed.Value = slider_StaticSpeed.Value;
		mainWindow.comboBox_SpdCtrlMD.SelectedIndex = comboBox_SpdCtrlMD.SelectedIndex;
		mainWindow.comboBox_FaultRestoreTimes.SelectedIndex = comboBox_FaultRestoreTimes.SelectedIndex;
		mainWindow.textBox_OcpDa.Text = textBox_OcpDa.Text;
		mainWindow.slider_UvpThr.Value = slider_UvpThr.Value;
		mainWindow.slider_OvpThr.Value = slider_OvpThr.Value;
		mainWindow.slider_HallLockSel.Value = slider_HallLockSel.Value;
		mainWindow.radioBtn_CarrierSel0.IsChecked = radioBtn_CarrierSel0.IsChecked;
		mainWindow.radioBtn_CarrierSel1.IsChecked = radioBtn_CarrierSel1.IsChecked;
		mainWindow.radioBtn_CarrierSel2.IsChecked = radioBtn_CarrierSel2.IsChecked;
		mainWindow.radioBtn_CarrierSel3.IsChecked = radioBtn_CarrierSel3.IsChecked;
		mainWindow.radioBtn_McOffSel0.IsChecked = radioBtn_McOffSel0.IsChecked;
		mainWindow.radioBtn_McOffSel1.IsChecked = radioBtn_McOffSel1.IsChecked;
		mainWindow.radioBtn_McOffSel2.IsChecked = radioBtn_McOffSel2.IsChecked;
		mainWindow.radioBtn_McOffSel3.IsChecked = radioBtn_McOffSel3.IsChecked;
		mainWindow.slider_TheCompSel.Value = slider_TheCompSel.Value;
		mainWindow.radioBtn_DirInv0.IsChecked = radioBtn_DirInv0.IsChecked;
		mainWindow.radioBtn_DirInv1.IsChecked = radioBtn_DirInv1.IsChecked;
		mainWindow.slider_LimitSel.Value = slider_LimitSel.Value;
		mainWindow.comboBox_CmpModeSel.SelectedIndex = comboBox_CmpModeSel.SelectedIndex;
	}

	public void MainWinTouc2hallIcCoolFans2()
	{
		comboBox_FgDiv.SelectedIndex = mainWindow.comboBox_FgDiv.SelectedIndex;
		comboBox_FgMul.SelectedIndex = mainWindow.comboBox_FgMul.SelectedIndex;
		comboBox_FgRdMd.SelectedIndex = mainWindow.comboBox_FgRdMd.SelectedIndex;
		slider_StaticSpeed.Value = mainWindow.slider_StaticSpeed.Value;
		comboBox_SpdCtrlMD.SelectedIndex = mainWindow.comboBox_SpdCtrlMD.SelectedIndex;
		comboBox_FaultRestoreTimes.SelectedIndex = mainWindow.comboBox_FaultRestoreTimes.SelectedIndex;
		textBox_OcpDa.Text = mainWindow.textBox_OcpDa.Text;
		slider_UvpThr.Value = mainWindow.slider_UvpThr.Value;
		slider_OvpThr.Value = mainWindow.slider_OvpThr.Value;
		slider_HallLockSel.Value = mainWindow.slider_HallLockSel.Value;
		radioBtn_CarrierSel0.IsChecked = mainWindow.radioBtn_CarrierSel0.IsChecked;
		radioBtn_CarrierSel1.IsChecked = mainWindow.radioBtn_CarrierSel1.IsChecked;
		radioBtn_CarrierSel2.IsChecked = mainWindow.radioBtn_CarrierSel2.IsChecked;
		radioBtn_CarrierSel3.IsChecked = mainWindow.radioBtn_CarrierSel3.IsChecked;
		radioBtn_McOffSel0.IsChecked = mainWindow.radioBtn_McOffSel0.IsChecked;
		radioBtn_McOffSel1.IsChecked = mainWindow.radioBtn_McOffSel1.IsChecked;
		radioBtn_McOffSel2.IsChecked = mainWindow.radioBtn_McOffSel2.IsChecked;
		radioBtn_McOffSel3.IsChecked = mainWindow.radioBtn_McOffSel3.IsChecked;
		slider_TheCompSel.Value = mainWindow.slider_TheCompSel.Value;
		radioBtn_DirInv0.IsChecked = mainWindow.radioBtn_DirInv0.IsChecked;
		radioBtn_DirInv1.IsChecked = mainWindow.radioBtn_DirInv1.IsChecked;
		slider_LimitSel.Value = mainWindow.slider_LimitSel.Value;
		comboBox_CmpModeSel.SelectedIndex = mainWindow.comboBox_CmpModeSel.SelectedIndex;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		textBox_OcpDa.Text = mainWindow.textBox_OcpDa.Text;
		ShowOcpDaVoltage(textBox_OcpDa.Text);
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
		ShowOcpDaVoltage(text);
	}
}
