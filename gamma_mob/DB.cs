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
        public static void SetConnectionString(string ipAddress, string database, string user, string password, string timeout)
        {
            ConnectionString = "Data Source=" + ipAddress + ";Initial Catalog=" + database + "" +
                               ";Persist Security Info=True;User ID=" + user + "" +
                               ";Password=" + password + ";Connect Timeout=" + timeout;

        }

        public static int CheckSqlConnection()
        {
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

        private static string ConnectionString {get; set;}

        public static Person PersonByBarcode(string barcode)
        {
            Person person = null;
            const string sql = "SELECT PersonID, Name FROM Persons WHERE Barcode = @Barcode";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BarCode", SqlDbType.VarChar)
                };
            parameters[0].Value = barcode;
            var table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                var row = table.Rows[0];
                person = new Person
                    {
                        PersonID = Convert.ToInt32(row["PersonID"]),
                        Name = row["Name"].ToString()
                    };
            }
            return person;
        }

        public static BindingList<DocShipmentOrder> PersonDocShipmentOrders(int personId)
        {
            BindingList<DocShipmentOrder> list = null;
            const string sql = "SELECT a.[1CDocShipmentOrderID] AS DocShipmentOrderID, b.[1CNumber] AS Number FROM" +
                               " ActiveOrders a" +
                               " JOIN [1CDocShipmentOrder] b ON a.[1CDocShipmentOrderID] = b.[1CDocShipmentOrderID]" +
                               " WHERE a.[PersonID] = @PersonID AND b.Posted = 0";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonID", SqlDbType.Int)
                };
            parameters[0].Value = personId;
            var table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                list = new BindingList<DocShipmentOrder>();
                foreach (DataRow row in table.Rows)
                {
                    list.Add(new DocShipmentOrder
                        {
                            DocShipmentOrderId = new Guid(row["DocShipmentOrderID"].ToString()),
                            Number = row["Number"].ToString()
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
            var table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
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
                            CollectedQuantity = Convert.ToDecimal(row["CollectedQuantity"])
                        });
                }
            }
            return list;
        }

        /// <summary>
        /// Удаление продукта из приказа
        /// </summary>
        /// <param name="persontId">Идентификатор оператора</param>
        /// <param name="barcode">ШК продукта</param>
        /// <param name="docShipmentOrderId">Идентификатор документа на отгрузку 1С</param>
        /// <returns>Описание результата действия</returns>
        public static DbOperationProductResult DeleteProductFromOrder(int persontId, string barcode, Guid docShipmentOrderId)
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
            var table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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


        /// <summary>
        /// Добавление продукта в приказ
        /// </summary>
        /// <param name="personId">Идентификатор оператора(грузчика)</param>
        /// <param name="barcode">ШК продукта</param>
        /// <param name="docShipmentOrderId">Идентификатор приказа 1С</param>
        /// <returns>Если успешно добавлено, то возвращает результат с последовтельностью элементов. В
        /// противном случае последовательность null, в ResultMessage - сообщение</returns>
        public static DbOperationProductResult AddProduct(int personId, string barcode, Guid docShipmentOrderId)
        {
            var addProductResult = new DbOperationProductResult()
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
            var table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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
        /// Возвращает ID документа Gamma
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
            var table = ExecuteSelectQuery(sql, parameters, CommandType.StoredProcedure);
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
            var table = ExecuteSelectQuery(sql, parameters, CommandType.Text);
            if (table != null && table.Rows.Count > 0)
            {
                list.AddRange(from DataRow row in table.Rows select row["Barcode"].ToString());
            }
            return list;
        }

        private static DataTable ExecuteSelectQuery(string sql, IEnumerable<SqlParameter> parameters, CommandType commandType)
        {
            DataTable table;
            using (var command = new SqlCommand(sql))
            {
                command.Connection = new SqlConnection(ConnectionString);
                command.CommandType = commandType;
                foreach (var parameter in parameters)
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
                        MessageBox.Show(ex.ToString());
#endif
                        Shared.LastQueryCompleted = false;
                        table = null;
                    }
                }
                
            }
            return table;
        }
    }

    

    
}
