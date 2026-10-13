using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace FT8215Debugger;

internal class DllDebuggerAPI
{
	public enum E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE
	{
		eGPL32_DOWNLOADER_CALLBACK_DEVICE_PLUGGED,
		eGPL32_DOWNLOADER_CALLBACK_DEVICE_UNPLUGGED,
		eGPL32_DOWNLOADER_CALLBACK_BEGIN_TASK,
		eGPL32_DOWNLOADER_CALLBACK_DONE_TASK_STEP,
		eGPL32_DOWNLOADER_CALLBACK_END_TASK,
		eGPL32_DOWNLOADER_CALLBACK_TEXT_CHANGED,
		eGPL32_DOWNLOADER_CALLBACK_WRITE_CODE_FILE,
		eGPL32_DOWNLOADER_CALLBACK_SET_PROGRESS_MAX_POS,
		eGPL32_DOWNLOADER_CALLBACK_SET_PROGRESS_POS,
		eGPL32_DOWNLOADER_CALLBACK_SEND_TRACK_INFO
	}

	public enum E_GPL32_MORTO_STATUS_TYPE
	{
		eFT8213_MOTOR_STATUS_IDLE,
		eFT8213_MOTOR_STATUS_INITIAL,
		eFT8213_MOTOR_STATUS_ISD,
		eFT8213_MOTOR_STATUS_START,
		eFT8213_MOTOR_STATUS_RUN,
		eFT8213_MOTOR_STATUS_BRAKE,
		eFT8213_MOTOR_STATUS_ALIGN,
		eFT8213_MOTOR_STATUS_FAULT,
		eFT8213_MOTOR_STATUS_SLEEP,
		eFT8213_MOTOR_STATUS_IPD
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_PreReg
	{
		public ushort byRegisterNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
		public byte[] byCheckOpt;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
		public uint[] adwRegAdd;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
		public uint[] adwRegVal;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_ByteArrayDef
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] byStatusBuf;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_DevMapInfo
	{
		public bool bDisplayd;

		public ushort wHubIndex;

		public ushort wPortIndex;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
		public char[] tszDevSN;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_StatusParam
	{
		public byte byFlag;

		public ushort wCBCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string ptszDevSN;

		public IntPtr hStatusReportWnd;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_UParam
	{
		public uint dwParam;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string ptszParam;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct S_GPL32DownloaderCallbackParam
	{
		public S_UParam sParam;

		public E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE eParamType;

		public S_StatusParam sStatusParam;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void PFN_GPL32DownloaderCallback(ref S_GPL32DownloaderCallbackParam psParam);

	public const int CBCODE_DEVICEPLUGED = 1;

	public const int CBCODE_DEVICEPULLOUTED = 2;

	public const int CBCODE_BEGINTASK = 3;

	public const int CBCODE_DOTASK = 4;

	public const int CBCODE_ENDTASK = 5;

	public const int CBCODE_STATUSTXTCHANGED = 6;

	public const int CBCODE_BEGINSUBTASK = 8;

	public const int CBCODE_DOSUBTASK = 9;

	public const int CBCODE_WRITECODEFILE = 16;

	public const int CBCODE_RESETTYPE = 17;

	public const int CBCODE_SENDTRACKINFO = 18;

	public const int MAX_EFUSE_NUM = 1024;

	public const int MAX_REG_NUM = 384;

	public const int MAX_VALUE = 250;

	private PFN_GPL32DownloaderCallback pfnStatusCallback = _DeviceStatusCallback;

	public static int _sTotalStepCnt;

	public static int _sCurrentStepCnt;

	public static string _strTextChanged;

	public static uint _dwDevStatus;

	public MainWindow mainWindow { get; set; }

	public DllDebuggerAPI()
	{
		mainWindow = (MainWindow)Application.Current.MainWindow;
	}

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPL32DownloaderInitialize(string ptszConfigFile, string ptszScriptFile, PFN_GPL32DownloaderCallback pfnCallback);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPL32DownloaderEnumerateDevices();

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DLL_GPL32DownloaderGetDeviceCount();

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern int DLL_GPL32DownloaderGetDeviceSerial(int i32DeviceIndex, StringBuilder strBldDevSn);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPL32DownloadPlugInDevice(ushort wHubId, ushort wPortId, string ptszDeviceSN);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPL32DownloadPlugOutDevice(ushort wHubId, ushort wPortId, string ptszDeviceSN);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPL32DownloaderSelectDevice(string ptszDeviceSN);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPL32DownloaderCloseDevice(int i32DeviceIndex);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DLL_GPLB32DownloaderUninit(string ptszDevSN);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DLL_GPL32DownloadUninitUSBAgentLib();

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPDwnLibGetUSBPortInfoByIdx(int i32DevIndex, ref S_DevMapInfo psMapInfo);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool DLL_GPLB32SetOptType(byte byOptType, byte byAccType);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DLL_GPLB32CmdPassThrough(byte byWriteData, ref S_PreReg psPreReg);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DLL_FT8215CmdPassThrough(byte byWriteData, ref S_PreReg psPreReg);

	[DllImport("FTechUsbWrapperDllU.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DLL_GPLB32DownloaderGetNandID(string ptszDeviceSN, uint dwStartAddr, ref S_ByteArrayDef pbyNandID, uint dwDataCnt);

	public static void _DeviceStatusCallback(ref S_GPL32DownloaderCallbackParam pParam)
	{
		uint num = 0u;
		uint eParamType = (uint)pParam.eParamType;
		string ptszDevSN = pParam.sStatusParam.ptszDevSN;
		switch (pParam.eParamType)
		{
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_DEVICE_PLUGGED:
			num = 1u;
			eParamType = 0u;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_DEVICE_UNPLUGGED:
			num = 2u;
			eParamType = 0u;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_BEGIN_TASK:
			num = 3u;
			eParamType = pParam.sParam.dwParam;
			_sTotalStepCnt = (int)eParamType;
			_dwDevStatus = 4u;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_DONE_TASK_STEP:
			num = 4u;
			eParamType = (ushort)pParam.sParam.dwParam;
			_sCurrentStepCnt = (int)eParamType;
			_dwDevStatus = 2u;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_END_TASK:
			num = 5u;
			eParamType = pParam.sParam.dwParam;
			_dwDevStatus = 3u;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_TEXT_CHANGED:
			num = 6u;
			ptszDevSN = pParam.sParam.ptszParam;
			_strTextChanged = ptszDevSN;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_WRITE_CODE_FILE:
			num = 16u;
			ptszDevSN = pParam.sParam.ptszParam;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_SET_PROGRESS_MAX_POS:
			num = 8u;
			eParamType = pParam.sParam.dwParam;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_SET_PROGRESS_POS:
			num = 9u;
			eParamType = pParam.sParam.dwParam;
			break;
		case E_GPL32_DOWNLOADER_CALLBACK_PARAM_TYPE.eGPL32_DOWNLOADER_CALLBACK_SEND_TRACK_INFO:
			num = 18u;
			ptszDevSN = pParam.sParam.ptszParam;
			break;
		}
	}
}
