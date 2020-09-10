using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Models;
using gamma_mob.Properties;
using System;
using System.Linq;
using System.IO;
using OpenNETCF.Windows.Forms;

using System.Runtime.InteropServices;

namespace gamma_mob.Common
{
    public class Shared
    {
        private static List<Warehouse> _warehouses;

        static Shared()
        {
            LoadImages();
        }

        public static bool LastQueryCompleted { get; set; }

        public static ImageList ImgList { get; private set; }

        public static Guid PersonId { get; set; }
        public static String PersonName { get; set; }

        public static byte ShiftId { get; set; }

        public static int PlaceId { get; set; }

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
        public static int? MaxAllowedPercentBreak 
        { 
            get
            {
                if (_maxAllowedPercentBreak == null)
                {
                    var maxAllowedPercentBreak = Db.GetProgramSettings("MaxAllowedPercentBreakInDocOrder");
                    if (maxAllowedPercentBreak == null) return null;
                    _maxAllowedPercentBreak = Convert.ToInt32(maxAllowedPercentBreak);
                }
                return _maxAllowedPercentBreak;
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

        private static BindingList<ChooseNomenclatureItem> _barcodes1C { get; set; }

        public static BindingList<ChooseNomenclatureItem> Barcodes1C 
        { 
            get
            {
                if (_barcodes1C == null)
                {
                    var lastTimeBarcodes1C = Db.GetServerDateTime();
                    var list = Db.GetBarcodes1C();
                    if (list == null) return null;
                    LastTimeBarcodes1C = lastTimeBarcodes1C;
                    _barcodes1C = list;
                }
                return _barcodes1C;
            } 
        }

        private static DateTime LastTimeBarcodes1C { get; set; }
        //private static BindingList<ChooseNomenclatureItem> Barcodes1CChanges { get; set; }
        
        public static void RefreshBarcodes1C()
        {
            if (Db.CheckSqlConnection() == 0)
            {
                var Barcodes1CChanges = Db.GetBarcodes1CChanges(Shared.LastTimeBarcodes1C);
                if (Barcodes1CChanges != null)
                {
                    LastTimeBarcodes1C = Db.GetServerDateTime();
                    foreach (var item in Barcodes1CChanges)
                    {
                        var ItemDel = Barcodes1C.Where(b => b.BarcodeId == item.BarcodeId).FirstOrDefault();
                        if (ItemDel != null) Barcodes1C.Remove(ItemDel);
                        Shared.Barcodes1C.Add(item);
                    }
                }
            }
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

        public static bool IntializationData()
        {
            return !(Shared.Barcodes1C == null || Shared.Warehouses == null || Shared.PlaceZones == null || Shared.MaxAllowedPercentBreak == null);
        }

        public static string _logFile { get; private set; }

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
            _logFile = Path.Combine(Application2.StartupPath + @"\", string.Format("{0:yyyMMdd}.log", DateTime.Now));
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
            }
            
        }

        public static void SaveToLog(string log)
        {
            
            try
            {
                if (!IsLocalDateTimeUpdated) 
                    _logFile = Application2.StartupPath + @"\_NoDate.log";
                TextWriter swFile = new StreamWriter(new FileStream(_logFile,
                               FileMode.Append),System.Text.Encoding.ASCII);
                swFile.WriteLine(string.Format(@"{0:yyyy.MM.dd HH:mm:ss} : ", DateTime.Now) + log);
                swFile.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}