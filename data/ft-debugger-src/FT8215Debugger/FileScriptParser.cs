using System;
using System.Windows;
using System.Xml;

namespace FT8215Debugger;

public class FileScriptParser
{
	public string m_strSwName { get; set; }

	public string m_sAppVersion { get; set; }

	public short m_i16SensorMode { get; set; }

	public byte m_byCircuitType { get; set; }

	public byte m_byPolePairs { get; set; }

	public float m_fRS { get; set; }

	public float m_fLQ { get; set; }

	public float m_fKe { get; set; }

	public float m_fHwRSHunt { get; set; }

	public byte m_byHwAmpGain { get; set; }

	public byte m_byVdcAdj { get; set; }

	public byte m_bySMPMode { get; set; }

	public byte m_byStaTsam0En { get; set; }

	public byte m_byPllSmoEn { get; set; }

	public byte m_byStaPllEn { get; set; }

	public byte m_bySeg7En { get; set; }

	public byte m_bySta7SegEn { get; set; }

	public byte m_byAtanEn { get; set; }

	public byte m_byStaSin { get; set; }

	public ushort m_wFreqBase { get; set; }

	public byte m_bySampleTime { get; set; }

	public byte m_bySpdBw { get; set; }

	public byte m_byEbwMode { get; set; }

	public float m_fLeadAngleSet { get; set; }

	public byte m_byEkNum { get; set; }

	public ushort m_wEbw { get; set; }

	public float m_fAtoBw { get; set; }

	public byte m_byTSMIN { get; set; }

	public float m_fDTSEL { get; set; }

	public byte m_byKSILDE { get; set; }

	public byte m_byRpdAlignEn { get; set; }

	public byte m_byAlignTime { get; set; }

	public byte m_byAlignDuty { get; set; }

	public byte m_byRPD10 { get; set; }

	public byte m_byRPD11 { get; set; }

	public byte m_byRPD12 { get; set; }

	public byte m_byRPD20 { get; set; }

	public byte m_byRPDRvsAlignEn { get; set; }

	public byte m_byRPD21 { get; set; }

	public byte m_byControlMode { get; set; }

	public float m_fStartCurrent { get; set; }

	public byte m_byFocEfreqAcc { get; set; }

	public float m_fFocEfreqMin { get; set; }

	public byte m_byFocEfreqHold { get; set; }

	public byte m_byCloSpeed { get; set; }

	public byte m_byStartDuty30 { get; set; }

	public byte m_byStaIncTime { get; set; }

	public byte m_byStbInc { get; set; }

	public byte m_byStaInc { get; set; }

	public float m_fDqKp { get; set; }

	public float m_fDqKi { get; set; }

	public float m_fWKp { get; set; }

	public float m_fWKi { get; set; }

	public byte m_byWoutMax { get; set; }

	public byte m_byPwmOnoffhys { get; set; }

	public byte m_byPwmOnoffhysClk { get; set; }

	public byte m_byRsdEn { get; set; }

	public byte m_byRsdMode { get; set; }

	public double m_dStaticSpeed { get; set; }

	public byte m_byRevBrakeSpeed { get; set; }

	public byte m_byForwardStaSpeed { get; set; }

	public byte m_byRSD230 { get; set; }

	public byte m_byRSD231 { get; set; }

	public byte m_byRsdHallFilter { get; set; }

	public byte m_bySpdCtrlMd { get; set; }

	public byte m_byPwmOffDis { get; set; }

	public byte m_byPwmRevEn { get; set; }

	public byte m_byQuickSta { get; set; }

	public byte m_byCurveEnSpd { get; set; }

	public byte m_byFgMul { get; set; }

	public byte m_byFgRdMd { get; set; }

	public byte m_byFgDiv { get; set; }

	public byte m_byFgIdleSet { get; set; }

	public byte m_byFgSet { get; set; }

	public byte m_byFgRd { get; set; }

	public byte m_byStartClk { get; set; }

	public byte m_byFaultRecovTimes { get; set; }

	public byte m_byLock0 { get; set; }

	public byte m_byLock1 { get; set; }

	public byte m_byLock2 { get; set; }

	public byte m_byLossPhaseEn { get; set; }

	public uint m_wLockMinSpeed { get; set; }

	public uint m_wLocMaxSpeed { get; set; }

	public byte m_byLockEmfThd { get; set; }

	public float m_fLockEmf { get; set; }

	public float m_fLpNorhCurrent { get; set; }

	public byte m_bySpeedVSet { get; set; }

	public byte m_byVRefSel { get; set; }

	public byte m_byFgTimeSel { get; set; }

	public uint m_wLimitSpeed { get; set; }

	public byte m_byXON0 { get; set; }

	public byte m_byX_ON { get; set; }

	public byte m_byPreCtrlMode { get; set; }

	public uint m_dwMinSpeed { get; set; }

	public uint m_dwMaxSpeed { get; set; }

	public float m_fMinCurrent { get; set; }

	public float m_fMaxCurrent { get; set; }

	public float m_fMinPower { get; set; }

	public float m_fMaxPower { get; set; }

	public byte m_byYMaxSel { get; set; }

	public byte m_byPwmX98En { get; set; }

	public byte m_byY_ON { get; set; }

	public byte m_byY_25 { get; set; }

	public byte m_byY_50 { get; set; }

	public byte m_byY_75 { get; set; }

	public string m_strSlaveId { get; set; }

	public string m_strUID1 { get; set; }

	public byte m_bySpdCtrl { get; set; }

	public byte m_bySpiDebug { get; set; }

	public byte m_byDbgMd { get; set; }

	public byte m_byMotorDir { get; set; }

	public byte m_byPwmEn { get; set; }

	public byte m_byFDS0 { get; set; }

	public byte m_byFDS1 { get; set; }

	public byte m_byOcpSel { get; set; }

	public byte m_bySlpDis { get; set; }

	public byte m_byIsdDirRev { get; set; }

	public byte m_byOvpSel { get; set; }

	public byte m_byDzSet { get; set; }

	public byte m_byDpmDis { get; set; }

	public byte m_byOvpDis { get; set; }

	public byte m_byOcpDis { get; set; }

	public byte m_byTsdDis { get; set; }

	public byte m_byFocDis { get; set; }

	public byte m_byBootMod { get; set; }

	public byte m_byUvloDis { get; set; }

	public byte m_byIsdBrakeEn { get; set; }

	public byte m_byIdleBrakeEn { get; set; }

	public byte m_byTSMin { get; set; }

	public byte m_byRvsAlignEn { get; set; }

	public byte m_byRvsCutSel { get; set; }

	public byte m_byNewSingle { get; set; }

	public byte m_byDigTst { get; set; }

	public byte m_byDigMoniCt { get; set; }

	public float m_fRV { get; set; }

	public byte m_byTestMode { get; set; }

	public byte m_byIrefDis { get; set; }

	public byte m_byRestaEn { get; set; }

	public byte m_byIsdFwMd { get; set; }

	public byte m_byLaMd { get; set; }

	public byte m_byNewFunc { get; set; }

	public byte m_byOnoffTestEn { get; set; }

	public byte m_byStartOnOffDuty { get; set; }

	public byte m_byStartDurationTime { get; set; }

	public byte m_byStopDuty { get; set; }

	public byte m_byStopDurationTime { get; set; }

	public ushort m_wTotalCnt { get; set; }

	public ushort m_wExecutedCnt { get; set; }

	public ushort m_wFailCnt { get; set; }

	public byte m_byDirSwitch { get; set; }

	public byte m_byLogEn { get; set; }

	public string m_strHW_Board_Curr_Base { get; set; }

	public string m_strHW_Board_Vol_Base2 { get; set; }

	public string m_strMotor_Power_Base { get; set; }

	public byte m_byHallMd { get; set; }

	public byte m_byHallComMd { get; set; }

	public byte m_byHall0RevEn { get; set; }

	public byte m_byHall1RevEn { get; set; }

	public byte m_byHall2RevEn { get; set; }

	public byte m_byHallFgMd { get; set; }

	public byte m_byHallForMd { get; set; }

	public byte m_byHallStaMd { get; set; }

	public byte m_byHallCorMd { get; set; }

	public float m_fHallLockSel { get; set; }

	public byte m_byHallPllEn { get; set; }

	public byte m_byHallLockSwi { get; set; }

	public byte m_byHallStaOp { get; set; }

	public byte m_byHallSwiMd { get; set; }

	public byte m_byHallCalMd { get; set; }

	public byte m_byHallSvMd { get; set; }

	public byte m_byHallLockExt { get; set; }

	public byte m_byHallLimSel { get; set; }

	public byte m_byHallRsv1 { get; set; }

	public byte m_byHaDu { get; set; }

	public byte m_byHaTi { get; set; }

	public byte m_byHallAlignSpeed { get; set; }

	public byte m_byHallModsel { get; set; }

	public byte m_byHallLA0 { get; set; }

	public byte m_byHallLA125 { get; set; }

	public byte m_byHallLA250 { get; set; }

	public byte m_byHallLA375 { get; set; }

	public byte m_byHallLA500 { get; set; }

	public byte m_byHallLA625 { get; set; }

	public byte m_byHallLA750 { get; set; }

	public byte m_byHallLA875 { get; set; }

	public byte m_byHallLA999 { get; set; }

	public byte m_byMfpEn { get; set; }

	public float m_fMfpEk3 { get; set; }

	public byte m_byCarrierSel { get; set; }

	public byte m_byRdsPu { get; set; }

	public byte m_byHallOvMd { get; set; }

	public byte m_byHallPu { get; set; }

	public byte m_byOuvpThr { get; set; }

	public byte m_byHallLttEn { get; set; }

	public byte m_byMcOffSel { get; set; }

	public byte m_byIdleTime { get; set; }

	public byte m_byS5TsdSel { get; set; }

	public byte m_byOvpThr { get; set; }

	public byte m_byPwrFltSel { get; set; }

	public byte m_byEtcEn { get; set; }

	public byte m_byPwrAmpSel { get; set; }

	public byte m_byLimitSel { get; set; }

	public byte m_byITripEn { get; set; }

	public byte m_byLimitITrip { get; set; }

	public byte m_byIsdMd2Time { get; set; }

	public byte m_byIsdFltSel { get; set; }

	public byte m_byIsdThr { get; set; }

	public byte m_byITripRe { get; set; }

	public byte m_byUvpThr { get; set; }

	public byte m_byFlocLdVoltKp { get; set; }

	public byte m_byFlocLqVoltKp { get; set; }

	public byte m_byFocFluxVoltKp { get; set; }

	public byte m_byDrvDisMod { get; set; }

	public byte m_byCmp0CkSel { get; set; }

	public ushort m_wSpdBlk0 { get; set; }

	public ushort m_wSpdBlk1 { get; set; }

	public ushort m_wSpdBlk2 { get; set; }

	public ushort m_wSpdBlkWid { get; set; }

	public byte m_byTheCompSel { get; set; }

	public byte m_byOcpDa { get; set; }

	public byte m_byCmp0ModSel { get; set; }

	public byte m_byCmp0Hys { get; set; }

	public byte m_byAmpSingle { get; set; }

	public byte m_byAmpFs { get; set; }

	public byte m_byOvpDisBk { get; set; }

	public byte m_byOcpDisBk { get; set; }

	public byte m_byOcpSelBk { get; set; }

	public byte m_byOcpMod { get; set; }

	public byte m_byStallTstQMax { get; set; }

	public byte m_byStallTstInc { get; set; }

	public byte m_byStallTstEn { get; set; }

	public byte m_byDirInv { get; set; }

	public byte m_byDirEn0 { get; set; }

	public byte m_byDirEn1 { get; set; }

	public string m_ConfigFile { get; set; }

	public int m_MapMode { get; set; }

	public uint m_CPIMode { get; set; }

	public uint m_MouseID { get; set; }

	public MainWindow mainWindow { get; set; }

	public FileScriptParser()
	{
		mainWindow = (MainWindow)Application.Current.MainWindow;
		m_MapMode = 1;
		m_CPIMode = 1u;
		m_MouseID = 0u;
		m_byLogEn = 0;
	}

	public double ReadXMLDoubleValue(XmlElement root)
	{
		double result = 0.0;
		if (root != null)
		{
			result = Convert.ToDouble(root.InnerText);
		}
		return result;
	}

	public float ReadXMLFloatValue(XmlElement root)
	{
		float result = 0f;
		if (root != null)
		{
			result = (float)Convert.ToDouble(root.InnerText);
		}
		return result;
	}

	public float ReadXMLFloatValue(XmlElement root, float defaultValue)
	{
		float num = 0f;
		if (root != null)
		{
			return (float)Convert.ToDouble(root.InnerText);
		}
		return defaultValue;
	}

	public int ReadXMLIntValue(XmlElement root)
	{
		int result = 0;
		if (root != null)
		{
			result = strToInt(root.InnerText);
		}
		return result;
	}

	public byte ReadXMLByteValue(XmlElement root)
	{
		byte b = 0;
		int num = 0;
		if (root != null)
		{
			num = strToInt(root.InnerText);
		}
		return (byte)num;
	}

	public byte ReadXMLByteValue(XmlElement root, int defaultValue)
	{
		byte b = 0;
		int num = 0;
		num = ((root == null) ? defaultValue : strToInt(root.InnerText));
		return (byte)num;
	}

	public ushort ReadXMLWordValue(XmlElement root)
	{
		ushort num = 0;
		int num2 = 0;
		if (root != null)
		{
			num2 = strToInt(root.InnerText);
		}
		return (ushort)num2;
	}

	public uint ReadXMLDWordValue(XmlElement root)
	{
		uint num = 0u;
		int result = 0;
		if (root != null)
		{
			result = strToInt(root.InnerText);
		}
		return (uint)result;
	}

	public string ReadXMLStringValue(XmlElement root)
	{
		string result = "";
		if (root != null)
		{
			result = root.InnerText;
		}
		return result;
	}

	public int LoadConfig(string strConfigFile)
	{
		int result = 0;
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(strConfigFile);
			m_ConfigFile = strConfigFile;
			m_strSwName = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SoftwareName"));
			m_sAppVersion = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/Version"));
			m_i16SensorMode = (short)ReadXMLIntValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SensorMode"));
			m_byCircuitType = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CircuitType"));
			m_byPolePairs = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamPolePairs"));
			m_fRS = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamRS"));
			m_fLQ = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamLQ"));
			m_fKe = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamKe"));
			m_fHwRSHunt = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamHwRSHunt"));
			m_byHwAmpGain = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamHwAmpGain"));
			m_fRV = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamHwRV"));
			m_byVdcAdj = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/MotorParamHwVdcAdj"));
			m_bySMPMode = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgSmpMode"));
			m_byStaTsam0En = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgStaTsam0En"));
			m_byPllSmoEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgPllSmoEn"));
			m_byStaPllEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgStaPllEn"));
			m_bySeg7En = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgSeg7En"));
			m_bySta7SegEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgSta7SegEn"));
			m_byAtanEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgAtan"));
			m_byStaSin = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgStaSin"));
			m_wFreqBase = ReadXMLWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgFreqBase"));
			m_bySampleTime = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgSampleTime"));
			m_bySpdBw = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgSpdBw"));
			m_byEbwMode = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgEbwMode"));
			m_fLeadAngleSet = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgLeadAngleSet"));
			m_byEkNum = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgEkNum"));
			m_wEbw = ReadXMLWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgFocEbw"));
			m_fAtoBw = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgFocAtoBw"));
			m_byTSMIN = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgFocTSMIN"));
			m_fDTSEL = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgFocDTSEL"));
			m_byKSILDE = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/FocCfgFocKSILDE"));
			m_byRpdAlignEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRpdAlignEn"));
			m_byAlignTime = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgAlignTime"));
			m_byAlignDuty = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgAlignDuty"));
			m_byRPD10 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRpd10"));
			m_byRPD11 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRpd11"));
			m_byRPD12 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRpd12"));
			m_byRPD20 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRpd20"));
			m_byRPDRvsAlignEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRvsAlignEn"));
			m_byRPD21 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/AlignCfgRpd21"));
			m_byControlMode = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgControlMode"));
			m_fStartCurrent = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgStartCurrent"));
			m_byFocEfreqAcc = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgFocDfreqAcc"));
			m_fFocEfreqMin = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgFocEfreqMin"));
			m_byFocEfreqHold = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgFocEFreqHold"));
			m_byCloSpeed = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgCloSpeed"));
			m_byStartDuty30 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartCfgStartDuty30"));
			m_byStaIncTime = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgStaIncTime"));
			m_byStbInc = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgStbInc"));
			m_byStaInc = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgStaInc"));
			m_fDqKp = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgDqKp"));
			m_fDqKi = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgDqKi"));
			m_fWKp = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgWKp"));
			m_fWKi = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgWKi"));
			m_byWoutMax = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgWoutMax"));
			m_byPwmOnoffhys = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgPwmOnoffhys"));
			m_byPwmOnoffhysClk = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RunCfgPwmOnoffhysClk"));
			m_byRsdEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/WindCfgRsdEn"));
			m_byRsdMode = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/WindCfgRsdMode"));
			m_dStaticSpeed = ReadXMLDoubleValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/WindCfgStaticSpeed"));
			m_byRevBrakeSpeed = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/WindCfgRevBrakeSpeed"));
			m_byForwardStaSpeed = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/WindCfgForwardStaSpeed"));
			m_byRSD230 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RSD230"));
			m_byRSD231 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RSD231"));
			m_byRsdHallFilter = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/RsdHallFilter"));
			m_byFaultRecovTimes = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgFaultRevTimes"));
			m_byLock0 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLock0"));
			m_byLock1 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLock1"));
			m_byLock2 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLock2"));
			m_byLossPhaseEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLossPhaseEn"));
			m_wLockMinSpeed = ReadXMLDWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLockMinSpeed"));
			m_wLocMaxSpeed = ReadXMLDWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLockMaxSpeed"));
			m_byLockEmfThd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLockEmfThd"));
			m_fLockEmf = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLockEmf"));
			m_fLpNorhCurrent = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/PtCfgLPNorhCur"));
			m_bySpdCtrlMd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgSpdCtrlMd"));
			m_byPwmOffDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgPwmOffDis"));
			m_byPwmRevEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgPwmReEn"));
			m_byQuickSta = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgQuickSta"));
			m_byCurveEnSpd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgCurveEnSpd"));
			m_byFgMul = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgFgMul"));
			m_byFgRdMd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgFgRdMd"));
			m_byFgDiv = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgFgDiv"));
			m_byFgIdleSet = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgFgIdleSet"));
			m_byFgSet = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgFgSet"));
			m_byFgRd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgFgRd"));
			m_byStartClk = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/OtherCfgStartClk"));
			m_bySpeedVSet = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/ExtendCfgSpeedVSet"));
			m_byVRefSel = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/ExtendCfgVRefSel"), 1);
			m_byFgTimeSel = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/ExtendCfgFgTimeSel"));
			m_wLimitSpeed = ReadXMLDWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/ExtendCfgLimitSpeed"));
			m_byXON0 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgXON0"));
			m_byX_ON = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgXON"));
			m_byPreCtrlMode = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgPreCtrlMode"));
			m_dwMinSpeed = ReadXMLDWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgMinSpeed"));
			m_dwMaxSpeed = ReadXMLDWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgMaxSpeed"));
			m_fMinCurrent = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgMinCurrent"));
			m_fMaxCurrent = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgMaxCurrent"));
			m_fMinPower = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgMinPower"));
			m_fMaxPower = ReadXMLFloatValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgMaxPower"));
			m_byYMaxSel = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgYMaxSel"));
			m_byPwmX98En = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgPwmX98"));
			m_byY_ON = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgYON"));
			m_byY_25 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgY25"));
			m_byY_50 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgY50"));
			m_byY_75 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/CurveCfgY75"));
			m_strSlaveId = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoCfgSlvID"));
			m_strUID1 = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoCfgUID1"));
			m_bySpdCtrl = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoCfgSpdCtrl"));
			m_bySpiDebug = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoSpiDebug"));
			m_byDbgMd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/DbgMd"));
			m_byMotorDir = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoMotorDir"));
			m_byPwmEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoPwmEn"));
			m_byFDS0 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoCfgFDS0"));
			m_byFDS1 = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/UserInfoCfgFDS1"));
			m_byOcpSel = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcOcpSel"));
			m_bySlpDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcSlpDis"));
			m_byIsdDirRev = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcIsdDirRev"));
			m_byOvpSel = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcOvpSel"));
			m_byDzSet = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcDzSet"));
			m_byDpmDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcDpmDis"));
			m_byOvpDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcOvpDis"));
			m_byOcpDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcOcpDis"));
			m_byTsdDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcTsdDis"));
			m_byFocDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcFocDis"));
			m_byBootMod = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcBootMod"));
			m_byUvloDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcUvloDis"));
			m_byIsdBrakeEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcIsdBrakeEn"));
			m_byIdleBrakeEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcIdleBrakeEn"));
			m_byTSMin = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcTSMin"));
			m_byRvsAlignEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcRvsAlignEn"));
			m_byRvsCutSel = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcRvsCutSel"));
			m_byNewSingle = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcNewSingle"));
			m_byDigTst = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcDigTst"));
			m_byDigMoniCt = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcDigMoniCt"));
			m_byTestMode = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcTestMode"));
			m_byIrefDis = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcIrefDis"));
			m_byRestaEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcRestaEn"));
			m_byIsdFwMd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcIsdFwMd"));
			m_byLaMd = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcLaMd"));
			m_byNewFunc = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/SpcNewFunc"));
			m_byStartOnOffDuty = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartOnoffDuty"));
			m_byStartDurationTime = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StartDurationTime"));
			m_byStopDuty = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StopDuty"));
			m_byStopDurationTime = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/StopDurationTime"));
			m_wTotalCnt = ReadXMLWordValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/TotalSetCnt"));
			ReadSettingPage(5, xmlDocument, null);
			m_strHW_Board_Curr_Base = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/HW_Board_Curr_Base"));
			m_strHW_Board_Vol_Base2 = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/HW_Board_Vol_Base2"));
			m_strMotor_Power_Base = ReadXMLStringValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/Motor_Power_Base"));
			m_byLogEn = ReadXMLByteValue((XmlElement)xmlDocument.SelectSingleNode("/UserConfig/LogEn"));
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message + "App will load default config.", "Exception Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			result = 1;
		}
		return result;
	}

	public void WriteXMLIntValue(XmlDocument doc, XmlElement root, string strKeyName, int iValue)
	{
		XmlElement xmlElement = doc.CreateElement(strKeyName);
		xmlElement.InnerText = iValue.ToString();
		root.AppendChild(xmlElement);
	}

	public void WriteXMLIntValue(XmlDocument doc, XmlElement root, string strKeyName, float fValue)
	{
		XmlElement xmlElement = doc.CreateElement(strKeyName);
		xmlElement.InnerText = fValue.ToString();
		root.AppendChild(xmlElement);
	}

	public void WriteXMLStringValue(XmlDocument doc, XmlElement root, string strKeyName, string strValue)
	{
		XmlElement xmlElement = doc.CreateElement(strKeyName);
		if (strValue != null)
		{
			xmlElement.InnerText = strValue;
		}
		root.AppendChild(xmlElement);
	}

	private void SetAttribute2Element(XmlDocument doc, XmlElement element, string attrName, string attrValue)
	{
		XmlAttribute xmlAttribute = doc.CreateAttribute(attrName);
		xmlAttribute.Value = attrValue;
		element.Attributes.Append(xmlAttribute);
	}

	public int SaveConfig(string strCfgFileName)
	{
		int result = 0;
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			XmlElement xmlElement = null;
			m_ConfigFile = strCfgFileName;
			xmlDocument.RemoveAll();
			xmlElement = xmlDocument.CreateElement("UserConfig");
			WriteXMLStringValue(xmlDocument, xmlElement, "SoftwareName", m_strSwName);
			WriteXMLStringValue(xmlDocument, xmlElement, "Version", m_sAppVersion);
			WriteXMLIntValue(xmlDocument, xmlElement, "SensorMode", m_i16SensorMode);
			WriteXMLIntValue(xmlDocument, xmlElement, "CircuitType", m_byCircuitType);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamPolePairs", m_byPolePairs);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamRS", m_fRS);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamLQ", m_fLQ);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamKe", m_fKe);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamHwRSHunt", m_fHwRSHunt);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamHwAmpGain", m_byHwAmpGain);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamHwRV", m_fRV);
			WriteXMLIntValue(xmlDocument, xmlElement, "MotorParamHwVdcAdj", m_byVdcAdj);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgSmpMode", m_bySMPMode);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgStaTsam0En", m_byStaTsam0En);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgPllSmoEn", m_byPllSmoEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgStaPllEn", m_byStaPllEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgSeg7En", m_bySeg7En);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgSta7SegEn", m_bySta7SegEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgAtan", m_byAtanEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgStaSin", m_byStaSin);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgFreqBase", m_wFreqBase);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgSampleTime", m_bySampleTime);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgSpdBw", m_bySpdBw);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgEbwMode", m_byEbwMode);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgLeadAngleSet", m_fLeadAngleSet);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgEkNum", m_byEkNum);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgFocEbw", m_wEbw);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgFocAtoBw", m_fAtoBw);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgFocTSMIN", m_byTSMIN);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgFocDTSEL", m_fDTSEL);
			WriteXMLIntValue(xmlDocument, xmlElement, "FocCfgFocKSILDE", m_byKSILDE);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRpdAlignEn", m_byRpdAlignEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgAlignTime", m_byAlignTime);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgAlignDuty", m_byAlignDuty);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRpd10", m_byRPD10);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRpd11", m_byRPD11);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRpd12", m_byRPD12);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRpd20", m_byRPD20);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRvsAlignEn", m_byRPDRvsAlignEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "AlignCfgRpd21", m_byRPD21);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgControlMode", m_byControlMode);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgStartCurrent", m_fStartCurrent);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgFocDfreqAcc", m_byFocEfreqAcc);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgFocEfreqMin", m_fFocEfreqMin);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgFocEFreqHold", m_byFocEfreqHold);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgCloSpeed", m_byCloSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartCfgStartDuty30", m_byStartDuty30);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgStaIncTime", m_byStaIncTime);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgStbInc", m_byStbInc);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgStaInc", m_byStaInc);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgDqKp", m_fDqKp);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgDqKi", m_fDqKi);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgWKp", m_fWKp);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgWKi", m_fWKi);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgWoutMax", m_byWoutMax);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgPwmOnoffhys", m_byPwmOnoffhys);
			WriteXMLIntValue(xmlDocument, xmlElement, "RunCfgPwmOnoffhysClk", m_byPwmOnoffhysClk);
			WriteXMLIntValue(xmlDocument, xmlElement, "WindCfgRsdEn", m_byRsdEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "WindCfgRsdMode", m_byRsdMode);
			WriteXMLStringValue(xmlDocument, xmlElement, "WindCfgStaticSpeed", m_dStaticSpeed.ToString());
			WriteXMLIntValue(xmlDocument, xmlElement, "WindCfgRevBrakeSpeed", m_byRevBrakeSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "WindCfgForwardStaSpeed", m_byForwardStaSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "RSD230", m_byRSD230);
			WriteXMLIntValue(xmlDocument, xmlElement, "RSD231", m_byRSD231);
			WriteXMLIntValue(xmlDocument, xmlElement, "RsdHallFilter", m_byRsdHallFilter);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgFaultRevTimes", m_byFaultRecovTimes);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLock0", m_byLock0);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLock1", m_byLock1);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLock2", m_byLock2);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLossPhaseEn", m_byLossPhaseEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLockMinSpeed", m_wLockMinSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLockMaxSpeed", m_wLocMaxSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLockEmfThd", m_byLockEmfThd);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLockEmf", m_fLockEmf);
			WriteXMLIntValue(xmlDocument, xmlElement, "PtCfgLPNorhCur", m_fLpNorhCurrent);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgSpdCtrlMd", m_bySpdCtrlMd);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgPwmOffDis", m_byPwmOffDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgPwmReEn", m_byPwmRevEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgQuickSta", m_byQuickSta);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgCurveEnSpd", m_byCurveEnSpd);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgFgMul", m_byFgMul);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgFgRdMd", m_byFgRdMd);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgFgDiv", m_byFgDiv);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgFgIdleSet", m_byFgIdleSet);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgFgSet", m_byFgSet);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgFgRd", m_byFgRd);
			WriteXMLIntValue(xmlDocument, xmlElement, "OtherCfgStartClk", m_byStartClk);
			WriteXMLIntValue(xmlDocument, xmlElement, "ExtendCfgSpeedVSet", m_bySpeedVSet);
			WriteXMLIntValue(xmlDocument, xmlElement, "ExtendCfgVRefSel", m_byVRefSel);
			WriteXMLIntValue(xmlDocument, xmlElement, "ExtendCfgFgTimeSel", m_byFgTimeSel);
			WriteXMLIntValue(xmlDocument, xmlElement, "ExtendCfgLimitSpeed", m_wLimitSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgXON0", m_byXON0);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgXON", m_byX_ON);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgPreCtrlMode", m_byPreCtrlMode);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgMinSpeed", m_dwMinSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgMaxSpeed", m_dwMaxSpeed);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgMinCurrent", m_fMinCurrent);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgMaxCurrent", m_fMaxCurrent);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgMinPower", m_fMinPower);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgMaxPower", m_fMaxPower);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgYMaxSel", m_byYMaxSel);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgPwmX98", m_byPwmX98En);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgYON", m_byY_ON);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgY25", m_byY_25);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgY50", m_byY_50);
			WriteXMLIntValue(xmlDocument, xmlElement, "CurveCfgY75", m_byY_75);
			WriteXMLStringValue(xmlDocument, xmlElement, "UserInfoCfgSlvID", m_strSlaveId);
			WriteXMLStringValue(xmlDocument, xmlElement, "UserInfoCfgUID1", m_strUID1);
			WriteXMLIntValue(xmlDocument, xmlElement, "UserInfoCfgSpdCtrl", m_bySpdCtrl);
			WriteXMLIntValue(xmlDocument, xmlElement, "UserInfoSpiDebug", m_bySpiDebug);
			WriteXMLIntValue(xmlDocument, xmlElement, "DbgMd", m_byDbgMd);
			WriteXMLIntValue(xmlDocument, xmlElement, "UserInfoMotorDir", m_byMotorDir);
			WriteXMLIntValue(xmlDocument, xmlElement, "UserInfoPwmEn", m_byPwmEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "UserInfoCfgFDS0", m_byFDS0);
			WriteXMLIntValue(xmlDocument, xmlElement, "UserInfoCfgFDS1", m_byFDS1);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcOcpSel", m_byOcpSel);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcSlpDis", m_bySlpDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcIsdDirRev", m_byIsdDirRev);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcOvpSel", m_byOvpSel);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcDzSet", m_byDzSet);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcDpmDis", m_byDpmDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcOvpDis", m_byOvpDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcOcpDis", m_byOcpDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcTsdDis", m_byTsdDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcFocDis", m_byFocDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcBootMod", m_byBootMod);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcUvloDis", m_byUvloDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcIsdBrakeEn", m_byIsdBrakeEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcIdleBrakeEn", m_byIdleBrakeEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcTSMin", m_byTSMin);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcRvsAlignEn", m_byRvsAlignEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcRvsCutSel", m_byRvsCutSel);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcNewSingle", m_byNewSingle);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcDigTst", m_byDigTst);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcDigMoniCt", m_byDigMoniCt);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcTestMode", m_byTestMode);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcIrefDis", m_byIrefDis);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcRestaEn", m_byRestaEn);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcIsdFwMd", m_byIsdFwMd);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcLaMd", m_byLaMd);
			WriteXMLIntValue(xmlDocument, xmlElement, "SpcNewFunc", m_byNewFunc);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartOnoffDuty", m_byStartOnOffDuty);
			WriteXMLIntValue(xmlDocument, xmlElement, "StartDurationTime", m_byStartDurationTime);
			WriteXMLIntValue(xmlDocument, xmlElement, "StopDuty", m_byStopDuty);
			WriteXMLIntValue(xmlDocument, xmlElement, "StopDurationTime", m_byStopDurationTime);
			WriteXMLIntValue(xmlDocument, xmlElement, "TotalSetCnt", m_wTotalCnt);
			WriteSettingPage(5, xmlDocument, xmlElement);
			WriteXMLStringValue(xmlDocument, xmlElement, "HW_Board_Curr_Base", m_strHW_Board_Curr_Base);
			WriteXMLStringValue(xmlDocument, xmlElement, "HW_Board_Vol_Base2", m_strHW_Board_Vol_Base2);
			WriteXMLStringValue(xmlDocument, xmlElement, "Motor_Power_Base", m_strMotor_Power_Base);
			WriteXMLIntValue(xmlDocument, xmlElement, "LogEn", m_byLogEn);
			xmlDocument.AppendChild(xmlElement);
			xmlDocument.Save(m_ConfigFile);
		}
		catch
		{
			result = 1;
		}
		return result;
	}

	public int strToInt(string strVal)
	{
		if (strVal.Length <= 0)
		{
			return 0;
		}
		return int.Parse(strVal);
	}

	public bool ReadSettingPage(byte byPgeNum, XmlDocument document, XmlElement root)
	{
		m_byHallMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallMd"));
		m_byHallComMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallComMd"));
		m_byHall0RevEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHall0RevEn"));
		m_byHall1RevEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHall1RevEn"));
		m_byHall2RevEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHall2RevEn"));
		m_byHallFgMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallFgMd"));
		m_byHallForMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallForMd"));
		m_byHallStaMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgStaMd"));
		m_byHallCorMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallCorMd"));
		m_fHallLockSel = ReadXMLFloatValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallLockSel"));
		m_byHallPllEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallPllEn"));
		m_byHallLockSwi = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallLockSwi"));
		m_byHallStaOp = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallStaOp"));
		m_byHallSwiMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallSwiMd"));
		m_byHallCalMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallCalMd"));
		m_byHallSvMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallSvMd"));
		m_byHallLockExt = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallLockExt"));
		m_byHallLimSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallLimSel"));
		m_byHallRsv1 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallRsv1"));
		m_byHaDu = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHaDu"));
		m_byHaTi = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHaTi"));
		m_byHallAlignSpeed = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallAlignSpeed"));
		m_byHallModsel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallParamCfgHallModsel"));
		m_byHallLA0 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA0"));
		m_byHallLA125 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA125"));
		m_byHallLA250 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA250"));
		m_byHallLA375 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA375"));
		m_byHallLA500 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA500"));
		m_byHallLA625 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA625"));
		m_byHallLA750 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA750"));
		m_byHallLA875 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA875"));
		m_byHallLA999 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgLA999"));
		m_byMfpEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgMfpEn"));
		m_fMfpEk3 = ReadXMLFloatValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgMfpEk3"));
		m_byCarrierSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgCarrierSel"));
		m_byRdsPu = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgRdsPu"));
		m_byHallOvMd = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/HallLaCfgHallOvMd"));
		m_byHallPu = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgHallPu"));
		m_byOuvpThr = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgOuvpThr"));
		m_byHallLttEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/MfpCfgHallLttEn"));
		m_byMcOffSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgMcOffSel"));
		m_byIdleTime = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgIdleTime"));
		m_byS5TsdSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgTsdSel"));
		m_byOvpThr = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgOvpThr"));
		m_byPwrFltSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgPwrFltSel"));
		m_byEtcEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgEtcEn"));
		m_byPwrAmpSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgPwrAmpSel"));
		m_byLimitSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgLimitSel"));
		m_byITripEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgITripEn"));
		m_byLimitITrip = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgLimitITrip"));
		m_byIsdMd2Time = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgIsdMd2Time"));
		m_byIsdFltSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgIsdFltSel"));
		m_byIsdThr = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgIsdThr"));
		m_byITripRe = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgItripRe"));
		m_byUvpThr = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgUvpThr"));
		m_byFlocLdVoltKp = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgFlocLdqVoltKp"));
		m_byFlocLqVoltKp = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgFlocLqVoltKp"));
		m_byFocFluxVoltKp = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgFocFluxVoltKp"));
		m_byDrvDisMod = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgDrvDisMod"));
		m_wSpdBlk0 = ReadXMLWordValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgSpdBlk0"));
		m_wSpdBlk1 = ReadXMLWordValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgSpdBlk1"));
		m_wSpdBlk2 = ReadXMLWordValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgSpdBlk2"));
		m_wSpdBlkWid = ReadXMLWordValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgSpdBlkWid"));
		m_byTheCompSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgTheCompSel"));
		m_byOcpDa = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgOcpDa"));
		m_byCmp0ModSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgCmp0ModSel"));
		m_byCmp0Hys = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgCmp0Hys"));
		m_byCmp0CkSel = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgCmp0CkSel"));
		m_byAmpSingle = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgAmpSingle"));
		m_byAmpFs = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgAmpFs"));
		m_byOvpDisBk = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgOvpDisBk"));
		m_byOcpDisBk = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgOcpDisBk"));
		m_byOcpSelBk = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgOcpSelBk"));
		m_byOcpMod = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgOcpMod"));
		m_byStallTstQMax = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgStallTstQMax"));
		m_byStallTstInc = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgStallTstInc"));
		m_byStallTstEn = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgStallTstEn"));
		m_byDirInv = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgDirInv"));
		m_byDirEn0 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgDirEn0"));
		m_byDirEn1 = ReadXMLByteValue((XmlElement)document.SelectSingleNode("/UserConfig/St5OthCfgDirEn1"));
		return true;
	}

	public bool WriteSettingPage(byte byPgeNum, XmlDocument doc, XmlElement root)
	{
		WriteXMLIntValue(doc, root, "HallParamCfgHallMd", m_byHallMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallComMd", m_byHallComMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHall0RevEn", m_byHall0RevEn);
		WriteXMLIntValue(doc, root, "HallParamCfgHall1RevEn", m_byHall1RevEn);
		WriteXMLIntValue(doc, root, "HallParamCfgHall2RevEn", m_byHall2RevEn);
		WriteXMLIntValue(doc, root, "HallParamCfgHallFgMd", m_byHallFgMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallForMd", m_byHallForMd);
		WriteXMLIntValue(doc, root, "HallParamCfgStaMd", m_byHallStaMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallCorMd", m_byHallCorMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallLockSel", m_fHallLockSel);
		WriteXMLIntValue(doc, root, "HallParamCfgHallPllEn", m_byHallPllEn);
		WriteXMLIntValue(doc, root, "HallParamCfgHallLockSwi", m_byHallLockSwi);
		WriteXMLIntValue(doc, root, "HallParamCfgHallStaOp", m_byHallStaOp);
		WriteXMLIntValue(doc, root, "HallParamCfgHallSwiMd", m_byHallSwiMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallCalMd", m_byHallCalMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallSvMd", m_byHallSvMd);
		WriteXMLIntValue(doc, root, "HallParamCfgHallLockExt", m_byHallLockExt);
		WriteXMLIntValue(doc, root, "HallParamCfgHallLimSel", m_byHallLimSel);
		WriteXMLIntValue(doc, root, "HallParamCfgHallRsv1", m_byHallRsv1);
		WriteXMLIntValue(doc, root, "HallParamCfgHaDu", m_byHaDu);
		WriteXMLIntValue(doc, root, "HallParamCfgHaTi", m_byHaTi);
		WriteXMLIntValue(doc, root, "HallParamCfgHallAlignSpeed", m_byHallAlignSpeed);
		WriteXMLIntValue(doc, root, "HallParamCfgHallModsel", m_byHallModsel);
		WriteXMLIntValue(doc, root, "HallLaCfgLA0", m_byHallLA0);
		WriteXMLIntValue(doc, root, "HallLaCfgLA125", m_byHallLA125);
		WriteXMLIntValue(doc, root, "HallLaCfgLA250", m_byHallLA250);
		WriteXMLIntValue(doc, root, "HallLaCfgLA375", m_byHallLA375);
		WriteXMLIntValue(doc, root, "HallLaCfgLA500", m_byHallLA500);
		WriteXMLIntValue(doc, root, "HallLaCfgLA625", m_byHallLA625);
		WriteXMLIntValue(doc, root, "HallLaCfgLA750", m_byHallLA750);
		WriteXMLIntValue(doc, root, "HallLaCfgLA875", m_byHallLA875);
		WriteXMLIntValue(doc, root, "HallLaCfgLA999", m_byHallLA999);
		WriteXMLIntValue(doc, root, "MfpCfgMfpEn", m_byMfpEn);
		WriteXMLIntValue(doc, root, "MfpCfgMfpEk3", m_fMfpEk3);
		WriteXMLIntValue(doc, root, "MfpCfgCarrierSel", m_byCarrierSel);
		WriteXMLIntValue(doc, root, "MfpCfgRdsPu", m_byRdsPu);
		WriteXMLIntValue(doc, root, "HallLaCfgHallOvMd", m_byHallOvMd);
		WriteXMLIntValue(doc, root, "MfpCfgHallPu", m_byHallPu);
		WriteXMLIntValue(doc, root, "MfpCfgOuvpThr", m_byOuvpThr);
		WriteXMLIntValue(doc, root, "MfpCfgHallLttEn", m_byHallLttEn);
		WriteXMLIntValue(doc, root, "St5OthCfgMcOffSel", m_byMcOffSel);
		WriteXMLIntValue(doc, root, "St5OthCfgIdleTime", m_byIdleTime);
		WriteXMLIntValue(doc, root, "St5OthCfgTsdSel", m_byS5TsdSel);
		WriteXMLIntValue(doc, root, "St5OthCfgOvpThr", m_byOvpThr);
		WriteXMLIntValue(doc, root, "St5OthCfgPwrFltSel", m_byPwrFltSel);
		WriteXMLIntValue(doc, root, "St5OthCfgEtcEn", m_byEtcEn);
		WriteXMLIntValue(doc, root, "St5OthCfgPwrAmpSel", m_byPwrAmpSel);
		WriteXMLIntValue(doc, root, "St5OthCfgLimitSel", m_byLimitSel);
		WriteXMLIntValue(doc, root, "St5OthCfgITripEn", m_byITripEn);
		WriteXMLIntValue(doc, root, "St5OthCfgLimitITrip", m_byLimitITrip);
		WriteXMLIntValue(doc, root, "St5OthCfgIsdMd2Time", m_byIsdMd2Time);
		WriteXMLIntValue(doc, root, "St5OthCfgIsdFltSel", m_byIsdFltSel);
		WriteXMLIntValue(doc, root, "St5OthCfgIsdThr", m_byIsdThr);
		WriteXMLIntValue(doc, root, "St5OthCfgItripRe", m_byITripRe);
		WriteXMLIntValue(doc, root, "St5OthCfgUvpThr", m_byUvpThr);
		WriteXMLIntValue(doc, root, "St5OthCfgFlocLdqVoltKp", m_byFlocLdVoltKp);
		WriteXMLIntValue(doc, root, "St5OthCfgFlocLqVoltKp", m_byFlocLqVoltKp);
		WriteXMLIntValue(doc, root, "St5OthCfgFocFluxVoltKp", m_byFocFluxVoltKp);
		WriteXMLIntValue(doc, root, "St5OthCfgDrvDisMod", m_byDrvDisMod);
		WriteXMLIntValue(doc, root, "St5OthCfgSpdBlk0", m_wSpdBlk0);
		WriteXMLIntValue(doc, root, "St5OthCfgSpdBlk1", m_wSpdBlk1);
		WriteXMLIntValue(doc, root, "St5OthCfgSpdBlk2", m_wSpdBlk2);
		WriteXMLIntValue(doc, root, "St5OthCfgSpdBlkWid", m_wSpdBlkWid);
		WriteXMLIntValue(doc, root, "St5OthCfgTheCompSel", m_byTheCompSel);
		WriteXMLIntValue(doc, root, "St5OthCfgOcpDa", m_byOcpDa);
		WriteXMLIntValue(doc, root, "St5OthCfgCmp0ModSel", m_byCmp0ModSel);
		WriteXMLIntValue(doc, root, "St5OthCfgCmp0Hys", m_byCmp0Hys);
		WriteXMLIntValue(doc, root, "St5OthCfgCmp0CkSel", m_byCmp0CkSel);
		WriteXMLIntValue(doc, root, "St5OthCfgAmpSingle", m_byAmpSingle);
		WriteXMLIntValue(doc, root, "St5OthCfgAmpFs", m_byAmpFs);
		WriteXMLIntValue(doc, root, "St5OthCfgOvpDisBk", m_byOvpDisBk);
		WriteXMLIntValue(doc, root, "St5OthCfgOcpDisBk", m_byOcpDisBk);
		WriteXMLIntValue(doc, root, "St5OthCfgOcpSelBk", m_byOcpSelBk);
		WriteXMLIntValue(doc, root, "St5OthCfgOcpMod", m_byOcpMod);
		WriteXMLIntValue(doc, root, "St5OthCfgStallTstQMax", m_byStallTstQMax);
		WriteXMLIntValue(doc, root, "St5OthCfgStallTstInc", m_byStallTstInc);
		WriteXMLIntValue(doc, root, "St5OthCfgStallTstEn", m_byStallTstEn);
		WriteXMLIntValue(doc, root, "St5OthCfgDirInv", m_byDirInv);
		WriteXMLIntValue(doc, root, "St5OthCfgDirEn0", m_byDirEn0);
		WriteXMLIntValue(doc, root, "St5OthCfgDirEn1", m_byDirEn1);
		return true;
	}
}
