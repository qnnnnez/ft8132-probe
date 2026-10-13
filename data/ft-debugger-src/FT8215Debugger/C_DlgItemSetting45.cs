using System.Windows;
using System.Windows.Controls;

namespace FT8215Debugger;

public class C_DlgItemSetting45
{
	public MainWindow mainWindow { get; set; }

	public C_DlgItemSetting45()
	{
		mainWindow = (MainWindow)Application.Current.MainWindow;
	}

	public int Init2DefaultConfig()
	{
		int result = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallForMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel = 0f;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaDu = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaTi = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallModsel = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999 = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3 = 2.5f;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu = 0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrAmpSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdThr = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.fFlocLdqVoltKp = 15f;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.fFlocLqVoltKp = 15f;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wFocFluxVoltKp = 10000;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0 = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1 = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2 = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpDis = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0 = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1 = 0;
		return result;
	}

	public int OpenSpecifyConfig(string filename)
	{
		int result = 0;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallMd = mainWindow.commonConfigSetting.m_byHallMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd = mainWindow.commonConfigSetting.m_byHallComMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn = mainWindow.commonConfigSetting.m_byHall0RevEn;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn = mainWindow.commonConfigSetting.m_byHall1RevEn;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn = mainWindow.commonConfigSetting.m_byHall2RevEn;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd = mainWindow.commonConfigSetting.m_byHallFgMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallForMd = mainWindow.commonConfigSetting.m_byHallForMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd = mainWindow.commonConfigSetting.m_byHallStaMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd = mainWindow.commonConfigSetting.m_byHallCorMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel = mainWindow.commonConfigSetting.m_fHallLockSel;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn = mainWindow.commonConfigSetting.m_byHallPllEn;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi = mainWindow.commonConfigSetting.m_byHallLockSwi;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp = mainWindow.commonConfigSetting.m_byHallStaOp;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd = mainWindow.commonConfigSetting.m_byHallSwiMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd = mainWindow.commonConfigSetting.m_byHallCalMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd = mainWindow.commonConfigSetting.m_byHallSvMd;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt = mainWindow.commonConfigSetting.m_byHallLockExt;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel = mainWindow.commonConfigSetting.m_byHallLimSel;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1 = mainWindow.commonConfigSetting.m_byHallRsv1;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaDu = mainWindow.commonConfigSetting.m_byHaDu;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaTi = mainWindow.commonConfigSetting.m_byHaTi;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed = mainWindow.commonConfigSetting.m_byHallAlignSpeed;
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallModsel = mainWindow.commonConfigSetting.m_byHallModsel;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0 = mainWindow.commonConfigSetting.m_byHallLA0;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125 = mainWindow.commonConfigSetting.m_byHallLA125;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250 = mainWindow.commonConfigSetting.m_byHallLA250;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375 = mainWindow.commonConfigSetting.m_byHallLA375;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500 = mainWindow.commonConfigSetting.m_byHallLA500;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625 = mainWindow.commonConfigSetting.m_byHallLA625;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750 = mainWindow.commonConfigSetting.m_byHallLA750;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875 = mainWindow.commonConfigSetting.m_byHallLA875;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999 = mainWindow.commonConfigSetting.m_byHallLA999;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn = mainWindow.commonConfigSetting.m_byMfpEn;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3 = mainWindow.commonConfigSetting.m_fMfpEk3;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel = mainWindow.commonConfigSetting.m_byCarrierSel;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu = mainWindow.commonConfigSetting.m_byRdsPu;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu = mainWindow.commonConfigSetting.m_byHallPu;
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr = mainWindow.commonConfigSetting.m_byOuvpThr;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel = mainWindow.commonConfigSetting.m_byMcOffSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime = mainWindow.commonConfigSetting.m_byIdleTime;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr = mainWindow.commonConfigSetting.m_byOvpThr;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn = mainWindow.commonConfigSetting.m_byEtcEn;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel = mainWindow.commonConfigSetting.m_byPwrFltSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrAmpSel = mainWindow.commonConfigSetting.m_byPwrAmpSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel = mainWindow.commonConfigSetting.m_byLimitSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn = mainWindow.commonConfigSetting.m_byITripEn;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip = mainWindow.commonConfigSetting.m_byLimitITrip;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time = mainWindow.commonConfigSetting.m_byIsdMd2Time;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel = mainWindow.commonConfigSetting.m_byIsdFltSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdThr = mainWindow.commonConfigSetting.m_byIsdThr;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe = mainWindow.commonConfigSetting.m_byITripRe;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr = mainWindow.commonConfigSetting.m_byUvpThr;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0 = mainWindow.commonConfigSetting.m_wSpdBlk0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1 = mainWindow.commonConfigSetting.m_wSpdBlk1;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2 = mainWindow.commonConfigSetting.m_wSpdBlk2;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid = mainWindow.commonConfigSetting.m_wSpdBlkWid;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel = mainWindow.commonConfigSetting.m_byTheCompSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa = mainWindow.commonConfigSetting.m_byOcpDa;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel = mainWindow.commonConfigSetting.m_byCmp0ModSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys = mainWindow.commonConfigSetting.m_byCmp0Hys;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle = mainWindow.commonConfigSetting.m_byAmpSingle;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs = mainWindow.commonConfigSetting.m_byAmpFs;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpDis = mainWindow.commonConfigSetting.m_byOvpDis;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis = mainWindow.commonConfigSetting.m_byOcpDis;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel = mainWindow.commonConfigSetting.m_byOcpSel;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod = mainWindow.commonConfigSetting.m_byOcpMod;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod = 0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis = mainWindow.commonConfigSetting.m_bySlpDis;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv = mainWindow.commonConfigSetting.m_byDirInv;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0 = mainWindow.commonConfigSetting.m_byDirEn0;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1 = mainWindow.commonConfigSetting.m_byDirEn1;
		return result;
	}

	public bool UpdateUIDataToUserCfg()
	{
		if (mainWindow.commonConfigSetting == null || mainWindow.m_sMotorParamCfg == null)
		{
			return false;
		}
		mainWindow.commonConfigSetting.m_byHallMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallMd;
		mainWindow.commonConfigSetting.m_byHallComMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd;
		mainWindow.commonConfigSetting.m_byHall0RevEn = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn;
		mainWindow.commonConfigSetting.m_byHall1RevEn = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn;
		mainWindow.commonConfigSetting.m_byHall2RevEn = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn;
		mainWindow.commonConfigSetting.m_byHallFgMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd;
		mainWindow.commonConfigSetting.m_byHallForMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallForMd;
		mainWindow.commonConfigSetting.m_byHallStaMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd;
		mainWindow.commonConfigSetting.m_byHallCorMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd;
		mainWindow.commonConfigSetting.m_fHallLockSel = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel;
		mainWindow.commonConfigSetting.m_byHallPllEn = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn;
		mainWindow.commonConfigSetting.m_byHallLockSwi = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi;
		mainWindow.commonConfigSetting.m_byHallStaOp = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp;
		mainWindow.commonConfigSetting.m_byHallSwiMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd;
		mainWindow.commonConfigSetting.m_byHallCalMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd;
		mainWindow.commonConfigSetting.m_byHallSvMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd;
		mainWindow.commonConfigSetting.m_byHallLockExt = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt;
		mainWindow.commonConfigSetting.m_byHallLimSel = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel;
		mainWindow.commonConfigSetting.m_byHallRsv1 = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1;
		mainWindow.commonConfigSetting.m_byHaDu = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaDu;
		mainWindow.commonConfigSetting.m_byHaTi = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaTi;
		mainWindow.commonConfigSetting.m_byHallAlignSpeed = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed;
		mainWindow.commonConfigSetting.m_byHallModsel = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallModsel;
		mainWindow.commonConfigSetting.m_byHallLA0 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0;
		mainWindow.commonConfigSetting.m_byHallLA125 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125;
		mainWindow.commonConfigSetting.m_byHallLA250 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250;
		mainWindow.commonConfigSetting.m_byHallLA375 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375;
		mainWindow.commonConfigSetting.m_byHallLA500 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500;
		mainWindow.commonConfigSetting.m_byHallLA625 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625;
		mainWindow.commonConfigSetting.m_byHallLA750 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750;
		mainWindow.commonConfigSetting.m_byHallLA875 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875;
		mainWindow.commonConfigSetting.m_byHallLA999 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999;
		mainWindow.commonConfigSetting.m_byMfpEn = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn;
		mainWindow.commonConfigSetting.m_fMfpEk3 = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3;
		mainWindow.commonConfigSetting.m_byCarrierSel = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel;
		mainWindow.commonConfigSetting.m_byRdsPu = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu;
		mainWindow.commonConfigSetting.m_byHallPu = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu;
		mainWindow.commonConfigSetting.m_byOuvpThr = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr;
		mainWindow.commonConfigSetting.m_byMcOffSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel;
		mainWindow.commonConfigSetting.m_byIdleTime = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime;
		mainWindow.commonConfigSetting.m_byOvpThr = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr;
		mainWindow.commonConfigSetting.m_byPwrFltSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel;
		mainWindow.commonConfigSetting.m_byEtcEn = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn;
		mainWindow.commonConfigSetting.m_byPwrAmpSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrAmpSel;
		mainWindow.commonConfigSetting.m_byLimitSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel;
		mainWindow.commonConfigSetting.m_byITripEn = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn;
		mainWindow.commonConfigSetting.m_byLimitITrip = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip;
		mainWindow.commonConfigSetting.m_byIsdMd2Time = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time;
		mainWindow.commonConfigSetting.m_byIsdFltSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel;
		mainWindow.commonConfigSetting.m_byIsdThr = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdThr;
		mainWindow.commonConfigSetting.m_byITripRe = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe;
		mainWindow.commonConfigSetting.m_byUvpThr = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr;
		mainWindow.commonConfigSetting.m_wSpdBlk0 = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0;
		mainWindow.commonConfigSetting.m_wSpdBlk1 = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1;
		mainWindow.commonConfigSetting.m_wSpdBlk2 = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2;
		mainWindow.commonConfigSetting.m_wSpdBlkWid = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid;
		mainWindow.commonConfigSetting.m_byTheCompSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel;
		mainWindow.commonConfigSetting.m_byOcpDa = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa;
		mainWindow.commonConfigSetting.m_byCmp0ModSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel;
		mainWindow.commonConfigSetting.m_byCmp0Hys = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys;
		mainWindow.commonConfigSetting.m_byAmpSingle = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle;
		mainWindow.commonConfigSetting.m_byAmpFs = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs;
		mainWindow.commonConfigSetting.m_byOvpDis = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpDis;
		mainWindow.commonConfigSetting.m_byOcpDis = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis;
		mainWindow.commonConfigSetting.m_byOcpSel = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel;
		mainWindow.commonConfigSetting.m_byOcpMod = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod;
		mainWindow.commonConfigSetting.m_bySlpDis = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis;
		mainWindow.commonConfigSetting.m_byDirInv = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv;
		mainWindow.commonConfigSetting.m_byDirEn0 = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0;
		mainWindow.commonConfigSetting.m_byDirEn1 = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1;
		return true;
	}

	public bool ClearUIContent()
	{
		mainWindow.comboBox_HallMode.Items.Clear();
		mainWindow.comboBox_HallFgMd.Items.Clear();
		mainWindow.comboBox_CmpModeSel.Items.Clear();
		return true;
	}

	public bool ConverUIToParam()
	{
		if (mainWindow.m_sMotorParamCfg == null)
		{
			return false;
		}
		if (mainWindow.radioBtn_HallComMd0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd = 0;
		}
		if (mainWindow.radioBtn_HallComMd1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd = 1;
		}
		if (mainWindow.radioBtn_HallComMd2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd = 2;
		}
		if (mainWindow.radioBtn_HallComMd3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd = 3;
		}
		if (mainWindow.radioBtn_Hall0RevDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn = 1;
		}
		if (mainWindow.radioBtn_Hall1RevDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn = 1;
		}
		if (mainWindow.radioBtn_Hall2RevDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn = 1;
		}
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd = (byte)mainWindow.comboBox_HallFgMd.SelectedIndex;
		if (mainWindow.radioBtn_HallForMdDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallForMd = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallForMd = 1;
		}
		if (mainWindow.radioBtn_HallStaMdDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd = 1;
		}
		if (mainWindow.radioBtn_HallCorMdDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd = 1;
		}
		if (mainWindow.radioBtn_HallPllDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn = 1;
		}
		if (mainWindow.radioBtn_HallLockSwiDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi = 1;
		}
		if (mainWindow.radioBtn_HallStaOpDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp = 1;
		}
		if (mainWindow.radioBtn_HallCalMdDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd = 1;
		}
		if (mainWindow.radioBtn_HallSvMdDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd = 1;
		}
		if (mainWindow.radioBtn_HallLockExtEn.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt = 1;
		}
		if (mainWindow.radioBtn_HallLimSel0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel = 1;
		}
		if (mainWindow.radioBtn_HallRsv1_0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1 = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1 = 1;
		}
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd = GetHallSwiMdByName(mainWindow.textBlock_HallSwiMd.Text);
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel = float.Parse(mainWindow.textBlock_HallLockSel.Text);
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaDu = byte.Parse(mainWindow.textBox_HaDu.Text);
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaTi = byte.Parse(mainWindow.textBox_HaTi.Text);
		mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed = byte.Parse(mainWindow.textBox_HallAlignSpeed.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0 = byte.Parse(mainWindow.textBlock_HallLa0.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125 = byte.Parse(mainWindow.textBlock_HallLa125.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250 = byte.Parse(mainWindow.textBlock_HallLa250.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375 = byte.Parse(mainWindow.textBlock_HallLa375.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500 = byte.Parse(mainWindow.textBlock_HallLa500.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625 = byte.Parse(mainWindow.textBlock_HallLa625.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750 = byte.Parse(mainWindow.textBlock_HallLa750.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875 = byte.Parse(mainWindow.textBlock_HallLa875.Text);
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999 = byte.Parse(mainWindow.textBlock_HallLa999.Text);
		if (mainWindow.radioBtn_MfpDis.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn = 1;
		}
		mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3 = float.Parse(mainWindow.textBox_MfpEk3.Text);
		if (mainWindow.radioBtn_CarrierSel0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel = 0;
		}
		if (mainWindow.radioBtn_CarrierSel1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel = 1;
		}
		if (mainWindow.radioBtn_CarrierSel2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel = 2;
		}
		if (mainWindow.radioBtn_CarrierSel3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel = 3;
		}
		if (mainWindow.radioBtn_RdsPu0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu = 1;
		}
		if (mainWindow.radioBtn_HallPu0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu = 1;
		}
		if (mainWindow.radioBtn_OuvpThr0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr = 1;
		}
		if (mainWindow.radioBtn_McOffSel0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel = 0;
		}
		if (mainWindow.radioBtn_McOffSel1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel = 1;
		}
		if (mainWindow.radioBtn_McOffSel2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel = 2;
		}
		if (mainWindow.radioBtn_McOffSel3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel = 3;
		}
		if (mainWindow.radioBtn_IdleTime0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime = 0;
		}
		if (mainWindow.radioBtn_IdleTime1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime = 1;
		}
		if (mainWindow.radioBtn_IdleTime2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime = 2;
		}
		if (mainWindow.radioBtn_IdleTime3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime = 3;
		}
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr = (byte)mainWindow.slider_OvpThr.Value;
		if (mainWindow.radioBtn_PwrFltSel0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel = 0;
		}
		if (mainWindow.radioBtn_PwrFltSel1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel = 1;
		}
		if (mainWindow.radioBtn_PwrFltSel2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel = 2;
		}
		if (mainWindow.radioBtn_PwrFltSel3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel = 3;
		}
		if (mainWindow.radioBtn_EtcEn0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn = 1;
		}
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel = byte.Parse(mainWindow.textBlock_LimitSel.Text);
		if (mainWindow.radioBtn_ITripEn0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn = 1;
		}
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip = byte.Parse(mainWindow.textBox_LimitITrip.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time = byte.Parse(mainWindow.textBlock_IsdMd2Time.Text);
		if (mainWindow.radioBtn_IsdFltSel0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel = 0;
		}
		if (mainWindow.radioBtn_IsdFltSel1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel = 1;
		}
		if (mainWindow.radioBtn_IsdFltSel2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel = 2;
		}
		if (mainWindow.radioBtn_IsdFltSel3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel = 3;
		}
		if (mainWindow.radioBtn_IripRe0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe = 1;
		}
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr = (byte)mainWindow.slider_UvpThr.Value;
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0 = ushort.Parse(mainWindow.textBox_SpdBlk0.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1 = ushort.Parse(mainWindow.textBox_SpdBlk1.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2 = ushort.Parse(mainWindow.textBox_SpdBlk2.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid = ushort.Parse(mainWindow.textBox_SpdBlkWid.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel = byte.Parse(mainWindow.textBox_TheCompSel.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa = byte.Parse(mainWindow.textBox_OcpDa.Text);
		mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel = (byte)mainWindow.comboBox_CmpModeSel.SelectedIndex;
		if (mainWindow.radioBtn_Cmp0Phy0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys = 0;
		}
		if (mainWindow.radioBtn_Cmp0Phy1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys = 1;
		}
		if (mainWindow.radioBtn_Cmp0Phy2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys = 2;
		}
		if (mainWindow.radioBtn_Cmp0Phy3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys = 3;
		}
		if (mainWindow.radioBtn_AmpSingle0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle = 1;
		}
		if (mainWindow.radioBtn_AmpFs0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs = 1;
		}
		if (mainWindow.radioBtn_OcpDis0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis = 1;
		}
		if (mainWindow.radioBtn_OcpSel0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel = 0;
		}
		if (mainWindow.radioBtn_OcpSel1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel = 1;
		}
		if (mainWindow.radioBtn_OcpSel2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel = 2;
		}
		if (mainWindow.radioBtn_OcpSel3.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel = 3;
		}
		if (mainWindow.radioBtn_OcpMod0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod = 0;
		}
		if (mainWindow.radioBtn_OcpMod1.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod = 1;
		}
		if (mainWindow.radioBtn_OcpMod2.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod = 2;
		}
		if (mainWindow.radioBtn_SlpDis0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis = 1;
		}
		if (mainWindow.radioBtn_DirInv0.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv = 1;
		}
		if (mainWindow.radioBtn_DirEn00.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0 = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0 = 1;
		}
		if (mainWindow.radioBtn_DirEn10.IsChecked == true)
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1 = 0;
		}
		else
		{
			mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1 = 1;
		}
		return true;
	}

	public void InitSliderTicks(Slider slider, string strValPrefix)
	{
		mainWindow.InitSliderTicks(slider, strValPrefix);
	}

	public int HandleParamDisplay()
	{
		int result = 0;
		int num = 0;
		string text = "Cfg_HallMode";
		string text2 = "";
		string text3 = "";
		for (num = 0; num < 5; num++)
		{
			text2 = ((num <= 1) ? (text + num) : (text + (num * 2 - 1)));
			text3 = (string)Window.GetWindow(mainWindow).FindResource(text2);
			mainWindow.comboBox_HallMode.Items.Add(text3);
		}
		byte byHallMd = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallMd;
		mainWindow.comboBox_HallMode.SelectedIndex = (byHallMd + 1) / 2;
		switch (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallComMd)
		{
		case 0:
			mainWindow.radioBtn_HallComMd0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_HallComMd1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_HallComMd2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_HallComMd3.IsChecked = true;
			break;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall0RevEn == 0)
		{
			mainWindow.radioBtn_Hall0RevDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_Hall0RevEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall1RevEn == 0)
		{
			mainWindow.radioBtn_Hall1RevDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_Hall1RevEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHall2RevEn == 0)
		{
			mainWindow.radioBtn_Hall2RevDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_Hall2RevEn.IsChecked = true;
		}
		Window window = Window.GetWindow(mainWindow);
		mainWindow.comboBox_HallFgMd.Items.Add((string)window.FindResource("Cfg_HallFgMode0"));
		mainWindow.comboBox_HallFgMd.Items.Add((string)window.FindResource("Cfg_HallFgMode1"));
		mainWindow.comboBox_HallFgMd.Items.Add((string)window.FindResource("Cfg_HallFgMode2"));
		mainWindow.comboBox_HallFgMd.Items.Add((string)window.FindResource("Cfg_HallFgMode3"));
		mainWindow.comboBox_HallFgMd.SelectedIndex = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallFgMd;
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallForMd == 0)
		{
			mainWindow.radioBtn_HallForMdDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallForMDEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaMd == 0)
		{
			mainWindow.radioBtn_HallStaMdDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallStaMdEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCorMd == 0)
		{
			mainWindow.radioBtn_HallCorMdDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallCorMdEn.IsChecked = true;
		}
		string strValPrefix = "Cfg_HallLockSel";
		InitSliderTicks(mainWindow.slider_HallLockSel, strValPrefix);
		mainWindow.textBlock_HallLockSel.Text = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.fHallLockSel.ToString();
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallPllEn == 0)
		{
			mainWindow.radioBtn_HallPllDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallPllEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockSwi == 0)
		{
			mainWindow.radioBtn_HallLockSwiDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallLockSwiEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallStaOp == 0)
		{
			mainWindow.radioBtn_HallStaOpDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallStaOpEn.IsChecked = true;
		}
		strValPrefix = "Cfg_HallSwiMd";
		InitSliderTicks(mainWindow.slider_HallSwiMd, strValPrefix);
		mainWindow.slider_HallSwiMd.Value = (int)mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd;
		string hallSwiStrByIdx = GetHallSwiStrByIdx(mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSwiMd);
		mainWindow.textBlock_HallSwiMd.Text = hallSwiStrByIdx;
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallCalMd == 0)
		{
			mainWindow.radioBtn_HallCalMdDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallCallMdEn.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallSvMd == 0)
		{
			mainWindow.radioBtn_HallSvMdDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallSvMdEn.IsChecked = true;
		}
		if (mainWindow.radioBtn_HallStaOpEn.IsChecked == true || mainWindow.radioBtn_HallSvMdEn.IsChecked == true)
		{
			mainWindow.label25.Visibility = Visibility.Hidden;
			mainWindow.textBox_StartCurrent.Visibility = Visibility.Hidden;
			mainWindow.textBox_StartCurrent.Text = mainWindow.m_sMotorParamCfg.m_sStartCfg.fStartCurrent.ToString();
			mainWindow.label_StartDuty30.Visibility = Visibility.Visible;
			mainWindow.textBox_StartDuty30.Visibility = Visibility.Visible;
			mainWindow.label_StartDutyUnit.Visibility = Visibility.Visible;
			mainWindow.textBox_StartDuty30.Text = mainWindow.m_sMotorParamCfg.m_sStartCfg.byStartDuty.ToString();
		}
		else
		{
			mainWindow.label25.Visibility = Visibility.Visible;
			mainWindow.textBox_StartCurrent.Visibility = Visibility.Visible;
			mainWindow.textBox_StartCurrent.Text = mainWindow.m_sMotorParamCfg.m_sStartCfg.fStartCurrent.ToString();
			mainWindow.label_StartDuty30.Visibility = Visibility.Hidden;
			mainWindow.textBox_StartDuty30.Visibility = Visibility.Hidden;
			mainWindow.label_StartDutyUnit.Visibility = Visibility.Hidden;
			mainWindow.textBox_StartDuty30.Text = mainWindow.m_sMotorParamCfg.m_sStartCfg.byStartDuty.ToString();
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLockExt == 0)
		{
			mainWindow.radioBtn_HallLockExtEn.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallLockExtDis.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallLimSel == 0)
		{
			mainWindow.radioBtn_HallLimSel0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallLimSel1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallRsv1 == 0)
		{
			mainWindow.radioBtn_HallRsv1_0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallRsv1_1.IsChecked = true;
		}
		mainWindow.textBox_HaDu.Text = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaDu.ToString();
		mainWindow.textBox_HaTi.Text = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHaTi.ToString();
		mainWindow.textBox_HallAlignSpeed.Text = mainWindow.m_sMotorParamCfg.m_sHallParamCfg.byHallAlignSpeed.ToString();
		strValPrefix = "Cfg_HallLA0";
		InitSliderTicks(mainWindow.slider_HallLa0, strValPrefix);
		mainWindow.textBlock_HallLa0.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA0.ToString();
		InitSliderTicks(mainWindow.slider_HallLa125, strValPrefix);
		mainWindow.textBlock_HallLa125.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA125.ToString();
		InitSliderTicks(mainWindow.slider_HallLa250, strValPrefix);
		mainWindow.textBlock_HallLa250.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA250.ToString();
		InitSliderTicks(mainWindow.slider_HallLa375, strValPrefix);
		mainWindow.textBlock_HallLa375.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA375.ToString();
		InitSliderTicks(mainWindow.slider_HallLa500, strValPrefix);
		mainWindow.textBlock_HallLa500.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA500.ToString();
		InitSliderTicks(mainWindow.slider_HallLa625, strValPrefix);
		mainWindow.textBlock_HallLa625.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA625.ToString();
		InitSliderTicks(mainWindow.slider_HallLa750, strValPrefix);
		mainWindow.textBlock_HallLa750.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA750.ToString();
		InitSliderTicks(mainWindow.slider_HallLa875, strValPrefix);
		mainWindow.textBlock_HallLa875.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA875.ToString();
		InitSliderTicks(mainWindow.slider_HallLa999, strValPrefix);
		mainWindow.textBlock_HallLa999.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallLA999.ToString();
		if (mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byMfpEn == 0)
		{
			mainWindow.radioBtn_MfpDis.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_MfpEn.IsChecked = true;
		}
		mainWindow.textBox_MfpEk3.Text = mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.fMfpEk3.ToString();
		switch (mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byCarrierSel)
		{
		case 0:
			mainWindow.radioBtn_CarrierSel0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_CarrierSel1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_CarrierSel2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_CarrierSel3.IsChecked = true;
			break;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byRdsPu == 0)
		{
			mainWindow.radioBtn_RdsPu0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_RdsPu1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byHallPu == 0)
		{
			mainWindow.radioBtn_HallPu0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_HallPu1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sHallLAMfpCfg.byOuvpThr == 0)
		{
			mainWindow.radioBtn_OuvpThr0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_OuvpThr1.IsChecked = true;
		}
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byMcOffSel)
		{
		case 0:
			mainWindow.radioBtn_McOffSel0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_McOffSel1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_McOffSel2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_McOffSel3.IsChecked = true;
			break;
		}
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIdleTime)
		{
		case 0:
			mainWindow.radioBtn_IdleTime0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_IdleTime1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_IdleTime2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_IdleTime3.IsChecked = true;
			break;
		}
		strValPrefix = "Cfg_S5TsdSel";
		strValPrefix = "Cfg_S5OvpThr";
		InitSliderTicks(mainWindow.slider_OvpThr, strValPrefix);
		mainWindow.slider_OvpThr.Value = (int)mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOvpThr;
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byPwrFltSel)
		{
		case 0:
			mainWindow.radioBtn_PwrFltSel0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_PwrFltSel1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_PwrFltSel2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_PwrFltSel3.IsChecked = true;
			break;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byEtcEn == 0)
		{
			mainWindow.radioBtn_EtcEn0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_EtcEn1.IsChecked = true;
		}
		strValPrefix = "Cfg_LimitSel";
		InitSliderTicks(mainWindow.slider_LimitSel, strValPrefix);
		mainWindow.textBlock_LimitSel.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitSel.ToString();
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripEn == 0)
		{
			mainWindow.radioBtn_ITripEn0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_ITripEn1.IsChecked = true;
		}
		strValPrefix = "Cfg_S5LimitTrip";
		InitSliderTicks(mainWindow.slider_LimitITrip, strValPrefix);
		mainWindow.textBox_LimitITrip.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byLimitITrip.ToString();
		strValPrefix = "Cfg_PwrAmpSel";
		InitSliderTicks(mainWindow.slider_IsdMd2Time, strValPrefix);
		mainWindow.textBlock_IsdMd2Time.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdMd2Time.ToString();
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byIsdFltSel)
		{
		case 0:
			mainWindow.radioBtn_IsdFltSel0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_IsdFltSel1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_IsdFltSel2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_IsdFltSel3.IsChecked = true;
			break;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byITripRe == 0)
		{
			mainWindow.radioBtn_IripRe0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_IripRe1.IsChecked = true;
		}
		strValPrefix = "Cfg_S5OvpThr";
		InitSliderTicks(mainWindow.slider_UvpThr, strValPrefix);
		mainWindow.slider_UvpThr.Value = (int)mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byUvpThr;
		mainWindow.textBox_SpdBlk0.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk0.ToString();
		mainWindow.ShowSpdBlk0Speed();
		mainWindow.textBox_SpdBlk1.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk1.ToString();
		mainWindow.ShowSpdBlk1Speed();
		mainWindow.textBox_SpdBlk2.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlk2.ToString();
		mainWindow.ShowSpdBlk2Speed();
		mainWindow.textBox_SpdBlkWid.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.wSpdBlkWid.ToString();
		mainWindow.ShowSpdBlkWidSpeed();
		strValPrefix = "Cfg_S5TheCompSel";
		InitSliderTicks(mainWindow.slider_TheCompSel, strValPrefix);
		mainWindow.textBox_TheCompSel.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byTheCompSel.ToString();
		mainWindow.textBox_OcpDa.Text = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDa.ToString();
		mainWindow.ShowOcpDaVoltage();
		num = 0;
		text = "Cfg_Com0MdSel";
		for (num = 0; num < 16; num++)
		{
			text2 = text + num;
			text3 = (string)Window.GetWindow(mainWindow).FindResource(text2);
			mainWindow.comboBox_CmpModeSel.Items.Add(text3);
		}
		mainWindow.comboBox_CmpModeSel.SelectedIndex = mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0ModSel;
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byCmp0Hys)
		{
		case 0:
			mainWindow.radioBtn_Cmp0Phy0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_Cmp0Phy1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_Cmp0Phy2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_Cmp0Phy3.IsChecked = true;
			break;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpSingle == 0)
		{
			mainWindow.radioBtn_AmpSingle0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_AmpSingle1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byAmpFs == 0 || mainWindow.m_sMotorParamCfg.m_sFocParamCfg.bySMPMode == 0)
		{
			mainWindow.radioBtn_AmpFs0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_AmpFs1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpDis == 0)
		{
			mainWindow.radioBtn_OcpDis0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_OcpDis1.IsChecked = true;
		}
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpSel)
		{
		case 0:
			mainWindow.radioBtn_OcpSel0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_OcpSel1.IsChecked = true;
			break;
		case 2:
			mainWindow.radioBtn_OcpSel2.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_OcpSel3.IsChecked = true;
			break;
		}
		switch (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byOcpMod)
		{
		case 0:
			mainWindow.radioBtn_OcpMod0.IsChecked = true;
			break;
		case 1:
			mainWindow.radioBtn_OcpMod1.IsChecked = true;
			break;
		default:
			mainWindow.radioBtn_OcpMod2.IsChecked = true;
			break;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.bySlpDis == 0)
		{
			mainWindow.radioBtn_SlpDis0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_SlpDis1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirInv == 0)
		{
			mainWindow.radioBtn_DirInv0.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_DirInv1.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn0 == 0)
		{
			mainWindow.radioBtn_DirEn00.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_DirEn01.IsChecked = true;
		}
		if (mainWindow.m_sMotorParamCfg.m_sOtherParamS5Cfg.byDirEn1 == 0)
		{
			mainWindow.radioBtn_DirEn10.IsChecked = true;
		}
		else
		{
			mainWindow.radioBtn_DirEn11.IsChecked = true;
		}
		return result;
	}

	private byte GetHallSwiMdByName(string strTxt)
	{
		byte b = 0;
		if (strTxt.CompareTo("ABC") == 0)
		{
			return 0;
		}
		if (strTxt.CompareTo("BAC") == 0)
		{
			return 1;
		}
		if (strTxt.CompareTo("CBA") == 0)
		{
			return 2;
		}
		if (strTxt.CompareTo("ACB") == 0)
		{
			return 3;
		}
		if (strTxt.CompareTo("BCA") == 0)
		{
			return 4;
		}
		return 5;
	}

	public string GetHallSwiStrByIdx(byte byHallSwiMd)
	{
		string result = "";
		switch (byHallSwiMd)
		{
		case 0:
			result = "ABC";
			break;
		case 1:
			result = "BAC";
			break;
		case 2:
			result = "CBA";
			break;
		case 3:
			result = "ACB";
			break;
		case 4:
			result = "BCA";
			break;
		case 5:
			result = "CAB";
			break;
		}
		return result;
	}

	public byte GetHallLockSelByIdx(float fHallLockSel)
	{
		byte b = 0;
		if (fHallLockSel == 0f)
		{
			return 0;
		}
		if (fHallLockSel == 1.6f)
		{
			return 1;
		}
		if (fHallLockSel == 2.4f)
		{
			return 2;
		}
		if (fHallLockSel == 3.2f)
		{
			return 3;
		}
		if (fHallLockSel == 4.1f)
		{
			return 4;
		}
		if (fHallLockSel == 4.9f)
		{
			return 5;
		}
		if (fHallLockSel == 5.7f)
		{
			return 6;
		}
		return 7;
	}
}
