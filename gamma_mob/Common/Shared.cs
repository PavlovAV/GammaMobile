using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Models;
using gamma_mob.Properties;
using System;
using System.Linq;
using System.IO;
using OpenNETCF.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using Datalogic.API;

namespace gamma_mob.Common
{
    public class Shared
    {
        private static List<Warehouse> _warehouses;

        static Shared()
        {
            LoadImages();
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
                    if (value == false)
                    {
                        ConnectionState.StartChecker();
                    }
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

        private static int? _maxAllowedPercentBreak {get; set;}
        public static int MaxAllowedPercentBreak 
        { 
            get
            {
                if (_maxAllowedPercentBreak == null)
                {
                    var maxAllowedPercentBreak = Db.GetProgramSettings("MaxAllowedPercentBreakInDocOrder");
                    if (maxAllowedPercentBreak != null) 
                        _maxAllowedPercentBreak = Convert.ToInt32(maxAllowedPercentBreak);
                }
                return _maxAllowedPercentBreak ?? 0;
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
                    if (timerPeriodForUnloadOfflineProducts != null) 
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
                    if (timerPeriodForBarcodesUpdate != null) 
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
                    if (timerPeriodForCheckBatteryLevel != null)
                        _timerPeriodForCheckBatteryLevel = Convert.ToInt32(timerPeriodForCheckBatteryLevel);
                }
                return _timerPeriodForCheckBatteryLevel ?? 540000;
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
                    if (minLengthProductBarcode != null)
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
            var n = DateTime.Now.ToString();
            System.Diagnostics.Debug.Write(n + " !!!!!RefreshBarcodes1CFromTimer(" + _refreshBarcodes1CRunning.ToString() + ")!" + Environment.NewLine);
            if (_refreshBarcodes1CRunning) return;
            lock (lockerForBarcodesUpdate)
            {
                _refreshBarcodes1CRunning = true;
                System.Diagnostics.Debug.Write(n + " !!!!!Barcodes1C.UpdateBarcodes(" + _refreshBarcodes1CRunning.ToString() + ")!" + Environment.NewLine);
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
            System.Diagnostics.Debug.Write(n + " !!!!!UnloadOfflineProductsFromTimer(" + _unloadOfflineProductsRunning.ToString() + ")!" + Environment.NewLine);
            if (!Shared.IsExistsUnloadOfflineProducts || _unloadOfflineProductsRunning || !ConnectionState.IsConnected) return;
            lock (lockerForUnloadOfflineProducts)
            {
                _unloadOfflineProductsRunning = true;
                System.Diagnostics.Debug.Write(n + " !!!!!UnloadOfflineProducts(" + _unloadOfflineProductsRunning.ToString() + ")!" + Environment.NewLine);
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
                    if (list == null) return null;
                    _placeZones = list;
                }
                return _placeZones;
            }
        }

        public static bool InitializationData()
        {
            Shared.DeleteOldUploadedToServerLogs();
            return !(Shared.Barcodes1C == null || Shared.PlaceZones == null || Shared.Warehouses == null || Shared.PlaceZones == null || Shared.Barcodes1C.UpdateBarcodes(true) == null/*Shared.Barcodes1C.InitCountBarcodes() == null ||*/
                || Shared.MaxAllowedPercentBreak == null || Shared.TimerPeriodForBarcodesUpdate == null || Shared.TimerPeriodForUnloadOfflineProducts == null
                || Shared.ScannedBarcodes == null );// // Shared.TimerForBarcodesUpdate == null);
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

        public static void SaveToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool isUploaded, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity)
        {

            try
            {
                Db.AddMessageToLog(scanId, dateScanned, barcode, placeId, placeZoneId, docTypeId, docId, isUploaded, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity);
            }
            catch (Exception err)
            {
                //MessageBox.Show(err.Message);
            }
        }

        public static void SaveToLog(Guid scanId, bool isUploaded, string log)
        {

            try
            {
                Db.AddMessageToLog(scanId, isUploaded, log);
            }
            catch (Exception err)
            {
               // MessageBox.Show(err.Message);
            }
        }

        public static void SaveToLog(Guid scanId, bool isUploaded, bool isDeleted, string log)
        {

            try
            {
                Db.AddMessageToLog(scanId, isUploaded, isDeleted, log);
            }
            catch (Exception err)
            {
                //MessageBox.Show(err.Message);
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
        public static void SaveToLog(string log)
        {

            try
            {
                Db.AddMessageToLog(log);
            }
            catch (Exception err)
            {
               // MessageBox.Show(err.Message);
            }
        }

        public static void DeleteOldUploadedToServerLogs()
        {

            try
            {
                Db.DeleteOldUploadedToServerLogs();
            }
            catch (Exception err)
            {
               // MessageBox.Show(err.Message);
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
                Shared.SaveToLog("Battery Level " + Device.GetBatteryLevel().ToString());
            }
            catch
            {
            }
        }
        
    }
}