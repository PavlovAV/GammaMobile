using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Models;
using gamma_mob.Properties;
using gamma_mob.Dialogs;
using System;
using System.Linq;
using System.IO;
using OpenNETCF.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using Datalogic.API;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;

using gamma_mob.Common;

using OpenNETCF.Net.NetworkInformation;

using System.Net;
using System.Data.SqlClient;
using System.Data;


namespace gamma_mob.Common
{
    public class Shared
    {
        private static List<Warehouse> _warehouses;
        
        static Shared()
        {
            LoadImages();
        }

        //public static readonly FileStream _txtLogFile = new FileStream(@"\Temp\TxtLogFile.txt", FileMode.Append, FileAccess.Write, FileShare.Write);

        private static IDeviceExtended _device { get; set; }
        public static IDeviceExtended Device
        {
            get
            {
                if (_device == null)
                {
                    if (Program.deviceName.Contains("CPT"))
                        _device = new DeviceCipherlab();
                    else if (Program.deviceName.Contains("Falcon"))
                        _device = new DeviceDatalogic();
                }
                return _device;
            }
        }

        private static SqlConnection _connection { get; set; }
        public static SqlConnection Connection
        {
            get
            {
                //if (_connection == null || _connection.ConnectionString == String.Empty || _connection.ConnectionString != Db.GetConnectionString())
                //{
                //    if (_connection != null && _connection.ConnectionString != String.Empty)
                //    {
                //        //_connection.StateChange -= HandleSqlConnectionDrop;
                //        _connection.Close();
                //    }
                //    _connection = new SqlConnection(Db.GetConnectionString());
                //    //_connection.StateChange += HandleSqlConnectionDrop;
                //}
                _connection = new SqlConnection(Db.GetConnectionString());
                return _connection;
            }
        }

        private static SqlConnection _connectionCheckStatus { get; set; }
        public static SqlConnection ConnectionCheckStatus
        {
            get
            {
                if (_connectionCheckStatus == null || _connectionCheckStatus.ConnectionString == String.Empty || _connectionCheckStatus.ConnectionString != Db.GetConnectionString(ConnectionCheckStatusTimeout))
                {
                    _connectionCheckStatus = new SqlConnection(Db.GetConnectionString(ConnectionCheckStatusTimeout));
                }
                return _connectionCheckStatus;
            }
        }

        private static int? _connectionCheckStatusTimeout { get; set; }
        private static int ConnectionCheckStatusTimeout
        {
            get
            {
                if (_connectionCheckStatusTimeout == null)
                {
                    var timeout = Db.GetProgramSettings("ConnectionCheckStatusTimeout");
                    if (timeout != null && timeout != String.Empty)
                        _connectionCheckStatusTimeout = Convert.ToInt32(timeout);
                }
                return _connectionCheckStatusTimeout ?? 60;
            }
        }

        private static bool? _lastQueryCompleted { get; set; }
        public static bool? LastQueryCompleted 
        {
            get
            {
                return _lastQueryCompleted;
            }
            set
            {
                if (value != _lastQueryCompleted)
                {
                    _lastQueryCompleted = value;
                    //if (value == false)
                    //{
                    //    //ConnectionState.StartChecker();
                    //    //if (Connection.State == System.Data.ConnectionState.Open)
                    //    //    Connection.Close();
                    //}
                }
            }
        }
        public static bool LastCeQueryCompleted { get; set; }
        
        
        public static ImageList ImgList { get; private set; }

        public static Guid PersonId { get; set; }
        public static String PersonName { get; set; }

        public static byte ShiftId { get; set; }

        public static int PlaceId { get; set; }

        public static bool IsFindBarcodeFromFirstLocalAndNextOnline { get; set; }

        public static bool IsAvailabilityCreateNewPalletNotOnOrder { get; set; }

        public static bool IsAvailabilityChoiseNomenclatureForMovingGroupPack { get; set; }
        
        public static bool IsNotUpdateCashedBarcodesOnFirst { get; set; }

        public static VisibleButtonsOnMainWindow VisibledButtonsOnMainWindow { get; set; }

        public static bool VisibleShortcutStartPoints { get; set; } 

        public static List<Warehouse> Warehouses
        {
            get
            {
                if (_warehouses == null)
                {
                    List<Warehouse> list = Db.GetWarehouses();
                    if (list == null) return null;
                    _warehouses = list;
                }
                return _warehouses;
            }
        }

        private static List<MeasureUnitNomenclature> _measureUnits = new List<MeasureUnitNomenclature>();
        public static List<MeasureUnit> GetMeasureUnitsForNomenclature(Guid nomenclatureId, Guid characteristicId)
        {
            if (_measureUnits == null || _measureUnits.Count == 0 || !_measureUnits.Any(m => m.NomenclatureID == nomenclatureId && m.CharacteristicID == characteristicId))
            {
                List<MeasureUnitNomenclature> list = Db.GetMeasureUnitsForNomenclature(nomenclatureId, characteristicId);
                if (list != null)
                {
                    foreach (var measure in list)
                        if (!_measureUnits.Any(m => m.NomenclatureID == measure.NomenclatureID && m.CharacteristicID == measure.CharacteristicID && m.MeasureUnitID == measure.MeasureUnitID))
                            _measureUnits.Add(measure);
                }
            }
            return _measureUnits.Where(m => m.NomenclatureID == nomenclatureId && m.CharacteristicID == characteristicId).Select(m => new MeasureUnit() { MeasureUnitID = m.MeasureUnitID, Name = m.Name, IsActive = m.IsActive, Numerator = m.Numerator, Denominator = m.Denominator, IsInteger = m.IsInteger }).ToList();
        }

        private static int? _countRowUploadToServerInOnePackage { get; set; }
        public static int CountRowUploadToServerInOnePackage
        {
            get
            {
                if (_countRowUploadToServerInOnePackage == null)
                {
                    var countRowUploadToServerInOnePackage = Db.GetProgramSettings("CountRowsUploadToServerInOnePackage");
                    if (countRowUploadToServerInOnePackage != null && countRowUploadToServerInOnePackage != String.Empty)
                        _countRowUploadToServerInOnePackage = Convert.ToInt32(countRowUploadToServerInOnePackage);
                }
                return _countRowUploadToServerInOnePackage ?? 1;
            }
        }

        private static bool? _isScanGroupPackOnlyFromProduct { get; set; }
        public static bool IsScanGroupPackOnlyFromProduct
        {
            get
            {
                if (_isScanGroupPackOnlyFromProduct == null)
                {
                    var isScanGroupPackOnlyFromProduct = Db.GetProgramSettings("IsScanGroupPackOnlyFromProduct");
                    if (isScanGroupPackOnlyFromProduct != null && isScanGroupPackOnlyFromProduct != String.Empty)
                        _isScanGroupPackOnlyFromProduct = Convert.ToBoolean(isScanGroupPackOnlyFromProduct);
                }
                return _isScanGroupPackOnlyFromProduct ?? false;
            }
        }

        public static bool RefreshIsScanGroupPackOnlyFromProduct()
        {
            _isScanGroupPackOnlyFromProduct = null;
            return (IsScanGroupPackOnlyFromProduct != null);
        }

        private static int? _maxAllowedPercentBreak {get; set;}
        public static int MaxAllowedPercentBreak 
        { 
            get
            {
                if (_maxAllowedPercentBreak == null)
                {
                    var maxAllowedPercentBreak = Db.GetProgramSettings("MaxAllowedPercentBreakInDocOrder");
                    if (maxAllowedPercentBreak != null && maxAllowedPercentBreak != String.Empty) 
                        _maxAllowedPercentBreak = Convert.ToInt32(maxAllowedPercentBreak);
                }
                return _maxAllowedPercentBreak ?? 0;
            }
        }
        
        private static int? _maxCountRowInPackOnFirstUpdateCashedBarcodes {get; set;}
        public static int MaxCountRowInPackOnFirstUpdateCashedBarcodes 
        { 
            get
            {
                if (_maxCountRowInPackOnFirstUpdateCashedBarcodes == null)
                {
                    var maxCountRowInPackOnFirstUpdateCashedBarcodes = Db.GetProgramSettings("MaxCountRowInPackOnFirstUpdateCashedBarcodes");
                    if (maxCountRowInPackOnFirstUpdateCashedBarcodes != null && maxCountRowInPackOnFirstUpdateCashedBarcodes != String.Empty) 
                        _maxCountRowInPackOnFirstUpdateCashedBarcodes = Convert.ToInt32(maxCountRowInPackOnFirstUpdateCashedBarcodes);
                }
                return _maxCountRowInPackOnFirstUpdateCashedBarcodes ?? 500;
            }
        }

        private static int? _limitNormalLevelBattery { get; set; }
        private static int limitNormalLevelBattery
        {
            get
            {
                if (_limitNormalLevelBattery == null)
                {
                    var limit = Db.GetProgramSettings("LimitNormalLevelBattery");
                    if (limit != null && limit != String.Empty)
                        _limitNormalLevelBattery = Convert.ToInt32(limit);
                }
                return _limitNormalLevelBattery ?? 10;
            }
        }

        private static uint? _lowLevelBatterySuspendTimeout { get; set; }
        private static uint lowLevelBatterySuspendTimeout
        {
            get
            {
                if (_lowLevelBatterySuspendTimeout == null)
                {
                    var timeout = Db.GetProgramSettings("LowLevelBatterySuspendTimeout");
                    if (timeout != null && timeout != String.Empty)
                        _lowLevelBatterySuspendTimeout = Convert.ToUInt32(timeout);
                }
                return _lowLevelBatterySuspendTimeout ?? 60;
            }
        }

        private static uint? _normalLevelBatterySuspendTimeout { get; set; }
        private static uint normalLevelBatterySuspendTimeout
        {
            get
            {
                if (_normalLevelBatterySuspendTimeout == null)
                {
                    var timeout = Db.GetProgramSettings("NormalLevelBatterySuspendTimeout");
                    if (timeout != null && timeout != String.Empty )
                        _normalLevelBatterySuspendTimeout = Convert.ToUInt32(timeout);
                }
                return _normalLevelBatterySuspendTimeout ?? 600;
            }
        }

        private static int? _timerPeriodForUnloadOfflineProducts { get; set; }
        public static int TimerPeriodForUnloadOfflineProducts
        {
            get
            {
                if (_timerPeriodForUnloadOfflineProducts == null)
                {
                    var timerPeriodForUnloadOfflineProducts = Db.GetProgramSettings("TimerPeriodForUnloadOfflineProducts");
                    if (timerPeriodForUnloadOfflineProducts != null && timerPeriodForUnloadOfflineProducts != String.Empty) 
                        _timerPeriodForUnloadOfflineProducts = Convert.ToInt32(timerPeriodForUnloadOfflineProducts);
                }
                return _timerPeriodForUnloadOfflineProducts ?? 20000;
            }
        }

        private static int? _timerPeriodForBarcodesUpdate { get; set; }
        public static int TimerPeriodForBarcodesUpdate
        {
            get
            {
                if (_timerPeriodForBarcodesUpdate == null)
                {
                    var timerPeriodForBarcodesUpdate = Db.GetProgramSettings("TimerPeriodForBarcodesUpdate");
                    if (timerPeriodForBarcodesUpdate != null && timerPeriodForBarcodesUpdate != String.Empty) 
                        _timerPeriodForBarcodesUpdate = Convert.ToInt32(timerPeriodForBarcodesUpdate);
                }
                return _timerPeriodForBarcodesUpdate ?? 360000;
            }
        }


        private static int? _timerPeriodForCheckBatteryLevel { get; set; }
        public static int TimerPeriodForCheckBatteryLevel
        {
            get
            {
                if (_timerPeriodForCheckBatteryLevel == null)
                {
                    var timerPeriodForCheckBatteryLevel = Db.GetProgramSettings("TimerPeriodForCheckBatteryLevel");
                    if (timerPeriodForCheckBatteryLevel != null && timerPeriodForCheckBatteryLevel != String.Empty)
                        _timerPeriodForCheckBatteryLevel = Convert.ToInt32(timerPeriodForCheckBatteryLevel);
                }
                return _timerPeriodForCheckBatteryLevel ?? 540000;
            }
        }

        private static int? _timerPeriodForCheckUpdateProgram { get; set; }
        public static int TimerPeriodForCheckUpdateProgram
        {
            get
            {
                if (_timerPeriodForCheckUpdateProgram == null)
                {
                    var timerPeriodForCheckUpdateProgram = Db.GetProgramSettings("TimerPeriodForCheckUpdateProgram");
                    if (timerPeriodForCheckUpdateProgram != null && timerPeriodForCheckUpdateProgram != String.Empty)
                        _timerPeriodForCheckUpdateProgram = Convert.ToInt32(timerPeriodForCheckUpdateProgram);
                }
                return _timerPeriodForCheckUpdateProgram ?? 1800000;
            }
        }

        private static int? _minLengthProductBarcode { get; set; }
        public static int MinLengthProductBarcode
        {
            get
            {
                if (_minLengthProductBarcode == null)
                {
                    var minLengthProductBarcode = Db.GetProgramSettings("MinLengthProductBarcode");
                    if (minLengthProductBarcode != null && minLengthProductBarcode != String.Empty)
                        _minLengthProductBarcode = Convert.ToInt32(minLengthProductBarcode);
                }
                return _minLengthProductBarcode ?? 5;//5 символов;
            }
        }

        private static ScannedBarcodes _scannedBarcodes { get; set; }
        public static ScannedBarcodes ScannedBarcodes 
        {
            get
            {
                if (_scannedBarcodes == null)
                {
                    ScannedBarcodes list = new ScannedBarcodes();
                    if (list == null) return null;
                    _scannedBarcodes = list;
                }
                return _scannedBarcodes;
            }
        }

        private static void LoadImages()
        {
            ImgList = new ImageList();
            ImgList.Images.Add(Resources.back); //
            ImgList.Images.Add(Resources.Binocle);
            ImgList.Images.Add(Resources.docplus);
            ImgList.Images.Add(Resources.network_offline);
            ImgList.Images.Add(Resources.network_offline_small);
            ImgList.Images.Add(Resources.network_transmit_receive);
            ImgList.Images.Add(Resources.network_transmit_receive_small);
            ImgList.Images.Add(Resources.network_transmit_receiveoff);
            ImgList.Images.Add(Resources.search);
            ImgList.Images.Add(Resources.edit_1518);
            ImgList.Images.Add(Resources.refresh);
            ImgList.Images.Add(Resources.UploadToDb);
            ImgList.Images.Add(Resources.question);
            ImgList.Images.Add(Resources.save);
            ImgList.Images.Add(Resources.print);
            ImgList.Images.Add(Resources.pallet);
            ImgList.Images.Add(Resources.add);
            ImgList.Images.Add(Resources.delete);
            ImgList.Images.Add(Resources.InfoProduct);
            ImgList.Images.Add(Resources.RDP);
            ImgList.Images.Add(Resources.ShortcutStartPointsPanelEnabled);
            ImgList.Images.Add(Resources.backOffline); 
        }

        private const int BS_MULTILINE = 0x00002000;
        private const int GWL_STYLE = -16;

        [System.Runtime.InteropServices.DllImport("coredll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("coredll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void MakeButtonMultiline(Button b)
        {
            IntPtr hwnd = b.Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            int newStyle = SetWindowLong(hwnd, GWL_STYLE, currentStyle | BS_MULTILINE);
        }

        private static /*BindingList<ChooseNomenclatureItem>*/ CashedBarcodes _barcodes1C { get; set; }

        public static /*BindingList<ChooseNomenclatureItem>*/ CashedBarcodes Barcodes1C 
        { 
            get
            {
                if (_barcodes1C == null)
                {
                    //var lastTimeBarcodes1C = Db.GetServerDateTime();
                    //var list = Db.GetBarcodes1C();
                    var list = new CashedBarcodes();
                    if (list == null) return null;
                    //LastUpdatedTimeBarcodes = lastTimeBarcodes1C;
                    _barcodes1C = list;
                }
                return _barcodes1C;
            } 
        }

        //private static DateTime _lastUpdatedTimeBarcodes { get; set; }
        //private static DateTime LastUpdatedTimeBarcodes 
        //{
        //    get
        //    {
        //       return _lastUpdatedTimeBarcodes ;
        //    }

        //    set
        //    {
        //       if (Db.UpdateLastUpdatedTimeBarcodes(value))
        //           _lastUpdatedTimeBarcodes = value;
        //    }
        //}
        //private static BindingList<ChooseNomenclatureItem> Barcodes1CChanges { get; set; }

        private static bool _refreshBarcodes1CRunning;

        public static void RefreshBarcodes1CFromTimer(object obj)
        {
#if OUTPUTDEBUGINFO
            var n = DateTime.Now.ToString();
            System.Diagnostics.Debug.Write(n + " !!!!!RefreshBarcodes1CFromTimer(" + _refreshBarcodes1CRunning.ToString() + ")!" + Environment.NewLine);
#endif
            if (_refreshBarcodes1CRunning) return;
            lock (lockerForBarcodesUpdate)
            {
                _refreshBarcodes1CRunning = true;
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.Write(n + " !!!!!Barcodes1C.UpdateBarcodes(" + _refreshBarcodes1CRunning.ToString() + ")!" + Environment.NewLine);
#endif
                Barcodes1C.UpdateBarcodes(false);
            }
            _refreshBarcodes1CRunning = false;
        }
        /*
        public static void RefreshBarcodes1C()
        {
            Barcodes1C.UpdateBarcodes();


            //if (Db.CheckSqlConnection() == 0)
            //{
            //    var Barcodes1CChanges = Db.GetBarcodes1CChanges(Shared.LastTimeBarcodes1C);
            //    if (Barcodes1CChanges != null)
            //    {
            //        LastTimeBarcodes1C = Db.GetServerDateTime();
            //        foreach (var item in Barcodes1CChanges)
            //        {
            //            var ItemDel = Barcodes1C.Where(b => b.BarcodeId == item.BarcodeId).FirstOrDefault();
            //            if (ItemDel != null) Barcodes1C.Remove(ItemDel);
            //            Shared.Barcodes1C.Add(item);
            //        }
            //    }
            //}
        }*/

        public static bool IsExistsUnloadOfflineProducts { get; set; }

        private static bool _unloadOfflineProductsRunning;

        public static void UnloadOfflineProductsFromTimer(object obj)
        {
            var n = DateTime.Now.ToString();
#if OUTPUTDEBUGINFO
            System.Diagnostics.Debug.Write(n + " !!!!!UnloadOfflineProductsFromTimer(" + _unloadOfflineProductsRunning.ToString() + ")!" + Environment.NewLine);
#endif
            if (!Shared.IsExistsUnloadOfflineProducts || _unloadOfflineProductsRunning || !ConnectionState.IsConnected) return;
            lock (lockerForUnloadOfflineProducts)
            {
                _unloadOfflineProductsRunning = true;
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.Write(n + " !!!!!UnloadOfflineProducts(" + _unloadOfflineProductsRunning.ToString() + ")!" + Environment.NewLine);
#endif
                ScannedBarcodes.UnloadOfflineProducts(false);
            }
            _unloadOfflineProductsRunning = false;
        }

        private static List<PlaceZone> _placeZones { get; set; }

        public static List<PlaceZone> PlaceZones
        {
            get
            {
                if (_placeZones == null)
                {
                    List<PlaceZone> list = Db.GetPlaceZones();
                    if (list == null) return new List<PlaceZone>();
                    _placeZones = list;
                }
                return _placeZones;
            }
        }

        private static string _batterySerialNumber { get; set; }

        private static string batterySerialNumber
        {
            get
            {
                return _batterySerialNumber;
            }
            set
            {
                _batterySerialNumber = value;
                Shared.SaveToLogStartProgramInformation("Battery SerialNumber " + value);
            }
        }

        private static int _batteryLevel { get; set; }

        private static int batteryLevel
        {
            get
            {
                return _batteryLevel;
            }
            set
            {
                _batteryLevel = value;
                Shared.SaveToLogInformation("Battery Level " + value.ToString());
                try
                {
                    var batterySuspendTimeout = Shared.Device.GetBatterySuspendTimeout();
                    if (value <= limitNormalLevelBattery && batterySuspendTimeout != lowLevelBatterySuspendTimeout)
                        Shared.SaveToLogInformation("SetBatterySuspendTimeout(" + lowLevelBatterySuspendTimeout.ToString() + ") " + Shared.Device.SetBatterySuspendTimeout(lowLevelBatterySuspendTimeout).ToString());
                    if (value > limitNormalLevelBattery && batterySuspendTimeout != normalLevelBatterySuspendTimeout)
                        Shared.SaveToLogInformation("SetBatterySuspendTimeout(" + normalLevelBatterySuspendTimeout.ToString() + ") " + Shared.Device.SetBatterySuspendTimeout(normalLevelBatterySuspendTimeout).ToString());
                }
                catch
                {
                    Shared.SaveToLogError("Error SetBatterySuspendTimeout in set batteryLevel");
                }
                
            }
        }

        public static bool InitializationData()
        {
            return !(Shared.Barcodes1C == null || Shared.PlaceZones == null || Shared.Warehouses == null || Shared.PlaceZones == null || Shared.Barcodes1C.UpdateBarcodes(true) == null/*Shared.Barcodes1C.InitCountBarcodes() == null ||*/
                || Shared.MaxAllowedPercentBreak == null || Shared.TimerPeriodForBarcodesUpdate == null || Shared.TimerPeriodForUnloadOfflineProducts == null
                || Shared.ScannedBarcodes == null );// // Shared.TimerForBarcodesUpdate == null);
            Cursor.Current = Cursors.Default;
        }

        //public static string _logFile { get; private set; }

        public static bool IsLocalDateTimeUpdated { get; private set; }


        [DllImport("coredll.dll", SetLastError = true)]
        static extern Int32 GetLastError();

        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool SetSystemTime(ref SYSTEMTIME time);
        [DllImport("coredll.dll", SetLastError = true)]
        static extern void GetSystemTime(out SYSTEMTIME lpSystemTime);
        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool SetLocalTime(ref SYSTEMTIME time);

        [DllImport("coredll.dll")]
        static extern bool SetTimeZoneInformation([In] ref TIME_ZONE_INFORMATION lpTimeZoneInformation);
        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern int GetTimeZoneInformation(out TIME_ZONE_INFORMATION lpTimeZoneInformation);

        private const int TIME_ZONE_ID_UNKNOWN = 0;
        private const int TIME_ZONE_ID_STANDARD = 1;
        private const int TIME_ZONE_ID_DAYLIGHT = 2;

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TIME_ZONE_INFORMATION
        {
            public int bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string standardName;
            public SYSTEMTIME standardDate;
            public int standardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string daylightName;
            public SYSTEMTIME daylightDate;
            public int daylightBias;
        }

        private static bool disableDST(TIME_ZONE_INFORMATION tzi)
        {
            //set wMonth in standardDate to zero
            SYSTEMTIME stStd;
            stStd = tzi.standardDate;
            stStd.wMonth = 0;
            //set wMonth in daylightDate to zero
            SYSTEMTIME stDST;
            stDST = tzi.daylightDate;
            stDST.wMonth = 0;

            tzi.daylightDate = stDST;
            tzi.standardDate = stStd;
            bool bRes = SetTimeZoneInformation(ref tzi);
           /* if (bRes)
                MessageBox.Show(@"*** Disabling DST OK***");
            else
                MessageBox.Show(@"*** Disabling DST failed***");*/
            return bRes;
        }

        public static void SetSystemDateTime(DateTime dt)
        {
            SYSTEMTIME systime = new SYSTEMTIME
            {
                wYear = (short)dt.Year,
                wMonth = (short)dt.Month,
                wDay = (short)dt.Day,
                wHour = (short)dt.Hour,
                wMinute = (short)dt.Minute,
                wSecond = (short)dt.Second,
                wMilliseconds = (short)dt.Millisecond,
                wDayOfWeek = (short)dt.DayOfWeek
            };
            SYSTEMTIME daylighttime = new SYSTEMTIME
            {
                wYear = (short)0,
                wMonth = (short)0,
                wDay = (short)0,
                wHour = (short)0,
                wMinute = (short)0,
                wSecond = (short)0,
                wMilliseconds = (short)0,
                wDayOfWeek = (short)0
            };
            TIME_ZONE_INFORMATION tzi;
                
            GetTimeZoneInformation(out tzi); // Get current time zone
            
            //Set Russian time zone
            tzi.bias = -180;
            tzi.daylightBias = -60;
            tzi.daylightName = "Russian Daylight Time";
            tzi.daylightDate = daylighttime;
            tzi.standardBias = 0;
            tzi.standardName = "Russian Standard Time";
            tzi.standardDate = daylighttime;
            SetTimeZoneInformation(ref tzi);

            //Clear time zone cashed during starting program
            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
                        
            SetSystemTime(ref systime);

            IsLocalDateTimeUpdated = true;
            /*_logFile = Path.Combine(Application2.StartupPath + @"\", string.Format("{0:yyyMMdd}.log", DateTime.Now));
            try
            {
                if (!File.Exists(_logFile)) // may have to specify path here!
                {
                    // may have to specify path here!
                    File.Create(_logFile).Close();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }*/
            
        }

        public static void SaveToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool? isUploaded, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity, int? quantityFractional, Guid? measureUnitId, Guid? fromProductId, int? fromPlaceId, Guid? fromPlaceZoneId, int? newWeight, DateTime? validUntilDate)
        {

            try
            {
                Db.AddMessageToLog(scanId, dateScanned, barcode, placeId, placeZoneId, docTypeId, docId, isUploaded, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity, quantityFractional, measureUnitId, fromProductId, fromPlaceId, fromPlaceZoneId, newWeight, validUntilDate);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static void SaveToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool? isUploaded, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity, int? quantityFractional)
        {

            try
            {
                Db.AddMessageToLog(scanId, dateScanned, barcode, placeId, placeZoneId, docTypeId, docId, isUploaded, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity, quantityFractional, null, null, null, null, null, null);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static void SaveToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool? isUploaded, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity)
        {

            try
            {
                Db.AddMessageToLog(scanId, dateScanned, barcode, placeId, placeZoneId, docTypeId, docId, isUploaded, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity, null, null, null, null, null, null, null);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static void SaveToLog(Guid scanId, bool? isUploaded, string log)
        {

            try
            {
                Db.AddMessageToLog(scanId, isUploaded, log);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static void SaveToLog(Guid scanId, bool? isUploaded, bool isDeleted, string log)
        {

            try
            {
                Db.AddMessageToLog(scanId, isUploaded, isDeleted, log);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }
        /*public static void SaveToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool isUploaded)
        {
            
            try
            {
                Db.AddMessageToLog(scanId, dateScanned, barcode, placeId, placeZoneId, docTypeId, docId, isUploaded);
                
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
*/
        private static void SaveToLog(string log, Guid? docId, Guid? productId)
        {

            try
            {
                Db.AddMessageToLog(log, docId, productId);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        private static void SaveToLog(string log)
        {

            try
            {
                Db.AddMessageToLog(log);
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static void SaveToLogInformation(string log, Guid? docId, Guid? productId)
        {
            Shared.SaveToLog(log, docId, productId);
        }

        public static void SaveToLogError(string log, Guid? docId, Guid? productId)
        {
            Shared.SaveToLog(@"ERROR! " + log, docId, productId);
        }

        public static void SaveToLogQuestion(string log, Guid? docId, Guid? productId)
        {
            Shared.SaveToLog(@"QUEST? " + log, docId, productId);
        }

        public static void SaveToLogInformation(string log)
        {
            Shared.SaveToLogInformation(log, null, null);
        }

        public static void SaveToLogError(string log)
        {
#if OUTPUTDEBUGINFO
            MessageBox.Show(log);
#endif            
            Shared.SaveToLogError(log, null, null);
        }

        public static void SaveToLogQuestion(string log)
        {
            Shared.SaveToLogQuestion(log, null, null);
        }


        public static void SaveToLogStartProgramInformation(string log)
        {
            Shared.SaveToLog(@"START: " + log);
        }

        public static void DeleteOldUploadedToServerLogs()
        {
            try
            {
                Db.DeleteOldUploadedToServerLogs();
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static List<ScannedBarcode> LoadFromLogBarcodesForCurrentUser()
        {
            return Db.GetBarcodesForCurrentUser() ?? new List<ScannedBarcode>();
        }

        public static object lockerForBarcodesUpdate = new object();

        private static System.Threading.Timer _timerForBarcodesUpdate { get; set; }
        public static System.Threading.Timer TimerForBarcodesUpdate
        {
            get
            {
                if (_timerForBarcodesUpdate == null)
                {
                    int num = 0;
                    TimerCallback tm = new TimerCallback(Shared.RefreshBarcodes1CFromTimer);
                    // создаем таймер
                    _timerForBarcodesUpdate = new System.Threading.Timer(tm, num, 0, Shared.TimerPeriodForBarcodesUpdate);
                }
                return _timerForBarcodesUpdate;
            }

        }
        
        public static object lockerForUnloadOfflineProducts = new object();

        private static System.Threading.Timer _timerForUnloadOfflineProducts { get; set; }
        public static System.Threading.Timer TimerForUnloadOfflineProducts
        {
            get
            {
                if (_timerForUnloadOfflineProducts == null)
                {
                    int num = 0;
                    TimerCallback tm = new TimerCallback(Shared.UnloadOfflineProductsFromTimer);
                    // создаем таймер
                    _timerForUnloadOfflineProducts = new System.Threading.Timer(tm, num, 0, Shared.TimerPeriodForUnloadOfflineProducts);
                }
                return _timerForUnloadOfflineProducts;
            }

        }

        private static System.Threading.Timer _timerForCheckBatteryLevel { get; set; }
        public static System.Threading.Timer TimerForCheckBatteryLevel
        {
            get
            {
                if (_timerForCheckBatteryLevel == null)
                {
                    int num = 0;
                    TimerCallback tm = new TimerCallback(Shared.CheckBatteryLevel);
                    // создаем таймер
                    _timerForCheckBatteryLevel = new System.Threading.Timer(tm, num, 0, Shared.TimerPeriodForCheckBatteryLevel);
                }
                return _timerForCheckBatteryLevel;
            }

        }

        public static void CheckBatteryLevel(object obj)
        {
            try
            {
                batteryLevel = Shared.Device.GetBatteryLevel();
                UpdateBatterySerialumber();
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
            }
        }

        public static void UpdateBatterySerialumber()
        {
            try
            {
                /*Device.BatteryInfo bInfo;
                var res = Device.GetBatteryInfo(out bInfo);
                if (bInfo != null && (batterySerialNumber == null || batterySerialNumber != bInfo.SerialNumber))
                    batterySerialNumber = bInfo.SerialNumber;*/
                var bsn = Shared.Device.GetBatterySerialNumber();
                if (batterySerialNumber == null || batterySerialNumber != bsn)
                    batterySerialNumber = bsn;
            }
            catch
            {
                Shared.SaveToLogError("Error UpdateBatterySerialumber");
            }
        }

        public static int ToolBarWeight = Program.deviceName.Contains("CPT") ? 33 : 29;

        public static int ToolBarHeight = Program.deviceName.Contains("CPT") ? 32 : 29;

        private static System.Threading.Timer _timerForCheckUpdateProgram { get; set; }
        public static System.Threading.Timer TimerForCheckUpdateProgram
        {
            get
            {
                if (_timerForCheckUpdateProgram == null)
                {
                    UpdateProgram.DropFlagUpdateLoading();
                    int num = 0;
                    TimerCallback tm = new TimerCallback(UpdateProgram.LoadUpdate);
                    // создаем таймер
                    _timerForCheckUpdateProgram = new System.Threading.Timer(tm, num, 10, Shared.TimerPeriodForCheckUpdateProgram);
                }
                return _timerForCheckUpdateProgram;
            }
        }

        public static bool ShowMessageError(string message)
        {
            return ShowMessageError(message, @"", null, null);
        }

        public static bool ShowMessageError(string message, string technicalMessage, Guid? docID, Guid? productID)
        {
            (new MessageBoxDialog(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1))
                .ShowDialog();
            Shared.SaveToLogError(message, docID, productID);
            return true;
        }

       public static DialogResult ShowMessageQuestion(string message)
        {
            return ShowMessageQuestion(message, @"", null, null);
        }

       public static DialogResult ShowMessageQuestion(string message, string technicalMessage, Guid? docID, Guid? productID)
        {
            DialogResult res = DialogResult.No;
            using (var m = new MessageBoxDialog(message, "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
            {
                res = m.ShowDialog();
            }
            Shared.SaveToLogQuestion(message + " => Ответ: " + res, docID, productID);
            return res;
        }

        public static bool ShowMessageInformation(string message)
        {
            return ShowMessageInformation(message, @"", null, null);
        }

        public static bool ShowMessageInformation(string message, string technicalMessage, Guid? docID, Guid? productID)
        {
            (new MessageBoxDialog(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1))
                .ShowDialog();
            Shared.SaveToLogInformation(message, docID, productID);
            return true;
        }

        public static List<ProductKind> FactProductKinds = new List<ProductKind>() { ProductKind.ProductSpool, ProductKind.ProductGroupPack, ProductKind.ProductPallet, ProductKind.ProductPalletR };

        public static bool? ExecRDP()
        {
            var cerdispProcess = GetProcessRunning(@"cerdisp");
            if (cerdispProcess == null)
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\CERDISP", true);
                //RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE", true);
                //reg.DeleteSubKey(@"CERDISP");
                if (reg == null)
                {
                    reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE", true);
                    reg.CreateSubKey(@"CERDISP");
                    reg = reg.OpenSubKey(@"CERDISP", true);
                }
                var serverIP = ConnectionState.GetServerIp();
                var hostname = (string)reg.GetValue("Hostname", "");
                if (serverIP != "" && hostname != serverIP)
                {
                    reg.SetValue("Hostname", serverIP, RegistryValueKind.String);
                }

                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                           @"\cerdisp.exe"))
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                               @"\cerdisp.exe", "-c");
                    Shared.SaveToLogInformation(@"RDP запущен по адресу " + serverIP);
                    return true;
                }
                else
                {
                    Shared.SaveToLogError(@"RDP не запущен. Файл" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                               @"\cerdisp.exe не найден.");
                    return null;
                }
            }
            else
            {
                cerdispProcess.Kill();
                Shared.SaveToLogInformation("RDP остановлен");
                return false;
            }
        }

        public static OpenNETCF.ToolHelp.ProcessEntry GetProcessRunning(string processsName)
        {
            try
            {
                OpenNETCF.ToolHelp.ProcessEntry cerdispProcess = null;
                foreach (OpenNETCF.ToolHelp.ProcessEntry clsProcess in OpenNETCF.ToolHelp.ProcessEntry.GetProcesses())
                {
                    if (cerdispProcess == null && clsProcess.ExeFile.Contains(processsName))
                    {
                        cerdispProcess = clsProcess;
                    }
                }
                return cerdispProcess;
            }
            catch (Exception ex)
            {
#if OUTPUTDEBUGINFO
                MessageBox.Show(ex.Message);
#endif
                return null;
            }

        }

        public static bool IsInt(string s)
        {
            bool isInt = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsDigit(s[i]))
                {
                    isInt = false;
                    break;
                }
            }
            return isInt;
        }

    }
}