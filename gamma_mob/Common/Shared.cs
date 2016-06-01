using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gamma_mob.Properties;

namespace gamma_mob.Common
{
    public class Shared
    {
        static Shared()
        {
            LoadImages();
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
        }

        public static bool LastQueryCompleted { get; set; }

        public static ImageList ImgList { get; private set; }

        public static int PersonId { get; set; }
    }
}
