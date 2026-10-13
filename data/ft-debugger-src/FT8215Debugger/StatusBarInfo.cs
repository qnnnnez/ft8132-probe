using System;
using System.Windows.Media;

namespace FT8215Debugger;

public class StatusBarInfo : ICloneable
{
	public string DeviceStatus;

	public string Time;

	public Brush StatusLight;

	public string MotorStatus;

	public double Progress;

	public object Clone()
	{
		return MemberwiseClone();
	}
}
