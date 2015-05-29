using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace gamma_mob
{
    public partial class OrderInfo : Form
    {
        private readonly DataTable _tableOrderInfo = new DataTable();

        private OrderInfo()
        {
            InitializeComponent();
        }

        public OrderInfo(Int64 docMobGroupPackOrderId)
            : this()
        {
            const string sql = "SELECT Nomenclature, Count(*) AS NumGroupPacks, " +
                               "SUM(Weight) AS Weight, SUM(GrossWeight) AS GrossWeight " +
                               "FROM vGroupPackOrders " +
                               "WHERE DocMobGroupPackOrderID = @DocMobGroupPackOrderID GROUP BY Nomenclature";
            using (var connection = new SqlConnection((GammaDataSet.ConnectionString)))
            {
                var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.Add("@DocMobGroupPackOrderID", SqlDbType.BigInt);
                cmd.Parameters["@DocMobGroupPackOrderID"].Value = docMobGroupPackOrderId;
                try
                {
                    connection.Open();
                    _tableOrderInfo.Load(cmd.ExecuteReader());
                }
                catch (SqlException)
                {
                    MessageBox.Show(@"Связь была прервана во время получения информации");
                    Close();
                }
                finally
                {
                    connection.Close();
                }
                gridOrderInfo.DataSource = _tableOrderInfo;
            }
        }
    }
}