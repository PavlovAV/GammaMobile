using System.Drawing;

namespace gamma_mob.Common
{
    public static class GraphFuncs
    {
        public static SizeF TextSize(string text, Font font)
        {
            SizeF size;
            using (var graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                size = graphics.MeasureString(text, font);
            }
            return size;
        }
    }
}
