using System;
using System.Data;
using System.Data.SqlClient;


namespace gamma_mob
{
    public class GroupPackInfo
    {
        public string NomenclatureName = "";
        public int GrossWeight = 0;
        public int Weight = 0;
        public string BarCode = "";
        public Int64 GroupPackId = 0;
        public bool Connected = false;
        public bool Found = false;
    }

    public class SqlWork
    {
        public SqlWork()
        {
            ConString = GammaDataSet.ConnectionString;
            //ConString = "Data Source=Gamma;Initial Catalog=Gamma;Persist Security Info=True;User ID=sa;Password=asutp1;Connect Timeout=5;";
        }

        private string ConString {get; set;}

        public GroupPackInfo GetGroupPackInfo(string barCode)
        {
            if (!ConnectionState.CheckConnection()) return DefaultGroupPackInfo(barCode);
            const string sql = "SELECT GroupPackID, BarCode, Nomenclature, Weight, GrossWeight FROM vGroupPacks WHERE BarCode = @BarCode";
            using (var connection = new SqlConnection(ConString))
            {
                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@BarCode", SqlDbType.VarChar);
                command.Parameters["@BarCode"].Value = barCode;
                try
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    var groupPackInfo = new GroupPackInfo();
                    if (reader.Read())
                    { 
                        groupPackInfo.NomenclatureName = reader["Nomenclature"].ToString();
                        groupPackInfo.BarCode = reader["BarCode"].ToString();
                        groupPackInfo.Weight = (Int32)reader["Weight"];
                        groupPackInfo.GrossWeight = (Int32)reader["GrossWeight"];
                        groupPackInfo.GroupPackId = (Int64)reader["GroupPackID"];
                        groupPackInfo.Found = true;
                        groupPackInfo.Connected = true;
                    }
                    else
                    {
                        groupPackInfo.Found = false;
                        groupPackInfo.Connected = true;
                        groupPackInfo.BarCode = barCode;
                        groupPackInfo.NomenclatureName = "";
                        groupPackInfo.Weight = 0;
                        groupPackInfo.GrossWeight = 0;
                        groupPackInfo.GroupPackId = 0;
                    }                 
                    return groupPackInfo;
                }
                catch
                {
                    return DefaultGroupPackInfo(barCode);
                }
                
            }
        }

        private static GroupPackInfo DefaultGroupPackInfo(string barCode)
        {
            var groupPackInfo = new GroupPackInfo
                {
                    BarCode = barCode,
                    NomenclatureName = "",
                    Weight = 0,
                    GrossWeight = 0,
                    GroupPackId = 0,
                    Connected = false,
                    Found = false
                };
            return groupPackInfo;
        }
    }

    

    
}
