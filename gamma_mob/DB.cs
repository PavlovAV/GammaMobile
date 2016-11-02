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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                DataRow row = table.Rows[0];
                person = new Person
                    {
                        PersonID = new Guid(row["PersonID"].ToString()),
                        Name = row["Name"].ToString()
                    };
            }
            return person;
        }

        public static DataTable DocShipmentOrderGoodProducts(Guid docShipmentOrderId, Guid nomenclatureId,
                                                             Guid characteristicId)
        {
            const string sql = "dbo.mob_GetGoodProducts";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocShipmentOrderID", SqlDbType.UniqueIdentifier)
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
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            return table;
        }

/*
        public static BindingList<DocMovementOrder> DocMovementOrders(Guid personId)
        {
            BindingList<DocMovementOrder> list = null;
            const string sql = "dbo.mob_GetDocMovementOrders";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.UniqueIdentifier)
                };
            parameters[0].Value = personId;
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                list = new BindingList<DocMovementOrder>();
                foreach (DataRow row in table.Rows)
                {
                    list.Add(new DocMovementOrder
                    {
                        DocId = new Guid(row["DocID"].ToString()),
                        Number = row["Number"].ToString(),
                        PlaceFrom = row["PlaceFrom"].ToString(),
                        PlaceTo = row["PlaceTo"].ToString()
                    });
                }
            }
            return list ?? new BindingList<DocMovementOrder>();
        }
*/

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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                list = new BindingList<DocOrder>();
                foreach (DataRow row in table.Rows)
                {
                    list.Add(new DocOrder
                        {
                            DocOrderId = new Guid(row["1COrderID"].ToString()),
                            Number = row["Number"].ToString(),
                            Consignee = row["Consignee"].ToString()
                        });
                }
            }
            return list ?? new BindingList<DocOrder>();
            //return table;
        }

/*
        public static Product ProductByBarcode(string barcode)
        {
            Product product = null;
            const string sql = "SELECT * FROM vProductsInfo WHERE Barcode = @Barcode";
            var parameters =
                new List<SqlParameter>
                    {
                        new SqlParameter("@Barcode", SqlDbType.VarChar)
                    };
            parameters[0].Value = barcode;
            var table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                
                product = new Product()
                    {
                        ProductId = new Guid(table.Rows[0]["ProductId"].ToString()),
                        ProductItems = new List<ProductItem>()
                    };
                foreach (DataRow row in table.Rows)
                {
                    product.ProductItems.Add(new ProductItem()
                        {
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            CharacteristicId = new Guid(row["1CCharacteristicID"].ToString()),
                            Quantity = Convert.ToDecimal(row["Quantity"])
                        });
                }
            }
            return product;
        }
*/
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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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
                            Quantity = Convert.ToDecimal(row["Quantity"])
                        });
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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
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
                            ShortNomenclatureName = row["ShortNomenclatureName"].ToString()
                        });
                }
            }
            return list;
        }

        public static List<Warehouse> GetWarehouses()
        {
            List<Warehouse> list = null;
            const string sql = "dbo.mob_GetWarehouses";
            DataTable table = ExecuteSelectQuery(sql, new List<SqlParameter>(), CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                list = new List<Warehouse>();
                list.AddRange(from DataRow row in table.Rows
                              select new Warehouse
                                  {
                                      WarehouseId = Convert.ToInt32(row["WarehouseID"]),
                                      WarehouseName = row["WarehouseName"].ToString(),
                                      WarehouseZones = new List<WarehouseZone>()
                                  });
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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                result = new DbOperationProductResult
                    {
                        AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString()
                    };
                if (!table.Rows[0].IsNull("NomenclatureID"))
                {
                    result.Product = new Product
                        {
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString()),
                            NomenclatureId = new Guid(table.Rows[0]["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(table.Rows[0]["CharacteristicID"].ToString()),
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"])
                        };
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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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
                            ProductId = new Guid(table.Rows[0]["ProductID"].ToString())
                        };
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
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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
                            Quantity = Convert.ToDecimal(table.Rows[0]["Quantity"])
                        };
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
                            Value = endPointInfo.WarehouseId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = endPointInfo.WarehouseZoneId
                        },
                    new SqlParameter("@PlaceZoneCellID", SqlDbType.UniqueIdentifier)
                        {
                            Value = endPointInfo.ZoneCellId
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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
                        DocMovementId = !table.Rows[0].IsNull("DocMovementID") ? new Guid(table.Rows[0]["DocMovementID"].ToString()) : new Guid()
                    };
            }
            return acceptProductResult;
        }
/*
        /// <summary>
        /// Добавление продукта в обе табличные части перемещения
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="barcode"></param>
        /// <param name="docOrderId">ID приказа основания, если перемещение на основании приказа</param>
        /// <param name="docType"></param>
        /// <param name="placeId"></param>
        /// <param name="placeZoneId"></param>
        /// <param name="placeZoneCellId"></param>
        /// <returns></returns>
        public static DbOperationProductResult AddOutInProduct(Guid personId, string barcode, Guid? docOrderId, DocType docType,
                    int? placeId, Guid? placeZoneId, Guid? placeZoneCellId)
        {
            var addProductResult = new DbOperationProductResult();
//            {
//                ProductItems = new List<ProductItem>()
//            };
            var sql = "dbo.[mob_AddOutInProduct]";
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
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@DocType", SqlDbType.Int)
                        {
                            Value = (int) docType
                        },
                    new SqlParameter("@PlaceID",SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId
                        },
                    new SqlParameter("@PlaceZoneCellID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneCellId
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                addProductResult.ResultMessage = table.Rows[0]["ResultMessage"].ToString();
                addProductResult.AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]);
                if (!table.Rows[0].IsNull("NomenclatureID"))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        addProductResult.ProductItems.Add(new ProductItem
                        {
                            NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(row["CharacteristicID"].ToString()),
                            Quantity = Convert.ToDecimal(row["Quantity"])
                        });
                    }
                }
            }
            return addProductResult;
        }


        /// <summary>
        ///     Добавление продукта в отгрузку
        /// </summary>
        /// <param name="personId">Идентификатор оператора(грузчика)</param>
        /// <param name="barcode">ШК продукта</param>
        /// <param name="docOrderId">Идентификатор приказа основания</param>
        /// <param name="docType">Тип документа, к которому добавляется продукт</param>
        /// <returns>
        ///     Если успешно добавлено, то возвращает результат с последовтельностью элементов. В
        ///     противном случае последовательность null, в ResultMessage - сообщение
        /// </returns>
        public static DbOperationProductResult AddOutProduct(Guid personId, string barcode, Guid? docOrderId, DocType docType)
        {
            var addProductResult = new DbOperationProductResult
                {
                    ProductItems = new List<ProductItem>()
                };
            var sql = "dbo.[mob_AddOutProduct]";
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
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@DocType", SqlDbType.Int)
                        {
                            Value = (int) docType
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                addProductResult.ResultMessage = table.Rows[0]["ResultMessage"].ToString();
                addProductResult.AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]);
                if (!table.Rows[0].IsNull("NomenclatureID"))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        addProductResult.ProductItems.Add(new ProductItem
                            {
                                NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                                CharacteristicId = new Guid(row["CharacteristicID"].ToString()),
                                Quantity = Convert.ToDecimal(row["Quantity"])
                            });
                    }
                }
            }
            return addProductResult;
        }

        

        public static DbOperationProductResult AddInProduct(Guid personId, string barcode, Guid? docOrderId, DocType docType,
                int placeId, Guid? placeZoneId, Guid? placeZoneCellId)
        {
            var addProductResult = new DbOperationProductResult
            {
                ProductItems = new List<ProductItem>()
            };
            var sql = "dbo.[mob_AddInProduct]";
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
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@DocType", SqlDbType.Int)
                        {
                            Value = (int) docType
                        },
                    new SqlParameter("@PlaceID", SqlDbType.Int)
                        {
                            Value = placeId
                        },
                    new SqlParameter("@PlaceZoneID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneId
                        },
                    new SqlParameter("@PlaceZoneCellID", SqlDbType.UniqueIdentifier)
                        {
                            Value = placeZoneCellId
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                addProductResult.ResultMessage = table.Rows[0]["ResultMessage"].ToString();
                addProductResult.AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyAdded"]);
                if (!table.Rows[0].IsNull("NomenclatureID"))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        addProductResult.ProductItems.Add(new ProductItem
                        {
                            NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                            CharacteristicId = new Guid(row["CharacteristicID"].ToString()),
                            Quantity = Convert.ToDecimal(row["Quantity"])
                        });
                    }
                }
            }
            return addProductResult;
        }
 */

/*
        /// <summary>
        ///     Возвращает ID документа Gamma
        /// </summary>
        /// <param name="docOrderId">ID документа основания</param>
        /// <param name="docType">Тип документа</param>
        /// <returns></returns>
        public static Guid? GetDocId(Guid? docOrderId, DocType docType)
        {
            const string sql = "dbo.mob_GetDocId";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docOrderId
                        },
                    new SqlParameter("@DocType", SqlDbType.Int)
                        {
                            Value = (int)docType
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table == null || table.Rows.Count < 1) return null;
            if (table.Rows[0].IsNull("DocID")) return null;
            return new Guid(table.Rows[0]["DocID"].ToString());
        }
*/

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
                    new SqlParameter("@IsOutDoc", SqlDbType.Bit)
                        {
                            Value = (int)docDirection > 1?0:docDirection
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                list.AddRange(from DataRow row in table.Rows select row["Barcode"].ToString());
            }
            return list;
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
                        catch (SqlException ex)
                        {
#if DEBUG
                            MessageBox.Show(ex.Message);
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
    }
}