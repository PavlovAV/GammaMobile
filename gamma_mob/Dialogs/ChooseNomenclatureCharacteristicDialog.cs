using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using gamma_mob.Models;
using gamma_mob.Common;

namespace gamma_mob.Dialogs
{
    public partial class ChooseNomenclatureCharacteristicDialog : Form
    {
        public ChooseNomenclatureCharacteristicDialog()
        {
            InitializeComponent();
        }

        public ChooseNomenclatureCharacteristicDialog(string barcode)
            : this()
        {
            Barcode = barcode;

            
            BindingList<ChooseNomenclatureItem> list = Db.GetNomenclatureCharacteristicQualityFromBarcode(barcode);
            if (!Shared.LastQueryCompleted || list == null)
            {
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                //list = barcodes1C;
                if (list == null) list = new BindingList<ChooseNomenclatureItem>();
                foreach (ChooseNomenclatureItem barcode1c in Shared.Barcodes1C)
                {
                    if (barcode1c.Barcode == barcode)
                        list.Add(barcode1c);
                }
                //return;
            }
            ChooseNomenclatureList = list;
            if (BSource == null)
                BSource = new BindingSource { DataSource = ChooseNomenclatureList };
            else
            {
                BSource.DataSource = ChooseNomenclatureList;
            }
            gridChoose.DataSource = BSource;

            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            var columnStyle = new DataGridTextBoxColumn();  
            columnStyle.HeaderText = "Наименование";
            columnStyle.MappingName = "Name";
            columnStyle.Width = 200;
            tableStyle.GridColumnStyles.Add(columnStyle);
            gridChoose.TableStyles.Add(tableStyle);
            //columnStyle.TextBox.Multiline=true;
            //columnStyle.TextBox.WordWrap = true;
            
            for (int i = 0; i < gridChoose.VisibleRowCount; i++)
            {
                SetGridRowHeight(gridChoose, i, (int)gridChoose.Font.Size*3*3);
            }
        }

        public void SetGridRowHeight(DataGrid dg, int nRow, int cy)
        {
            ArrayList arrRows = ((ArrayList)(dg.GetType().GetField("m_rlrow", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(dg)));
            object row = arrRows[nRow];
            row.GetType().GetField("m_cy", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).SetValue(row, cy);
            dg.Invalidate();
        }

        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        private string Barcode { get; set; }

        private BindingSource BSource { get; set; }
        private BindingList<ChooseNomenclatureItem> ChooseNomenclatureList { get; set; }

        private bool setProductId()
        {
            var good = ChooseNomenclatureList[gridChoose.CurrentRowIndex];
            if (good == null)
            {
                return false; 
            }
            NomenclatureId = good.NomenclatureId;
            CharacteristicId = good.CharacteristicId;
            QualityId = good.QualityId;
            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (setProductId())
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Close();
        }
                
        private void gridChoose_DoubleClick(object sender, EventArgs e)
        {
            if (setProductId())
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}