using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace FT8215Debugger;

public class ParamStructureDef
{
	public enum E_ControlMode
	{
		eFT32_CTRL_VOLTAGE_LOOP,
		eFT32_CTRL_CURRENT_LOOP,
		eFT32_CTRL_SPEED_LOOP
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_AppStartupCfg
	{
		public short i16SensorMode;

		public byte byCircuitType;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_MotorParamHwCfg
	{
		public byte byJiDuiShu;

		public float fDianZu;

		public float fDianGan;

		public float fFanDianDongShi;

		public float fCaiYangDDianZu;

		public byte byFanDaBeiShu;

		public float fRV;

		public byte byVdcAdj;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_FocParamCfg
	{
		public byte bySMPMode;

		public byte byStaTsam0En;

		public byte byPllSmoEn;

		public byte byStaPllEn;

		public byte bySeg7En;

		public byte bySta7SegEn;

		public byte byAtanEn;

		public byte byStaSin;

		public ushort wFreqBase;

		public byte bySampleTime;

		public byte bySpdBw;

		public byte byEbwMode;

		public float fLeadAngleSet;

		public byte byEkNum;

		public ushort wEbw;

		public float fAtoBw;

		public byte byTSMIN;

		public float fDTSEL;

		public byte byKSILDE;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_RpdAlignCfg
	{
		public byte byRpdAlignEn;

		public byte byAlignTime;

		public byte byAlignDuty;

		public byte byRPD10;

		public byte byRPD11;

		public byte byRPD12;

		public byte byRPD20;

		public byte byRPD21;

		public byte byRPD123_2;

		public byte byRvsAlignEn;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_StartCfg
	{
		public byte byControlMode;

		public float fStartCurrent;

		public byte byFocEfreqAccx;

		public float fFocEfreqMinx;

		public byte byFocEfreqHold;

		public byte byCloSpeed;

		public byte byStartDuty;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_RunCfg
	{
		public byte byStaIncTime;

		public byte byStbInc;

		public byte byStaInc;

		public float fDqKp;

		public float fDqKi;

		public float fWKp;

		public float fWKi;

		public byte byWoutMax;

		public byte byPwmOnoffhys;

		public byte byPwmOnoffhysClk;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_WindHandleCfg
	{
		public byte byRsdEn;

		public byte byRsdMode;

		public double dStaticSpeed;

		public byte byRevBrakeSpeed;

		public byte byForwardStaSpeed;

		public byte byRSD230;

		public byte byRSD231;

		public byte byRsdHallFilter;

		public byte byIsdBrakeEn;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_OtherCfg
	{
		public byte bySpdCtrlMd;

		public byte byPwmOffDis;

		public byte byPwmRevEn;

		public byte byQuickSta;

		public byte byCurveEnSpd;

		public byte byFgMul;

		public byte byFgDiv;

		public byte byFgIdleSet;

		public byte byFgSet;

		public byte byFgRd;

		public byte byStartClk;

		public byte byFgRdMd;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_TargetSpeedCurveCfg
	{
		public byte byXON0;

		public byte byX_ON;

		public byte byPreCtrlMode;

		public uint dwMinSpeed;

		public uint dwMaxSpeed;

		public float fMinCurrent;

		public float fMaxCurrent;

		public float fMinPower;

		public float fMaxPower;

		public byte byYMaxSel;

		public byte byPwmX98En;

		public byte byY_ON;

		public byte byY_25;

		public byte byY_50;

		public byte byY_75;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_ProtectCfg
	{
		public byte byFaultRecovTimes;

		public byte byLock0;

		public byte byLock1;

		public byte byLock2;

		public byte byLossPhaseEn;

		public uint wLockMinSpeed;

		public uint wLocMaxSpeed;

		public byte byLockEmfThd;

		public float fLockEmf;

		public float fLpNorhCurrent;

		public byte byUvloDis;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_UserStatusInfoCtrlCfg
	{
		public byte bySlaveId;

		public byte UID1;

		public byte bySpdCtrl;

		public byte bySpiDebug;

		public byte byDbgMd;

		public byte byMotorDir;

		public byte byPwmEn;

		public byte byFDS0;

		public byte byFDS1;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_ExtendParamCfg
	{
		public byte bySpeedVSet;

		public byte byVRefSel;

		public byte byFgTimeSel;

		public uint wLimitSpeed;

		public byte byAddr_A8;

		public byte byAddr_A9;

		public byte byAddr_AA;

		public byte byAddr_AB;

		public byte byAddr_AC;

		public byte byAddr_AD;

		public byte byAddr_AE;

		public byte byAddr_AF;

		public byte byAddr_B0;

		public byte byAddr_B1;

		public byte byAddr_B2;

		public byte byAddr_B3;

		public byte byAddr_B4;

		public byte byAddr_B5;

		public byte byAddr_B6;

		public byte byAddr_B7;

		public byte byAddr_94;

		public float fMpf_K3;

		public byte byAddr_B8;

		public byte byAddr_B9;

		public byte byAddr_BA;

		public byte byAddr_BB;

		public byte byAddr_BC;

		public byte byAddr_BD;

		public byte byAddr_BE;

		public byte byAddr_BF;

		public byte byAddr_97;

		public byte byAddr_98;

		public byte byAddr_99;

		public byte byAddr_9A;

		public byte byAddr_9B;

		public byte byAddr_A1;

		public byte byAddr_A2;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_SpecialRegCfg
	{
		public byte byOcpSel;

		public byte byIsdDirRev;

		public byte byOvpSel;

		public byte byTsdSel;

		public byte byDzSet;

		public byte byDpmDis;

		public byte byUvloDis;

		public byte byOvpDis;

		public byte byOcpDis;

		public byte byTsdDis;

		public byte byFocDis;

		public byte byBootMod;

		public byte byRestaEn;

		public byte byIsdFwMd;

		public byte byLaMd;

		public byte byNewFunc;

		public byte byIdleBrakeEn;

		public byte byIsdBrakeEn;

		public byte byTSMin;

		public byte byRvsAlignEn;

		public byte byRvsCutSel;

		public byte byNewSingle;

		public byte byDigTst;

		public byte byDigMoniCt;

		public float fRV;

		public byte byTestMode;

		public byte byIrefDis;

		public byte byHallOvMd;

		public byte byHallLttEn;

		public byte byStallTstQMax;

		public byte byStallTstInc;

		public byte byStallTstEn;

		public byte byDrvDisMod;

		public byte byCmp0CkSel;

		public byte byFlocLdVoltKp;

		public byte byFlocLqVoltKp;

		public byte byFocFluxVoltKp;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_OnOffTestCfg
	{
		public byte byOnOffTestEn;

		public byte byStartDuty;

		public byte byStopDuty;

		public byte byStartDuration;

		public byte byStopDuration;

		public ushort wTotalSetCnt;

		public ushort wExecutedCnt;

		public ushort wFailCnt;

		public ushort wPassCnt;

		public double dTgtSpeed;

		public double dMinDetectTime;

		public double dMaxDetectTime;

		public byte byDirSwitch;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_OnOffTestRecordCfg
	{
		public DateTime dtBeginTime;

		public DateTime dtEndTime;

		public double dTimeInterval;

		public double dCurSpeed;

		public string strStatus;

		public byte byPassStatus;
	}

	public struct S_ShowBASECfg
	{
		public string strHW_Board_Curr_Base;

		public string strHW_Board_Vol_Base2;

		public string strMotor_Power_Base;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_HallParamCfg
	{
		public byte byHallMd;

		public byte byHallComMd;

		public byte byHall0RevEn;

		public byte byHall1RevEn;

		public byte byHall2RevEn;

		public byte byHallFgMd;

		public byte byHallForMd;

		public byte byHallStaMd;

		public byte byHallCorMd;

		public float fHallLockSel;

		public byte byHallPllEn;

		public byte byHallLockSwi;

		public byte byHallStaOp;

		public byte byHallSwiMd;

		public byte byHallCalMd;

		public byte byHallSvMd;

		public byte byHallLockExt;

		public byte byHallLimSel;

		public byte byHallRsv1;

		public byte byHaDu;

		public byte byHaTi;

		public byte byHallAlignSpeed;

		public byte byHallModsel;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_HallLeadAngelMfpCfg
	{
		public byte byHallLA0;

		public byte byHallLA125;

		public byte byHallLA250;

		public byte byHallLA375;

		public byte byHallLA500;

		public byte byHallLA625;

		public byte byHallLA750;

		public byte byHallLA875;

		public byte byHallLA999;

		public byte byMfpEn;

		public float fMfpEk3;

		public byte byCarrierSel;

		public byte byRdsPu;

		public byte byHallPu;

		public byte byOuvpThr;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_OhterParamS5Cfg
	{
		public byte byMcOffSel;

		public byte byIdleTime;

		public byte byOvpThr;

		public byte byPwrFltSel;

		public byte byEtcEn;

		public byte byPwrAmpSel;

		public byte byLimitSel;

		public byte byITripEn;

		public byte byLimitITrip;

		public byte byIsdMd2Time;

		public byte byIsdFltSel;

		public byte byIsdThr;

		public byte byITripRe;

		public byte byUvpThr;

		public float fFlocLdqVoltKp;

		public float fFlocLqVoltKp;

		public ushort wFocFluxVoltKp;

		public ushort wSpdBlk0;

		public ushort wSpdBlk1;

		public ushort wSpdBlk2;

		public ushort wSpdBlkWid;

		public byte byTheCompSel;

		public byte byOcpDa;

		public byte byCmp0ModSel;

		public byte byCmp0Hys;

		public byte byAmpSingle;

		public byte byAmpFs;

		public byte byOvpDis;

		public byte byOcpDis;

		public byte byOcpSel;

		public byte byOcpMod;

		public byte bySlpDis;

		public byte byDirInv;

		public byte byDirEn0;

		public byte byDirEn1;
	}

	public const int MAX_VALUE = 250;

	public S_HallParamCfg m_sHallParamCfg;

	public S_HallLeadAngelMfpCfg m_sHallLAMfpCfg;

	public S_OhterParamS5Cfg m_sOtherParamS5Cfg;

	public S_AppStartupCfg m_sAppStartupCfg;

	public S_MotorParamHwCfg m_sMotorParamHwCfg;

	public S_FocParamCfg m_sFocParamCfg;

	public S_RpdAlignCfg m_sIpdAlignCfg;

	public S_StartCfg m_sStartCfg;

	public S_RunCfg m_sRunCfg;

	public S_WindHandleCfg m_sWindHandleCfg;

	public S_OtherCfg m_sOhterCfg;

	public S_ProtectCfg m_sProtectCfg;

	public S_OnOffTestCfg m_sOnOffTestCfg;

	public S_UserStatusInfoCtrlCfg m_sUserStatusInfoCfg;

	public E_ControlMode m_eCtrlMode;

	public S_SpecialRegCfg m_eSpecialRegCfg;

	public S_ExtendParamCfg m_sExtendCfg;

	public S_TargetSpeedCurveCfg m_sTargetSpdCurve;

	public S_ShowBASECfg m_sShowBASECfg;

	public MainWindow mainWindow { get; set; }

	public ParamStructureDef()
	{
		mainWindow = (MainWindow)Application.Current.MainWindow;
	}
}
