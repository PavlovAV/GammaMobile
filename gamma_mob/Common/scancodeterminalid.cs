namespace Scancode
{
	static class TerminalID
	{
		/// <summary>
		/// Чтоб не вызывать native-функции много раз, после определения модели сохраним ее в эту переменную
		/// </summary>
		private static TerminalModel ms_terminalmodel=TerminalModel.Unknown;
		/// <summary>
		/// Чтоб не вызывать native-функции много раз, после определения платформы сохраним ее в эту переменную
		/// </summary>
		private static TerminalPlatform ms_terminalplatform=TerminalPlatform.Unknown;

		/// <summary>
		/// Модель терминала
		/// </summary>
		public static TerminalModel Model
		{
			get
			{
				if(ms_terminalmodel!=TerminalModel.Unknown)
				{
					return(ms_terminalmodel);
				}

				TerminalModel terminalmodel=TerminalModel.Unknown;

#if !DEMO_PC
				char[] ac=new char[256];
				if(SystemParametersInfo(SPI_GETOEMINFO,ac.Length*2,ac,0)>0)
				{
					string str=(new string(ac)).TrimEnd(new char[1] { '\0' }).Replace('\0',' ');
					if(str=="CipherLab Co., Ltd. CP30")
					{
						terminalmodel=TerminalModel.CL_CP30;
					}
					else if(str=="CipherLab CP50")
					{
						terminalmodel=TerminalModel.CL_CP50;
					}
				}

				if(terminalmodel==TerminalModel.Unknown)
				{
					if(SystemParametersInfo(SPI_GETPLATFORMTYPE,ac.Length*2,ac,0)>0)
					{
						string s=(new string(ac)).ToLower();
						if(s.IndexOf("evm3730")==0)
						{
							terminalmodel=TerminalModel.CL_9700;
						}
						else if(s.IndexOf("sdp4430")==0)
						{
							terminalmodel=TerminalModel.CL_CP55;
						}
					}
				}

				if(terminalmodel==TerminalModel.Unknown) {
					try	{
						byte[] buf=new byte[512];
						if(GetSysInfoWM(buf)==0) {
							string str;
							str=System.Text.Encoding.ASCII.GetString(buf,0,4);
							if(str=="CP55") {
								terminalmodel=TerminalModel.CL_CP55;
							} else {
								str=System.Text.Encoding.Unicode.GetString(buf,0,14);
								if(str=="CPT9200") {
									terminalmodel=TerminalModel.CL_9200;
								} else {
									str=System.Text.Encoding.ASCII.GetString(buf,0,7);
									if(str=="CPT9700") {
										terminalmodel=TerminalModel.CL_9700;
									}
								}
							}
						}
					}
					catch {
					}
				}

				if(terminalmodel==TerminalModel.Unknown)
				{
					byte[] lpOutBuf=new byte[512];
					int pBytesReturned=0;
					
					lpOutBuf[0]=0;
					lpOutBuf[1]=2;
					lpOutBuf[2]=0;
					lpOutBuf[3]=0;
					if(KernelIoControl(0x01010054, // IOCTL_HAL_GET_DEVICEID
						System.IntPtr.Zero,
						0,
						lpOutBuf,
						lpOutBuf.Length,
						ref pBytesReturned
						)!=0)
					{
						int dwPlatformIDOffset=System.BitConverter.ToInt32(lpOutBuf,12);
						int dwPlatformIDBytes=System.BitConverter.ToInt32(lpOutBuf,16);
						System.Text.StringBuilder sb=new System.Text.StringBuilder(32);

						for(int i=dwPlatformIDOffset;i<dwPlatformIDOffset+dwPlatformIDBytes;i++)
						{
							if(lpOutBuf[i]==0x00)
							{
								sb.Append(" ");
							}
							else
							{
								sb.Append(((char)lpOutBuf[i]).ToString());
							}
						}
						if(sb.ToString(0,4)=="CP60")
						{
							terminalmodel=TerminalModel.CL_CP60;
						}
						else
						{
							if(sb.Length>=10 && sb.ToString(0,10)=="\x83\xE0\x21\x70\x0F\x4A\x01\x08\x43\x4C")
							{
								terminalmodel=TerminalModel.CL_9200;
							}
						}
					}
				}

				if(terminalmodel==TerminalModel.Unknown)
				{
					if(System.IO.File.Exists("/Windows/9300CE_SYS.dll"))
					{
						terminalmodel=TerminalModel.CL_9300;
					}
					else if(System.IO.File.Exists("/Windows/9400CE_SYS.dll"))
					{
						terminalmodel=TerminalModel.CL_9400;
					}
					else if(System.IO.File.Exists("/Windows/9500CE_SYS.dll"))
					{
						terminalmodel=TerminalModel.CL_9500;
					}
					else if(System.IO.File.Exists("/Windows/9500DLL.dll"))
					{
						terminalmodel=TerminalModel.CL_9500;
					}
					else if(System.IO.File.Exists("/Windows/9600CE_SYS.dll"))
					{
						terminalmodel=TerminalModel.CL_9600;
					}
				}

#endif // !DEMO_PC
				ms_terminalmodel=terminalmodel;
				return(terminalmodel);
			}
		}

		/// <summary>
		/// Платформа (ОС) терминала
		/// </summary>
		public static TerminalPlatform Platform
		{
			get
			{
				if(ms_terminalplatform!=TerminalPlatform.Unknown)
				{
					return(ms_terminalplatform);
				}

				TerminalPlatform terminalplatform=TerminalPlatform.Unknown;

#if !DEMO_PC
				char[] ac=new char[256];
				if(SystemParametersInfo(SPI_GETPLATFORMTYPE,ac.Length*2,ac,0)>0)
				{
					terminalplatform=TerminalPlatform.WindowsCE;
					string s=(new string(ac)).ToLower();
					if(s.IndexOf("pocketpc")>=0)
					{
						terminalplatform=TerminalPlatform.WindowsMobile;
					}
				}
#else // !DEMO_PC
				terminalPlatform=TerminalPlatform.PC;
#endif // !DEMO_PC
				ms_terminalplatform=terminalplatform;
				return(terminalplatform);
			}
		}

		/// <summary>
		/// Серийный номер терминала
		/// </summary>
		public static string SerialNumber
		{
			get
			{
				string sn="";
#if !DEMO_PC
				byte[] buf=new byte[512];
				switch(Model)
				{
					case TerminalModel.CL_9600:
						if(GetSysInfo9600(buf)==0)
						{
							sn=System.Text.Encoding.ASCII.GetString(buf,32,9);
						}
						break;
					case TerminalModel.CL_CP30:
						if(GetSysInfoWM(buf)==0)
						{
							sn=System.Text.Encoding.Unicode.GetString(buf,64,26);
						}
						break;
					case TerminalModel.CL_9200:
						if(GetSysInfoWM(buf)==0)
						{
							sn=System.Text.Encoding.Unicode.GetString(buf,64,30);
						}
						break;
					case TerminalModel.CL_9700:
						if(GetSysInfoCE(buf)==0)
						{
							sn=System.Text.Encoding.ASCII.GetString(buf,32,13);
						}
						break;
					case TerminalModel.CL_CP55:
						if(GetSysInfoCE(buf)==0)
						{
							sn=System.Text.Encoding.ASCII.GetString(buf,32,13);
						}
						break;

				}
#endif // !DEMO_PC

				return(sn);
			}
		}

		/// <summary>
		/// Константа Win32 API
		/// </summary>
		internal static int SPI_GETPLATFORMTYPE=257;

		/// <summary>
		/// Константа Win32 API
		/// </summary>
		internal static int SPI_GETOEMINFO=258;

		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[System.Runtime.InteropServices.DllImport("coredll")]
		internal extern static int KernelIoControl(uint dwIoControlCode,System.IntPtr lpInBuf,int nInBufSize,byte[] lpOutBuf,int nOutBufSize,ref int pBytesReturned);

		/// <summary>
		/// Функция Win32 API
		/// </summary>
		[System.Runtime.InteropServices.DllImport("coredll")]
		internal static extern int SystemParametersInfo(int uiAction,int uiParam,char[] pvParam,int fWinIni);


		/// <summary>
		/// Native-функция из system.dll
		/// </summary>
		[System.Runtime.InteropServices.DllImport("9600CE_SYS", EntryPoint="GetSysInfo")]
		internal static extern int GetSysInfo9600(byte[] buf);

		/// <summary>
		/// Native-функция из system.dll
		/// </summary>
		[System.Runtime.InteropServices.DllImport("SystemMobile", EntryPoint="GetSysInfo")]
		internal static extern int GetSysInfoWM(byte[] buf);

		/// <summary>
		/// Native-функция из system.dll
		/// </summary>
		[System.Runtime.InteropServices.DllImport("SystemCE", EntryPoint="GetSysInfo")]
		internal static extern int GetSysInfoCE(byte[] buf);
		
	};


	/// <summary>
	/// Модель терминала
	/// </summary>
	public enum TerminalModel
	{
		Unknown,
		CL_9300,
		CL_9400,
		CL_9500,
		CL_9600,
		CL_CP30,
		CL_CP50,
		CL_CP60,
		CL_9200,
		CL_9700,
		CL_CP55
	}

	/// <summary>
	/// Платформа (ОС) терминала
	/// </summary>
	public enum TerminalPlatform
	{
		Unknown=-1,
		PC,
		WindowsCE,
		WindowsMobile
	}
}
