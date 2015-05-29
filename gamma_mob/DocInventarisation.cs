using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Datalogic.API;

namespace gamma_mob
{
    public partial class DocInventarisation : Form
    {
        private DecodeEvent dcdEvent;
        private DecodeHandle hDcd;
        private DataTable sTable;

        public DocInventarisation()
        {
            InitializeComponent();
        }

        public void dcdEvent_OnScanned()
        {

        }

        private void DocInventarisation_Load(object sender, EventArgs e)
        {
            try
			{
				hDcd = new DecodeHandle(DecodeDeviceCap.Exists | DecodeDeviceCap.Barcode);
			}
			catch(DecodeException)
			{
				MessageBox.Show("Exception loading barcode decoder.", "Decoder Error");
				return;
			}

			// Now that we've got a connection to a barcode reading device, assign a
			// method for the DcdEvent.  A recurring request is used so that we will
			// continue to get barcode data until our dialog is closed.
			DecodeRequest reqType = (DecodeRequest)1 | DecodeRequest.PostRecurring;

			// Initialize event
			dcdEvent = new DecodeEvent(hDcd, reqType, this);
			dcdEvent.Scanned += new DecodeScanned(dcdEvent_Scanned);
            dcdEvent.TimeOut += new DecodeTimeOut(dcdEvent_TimeOut);
		
            sTable = new DataTable();
            sTable.Columns.Add("Number",typeof (string));
            
            dataGrid1.DataSource = sTable;
        }

        private void dcdEvent_TimeOut(object sender, DecodeEventArgs e)
        {
            MessageBox.Show("Timed out before string read!");
        }

        /// <summary>
        /// This method will be called when the DcdEvent is invoked.
        /// </summary>
        private void dcdEvent_Scanned(object sender, DecodeEventArgs e)
        {
            CodeId cID = CodeId.NoData;
            string dcdData = string.Empty;

            // Obtain the string and code id.
            try
            {
                dcdData = hDcd.ReadString(e.RequestID, ref cID);
            }
            catch (Exception)
            {
                MessageBox.Show("Error reading string!");
                return;
            }

            DataRow row = sTable.NewRow();
            row["number"] = dcdData;
            sTable.Rows.Add(row);
            SizeColumnsToContent(dataGrid1, 1);
//            this.sTable.Rows.A = dcdData;
//            this.txtCodeID.Text = cID.ToString();
        }

        public void SizeColumnsToContent(DataGrid dataGrid, int nRowsToScan)
        {
            // Create graphics object for measuring widths.
            Graphics Graphics = dataGrid.CreateGraphics();

            // Define new table style.
            DataGridTableStyle tableStyle = new DataGridTableStyle();

            try
            {
                DataTable dataTable = (DataTable)dataGrid.DataSource;

                if (-1 == nRowsToScan)
                {
                    nRowsToScan = dataTable.Rows.Count;
                }
                else
                {
                    // Can only scan rows if they exist.
                    nRowsToScan = System.Math.Min(nRowsToScan, dataTable.Rows.Count);
                }

                // Clear any existing table styles.
                dataGrid.TableStyles.Clear();

                // Use mapping name that is defined in the data source.
                tableStyle.MappingName = dataTable.TableName;

                // Now create the column styles within the table style.
                DataGridTextBoxColumn columnStyle;
                int iWidth;

                for (int iCurrCol = 0; iCurrCol < dataTable.Columns.Count; iCurrCol++)
                {
                    DataColumn dataColumn = dataTable.Columns[iCurrCol];

                    columnStyle = new DataGridTextBoxColumn();

                   // columnStyle.TextBox.Enabled = true;
                    columnStyle.HeaderText = dataColumn.ColumnName;
                    columnStyle.MappingName = dataColumn.ColumnName;

                    // Set width to header text width.
                    iWidth = (int)(Graphics.MeasureString(columnStyle.HeaderText, dataGrid.Font).Width);

                    // Change width, if data width is wider than header text width.
                    // Check the width of the data in the first X rows.
                    DataRow dataRow;
                    for (int iRow = 0; iRow < nRowsToScan; iRow++)
                    {
                        dataRow = dataTable.Rows[iRow];

                        if (null != dataRow[dataColumn.ColumnName])
                        {
                            int iColWidth = (int)(Graphics.MeasureString(dataRow.ItemArray[iCurrCol].ToString(), dataGrid.Font).Width);
                            iWidth = (int)System.Math.Max(iWidth, iColWidth);
                        }
                    }
                    columnStyle.Width = iWidth + 4;

                    // Add the new column style to the table style.
                    tableStyle.GridColumnStyles.Add(columnStyle);
                }
                // Add the new table style to the data grid.
                dataGrid.TableStyles.Add(tableStyle);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                Graphics.Dispose();
            }
        }

        
        
    }
}