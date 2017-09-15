using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob
{
    public static class Db
    {
        private static string ConnectionString { get; set; }

        public static void SetConnectionString(string ipAddress, string database, string user, string password,
                                               string timeout)
        {
            ConnectionString = "Data Source=" + ipAddress + ";Initial Catalog=" + database + "" +
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

        public static Person PersonByBarcode(string barcode)
        {
            Person person = null;
            const string sql = "SELECT PersonID, Name FROM Persons WHERE Barcode = @Barcode";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BarCode", SqlDbType.VarChar)
                };
            parameters[0].Value = barcode;
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    person = new Person
                    {
                        PersonID = new Guid(row["PersonID"].ToString()),
                        Name = row["Name"].ToString()
                    };
                }
            }
            return person;
        }

        public static DataTable DocShipmentOrderGoodProducts(Guid docShipmentOrderId, Guid nomenclatureId,
                                                             Guid characteristicId, DocDirection docDirection)
        {
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
                    new SqlParameter("@IsOutDoc", SqlDbType.Bit)
                        {
                            Value = docDirection == DocDirection.DocOut
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return table;
        }

        public static DataTable GetInventarisations()
        {
            const string sql = "dbo.mob_GetInventarisations";
            DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure);
            return table;
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
            List<PlaceZone> list = null;
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
            return list;
        }

        public static List<PlaceZone> GetPlaceZoneChilds(Guid placeZoneId)
        {
            List<PlaceZone> list = null;
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
            return list;
        }

        public static BindingList<MovementProduct> GetMovementProducts(int placeId)
        {
            BindingList<MovementProduct> list = null;
            const string sql = "dbo.mob_GetLastMovementProducts";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PlaceIdTo", SqlDbType.Int)
                        {
                            Value = placeId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    list = new BindingList<MovementProduct>();
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new MovementProduct
                        {
                            Barcode = row["Barcode"].ToString(),
                            Number = row["Number"].ToString(),
                            NomenclatureName = row["ShortNomenclatureName"].ToString(),
                            Quantity = Convert.ToDecimal(row["Quantity"]),
                            DocMovementId = new Guid(row["DocMovementID"].ToString()),
                            Date = Convert.ToDateTime(row["Date"].ToString())
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
                        if (docDirection == DocDirection.DocOut || orderType == OrderType.MovementOrder)
                        {
                            quantity = row.IsNull("Quantity") ? "0" : row["Quantity"].ToString();
                            collectedQuantity = row.IsNull("OutQuantity") ? 0 : Convert.ToDecimal(row["OutQuantity"]);
                        }
                        else
                        {
                            quantity = row.IsNull("OutQuantity") ? "0" : row["OutQuantity"].ToString();
                            collectedQuantity = row.IsNull("InQuantity") ? 0 : Convert.ToDecimal(row["InQuantity"]);
                        }
                        list.Add(new DocNomenclatureItem
                        {
                            CharacteristicId = new Guid(row["1CCharacteristicID"].ToString()),
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            Quantity = quantity,
                            CollectedQuantity = collectedQuantity,
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString(),
                            CountProductSpools = Convert.ToInt16(row["CountProductSpools"]),
                            CountProductSpoolsWithBreak = Convert.ToInt16(row["CountProductSpoolsWithBreak"])
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
                        DocIsConfirmed = Convert.ToBoolean(table.Rows[0]["IsConfirmed"])
                    };
                    if (!table.Rows[0].IsNull("NomenclatureID"))
                    {
                        result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
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
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            CountProductSpools = Convert.ToInt16(table.Rows[0]["CountProductSpools"]),
                            CountProductSpoolsWithBreak = Convert.ToInt16(table.Rows[0]["CountProductSpoolsWithBreak"])                        };
                    }
                }
            }
            return result;
        }
            


        public static DbOperationProductResult AddProductToOrder(Guid docOrderId, OrderType orderType, Guid personId
            , string barcode, DocDirection docDirection)
        {
            DbOperationProductResult result = null;
            const string sql = "dbo.[mob_AddProductToOrder]";
            var parameters = new List<SqlParameter>
                {
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
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString(),
                            CountProductSpools = Convert.ToInt16(table.Rows[0]["CountProductSpools"]),
                            CountProductSpoolsWithBreak = Convert.ToInt16(table.Rows[0]["CountProductSpoolsWithBreak"])
                        };
                    }
                }
            }            
            return result;
        }

        public static MoveProductResult MoveProduct(Guid personId, string barcode, EndPointInfo endPointInfo)
        {
            MoveProductResult acceptProductResult = null;
            const string sql = "dbo.[mob_AddProductToMovement]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                        {
                            Value = personId
                        },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar)
                        {
                            Value = barcode
                        },
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = endPointInfo.PlaceId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = endPointInfo.PlaceZoneId
                        }
                };
            using (DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure))
            {
                if (table != null && table.Rows.Count > 0)
                {
                    acceptProductResult = new MoveProductResult
                    {
                        NomenclatureName =
                            table.Rows[0].IsNull("NomenclatureName") ? "" : table.Rows[0]["NomenclatureName"].ToString(),
                        Number = table.Rows[0].IsNull("Number") ? "" : table.Rows[0]["Number"].ToString(),
                        Quantity = table.Rows[0].IsNull("Quantity") ? 0 : Convert.ToDecimal(table.Rows[0]["Quantity"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyAdded =
                            !table.Rows[0].IsNull("AlreadyAdded") &&
                            Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]),
                        OutPlace = table.Rows[0].IsNull("OutPlace") ? "" : table.Rows[0]["OutPlace"].ToString(),
                        DocMovementId = !table.Rows[0].IsNull("DocMovementID") ? new Guid(table.Rows[0]["DocMovementID"].ToString()) : new Guid(),
                        Date = table.Rows[0].IsNull("Date") ? DateTime.Now : Convert.ToDateTime(table.Rows[0]["Date"].ToString())
                    };
                }
            }
            return acceptProductResult;
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
                    countProductSpools = Convert.ToInt32(table.Rows[0]["CountProductSpools"]);
                }
            }
            return countProductSpools;
        }

        public static string GetProgramSettings(string NameSetting)
        {
            var valueSetting = "";
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
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DataTable table;
                using (var command = new SqlCommand(sql))
                {
                    command.Connection = new SqlConnection(ConnectionString);
                    command.CommandType = commandType;
                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    using (var sda = new SqlDataAdapter(command))
                    {
                        try
                        {
                            table = new DataTable();
                            sda.Fill(table);
                            Shared.LastQueryCompleted = true;
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
                            Shared.LastQueryCompleted = false;
                            table = null;
                        }
                    }
                }
                return table;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
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
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            CollectedQuantity = Convert.ToDecimal(row["Quantity"]),
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

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
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"]),
                            NomenclatureName = table.Rows[0]["NomenclatureName"].ToString(),
                            ShortNomenclatureName = table.Rows[0]["ShortNomenclatureName"].ToString()
                        };
                    }
                }
            }
            return result;
        }

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

        internal static DataTable DocInventarisationNomenclatureProducts(Guid docInventarisationId, Guid nomenclatureId, Guid characteristicId)
        {
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
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return table;
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
            return Shared.LastQueryCompleted;
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
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    infoproduct = row["Name"].ToString();
                }
            }
            return infoproduct;
        }

    }
}