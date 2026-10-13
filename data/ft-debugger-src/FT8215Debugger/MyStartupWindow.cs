using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class MyStartupWindow : Window, IComponentConnector
{
	private const byte SENSOR_CIRCUIT_TYPE_COUNT = 7;

	private const byte SENSORLESS_CIRCUIT_TYPE_COUNT = 4;

	private short m_i16SensorMode = -1;

	private byte m_byCircuitType = 0;

	private bool m_bReload = false;

	private FileScriptParser commonConfigSetting = new FileScriptParser();

	public MyStartupWindow()
	{
		InitializeComponent();
		AddSensorCircuitTypeItems();
		AddSensorlessCircuitTypeItems();
		InitSensorModeAndCircuitType();
	}

	private void AddSensorCircuitTypeItems()
	{
		for (int i = 0; i < 7; i++)
		{
			string resourceKey = "SensorCircuitType" + i;
			comboBox_Sensor_CircuitType.Items.Add((string)FindResource(resourceKey));
		}
		comboBox_Sensor_CircuitType.SelectedIndex = 0;
	}

	private void AddSensorlessCircuitTypeItems()
	{
		for (int i = 0; i < 4; i++)
		{
			string resourceKey = "SensorlessCircuitType" + i;
			comboBox_Sensorless_CircuitType.Items.Add((string)FindResource(resourceKey));
		}
		comboBox_Sensorless_CircuitType.SelectedIndex = 0;
	}

	private void InitSensorModeAndCircuitType()
	{
		commonConfigSetting.LoadConfig(AppDomain.CurrentDomain.BaseDirectory + "UserConfigFile\\FTClosedConfig.pfd");
		short i16SensorMode = commonConfigSetting.m_i16SensorMode;
		byte byCircuitType = commonConfigSetting.m_byCircuitType;
		switch (i16SensorMode)
		{
		case 1:
			radioButton_Sensor.IsChecked = true;
			comboBox_Sensor_CircuitType.SelectedIndex = byCircuitType;
			break;
		case 2:
			radioButton_Sensorless.IsChecked = true;
			comboBox_Sensorless_CircuitType.SelectedIndex = byCircuitType;
			break;
		case 3:
			radioButton_2Hall_ICCoolFan.IsChecked = true;
			break;
		case 4:
			radioButton_RoboticVacuum.IsChecked = true;
			break;
		default:
			radionButton_Full.IsChecked = true;
			break;
		}
	}

	public short GetSensorMode()
	{
		return m_i16SensorMode;
	}

	public byte GetCircuitType()
	{
		return m_byCircuitType;
	}

	public bool GetReloadFlag()
	{
		return m_bReload;
	}

	private void radioButton_Sensor_Checked(object sender, RoutedEventArgs e)
	{
		m_i16SensorMode = 1;
		comboBox_Sensor_CircuitType.IsEnabled = true;
		comboBox_Sensorless_CircuitType.IsEnabled = false;
	}

	private void radioButton_Sensorless_Checked(object sender, RoutedEventArgs e)
	{
		m_i16SensorMode = 2;
		comboBox_Sensor_CircuitType.IsEnabled = false;
		comboBox_Sensorless_CircuitType.IsEnabled = true;
	}

	private void radionButton_Full_Checked(object sender, RoutedEventArgs e)
	{
		m_i16SensorMode = 0;
		m_byCircuitType = 0;
		comboBox_Sensor_CircuitType.IsEnabled = false;
		comboBox_Sensorless_CircuitType.IsEnabled = false;
	}

	private void radioButton_2Hall_ICCoolFan_Checked(object sender, RoutedEventArgs e)
	{
		m_i16SensorMode = 3;
		comboBox_Sensor_CircuitType.IsEnabled = false;
		comboBox_Sensorless_CircuitType.IsEnabled = false;
	}

	private void comboBox_Sensor_CircuitType_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		int selectedIndex = comboBox_Sensor_CircuitType.SelectedIndex;
		string toolTip = "";
		switch (selectedIndex)
		{
		case 0:
			toolTip = "3/2 HALL IC + SVPWM/单电阻FOC + ASpeed";
			break;
		case 1:
			toolTip = "3/2 HALL sensor + SVPWM/单电阻FOC + ASpeed";
			break;
		case 2:
			toolTip = "1 HALL IC/sensor + SVPWM/单电阻FOC + UV/UW + AS";
			break;
		case 3:
			toolTip = "2 HALL sensor + SVPWM/单电阻FOC + NTC + AS";
			break;
		case 4:
			toolTip = "3/2 HALL IC + 双电阻 FOC + AS";
			break;
		case 5:
			toolTip = "2 HALL sensor + 双电阻FOC + AS";
			break;
		case 6:
			toolTip = "3/2 HALL IC + 三电阻FOC + AS";
			break;
		}
		comboBox_Sensor_CircuitType.ToolTip = toolTip;
	}

	private void comboBox_Sensorless_CircuitType_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		int selectedIndex = comboBox_Sensorless_CircuitType.SelectedIndex;
		string toolTip = "";
		switch (selectedIndex)
		{
		case 0:
			toolTip = "单电阻FOC + UVW + AS";
			break;
		case 1:
			toolTip = "单电阻FOC + UV/UW + NTC + AS";
			break;
		case 2:
			toolTip = "双电阻FOC + UVW + AS";
			break;
		case 3:
			toolTip = "三电阻FOC + UVW + AS";
			break;
		}
		comboBox_Sensorless_CircuitType.ToolTip = toolTip;
	}

	private void button_Confirm_Click(object sender, RoutedEventArgs e)
	{
		if (radioButton_Sensor.IsChecked == false)
		{
			bool? isChecked = radioButton_Sensorless.IsChecked;
			if (isChecked != true && isChecked.HasValue && radionButton_Full.IsChecked == false)
			{
				isChecked = radioButton_2Hall_ICCoolFan.IsChecked;
				if (isChecked != true && isChecked.HasValue && radioButton_RoboticVacuum.IsChecked == false)
				{
					MessageBox.Show("Choose one of these four modes.", "Message", MessageBoxButton.OK);
					return;
				}
			}
		}
		if (m_i16SensorMode == 1)
		{
			m_byCircuitType = (byte)comboBox_Sensor_CircuitType.SelectedIndex;
		}
		else if (m_i16SensorMode == 2)
		{
			m_byCircuitType = (byte)comboBox_Sensorless_CircuitType.SelectedIndex;
		}
		else
		{
			m_byCircuitType = 0;
		}
		short i16SensorMode = commonConfigSetting.m_i16SensorMode;
		byte byCircuitType = commonConfigSetting.m_byCircuitType;
		if (m_i16SensorMode != i16SensorMode || m_byCircuitType != byCircuitType)
		{
			MessageBoxResult messageBoxResult = MessageBox.Show("The choice is different from last time! If you click [Yes] button, some parameters will be reset.\r\n\r\nStill confirm?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
			if (messageBoxResult != MessageBoxResult.Yes && messageBoxResult != MessageBoxResult.OK)
			{
				return;
			}
			m_bReload = true;
		}
		base.DialogResult = true;
		Close();
	}

	private void radioButton_RoboticVacuum_Checked(object sender, RoutedEventArgs e)
	{
		m_i16SensorMode = 4;
		comboBox_Sensor_CircuitType.IsEnabled = false;
		comboBox_Sensorless_CircuitType.IsEnabled = false;
	}
}
