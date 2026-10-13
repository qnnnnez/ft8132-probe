namespace FT8215Debugger;

public class CDevListItemData
{
	public enum E_MEMOPT
	{
		eMemOpt_Unkonwn = 0,
		eMemOpt_PIN_MON = 1,
		eMemOpt_TESTER = 2,
		eMemOpt_BCFG = 19,
		eMemOpt_CCFG = 4,
		eMemOpt_BMTP = 5,
		eMemOpt_CMTP = 6,
		eMemOpt_OnlyPrePrg = 39,
		eMemOpt_OnlyChipErase = 40,
		eMemOpt_OnlyWrite = 133,
		eMemOpt_Addr = 48,
		eMemOpt_Data = 49,
		eMemOpt_Reset = 50
	}

	public enum E_MEMACC
	{
		eMemAcc_AREA_DATA = 1,
		eMemAcc_AREA_IDATA = 2,
		eMemAcc_AREA_XDATA = 19,
		eMemAcc_AREA_SFR = 4,
		eMemAcc_AREA_CODE = 5
	}

	public enum WORKER_STATE
	{
		WS_IDLE,
		WS_READY,
		WS_DOINGTASK,
		WS_ENDEDTASK,
		WS_DOWAIT,
		WS_WAITRESETTYPE
	}

	public enum MOTOR_STATE
	{
		MS_IDLE,
		MS_INITIAL,
		MS_ISD,
		MS_START,
		MS_RUN,
		MS_BRAKE,
		MS_ALIGN,
		MS_FAULT,
		MS_SLEEP,
		MS_IPD
	}

	public enum FAULT_STATE
	{
		FS_DUZHAN = 1,
		FS_LOSSPHASE = 2,
		FS_UNDERVOL = 4,
		FS_OVERVOL = 8,
		FS_OVERTMEP = 0x10,
		FS_OVERCURRENT = 0x20
	}

	private WORKER_STATE m_WorkerState = WORKER_STATE.WS_IDLE;

	public string m_strDevSN;

	public WORKER_STATE DevWorkState
	{
		get
		{
			return m_WorkerState;
		}
		set
		{
			m_WorkerState = value;
		}
	}
}
