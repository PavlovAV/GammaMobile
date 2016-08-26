﻿using System;
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
                        PersonID = Convert.ToInt32(row["PersonID"]),
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

        public static BindingList<DocShipmentOrder> PersonDocShipmentOrders(int personId)
        {
            BindingList<DocShipmentOrder> list = null;
            const string sql = "SELECT a.[1CDocShipmentOrderID] AS DocShipmentOrderID, b.[1CNumber] AS Number, c.Description AS Buyer FROM" +
                               " DocShipmentOrderInfo a" +
                               " JOIN [1CDocShipmentOrder] b ON a.[1CDocShipmentOrderID] = b.[1CDocShipmentOrderID]" +
                               " JOIN [1CContractors] c ON b.[1CConsigneeID] = c.[1CContractorID]" +                               
                               " WHERE a.[ActivePersonID] = @PersonID AND b.Posted = 0";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.Int)
                };
            parameters[0].Value = personId;
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                list = new BindingList<DocShipmentOrder>();
                foreach (DataRow row in table.Rows)
                {
                    list.Add(new DocShipmentOrder
                        {
                            DocShipmentOrderId = new Guid(row["DocShipmentOrderID"].ToString()),
                            Number = row["Number"].ToString(),
                            Buyer = row["Buyer"].ToString()
                        });
                }
            }
            return list ?? new BindingList<DocShipmentOrder>();
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

        public static BindingList<DocShipmentGood> DocShipmentGoods(Guid docId)
        {
            BindingList<DocShipmentGood> list = null;
            const string sql = "SELECT * FROM vDocShipmentOrders WHERE [1CDocShipmentOrderID] = @DocID";
            var parameters =
                new List<SqlParameter>
                    {
                        new SqlParameter("@DocID", SqlDbType.UniqueIdentifier)
                    };
            parameters[0].Value = docId;
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                list = new BindingList<DocShipmentGood>();
                foreach (DataRow row in table.Rows)
                {
                    list.Add(new DocShipmentGood
                        {
                            CharacteristicId = new Guid(row["1CCharacteristicID"].ToString()),
                            NomenclatureId = new Guid(row["1CNomenclatureID"].ToString()),
                            NomenclatureName = row["NomenclatureName"].ToString(),
                            Quantity = row["Quantity"].ToString(),
                            CollectedQuantity = Convert.ToDecimal(row["CollectedQuantity"]),
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

        public static bool CancelLastMovement(string barcode)
        {
            bool result = false;
            const string sql = "dbo.[mob_CancelLastMovement]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@barcode", SqlDbType.VarChar)
                        {
                            Value = barcode
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null)
            {
                result = Convert.ToBoolean(table.Rows[0]["Result"]);
            }
            return result;
        }


        /// <summary>
        ///     Удаление продукта из приказа
        /// </summary>
        /// <param name="persontId">Идентификатор оператора</param>
        /// <param name="barcode">ШК продукта</param>
        /// <param name="docShipmentOrderId">Идентификатор документа на отгрузку 1С</param>
        /// <returns>Описание результата действия</returns>
        public static DbOperationProductResult DeleteProductFromOrder(int persontId, string barcode,
                                                                      Guid docShipmentOrderId)
        {
            var deleteResult = new DbOperationProductResult
                {
                    ProductItems = new List<ProductItem>()
                };
            const string sql = "dbo.[mob_DelProductFromOrder]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.Int)
                        {
                            Value = persontId
                        },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar)
                        {
                            Value = barcode
                        },
                    new SqlParameter("@DocShipmentOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docShipmentOrderId
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                deleteResult.ResultMessage = table.Rows[0]["ResultMessage"].ToString();
                deleteResult.AlreadyMadeChanges = Convert.ToBoolean(table.Rows[0]["AlreadyRemoved"]);
                if (!table.Rows[0].IsNull("NomenclatureID"))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        deleteResult.ProductItems.Add(new ProductItem
                            {
                                NomenclatureId = new Guid(row["NomenclatureID"].ToString()),
                                CharacteristicId = new Guid(row["CharacteristicID"].ToString()),
                                Quantity = Convert.ToDecimal(row["Quantity"])
                            });
                    }
                }
            }
            return deleteResult;
        }

        public static AcceptProductResult AcceptProduct(int personId, string barcode, EndPointInfo endPointInfo)
        {
            AcceptProductResult acceptProductResult = null;
            const string sql = "dbo.[mob_AcceptProduct]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.Int)
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
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table != null && table.Rows.Count > 0)
            {
                acceptProductResult = new AcceptProductResult
                    {
                        NomenclatureName =
                            table.Rows[0].IsNull("NomenclatureName") ? "" : table.Rows[0]["NomenclatureName"].ToString(),
                        Number = table.Rows[0].IsNull("Number") ? "" : table.Rows[0]["Number"].ToString(),
                        Quantity = table.Rows[0].IsNull("Quantity") ? 0 : Convert.ToDecimal(table.Rows[0]["Quantity"]),
                        ResultMessage = table.Rows[0]["ResultMessage"].ToString(),
                        AlreadyAccepted =
                            !table.Rows[0].IsNull("AlreadyAccepted") &&
                            Convert.ToBoolean(table.Rows[0]["AlreadyAccepted"]),
                        SourcePlace = table.Rows[0].IsNull("SourcePlace") ? "" : table.Rows[0]["SourcePlace"].ToString()
                    };
            }
            return acceptProductResult;
        }

        /// <summary>
        ///     Добавление продукта в приказ
        /// </summary>
        /// <param name="personId">Идентификатор оператора(грузчика)</param>
        /// <param name="barcode">ШК продукта</param>
        /// <param name="docShipmentOrderId">Идентификатор приказа 1С</param>
        /// <returns>
        ///     Если успешно добавлено, то возвращает результат с последовтельностью элементов. В
        ///     противном случае последовательность null, в ResultMessage - сообщение
        /// </returns>
        public static DbOperationProductResult AddProduct(int personId, string barcode, Guid docShipmentOrderId)
        {
            var addProductResult = new DbOperationProductResult
                {
                    ProductItems = new List<ProductItem>()
                };
            const string sql = "dbo.[mob_AddProduct]";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.Int)
                        {
                            Value = personId
                        },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar)
                        {
                            Value = barcode
                        },
                    new SqlParameter("@DocShipmentOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docShipmentOrderId
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
/*            else
            {
                addProductResult.ResultMessage = "При добавлении произошла ошибка";
            }
 */
            return addProductResult;
        }

        /// <summary>
        ///     Возвращает ID документа Gamma
        /// </summary>
        /// <param name="docShipmentOrderId">ID документа 1С</param>
        /// <param name="personId">ID оператора</param>
        /// <returns></returns>
        public static Guid? GetDocId(Guid docShipmentOrderId, int personId)
        {
            const string sql = "dbo.mob_GetDocId";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocShipmentOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docShipmentOrderId
                        },
                    new SqlParameter("@PersonID", SqlDbType.Int)
                        {
                            Value = personId
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
            if (table == null || table.Rows.Count < 1) return null;
            if (table.Rows[0]["DocID"] == null) return null;
            return new Guid(table.Rows[0]["DocID"].ToString());
        }

        public static List<string> CurrentBarcodes(Guid docShipmentId, int personId)
        {
            var list = new List<string>();
            const string sql = "SELECT c.Barcode FROM DocShipments a" +
                               " JOIN DocProducts b ON a.DocID = b.DocID" +
                               " JOIN Products c ON b.ProductID = c.ProductID" +
                               " WHERE a.[1CDocShipmentOrderId] = @DocShipmentOrderID AND a.PersonID = @PersonID";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DocShipmentOrderID", SqlDbType.UniqueIdentifier)
                        {
                            Value = docShipmentId
                        },
                    new SqlParameter("@PersonID", SqlDbType.Int)
                        {
                            Value = personId
                        }
                };
            DataTable table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
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