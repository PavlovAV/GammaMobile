// Вспомогательный класс для поддержки сканирования в терминалах CipherLab
// CP30, CP50, CP60, 9200, 9700 и CP55.
// (c) 2014-2015 Aleksandr Samusenko

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace Scancode
{
	/// <summary>
	/// Делегат события "сосканирован штрихкод"
	/// </summary>
	public delegate void ReaderEventHandler(ReaderEventArgs e);

	class BarcodeReaderHelper:Microsoft.WindowsCE.Forms.MessageWindow
	{
		static bool ms_isInitDone=false;
		static bool ms_isReaderInitialized;
		static bool ms_isReaderActive;
		static IBarcodeProcessor ms_formForProcessBarcode;
		static uint ms_uDecodeMsg;
		static IntPtr ms_evtThreadNeedExit;
		static IntPtr ms_evtThreadIsExited;
		static IntPtr ms_evtBarcode;
		static Thread ms_threadWaitBarcode;
		static BarcodeReaderHelper ms_helperInstance; // "как бы синглтон", НО ЭТО НЕ СИНГЛТОН!
		static TerminalModel ms_terminalModel;
		static TerminalPlatform ms_terminalPlatform;
		static ReaderType ms_readerType;

        delegate void OnBarcode(string text,int t);
		
		public event ReaderEventHandler OnScan;

		public BarcodeReaderHelper()
		{
			initIfNeed();
			ms_helperInstance=this;
		}

		static void initIfNeed()
		{
			if(ms_isInitDone) {
				return;
			}

			ms_isReaderInitialized=false;
			ms_isReaderActive=true;
			ms_formForProcessBarcode=null;
			ms_uDecodeMsg=0;
			ms_evtThreadNeedExit=IntPtr.Zero;
			ms_evtThreadIsExited=IntPtr.Zero;
			ms_evtBarcode=IntPtr.Zero;
			ms_threadWaitBarcode=null;
			ms_helperInstance=null;
			
			ms_terminalModel=TerminalID.Model;
			ms_terminalPlatform=TerminalID.Platform;
			
			if (ms_terminalModel == TerminalModel.CL_CP55 || ms_terminalModel == TerminalModel.CL_CP60 || ms_terminalModel == TerminalModel.CL_9700)
            {
                ms_uDecodeMsg = Win32API.WM_USER + 20; //WM_USER + 20
            }
            else
            {
				ms_uDecodeMsg=Win32API.RegisterWindowMessage("WM_DECODEDATA");
			}   

			//if(
			//	ms_terminalModel==TerminalModel.CL_CP30
			//	|| ms_terminalModel==TerminalModel.CL_CP50
			//	|| ms_terminalModel==TerminalModel.CL_9200
			//	) {
			//	ms_uDecodeMsg=Win32API.RegisterWindowMessage("WM_DECODEDATA");
			//}

			ms_readerType=ReaderType.NONE;
			ms_isInitDone=true;
		}

		protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message msg)
		{
			if(ms_uDecodeMsg!=0 &&
				msg.Msg==ms_uDecodeMsg &&
				msg.WParam.ToInt32()==7 // 7 - DC_READER_BC
				) {
				int barcodeTypeID;
				string barcode;

				ReaderNative.GetDecodeDataAndType(ms_terminalPlatform,out barcode,out barcodeTypeID);
				if(barcode.Length>0) {
					if(ms_formForProcessBarcode!=null) {
						ms_formForProcessBarcode.OnScan(barcode,barcodeTypeID);
					}
					if(ms_helperInstance!=null && ms_helperInstance.OnScan!=null) {
						ms_helperInstance.OnScan(new ReaderEventArgs(barcode,barcodeTypeID));
					}
				}


			}
			base.WndProc(ref msg);
		}

        static void ReadBarcodeThread()
        {
			IntPtr[] evts=new IntPtr[] {ms_evtThreadNeedExit,ms_evtBarcode};
			int waitRes=Win32API.WAIT_TIMEOUT;

			while(waitRes==Win32API.WAIT_TIMEOUT) {
				waitRes=Win32API.WaitForMultipleObjects(2,evts,0,Timeout.Infinite);
				switch(waitRes) {
				case 0:
					// ничего не делаем
					break;
				case 1:
					int barcodeTypeID;
					string barcode;
					ReaderNative.GetDecodeDataAndType(ms_terminalPlatform,out barcode,out barcodeTypeID);
					if(barcode.Length>0) {
						if(ms_formForProcessBarcode!=null) {
							OnBarcode d = new OnBarcode(ms_formForProcessBarcode.OnScan);
							((System.Windows.Forms.Control)ms_formForProcessBarcode).
								Invoke(
									d,
									new object[] { barcode,barcodeTypeID }
								);
						}
						if(ms_helperInstance!=null && ms_helperInstance.OnScan!=null) {
							ms_helperInstance.OnScan(new ReaderEventArgs(barcode,barcodeTypeID));
						}
					}
					waitRes=Win32API.WAIT_TIMEOUT;
					break;
				default:
					waitRes=Win32API.WAIT_TIMEOUT;
					break;
				}
			}

			Win32API.SetEvent(ms_evtThreadIsExited);
        }

		/// <summary>
		/// Запуск сканера
		/// </summary>
		/// <returns></returns>
		public static int ReaderInit()
		{
			initIfNeed();
			if(ms_isReaderInitialized) {
				return(0);
			}

			int res=0;

			res=ReaderNative.InitReader(ms_terminalPlatform);
			if(ms_terminalModel==TerminalModel.CL_CP30) {
				if(res==1) {
					res=0;
				} else {
					res=1;
				}
			}

			if(res==0) {
				if(
					ms_terminalModel==TerminalModel.CL_CP30
					|| ms_terminalModel==TerminalModel.CL_CP50
					|| ms_terminalModel==TerminalModel.CL_9200
					) {
					ms_readerType=(ReaderType)ReaderNative.GetReaderType(ms_terminalPlatform);
				} else if(
					ms_terminalModel==TerminalModel.CL_CP60
					|| ms_terminalModel==TerminalModel.CL_9700
					|| ms_terminalModel==TerminalModel.CL_CP55
					) {
					switch(ReaderNative.GetBcReaderType(ms_terminalPlatform)) {
					case 0: // None
						ms_readerType=ReaderType.NONE;
						break;
					case 1: // Moto_SE955-E
						ms_readerType=ReaderType.ID_MOD_1D_955;
						break;
					case 2: // Moto_SE955-I
						ms_readerType=ReaderType.ID_MOD_1D_955;
						break;
					case 3: // Moto_SE4500
						ms_readerType=ReaderType.ID_MOD_2D_4500;
						break;
					case 4: // Moto_SE965-E
						ms_readerType=ReaderType.ID_MOD_1D_955; //?
						break;
					case 5: // Moto_SE4507
						ms_readerType=ReaderType.ID_MOD_2D_4500; //?
						break;
					case 7: // Intermec_EX25
						ms_readerType=ReaderType.ID_MOD_2D_4500; //?
						break;
					case 8: // Moto_SE1524
						ms_readerType=ReaderType.ID_MOD_1D_ELR; //?
						break;
					case 9: // SM1
						ms_readerType=ReaderType.ID_MOD_1D_SM1;
						break;
					default:
						ms_readerType=ReaderType.NONE;
						break;
					};
				}
                //ПАВ Скидывает настройки в По умолчанию
				//ReaderNative.ResetReaderToDefault(ms_terminalPlatform,ms_terminalModel);
				ReaderDisableKeyboardEmulation();

				if(ms_terminalModel==TerminalModel.CL_CP60 || ms_terminalModel==TerminalModel.CL_9700 || ms_terminalModel==TerminalModel.CL_CP55) {
					ms_evtThreadNeedExit=Win32API.CreateEvent(0,0,0,null);
					ms_evtThreadIsExited=Win32API.CreateEvent(0,0,0,null);
					ReaderNative.GetReaderHandle(ms_terminalPlatform,ref ms_evtBarcode);
					Win32API.WaitForSingleObject(ms_evtBarcode,0);
					Win32API.ResetEvent(ms_evtBarcode);
					ms_threadWaitBarcode=new Thread(new ThreadStart(ReadBarcodeThread));
					ms_threadWaitBarcode.Start();
				}
				ms_isReaderInitialized=true;
			}
			return(res);
		}

		/// <summary>
		/// Остановка сканера
		/// </summary>
		public static void ReaderUninit()
		{
			initIfNeed();

			if(ms_terminalModel==TerminalModel.CL_CP60 || ms_terminalModel==TerminalModel.CL_9700 || ms_terminalModel==TerminalModel.CL_CP55) {
				Win32API.SetEvent(ms_evtThreadNeedExit);
				Win32API.WaitForSingleObject(ms_evtThreadIsExited,Timeout.Infinite);
				Win32API.CloseHandle(ms_evtThreadNeedExit);
				Win32API.CloseHandle(ms_evtThreadIsExited);
				ms_threadWaitBarcode=null;
				ReaderNative.FreeReaderHandle(ms_terminalPlatform,ref ms_evtBarcode);
			}
            //ПАВ ReaderActivate(false);
			if(ms_terminalModel==TerminalModel.CL_CP60 || ms_terminalModel==TerminalModel.CL_9700 || ms_terminalModel==TerminalModel.CL_CP55) {
				ReaderNative.DeInitReader(ms_terminalPlatform);
			}
			if(ms_terminalModel==TerminalModel.CL_CP30 || ms_terminalModel==TerminalModel.CL_CP50 || ms_terminalModel==TerminalModel.CL_9200) {
				if(ms_helperInstance!=null) {
					ms_helperInstance.Dispose();
					ms_helperInstance=null;
				}
			}
			ms_isReaderInitialized=false;
		}

		public static void ReaderDisableKeyboardEmulation()
		{
			int rw;
			int enableKeyboardEmulation=0;
			int autoEnterWay=0;
			int autoEnterChar=0;
			int showCodeType=0;
			int showCodeLen=0;
			byte[] prefixCode=new byte[256];
			byte[] suffixCode=new byte[256];

			initIfNeed();
			rw=(int)'r';
			ReaderNative.DataOutputSettings(
				ms_terminalPlatform,
				rw,
				ref enableKeyboardEmulation,
				ref autoEnterWay,
				ref autoEnterChar,
				ref showCodeType,
				ref showCodeLen,
				prefixCode,
				prefixCode.Length,
				suffixCode,
				suffixCode.Length
				);
			rw=(int)'w';
			enableKeyboardEmulation=0;
			autoEnterWay=0;
			autoEnterChar=0;
			ReaderNative.DataOutputSettings(
				ms_terminalPlatform,
				rw,
				ref enableKeyboardEmulation,
				ref autoEnterWay,
				ref autoEnterChar,
				ref showCodeType,
				ref showCodeLen,
				prefixCode,
				0,//prefixCode.Length,
				suffixCode,
				0//suffixCode.Length
				);
		}

		/// <summary>
		/// Активация/деактивация сканера
		/// </summary>
		/// <param name="isActive">true - включен, false - выключен</param>
		public static void ReaderActivate(bool isActive)
		{
			initIfNeed();

			if(ms_isReaderActive==isActive) {
				return;
			}

			if(!ms_isReaderInitialized) {
				return;
			}

			if(ms_terminalModel==TerminalModel.CL_CP30 || ms_terminalModel==TerminalModel.CL_CP50 || ms_terminalModel==TerminalModel.CL_9200) {
				ReaderNative.SetActiveDevice(ms_terminalPlatform,isActive?ReaderNative.DC_READER_BC:ReaderNative.NO_ACTIVE_DEVICE);
			}
			if(ms_terminalModel==TerminalModel.CL_CP60 || ms_terminalModel==TerminalModel.CL_9700 || ms_terminalModel==TerminalModel.CL_CP55) {
				ReaderNative.SetActiveBcReader(ms_terminalPlatform,isActive?1:0);
			}
			if(isActive) {
				// (для новых прошивок 9200: нужно сбрасывать после активации, возможно потом исправят)
				//ReaderNative.ResetReaderToDefault(ms_terminalPlatform,ms_terminalModel);
				//ReaderDisableKeyboardEmulation(); // (после сброса опять надо запретить эмуляцию клавиатуры)
			}
			ms_isReaderActive=isActive;
		}

		/// <summary>
		/// Активность сканера (выключен/включен)
		/// </summary>
		public static bool IsReaderActive
		{
			get
			{
				initIfNeed();
				return(ms_isReaderActive);
			}
			set
			{
				initIfNeed();
				ReaderActivate(value);
			}
		}

		/// <summary>
		/// Форма, которая обрабатывает отсканированный штрихкод
		/// </summary>
		public static IBarcodeProcessor FormForProcessBarcode
		{
			get
			{
				initIfNeed();
				return(ms_formForProcessBarcode);
			}
			set
			{
				initIfNeed();
				ms_formForProcessBarcode=value;
			}
		}

		/// <summary>
		/// Получение идентификатора модели терминала
		/// </summary>
		public static TerminalModel ModelOfTerminal
		{
			get
			{
				initIfNeed();
				return(ms_terminalModel);
			}
		}

		public static ReaderType ReaderType
		{
			get
			{
				initIfNeed();
				return(ms_readerType);
			}
		}

		public static int ReadBarcodeData(out string barcode,out int barcodeTypeID,int timeoutSec)
		{
			initIfNeed();
			if(!ms_isReaderInitialized) {
				barcode=string.Empty;
				barcodeTypeID=0;
				return(-1);
			}
			return(ReaderNative.ReadBarcodeData(ms_terminalPlatform,out barcode,out barcodeTypeID,timeoutSec));
		}

		public static void SimbologyEAN8(bool isenabled)
		{
			initIfNeed();
			ReaderNative.EanJanEnable(ms_terminalPlatform,ms_terminalModel,ms_readerType,isenabled,null,null,null,null,null,null,null,null,null);
		}

		public static void SimbologyEAN13(bool isenabled)
		{
			initIfNeed();
			ReaderNative.EanJanEnable(ms_terminalPlatform,ms_terminalModel,ms_readerType,null,isenabled,null,null,null,null,null,null,null,null);
		}

		public static void SimbologyPDF417(bool isenabled)
		{
			initIfNeed();
			ReaderNative.Symbologies2DEnable(ms_terminalPlatform,ms_terminalModel,ms_readerType,isenabled,null,null,null,null,null,null,null,null,null,null,null);
		}

		public static void SimbologyDataMatrix(bool isenabled)
		{
			initIfNeed();
			int intisenabled=isenabled?2:0;
			ReaderNative.Symbologies2DEnable(ms_terminalPlatform,ms_terminalModel,ms_readerType,null,null,null,isenabled,intisenabled,null,null,null,null,null,null,null);
		}

		public static void SimbologyQRCode(bool isenabled)
		{
			initIfNeed();
			int intisenabled=isenabled?2:0;
			ReaderNative.Symbologies2DEnable(ms_terminalPlatform,ms_terminalModel,ms_readerType,null,null,null,null,null,null,null,isenabled,intisenabled,null,null,null);
		}

		public static void BeeperByIndex(int idxsound)
		{
			ReaderNative.BeeperByIndex(ms_terminalPlatform,idxsound);
		}

		public static int GetSuccessSoundIndex()
		{
			int goodRead=0;
			int enableVibrator=0;
			int vibrationTime=0;
			int ledDuration=0;

			if(ReaderNative.NotificationSettings(ms_terminalPlatform,(int)'r',ref goodRead,ref enableVibrator,ref vibrationTime,ref ledDuration)!=0) {
				return(-2);
			}

			return(goodRead);
		}
	}

	/// <summary>
	/// Native reader.dll functions
	/// </summary>
	internal class ReaderNativeCE
	{
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int DataOutputSettings(
			int rw,
			ref int enableKeyboardEmulation,
			ref int autoEnterWay,
			ref int autoEnterChar,
			ref int showCodeType,
			ref int showCodeLen,
			byte[] prefixCode1,
			int prefixCodeLen,
			byte[] suffixCode1,
			int suffixCodeLen
			);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int GetReaderHandle(ref IntPtr handle);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int FreeReaderHandle(ref IntPtr handle);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int InitReader();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern void DeInitReader();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int SetActiveBcReader(int isEnabled);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int SetActiveDevice(int actDC);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE", EntryPoint="ResetReaderToDefault")]
		internal static extern int ResetReaderToDefaultCP30CP509200(int readerType);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE", EntryPoint="ResetReaderToDefault")]
		internal static extern int ResetReaderToDefaultVoid();
		
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int GetDecodeType();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int GetDecodeData(byte[] lpBuf,int nBufSize);

		// (этой функции нет в CP60!!!)
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int ReadBarcodeData(
			ref int codeType,
			byte[] buf,
			int bufSize,
			int timeout0
			);

		// (этой функции нет в CP60!!!)
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE", EntryPoint="Beeper")]
		internal static extern int BeeperByIndex(int mode,int path0);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int NotificationSettings(
			int rw,
			ref int goodRead,
			ref int enableVibrator,
			ref int vibrationTime,
			ref int ledDuration
			);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int GetBcReaderType();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int GetReaderType();

		// CP30, CP50, 9200
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE", EntryPoint="EanJan_1D_SE955")]
		internal static extern int EanJan_1D_SE955_9(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode,
			ref int upcEanSecurityLevel
			);

		// CP60, 9700, CP55
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE", EntryPoint="EanJan_1D_SE955")]
		internal static extern int EanJan_1D_SE955_10(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode,
			ref int upcEanSecurityLevel,
			ref int booklandIsbnFormat
			);

		// CP55, 9200:
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int EanJan_1D_SM1(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int UPC_EAN_JAN_decodeSupplementals,
			ref int UPC_EAN_JAN_supplementalsRedundancy,
			ref int EAN8_JAN8_extend
			);

		// CP30, CP50
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int EanJan_2D_SE4507(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode
			);

		// CP60, 9700, CP55, 9200
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int EanJan_2D_SE4500(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode,
			ref int booklandIsbnFormat,
			ref int enableIssnEan
			);

		// CP30, CP50
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int Symbologies_2D_4507(
			int rw,
			ref int enablePDF417,
			ref int enableMicroPDF417,
			ref int enableCode128Emulation,
			ref int enableDataMatrix,
			ref int enableDataMatrixInverse,
			ref int decodeMirrorImage,
			ref int enableMaxicode,
			ref int enableQRCode,
			ref int enableQRCodeInverse,
			ref int enableMicroQR,
			ref int enableAztec,
			ref int enableAztecInverse
			);

		// CP60, 9700, CP55, 9200
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDll_CE")]
		internal static extern int Symbologies_2D_SE4500(
			int rw,
			ref int enablePDF417,
			ref int enableMicroPDF417,
			ref int enableCode128Emulation,
			ref int enableDataMatrix,
			ref int enableDataMatrixInverse,
			ref int decodeMirrorImage,
			ref int enableMaxicode,
			ref int enableQRCode,
			ref int enableQRCodeInverse,
			ref int enableMicroQR,
			ref int enableAztec,
			ref int enableAztecInverse
			);

	}

	/// <summary>
	/// Native reader.dll functions
	/// </summary>
	internal class ReaderNativeMobile
	{
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int DataOutputSettings(
			int rw,
			ref int enableKeyboardEmulation,
			ref int autoEnterWay,
			ref int autoEnterChar,
			ref int showCodeType,
			ref int showCodeLen,
			byte[] prefixCode1,
			int prefixCodeLen,
			byte[] suffixCode1,
			int suffixCodeLen
			);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int GetReaderHandle(ref IntPtr handle);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int FreeReaderHandle(ref IntPtr handle);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int InitReader();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern void DeInitReader();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int SetActiveBcReader(int isEnabled);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int SetActiveDevice(int actDC);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile", EntryPoint="ResetReaderToDefault")]
		internal static extern int ResetReaderToDefaultCP30CP509200(int readerType);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile", EntryPoint="ResetReaderToDefault")]
		internal static extern int ResetReaderToDefaultVoid();
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int GetDecodeType();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int GetDecodeData(byte[] lpBuf,int nBufSize);

		// (этой функции нет в CP60!!!)
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int ReadBarcodeData(
			ref int codeType,
			byte[] buf,
			int bufSize,
			int timeout0
			);

		// (этой функции нет в CP60!!!)
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile", EntryPoint="Beeper")]
		internal static extern int BeeperByIndex(int mode,int path0);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int NotificationSettings(
			int rw,
			ref int goodRead,
			ref int enableVibrator,
			ref int vibrationTime,
			ref int ledDuration
			);

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int GetBcReaderType();

		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int GetReaderType();

		// CP30, CP50, 9200
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile", EntryPoint="EanJan_1D_SE955")]
		internal static extern int EanJan_1D_SE955_9(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode,
			ref int upcEanSecurityLevel
			);

		// CP60, 9700, CP55
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile", EntryPoint="EanJan_1D_SE955")]
		internal static extern int EanJan_1D_SE955_10(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode,
			ref int upcEanSecurityLevel,
			ref int booklandIsbnFormat
			);

		// CP55, 9200:
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int EanJan_1D_SM1(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int UPC_EAN_JAN_decodeSupplementals,
			ref int UPC_EAN_JAN_supplementalsRedundancy,
			ref int EAN8_JAN8_extend
			);

		// CP30, CP50
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int EanJan_2D_SE4507(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode
			);

		// CP60, 9700, CP55, 9200
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int EanJan_2D_SE4500(
			int rw,
			ref int enableEAN8_JAN8,
			ref int enableEAN13_JAN13,
			ref int enableBooklandEAN,
			ref int enableAddons,
			ref int addonsRedundancy, 
			ref int enableEanJan8Extended,
			ref int uccCouponExtendedCode,
			ref int booklandIsbnFormat,
			ref int enableIssnEan
			);

		// CP30, CP50
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int Symbologies_2D_4507(
			int rw,
			ref int enablePDF417,
			ref int enableMicroPDF417,
			ref int enableCode128Emulation,
			ref int enableDataMatrix,
			ref int enableDataMatrixInverse,
			ref int decodeMirrorImage,
			ref int enableMaxicode,
			ref int enableQRCode,
			ref int enableQRCodeInverse,
			ref int enableMicroQR,
			ref int enableAztec,
			ref int enableAztecInverse
			);

		// CP60, 9700, CP55, 9200
		/// <summary>
		/// Native reader.dll function
		/// </summary>
		[DllImport("ReaderDllMobile")]
		internal static extern int Symbologies_2D_SE4500(
			int rw,
			ref int enablePDF417,
			ref int enableMicroPDF417,
			ref int enableCode128Emulation,
			ref int enableDataMatrix,
			ref int enableDataMatrixInverse,
			ref int decodeMirrorImage,
			ref int enableMaxicode,
			ref int enableQRCode,
			ref int enableQRCodeInverse,
			ref int enableMicroQR,
			ref int enableAztec,
			ref int enableAztecInverse
			);
	}

	/// <summary>
	/// Native reader.dll functions
	/// </summary>
	public class ReaderNative
	{
		/// <summary>
		/// Native reader.dll constant
		/// </summary>
		internal const int NO_ACTIVE_DEVICE=0;

		/// <summary>
		/// Native reader.dll constant
		/// </summary>
		internal const int DC_READER_BC=7;

		/// <summary>
		/// Native reader.dll constant
		/// </summary>
		internal const int ALL_DEVICE=255;

		public static int DataOutputSettings(TerminalPlatform terminalPlatform,int rw,ref int enableKeyboardEmulation,ref int autoEnterWay,ref int autoEnterChar,ref int showCodeType,ref int showCodeLen,byte[] prefixCode1,int prefixCodeLen,byte[] suffixCode1,int suffixCodeLen)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.DataOutputSettings(rw,ref enableKeyboardEmulation,ref autoEnterWay,ref autoEnterChar,ref showCodeType,ref showCodeLen,prefixCode1,prefixCodeLen,suffixCode1,suffixCodeLen));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.DataOutputSettings(rw,ref enableKeyboardEmulation,ref autoEnterWay,ref autoEnterChar,ref showCodeType,ref showCodeLen,prefixCode1,prefixCodeLen,suffixCode1,suffixCodeLen));
			}
			return(0);
		}

		public static int GetReaderHandle(TerminalPlatform terminalPlatform,ref IntPtr handle)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.GetReaderHandle(ref handle));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.GetReaderHandle(ref handle));
			}
			return(0);
		}

		public static int FreeReaderHandle(TerminalPlatform terminalPlatform,ref IntPtr handle)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.FreeReaderHandle(ref handle));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.FreeReaderHandle(ref handle));
			}
			return(0);
		}

		public static int InitReader(TerminalPlatform terminalPlatform)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.InitReader());
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.InitReader());
			}
			return(0);
		}

		public static void DeInitReader(TerminalPlatform terminalPlatform)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				ReaderNativeCE.DeInitReader();
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				ReaderNativeMobile.DeInitReader();
			}
		}

		public static int SetActiveBcReader(TerminalPlatform terminalPlatform,int isEnabled)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.SetActiveBcReader(isEnabled));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.SetActiveBcReader(isEnabled));
			}
			return(0);
		}

		public static int SetActiveDevice(TerminalPlatform terminalPlatform,int actDC)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.SetActiveDevice(actDC));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.SetActiveDevice(actDC));
			}
			return(0);
		}

		public static int ResetReaderToDefault(TerminalPlatform terminalPlatform,TerminalModel terminalModel)
		{
			int res=0;

			if(terminalModel==TerminalModel.CL_CP30 || terminalModel==TerminalModel.CL_CP50 || terminalModel==TerminalModel.CL_9200) {
				if(terminalPlatform==TerminalPlatform.WindowsCE) {
					res=ReaderNativeCE.ResetReaderToDefaultCP30CP509200(ReaderNative.ALL_DEVICE);
				} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
					res=ReaderNativeMobile.ResetReaderToDefaultCP30CP509200(ReaderNative.ALL_DEVICE);
				}
			}
			if(terminalModel==TerminalModel.CL_CP60 || terminalModel==TerminalModel.CL_9700 || terminalModel==TerminalModel.CL_CP55) {
				if(terminalPlatform==TerminalPlatform.WindowsCE) {
					res=ReaderNativeCE.ResetReaderToDefaultVoid();
				} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
					res=ReaderNativeMobile.ResetReaderToDefaultVoid();
				}
			}
			return(res);
		}


		private static string barcodeFromBytes(byte[] buf,int buflen)
		{
			string barcode=string.Empty;

			if(buflen>0) {
				char[] pc=new char[buflen];
				for(int i=0;i<buflen;i++) {
					pc[i]=(char)buf[i];
				}
				barcode=new string(pc);
			}
			return(barcode);
		}

		/// <summary>
		/// Вызов GetDecodeData и GetDecodeType
		/// </summary>
		/// <param name="terminalPlatform">Идентификатор платформы (WindowsCE или WindowsMobile)</param>
		/// <param name="barcode">Строка, в которой будет отсканированный штрихкод</param>
		/// <param name="barcodeTypeID">Тип штрихкода (см. возвращаемое значение GetDecodeType)</param>
		/// <returns>См. возвращаемое значение GetDecodeData</returns>
		public static int GetDecodeDataAndType(TerminalPlatform terminalPlatform,out string barcode,out int barcodeTypeID)
		{
			byte[] buf=new byte[2048];
			int res=0;
			
			barcodeTypeID=0;
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				barcodeTypeID=ReaderNativeCE.GetDecodeType();
				res=ReaderNativeCE.GetDecodeData(buf,buf.Length);
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				barcodeTypeID=ReaderNativeMobile.GetDecodeType();
				res=ReaderNativeMobile.GetDecodeData(buf,buf.Length);
			}
			barcode=barcodeFromBytes(buf,res);
			return(res);
		}

		public static int ReadBarcodeData(TerminalPlatform terminalPlatform,out string barcode,out int barcodeTypeID,int timeoutSec)
		{
			byte[] buf=new byte[2048];
			int res=0;
			
			barcodeTypeID=0;
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				res=ReaderNativeCE.ReadBarcodeData(ref barcodeTypeID,buf,buf.Length,timeoutSec);
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				res=ReaderNativeMobile.ReadBarcodeData(ref barcodeTypeID,buf,buf.Length,timeoutSec);
			}
			barcode=barcodeFromBytes(buf,res);
			return(res);
		}

		public static int BeeperByIndex(TerminalPlatform terminalPlatform,int idx)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.BeeperByIndex(idx,0));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.BeeperByIndex(idx,0));
			}
			return(0);
		}

		public static int NotificationSettings(TerminalPlatform terminalPlatform,int rw,ref int goodRead,ref int enableVibrator,ref int vibrationTime,ref int ledDuration)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.NotificationSettings(rw,ref goodRead,ref enableVibrator,ref vibrationTime,ref ledDuration));
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.NotificationSettings(rw,ref goodRead,ref enableVibrator,ref vibrationTime,ref ledDuration));
			}
			return(0);
		}

		/// <summary>
		/// Вызов GetBcReaderType
		/// </summary>
		/// <param name="terminalPlatform">Идентификатор платформы (WindowsCE или WindowsMobile)</param>
		/// <returns>См. возвращаемое значение GetBcReaderType</returns>
		public static int GetBcReaderType(TerminalPlatform terminalPlatform)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.GetBcReaderType());
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.GetBcReaderType());
			}
			return(0);
			// CP60: 1,2,3,4
			// 9700: 1,3,5,7,8
			// CP55: 1,3,4,9
		}

		/// <summary>
		/// Вызов GetReaderType
		/// </summary>
		/// <param name="terminalPlatform">Идентификатор платформы (WindowsCE или WindowsMobile)</param>
		/// <returns>См. возвращаемое значение GetReaderType</returns>
		public static int GetReaderType(TerminalPlatform terminalPlatform)
		{
			if(terminalPlatform==TerminalPlatform.WindowsCE) {
				return(ReaderNativeCE.GetReaderType());
			} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
				return(ReaderNativeMobile.GetReaderType());
			}
			// CP30, CP50: 64,128
			// 9200: 128 ,512, 1024
			return(0);
		}

		public static void EanJanEnable(
			TerminalPlatform terminalPlatform,
			TerminalModel terminalModel,
			ReaderType readerType,
			bool? enableEAN8_JAN8,
			bool? enableEAN13_JAN13,
			bool? enableBooklandEAN,
			int? enableAddons,
			int? addonsRedundancy, 
			bool? enableEanJan8Extended,
			bool? uccCouponExtendedCode,
			int? upcEanSecurityLevel,
			int? booklandIsbnFormat,
			bool? enableIssnEan
			)
		{
			int rw=0;
			int venableEAN8_JAN8=0;
			int venableEAN13_JAN13=0;
			int venableBooklandEAN=0;
			int venableAddons=0;
			int vaddonsRedundancy=0;
			int venableEanJan8Extended=0;
			int vuccCouponExtendedCode=0;
			int vupcEanSecurityLevel=0;
			int vbooklandIsbnFormat=0;
			int venableIssnEan=0;

			if(readerType==ReaderType.ID_MOD_1D_955) {
				if(
					terminalModel==TerminalModel.CL_CP30
					|| terminalModel==TerminalModel.CL_CP50
					|| terminalModel==TerminalModel.CL_9200
					) {
					rw=(int)'r';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_1D_SE955_9(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_1D_SE955_9(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel
							);
					}
					if(enableEAN8_JAN8.HasValue) {
						venableEAN8_JAN8=(bool)enableEAN8_JAN8?1:0;
					}
					if(enableEAN13_JAN13.HasValue) {
						venableEAN13_JAN13=(bool)enableEAN13_JAN13?1:0;
					}
					if(enableBooklandEAN.HasValue) {
						venableBooklandEAN=(bool)enableBooklandEAN?1:0;
					}
					if(enableAddons.HasValue) {
						venableAddons=(int)enableAddons;
					}
					if(addonsRedundancy.HasValue) {
						vaddonsRedundancy=(int)addonsRedundancy;
					}
					if(enableEanJan8Extended.HasValue) {
						venableEanJan8Extended=(bool)enableEanJan8Extended?1:0;
					}
					if(uccCouponExtendedCode.HasValue) {
						vuccCouponExtendedCode=(bool)uccCouponExtendedCode?1:0;
					}
					if(upcEanSecurityLevel.HasValue) {
						vupcEanSecurityLevel=(int)upcEanSecurityLevel;
					}
					rw=(int)'w';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_1D_SE955_9(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_1D_SE955_9(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel
							);
					}
				} else if(
					terminalModel==TerminalModel.CL_CP60
					|| terminalModel==TerminalModel.CL_9700
					|| terminalModel==TerminalModel.CL_CP55
					) {
					rw=(int)'r';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_1D_SE955_10(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel,
							ref vbooklandIsbnFormat
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_1D_SE955_10(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel,
							ref vbooklandIsbnFormat
							);
					}
					if(enableEAN8_JAN8.HasValue) {
						venableEAN8_JAN8=(bool)enableEAN8_JAN8?1:0;
					}
					if(enableEAN13_JAN13.HasValue) {
						venableEAN13_JAN13=(bool)enableEAN13_JAN13?1:0;
					}
					if(enableBooklandEAN.HasValue) {
						venableBooklandEAN=(bool)enableBooklandEAN?1:0;
					}
					if(enableAddons.HasValue) {
						venableAddons=(int)enableAddons;
					}
					if(addonsRedundancy.HasValue) {
						vaddonsRedundancy=(int)addonsRedundancy;
					}
					if(enableEanJan8Extended.HasValue) {
						venableEanJan8Extended=(bool)enableEanJan8Extended?1:0;
					}
					if(uccCouponExtendedCode.HasValue) {
						vuccCouponExtendedCode=(bool)uccCouponExtendedCode?1:0;
					}
					if(upcEanSecurityLevel.HasValue) {
						vupcEanSecurityLevel=(int)upcEanSecurityLevel;
					}
					if(booklandIsbnFormat.HasValue) {
						vbooklandIsbnFormat=(int)booklandIsbnFormat;
					}
					rw=(int)'w';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_1D_SE955_10(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel,
							ref vbooklandIsbnFormat
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_1D_SE955_10(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vupcEanSecurityLevel,
							ref vbooklandIsbnFormat
							);
					}
				}
			} else if(readerType==ReaderType.ID_MOD_1D_SM1) {
				if(
					terminalModel==TerminalModel.CL_CP55
					|| terminalModel==TerminalModel.CL_9200
					) {
					rw=(int)'r';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_1D_SM1(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_1D_SM1(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended
							);
					}
					if(enableEAN8_JAN8.HasValue) {
						venableEAN8_JAN8=(bool)enableEAN8_JAN8?1:0;
					}
					if(enableEAN13_JAN13.HasValue) {
						venableEAN13_JAN13=(bool)enableEAN13_JAN13?1:0;
					}
					if(enableBooklandEAN.HasValue) {
						venableBooklandEAN=(bool)enableBooklandEAN?1:0;
					}
					if(enableAddons.HasValue) {
						venableAddons=(int)enableAddons;
					}
					if(addonsRedundancy.HasValue) {
						vaddonsRedundancy=(int)addonsRedundancy;
					}
					if(enableEanJan8Extended.HasValue) {
						venableEanJan8Extended=(bool)enableEanJan8Extended?1:0;
					}
					rw=(int)'w';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_1D_SM1(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_1D_SM1(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended
							);
					}
				}
			} else if(readerType==ReaderType.ID_MOD_2D_4507) {
				if(
					terminalModel==TerminalModel.CL_CP30
					|| terminalModel==TerminalModel.CL_CP50
					) {
					rw=(int)'r';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_2D_SE4507(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_2D_SE4507(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode
							);
					}
					if(enableEAN8_JAN8.HasValue) {
						venableEAN8_JAN8=(bool)enableEAN8_JAN8?1:0;
					}
					if(enableEAN13_JAN13.HasValue) {
						venableEAN13_JAN13=(bool)enableEAN13_JAN13?1:0;
					}
					if(enableBooklandEAN.HasValue) {
						venableBooklandEAN=(bool)enableBooklandEAN?1:0;
					}
					if(enableAddons.HasValue) {
						venableAddons=(int)enableAddons;
					}
					if(addonsRedundancy.HasValue) {
						vaddonsRedundancy=(int)addonsRedundancy;
					}
					if(enableEanJan8Extended.HasValue) {
						venableEanJan8Extended=(bool)enableEanJan8Extended?1:0;
					}
					if(uccCouponExtendedCode.HasValue) {
						vuccCouponExtendedCode=(bool)uccCouponExtendedCode?1:0;
					}
					rw=(int)'w';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_2D_SE4507(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_2D_SE4507(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode
							);
					}
				}
			} else if(readerType==ReaderType.ID_MOD_2D_4500) {
				if(
					terminalModel==TerminalModel.CL_CP60
					|| terminalModel==TerminalModel.CL_9700
					|| terminalModel==TerminalModel.CL_CP55
					|| terminalModel==TerminalModel.CL_9200
					) {
					rw=(int)'r';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_2D_SE4500(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vbooklandIsbnFormat,
							ref venableIssnEan
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_2D_SE4500(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vbooklandIsbnFormat,
							ref venableIssnEan
							);
					}
					if(enableEAN8_JAN8.HasValue) {
						venableEAN8_JAN8=(bool)enableEAN8_JAN8?1:0;
					}
					if(enableEAN13_JAN13.HasValue) {
						venableEAN13_JAN13=(bool)enableEAN13_JAN13?1:0;
					}
					if(enableBooklandEAN.HasValue) {
						venableBooklandEAN=(bool)enableBooklandEAN?1:0;
					}
					if(enableAddons.HasValue) {
						venableAddons=(int)enableAddons;
					}
					if(addonsRedundancy.HasValue) {
						vaddonsRedundancy=(int)addonsRedundancy;
					}
					if(enableEanJan8Extended.HasValue) {
						venableEanJan8Extended=(bool)enableEanJan8Extended?1:0;
					}
					if(uccCouponExtendedCode.HasValue) {
						vuccCouponExtendedCode=(bool)uccCouponExtendedCode?1:0;
					}
					if(booklandIsbnFormat.HasValue) {
						vbooklandIsbnFormat=(int)booklandIsbnFormat;
					}
					if(enableIssnEan.HasValue) {
						venableIssnEan=(bool)enableIssnEan?1:0;
					}
					rw=(int)'w';
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.EanJan_2D_SE4500(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vbooklandIsbnFormat,
							ref venableIssnEan
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.EanJan_2D_SE4500(
							rw,
							ref venableEAN8_JAN8,
							ref venableEAN13_JAN13,
							ref venableBooklandEAN,
							ref venableAddons,
							ref vaddonsRedundancy,
							ref venableEanJan8Extended,
							ref vuccCouponExtendedCode,
							ref vbooklandIsbnFormat,
							ref venableIssnEan
							);
					}
				}
			}
		}

		public static void Symbologies2DEnable(
			TerminalPlatform terminalPlatform,
			TerminalModel terminalModel,
			ReaderType readerType,
			bool? enablePDF417,
			bool? enableMicroPDF417,
			bool? enableCode128Emulation,
			bool? enableDataMatrix,
			int? enableDataMatrixInverse,
			int? decodeMirrorImage,
			bool? enableMaxicode,
			bool? enableQRCode,
			int? enableQRCodeInverse,
			bool? enableMicroQR,
			bool? enableAztec,
			int? enableAztecInverse
			)
		{
			int rw=0;
			int venablePDF417=0;
			int venableMicroPDF417=0;
			int venableCode128Emulation=0;
			int venableDataMatrix=0;
			int venableDataMatrixInverse=0;
			int vdecodeMirrorImage=0;
			int venableMaxicode=0;
			int venableQRCode=0;
			int venableQRCodeInverse=0;
			int venableMicroQR=0;
			int venableAztec=0;
			int venableAztecInverse=0;

			if(((
				terminalModel==TerminalModel.CL_CP30
				|| terminalModel==TerminalModel.CL_CP50
				) && readerType==ReaderType.ID_MOD_2D_4507
				) || (
				terminalModel==TerminalModel.CL_CP60
				|| terminalModel==TerminalModel.CL_9700
				|| terminalModel==TerminalModel.CL_CP55
				|| terminalModel==TerminalModel.CL_9200
				) && readerType==ReaderType.ID_MOD_2D_4500
				) {
				rw=(int)'r';
				if(readerType==ReaderType.ID_MOD_2D_4507) {
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.Symbologies_2D_4507(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.Symbologies_2D_4507(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					}
				} else {
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.Symbologies_2D_SE4500(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.Symbologies_2D_SE4500(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					}
				}
			
				if(enablePDF417.HasValue) {
					venablePDF417=(bool)enablePDF417?1:0;
				}
				if(enableMicroPDF417.HasValue) {
					venableMicroPDF417=(bool)enableMicroPDF417?1:0;
				}
				if(enableCode128Emulation.HasValue) {
					venableCode128Emulation=(bool)enableCode128Emulation?1:0;
				}
				if(enableDataMatrix.HasValue) {
					venableDataMatrix=(bool)enableDataMatrix?1:0;
				}
				if(enableDataMatrixInverse.HasValue) {
					venableDataMatrixInverse=(int)enableDataMatrixInverse;
				}
				if(decodeMirrorImage.HasValue) {
					vdecodeMirrorImage=(int)decodeMirrorImage;
				}
				if(enableMaxicode.HasValue) {
					venableMaxicode=(bool)enableMaxicode?1:0;
				}
				if(enableQRCode.HasValue) {
					venableQRCode=(bool)enableQRCode?1:0;
				}
				if(enableQRCodeInverse.HasValue) {
					venableQRCodeInverse=(int)enableQRCodeInverse;
				}
				if(enableMicroQR.HasValue) {
					venableMicroQR=(bool)enableMicroQR?1:0;
				}
				if(enableAztec.HasValue) {
					venableAztec=(bool)enableAztec?1:0;
				}
				if(enableAztecInverse.HasValue) {
					venableAztecInverse=(int)enableAztecInverse;
				}
				rw=(int)'w';
				if(readerType==ReaderType.ID_MOD_2D_4507) {
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.Symbologies_2D_4507(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.Symbologies_2D_4507(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					}
				} else {
					if(terminalPlatform==TerminalPlatform.WindowsCE) {
						ReaderNativeCE.Symbologies_2D_SE4500(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					} else if(terminalPlatform==TerminalPlatform.WindowsMobile) {
						ReaderNativeMobile.Symbologies_2D_SE4500(
							rw,
							ref venablePDF417,
							ref venableMicroPDF417,
							ref venableCode128Emulation,
							ref venableDataMatrix,
							ref venableDataMatrixInverse,
							ref vdecodeMirrorImage,
							ref venableMaxicode,
							ref venableQRCode,
							ref venableQRCodeInverse,
							ref venableMicroQR,
							ref venableAztec,
							ref venableAztecInverse
							);
					}
				}
			}
		}
	}

	/// <summary>
	/// Win32 API functions and constants
	/// </summary>
	internal class Win32API
	{
        internal const uint WM_USER = 0;
		/// <summary>
		/// Константа Win32 API
		/// </summary>
		internal const int WAIT_TIMEOUT=0x00000102;
		
		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[DllImport("coredll")]
		internal static extern uint RegisterWindowMessage(string lpString);

		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[DllImport("coredll")]
		internal static extern IntPtr CreateEvent(int lpEventAttributes0,int bManualReset,int bInitialState,string lpName); 

		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[DllImport("coredll")]
		internal static extern int CloseHandle(IntPtr hObject);

		/// <summary>
		/// Функция Win32 API
		/// </summary>
        [DllImport("coredll")]
        internal static extern int WaitForMultipleObjects(int nCount,IntPtr[] lpHandles,int fWaitAll,int dwMilliseconds);
		
		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[DllImport("coredll")]
		internal static extern uint WaitForSingleObject(IntPtr hHandle,int dwMilliseconds);
		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[DllImport("coredll")]
		internal static extern int EventModify(IntPtr hEvent,int func);

		/// <summary>
		/// Функция Win32 API
		/// </summary>
		internal static int ResetEvent(IntPtr hEvent)
		{
			return(EventModify(hEvent,2));
		}
		/// <summary>
		/// Функция Win32 API
		/// </summary>
		internal static int SetEvent(IntPtr hEvent)
		{
			return(EventModify(hEvent,3));
		}
	}

	interface IBarcodeProcessor
	{
		void OnScan(string barcode,int barcodeTypeID);
	}

	/// <summary>
	/// Аргументы события "сообщение отрисовано"
	/// </summary>
	public class ReaderEventArgs : System.EventArgs
	{
		/// <summary>
		/// Штрихкод
		/// </summary>
		public string barcode;
		/// <summary>
		/// Тип штрихкода
		/// </summary>
		public int barcodeTypeID;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="barcode">Штрихкод</param>
		/// <param name="barcodeTypeID">Тип штрихкода</param>
		internal ReaderEventArgs(string barcode,int barcodeTypeID)
		{
			this.barcode=barcode;
			this.barcodeTypeID=barcodeTypeID;
		}
	}

	public enum ReaderType
	{
		NONE           = 0,
		ID_MOD_1D      = 0x0001, // SE950,CCD 1D 
		ID_MOD_1D_ELR  = 0x0002, // 1D Long Laser
		ID_MOD_2D      = 0x0004, // SE4407 2D
		ID_MOD_TI_RFID = 0x0010, // Texas Instrument RFID
		ID_MOD_RFID    = 0x0020, // RFID
		ID_MOD_2D_4507 = 0x0040, // SE4507 2D
		ID_MOD_1D_955  = 0x0080, // SE955  1D
		ID_MOD_MP_RFID = 0x0100, // Microprogram RFID
		ID_MOD_2D_4500 = 0x0200, // Moto_SE4500
		ID_MOD_1D_SM1  = 0x0400  //
	}

}
