using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FT8215Debugger.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[UserScopedSetting]
	public string CurrentConfig
	{
		get
		{
			return (string)this["CurrentConfig"];
		}
		set
		{
			this["CurrentConfig"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("1")]
	public byte RestartCount
	{
		get
		{
			return (byte)this["RestartCount"];
		}
		set
		{
			this["RestartCount"] = value;
		}
	}

	[DefaultSettingValue("-1")]
	[DebuggerNonUserCode]
	[UserScopedSetting]
	public short SensorMode
	{
		get
		{
			return (short)this["SensorMode"];
		}
		set
		{
			this["SensorMode"] = value;
		}
	}

	[DefaultSettingValue("0")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public string CircuitType
	{
		get
		{
			return (string)this["CircuitType"];
		}
		set
		{
			this["CircuitType"] = value;
		}
	}
}
