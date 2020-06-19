using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Models;
using gamma_mob.Properties;
using System;
using System.Linq;

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

        public static BindingList<ChooseNomenclatureItem> Barcodes1C { get; set; }
        public static DateTime LastTimeBarcodes1C { get; set; }
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
    }
}