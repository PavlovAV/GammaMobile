using System.Windows.Forms;

namespace gamma_mob.Common
{
    public static class Extensions
    {
        public static void UnselectAll(this DataGrid grid)
        {
            for (int i = 0; i < grid.BindingContext[grid.DataSource].Count; i++)
            {
                grid.UnSelect(i);
            }
        }
    }
}