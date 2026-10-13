using System;

namespace FT8215Debugger;

public class PassValuesEventArgs : EventArgs
{
	private string _name;

	private string _place;

	private int _age;

	private ParamStructureDef.S_SpecialRegCfg m_sPassSpecialRegCfg;

	public ParamStructureDef.S_SpecialRegCfg SpecialParamCfg
	{
		get
		{
			return m_sPassSpecialRegCfg;
		}
		set
		{
			m_sPassSpecialRegCfg = value;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public string Place
	{
		get
		{
			return _place;
		}
		set
		{
			_place = value;
		}
	}

	public int Age
	{
		get
		{
			return _age;
		}
		set
		{
			_age = value;
		}
	}

	public PassValuesEventArgs()
	{
	}

	public PassValuesEventArgs(ParamStructureDef.S_SpecialRegCfg sMotorParamCfg)
	{
		m_sPassSpecialRegCfg = sMotorParamCfg;
	}

	public PassValuesEventArgs(string refName, string refPlace, int refAge)
	{
		_name = refName;
		_place = refPlace;
		_age = refAge;
	}
}
