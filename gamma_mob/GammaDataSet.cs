using System.Data.SqlClient;
using System.Windows.Forms;
using gamma_mob.GammaDataSetTableAdapters;


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

        public static string ConnectionString { get; private set; }
        public static void SetConnectionString(string ipAddress, string database, string user, string password, string timeout)
        {
            ConnectionString = "Data Source="+ipAddress+";Initial Catalog="+database+"" +
                               ";Persist Security Info=True;User ID="+user+"" +
                               ";Password="+password+";Connect Timeout=" + timeout;
            
        }
        public static int CheckSQLConnection()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    return ex.Class == 14 ? 1 : 2;
                }
            }
            return 0;
        }
    }

}

namespace gamma_mob.GammaDataSetTableAdapters
{
    public partial class DocMobGroupPackOrderGroupPacksTableAdapter
    {
        public string ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }
    }

    public partial class vGroupPackOrdersTableAdapter
    {
        public string ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }
    }
    
    public partial class DocOrders
    {
        public string ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }
    }

}