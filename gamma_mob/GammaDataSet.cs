namespace gamma_mob {
    
    
    public partial class GammaDataSet {
        partial class GroupPacksDataTable
        {
        }
    
        partial class DocMobGroupPackOrderGroupPacksDataTable
        {
            public DocMobGroupPackOrderGroupPacksRow FindByBarcode(string Barcode)
            {
                foreach (DocMobGroupPackOrderGroupPacksRow Row in this.Rows)
                {
                    if (Row.RowState == System.Data.DataRowState.Deleted) continue;
                    if (Row.Barcode != null && Row.Barcode == Barcode) return Row;
                }
                return null;
            }
        }
        public static string ConnectionString = "Data Source=Gamma;Initial Catalog=Gamma;Persist Security Info=True;User ID=sa;Password=asutp1;Connect Timeout=5;";
    }
}
