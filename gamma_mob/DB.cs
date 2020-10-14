using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.Data.SqlServerCe;
using System.IO;
using System.Net;
using OpenNETCF.Windows.Forms;
using OpenNETCF.ComponentModel;

namespace gamma_mob
{
    public static class Db
    {
        private static string ConnectionString { get; set; }

        private static string _deviceName  { get; set; }
        private static string deviceName
        {
            get
            {
                if (_deviceName == null || _deviceName == String.Empty)
                {
                    try
                    {
                        _deviceName = Datalogic.API.Device.GetSerialNumber();
                    }
                    catch
                    {
                        _deviceName = "Error";
                    }
                }
                return _deviceName;
            }
        }

        private static string _deviceIP { get; set; }
        private static string deviceIP
        {
            get
            {
                if (_deviceIP == null || _deviceIP == String.Empty)
                {
                    try
                    {
                        IPHostEntry ipEntry = Dns.GetHostByName(Dns.GetHostName());
                        IPAddress[] addr = ipEntry.AddressList;

                        _deviceIP = addr[0].ToString();
                    }
                    catch
                    {
                        _deviceIP =  "000.000.000.000";
                    }
                }
                return _deviceIP;
            }
        }

        private static bool IsNotFirstGetConnectionCeString { get; set; }
        private static string ConnectionCeString 
        {
            get
            {
                var dbFile = @"\GammaDB.sdf";//Application2.StartupPath + @"\GammaDB.sdf";
                if (!IsNotFirstGetConnectionCeString)
                {
                    if (!File.Exists(dbFile))
                    {
                        SqlCeEngine empEngine = new SqlCeEngine(@"Data Source = " + dbFile);
                        empEngine.CreateDatabase();
                    }
                    SqlCeConnection empCon;
                    SqlCeCommand empCom;
                    empCon = new SqlCeConnection(@"Data Source = " + dbFile);
                    empCon.Open();
                    empCom = empCon.CreateCommand();
                    string strQuery;
                    if (TableCeExists(empCon, "ScannedBarcodes"))
                    {
                        strQuery = "DROP TABLE ScannedBarcodes";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                    }
                    if (!TableCeExists(empCon, "Settings"))
                    {
                        strQuery = "CREATE TABLE Settings (LastUpdatedTimeBarcodes DateTime)";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                        strQuery = "INSERT INTO Settings (LastUpdatedTimeBarcodes) VALUES (DATEADD(HOUR,-1,GETDATE()))";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                    }

                    if (!TableCeExists(empCon, "Logs"))
                    {
                        strQuery = "CREATE TABLE Logs (LogId uniqueidentifier default newid() PRIMARY KEY,LogDate DateTime default GetDate(),Log nvarchar(1000),Barcode nvarchar(100), UserName nvarchar(250), PersonId uniqueidentifier, PlaceId int, DocTypeId int, IsUploaded bit, DocId uniqueidentifier, PlaceZoneId uniqueidentifier, ToDelete bit default (0), IsDeleted bit default (0), ProductId uniqueidentifier, ProductKindId int, NomenclatureId uniqueidentifier, CharacteristicId uniqueidentifier, QualityId uniqueidentifier, Quantity int, IsUploadedToServer bit default (0))";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                        strQuery = "CREATE INDEX IX_LogId ON Logs (LogId ASC)";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empCom.CommandText = strQuery;
                        empCom.Parameters.Add("@Log", DbType.String).Value = @"Создана " + dbFile;
                        empCom.ExecuteNonQuery();
                        
                    }

                    if (!TableCeExists(empCon, "Barcodes"))
                    {
                        strQuery = "CREATE TABLE Barcodes (Barcode nvarchar(100), Name nvarchar(600), NomenclatureID uniqueidentifier, CharacteristicID uniqueidentifier, QualityID uniqueidentifier, MeasureUnitID uniqueidentifier, BarcodeID uniqueidentifier, Number nvarchar(50), KindId tinyint)";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                        strQuery = "CREATE INDEX IX_Barcode ON Barcodes (Barcode ASC)";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                        strQuery = "CREATE INDEX IX_BarcodeId ON Barcodes (BarcodeId)";
                        empCom.CommandText = strQuery;
                        empCom.ExecuteNonQuery();
                        
                        //Db.GetBarcodes1C();
                    }

                    //if (!ColumnCeExists(empCon, "ScannedBarcodes", "DocId"))
                    //{
                    //    strQuery = "ALTER TABLE ScannedBarcodes ADD DocId uniqueidentifier";
                    //    empCom.CommandText = strQuery;
                    //    empCom.ExecuteNonQuery();
                    //}                    
                    //strQuery = "INSERT INTO ScannedBarcodes (Barcode) VALUES (@Barcode)";
                    //empCom.CommandText = strQuery;
                    //empCom.Parameters.AddWithValue("@Barcode", @"1234567");
                    //empCom.ExecuteNonQuery();
                    empCon.Close();
                    IsNotFirstGetConnectionCeString = true;
                }
                return @"Data Source = " + dbFile;
            }
            
        }

        public static void SetConnectionString(string ipAddress, string database, string user, string password,
                                               string timeout)
        {
            ConnectionString = "Application Name=mob_gamma v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + ";Data Source=" + ipAddress + ";Initial Catalog=" + database + "" +
                               ";Persist Security Info=True;User ID=" + user + "" +
                               ";Password=" + password + ";Connect Timeout=" + timeout;
        }

        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        public static int CheckSqlConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString)) return 1;
            if (!ConnectionState.CheckConnection()) return 1;
            using (var connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    if (!Shared.IsLocalDateTimeUpdated)
                    {
                        DateTime serverDateTime = Db.GetServerDateTime();
                        if (serverDateTime != null)
                        {
                            Shared.SetSystemDateTime(serverDateTime);
                        }
                    }
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    return ex.Class == 14 ? 1 : 2;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
            
            return 0;
        }

        private static bool TableCeExists(SqlCeConnection connection, string tableName)
        {
            using (var command = new SqlCeCommand())
            {
                command.Connection = connection;
                var sql = string.Format(
                        "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{0}'",
                         tableName);
                command.CommandText = sql;
                var count = Convert.ToInt32(command.ExecuteScalar());
                return (count > 0);
            }
        }

        private static bool ColumnCeExists(SqlCeConnection connection, string tableName, string columnName)
        {
            using (var command = new SqlCeCommand())
            {
                command.Connection = connection;
                var sql = string.Format(
                        "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = '{0}' AND column_name = '{1}'",
                         tableName, columnName);
                command.CommandText = sql;
                var count = Convert.ToInt32(command.ExecuteScalar());
                return (count > 0);
            }
        }

        public static DateTime GetLastUpdatedTimeBarcodes()
        {
            DateTime lastDateTime = new DateTime();
            const string sql = "SELECT LastUpdatedTimeBarcodes FROM Settings";
            var parameters = new List<SqlCeParameter>();
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    lastDateTime = Convert.ToDateTime(row["LastUpdatedTimeBarcodes"]);
                }
            }
            return lastDateTime;
        }

        public static bool UpdateLastUpdatedTimeBarcodes(DateTime date)
        {
            const string sql = "UPDATE Settings SET LastUpdatedTimeBarcodes = @Date";

            var parameters = new List<SqlCeParameter>();
            SqlCeParameter p = new SqlCeParameter();
            p.ParameterName = "@Date";
            p.DbType = DbType.DateTime;
            p.Value = date;
            parameters.Add(p);
            
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }


        public static bool UpdateBarcodes1C(CashedBarcode item)
        {
            
            {
                
                switch (item.TypeChange)
                {
                    case 2:
                        var parameters2 = new List<SqlCeParameter>();

                        SqlCeParameter p26 = new SqlCeParameter();
                        p26.ParameterName = "@BarcodeID";
                        p26.SqlDbType = SqlDbType.UniqueIdentifier;
                        p26.Value = item.BarcodeId;
                        parameters2.Add(p26);

                        string sql2 = "DELETE Barcodes WHERE BarcodeId = @BarcodeID";
                        return ExecuteCeNonQuery(sql2, parameters2, CommandType.Text);

                        break;
                    case 0:

                        var parameters0 = new List<SqlCeParameter>();

                        SqlCeParameter p0 = new SqlCeParameter();
                        p0.ParameterName = "@Barcode";
                        p0.DbType = DbType.String;
                        p0.Value = item.Barcode;
                        parameters0.Add(p0);
                        SqlCeParameter p01 = new SqlCeParameter();
                        p01.ParameterName = "@Name";
                        p01.DbType = DbType.String;
                        p01.Value = item.Name;
                        parameters0.Add(p01);
                        SqlCeParameter p02 = new SqlCeParameter();
                        p02.ParameterName = "@NomenclatureID";
                        p02.SqlDbType = SqlDbType.UniqueIdentifier;
                        p02.Value = item.NomenclatureId;
                        parameters0.Add(p02);
                        SqlCeParameter p03 = new SqlCeParameter();
                        p03.ParameterName = "@CharacteristicID";
                        p03.SqlDbType = SqlDbType.UniqueIdentifier;
                        p03.Value = item.CharacteristicId;
                        parameters0.Add(p03);
                        SqlCeParameter p04 = new SqlCeParameter();
                        p04.ParameterName = "@QualityID";
                        p04.SqlDbType = SqlDbType.UniqueIdentifier;
                        p04.Value = item.QualityId;
                        parameters0.Add(p04);
                        SqlCeParameter p05 = new SqlCeParameter();
                        p05.ParameterName = "@MeasureUnitID";
                        p05.SqlDbType = SqlDbType.UniqueIdentifier;
                        p05.Value = item.MeasureUnitId;
                        parameters0.Add(p05);
                        SqlCeParameter p06 = new SqlCeParameter();
                        p06.ParameterName = "@BarcodeID";
                        p06.SqlDbType = SqlDbType.UniqueIdentifier;
                        p06.Value = item.BarcodeId;
                        parameters0.Add(p06);
                        SqlCeParameter p07 = new SqlCeParameter();
                        p07.ParameterName = "@Number";
                        p07.DbType = DbType.String;
                        p07.Value = item.Number;
                        parameters0.Add(p07);
                        SqlCeParameter p08 = new SqlCeParameter();
                        p08.ParameterName = "@KindID";
                        p08.DbType = DbType.Int16;
                        p08.Value = (item.KindId as object) ?? DBNull.Value;
                        parameters0.Add(p08);
                        string sql0 = "INSERT INTO Barcodes (Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID, Number, KindId) VALUES (@Barcode, @Name, @NomenclatureID, @CharacteristicID, @QualityID, @MeasureUnitID, @BarcodeID, @Number, @KindID)";

                        return ExecuteCeNonQuery(sql0, parameters0, CommandType.Text);
                        break;
                    case 1:

                        var parameters1 = new List<SqlCeParameter>();

                        SqlCeParameter p = new SqlCeParameter();
                        p.ParameterName = "@Barcode";
                        p.DbType = DbType.String;
                        p.Value = item.Barcode;
                        parameters1.Add(p);
                        SqlCeParameter p1 = new SqlCeParameter();
                        p1.ParameterName = "@Name";
                        p1.DbType = DbType.String;
                        p1.Value = item.Name;
                        parameters1.Add(p1);
                        SqlCeParameter p2 = new SqlCeParameter();
                        p2.ParameterName = "@NomenclatureID";
                        p2.SqlDbType = SqlDbType.UniqueIdentifier;
                        p2.Value = item.NomenclatureId;
                        parameters1.Add(p2);
                        SqlCeParameter p3 = new SqlCeParameter();
                        p3.ParameterName = "@CharacteristicID";
                        p3.SqlDbType = SqlDbType.UniqueIdentifier;
                        p3.Value = item.CharacteristicId;
                        parameters1.Add(p3);
                        SqlCeParameter p4 = new SqlCeParameter();
                        p4.ParameterName = "@QualityID";
                        p4.SqlDbType = SqlDbType.UniqueIdentifier;
                        p4.Value = item.QualityId;
                        parameters1.Add(p4);
                        SqlCeParameter p5 = new SqlCeParameter();
                        p5.ParameterName = "@MeasureUnitID";
                        p5.SqlDbType = SqlDbType.UniqueIdentifier;
                        p5.Value = item.MeasureUnitId;
                        parameters1.Add(p5);
                        SqlCeParameter p6 = new SqlCeParameter();
                        p6.ParameterName = "@BarcodeID";
                        p6.SqlDbType = SqlDbType.UniqueIdentifier;
                        p6.Value = item.BarcodeId;
                        parameters1.Add(p6);
                        SqlCeParameter p7 = new SqlCeParameter();
                        p7.ParameterName = "@Number";
                        p7.DbType = DbType.String;
                        p7.Value = item.Number;
                        parameters1.Add(p7);
                        SqlCeParameter p8 = new SqlCeParameter();
                        p8.ParameterName = "@KindID";
                        p8.DbType = DbType.Int16;
                        p8.Value = (item.KindId as object) ?? DBNull.Value;
                        parameters1.Add(p8);
                        string sql1 = "UPDATE Barcodes SET Barcode = @Barcode, Name = @Name, NomenclatureID = @NomenclatureID, CharacteristicID = @CharacteristicID, QualityID = @QualityID, MeasureUnitID = @MeasureUnitID, BarcodeID = @BarcodeID, Number = @Number, KindId = @KindID  WHERE BarcodeId = @BarcodeID";

                        return ExecuteCeNonQuery(sql1, parameters1, CommandType.Text);
                        break;
                }

            }
            return false;
        }

        public static bool AddMessageToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool isUploaded, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity)
        {
            string sql = (Db.ExistsMessageLogFromLogId(scanId))
                    ? "UPDATE Logs SET LogDate = @DateScanned, Barcode = @Barcode, PlaceId = @PlaceId, PlaceZoneId = @PlaceZoneId, DocTypeId = @DocTypeId, DocId = @DocId, IsUploaded = @IsUploaded, ProductId = @ProductId, ProductKindId = @ProductKindId, NomenclatureId = @NomenclatureId, CharacteristicId = @CharacteristicId, QualityId = @QualityId, Quantity = @Quantity WHERE LogId = @ScanId"
                    : "INSERT INTO Logs (LogId, LogDate, UserName, PersonId, Barcode, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded, ProductId, ProductKindId, NomenclatureId, CharacteristicId, QualityId, Quantity) VALUES (@ScanId, @DateScanned, @UserName, @PersonId, @Barcode, @PlaceId, @PlaceZoneId, @DocTypeId, @DocId, @IsUploaded, @ProductId, @ProductKindId, @NomenclatureId, @CharacteristicId, @QualityId, @Quantity)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@ScanId", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        },
                    new SqlCeParameter("@DateScanned", DbType.DateTime)
                        {
                            Value = dateScanned
                        },
                    new SqlCeParameter("@UserName", DbType.String)
                        {
                            Value = Settings.UserName
                        },
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        },                    
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        },
                    new SqlCeParameter("@PlaceId", DbType.Int16)
                        {
                            Value = placeId
                        },
                    new SqlCeParameter("@PlaceZoneId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (placeZoneId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@DocTypeId", DbType.Int16)
                        {
                            Value = docTypeId
                        },
                    new SqlCeParameter("@DocId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (docId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@IsUploaded", DbType.Boolean)
                        {
                            Value = isUploaded
                        },
                    new SqlCeParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (productId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@ProductKindId", SqlDbType.Int)
                        {
                            Value = (productKindId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@NomenclatureId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (nomenclatureId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@CharacteristicId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (characteristicId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@QualityId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (qualityId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@Quantity", SqlDbType.Int)
                        {
                            Value = (quantity as object) ?? DBNull.Value
                        }

                };
            
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }

        public static bool AddMessageToLog(Guid scanId, bool isUploaded, string log)
        {
            string sql = (Db.ExistsMessageLogFromLogId(scanId))
                ? log == string.Empty ? "UPDATE Logs SET IsUploaded = @IsUploaded WHERE LogId = @ScanId" : "UPDATE Logs SET IsUploaded = @IsUploaded, Log = @Log WHERE LogId = @ScanId"
                    : "INSERT INTO Logs (LogId, IsUploaded, Log, UserName, PersonId) VALUES (@ScanId, @IsUploaded, @Log, @UserName, @PersonId)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@ScanId", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        },
                    new SqlCeParameter("@IsUploaded", DbType.Boolean)
                        {
                            Value = isUploaded
                        },
                    new SqlCeParameter("@Log", DbType.String)
                        {
                            Value = (log as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@UserName", DbType.String)
                        {
                            Value = Settings.UserName
                        },
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        }
                    

                };

            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }

        public static bool AddMessageToLog(Guid scanId, bool isUploaded, bool isDeleted, string log)
        {
            string sql = (Db.ExistsMessageLogFromLogId(scanId))
                ? log == string.Empty ? "UPDATE Logs SET IsUploaded = @IsUploaded, IsDeleted = @IsDeleted, ToDelete = CASE WHEN @IsDeleted = 1 AND ToDelete = 0 THEN 1 ELSE ToDelete END WHERE LogId = @ScanId" : "UPDATE Logs SET IsUploaded = @IsUploaded, IsDeleted = @IsDeleted, ToDelete = CASE WHEN @IsDeleted = 1 AND ToDelete = 0 THEN 1 ELSE ToDelete END, Log = @Log WHERE LogId = @ScanId"
                    : "INSERT INTO Logs (LogId, IsUploaded, IsDeleted, Log, UserName, PersonId) VALUES (@ScanId, @IsUploaded, @IsDeleted, @Log, @UserName, @PersonId)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@ScanId", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        },
                    new SqlCeParameter("@IsUploaded", DbType.Boolean)
                        {
                            Value = isUploaded
                        },
                    new SqlCeParameter("@IsDeleted", DbType.Boolean)
                        {
                            Value = isDeleted
                        },
                    new SqlCeParameter("@Log", DbType.String)
                        {
                            Value = (log as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@UserName", DbType.String)
                        {
                            Value = Settings.UserName
                        },
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        }
                    

                };

            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }

        public static bool AddMessageToLog(string log)
        {
            const string sql = "INSERT INTO Logs (Log, UserName, PersonId) VALUES (@log, @UserName, @PersonId)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Log", DbType.String)
                        {
                            Value = (log as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@UserName", DbType.String)
                        {
                            Value = Settings.UserName
                        },
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        }
                    

                };
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }

        public static List<ScannedBarcode> GetBarcodesForCurrentUser()
        {
            List<ScannedBarcode> list = null;
            const string sql = "SELECT LogId, LogDate, Barcode, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded, ToDelete, IsDeleted, ProductId, ProductKindId, NomenclatureId, CharacteristicId, QualityId, Quantity FROM Logs WHERE LogDate >= DATEADD(DAY,-4,GETDATE()) AND PersonId = @PersonId AND Barcode IS NOT NULL ORDER BY LogDate";
            
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        }
                };

            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<ScannedBarcode>();
                    foreach (DataRow row in table.Rows)
                    {
                        var t = row["IsUploaded"].ToString();
                        var item = new ScannedBarcode
                            {
                                Barcode = row["Barcode"].ToString(),
                                ScanId = new Guid(row["LogID"].ToString()),
                                PlaceId = row.IsNull("PlaceId") ? -1 : Convert.ToInt32(row["PlaceId"]),
                                PlaceZoneId = row.IsNull("PlaceZoneId") ? (Guid?)null : new Guid(row["PlaceZoneId"].ToString()),
                                DocTypeId = row.IsNull("DocTypeId") ? -1 : Convert.ToInt32(row["DocTypeId"]),
                                DocId = row.IsNull("DocId") ? (Guid?)null : new Guid(row["DocId"].ToString()),
                                IsUploaded = row.IsNull("IsUploaded") ? false : row["IsUploaded"].ToString() == "True" ? true : false,
                                ToDelete = row.IsNull("ToDelete") ? false : row["ToDelete"].ToString() == "True" ? true : false,
                                IsDeleted = row.IsNull("IsDeleted") ? false : row["IsDeleted"].ToString() == "True" ? true : false,
                                DateScanned = Convert.ToDateTime(row["LogDate"]),
                                ProductId = row.IsNull("ProductId") ? (Guid?)null : new Guid(row["ProductId"].ToString()),
                                ProductKindId = Convert.ToInt32(row["ProductKindId"]),
                                NomenclatureId = row.IsNull("NomenclatureId") ? (Guid?)null : new Guid(row["NomenclatureId"].ToString()),
                                CharacteristicId = row.IsNull("CharacteristicId") ? (Guid?)null : new Guid(row["CharacteristicId"].ToString()),
                                QualityId = row.IsNull("QualityId") ? (Guid?)null : new Guid(row["QualityId"].ToString()),
                                Quantity = row.IsNull("Quantity") ? (int?)null : Convert.ToInt32(row["Quantity"]),
                            };
                        list.Add(item);
                    };
                    /*
                    list = (from DataRow row in table.Rows
                            select new ScannedBarcode
                            {
                                Barcode = row["Barcode"].ToString(),
                                ScanId = new Guid(row["LogID"].ToString()),
                                PlaceId = row.IsNull("PlaceId") ? -1 : Convert.ToInt32(row["PlaceId"]),
                                PlaceZoneId = row.IsNull("PlaceZoneId") ? (Guid?)null : new Guid(row["PlaceZoneId"].ToString()),
                                DocTypeId = row.IsNull("DocTypeId") ? -1 : Convert.ToInt32(row["DocTypeId"]),
                                DocId = row.IsNull("DocId") ? (Guid?)null : new Guid(row["DocId"].ToString()),
                                IsUploaded = row.IsNull("LogDate") ? false : Convert.ToBoolean(row["LogDate"]),
                                DateScanned = Convert.ToDateTime(row["LogDate"]),
                                ProductId = row.IsNull("ProductId") ? (Guid?)null : new Guid(row["ProductId"].ToString()),
                                ProductKindId = Convert.ToInt32(row["ProductKindId"]),
                                NomenclatureId = row.IsNull("NomenclatureId") ? (Guid?)null : new Guid(row["NomenclatureId"].ToString()),
                                CharacteristicId = row.IsNull("CharacteristicId") ? (Guid?)null : new Guid(row["CharacteristicId"].ToString()),
                                QualityId = row.IsNull("QualityId") ? (Guid?)null : new Guid(row["QualityId"].ToString()),
                                Quantity = row.IsNull("Quantity") ? (int?)null : Convert.ToInt32(row["Quantity"]),
                            }).ToList();*/
                }
            }
            return list;
        }

        public static bool DeleteOldUploadedToServerLogs()
        {
            const string sql = "DELETE Logs WHERE IsUploadedToServer = 1 AND LogDate < DATEADD(DAY,-4,GETDATE())";
            return ExecuteCeNonQuery(sql, new List<SqlCeParameter>(), CommandType.Text);
        }
        
        public static void UploadLogToServer()
        {
            const string sqlCE = "SELECT LogId, LogDate, Log, Barcode, UserName, PersonId, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded, ToDelete, IsDeleted, ProductId, ProductKindId, NomenclatureId, CharacteristicId, QualityId, Quantity FROM Logs WHERE IsUploadedToServer = 0 ORDER BY LogDate";

            var parametersCE = new List<SqlCeParameter>();
                //{
                //    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                //        {
                //            Value = (Shared.PersonId as object) ?? DBNull.Value
                //        }
                //};

            using (DataTable table = ExecuteCeSelectQuery(sqlCE, parametersCE, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        const string sql = "INSERT INTO LogFromMobileDevices(DeviceName, DeviceIP, UserName,PersonId,LogId,LogDate,Log,Barcode,PlaceId,DocTypeId,IsUploaded,DocId,PlaceZoneId,ToDelete,IsDeleted,ProductId,ProductKindId,NomenclatureId,CharacteristicId,QualityId,Quantity) VALUES(@DeviceName, @DeviceIP, @UserName,@PersonId,@LogId,@LogDate,@Log,@Barcode,@PlaceId,@DocTypeId,@IsUploaded,@DocId,@PlaceZoneId,@ToDelete,@IsDeleted,@ProductId,@ProductKindId,@NomenclatureId,@CharacteristicId,@QualityId,@Quantity)";
                        var parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@DeviceName", SqlDbType.Text)
                                {
                                    Value = deviceName,//.ToString(),
                                },
                            new SqlParameter("@DeviceIP", SqlDbType.Text)
                                {
                                    Value = deviceIP,//.ToString(),
                                },
                            new SqlParameter("@UserName", SqlDbType.Text)
                                {
                                    Value = row["UserName"],//.ToString(),
                                },
                            new SqlParameter("@PersonId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["PersonId"],// row.IsNull("PersonId") ? (Guid?)null : new Guid(row["PersonId"].ToString()),
                                },
                            new SqlParameter("@LogId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["LogId"],//row.IsNull("LogId") ? (Guid?)null : new Guid(row["LogId"].ToString()),
                                },
                            new SqlParameter("@LogDate", SqlDbType.DateTime)
                                {
                                    Value = row["LogDate"],//row.IsNull("LogDate") ? (DateTime?)null : Convert.ToDateTime(row["LogDate"]),
                                },
                            new SqlParameter("@Log", SqlDbType.Text)
                                {
                                    Value = row["Log"].ToString(),
                                },
                            new SqlParameter("@Barcode", SqlDbType.Text)
                                {
                                    Value = row["Barcode"].ToString(),
                                },
                            new SqlParameter("@PlaceId", SqlDbType.Int)
                                {
                                    Value = row["PlaceId"],// row.IsNull("PlaceId") ? (int?)null : Convert.ToInt32(row["PlaceId"]),
                                },
                            new SqlParameter("@DocTypeId", SqlDbType.Int)
                                {
                                    Value = row["DocTypeId"],// row.IsNull("DocTypeId") ? (int?)null : Convert.ToInt32(row["DocTypeId"]),
                                }, 
                            new SqlParameter("@IsUploaded", SqlDbType.Bit)
                                {
                                    Value = row["IsUploaded"],// row.IsNull("IsUploaded") ? false : row["IsUploaded"].ToString() == "True" ? true : false,
                                },
                            new SqlParameter("@DocId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["DocId"],// row.IsNull("DocId") ? (Guid?)null : new Guid(row["DocId"].ToString()),
                                },
                            new SqlParameter("@PlaceZoneId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["PlaceZoneId"], // row.IsNull("PlaceZoneId") ? (Guid?)null : new Guid(row["PlaceZoneId"].ToString()),
                                },
                            new SqlParameter("@ToDelete", SqlDbType.Bit)
                                {
                                    Value = row["ToDelete"], // row.IsNull("ToDelete") ? false : row["ToDelete"].ToString() == "True" ? true : false,
                                },
                            new SqlParameter("@IsDeleted", SqlDbType.Bit)
                                {
                                    Value = row["IsDeleted"], // row.IsNull("IsDeleted") ? false : row["IsDeleted"].ToString() == "True" ? true : false,
                                },
                            new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["ProductId"], // row.IsNull("ProductId") ? (Guid?)null : new Guid(row["ProductId"].ToString()),
                                },
                            new SqlParameter("@ProductKindId", SqlDbType.Int)
                                {
                                    Value = row["ProductKindId"], // row.IsNull("ProductKindId") ? (int?)null : Convert.ToInt32(row["ProductKindId"]),
                                },
                            new SqlParameter("@NomenclatureId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["NomenclatureId"], // row.IsNull("NomenclatureId") ? (Guid?)null : new Guid(row["NomenclatureId"].ToString()),
                                },
                            new SqlParameter("@CharacteristicId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["CharacteristicId"], // row.IsNull("CharacteristicId") ? (Guid?)null : new Guid(row["CharacteristicId"].ToString()),
                                },
                            new SqlParameter("@QualityId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["QualityId"], // row.IsNull("QualityId") ? (Guid?)null : new Guid(row["QualityId"].ToString()),
                                },
                            new SqlParameter("@Quantity", SqlDbType.Int)
                                {
                                    Value = row["Quantity"], // row.IsNull("Quantity") ? (int?)null : Convert.ToInt32(row["Quantity"]),
                                }
                        };
                        if (ExecuteSilentlyNonQuery(sql, parameters, CommandType.Text))
                            {
                                var g = row.IsNull("LogId") ? "" : new Guid(row["LogId"].ToString()).ToString();
                                var sqlCEUpdate = "UPDATE Logs SET IsUploadedToServer = 1 WHERE LogId = '"+g+"'";
                                var r = ExecuteCeNonQuery(sqlCEUpdate, new List<SqlCeParameter>(), CommandType.Text);
                            };
                    }
                }
            }
            return;
        }

        public static bool AddScannedBarcode(string barcode)
        {
            const string sql = "INSERT INTO Logs (Log, Barcode, UserName, PersonId) VALUES (@Log, @Barcode, @UserName, @PersonId)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Log", DbType.String)
                        {
                            Value = ((@"Scan barcode " + barcode) as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = (barcode as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@UserName", DbType.String)
                        {
                            Value = Settings.UserName
                        },
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        } 
                }; 
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }

        public static bool AddScannedBarcode(string barcode, Guid scanId)
        {
            const string sql = "INSERT INTO Logs (Log,Barcode, LogId, UserName, PersonId) VALUES (@Log,@Barcode,@LogId, @UserName, @PersonId)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Log", DbType.String)
                        {
                            Value = ((@"Scan barcode " + barcode) as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = (barcode as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@LogId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (scanId as object) ?? DBNull.Value
                        } ,
                    new SqlCeParameter("@UserName", DbType.String)
                        {
                            Value = Settings.UserName
                        },
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        } 
                };
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text);
        }

        public static bool ExistsMessageLogFromLogId(Guid logId)
        {
            bool result = false;

            const string sql = "SELECT LogId FROM Logs WHERE LogId = @LogId";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@LogId", SqlDbType.UniqueIdentifier)
                        {
                            Value = logId
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = true;
                        //ScanCode нельзя, иначе пойдет запись в лог
                    //    new ScannedBarcode
                    //{
                    //    Barcode = table.Rows[0]["Barcode"].ToString(),
                    //    ScanId = new Guid(table.Rows[0]["LogId"].ToString()),
                    //    PlaceId = table.Rows[0].IsNull("PlaceId") ? (Guid?)null : new Guid(table.Rows[0]["PlaceId"].ToString()),
                    //    PlaceZoneId = table.Rows[0].IsNull("PlaceZoneId") ? null : new Guid(table.Rows[0]["PlaceZoneId"].ToString()),
                    //    DocTypeId = Convert.ToInt16(table.Rows[0]["DocTypeId"].ToString()),
                    //    DocId = table.Rows[0].IsNull("DocId") ? null : new Guid(table.Rows[0]["DocId"].ToString()),
                    //    IsUploaded = Convert.ToBoolean(table.Rows[0]["IsUploaded"]),
                    //    DateScanned = Convert.ToDateTime(table.Rows[0]["LogDate"])
                    //};
                }
            }
            return result;
        }

        /*
        public static List<ScannedBarcode> GetScannedBarcodes()
        {
            List<ScannedBarcode> list = null;
            const string sql = "SELECT LogId, Barcode, LogDate FROM Logs ORDER BY LogDate";
            using (DataTable table = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<ScannedBarcode>();
                    list.AddRange(from DataRow row in table.Rows
                                  select new ScannedBarcode
                                  {
                                      Barcode = row["Barcode"].ToString(),
                                      ScanId = row.IsNull("LogId") ? new Guid() : new Guid(row["LogId"].ToString()),
                                      DateScanned = Convert.ToDateTime(row["LogDate"].ToString())
                                  });
                }
            }
            return list;
        }
*/
        public static List<string> GetLogs()
        {
            List<string> list = null;
            const string sql = "SELECT LogId, Log, LogDate, UserId, PersonId FROM Logs ORDER BY LogDate";
            using (DataTable table = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<string>();
                    list.AddRange(from DataRow row in table.Rows
                                  select 
                                      row["Barcode"].ToString() +
                                      (row.IsNull("LogId") ? new Guid() : new Guid(row["LogId"].ToString())) +
                                      Convert.ToDateTime(row["LogDate"].ToString())
                                  );
                }
            }
            return list;
        }

        public static DateTime GetServerDateTime()
        {
            DateTime serverDateTime = new DateTime();
            const string sql = "SELECT GETUTCDATE() AS ServerDateTime";
            var parameters = new List<SqlParameter>();
            using (DataTable table = ExecuteSilentlySelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    serverDateTime = Convert.ToDateTime(row["ServerDateTime"]);
                }
            }
            return serverDateTime;
        }

        public static Person PersonByBarcode(string barcode)
        {
            Person person = null;
            const string sql = "mob_GetPersonFromBarcode";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@BarCode", SqlDbType.VarChar)
            };
            parameters[0].Value = barcode;
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    person = new Person
                    {
                        PersonID = new Guid(row["PersonID"].ToString()),
                        Name = row["Name"].ToString(),
                        UserName = row["UserName"].ToString()
                    };
                }
            }
            return person;
            
        }

        public static BindingList<ProductBase> DocShipmentOrderGoodProducts(Guid docShipmentOrderId, Guid nomenclatureId,
                                                             Guid characteristicId, Guid qualityId, DocDirection docDirection)
        {
            BindingList<ProductBase> list = null;
            const string sql = "dbo.mob_GetGoodProducts";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docShipmentOrderId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@IsOutDoc", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocOut
                        }
                };
            //DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            //return table;
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<ProductBase>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ProductBase
                        {
                            Number = row["Number"].ToString(),
                            Barcode = row["BarCode"].ToString(),
                            MovementId = row.IsNull("MovementID") ? Guid.Empty : new Guid(row["MovementID"].ToString()),
                            Date = Convert.ToDateTime(row["Date"]),
                            OutPlace = row["OutPlace"].ToString(),
                            InPlace = row["InPlace"].ToString(),
                            IsProductR = row.IsNull("IsProductR") ? false : Convert.ToBoolean(row["IsProductR"]),
                            Quantity = row["Quantity"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public static DataTable GetInventarisations()
        {
            const string sql = "dbo.mob_GetInventarisations";
            DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure);
            return table;
        }

        public static string FindDocOrderNomenclatureStoragePlaces(Guid docOrderId, Guid nomenclatureId, Guid characteristicId, Guid qualityId)
        {
            string result = null;
            const string sql = "dbo.mob_FindDocOrderNomenclatureStoragePlaces";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@NomenclatureId", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicId", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityId", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = table.Rows[0].IsNull("ResultMessage") ? null : table.Rows[0]["ResultMessage"].ToString();
                }
            }
            return result;
        }

        public static string FindDocOrderItemPosition(Guid docOrderId, int lineNumber)
        {
            string result = null;
            const string sql = "dbo.mob_FindDocOrderItemStoragePlaces";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@LineNumber", SqlDbType.Int)
                        {
                            Value = lineNumber
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = table.Rows[0].IsNull("ResultMessage") ? null : table.Rows[0]["ResultMessage"].ToString();
                }
            }
            return result;
        }

        public static BindingList<DocOrder> PersonDocOrders(Guid personId, DocDirection docDirection)
        {
            BindingList<DocOrder> list = null;
            const string sql = "dbo.mob_GetPersonDocOrders";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@IsOutOrders", SqlDbType.Bit)
                        {
                            Value = docDirection != DocDirection.DocIn
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<DocOrder>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new DocOrder
                        {
                            DocOrderId = new Guid(row["1COrderID"].ToString()),
                            Number = row["Number"].ToString(),
                            Consignee = row["Consignee"].ToString(),
                            OrderType = (OrderType)Convert.ToInt32(row["OrderKindID"])
                        });
                    }
                }
            }
            return list ?? new BindingList<DocOrder>();
        }

        public static List<PlaceZone> GetWarehousePlaceZones(int placeId)
        {
            var placeZones = Shared.PlaceZones.Where(p => p.PlaceId == placeId && p.PlaceZoneParentId == Guid.Empty && p.IsValid).ToList();
            return placeZones;
            /*List<PlaceZone> list = null;
            const string sql = "dbo.mob_GetWarehousePlaceZones";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = placeId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = (from DataRow row in table.Rows
                            select new PlaceZone
                                {
                                    PlaceZoneId = new Guid(row["PlaceZoneID"].ToString()), Name = row["Name"].ToString()
                                }).ToList();
                }
            }
            return list;*/
        }

        public static List<PlaceZone> GetPlaceZoneChilds(Guid placeZoneId)
        {
            var placeZones = Shared.PlaceZones.Where(p => p.PlaceZoneParentId == placeZoneId && p.IsValid).ToList();
            return placeZones;
            /*List<PlaceZone> list = null;
            const string sql = "dbo.mob_GetPlaceZoneChilds";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = (from DataRow row in table.Rows
                            select new PlaceZone
                            {
                                PlaceZoneId = new Guid(row["PlaceZoneID"].ToString()),
                                Name = row["Name"].ToString()
                            }).ToList();
                }
            }
            return list;*/
        }

        public static List<Barcodes> GetCurrentMovementBarcodes(int placeId, Guid personId)
        {
            List<Barcodes> list = null;
            const string sql = "mob_GetMovementBarcodeForPerson";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceIdTo", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = (from DataRow row in table.Rows
                            select new Barcodes
                            {
                                Barcode = row["Barcode"].ToString(),
                                ProductKindId = Convert.ToInt32(row["ProductKindID"])
                            }).ToList();
                }
            }
            return list;
        }

        public static BindingList<MovementProduct> GetMovementProductsList(int placeId, Guid personId)
        {
            BindingList<MovementProduct> list = null;
            const string sql = "dbo.mob_GetLastMovementProductsListForPerson";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceIdTo", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        }

                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<MovementProduct>();
                    foreach (DataRow row in table.Rows)
                    {
                        //string quantity;
                        decimal collectedQuantity;
                        //СГБ учитываем по весу, а СГИ - по групповым упаковкам
                        //!(row["Quantity"].ToString().All(char.IsDigit)) - проверяем, является ли числом (внимание: не отрабатывает отрицательное значение)
                        //quantity = row.IsNull("Quantity") ? "0" : (row.IsNull("CoefficientPackage") || !(row["Quantity"].ToString().All(char.IsDigit))) ? row["Quantity"].ToString() : (Convert.ToInt32(row["Quantity"]) / Convert.ToInt32(row["CoefficientPackage"])).ToString();
                        collectedQuantity = row.IsNull("Quantity") ? 0 : Convert.ToDecimal(row["Quantity"]);
                        
                        list.Add(new MovementProduct
                        {
                            CharacteristicId = row.IsNull("1CCharacteristicID") ? new Guid() : new Guid(row["1CCharacteristicID"].ToString()),
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            QualityId = new Guid(row["1CQualityID"].ToString()),
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString(),
                            PlaceZoneId = row.IsNull("PlaceZoneID") ? new Guid() : new Guid(row["PlaceZoneID"].ToString()),
                            CoefficientPackage = row.IsNull("CoefficientPackage") ? (int?)null : Convert.ToInt32(row["CoefficientPackage"]),
                            CoefficientPallet = row.IsNull("CoefficientPallet") ? (int?)null : Convert.ToInt32(row["CoefficientPallet"]),
                            //количество, пересчитанное в групповые упаковки для СГИ
                            //CollectedQuantityComputedColumn = ((row.IsNull("CoefficientPackage") || Convert.ToInt32(row["CoefficientPackage"]) == 0) ? collectedQuantity.ToString("0.###") : (collectedQuantity / Convert.ToInt32(row["CoefficientPackage"])).ToString("0.###")),
                            //SpoolWithBreakPercentColumn = (row.IsNull("CountProductSpools") || Convert.ToDecimal(row["CountProductSpools"]) == 0) ? 0 : (100 * Convert.ToDecimal(row["CountProductSpoolsWithBreak"]) / Convert.ToDecimal(row["CountProductSpools"])),
                            //Quantity = quantity,
                            CollectedQuantity = collectedQuantity,
                            CollectedQuantityUnits = row.IsNull("QuantityUnits") ? 0 : Convert.ToInt32(row["QuantityUnits"])
                            /*Barcode = row["Barcode"].ToString(),
                            Number = row["Number"].ToString(),
                            NomenclatureName = row["ShortNomenclatureName"].ToString(),
                            Quantity = Convert.ToDecimal(row["Quantity"]),
                            DocMovementId = new Guid(row["DocMovementID"].ToString()),
                            Date = Convert.ToDateTime(row["Date"].ToString()),
                            NumberAndInPlaceZone = row["NumberAndInPlaceZone"].ToString(),
                            ProductKindId = Convert.ToInt32(row["ProductKindID"])
                             */
                        });
                    }
                }
            }
            return list;
        }

        public static BindingList<ProductBase> GetMovementGoodProducts(int placeId, Guid personId, Guid nomenclatureId,
                                                             Guid characteristicId, Guid qualityId, Guid? placeZoneId)
        {
            BindingList<ProductBase> list = null;
            const string sql = "dbo.mob_GetLastMovementGoodProductsListForPerson";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceIdTo", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId
                        }

                };

            //DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            //return table;
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<ProductBase>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ProductBase
                        {
                            Number = row["Number"].ToString(),
                            Barcode = row["BarCode"].ToString(),
                            MovementId = row.IsNull("MovementID") ? Guid.Empty : new Guid(row["MovementID"].ToString()),
                            Date = Convert.ToDateTime(row["Date"]),
                            OutPlace = row["OutPlace"].ToString(),
                            InPlace = row["InPlace"].ToString(),
                            IsProductR = row.IsNull("IsProductR") ? false : Convert.ToBoolean(row["IsProductR"]),
                            Quantity = row["Quantity"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public static BindingList<DocNomenclatureItem> DocNomenclatureItems(Guid docOrderId, OrderType orderType, DocDirection docDirection)
        {
            BindingList<DocNomenclatureItem> list = null;
            var sql = "";
            switch (orderType)
            {
                case OrderType.ShipmentOrder:
                case OrderType.InternalOrder:
                    sql = "SELECT * FROM v1COrderGoods WHERE DocOrderID = @DocID";
                    break;
                case OrderType.MovementOrder:
                    sql = "SELECT * FROM vDocMovementOrders WHERE DocOrderID = @DocID";
                    break;
            }
                
            var parameters =
                new List<SqlParameter>
                    {
                        new SqlParameter("@DocID", SqlDbType.UniqueIdentifier)
                    };
            parameters[0].Value = docOrderId;
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<DocNomenclatureItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        string quantity;
                        decimal collectedQuantity;
                        int quantityUnits;
                        if (docDirection == DocDirection.DocOut || orderType == OrderType.MovementOrder)
                        {
                            //СГБ учитываем по весу, а СГИ - по групповым упаковкам
                            //!(row["Quantity"].ToString().All(char.IsDigit)) - проверяем, является ли числом (внимание: не отрабатывает отрицательное значение)
                            quantity = row.IsNull("Quantity") ? "0" : (row.IsNull("CoefficientPackage") || !(row["Quantity"].ToString().All(char.IsDigit))) ? row["Quantity"].ToString() : (Convert.ToInt32(row["Quantity"]) / Convert.ToInt32(row["CoefficientPackage"])).ToString();
                            collectedQuantity = row.IsNull("OutQuantity") ? 0 : Convert.ToDecimal(row["OutQuantity"]);
                            quantityUnits = row.IsNull("OutQuantityUnits") ? 0 : Convert.ToInt32(row["OutQuantityUnits"]);
                        }
                        else
                        {
                            quantity = row.IsNull("OutQuantity") ? "0" : (row.IsNull("CoefficientPackage") || !(row["OutQuantity"].ToString().All(char.IsDigit))) ? row["OutQuantity"].ToString() : (Convert.ToInt32(row["OutQuantity"]) / Convert.ToInt32(row["CoefficientPackage"])).ToString();
                            collectedQuantity = row.IsNull("InQuantity") ? 0 : Convert.ToDecimal(row["InQuantity"]);
                            quantityUnits = row.IsNull("InQuantityUnits") ? 0 : Convert.ToInt32(row["InQuantityUnits"]);
                        }
                        list.Add(new DocNomenclatureItem
                        {
                            CharacteristicId = row.IsNull("1CCharacteristicID") ? new Guid() : new Guid(row["1CCharacteristicID"].ToString()),
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            QualityId = new Guid(row["1CQualityID"].ToString()),
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString(),
                            CountProductSpools = table.Rows[0].IsNull("CountProductSpools") ? 0 : Convert.ToInt32(row["CountProductSpools"]),
                            CountProductSpoolsWithBreak = table.Rows[0].IsNull("CountProductSpoolsWithBreak") ? 0 : Convert.ToInt32(row["CountProductSpoolsWithBreak"]),
                            CoefficientPackage = row.IsNull("CoefficientPackage") ? (int?)null : Convert.ToInt32(row["CoefficientPackage"]),
                            CoefficientPallet = row.IsNull("CoefficientPallet") ? (int?)null : Convert.ToInt32(row["CoefficientPallet"]),
                            //количество, пересчитанное в групповые упаковки для СГИ
                            //CollectedQuantityComputedColumn = ((row.IsNull("CoefficientPackage") || Convert.ToInt32(row["CoefficientPackage"]) == 0) ? collectedQuantity.ToString("0.###") : (collectedQuantity / Convert.ToInt32(row["CoefficientPackage"])).ToString("0.###")),
                            //SpoolWithBreakPercentColumn = (row.IsNull("CountProductSpools") || Convert.ToDecimal(row["CountProductSpools"]) == 0) ? 0 : (100 * Convert.ToDecimal(row["CountProductSpoolsWithBreak"]) / Convert.ToDecimal(row["CountProductSpools"])),
                            Quantity = quantity,
                            CollectedQuantity = collectedQuantity,
                            CollectedQuantityUnits = quantityUnits
                        });
                    }
                }
            }
            return list;
        }

        public static List<Warehouse> GetWarehouses()
        {
            List<Warehouse> list = null;
            const string sql = "dbo.mob_GetWarehouses";
            using (DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<Warehouse>();
                    list.AddRange(from DataRow row in table.Rows
                                  select new Warehouse
                                  {
                                      WarehouseId = Convert.ToInt32(row["WarehouseID"]),
                                      WarehouseName = row["WarehouseName"].ToString(),
                                      WarehouseZones = GetWarehousePlaceZones(Convert.ToInt32(row["WarehouseID"]))
                                  });
                }
            }
            return list;
        }

        public static List<PlaceZone> GetPlaceZones()
        {
            List<PlaceZone> list = null;
            const string sql = "SELECT PlaceID, PlaceZoneID, Name, Barcode, PlaceZoneParentID, v FROM vPlaceZones ORDER BY SortOrder";
            using (DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<PlaceZone>();
                    list.AddRange(from DataRow row in table.Rows
                                  select new PlaceZone
                                  {
                                      PlaceId = Convert.ToInt32(row["PlaceID"]),
                                      PlaceZoneId = row.IsNull("PlaceZoneID") ? new Guid() : new Guid(row["PlaceZoneID"].ToString()),
                                      Name = row["Name"].ToString(),
                                      Barcode = row["Barcode"].ToString(),
                                      PlaceZoneParentId = row.IsNull("PlaceZoneParentID") ? new Guid() : new Guid(row["PlaceZoneParentID"].ToString()),
                                      IsValid = row.IsNull("v") ? false : Convert.ToBoolean(row["v"]),
                                  });
                }
            }
            return list;
        }

        public static DbOperationProductResult DeleteLastProductFromMovement(string barcode, int placeId, Guid personId, DocDirection docDirection)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_DelLastProductFromMovement]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        },
                    new SqlParameter("@PlaceIdTo", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@DocDirection", SqlDbType.Int)
                        {
                            Value = (int)docDirection
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        DocIsConfirmed = table.Rows[0].IsNull("IsConfirmed") ? false : Convert.ToBoolean(table.Rows[0]["IsConfirmed"]),
                        ProductKindId = table.Rows[0].IsNull("ProductKindID") ? (int?)null : Convert.ToInt16(table.Rows[0]["ProductKindID"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }

        public static DbOperationProductResult DeleteProductFromMovement(string barcode, Guid docMovementId, DocDirection docDirection)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromMovement]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        },
                    new SqlParameter("@DocMovementID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docMovementId
                        },
                    new SqlParameter("@DocDirection", SqlDbType.Int)
                        {
                            Value = (int)docDirection
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        DocIsConfirmed = Convert.ToBoolean(table.Rows[0]["IsConfirmed"]),
                        ProductKindId = table.Rows[0].IsNull("ProductKindID") ? (int?)null : Convert.ToInt16(table.Rows[0]["ProductKindID"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }


        public static DbOperationProductResult DeleteProductFromMovementOnMovementID(Guid scanId)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromMovementOnMovementID]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@MovementID", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        DocIsConfirmed = table.Rows[0].IsNull("IsConfirmed") ? false : Convert.ToBoolean(table.Rows[0]["IsConfirmed"]),
                        ProductKindId = table.Rows[0].IsNull("ProductKindID") ? (int?)null : Convert.ToInt16(table.Rows[0]["ProductKindID"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }

        public static DbOperationProductResult DeleteProductFromInventarisationOnInvProductID(Guid scanId)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromInventarisationOnInvProductID]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@InventarisationProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        DocIsConfirmed = table.Rows[0].IsNull("IsConfirmed") ? false : Convert.ToBoolean(table.Rows[0]["IsConfirmed"]),
                        ProductKindId = table.Rows[0].IsNull("ProductKindID") ? (int?)null : Convert.ToInt16(table.Rows[0]["ProductKindID"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Удаление продукта из приказа
        /// </summary>
        /// <param name="barcode">ШК продукта</param>
        /// <param name="docOrderId">Идентификатор документа на отгрузку 1С</param>
        /// <param name="docDirection">in,out,outin</param>
        /// <returns>Описание результата действия</returns>
        public static DbOperationProductResult DeleteProductFromOrder(string barcode,
                                                                      Guid docOrderId, DocDirection docDirection)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromOrder]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        },
                    new SqlParameter("@IsDocOut", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocOut
                        }
                    
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"])
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString(),
                            CountProductSpools = table.Rows[0].IsNull("CountProductSpools") ? 0 : Convert.ToInt16(table.Rows[0]["CountProductSpools"]),
                            CountProductSpoolsWithBreak = table.Rows[0].IsNull("CountProductSpoolsWithBreak") ? 0 : Convert.ToInt16(table.Rows[0]["CountProductSpoolsWithBreak"])
                        };
                    }
                }
            }
            return result;
        }

        public static DbOperationProductResult DeleteProductFromOrderOnMovementID(Guid scanId)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromMovementOnMovementID]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@MovementID", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        }
                    
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]),
                        DocIsConfirmed = table.Rows[0].IsNull("IsConfirmed") ? false : Convert.ToBoolean(table.Rows[0]["IsConfirmed"]),
                        ProductKindId = table.Rows[0].IsNull("ProductKindID") ? (int?)null : Convert.ToInt16(table.Rows[0]["ProductKindID"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString(),
                            CountProductSpools = table.Rows[0].IsNull("CountProductSpools") ? 0 : Convert.ToInt16(table.Rows[0]["CountProductSpools"]),
                            CountProductSpoolsWithBreak = table.Rows[0].IsNull("CountProductSpoolsWithBreak") ? 0 : Convert.ToInt16(table.Rows[0]["CountProductSpoolsWithBreak"])
                        };
                    }
                }
            }
            return result;
        }

        public static DbProductIdFromBarcodeResult GetFirstProductFromCashedBarcodes(string barcode)
        {
            DbProductIdFromBarcodeResult result = null;

            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID AS ProductID, Number, KindId AS ProductKindID FROM Barcodes WHERE Barcode = @Barcode AND KindId IS NOT NULL";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbProductIdFromBarcodeResult();
                    if (!table.Rows[0].IsNull("ProductKindID"))
                        result.ProductKindId = Convert.ToInt16(table.Rows[0]["ProductKindID"]);
                    if (!table.Rows[0].IsNull("ProductID"))
                        result.ProductId = (Guid)table.Rows[0]["ProductID"];
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                        result.NomenclatureId = (Guid)table.Rows[0]["NomenclatureID"];
                    if (!table.Rows[0].IsNull("CharacteristicID"))
                        result.CharacteristicId = (Guid)table.Rows[0]["CharacteristicID"];
                    if (!table.Rows[0].IsNull("MeasureUnitID"))
                        result.MeasureUnitId = (Guid)table.Rows[0]["MeasureUnitID"];
                    if (!table.Rows[0].IsNull("QualityID"))
                        result.QualityId = (Guid)table.Rows[0]["QualityID"];
                    result.CountProducts = 0;
                }
            }
            return result;
        }

        public static DbProductIdFromBarcodeResult GetFirstNomenclatureFromCashedBarcodes(string barcode)
        {
            DbProductIdFromBarcodeResult result = null;

            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID AS ProductID, Number, KindId AS ProductKindID FROM Barcodes WHERE Barcode = @Barcode AND KindId IS NULL";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbProductIdFromBarcodeResult();
                    if (!table.Rows[0].IsNull("ProductKindID"))
                        result.ProductKindId = Convert.ToInt16(table.Rows[0]["ProductKindID"]);
                    if (!table.Rows[0].IsNull("ProductID"))
                        result.ProductId = (Guid)table.Rows[0]["ProductID"];
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                        result.NomenclatureId = (Guid)table.Rows[0]["NomenclatureID"];
                    if (!table.Rows[0].IsNull("CharacteristicID"))
                        result.CharacteristicId = (Guid)table.Rows[0]["CharacteristicID"];
                    if (!table.Rows[0].IsNull("MeasureUnitID"))
                        result.MeasureUnitId = (Guid)table.Rows[0]["MeasureUnitID"];
                    if (!table.Rows[0].IsNull("QualityID"))
                        result.QualityId = (Guid)table.Rows[0]["QualityID"];
                    result.CountProducts = 0;
                }
            }
            return result;
        }

        public static List<DbProductIdFromBarcodeResult> GetAllNomenclatureFromCashedBarcodes(string barcode)
        {
            List<DbProductIdFromBarcodeResult> result = null;

            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID AS ProductID, Number, KindId AS ProductKindID FROM Barcodes WHERE Barcode = @Barcode AND KindId IS NULL";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new List<DbProductIdFromBarcodeResult>();
                    result.AddRange(from DataRow row in table.Rows
                                    select new DbProductIdFromBarcodeResult
                                  {
                                      ProductKindId = row.IsNull("ProductKindID") ? (int?)null : Convert.ToInt16(row["ProductKindID"]),
                                      ProductId = row.IsNull("ProductID") ? new Guid() : new Guid(row["ProductID"].ToString()),
                                      NomenclatureId = row.IsNull("NomenclatureID") ? new Guid() : new Guid(row["NomenclatureID"].ToString()),
                                      CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                                      MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                                      QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString())
                                  });
                }
            }
            return result;
        }


        public static DbProductIdFromBarcodeResult GetProductIdFromBarcodeOrNumber(string barcode)
        {
            DbProductIdFromBarcodeResult result = null;

            const string sql = "mob_GetProductIdFromBarcodeOrNumber";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbProductIdFromBarcodeResult();
                    if (!table.Rows[0].IsNull("ProductKindID"))
                        result.ProductKindId = Convert.ToInt16(table.Rows[0]["ProductKindID"]);
                    if (!table.Rows[0].IsNull("ProductID"))
                        result.ProductId = (Guid)table.Rows[0]["ProductID"];
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                        result.NomenclatureId = (Guid)table.Rows[0]["NomenclatureID"];
                    if (!table.Rows[0].IsNull("CharacteristicID"))
                        result.CharacteristicId = (Guid)table.Rows[0]["CharacteristicID"];
                    if (!table.Rows[0].IsNull("MeasureUnitID"))
                        result.MeasureUnitId = (Guid)table.Rows[0]["MeasureUnitID"];
                    if (!table.Rows[0].IsNull("QualityID"))
                        result.QualityId = (Guid)table.Rows[0]["QualityID"];
                    result.CountProducts = 0;
                }
            }
            return result;
        }

        public static DbOperationProductResult AddProductIdToOrder(Guid? scanId, Guid docOrderId, OrderType orderType, Guid personId
            , Guid productId, DocDirection docDirection, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_AddScanIdToOrder]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ScanID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (scanId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@OrderType", SqlDbType.Int)
                        {
                            Value = (int) orderType
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        },
                    new SqlParameter("@IsDocOut", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocOut
                        },
                    new SqlParameter("@ProductKindID", SqlDbType.Int)
                        {
                            Value = productKindId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@QuantityRos", SqlDbType.Int)
                        {
                            Value = quantity
                        },
                    new SqlParameter("@ShiftId", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString()
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString(),
                            CountProductSpools = table.Rows[0].IsNull("CountProductSpools") ? 0 : Convert.ToInt16(table.Rows[0]["CountProductSpools"]),
                            CountProductSpoolsWithBreak = table.Rows[0].IsNull("CountProductSpoolsWithBreak") ? 0 : Convert.ToInt16(table.Rows[0]["CountProductSpoolsWithBreak"])
                        };
                    }
                }
            }            
            return result;
        }

        public static MoveProductResult MoveProduct(Guid personId, Guid productId, EndPointInfo endPointInfo, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity)
        {
            MoveProductResult acceptProductResult = null;
            const string sql = "dbo.[mob_AddProductIdToMovement]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        },
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = endPointInfo.PlaceId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = endPointInfo.PlaceZoneId
                        },
                    new SqlParameter("@ProductKindID", SqlDbType.Int)
                        {
                            Value = productKindId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@QuantityRos", SqlDbType.Int)
                        {
                            Value = quantity
                        },
                    new SqlParameter("@ShiftId", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    acceptProductResult = new MoveProductResult
                    {
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyAdded =
                            !table.Rows[0].IsNull("AlreadyAdded") &&
                            Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (acceptProductResult != null & !table.Rows[0].IsNull("NomenclatureID"))
                    {
                        acceptProductResult.NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString());
                        acceptProductResult.CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString());
                        acceptProductResult.QualityId = new Guid(table.Rows[0]["QualityID"].ToString());
                        acceptProductResult.Quantity = table.Rows[0].IsNull("Quantity") ? 0 : Convert.ToDecimal(table.Rows[0]["Quantity"]);
                        acceptProductResult.NomenclatureName = table.Rows[0].IsNull("NomenclatureName") ? "" : table.Rows[0]["NomenclatureName"].ToString();
                        acceptProductResult.ShortNomenclatureName = table.Rows[0].IsNull("ShortNomenclatureName") ? "" : table.Rows[0]["ShortNomenclatureName"].ToString();
                        acceptProductResult.CoefficientPackage = table.Rows[0].IsNull("CoefficientPackage") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPackage"]);
                        acceptProductResult.CoefficientPallet = table.Rows[0].IsNull("CoefficientPallet") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPallet"]);
                    };
                }
            }
            return acceptProductResult;
        }

        public static MoveProductResult MoveProduct(Guid? scanId, Guid personId, Guid productId, EndPointInfo endPointInfo, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity)
        {
            MoveProductResult acceptProductResult = null;
            const string sql = "dbo.[mob_AddScanIdToMovement]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ScanID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (scanId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        },
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = endPointInfo.PlaceId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = endPointInfo.PlaceZoneId
                        },
                    new SqlParameter("@ProductKindID", SqlDbType.Int)
                        {
                            Value = productKindId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@QuantityRos", SqlDbType.Int)
                        {
                            Value = quantity
                        },
                    new SqlParameter("@ShiftId", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    acceptProductResult = new MoveProductResult
                    {
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyAdded =
                            !table.Rows[0].IsNull("AlreadyAdded") &&
                            Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (acceptProductResult != null & !table.Rows[0].IsNull("NomenclatureID"))
                    {
                        acceptProductResult.NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString());
                        acceptProductResult.CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString());
                        acceptProductResult.QualityId = new Guid(table.Rows[0]["QualityID"].ToString());
                        acceptProductResult.Quantity = table.Rows[0].IsNull("Quantity") ? 0 : Convert.ToDecimal(table.Rows[0]["Quantity"]);
                        acceptProductResult.NomenclatureName = table.Rows[0].IsNull("NomenclatureName") ? "" : table.Rows[0]["NomenclatureName"].ToString();
                        acceptProductResult.ShortNomenclatureName = table.Rows[0].IsNull("ShortNomenclatureName") ? "" : table.Rows[0]["ShortNomenclatureName"].ToString();
                        acceptProductResult.CoefficientPackage = table.Rows[0].IsNull("CoefficientPackage") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPackage"]);
                        acceptProductResult.CoefficientPallet = table.Rows[0].IsNull("CoefficientPallet") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPallet"]);
                    };
                }
            }
            return acceptProductResult;
        }

        public static List<Barcodes> GetCurrentBarcodes(Guid docOrderId, DocDirection docDirection)
        {
            List<Barcodes> list = null;
            const string sql = "mob_GetDocBarcode";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@IsInDoc", SqlDbType.Bit)
                        {
                            Value = (int)docDirection > 1?1:(int)docDirection
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = (from DataRow row in table.Rows
                                  select new Barcodes
                                  {
                                      Barcode = row["Barcode"].ToString(),
                                      ProductKindId = Convert.ToInt32(row["ProductKindID"])
                                  }).ToList();
                }
            }
            return list;
        }
        
        public static List<string> CurrentBarcodes(Guid docOrderId, DocDirection docDirection)
        {
            var list = new List<string>();
            const string sql = "mob_GetDocBarcodes";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@IsInDoc", SqlDbType.Bit)
                        {
                            Value = (int)docDirection > 1?1:(int)docDirection
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list.AddRange(from DataRow row in table.Rows select row["Barcode"].ToString());
                }
            }
            return list;
        }

        public static int CurrentCountProductSpools(Guid docOrderId,bool isWithBreak, DocDirection docDirection)
        {
            var countProductSpools = (int) 0;
            const string sql = "mob_GetDocCountProductSpools";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@IsWithBreak", SqlDbType.Bit)
                        {
                            Value = isWithBreak?1:0
                        },
                    new SqlParameter("@IsInDoc", SqlDbType.Bit)
                        {
                            Value = (int)docDirection > 1?1:(int)docDirection
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    countProductSpools = table.Rows[0].IsNull("CountProductSpools") ? 0 : Convert.ToInt32(table.Rows[0]["CountProductSpools"]);
                }
            }
            return countProductSpools;
        }

        public static string GetProgramSettings(string NameSetting)
        {
            string valueSetting = null;
            const string sql = "GetProgramSettings";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Setting", SqlDbType.VarChar)
                        {
                            Value = NameSetting
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    valueSetting = table.Rows[0]["ValueSetting"].ToString();
                }
            }
            return valueSetting;
        }

        private static DataTable ExecuteSelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType)
        {
            return ExecuteSelectQuery(sql, parameters, commandType, false);
        }

        private static DataTable ExecuteSilentlySelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType)
        {
            return ExecuteSelectQuery(sql, parameters, commandType, true);
        }

        private static DataTable ExecuteSelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType, bool IsSilently)
        {
            try
            {
                DataTable table = new DataTable();
                if (!ConnectionState.GetCheckerRunning)
                {
                    if (!ConnectionState.GetServerPortEnabled)
                    {//для запуска StartChecker
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                    }
                    else
                    {
                        if (!IsSilently) Cursor.Current = Cursors.WaitCursor;
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                        using (var command = new SqlCommand(sql))
                        {
                            command.Connection = new SqlConnection(ConnectionString);
                            command.CommandType = commandType;
                            //command.CommandTimeout = 3600;
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                            using (var sda = new SqlDataAdapter(command))
                            {
                                try
                                {
                                    sda.Fill(table);
                                    Shared.LastQueryCompleted = true;
                                }
                                catch (Exception ex)
                                {
                                    //#if DEBUG
                                    //                            MessageBox.Show(ex.Message);
                                    //                            var sqlex = ex as SqlException;
                                    //                            if (sqlex != null)
                                    //                                foreach (var error in sqlex.Errors)
                                    //                                {
                                    //                                    MessageBox.Show(error.ToString());
                                    //                                }
                                    //#endif
                                    if (!IsSilently) Shared.LastQueryCompleted = false;
                                    table = null;
                                }
                            }
                        }
                    }
                }
                return table;
            }
            finally
            {
                if (!IsSilently) Cursor.Current = Cursors.Default;
                if (!IsSilently && Shared.LastQueryCompleted == null) Shared.LastQueryCompleted = false;
            }
        }

        private static bool ExecuteNonQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType)
        {
            return ExecuteNonQuery(sql, parameters, commandType, false);
        }

        private static bool ExecuteSilentlyNonQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType)
        {
            return ExecuteNonQuery(sql, parameters, commandType, true);
        }

        private static bool ExecuteNonQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType, bool IsSilently)
        {
            try
            {
                bool ret = false;
                if (!ConnectionState.GetCheckerRunning)
                {
                    if (!ConnectionState.GetServerPortEnabled)
                    {//для запуска StartChecker
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                    }
                    else
                    {
                        if (!IsSilently) Cursor.Current = Cursors.WaitCursor;
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                        try
                        {
                            using (var connection = new SqlConnection(ConnectionString))
                            {
                                connection.Open();
                                using (var command = new SqlCommand(sql))
                                {
                                    command.Connection = connection;
                                    command.CommandType = commandType;
                                    command.Parameters.Clear();
                                    foreach (SqlParameter parameter in parameters)
                                    {
                                        command.Parameters.Add(parameter);
                                    }

                                    //try
                                    {
                                        command.ExecuteNonQuery();
                                        Shared.LastQueryCompleted = true;
                                        ret = true;
                                    }
                                    //catch (Exception ex)
                                    //{
                                    //    if (!IsSilently) Shared.LastQueryCompleted = false;
                                    //    ret = false;
                                    //}

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!IsSilently) Shared.LastQueryCompleted = false;
                            ret = false;
                        }

                    }
                }
                return ret;
            }
            finally
            {
                if (!IsSilently) Cursor.Current = Cursors.Default;
                if (!IsSilently && Shared.LastQueryCompleted == null) Shared.LastQueryCompleted = false;
            }
        }

        private static DataTable ExecuteCeSelectQuery(string sql, IEnumerable<SqlCeParameter> parameters,
                                                    CommandType commandType)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DataTable table;
                Shared.LastCeQueryCompleted = false;
                using (var command = new SqlCeCommand(sql))
                {
                    command.Connection = new SqlCeConnection(ConnectionCeString);
                    command.CommandType = commandType;
                    foreach (SqlCeParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    using (var sda = new SqlCeDataAdapter(command))
                    {
                        try
                        {
                            table = new DataTable();
                            sda.Fill(table);
                            Shared.LastCeQueryCompleted = true;
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            MessageBox.Show(ex.Message);
                            var sqlex = ex as SqlException;
                            if (sqlex != null)
                                foreach (var error in sqlex.Errors)
                                {
                                    MessageBox.Show(error.ToString());
                                }
#endif
                            Shared.LastCeQueryCompleted = false;
                            table = null;
                        }
                    }
                }
                return table;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                if (Shared.LastCeQueryCompleted == null) Shared.LastCeQueryCompleted = false;
            }
        }

        private static bool ExecuteCeNonQuery(string sql, IEnumerable<SqlCeParameter> parameters,
                                                    CommandType commandType)
        {
            try
            {
                bool ret = false;
                //Cursor.Current = Cursors.WaitCursor;
                using (var connection = new SqlCeConnection(ConnectionCeString))
                {
                    connection.Open();
                    using (var command = new SqlCeCommand(sql))
                    {
                        command.Connection = connection;
                        command.CommandType = commandType;
                        command.Parameters.Clear();
                        foreach (SqlCeParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }

                        try
                        {
                            command.ExecuteNonQuery();
                            Shared.LastCeQueryCompleted = true;
                            ret = true;
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            MessageBox.Show(ex.Message);
                            var sqlex = ex as SqlException;
                            if (sqlex != null)
                                foreach (var error in sqlex.Errors)
                                {
                                    MessageBox.Show(error.ToString());
                                }
#endif
                            Shared.LastCeQueryCompleted = false;
                            ret = false;
                        }

                    }
                }
                return ret;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public static List<Barcodes> GetCurrentInventarisationBarcodes(Guid docInventarisationId)
        {
            List<Barcodes> list = null;
            const string sql = "mob_GetInventarisationBarcode";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docInventarisationId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = (from DataRow row in table.Rows
                            select new Barcodes
                            {
                                Barcode = row["Barcode"].ToString(),
                                ProductKindId = Convert.ToInt32(row["ProductKindID"])
                            }).ToList();
                }
            }
            return list;
        }

        public static List<string> CurrentInventarisationBarcodes(Guid docInventarisationId)
        {
            var list = new List<string>();
            const string sql = "mob_GetInventarisationBarcodes";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docInventarisationId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list.AddRange(from DataRow row in table.Rows select row["Barcode"].ToString());
                }
            }
            return list;
        }

        internal static BindingList<DocNomenclatureItem> InventarisationProducts(Guid docInventarisationId)
        {
            BindingList<DocNomenclatureItem> list = null;
            const string sql = "mob_GetInventarisationProducts";
            
            var parameters =
                new List<SqlParameter>
                    {
                        new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                            {
                                Value = docInventarisationId
                            }
                    };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null)
                {
                    list = new BindingList<DocNomenclatureItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new DocNomenclatureItem
                        {
                            CharacteristicId = new Guid(row["1CCharacteristicID"].ToString()),
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            QualityId = new Guid(row["1CQualityID"].ToString()),
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            CollectedQuantity = Convert.ToDecimal(row["Quantity"]),
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString(),
                            CollectedQuantityUnits = row.IsNull("QuantityUnits") ? 0 : Convert.ToInt32(row["QuantityUnits"])
                        });
                    }
                }
            }
            return list;
        }

        internal static DbOperationProductResult AddProductIdToInventarisation(Guid? scanId, Guid docInventarisationId, Guid personId, EndPointInfo endPointInfo, Guid productId, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_AddScanIdToInventarisation]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ScanID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (scanId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (docInventarisationId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (personId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (productId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (endPointInfo.PlaceZoneId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@ProductKindID", SqlDbType.Int)
                        {
                            Value = productKindId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (nomenclatureId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (characteristicId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (qualityId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@QuantityRos", SqlDbType.Int)
                        {
                            Value = quantity
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString()
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }
        /*
        internal static DbOperationProductResult AddProductToInventarisation(Guid docInventarisationId, string barcode)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_AddProductToInventarisation]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docInventarisationId
                        },
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString()
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }
        */
        internal static DocInventarisation CreateNewInventarisation(Guid personId, int placeId)
        {
            DocInventarisation result = null;
            const string sql = "dbo.[mob_CreateNewInventarisation]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@ShiftID", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DocInventarisation
                    {
                        DocInventarisationId = new Guid(table.Rows[0]["DocInventarisationID"].ToString()),
                        Number = table.Rows[0]["Number"].ToString()
                    };
                }
            }
            return result;
        }

        internal static BindingList<ProductBase> DocInventarisationNomenclatureProducts(Guid docInventarisationId, Guid nomenclatureId, Guid characteristicId, Guid qualityId)
        {
            BindingList<ProductBase> list = null;
            const string sql = "dbo.mob_GetInventarisationNomenclatureProducts";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docInventarisationId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        }
                };
            //DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            //return table;
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<ProductBase>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ProductBase
                        {
                            Number = row["Number"].ToString(),
                            Barcode = row["BarCode"].ToString(),
                            MovementId = row.IsNull("MovementID") ? Guid.Empty : new Guid(row["MovementID"].ToString()),
                            Date = Convert.ToDateTime(row["Date"]),
                            OutPlace = row["OutPlace"].ToString(),
                            InPlace = row["InPlace"].ToString(),
                            IsProductR = row.IsNull("IsProductR") ? false : Convert.ToBoolean(row["IsProductR"]),
                            Quantity = row["Quantity"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        internal static List<DocNomenclatureItem> GetPalletItems(Guid productId)
        {
            var list = new List<DocNomenclatureItem>();
            const string sql = "dbo.mob_GetPalletItems";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        }
                };
            using (var table = ExecuteSelectQuery(sql,parameters, CommandType.StoredProcedure))
            {
                if (table != null)
                {
                    list.AddRange(from DataRow row in table.Rows
                                  select new DocNomenclatureItem
                                      {
                                          NomenclatureId = new Guid(row["NomenclatureId"].ToString()),
                                          CharacteristicId = new Guid(row["CharacteristicId"].ToString()),
                                          QualityId = new Guid(row["QualityId"].ToString()),
                                          CollectedQuantity = Convert.ToInt32(row["Quantity"]), 
                                          ShortNomenclatureName = row["ShortNomenclatureName"].ToString(), 
                                          NomenclatureName = row["NomenclatureName"].ToString()
                                      });
                }
            }
            return list;
        }

        /// <summary>
        /// Добавление в паллету
        /// </summary>
        /// <param name="barcode">шк номенклатуры</param>
        /// <param name="productId">Id паллеты</param>
        /// <param name="docOrderId">id приказа</param>
        /// <param name="quantity">Количество пачек</param>
        /// <returns></returns>
        internal static AddPalletItemResult AddItemToPallet(Guid productId, Guid docOrderId, string barcode, int quantity)
        {
            AddPalletItemResult result = null;
            const string sql = "dbo.mob_AddItemToPallet";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                    {
                        Value = docOrderId
                    },
                    new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                    {
                        Value = productId
                    },
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                    {
                        Value = barcode
                    },
                    new SqlParameter("@Quantity", SqlDbType.Int)
                    {
                        Value = quantity
                    }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new AddPalletItemResult()
                        {
                            ResultMessage = table.Rows[0]["ResultMessage"].ToString()
                        };
                    if (!table.Rows[0].IsNull("NomenclatureId"))
                    {
                        result.NomenclatureId = new Guid(table.Rows[0]["NomenclatureId"].ToString());
                        result.CharacteristicId = new Guid(table.Rows[0]["CharacteristicId"].ToString());
                        result.QualityId = new Guid(table.Rows[0]["QualityId"].ToString());
                        result.NomenclatureName = table.Rows[0]["NomenclatureName"].ToString();
                        result.ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString();
                        result.Quantity = Convert.ToInt32(table.Rows[0]["Quantity"]);
                    }
                }
            }
            return result;
        }

        internal static int GetMeasureUnitPackageCoefficient(Guid characteristicId)
        {
            var coefficient = 0;
            const string sql = "dbo.mob_GetMeasureUnitPackageCoefficient";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CharacteristicId", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    coefficient = Convert.ToInt32(table.Rows[0]["Coefficient"]);
                }
            }
            return coefficient;
        }

        internal static BindingList<PalletListItem> GetOrderPallets(Guid docOrderId)
        {
            var pallets = new BindingList<PalletListItem>();
            const string sql = "dbo.mob_GetOrderPallets";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        pallets.Add(new PalletListItem
                            {
                                ProductId = new Guid(row["ProductId"].ToString()),
                                Date = Convert.ToDateTime(row["Date"]),
                                Number = row["Number"].ToString()
                            });
                    }
                }
            }
            return pallets;
        }

        internal static string CreateNewPallet(Guid productId, Guid docOrderId)
        {
            var result = "";
            const string sql = "dbo.mob_CreateNewPallet";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        },
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table == null)
                {
                    result = "Не удалось создать паллету, возможно была потеряна связь";
                }
            }
            return result;
        }

        internal static string DeletePallet(Guid productId)
        {
            string result;
            const string sql = "dbo.mob_DeletePallet";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = table.Rows[0]["Result"].ToString();
                }
                else
                {
                    result = "Не удалось создать паллету в базе";
                }
            }
            return result;
        }

        internal static bool DeleteItemFromPallet(Guid productId, Guid nomenclatureId, Guid characteristicId)
        {
            const string sql = "dbo.mob_DeleteItemFromPallet";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        },
                    new SqlParameter("@NomenclatureId", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicId", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        }
                };
            ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return Shared.LastQueryCompleted ?? false;
        }
        
        public static string InfoProductByBarcode(string barcode)
        {
            string infoproduct = null;
            const string sql = "dbo.mob_GetInfoProduct";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BarCode", SqlDbType.VarChar)
                    {
                         Value = barcode
                    }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null )
                    if (table.Rows.Count == 0)
                    {
                        infoproduct = @"Неверный штрих-код";
                    }
                    else
                    {
                        DataRow row = table.Rows[0];
                        infoproduct = row["Name"].ToString();
                    }
            }
            return infoproduct;
        }

        public static BindingList<ChooseNomenclatureItem> GetNomenclatureCharacteristicQualityFromBarcode(string barcode)
        {
            BindingList<ChooseNomenclatureItem> list = null;
            const string sql = "dbo.mob_GetNomenclatureCharacteristicQualityFromBarcode";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<ChooseNomenclatureItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ChooseNomenclatureItem
                        {
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            CharacteristicId = row.IsNull("1CCharacteristicID") ? Guid.Empty : new Guid(row["1CCharacteristicID"].ToString()),
                            QualityId = new Guid(row["1CQualityID"].ToString()),
                            Name = row["Name"].ToString(),
                            Barcode = row["Barcode"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public static string GetCountBarcodes()
        {
            string ret;
            const string sql = "SELECT Count(*) AS CountBarcodes, Sum(Case When KindId is null Then 1 Else 0 End) AS CountNomenclatures, Sum(Case When KindId is not null Then 1 Else 0 End) AS CountProducts FROM Barcodes ";
            using (DataTable table = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    ret = table.Rows[0]["CountBarcodes"].ToString() + "/" + table.Rows[0]["CountNomenclatures"].ToString() + "/" + table.Rows[0]["CountProducts"].ToString();
                }
                else
                {
                    ret = "0/0/0";
                }

            }
            return ret;
        }

        public static int GetCountNomenclatureBarcodes()
        {
            int ret;
            const string sql = "SELECT Count(*) AS CountNomenclatures FROM Barcodes WHERE KindId is null ";
            using (DataTable table = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    ret = Convert.ToInt32(table.Rows[0]["CountNomenclatures"]);
                }
                else
                {
                    ret = 0;
                }

            }
            return ret;
        }

        public static List<ChooseNomenclatureItem> GetBarcodes1C(string barcode)
        {
            List<ChooseNomenclatureItem> list = null;
            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID FROM Barcodes WHERE Barcode = @Barcode ORDER BY NomenclatureID, CharacteristicID";
            SqlCeParameter p = new SqlCeParameter();
            p.ParameterName = "@Barcode";
            p.DbType = DbType.String;
            p.Value = barcode;
            var parameters = new List<SqlCeParameter>();
            parameters.Add(p);
            DataTable table = null;
            table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text);
            {
                if (table == null || table.Rows.Count == 0)
                {
                    Shared.SaveToLog(@"Не найден в локальной БД " + barcode + " (Посл.обн.кэша " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodes.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                    const string sqlDB = "dbo.mob_GetNomenclatureCharacteristicQualityFromBarcode";
                    var parametersDB = new List<SqlParameter>
                    {
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        }
                    };
                    table = ExecuteSilentlySelectQuery(sqlDB, parametersDB, CommandType.StoredProcedure);
                }
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<ChooseNomenclatureItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(
                            new ChooseNomenclatureItem
                                  {
                                      Barcode = row["Barcode"].ToString(),
                                      Name = row["Name"].ToString(),
                                      NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                                      CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                                      QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString()),
                                      MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                                      BarcodeId = new Guid(row["BarcodeID"].ToString())
                                  });
                    }
                }

                

            }
            return list;
        }
        

        public static bool GetBarcodes1C(DateTime date)
        {
            bool list = false;
            const string sql = "dbo.mob_GetBarcodes1C";
            using (DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = true;
                    var form = new ProgressBarForm(date, date, table, @"Идет загрузка номенклатур");
                            if (form != null)
                            {
                                if (ExecuteCeNonQuery("DELETE Barcodes WHERE KindId is null", new List<SqlCeParameter>(), CommandType.Text))
                                {
                                    var r = form.ShowDialog();
                                    var endDate = form.ret;
                                }
                                //form.bkgndWorker.DoWork += new DoWorkEventHandler(UpdateCashedBarcodesProgress);
                                //form.bkgndWorker.WorkerReportsProgress = true;
                                //form.bkgndWorker.RunWorkerAsync(table);
                            }
                    /*foreach (DataRow row in table.Rows)
                    {
                        UpdateBarcodes1C(
                            new CashedBarcode
                            {
                                DateChange = date,
                                TypeChange = 0,
                                Barcode = row["Barcode"].ToString(),
                                Name = row["Name"].ToString(),
                                NomenclatureId = row.IsNull("NomenclatureID") ? new Guid() : new Guid(row["NomenclatureID"].ToString()),
                                CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                                QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString()),
                                MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                                BarcodeId = new Guid(row["BarcodeID"].ToString()),
                                Number = row["Number"].ToString(),
                                KindId = row.IsNull("KindID") ? null : (int?)Convert.ToInt32(row["KindID"])
                            });
                        //list.Add(
                        //    new ChooseNomenclatureItem
                        //{
                        //    Barcode = row["Barcode"].ToString(),
                        //    Name = row["Name"].ToString(),
                        //    NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                        //    CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                        //    QualityId = new Guid(row["QualityID"].ToString()),
                        //    MeasureUnitId = new Guid(row["MeasureUnitID"].ToString()),
                        //    BarcodeId = new Guid(row["BarcodeID"].ToString())
                        //});
                    }*/
                }
            }
            return list;
        }

        public static void UpdateCashedBarcodesProgress(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var table = (DataTable)e.Argument;
            int index = 0;
            int count = table.Rows.Count;
            //int percent = 0;
            DateTime ret = Convert.ToDateTime("2020/10/01");
            DateTime previousDate = ret;
            try
            {
                foreach (DataRow row in table.Rows)
                {
                    var percent = Convert.ToInt32((index * 100) / count);
                    worker.ReportProgress(percent);
                    if (UpdateBarcodes1C(
                         new CashedBarcode
                         {
                             DateChange = Convert.ToDateTime(row["DateChange"]),
                             TypeChange = Convert.ToInt32(row["TypeChange"]),
                             Barcode = row["Barcode"].ToString(),
                             Name = row["Name"].ToString(),
                             NomenclatureId = row.IsNull("NomenclatureID") ? new Guid() : new Guid(row["NomenclatureID"].ToString()),
                             CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                             QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString()),
                             MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                             BarcodeId = new Guid(row["BarcodeID"].ToString()),
                             Number = row["Number"].ToString(),
                             KindId = row.IsNull("KindID") ? null : (int?)Convert.ToInt32(row["KindID"])
                         }))
                    {
                        if (previousDate != Convert.ToDateTime(row["DateChange"]))
                        {
                            ret = previousDate;//возвращаем время последней удачно записанной строки с предыдущим временем (чтобы если несколько изменений в один момент и произойдет сбой, то все эти записи этого момента кешировались повторно) 
                            previousDate = Convert.ToDateTime(row["DateChange"]);
                        }
                    }
                    else
                        break; ;//ошибка при обновлении текущей строки - дата последнего обновления есть дата предыдущей строки с дытой, отличной от текущей
                    index++;
                }
            }
            finally
            {
                if (worker.WorkerSupportsCancellation == true)
                {
                    // Cancel the asynchronous operation.
                    worker.CancelAsync();
                }
            }
        }


        public static DateTime UpdateCashedBarcodes(DateTime startDate, DateTime endDate, bool IsFirst)
        {
            //BindingList<ChooseNomenclatureItem> list = null;
            DateTime ret = startDate;
            DateTime previousDate = ret;
            const string sql = "dbo.mob_GetBarcodesChanges";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@StartUTCDate", SqlDbType.DateTime)
                        {
                            Value = startDate
                        },
                        new SqlParameter("@EndUTCDate", SqlDbType.DateTime)
                        {
                            Value = endDate
                        }
                };
            //Первоначальная загрузка изменений должна обязательно завершится удачно
            using (DataTable table = (IsFirst) ? ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure) : ExecuteSilentlySelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null)
                {
                    if (table.Rows.Count > 0)
                    {
                        //list = new BindingList<ChooseNomenclatureItem>();
                        int index = 0;
                        if (IsFirst)
                        {
                            var form = new ProgressBarForm(startDate, endDate, table, @"Идет загрузка штрих-кодов");
                            if (form != null)
                            {
                                var r = form.ShowDialog();
                                endDate = form.ret;

                                //form.bkgndWorker.DoWork += new DoWorkEventHandler(UpdateCashedBarcodesProgress);
                                //form.bkgndWorker.WorkerReportsProgress = true;
                                //form.bkgndWorker.RunWorkerAsync(table);
                            }
                        }
                        else
                        foreach (DataRow row in table.Rows)
                        {
                            if (UpdateBarcodes1C(
                                 new CashedBarcode
                                 {
                                     DateChange = Convert.ToDateTime(row["DateChange"]),
                                     TypeChange = Convert.ToInt32(row["TypeChange"]),
                                     Barcode = row["Barcode"].ToString(),
                                     Name = row["Name"].ToString(),
                                     NomenclatureId = row.IsNull("NomenclatureID") ? new Guid() : new Guid(row["NomenclatureID"].ToString()),
                                     CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                                     QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString()),
                                     MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                                     BarcodeId = new Guid(row["BarcodeID"].ToString()),
                                     Number = row["Number"].ToString(),
                                     KindId = row.IsNull("KindID") ? null : (int?)Convert.ToInt32(row["KindID"])
                                 }))
                            {
                                if (previousDate != Convert.ToDateTime(row["DateChange"]))
                                {
                                    ret = previousDate;//возвращаем время последней удачно записанной строки с предыдущим временем (чтобы если несколько изменений в один момент и произойдет сбой, то все эти записи этого момента кешировались повторно) 
                                    previousDate = Convert.ToDateTime(row["DateChange"]);
                                }
                            }
                            else
                            { endDate = ret; break; }
                                //return ret;//ошибка при обновлении текущей строки - дата последнего обновления есть дата предыдущей строки с дытой, отличной от текущей
                        }
                        ret = endDate;//все обновлено корректно - дата последнего обновления есть дата окончания запрошенного периода
                    }
                    else
                        ret = endDate;//обновлять нечего - дата последнего обновления есть дата окончания запрошенного периода
                }
                else
                    ret = startDate;//ошибка при получении данных - дата последнего обновления не меняется
            }
            return ret;
        }

       /* public static List<ChooseNomenclatureItem> GetBarcodes1C1()
        {
            List<ChooseNomenclatureItem> list = null;
            const string sql = "dbo.mob_GetBarcodes1C";
            using (DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = (from DataRow row in table.Rows
                                  select new ChooseNomenclatureItem
                                  {
                                      Barcode = row["Barcode"].ToString(),
                                      Name = row["Name"].ToString(),
                                      NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                                      CharacteristicId = new Guid(row["CharacteristicID"].ToString()),
                                      QualityId = new Guid(row["QualityID"].ToString()),
                                      MeasureUnitId = new Guid(row["MeasureUnitID"].ToString())
                                  }).ToList();
                }
            }
            return list;
        }
        */
        public static DataTable RemoveProductRFromOrder(Guid docShipmentOrderId, Guid nomenclatureId,
                                                             Guid characteristicId, Guid qualityId, decimal quantity)
        {
            const string sql = "dbo.mob_RemoveProductRFromOrder";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docShipmentOrderId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@Quantity", SqlDbType.Int)
                        {
                            Value = quantity
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return table;
        }

        public static DataTable RemoveProductRFromInventarisation(Guid docInventarisationId, Guid nomenclatureId,
                                                     Guid characteristicId, Guid qualityId, decimal quantity)
        {
            const string sql = "dbo.mob_RemoveProductRFromInventarisation";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocInventarisationID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docInventarisationId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@Quantity", SqlDbType.Int)
                        {
                            Value = quantity
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return table;
        }

        public static DataTable RemoveProductRFromMovement(int placeId, Guid personId, Guid nomenclatureId,
                                                             Guid characteristicId, Guid qualityId, Guid? placeZoneId, decimal quantity)
        {
            const string sql = "dbo.mob_RemoveProductRFromMovement";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceIdTo", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        },
                    new SqlParameter("@QualityID", SqlDbType.UniqueIdentifier)
                        {
                            Value = qualityId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId
                        },
                    new SqlParameter("@Quantity", SqlDbType.Int)
                        {
                            Value = quantity
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return table;
        }

        public static int? CloseShiftWarehouse(Guid personId, int shiftId)
        {
            int? result = null;
            const string sql = "dbo.mob_CloseShiftWarehouse";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@ShiftID", SqlDbType.Int)
                        {
                            Value = shiftId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = table.Rows[0].IsNull("Result") ? null : (int?)Convert.ToInt32(table.Rows[0]["Result"]);
                }
            }
            return result;
        }
    }
}