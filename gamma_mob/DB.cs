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
using OpenNETCF.Diagnostics;
//using CipherLab.SystemApi;

namespace gamma_mob
{
    public static class Db
    {
        private static string ConnectionString { get; set; }

        private static string _deviceName  { get; set; }
        public static string deviceName
        {
            get
            {
                if (_deviceName == null || _deviceName == String.Empty)
                {
                    try
                    {
                        _deviceName = Shared.Device.GetDeviceName();
                    }
                    catch (Exception e)
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
                        _deviceIP = Shared.Device.GetDeviceIP();
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
        private static bool IsInitializationFirstConnectionCeString { get; set; }
        private static string ConnectionCeString (ConnectServerCe serverCe) 
        {
            {
//#if DEBUG
//                string startupPath = Program.deviceName.Contains("Falcon") ? @"\FlashDisk\gamma_mob" : 
//                    Program.deviceName.Contains("CPT") ? @"\USER_DATA\gamma_mob" : "";
//#else
                        string startupPath = Application2.StartupPath;
//#endif

                var dbFileLog = /*@"\Temp\GammaDBLog.sdf";*/startupPath + @"\..\GammaDBLog.sdf";
                var dbFileBarcodes = startupPath + @"\..\..\GammaDBBarcodes.sdf";
                var dbFileBackupBarcodes = startupPath + @"\..\GammaDBBackupBarcodes.sdf";

                var dbFile = serverCe == ConnectServerCe.LogServer ? dbFileLog 
                        : serverCe == ConnectServerCe.BarcodesServer ? dbFileBarcodes 
                        : serverCe == ConnectServerCe.BackupBarcodesServer ? dbFileBackupBarcodes 
                        : startupPath + @"\GammaDBLocal.sdf";
                if (!IsNotFirstGetConnectionCeString && !IsInitializationFirstConnectionCeString)
                {
                    DateTime dbCurrentFileBarcodesCreateTime = new DateTime();
                    DateTime dbNewFileBarcodesCreateTime = new DateTime();

                    IsInitializationFirstConnectionCeString = true;

                    bool isCopyAutostartIni = false;
                    bool isErrorCopyAutostartIni = false;
                    
                    if (!File.Exists(@"\FlashDisk\Autostart.ini"))
                    {
                        if (File.Exists(startupPath + @"\Autostart.ini"))
                        {
                            try
                            {
                                isCopyAutostartIni = true;
                                File.Copy(startupPath + @"\Autostart.ini", @"\FlashDisk\Autostart.ini");
                            }
                            catch
                            {
                                isErrorCopyAutostartIni = true;
                            }
                        }
                    }
                    
                    bool isCreateDatabaseLog = false;
                    bool isCreateDatabaseBarcodes = false;
                    bool isCopyDatabaseLog = false;
                    bool isCopyDatabaseBarcodes = false;
                    bool IsDeletedDbFileLog = false;
                    bool IsErrorDbFileLog = false;
                    bool IsDeletedDbFileBarcodes = false;
                    bool IsErrorDbFileBarcodes = false;
                    bool isDbFileLogToDelete = false;
                    bool isDbFileBarcodesToDelete = false;
                    bool IsExistNewFileBarcodes = false;
                    SqlCeConnection empConLog;
                    SqlCeCommand empComLog;
                    SqlCeConnection empConBarcodes;
                    SqlCeCommand empComBarcodes;
                    try
                    {
                        empConLog = new SqlCeConnection(@"Data Source = " + dbFileLog);
                        empConLog.Open();
                        empComLog = empConLog.CreateCommand();
                        empConLog.Close();
                    }
                    catch
                    {
                        IsErrorDbFileLog = true;
                    }

                    try
                    {
                        empConBarcodes = new SqlCeConnection(@"Data Source = " + dbFileBackupBarcodes);
                        empConBarcodes.Open();
                        empComBarcodes = empConBarcodes.CreateCommand();
                        empComBarcodes.CommandText = "SELECT DatabaseCreateTime FROM Settings";
                        dbCurrentFileBarcodesCreateTime = Convert.ToDateTime(empComBarcodes.ExecuteScalar());
                        empConBarcodes.Close();
                    }
                    catch
                    {
                        IsErrorDbFileBarcodes = true;
                    }

                    if (File.Exists(dbFileLog))
                    {
                        isDbFileLogToDelete = true;
                        if (IsErrorDbFileLog)
                            try
                            {
                                File.Delete(dbFileLog);
                                IsDeletedDbFileLog = true;
                            }
                            catch
                            {
                                IsDeletedDbFileLog = false;
                            }
                    }

                    if (File.Exists(dbFileBackupBarcodes))
                    {
                        isDbFileBarcodesToDelete = true;
                        try
                        {
                            using (StreamReader sr = new StreamReader(startupPath + @"\GammaDBBarcodesVersion.txt"))
                            {
                                dbNewFileBarcodesCreateTime = Convert.ToDateTime(sr.ReadLine());
                            }
                            if ((dbCurrentFileBarcodesCreateTime != null && dbNewFileBarcodesCreateTime != null) && dbCurrentFileBarcodesCreateTime < dbNewFileBarcodesCreateTime)
                                IsExistNewFileBarcodes = true;
                        }
                        catch
                        {
                            IsExistNewFileBarcodes = false;
                        }
                        if (IsErrorDbFileBarcodes || IsExistNewFileBarcodes)
                            try
                            {
                                File.Delete(dbFileBackupBarcodes);
                                IsDeletedDbFileBarcodes = true;
                            }
                            catch
                            {
                                IsDeletedDbFileBarcodes = false;
                            }
                    }
                    if (!File.Exists(dbFileLog))
                    {
                        SqlCeEngine empEngine = new SqlCeEngine(@"Data Source = " + dbFileLog);
                            empEngine.CreateDatabase();
                            isCreateDatabaseLog = true;
                    }
                    if (!File.Exists(dbFileBackupBarcodes))
                    {
                        if (File.Exists(startupPath + @"\GammaDBBarcodes.sdf"))
                        {
                            try
                            {
                                isCopyDatabaseBarcodes = true;
                                File.Copy(startupPath + @"\GammaDBBarcodes.sdf", dbFileBackupBarcodes);
                            }
                            catch
                            {//Скопировать не получилось - надо создать
                                isCreateDatabaseBarcodes = true;
                            }
                        }
                        else
                            isCreateDatabaseBarcodes = true;
                        if (isCreateDatabaseBarcodes)
                        {
                            SqlCeEngine empEngine = new SqlCeEngine(@"Data Source = " + dbFileBackupBarcodes);
                            empEngine.CreateDatabase();
                            isCreateDatabaseBarcodes = true;
                        }
                    }

                    empConLog = new SqlCeConnection(@"Data Source = " + dbFileLog);
                    empConLog.Open();
                    empComLog = empConLog.CreateCommand();
                    empConBarcodes = new SqlCeConnection(@"Data Source = " + dbFileBackupBarcodes);
                    empConBarcodes.Open();
                    empComBarcodes = empConBarcodes.CreateCommand();
                    string strQuery;
                    if (TableCeExists(empConLog, "ScannedBarcodes"))
                    {
                        strQuery = "DROP TABLE ScannedBarcodes";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }
                    if (!TableCeExists(empConBarcodes, "Settings"))
                    {
                        strQuery = "CREATE TABLE Settings (LastUpdatedTimeBarcodes DateTime)";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                        strQuery = "INSERT INTO Settings (LastUpdatedTimeBarcodes) VALUES (DATEADD(HOUR,-8,GETDATE()))";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConBarcodes, "Settings", "CountBarcodeNomenclatures"))
                    {
                        strQuery = "ALTER TABLE Settings ADD CountBarcodeNomenclatures int";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                    }
                    if (!ColumnCeExists(empConBarcodes, "Settings", "CountBarcodeProducts"))
                    {
                        strQuery = "ALTER TABLE Settings ADD CountBarcodeProducts int";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConBarcodes, "Settings", "DatabaseCreateTime"))
                    {
                        strQuery = "ALTER TABLE Settings ADD column DatabaseCreateTime datetime";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                        strQuery = "UPDATE Settings SET DatabaseCreateTime = GETDATE()";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                    }

                    if (!TableCeExists(empConLog, "Logs"))
                    {
                        strQuery = "CREATE TABLE Logs (LogId uniqueidentifier default newid() PRIMARY KEY,LogDate DateTime default GetDate(),Log nvarchar(1000),Barcode nvarchar(100), UserName nvarchar(250), PersonId uniqueidentifier, PlaceId int, DocTypeId int, IsUploaded bit, DocId uniqueidentifier, PlaceZoneId uniqueidentifier, ToDelete bit default (0), IsDeleted bit default (0), ProductId uniqueidentifier, ProductKindId int, NomenclatureId uniqueidentifier, CharacteristicId uniqueidentifier, QualityId uniqueidentifier, Quantity int, IsUploadedToServer bit default (0))";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                        strQuery = "CREATE INDEX IX_LogId ON Logs (LogId ASC)";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Create table Logs ";
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "FromProductId"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column FromProductId uniqueidentifier";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "InPlaceZoneID"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column InPlaceZoneID uniqueidentifier";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "FromPlaceID"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column FromPlaceID int";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "FromPlaceZoneID"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column FromPlaceZoneID uniqueidentifier";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "MeasureUnitID"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column MeasureUnitID uniqueidentifier";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "NewWeight"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column NewWeight int";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "QuantityFractional"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column QuantityFractional int";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!ColumnCeExists(empConLog, "Logs", "ValidUntilDate"))
                    {
                        strQuery = "ALTER TABLE Logs ADD column ValidUntilDate DateTime";
                        empComLog.CommandText = strQuery;
                        empComLog.ExecuteNonQuery();
                    }

                    if (!TableCeExists(empConBarcodes, "Barcodes"))
                    {
                        strQuery = "CREATE TABLE Barcodes (Barcode nvarchar(100), Name nvarchar(600), NomenclatureID uniqueidentifier, CharacteristicID uniqueidentifier, QualityID uniqueidentifier, MeasureUnitID uniqueidentifier, BarcodeID uniqueidentifier, Number nvarchar(50), KindId tinyint)";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                        strQuery = "CREATE INDEX IX_Barcode ON Barcodes (Barcode ASC)";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                        strQuery = "CREATE INDEX IX_BarcodeId ON Barcodes (BarcodeId)";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                        
                        //Db.GetBarcodes1C();
                    }

                    if (!ColumnCeExists(empConBarcodes, "Barcodes", "IsMovementFromPallet"))
                    {
                        strQuery = "ALTER TABLE Barcodes ADD IsMovementFromPallet tinyint";
                        empComBarcodes.CommandText = strQuery;
                        empComBarcodes.ExecuteNonQuery();
                    }

                    empConLog.Close();
                    empConBarcodes.Close();
                    empConLog.Open();
                    if (IsErrorDbFileLog)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        if (!isDbFileLogToDelete)
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"Error database " + dbFileLog + @" - Database not exist";
                        else if (IsDeletedDbFileLog)
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"Error database " + dbFileLog + @" - Database deleted";
                        else
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"error database " + dbFileLog + @" - Error database deleted";
                        empComLog.ExecuteNonQuery();
                    }
                    
                    if (IsErrorDbFileBarcodes)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        if (!isDbFileBarcodesToDelete)
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"Error database " + dbFileBackupBarcodes + @" - Database not exist";
                        else if (IsDeletedDbFileBarcodes)
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"Error database " + dbFileBackupBarcodes + @" - Database deleted";
                        else
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"error database " + dbFileBackupBarcodes + @" - Error database deleted";
                        empComLog.ExecuteNonQuery();
                    }
                    
                    if (isCopyDatabaseLog)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Copy database " + dbFileLog;
                        empComLog.ExecuteNonQuery();
                    }
                    if (isCreateDatabaseLog)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Create database " + dbFileLog;
                        empComLog.ExecuteNonQuery();
                    }
                    if (isCopyDatabaseBarcodes)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Copy database " + dbFileBackupBarcodes;
                        empComLog.ExecuteNonQuery();
                    }
                    if (isCreateDatabaseBarcodes)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Create database " + dbFileBackupBarcodes;
                        empComLog.ExecuteNonQuery();
                    }
                    bool isCopyDatabaseBarcodesFromBackup = false;
                    if (isCopyDatabaseBarcodes || isCreateDatabaseBarcodes)
                    {
                        isCopyDatabaseBarcodesFromBackup = true;
                    }
                    else
                    {
                        try
                        {
                            empConBarcodes = new SqlCeConnection(@"Data Source = " + dbFileBarcodes);
                            empConBarcodes.Open();
                            empComBarcodes = empConBarcodes.CreateCommand();
                            empComBarcodes.CommandText = "SELECT DatabaseCreateTime FROM Settings";
                            dbCurrentFileBarcodesCreateTime = Convert.ToDateTime(empComBarcodes.ExecuteScalar());
                            empConBarcodes.Close();
                        }
                        catch
                        {
                            isCopyDatabaseBarcodesFromBackup = true;
                        }
                    }
                    if (isCopyDatabaseBarcodesFromBackup)
                    {
                        try
                        {
                            if (File.Exists(dbFileBarcodes))
                            {
                                File.Delete(dbFileBarcodes);
                            }
                            File.Copy(dbFileBackupBarcodes, dbFileBarcodes);
                            strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                            empComLog.CommandText = strQuery;
                            empComLog.Parameters.Clear();
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"Copy database " + dbFileBackupBarcodes + " to " + dbFileBarcodes;
                            empComLog.ExecuteNonQuery();

                        }
                        catch
                        {
                            strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                            empComLog.CommandText = strQuery;
                            empComLog.Parameters.Clear();
                            empComLog.Parameters.Add("@Log", DbType.String).Value = @"Error copy database " + dbFileBackupBarcodes + " to " + dbFileBarcodes;
                            empComLog.ExecuteNonQuery();
                        }
                    }
                    if (isCopyAutostartIni)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Copy Autostart.Ini";
                        empComLog.ExecuteNonQuery();
                    }
                    if (isErrorCopyAutostartIni)
                    {
                        strQuery = "INSERT INTO Logs (Log) VALUES(@Log)";
                        empComLog.CommandText = strQuery;
                        empComLog.Parameters.Clear();
                        empComLog.Parameters.Add("@Log", DbType.String).Value = @"Error copy Autostart.Ini";
                        empComLog.ExecuteNonQuery();
                    } 
                    empConLog.Close();
                    IsNotFirstGetConnectionCeString = true;
                }
                return @"Data Source = " + dbFile;
            }
            
        }

        public static void SetConnectionString(string ipAddress, string database, string user, string password,
                                               string timeout)
        {
            ConnectionString = "Application Name=mob_gamma v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " / " + deviceName + ";Data Source=" + ipAddress + ";Initial Catalog=" + database + "" +
                               ";Persist Security Info=True;User ID=" + user + "" +
                               ";Password=" + password + ";Connection Timeout=" + timeout;
        }

        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        public static int CheckSqlConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString)) return 1;
            if (!ConnectionState.CheckConnection()) return 1;
            DateTime? serverDateTime = null;//new DateTime();
            const string sql = "SELECT 1 AS One";
            var parameters = new List<SqlParameter>();
            using (DataTable table = ExecuteSilentlySelectQuery(sql, parameters, CommandType.Text, 2))
            {
                if (table != null && table.Rows.Count > 0)
                    return 0;
                else 
                    return 2;
            }
            Db.AddMessageToLog("3:" + DateTime.Now);
            return -1;// 0;
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
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
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
            
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer);
        }

        public static int? GetCountBarcodeNomenclatures()
        {
            int? ret = 0;
            const string sql = "SELECT count(*) AS CountBarcodeNomenclatures FROM Barcodes WHERE KindId is null";
            var parameters = new List<SqlCeParameter>();
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    ret = row.IsNull("CountBarcodeNomenclatures") ? null : (int?)Convert.ToInt32(row["CountBarcodeNomenclatures"]);
                }
            }
            return ret;
        }
        
        public static int? GetCountBarcodeProducts()
        {
            int? ret = 0;
            const string sql = "SELECT count(*) AS CountBarcodeProducts FROM Barcodes WHERE KindId is not null";
            var parameters = new List<SqlCeParameter>();
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    ret = row.IsNull("CountBarcodeProducts") ? null : (int?)Convert.ToInt32(row["CountBarcodeProducts"]);
                }
            }
            return ret;
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
                        var parameters2_ = new List<SqlCeParameter>();
                        foreach (var par in parameters2)
                        {
                            var pp = new SqlCeParameter()
                                    {
                                        ParameterName = par.ParameterName,
                                        DbType = par.DbType,
                                        Value = par.Value
                                    };
                            parameters2_.Add(pp);
                        }
                        string sql2 = "DELETE Barcodes WHERE BarcodeId = @BarcodeID";
                        var ret2 = ExecuteCeNonQuery(sql2, parameters2, CommandType.Text, ConnectServerCe.BarcodesServer);
                        var ret2_ = ExecuteCeNonQuery(sql2, parameters2_, CommandType.Text, ConnectServerCe.BackupBarcodesServer);
                        //Shared.Barcodes1C.RemovedBarcode(item.KindId);
                        return ret2;
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
                        SqlCeParameter p09 = new SqlCeParameter();
                        p09.ParameterName = "@IsMovementFromPallet";
                        p09.DbType = DbType.Byte;
                        p09.Value = (item.IsMovementFromPallet as object) ?? 0;
                        parameters0.Add(p09);
                        var parameters0_ = new List<SqlCeParameter>();
                        foreach (var par in parameters0)
                        {
                            var pp = new SqlCeParameter()
                                    {
                                        ParameterName = par.ParameterName,
                                        DbType = par.DbType,
                                        Value = par.Value
                                    };
                            parameters0_.Add(pp);
                        }
                        string sql0 = "INSERT INTO Barcodes (Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID, Number, KindId, IsMovementFromPallet) VALUES (@Barcode, @Name, @NomenclatureID, @CharacteristicID, @QualityID, @MeasureUnitID, @BarcodeID, @Number, @KindID, @IsMovementFromPallet)";
                        var ret0 = ExecuteCeNonQuery(sql0, parameters0, CommandType.Text, ConnectServerCe.BarcodesServer);
                        var ret0_ = ExecuteCeNonQuery(sql0, parameters0_, CommandType.Text, ConnectServerCe.BackupBarcodesServer);
                        //Shared.Barcodes1C.AddedBarcode(item.KindId);
                        return ret0; 
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
                        SqlCeParameter p9 = new SqlCeParameter();
                        p9.ParameterName = "@IsMovementFromPallet";
                        p9.DbType = DbType.Byte;
                        p9.Value = (item.IsMovementFromPallet as object) ?? 0;
                        parameters1.Add(p9);
                        var parameters1_ =  new List<SqlCeParameter>();
                        foreach (var par in parameters1)
                        {
                            var pp = new SqlCeParameter()
                                {
                                    ParameterName = par.ParameterName,
                                    DbType = par.DbType,
                                    Value = par.Value
                                };
                            parameters1_.Add(pp);
                        }
                        string sql1 = "UPDATE Barcodes SET Barcode = @Barcode, Name = @Name, NomenclatureID = @NomenclatureID, CharacteristicID = @CharacteristicID, QualityID = @QualityID, MeasureUnitID = @MeasureUnitID, BarcodeID = @BarcodeID, Number = @Number, KindId = @KindID, IsMovementFromPallet = @IsMovementFromPallet WHERE BarcodeId = @BarcodeID";
                        var ret1 = ExecuteCeNonQuery(sql1, parameters1, CommandType.Text, ConnectServerCe.BarcodesServer);
                        var ret1_ = ExecuteCeNonQuery(sql1, parameters1_, CommandType.Text, ConnectServerCe.BackupBarcodesServer);
                        return ret1;
                        break;
                }

            }            
            return false;
        }

        public static DateTime GetLocalDbBarcodesDateCreated()
        {
            DateTime ret = new DateTime() ;
            const string sql = "SELECT DatabaseCreateTime FROM Settings";
            var parameters = new List<SqlCeParameter>();
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    ret = row.IsNull("DatabaseCreateTime") ? new DateTime() : Convert.ToDateTime(row["DatabaseCreateTime"]);
                }
            }
            return ret;
        }

        public static bool RefreshIsUplodedFalase()
        {
            const string sql = "UPDATE Logs SET IsUploaded = 0 WHERE IsUploaded is null AND LogDate < @DateScanned";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@DateScanned", DbType.DateTime)
                        {
                            Value = DateTime.Now.AddSeconds(-30)
                        }
                };
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
        }

        public static bool AddMessageToLog(Guid scanId, DateTime dateScanned, string barcode, int placeId, Guid? placeZoneId, int docTypeId, Guid? docId, bool? isUploaded, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity, int? quantityFractional, Guid? measureUnitId, Guid? fromProductId, int? fromPlaceId, Guid? fromPlaceZoneId, int? newWeight, DateTime? validUntilDate)
        {
            string sql = (Db.ExistsMessageLogFromLogId(scanId))
                    ? "UPDATE Logs SET LogDate = @DateScanned, Barcode = @Barcode, PlaceId = @PlaceId, PlaceZoneId = @PlaceZoneId, DocTypeId = @DocTypeId, DocId = @DocId, IsUploaded = @IsUploaded, ProductId = @ProductId, ProductKindId = @ProductKindId, NomenclatureId = @NomenclatureId, CharacteristicId = @CharacteristicId, QualityId = @QualityId, Quantity = @Quantity, MeasureUnitId = @MeasureUnitId, FromProductId = @FromProductId, FromPlaceId = @FromPlaceId, FromPlaceZoneId = @FromPlaceZoneId, NewWeight = @NewWeight, QuantityFractional = @QuantityFractional, ValidUntilDate = @ValidUntilDate WHERE LogId = @ScanId"
                    : "INSERT INTO Logs (LogId, LogDate, UserName, PersonId, Barcode, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded, ProductId, ProductKindId, NomenclatureId, CharacteristicId, QualityId, Quantity, MeasureUnitId, FromProductId, FromPlaceId, FromPlaceZoneId, NewWeight, QuantityFractional, ValidUntilDate) VALUES (@ScanId, @DateScanned, @UserName, @PersonId, @Barcode, @PlaceId, @PlaceZoneId, @DocTypeId, @DocId, @IsUploaded, @ProductId, @ProductKindId, @NomenclatureId, @CharacteristicId, @QualityId, @Quantity, @MeasureUnitId, @FromProductId, @FromPlaceId, @FromPlaceZoneId, @NewWeight, @QuantityFractional, @ValidUntilDate)";

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
                            Value = (isUploaded as object) ?? DBNull.Value
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
                        },
                    new SqlCeParameter("@MeasureUnitId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (measureUnitId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@FromProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromProductId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@FromPlaceId", SqlDbType.Int)
                        {
                            Value = (fromPlaceId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@FromPlaceZoneId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromPlaceZoneId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@NewWeight", SqlDbType.Int)
                        {
                            Value = (newWeight as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@QuantityFractional", SqlDbType.Int)
                        {
                            Value = (quantityFractional as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@ValidUntilDate", SqlDbType.DateTime)
                        {
                            Value = (validUntilDate as object) ?? DBNull.Value
                        }

                };
            
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
        }

        public static bool AddMessageToLog(Guid scanId, bool? isUploaded, string log)
        {
            //byte[] buffer = System.Text.Encoding.Default.GetBytes(log + "\r\n");
            //// запись массива байтов в файл
            //Shared._txtLogFile.Write(buffer, 0, buffer.Length);
            
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
                            Value = (isUploaded as object) ?? DBNull.Value
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

            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
        }

        public static bool AddMessageToLog(Guid scanId, bool? isUploaded, bool isDeleted, string log)
        {
            //byte[] buffer = System.Text.Encoding.Default.GetBytes(log + "\r\n");
            //// запись массива байтов в файл
            //Shared._txtLogFile.Write(buffer, 0, buffer.Length);
            
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
                            Value = (isUploaded as object) ?? DBNull.Value
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

            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
        }

        public static bool AddMessageToLog(string log, Guid? docId, Guid? productId)
        {
            //byte[] buffer = System.Text.Encoding.Default.GetBytes(log + "\r\n");
            //// запись массива байтов в файл
            //Shared._txtLogFile.Write(buffer, 0, buffer.Length);
            
            const string sql = "INSERT INTO Logs (Log, DocId, ProductId, UserName, PersonId) VALUES (@Log, @DocId, @ProductId, @UserName, @PersonId)";

            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Log", DbType.String)
                        {
                            Value = (log as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@DocId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (docId as object) ?? DBNull.Value
                        },
                    new SqlCeParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (productId as object) ?? DBNull.Value
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
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
        }

        public static bool AddMessageToLog(string log)
        {
            //byte[] buffer = System.Text.Encoding.Default.GetBytes(log + "\r\n");
            //// запись массива байтов в файл
            //Shared._txtLogFile.Write(buffer, 0, buffer.Length);
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
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
        }

        private static Stopwatch _stopWatch;
        public static Stopwatch stopWatch 
        {
            get
            {
                if (_stopWatch == null)
                {
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                }
                return _stopWatch;
            }
        }

        //private static DateTime? _start;
        //private static int _startTick;

        public static bool AddTimeStampToLog(string log)
        {
            return AddTimeStampToLog(log, false);
        }

        public static bool AddTimeStampToLog(string log, bool stop)
        {
            //if (!stopWatch.IsRunning)
            //{
            //    AddMessageToLog(log + ": " +//DateTime timeStamp =
            //    "Start StopWatch");
            //    stopWatch.Start();
            //}
            
            AddMessageToLog(DateTime.Now + " " +log + ": " +//DateTime timeStamp =
                    stopWatch.Elapsed.Milliseconds);
            if (stop)
                stopWatch.Stop();
            else
            {
                stopWatch.Reset();
                stopWatch.Start();
                AddMessageToLog(DateTime.Now + " " + log + ": " +"Start StopWatch "+
                    stopWatch.Elapsed.Milliseconds);
            }

            return true;
        }

        public static List<ScannedBarcode> GetBarcodesForCurrentUser()
        {
            List<ScannedBarcode> list = null;
            const string sql = "SELECT LogId, LogDate, Barcode, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded, ToDelete, IsDeleted, ProductId, ProductKindId, NomenclatureId, CharacteristicId, QualityId, Quantity, MeasureUnitId, FromProductId, FromPlaceId, FromPlaceZoneId, NewWeight, QuantityFractional, ValidUntilDate FROM Logs WHERE LogDate >= DATEADD(DAY,-4,GETDATE()) AND PersonId = @PersonId AND Barcode IS NOT NULL ORDER BY LogDate";
            
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (Shared.PersonId as object) ?? DBNull.Value
                        }
                };

            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer))
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
                                MeasureUnitId = row.IsNull("MeasureUnitId") ? (Guid?)null : new Guid(row["MeasureUnitId"].ToString()),
                                FromProductId = row.IsNull("FromProductId") ? (Guid?)null : new Guid(row["FromProductId"].ToString()),
                                FromPlaceId = row.IsNull("FromPlaceId") ? (int?)null : Convert.ToInt32(row["FromPlaceId"]),
                                FromPlaceZoneId = row.IsNull("FromPlaceZoneId") ? (Guid?)null : new Guid(row["FromPlaceZoneId"].ToString()),
                                NewWeight = row.IsNull("NewWeight") ? (int?)null : Convert.ToInt32(row["NewWeight"]),
                                QuantityFractional = row.IsNull("QuantityFractional") ? (int?)null : Convert.ToInt32(row["QuantityFractional"]),
                                ValidUntilDate = row.IsNull("ValidUntilDate") ? (DateTime?)null : Convert.ToDateTime(row["ValidUntilDate"])
                            };
                        list.Add(item);
                    };
                }
            }
            return list;
        }

        public static bool DeleteOldUploadedToServerLogs()
        {
            const string sql = "DELETE Logs WHERE IsUploadedToServer = 1 AND LogDate < DATEADD(DAY,-4,GETDATE())";
            return ExecuteCeNonQuery(sql, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.LogServer);
        }
        
        public static void UploadLogToServer()
        {
            const string sqlCE = "SELECT LogId, LogDate, Log, Barcode, UserName, PersonId, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded, ToDelete, IsDeleted, ProductId, ProductKindId, NomenclatureId, CharacteristicId, QualityId, Quantity, MeasureUnitId, FromProductId, FromPlaceId, FromPlaceZoneId, NewWeight, QuantityFractional, ValidUntilDate FROM Logs WHERE IsUploadedToServer = 0 ORDER BY LogDate";

            var parametersCE = new List<SqlCeParameter>();

            using (DataTable table = ExecuteCeSelectQuery(sqlCE, parametersCE, CommandType.Text, ConnectServerCe.LogServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    if (Shared.CountRowUploadToServerInOnePackage == 1)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            const string sql = "INSERT INTO LogFromMobileDevices(DeviceName, DeviceIP, UserName,PersonId,LogId,LogDate,Log,Barcode,PlaceId,DocTypeId,IsUploaded,DocId,PlaceZoneId,ToDelete,IsDeleted,ProductId,ProductKindId,NomenclatureId,CharacteristicId,QualityId,Quantity,MeasureUnitId,FromProductId,FromPlaceId,FromPlaceZoneId,NewWeight,QuantityFractional, ValidUntilDate) VALUES(@DeviceName, @DeviceIP, @UserName,@PersonId,@LogId,@LogDate,@Log,@Barcode,@PlaceId,@DocTypeId,@IsUploaded,@DocId,@PlaceZoneId,@ToDelete,@IsDeleted,@ProductId,@ProductKindId,@NomenclatureId,@CharacteristicId,@QualityId,@Quantity,@MeasureUnitId,@FromProductId,@FromPlaceId,@FromPlaceZoneId,@NewWeight,@QuantityFractional,@ValidUntilDate)";
                            var parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@DeviceName", SqlDbType.Text)
                                {
                                    Value = deviceName,
                                },
                            new SqlParameter("@DeviceIP", SqlDbType.Text)
                                {
                                    Value = deviceIP,
                                },
                            new SqlParameter("@UserName", SqlDbType.Text)
                                {
                                    Value = row["UserName"],
                                },
                            new SqlParameter("@PersonId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["PersonId"],
                                },
                            new SqlParameter("@LogId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["LogId"],
                                },
                            new SqlParameter("@LogDate", SqlDbType.DateTime)
                                {
                                    Value = row["LogDate"],
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
                                    Value = row["PlaceId"],
                                },
                            new SqlParameter("@DocTypeId", SqlDbType.Int)
                                {
                                    Value = row["DocTypeId"],
                                }, 
                            new SqlParameter("@IsUploaded", SqlDbType.Bit)
                                {
                                    Value = row["IsUploaded"],
                                },
                            new SqlParameter("@DocId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["DocId"],
                                },
                            new SqlParameter("@PlaceZoneId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["PlaceZoneId"],
                                },
                            new SqlParameter("@ToDelete", SqlDbType.Bit)
                                {
                                    Value = row["ToDelete"],
                                },
                            new SqlParameter("@IsDeleted", SqlDbType.Bit)
                                {
                                    Value = row["IsDeleted"],
                                },
                            new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["ProductId"],
                                },
                            new SqlParameter("@ProductKindId", SqlDbType.Int)
                                {
                                    Value = row["ProductKindId"],
                                },
                            new SqlParameter("@NomenclatureId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["NomenclatureId"],
                                },
                            new SqlParameter("@CharacteristicId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["CharacteristicId"],
                                },
                            new SqlParameter("@QualityId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["QualityId"],
                                },
                            new SqlParameter("@Quantity", SqlDbType.Int)
                                {
                                    Value = row["Quantity"],
                                },
                            new SqlParameter("@MeasureUnitId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["MeasureUnitId"],
                                },
                            new SqlParameter("@FromProductId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["FromProductId"],
                                },
                            new SqlParameter("@FromPlaceId", SqlDbType.Int)
                                {
                                    Value = row["FromPlaceId"],
                                },
                            new SqlParameter("@FromPlaceZoneId", SqlDbType.UniqueIdentifier)
                                {
                                    Value = row["FromPlaceZoneId"],
                                },
                            new SqlParameter("@NewWeight", SqlDbType.Int)
                                {
                                    Value = row["NewWeight"]
                                },
                            new SqlParameter("@QuantityFractional", SqlDbType.Int)
                                {
                                    Value = row["QuantityFractional"]
                                },
                            new SqlParameter("@ValidUntilDate", SqlDbType.DateTime)
                                {
                                    Value = row["ValidUntilDate"]
                                }
                        };
                            if (ExecuteSilentlyNonQuery(sql, parameters, CommandType.Text))
                            {
                                var g = row.IsNull("LogId") ? "" : new Guid(row["LogId"].ToString()).ToString();
                                var sqlCEUpdate = "UPDATE Logs SET IsUploadedToServer = 1 WHERE LogId = '" + g + "'";
                                var r = ExecuteCeNonQuery(sqlCEUpdate, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.LogServer);
                            };
                        }
                    }
                    else
                    // Вариант для добавления логов в БД пакетом по несколько строк. Поле DateAdd в БД не должно быть первичным ключем, так как при пакетном добавлении значение для всех строк будет одинаковое
                    {
                        string sql = String.Empty;
                        int i = 1;
                        string value = String.Empty;
                        string logIds = String.Empty;
                        foreach (DataRow row in table.Rows)
                        {
                            if (row["LogId"].ToString() != String.Empty)
                            {
                                logIds += (logIds.Length == 0 ? "'" : ",'") + row["LogId"] + "'";
                            }
                            value = "('" + (deviceName ?? "NULL") + "','" + (deviceIP ?? "NULL") + "'"
                                + "," + (row["UserName"].ToString() == String.Empty ? "NULL" : "'" + row["UserName"] + "'")
                                + "," + (row["PersonId"].ToString() == String.Empty ? "NULL" : "'" + row["PersonId"] + "'")
                                + "," + (row["LogId"].ToString() == String.Empty ? "NULL" : "'" + row["LogId"] + "'")
                                + "," + (row["LogDate"].ToString() == String.Empty ? "NULL" : "'" + ((DateTime)row["LogDate"]).ToString("yyyyMMdd HH:mm:ss.fff") + "'")
                                + "," + (row["Log"].ToString() == String.Empty ? "NULL" : "'" + row["Log"].ToString().Replace("'","''") + "'")
                                + "," + (row["Barcode"].ToString() == String.Empty ? "NULL" : "'" + row["Barcode"] + "'")
                                + "," + (row["PlaceId"].ToString() == String.Empty ? "NULL" : row["PlaceId"])
                                + "," + (row["DocTypeId"].ToString() == String.Empty ? "NULL" : row["DocTypeId"])
                                + "," + (row["IsUploaded"].ToString() == String.Empty ? "NULL" : ((bool)row["IsUploaded"] ? "1" : "0"))
                                + "," + (row["DocId"].ToString() == String.Empty ? "NULL" : "'" + row["DocId"] + "'")
                                + "," + (row["PlaceZoneId"].ToString() == String.Empty ? "NULL" : "'" + row["PlaceZoneId"] + "'")
                                + "," + (row["ToDelete"].ToString() == String.Empty ? "NULL" : ((bool)row["ToDelete"] ? "1" : "0"))
                                + "," + (row["IsDeleted"].ToString() == String.Empty ? "NULL" : ((bool)row["IsDeleted"] ? "1" : "0"))
                                + "," + (row["ProductId"].ToString() == String.Empty ? "NULL" : "'" + row["ProductId"] + "'")
                                + "," + (row["ProductKindId"].ToString() == String.Empty ? "NULL" : row["ProductKindId"])
                                + "," + (row["NomenclatureId"].ToString() == String.Empty ? "NULL" : "'" + row["NomenclatureId"] + "'")
                                + "," + (row["CharacteristicId"].ToString() == String.Empty ? "NULL" : "'" + row["CharacteristicId"] + "'")
                                + "," + (row["QualityId"].ToString() == String.Empty ? "NULL" : "'" + row["QualityId"] + "'")
                                + "," + (row["Quantity"].ToString() == String.Empty ? "NULL" : row["Quantity"])
                                + "," + (row["MeasureUnitId"].ToString() == String.Empty ? "NULL" : "'" + row["MeasureUnitId"] + "'")
                                + "," + (row["FromProductId"].ToString() == String.Empty ? "NULL" : "'" + row["FromProductId"] + "'")
                                + "," + (row["FromPlaceId"].ToString() == String.Empty ? "NULL" : "'" + row["FromPlaceId"] + "'")
                                + "," + (row["FromPlaceZoneId"].ToString() == String.Empty ? "NULL" : "'" + row["FromPlaceZoneId"] + "'")
                                + "," + (row["NewWeight"].ToString() == String.Empty ? "NULL" : "'" + row["NewWeight"] + "'")
                                + "," + (row["QuantityFractional"].ToString() == String.Empty ? "NULL" : "'" + row["QuantityFractional"] + "'")
                                + "," + (row["ValidUntilDate"].ToString() == String.Empty ? "NULL" : "'" + row["ValidUntilDate"] + "'")
                                + ")";
                            if (value.Length > 0)
                            {
                                if (i > 1)
                                    sql += ",";
                                sql += /*Environment.NewLine + */value;
                            }
                            value = string.Empty;
                            i++;
                            if (i > Shared.CountRowUploadToServerInOnePackage || sql.Length > 2500)
                            {
                                i = 1;
                                sql = "INSERT INTO LogFromMobileDevices(DeviceName, DeviceIP, UserName,PersonId,LogId,LogDate,Log,Barcode,PlaceId,DocTypeId,IsUploaded,DocId,PlaceZoneId,ToDelete,IsDeleted,ProductId,ProductKindId,NomenclatureId,CharacteristicId,QualityId,Quantity,MeasureUnitId,FromProductId,FromPlaceId,FromPlaceZoneId,NewWeight,QuantityFractional,ValidUntilDate) VALUES" + sql;
                                if (ExecuteSilentlyNonQuery(sql, new List<SqlParameter>(), CommandType.Text))
                                {
                                    var sqlCEUpdate = "UPDATE Logs SET IsUploadedToServer = 1 WHERE LogId in (" + logIds + ")";
                                    var r = ExecuteCeNonQuery(sqlCEUpdate, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.LogServer);
                                };
                                sql = String.Empty;
                                logIds = String.Empty;
                            }
                        }
                        if (i > 1)
                        {
                            sql = "INSERT INTO LogFromMobileDevices(DeviceName, DeviceIP, UserName,PersonId,LogId,LogDate,Log,Barcode,PlaceId,DocTypeId,IsUploaded,DocId,PlaceZoneId,ToDelete,IsDeleted,ProductId,ProductKindId,NomenclatureId,CharacteristicId,QualityId,Quantity,MeasureUnitId,FromProductId,FromPlaceId,FromPlaceZoneId,NewWeight,QuantityFractional,ValidUntilDate) VALUES"
                                + sql;
                            if (ExecuteSilentlyNonQuery(sql, new List<SqlParameter>(), CommandType.Text))
                            {
                                var sqlCEUpdate = "UPDATE Logs SET IsUploadedToServer = 1 WHERE LogId in (" + logIds + ")";
                                var r = ExecuteCeNonQuery(sqlCEUpdate, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.LogServer);
                            };
                            sql = String.Empty;
                            logIds = String.Empty;
                        }
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
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
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
            return ExecuteCeNonQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer);
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
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.LogServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        public static List<string> GetLogs()
        {
            List<string> list = null;
            const string sql = "SELECT LogId, Log, LogDate, UserId, PersonId FROM Logs ORDER BY LogDate";
            using (DataTable table = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.LogServer))
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

        public static DateTime? GetServerDateTime()
        {
            DateTime? serverDateTime = null;//new DateTime();
            const string sql = "SELECT GETUTCDATE() AS ServerDateTime";
            var parameters = new List<SqlParameter>();
            using (DataTable table = ExecuteSilentlySelectQuery(sql, parameters, CommandType.Text, 2))
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
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure, 3))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    person = new Person
                    {
                        PersonID = new Guid(row["PersonID"].ToString()),
                        Name = row["Name"].ToString(),
                        UserName = row["UserName"].ToString(),
                        b1 = row.IsNull("b1") ? (bool?)null : Convert.ToBoolean(row["b1"]),
                        b2 = row.IsNull("b2") ? (bool?)null : Convert.ToBoolean(row["b2"]),
                        b3 = row.IsNull("b3") ? (bool?)null : Convert.ToBoolean(row["b3"]),
                        b4 = row.IsNull("b4") ? (bool?)null : Convert.ToBoolean(row["b4"]),
                        i1 = row.IsNull("i1") ? (int?)null : Convert.ToInt32(row["i1"]),
                        i2 = row.IsNull("i2") ? (int?)null : (int?)Convert.ToInt32(row["i2"]),
                        s1 = row.IsNull("s1") ? null : row["s1"].ToString(),
                        s2 = row.IsNull("s2") ? null : row["s2"].ToString(),
                        PlaceID = Convert.ToInt32(row["PlaceID"])
                    };
                }
            }
            return person;
            
        }

        public static BindingList<ProductBase> DocShipmentOrderGoodProducts(Guid docShipmentOrderId, Guid nomenclatureId,
                                                             Guid characteristicId, Guid qualityId, DocDirection docDirection, bool isMovementForOrder, OrderType orderType)
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
                            Value = docDirection == DocDirection.DocOut || docDirection == DocDirection.DocOutIn
                        },
                    new SqlParameter("@IsInDoc", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocIn || docDirection == DocDirection.DocOutIn
                        },
                    new SqlParameter("@OrderType", SqlDbType.Int)
                        {
                            Value = (int) orderType
                        },
                    new SqlParameter("@IsMovementForOrder", SqlDbType.Bit)
                        {
                            Value = (bool) isMovementForOrder
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
                            Quantity = row["Quantity"].ToString(),
                            OutPlaceID = row.IsNull("OutPlaceID") ? (int?)null : Convert.ToInt32(row["OutPlaceID"]),
                            OutPlaceZoneID = row.IsNull("OutPlaceZoneID") ? (Guid?)null : new Guid(row["OutPlaceZoneID"].ToString()),
                            InPlaceID = row.IsNull("InPlaceID") ? (int?)null : Convert.ToInt32(row["InPlaceID"]),
                            InPlaceZoneID = row.IsNull("InPlaceZoneID") ? (Guid?)null : new Guid(row["InPlaceZoneID"].ToString()),
                            DateEnd = row.IsNull("DateEnd") ? (DateTime?)null : Convert.ToDateTime(row["DateEnd"]),
                            ProductKind = (ProductKind)Convert.ToInt32(row["ProductKindID"])
                        });
                    }
                }
            }

            return list;
        }

        public static BindingList<ProductBase> PalletItemProducts(Guid productId, Guid nomenclatureId,
                                                             Guid characteristicId, Guid qualityId)
        {
            BindingList<ProductBase> list = null;
            const string sql = "[mob_GetPalletItemProducts]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
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

        public static BindingList<DocOrder> PersonDocOrders(Guid personId, DocDirection docDirection, bool isMovementForOrder)
        {
            //Db.AddTimeStampToLog("Start PersonDocOrders");
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
                            Value = docDirection == DocDirection.DocOut
                        },
                    new SqlParameter("@DocDirection", SqlDbType.SmallInt)
                        {
                            Value = docDirection
                        },
                    new SqlParameter("@IsMovementForOrder", SqlDbType.Bit)
                        {
                            Value = isMovementForOrder
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
                            OutPlaceName = row["OutPlaceShortName"].ToString(),
                            InPlaceName = row["InPlaceShortName"].ToString(),
                            OrderType = (OrderType)Convert.ToInt32(row["OrderKindID"]),
                            OutPlaceID = row.IsNull("OutPlaceID") ? (int?)null : Convert.ToInt32(row["OutPlaceID"]),
                            InPlaceID = row.IsNull("InPlaceID") ? (int?)null : Convert.ToInt32(row["InPlaceID"]),
                            IsControlExec = row.IsNull("IsControlExec") ? false : Convert.ToBoolean(row["IsControlExec"]),
                            StartExec = row.IsNull("StartExec") ? (DateTime?)null : Convert.ToDateTime(row["StartExec"].ToString()),
                            EndExec = row.IsNull("EndExec") ? (DateTime?)null : Convert.ToDateTime(row["EndExec"].ToString()),
                            State = row["State"].ToString(),
                            CheckExistMovementToZone = row.IsNull("CheckExistMovementToZone") ? false : Convert.ToBoolean(row["CheckExistMovementToZone"])
                        });
                    }
                }
            }
            //Db.AddTimeStampToLog("End PersonDocOrders");
            return list ?? new BindingList<DocOrder>();
        }

        public static List<PlaceZone> GetPlaceZones(int placeId, Guid? placeZoneId)
        {
            var placeZones = placeZoneId == null
                ? Shared.PlaceZones.Where(p => p.PlaceId == placeId && p.PlaceZoneParentId == Guid.Empty && p.IsValid).ToList()
                : Shared.PlaceZones.Where(p => p.PlaceZoneParentId == placeZoneId && p.IsValid).ToList();
            return placeZones;
        }

        public static List<PlaceZone> GetWarehousePlaceZones(int placeId)
        {
            return GetWarehousePlaceZones(placeId, false);
        }
        
        public static List<PlaceZone> GetWarehousePlaceZones(int placeId, bool checkExistMovementToZone)
        {
            var placeZones = Shared.PlaceZones.Where(p => p.PlaceId == placeId && p.PlaceZoneParentId == Guid.Empty && p.IsValid).ToList();
            if (checkExistMovementToZone && placeZones != null && placeZones.Count > 0)
            {
                var placeZoneExistMovements = Db.GetPlaceZoneExistMovements(placeZones);
                foreach (var placeZone in placeZones)
                {
                    var zone = placeZoneExistMovements.FirstOrDefault(z => z.PlaceZoneId == placeZone.PlaceZoneId);
                    if (zone != null)
                        placeZone.IsExistMovementToZone = zone.IsExistMovementToZone;
                }
            }
            return placeZones;
        }

        public static List<PlaceZone> GetPlaceZoneExistMovements(List<PlaceZone> placeZones)
        {
            const string sql = "dbo.mob_GetPlaceZoneExistMovements";
            var parameters = new List<SqlParameter>
                {
                     new SqlParameter("@PlaceId", SqlDbType.Int)
                        {
                            Value = placeZones.First().PlaceId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = Shared.PersonId
                        },
                    new SqlParameter("@ShiftId", SqlDbType.SmallInt)
                        {
                            Value = Shared.ShiftId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var zone = placeZones.FirstOrDefault(z => z.PlaceZoneId == new Guid(row["PlaceZoneID"].ToString()));
                        if (zone != null)
                            zone.IsExistMovementToZone = Convert.ToBoolean(row["IsExistMovementToZone"]);
                    }
                }
            }
            return placeZones;
        }

        public static List<PlaceZone> GetPlaceZoneChilds(Guid placeZoneId)
        {
            var placeZones = Shared.PlaceZones.Where(p => p.PlaceZoneParentId == placeZoneId && p.IsValid).ToList();
            return placeZones;
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
                            ProductKindId = row.IsNull("ProductKindID") ? (byte?)null : Convert.ToByte(table.Rows[0]["ProductKindID"]),
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
                            CollectedQuantityUnits = row.IsNull("QuantityUnits") ? 0 : Convert.ToInt32(row["QuantityUnits"]),
                            IsEnableAddProductManual = Convert.ToBoolean(row["IsEnableAddProductManual"])
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

        public static BindingList<DocNomenclatureItem> DocNomenclatureItems(Guid docOrderId, OrderType orderType, DocDirection docDirection, bool isMovementForOrder)
        {
            BindingList<DocNomenclatureItem> list = null;
            var sql = "dbo.mob_GetOrderGoods";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@IsOutDoc", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocOut || docDirection == DocDirection.DocOutIn
                        },
                    new SqlParameter("@IsInDoc", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocIn || docDirection == DocDirection.DocOutIn
                        },
                    new SqlParameter("@OrderType", SqlDbType.Int)
                        {
                            Value = (int) orderType
                        },
                    new SqlParameter("@IsMovementForOrder", SqlDbType.Bit)
                        {
                            Value = (bool) isMovementForOrder
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<DocNomenclatureItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        string quantity;
                        decimal collectedQuantity;
                        int quantityUnits;
                        if (docDirection == DocDirection.DocOut || orderType == OrderType.MovementOrder || (docDirection == DocDirection.DocOutIn || orderType == OrderType.ShipmentOrder))
                        {
                            //СГБ учитываем по весу, а СГИ - по групповым упаковкам
                            //!(row["Quantity"].ToString().All(char.IsDigit)) - проверяем, является ли числом (внимание: не отрабатывает отрицательное значение)
                            quantity = row.IsNull("Quantity") ? "0" : (row.IsNull("CoefficientPackage") || !(row["Quantity"].ToString().All(char.IsDigit))) ? row["Quantity"].ToString() : (Convert.ToInt32(row["Quantity"]) / Convert.ToInt32(row["CoefficientPackage"])).ToString();
                            collectedQuantity = row.IsNull("OutQuantity") ? 0 : Convert.ToDecimal(row["OutQuantity"]);
                            quantityUnits = row.IsNull("OutQuantityUnits") ? 0 : Convert.ToInt32(row["OutQuantityUnits"]);
                        }
                        else
                        {
                            //quantity = row.IsNull("OutQuantity") ? "0" : (row.IsNull("CoefficientPackage") || !(row["OutQuantity"].ToString().All(char.IsDigit))) ? row["OutQuantity"].ToString() : (Convert.ToInt32(row["OutQuantity"]) / Convert.ToInt32(row["CoefficientPackage"])).ToString();
                            quantity = row.IsNull("Quantity") ? "0" : (row.IsNull("CoefficientPackage") || !(row["Quantity"].ToString().All(char.IsDigit))) ? row["Quantity"].ToString() : (Convert.ToInt32(row["Quantity"]) / Convert.ToInt32(row["CoefficientPackage"])).ToString();
                            collectedQuantity = row.IsNull("InQuantity") ? 0 : Convert.ToDecimal(row["InQuantity"]);
                            quantityUnits = row.IsNull("InQuantityUnits") ? 0 : Convert.ToInt32(row["InQuantityUnits"]);
                        }
                        list.Add(new DocNomenclatureItem
                        {
                            CharacteristicId = row.IsNull("1CCharacteristicID") ? new Guid() : new Guid(row["1CCharacteristicID"].ToString()),
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            QualityId = new Guid(row["1CQualityID"].ToString()),
                            ProductKindId = row.IsNull("ProductKindID") ? (byte?)null : Convert.ToByte(table.Rows[0]["ProductKindID"]),
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
                            CollectedQuantityUnits = quantityUnits,
                            IsEnableAddProductManual = Convert.ToBoolean(row["IsEnableAddProductManual"])
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
                                      WarehouseZones = GetWarehousePlaceZones(Convert.ToInt32(row["WarehouseID"])),
                                      PlaceGroupId = Convert.ToInt32(row["PlaceGroupID"])
                                  });
                }
            }
            return list;
        }

        public static List<MeasureUnitNomenclature> GetMeasureUnitsForNomenclature(Guid nomenclatureId, Guid characteristicId)
        {
            List<MeasureUnitNomenclature> list = null;
            const string sql = "[mob_GetMeasureUnitsForNomenclature]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@NomenclatureID", SqlDbType.UniqueIdentifier)
                        {
                            Value = nomenclatureId
                        },
                    new SqlParameter("@CharacteristicID", SqlDbType.UniqueIdentifier)
                        {
                            Value = characteristicId
                        }

                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new List<MeasureUnitNomenclature>();
                    list.AddRange(from DataRow row in table.Rows
                                  select new MeasureUnitNomenclature
                                  {
                                      MeasureUnitID = new Guid(row["MeasureUnitID"].ToString()),
                                      Name = row["MeasureUnitName"].ToString(),
                                      NomenclatureID = new Guid(row["NomenclatureID"].ToString()),
                                      CharacteristicID = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                                      IsActive = Convert.ToBoolean(row["IsActive"]),
                                      //Coefficient = Convert.ToInt32(row["Coefficient"])
                                      Numerator = Convert.ToInt32(row["Numerator"]),
                                      Denominator = Convert.ToInt32(row["Denominator"]),
                                      IsInteger = row.IsNull("IsInteger") ? false : Convert.ToBoolean(row["IsInteger"])
                                  });
                }
            }
            return list;
        }

        public static List<PlaceZone> GetPlaceZones()
        {
            List<PlaceZone> list = null;
            const string sql = "SELECT PlaceID, PlaceZoneID, Name, Barcode, PlaceZoneParentID, v, RootPlaceID, AllowedUseZonesOfPlaceGroup FROM vPlaceZones ORDER BY SortOrder";
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
                                      RootPlaceId = row.IsNull("RootPlaceID") ? (int?)null : Convert.ToInt32(row["RootPlaceID"]),
                                      AllowedUseZonesOfPlaceGroup = row.IsNull("AllowedUseZonesOfPlaceGroup") ? false : Convert.ToBoolean(row["AllowedUseZonesOfPlaceGroup"])
                                  });
                }
            }
            return list;
        }

        public static DbDeleteOperationProductResult DeleteLastProductFromMovement(string barcode, int placeId, Guid personId, DocDirection docDirection)
        {
            DbDeleteOperationProductResult result = null;
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
                    result = new DbDeleteOperationProductResult
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

        public static DbDeleteOperationProductResult DeleteProductFromMovement(string barcode, Guid docMovementId, DocDirection docDirection)
        {
            DbDeleteOperationProductResult result = null;
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
                    result = new DbDeleteOperationProductResult
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

        public static DbDeleteOperationProductResult DeleteProductFromMovementOnMovementID(Guid scanId)
        {
            return DeleteProductFromMovementOnMovementID(scanId, null, null, null, null, null, null);
        }

        public static DbDeleteOperationProductResult DeleteProductFromMovementOnMovementID(Guid scanId, DateTime? dateBeg, DateTime? dateEnd, int? outPlaceID, Guid? outPlaceZoneID, int? inPlaceID, Guid? inPlaceZoneID)
        {
            DbDeleteOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromMovementOnMovementIDV1]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@MovementID", SqlDbType.UniqueIdentifier)
                        {
                            Value = scanId
                        },
                    new SqlParameter("@DateBeg", SqlDbType.DateTime)
                        {
                            Value = (dateBeg as Object) ?? DBNull.Value
                        },
                        new SqlParameter("@DateEnd", SqlDbType.DateTime)
                        {
                            Value = (dateEnd as Object) ?? DBNull.Value
                        },
                    new SqlParameter("@OutPlaceID", SqlDbType.Int)
                        {
                            Value = (outPlaceID as Object) ?? DBNull.Value
                        },
                    new SqlParameter("@OutPlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (outPlaceZoneID as Object) ?? DBNull.Value
                        },
                    new SqlParameter("@InPlaceID", SqlDbType.Int)
                        {
                            Value = (inPlaceID as Object) ?? DBNull.Value
                        },
                    new SqlParameter("@InPlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (inPlaceZoneID as Object) ?? DBNull.Value
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbDeleteOperationProductResult
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
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
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

        public static DbDeleteOperationProductResult DeleteProductItemFromPalletOnMovementID(Guid scanId)
        {
            DbDeleteOperationProductResult result = null;
            const string sql = "dbo.[mob_DelProductFromPalletItemOnMovementID]";
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
                    result = new DbDeleteOperationProductResult
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
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
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

        public static DbDeleteOperationProductResult DeleteProductFromInventarisationOnInvProductID(Guid scanId)
        {
            DbDeleteOperationProductResult result = null;
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
                    result = new DbDeleteOperationProductResult
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
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
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
        public static DbDeleteOperationProductResult DeleteProductFromOrder(string barcode,
                                                                      Guid docOrderId, DocDirection docDirection)
        {
            DbDeleteOperationProductResult result = null;
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
                    result = new DbDeleteOperationProductResult
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

        public static DbDeleteOperationProductResult DeleteProductFromOrderOnMovementID(Guid scanId)
        {
            DbDeleteOperationProductResult result = null;
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
                    result = new DbDeleteOperationProductResult
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

            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID AS ProductID, Number, KindId AS ProductKindID, IsMovementFromPallet FROM Barcodes WHERE Barcode = @Barcode AND KindId IS NOT NULL";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbProductIdFromBarcodeResult();
                    if (!table.Rows[0].IsNull("ProductKindID"))
                        result.ProductKindId = (ProductKind?)Convert.ToInt16(table.Rows[0]["ProductKindID"]);
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
                    result.IsMovementFromPallet = table.Rows[0].IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(table.Rows[0]["IsMovementFromPallet"]);
                    result.CountProducts = 0;
                }
            }
            return result;
        }

        public static DbProductIdFromBarcodeResult GetFirstNomenclatureFromCashedBarcodes(string barcode)
        {
            DbProductIdFromBarcodeResult result = null;

            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID AS ProductID, Number, KindId AS ProductKindID, IsMovementFromPallet FROM Barcodes WHERE Barcode = @Barcode AND KindId IS NULL";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbProductIdFromBarcodeResult();
                    if (!table.Rows[0].IsNull("ProductKindID"))
                        result.ProductKindId = (ProductKind?)Convert.ToInt16(table.Rows[0]["ProductKindID"]);
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
                    result.IsMovementFromPallet = table.Rows[0].IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(table.Rows[0]["IsMovementFromPallet"]);
                    result.CountProducts = 0;
                }
            }
            return result;
        }

        public static List<DbProductIdFromBarcodeResult> GetAllNomenclatureFromCashedBarcodes(string barcode)
        {
            List<DbProductIdFromBarcodeResult> result = null;

            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID AS ProductID, Number, KindId AS ProductKindID, IsMovementFromPallet FROM Barcodes WHERE Barcode = @Barcode AND KindId IS NULL";
            var parameters = new List<SqlCeParameter>
                {
                    new SqlCeParameter("@Barcode", DbType.String)
                        {
                            Value = barcode
                        }
                };
            using (DataTable table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new List<DbProductIdFromBarcodeResult>();
                    result.AddRange(from DataRow row in table.Rows
                                    select new DbProductIdFromBarcodeResult
                                  {
                                      ProductKindId = row.IsNull("ProductKindID") ? (ProductKind?)null : (ProductKind?)Convert.ToInt16(row["ProductKindID"]),
                                      ProductId = row.IsNull("ProductID") ? new Guid() : new Guid(row["ProductID"].ToString()),
                                      NomenclatureId = row.IsNull("NomenclatureID") ? new Guid() : new Guid(row["NomenclatureID"].ToString()),
                                      CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                                      MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                                      QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString()),
                                      IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
                                  });
                }
            }
            return result;
        }


        public static DbProductIdFromBarcodeResult GetProductIdFromBarcodeOrNumber(string barcode)
        {
            DbProductIdFromBarcodeResult result = null;

            const string sql = "mob_GetProductIdFromBarcodeOrNumberV3";
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
                        result.ProductKindId = (ProductKind?)Convert.ToInt16(table.Rows[0]["ProductKindID"]);
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
                    result.IsMovementFromPallet = table.Rows[0].IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(table.Rows[0]["IsMovementFromPallet"]);
                    result.CountProducts = table.Rows[0].IsNull("CountProducts") ? 0 : Convert.ToInt32(table.Rows[0]["CountProducts"]);
                    result.CountFractionalProducts = table.Rows[0].IsNull("CountFractionalProducts") ? 0 : Convert.ToInt32(table.Rows[0]["CountFractionalProducts"]);
                    if (!table.Rows[0].IsNull("OriginalMeasureUnitID"))
                        result.OriginalMeasureUnitId = (Guid?)table.Rows[0]["OriginalMeasureUnitID"];
                    if (!table.Rows[0].IsNull("CoeffCountProductOriginalMeasureUnit"))
                        result.CoeffCountProductOriginalMeasureUnit = (int?)Convert.ToInt16(table.Rows[0]["CoeffCountProductOriginalMeasureUnit"]);
                    if (!table.Rows[0].IsNull("ValidUntilDate"))
                        result.ValidUntilDate = Convert.ToDateTime(table.Rows[0]["ValidUntilDate"]);
                    
                }
            }
            return result;
        }

        public static DbOrderOperationProductResult AddProductIdToOrder(Guid? scanId, Guid docOrderId, OrderType orderType, Guid personId
            , Guid productId, DocDirection docDirection, EndPointInfo endPointInfo, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity, int? quantityFractional, Guid measureUnitId, Guid? fromProductId, int? fromPlaceId, Guid? fromPlaceZoneId, int? newWeight, DateTime? validUntilDate)
        {
            DbOrderOperationProductResult result = null;
            const string sql = "dbo.[mob_AddScanIdToOrderV6]";
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
                            Value = docDirection == DocDirection.DocOut || docDirection == DocDirection.DocOutIn
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
                    new SqlParameter("@QuantityFractional", SqlDbType.Int)
                        {
                            Value = (quantityFractional as object) ?? DBNull.Value
                        },
                    new SqlParameter("@MeasureUnitID", SqlDbType.UniqueIdentifier)
                        {
                            Value = measureUnitId
                        },
                    new SqlParameter("@ShiftId", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        },
                    new SqlParameter("@FromProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromProductId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@InPlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = endPointInfo != null && endPointInfo.PlaceZoneId != null ? (endPointInfo.PlaceZoneId as object) : DBNull.Value
                        },
                    new SqlParameter("@FromPlaceID", SqlDbType.Int)
                        {
                            Value = (fromPlaceId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@FromPlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromPlaceZoneId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@NewWeight", SqlDbType.Int)
                        {
                            Value = (newWeight as object) ?? DBNull.Value
                        },
                    new SqlParameter("@ValidUntilDate", SqlDbType.DateTime)
                        {
                            Value = (validUntilDate as object) ?? DBNull.Value
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbOrderOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        ScanId = scanId
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.OutPlace = table.Rows[0]["OutPlace"].ToString();
                        result.OutPlaceZone = table.Rows[0]["OutPlaceZone"].ToString();
                        result.InPlace = table.Rows[0]["InPlace"].ToString();
                        result.InPlaceZone = table.Rows[0]["InPlaceZone"].ToString();
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = table.Rows[0].IsNull("Quantity") ? 0 : Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            //MeasureUnitId = table.Rows[0].IsNull("MeasureUnitID") ? Guid.Empty : new Guid(table.Rows[0]["MeasureUnitID"].ToString()),
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

        public static DbMoveOperationProductResult MoveProduct(Guid personId, Guid productId, EndPointInfo endPointInfo, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity)
        {
            DbMoveOperationProductResult acceptProductResult = null;
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
                    acceptProductResult = new DbMoveOperationProductResult
                    {
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyAdded =
                            !table.Rows[0].IsNull("AlreadyAdded") &&
                            Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        PlaceZoneId = table.Rows[0].IsNull("PlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["PlaceZoneID"].ToString())
                    };
                    if (acceptProductResult != null & !table.Rows[0].IsNull("NomenclatureID"))
                    {
                        acceptProductResult.OutPlace = table.Rows[0]["OutPlace"].ToString();
                        acceptProductResult.OutPlaceZone = table.Rows[0]["OutPlaceZone"].ToString();
                        acceptProductResult.InPlace = table.Rows[0]["InPlace"].ToString();
                        acceptProductResult.InPlaceZone = table.Rows[0]["InPlaceZone"].ToString();
                        acceptProductResult.Product = new Product
                        {
                            //ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString(),
                            CoefficientPackage = table.Rows[0].IsNull("CoefficientPackage") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPackage"]),
                            CoefficientPallet = table.Rows[0].IsNull("CoefficientPallet") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPallet"])
                        };
                    };
                }
            }
            return acceptProductResult;
        }

        public static DbMoveOperationProductResult MoveProduct(Guid? scanId, Guid personId, Guid productId, EndPointInfo endPointInfo, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity, int quantityFractional, Guid measureUnitId, Guid? fromProductId, int? fromPlaceId, Guid? fromPlaceZoneId, int? newWeight, DateTime? validUntilDate)
        {
            DbMoveOperationProductResult acceptProductResult = null;
            const string sql = "dbo.[mob_AddScanIdToMovementV6]";
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
                    new SqlParameter("@QuantityFractional", SqlDbType.Int)
                        {
                            Value = quantityFractional
                        },
                    new SqlParameter("@MeasureUnitID", SqlDbType.UniqueIdentifier)
                        {
                            Value = measureUnitId
                        },
                    new SqlParameter("@ShiftId", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        },
                    new SqlParameter("@FromProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromProductId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@IntoProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = DBNull.Value
                        },
                    new SqlParameter("@FromPlaceID", SqlDbType.Int)
                        {
                            Value = (fromPlaceId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@FromPlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromPlaceZoneId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@NewWeight", SqlDbType.Int)
                        {
                            Value = (newWeight as object) ?? DBNull.Value
                        },
                    new SqlParameter("@ValidUntilDate", SqlDbType.DateTime)
                        {
                            Value = (validUntilDate as object) ?? DBNull.Value
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    acceptProductResult = new DbMoveOperationProductResult
                    {
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyAdded =
                            !table.Rows[0].IsNull("AlreadyAdded") &&
                            Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        PlaceZoneId = table.Rows[0].IsNull("InPlaceZoneID") ? new Guid() : new Guid(table.Rows[0]["InPlaceZoneID"].ToString()),
                        ScanId = scanId
                    };
                    if (acceptProductResult != null & !table.Rows[0].IsNull("NomenclatureID"))
                    {
                        acceptProductResult.Product = new Product
                        {
                            //ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = table.Rows[0].IsNull("CharacteristicID") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            QualityId = new Guid(table.Rows[0]["QualityID"].ToString()),
                            Quantity = table.Rows[0].IsNull("Quantity") ? 0 : Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString(),
                            CoefficientPackage = table.Rows[0].IsNull("CoefficientPackage") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPackage"]),
                            CoefficientPallet = table.Rows[0].IsNull("CoefficientPallet") ? (int?)null : Convert.ToInt32(table.Rows[0]["CoefficientPallet"])
                        };
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
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure, 2))
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
            return ExecuteSelectQuery(sql, parameters, commandType, false, null);
        }

        private static DataTable ExecuteSelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType, int commandTimeout)
        {
            return ExecuteSelectQuery(sql, parameters, commandType, false, commandTimeout);
        }

        private static DataTable ExecuteSilentlySelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType)
        {
            return ExecuteSelectQuery(sql, parameters, commandType, true, null);
        }

        private static DataTable ExecuteSilentlySelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType, int commandTimeout)
        {
            return ExecuteSelectQuery(sql, parameters, commandType, true, commandTimeout);
        }

        private static DataTable ExecuteSelectQuery(string sql, IEnumerable<SqlParameter> parameters,
                                                    CommandType commandType, bool IsSilently, int? commandTimeout)
        {
            try
            {
                //Db.AddTimeStampToLog("Start ExecuteSelectQuery");
                DataTable table = new DataTable();
                if (!ConnectionState.IsConnected)//!ConnectionState.GetCheckerRunning)
                {
                    if (!IsSilently) Shared.LastQueryCompleted = null;
                }
                else
                {
                    if (1 == 0 && !ConnectionState.GetServerPortEnabled)
                    {//для запуска StartChecker
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                    }
                    else
                    {
                        //Db.AddTimeStampToLog("New ExecuteSelectQuery.SqlCommand");
                        if (!IsSilently) Cursor.Current = Cursors.WaitCursor;
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                        var exc = false;
                        using (var command = new SqlCommand(sql))
                        {
                            command.Connection = Shared.Connection;
                            command.CommandType = commandType;
                            if (commandTimeout != null)
                                command.CommandTimeout = (int)commandTimeout;
                            //command.CommandTimeout = 3600;
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                            //Db.AddTimeStampToLog("New ExecuteSelectQuery.SqlDataAdapter");
                            using (var sda = new SqlDataAdapter(command))
                            {
                                //Db.AddMessageToLog("7.1.5:" + DateTime.Now);
                                try
                                {
                                    sda.Fill(table);
                                    Shared.LastQueryCompleted = true;
                                }
                                catch (Exception ex)
                                {
                                    //#if DEBUG
                                    //                            MessageBox.Show(ex.Message);
                                                                var sqlex = ex as SqlException;
                                                                if (sqlex != null)
                                                                    //if (sqlex.Message == "General network error.  Check your network documentation.")
                                                                        ConnectionState.ConnectionLost();
                                    //                                foreach (var error in sqlex.Errors)
                                    //                                {
                                    //                                    if (error.ToString() == "General network error.  Check your network documentation.")
                                    //                                        ConnectionLost();
                                    ////                                    MessageBox.Show(error.ToString());
                                    //                                }
                                    //#endif
                                    //if (!IsSilently) Shared.LastQueryCompleted = false;
                                    //table = null;
                                    exc = true;
                                }
                                /*if (exc)
                                {
                                    System.Threading.Thread.Sleep(220);
                                    try
                                    {
                                        command.Connection = new SqlConnection(ConnectionString);
                                        table.Clear();
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
                                }*/
                            }
                        }
                        if (1==0 && exc)
                        {
                            using (var command = new SqlCommand(sql))
                            {
                                command.Connection = new SqlConnection(ConnectionString);
                                command.CommandType = commandType;
                                //command.CommandTimeout = 3600;
                                foreach (SqlParameter parameter in parameters)
                                {
                                    command.Parameters.Add(new SqlParameter(parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Direction, parameter.IsNullable, parameter.Precision, parameter.Scale, parameter.SourceColumn, parameter.SourceVersion, parameter.Value));
                                }
                                //Db.AddTimeStampToLog("New ExecuteSelectQuery.SqlDataAdapter");
                                using (var sda = new SqlDataAdapter(command))
                                {
                                    //Db.AddMessageToLog("7.1.5:" + DateTime.Now);
                                        try
                                        {
                                            table.Clear();
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
                }
                //Db.AddTimeStampToLog("End ExecuteSelectQuery");
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
                if (ConnectionState.IsConnected)//(!ConnectionState.GetCheckerRunning)
                {
                    if (1==0 && !ConnectionState.GetServerPortEnabled)
                    {//для запуска StartChecker
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                    }
                    else
                    {
                        if (!IsSilently) Cursor.Current = Cursors.WaitCursor;
                        if (!IsSilently) Shared.LastQueryCompleted = null;
                        var exc = false; 
                        try
                        {
                            //using (var connection = new SqlConnection(ConnectionString))
                            {
                                //connection.Open();
                                using (var command = new SqlCommand(sql))
                                {
                                    command.Connection = Shared.Connection;
                                    command.CommandType = commandType;
                                    command.Parameters.Clear();
                                    foreach (SqlParameter parameter in parameters)
                                    {
                                        command.Parameters.Add(parameter);
                                    }

                                    //try
                                    {
                                        if (command.Connection.State != System.Data.ConnectionState.Open)
                                            command.Connection.Open();
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
                            var sqlex = ex as SqlException;
                            if (sqlex != null)
                                if (sqlex.Message == "General network error.  Check your network documentation.")
                                    ConnectionState.ConnectionLost();
                            exc = true; 
                            //if (!IsSilently) Shared.LastQueryCompleted = false;
                            //ret = false;
                        }
                        if (1 == 0 && exc)
                        {
                            try
                            {
                                //using (var connection = new SqlConnection(ConnectionString))
                                {
                                    //connection.Open();
                                    using (var command = new SqlCommand(sql))
                                    {
                                        command.Connection = new SqlConnection(ConnectionString);
                                        command.CommandType = commandType;
                                        command.Parameters.Clear();
                                        foreach (SqlParameter parameter in parameters)
                                        {
                                            command.Parameters.Add(new SqlParameter(parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Direction, parameter.IsNullable, parameter.Precision, parameter.Scale, parameter.SourceColumn, parameter.SourceVersion, parameter.Value));
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
                                                    CommandType commandType, ConnectServerCe serverCe)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DataTable table;
                Shared.LastCeQueryCompleted = false;
                using (var command = new SqlCeCommand(sql))
                {
                    command.Connection = new SqlCeConnection(ConnectionCeString(serverCe));
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
                                                    CommandType commandType, ConnectServerCe serverCe)
        {
            try
            {
                bool ret = false;
                //Cursor.Current = Cursors.WaitCursor;
                using (var connection = new SqlCeConnection(ConnectionCeString(serverCe)))
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
                            ProductKindId = row.IsNull("ProductKindID") ? (byte?)null : Convert.ToByte(table.Rows[0]["ProductKindID"]),
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            CollectedQuantity = Convert.ToDecimal(row["Quantity"]),
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString(),
                            CollectedQuantityUnits = row.IsNull("QuantityUnits") ? 0 : Convert.ToInt32(row["QuantityUnits"]),
                            IsEnableAddProductManual = Convert.ToBoolean(row["IsEnableAddProductManual"])
                        });
                    }
                }
            }
            return list;
        }

        internal static DbInventarisationOperationProductResult AddProductIdToInventarisation(Guid? scanId, Guid docInventarisationId, Guid personId, EndPointInfo endPointInfo, Guid productId, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity)
        {
            DbInventarisationOperationProductResult result = null;
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
                    result = new DbInventarisationOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        ScanId = scanId
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
                                          CharacteristicId = row.IsNull("CharacteristicId") ? Guid.Empty : new Guid(row["CharacteristicId"].ToString()),
                                          QualityId = new Guid(row["QualityId"].ToString()),
                                          ProductKindId = row.IsNull("ProductKindID") ? (byte?)null : Convert.ToByte(table.Rows[0]["ProductKindID"]),
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
        /// <param name="scanId">id операции сканирования</param>
        /// <param name="productId">Id паллеты</param>
        /// <param name="docOrderId">id приказа</param>
        /// <param name="getProductResult">номенклатура с количеством</param>
        /// <returns></returns>
        internal static AddPalletItemResult AddItemToPallet(Guid? scanId, Guid productId, Guid? docOrderId, int? productKindId, Guid nomenclatureId, Guid characteristicId
            , Guid qualityId, int quantity, Guid? fromProductId)
        {
            AddPalletItemResult result = null;
            const string sql = "dbo.mob_AddItemToPalletV1";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ScanID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (scanId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (docOrderId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = Shared.PersonId
                        },
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
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
                        },
                    new SqlParameter("@FromProductID", SqlDbType.UniqueIdentifier)
                        {
                            Value = (fromProductId as object) ?? DBNull.Value
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
                        result.CharacteristicId = table.Rows[0].IsNull("CharacteristicId") ? Guid.Empty : new Guid(table.Rows[0]["CharacteristicId"].ToString());
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

        internal static BindingList<PalletListItem> GetNewPalletsByPerson(Guid? docOrderId)
        {
            var pallets = new BindingList<PalletListItem>();
            const string sql = "dbo.mob_GetOrderPallets";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = Shared.PersonId
                        },
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (docOrderId  as object) ?? DBNull.Value
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        pallets.Add(new PalletListItem(new Guid(row["ProductId"].ToString()), row["Number"].ToString(), Convert.ToDateTime(row["Date"]), row["PersonName"].ToString()));
                    }
                }
            }
            return pallets;
        }

        internal static DbCreateNewPalletResult CreateNewPallet(Guid productId, Guid? docOrderId, int placeId, Guid? placeZoneId)
        {
            DbCreateNewPalletResult result = null;
            const string sql = "dbo.mob_CreateNewPallet";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductId", SqlDbType.UniqueIdentifier)
                        {
                            Value = productId
                        },
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                        {
                            Value = (docOrderId as object) ?? DBNull.Value
                        },
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = placeId,
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId,
                        },
                    new SqlParameter("@ShiftID", SqlDbType.TinyInt)
                        {
                            Value = Shared.ShiftId
                        },
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = Shared.PersonId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    result = new DbCreateNewPalletResult
                    {
                        ProductID = table.Rows[0].IsNull("ProductID") ? Guid.Empty : new Guid(table.Rows[0]["ProductID"].ToString()),
                        Number = table.Rows[0].IsNull("ProductID") ? String.Empty : table.Rows[0]["Number"].ToString(),
                        Date = table.Rows[0].IsNull("ProductID") ? (DateTime?)null : Convert.ToDateTime(table.Rows[0]["Date"]),
                        Person = table.Rows[0].IsNull("ProductID") ? String.Empty : table.Rows[0]["PersonName"].ToString(),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString()
                    };
                }
                else
                {
                    result = new DbCreateNewPalletResult
                    {
                        ProductID = null,
                        Number = String.Empty,
                        Date = null,
                        Person = String.Empty,
                        ResultMessage = "Не удалось создать паллету, возможно была потеряна связь"
                    };
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
                            Barcode = row["Barcode"].ToString(),
                            IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
                        });
                    }
                }
            }
            return list;
        }

        public static DataTable GetCountBarcodes()
        {
            DataTable ret;
            const string sql = "SELECT Count(*) AS CountBarcodes, Sum(Case When KindId is null Then 1 Else 0 End) AS CountNomenclatures, Sum(Case When KindId is not null Then 1 Else 0 End) AS CountProducts FROM Barcodes ";
            ret = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.BarcodesServer);
            return ret;
        }

        public static int GetCountNomenclatureBarcodes()
        {
            int ret;
            const string sql = "SELECT Count(*) AS CountNomenclatures FROM Barcodes WHERE KindId is null ";
            using (DataTable table = ExecuteCeSelectQuery(sql, new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.BarcodesServer))
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
            const string sql = "SELECT Barcode, Name, NomenclatureID, CharacteristicID, QualityID, MeasureUnitID, BarcodeID, IsMovementFromPallet FROM Barcodes WHERE Barcode = @Barcode ORDER BY NomenclatureID, CharacteristicID";
            SqlCeParameter p = new SqlCeParameter();
            p.ParameterName = "@Barcode";
            p.DbType = DbType.String;
            p.Value = barcode;
            var parameters = new List<SqlCeParameter>();
            parameters.Add(p);
            DataTable table = null;
            if (Shared.IsFindBarcodeFromFirstLocalAndNextOnline)
                table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer);
            //if (table = ExecuteCeSelectQuery(sql, parameters, CommandType.Text, ConnectServerCe.BarcodesServer);
            {
                if (table == null || table.Rows.Count == 0)
                {
                    if (!Shared.LastCeQueryCompleted)
                        Shared.SaveToLogError(@"Ошибка при поиске в локальной БД " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                    else
                        if (Shared.IsFindBarcodeFromFirstLocalAndNextOnline)
                            Shared.SaveToLogInformation(@"Не найден в локальной БД " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                    const string sqlDB = "dbo.mob_GetNomenclatureCharacteristicQualityFromBarcode";
                    var parametersDB = new List<SqlParameter>
                    {
                    new SqlParameter("@Barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        }
                    };
                    table = ExecuteSelectQuery(sqlDB, parametersDB, CommandType.StoredProcedure);
                    if (table == null || table.Rows.Count == 0)
                    {
                        if (!Shared.LastCeQueryCompleted)
                            Shared.SaveToLogError(@"Ошибка при поиске в серверной БД " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                        else
                            Shared.SaveToLogInformation(@"Не найден в серверной БД " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                    }
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
                                      BarcodeId = new Guid(row["BarcodeID"].ToString()),
                                      IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
                                  });
                    }
                }

                

            }
            return list;
        }
        

        public static int? GetBarcodes1C(DateTime date)
        {
            int? list = null;
            const string sql = "dbo.mob_GetBarcodes1C";
            using (DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = 0;
                    var form = new ProgressBarForm(date, date, table, @"Идет загрузка номенклатур");
                            if (form != null)
                            {
                                if (ExecuteCeNonQuery("DELETE Barcodes WHERE KindId is null", new List<SqlCeParameter>(), CommandType.Text, ConnectServerCe.BarcodesServer))
                                {
                                    var r = form.ShowDialog();
                                    var endDate = form.ret;
                                    list = form.retCount;
                                }
                            }
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
                             KindId = row.IsNull("KindID") ? null : (int?)Convert.ToInt32(row["KindID"]),
                             IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
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
                            if (table.Rows.Count > Shared.MaxCountRowInPackOnFirstUpdateCashedBarcodes)
                            {
                                endDate = ret;
                            }
                            else
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
                                     KindId = row.IsNull("KindID") ? null : (int?)Convert.ToInt32(row["KindID"]),
                                     IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
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

        public static string CheckWhetherProductCanBeWithdrawal(Guid productID, int countWithdrawal)
        {
            using (var command = new SqlCommand())
            {
                string checkResult = String.Empty;
                try
                {
                    //using (var connection = new SqlConnection(ConnectionString))
                    {
                        //connection.Open();
                        command.Connection = Shared.Connection;
                        var sql = "SELECT dbo.CheckWhetherProductCanBeWithdrawalV1(@ProductID, @CountWithdrawal)";
                        command.CommandText = sql;
                        command.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                                {
                                    Value = productID
                                });
                        command.Parameters.Add(new SqlParameter("@CountWithdrawal", SqlDbType.Int)
                                {
                                    Value = countWithdrawal
                                });
                        checkResult = Convert.ToString(command.ExecuteScalar());
                        //connection.Close();
                    }
                }
                catch (Exception _)
                { }
                return checkResult;
            }
        }

        public static Guid? GetProductNomenclature(Guid productId)
        {
            Guid? nomenclatureId = null;
            const string sql = "SELECT TOP 1 p.[1CNomenclatureID] AS NomenclatureID FROM vProductsBaseInfo p WHERE p.ProductID = @ProductID";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ProductID", SqlDbType.UniqueIdentifier)
                    {
                         Value = productId
                    }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null)
                    if (table.Rows.Count > 0)
                    {
                        DataRow row = table.Rows[0];
                        var nomenclatureIdString = row["NomenclatureID"].ToString();
                        nomenclatureId = new Guid(nomenclatureIdString);
                    }
            }
            return nomenclatureId;
        }

        /// <summary>
        /// Возвращает продукцию в зоне, если она только одна в этой зоне
        /// </summary>
        /// <param name="placeZoneId"></param>
        /// <returns></returns>
        public static DbProductIdFromBarcodeResult GetSingleProductInPlaceZone(Guid placeZoneId)
        {
            DbProductIdFromBarcodeResult result = null;
            const string sql = "mob_GetSingleProductInPlaceZone";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                    {
                         Value = placeZoneId
                    }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null)
                    if (table.Rows.Count > 0)
                    {
                        result = new DbProductIdFromBarcodeResult();
                        if (!table.Rows[0].IsNull("ProductKindID"))
                            result.ProductKindId = (ProductKind?)Convert.ToInt16(table.Rows[0]["ProductKindID"]);
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
                        if (!table.Rows[0].IsNull("CountProducts"))
                            result.CountProducts = Convert.ToInt16(table.Rows[0]["CountProducts"]);
                    }
            }
            return result;
        }

        internal static string UpdateStartExecInDocOrder(Guid docOrderId, Guid personId)
        {
            string result;
            const string sql = "dbo.mob_UpdateStartExecInDocOrder";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
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
                    result = "Не удалось сохранить начало погрузки на сервере";
                }
            }
            return result;
        }

        internal static string UpdateEndExecInDocOrder(Guid docOrderId, Guid personId)
        {
            string result;
            const string sql = "dbo.mob_UpdateEndExecInDocOrder";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderId", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@PersonId", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
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
                    result = "Не удалось сохранить окончание погрузки на сервере";
                }
            }
            return result;
        }

        public static BindingList<ChooseNomenclatureItem> GetNomenclatureCharacteristicQualityFromId(Guid nomenclatureId, Guid characteristicId, Guid qualityId)
        {
            BindingList<ChooseNomenclatureItem> list = null;
            const string sql = "dbo.mob_GetNomenclatureCharacteristicQualityFromID";
            var parameters = new List<SqlParameter>
                {
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
                            Barcode = row["Barcode"].ToString(),
                            IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
                        });
                    }
                }
            }
            return list;
        }

        public static BindingList<ChooseNomenclatureItem> GetNomenclatureInPlaceZone(int placeId, Guid placeZoneId, bool isFilteringOnNomenclature, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId)
        {
            BindingList<ChooseNomenclatureItem> list = new BindingList<ChooseNomenclatureItem>(); 
            const string sql = "dbo.mob_GetNomenclatureInPlaceZoneV1";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                        new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId
                        },
                        new SqlParameter("@IsFilteringOnNomenclature", SqlDbType.Bit)
                        {
                            Value = isFilteringOnNomenclature
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
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    //list = new BindingList<ChooseNomenclatureItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ChooseNomenclatureItem
                        {
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            CharacteristicId = row.IsNull("1CCharacteristicID") ? Guid.Empty : new Guid(row["1CCharacteristicID"].ToString()),
                            QualityId = row.IsNull("1CQualityID") ? Guid.Empty : new Guid(row["1CQualityID"].ToString()),
                            ProductKindId = row.IsNull("ProductKindID") ? (byte?)null : Convert.ToByte(table.Rows[0]["ProductKindID"]),
                            Name = row["Name"].ToString(),
                            Barcode = row["Barcode"].ToString(),
                            MeasureUnitId = row.IsNull("MeasureUnitID") ? Guid.Empty : new Guid(row["MeasureUnitID"].ToString()),
                            MeasureUnits = row["MeasureUnits"].ToString(),
                            IsMovementFromPallet = row.IsNull("IsMovementFromPallet") ? false : Convert.ToBoolean(row["IsMovementFromPallet"])
                        });
                    }
                }
            }
            return list;
        }

        //public static List<MeasureUnit> GetNomenclatureMeasureUnits(Guid nomenclatureId)
        //{
        //    List<MeasureUnit> list = null;
        //    const string sql = "dbo.mob_GetNomenclatureMeasureUnits";
        //    var parameters = new List<SqlParameter>
        //        {
        //            new SqlParameter("@PlaceID", SqlDbType.Int)
        //                {
        //                    Value = placeId
        //                },
        //                new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
        //                {
        //                    Value = placeZoneId
        //                },
        //        };
        //    using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
        //    {
        //        if (table != null && table.Rows.Count > 0)
        //        {
        //            list = new BindingList<ChooseNomenclatureItem>();
        //            foreach (DataRow row in table.Rows)
        //            {
        //                list.Add(new ChooseNomenclatureItem
        //                {
        //                    NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
        //                    CharacteristicId = row.IsNull("1CCharacteristicID") ? Guid.Empty : new Guid(row["1CCharacteristicID"].ToString()),
        //                    QualityId = new Guid(row["1CQualityID"].ToString()),
        //                    Name = row["Name"].ToString(),
        //                    Barcode = row["Barcode"].ToString(),
        //                    MeasureUnitId = new Guid(row["MeasureUnitID"].ToString()),
        //                    MeasureUnits = Db.GetNomenclatureMeasureUnits(new Guid(row["1CNomenclatureID"].ToString()))
        //                });
        //            }
        //        }
        //    }
        //    return list;
        //}
    }
}