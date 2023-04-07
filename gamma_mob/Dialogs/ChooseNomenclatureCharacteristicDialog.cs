using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using gamma_mob.Models;
using gamma_mob.Common;
using OpenNETCF.Windows.Forms;

namespace gamma_mob.Dialogs
{
    public partial class ChooseNomenclatureCharacteristicDialog : BaseForm
    {
        public ChooseNomenclatureCharacteristicDialog()
        {
            InitializeComponent();
            if (!Shared.IsScanGroupPackOnlyFromProduct && Shared.IsAvailabilityChoiseNomenclatureForMovingGroupPack)
            {
                gridChoose.Visible = true;
                label4.Text = "Выберите номенклатуру или отсканируйте паллету, из которой упаковка/коробка, или зону стеллажа"; ;
            }
            else
            {
                gridChoose.Visible = false;
                label4.Text = "Отсканируйте паллету, из которой упаковка/коробка, или зону стеллажа"; ;
                btnOK.Text = "Отмена";
            }
        }

        public ChooseNomenclatureCharacteristicDialog(string barcode)
            : this()
        {
            Barcode = barcode;
            Shared.SaveToLogInformation("Open ChooseNomenclatureCharacteristicDialog ('" + barcode + "')");
        }

        public ChooseNomenclatureCharacteristicDialog(string barcode, Form parentForm)
            : this(barcode)
        {
            ParentForm = parentForm;
            Shared.SaveToLogInformation("Open ChooseNomenclatureCharacteristicDialog (barcode, '" + parentForm.Name + "')");
        }

        public void SetGridRowHeight(DataGrid dg, int nRow, int cy)
        {
            ArrayList arrRows = ((ArrayList)(dg.GetType().GetField("m_rlrow", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(dg)));
            object row = arrRows[nRow];
            row.GetType().GetField("m_cy", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).SetValue(row, cy);
            dg.Invalidate();
        }

        public Guid? FromProductId { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public int CountProducts { get; set; }
        private string Barcode { get; set; }

        private BindingSource BSource { get; set; }
        private List<ChooseNomenclatureItem> ChooseNomenclatureList { get; set; }


        private void GetNomenclatureList()
        {
            List<ChooseNomenclatureItem> list = Shared.Barcodes1C.GetNomenclaturesFromBarcodeInBarcodes(Barcode);
            ChooseNomenclatureList = list;
            if (BSource == null)
                BSource = new BindingSource { DataSource = ChooseNomenclatureList };
            else
            {
                BSource.DataSource = ChooseNomenclatureList;
            }
            gridChoose.DataSource = BSource;
        }

        private bool CreateNomenclatureList()
        {
            GetNomenclatureList();
            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            var columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Наименование";
            columnStyle.MappingName = "Name";
            columnStyle.Width = 200;
            tableStyle.GridColumnStyles.Add(columnStyle);
            gridChoose.TableStyles.Add(tableStyle);
            //columnStyle.TextBox.Multiline=true;
            //columnStyle.TextBox.WordWrap = true;

            for (int i = 0; i < gridChoose.BindingContext[gridChoose.DataSource].Count; i++)
            {
                SetGridRowHeight(gridChoose, i, (int)gridChoose.Font.Size * 3 * 3);
            }
            return true;
        }

        private bool setNomenclaturId()
        {
            var good = ChooseNomenclatureList[gridChoose.CurrentRowIndex];
            if (good == null || gridChoose.Visible == false)
            {
                return false; 
            }
            NomenclatureId = good.NomenclatureId;
            CharacteristicId = good.CharacteristicId;
            QualityId = good.QualityId;
            return GetCountProducts();
        }
        
        private bool GetCountProducts()
        {
            using (var form = new SetCountProductsDialog())
            {
                DialogResult result = form.ShowDialog();
                Invoke((MethodInvoker)Activate);
                if (result != DialogResult.OK || form.Quantity == 0)
                {
                    Shared.ShowMessageInformation(@"Не указано количество продукта. Продукт не добавлен!");
                    return false;
                }
                else
                {
                    CountProducts = form.Quantity;
                    Shared.SaveToLogInformation(@"Установлено кол-во " + form.Quantity);
                }
            }
            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (setNomenclaturId())
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Close();
        }

        private void gridChoose_DoubleClick(object sender, EventArgs e)
        {
            if (setNomenclaturId())
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = ChooseNomenclatureCharacteristicFromBarcode;
            if (!CreateNomenclatureList())
            {
                DialogResult = DialogResult.Abort;
                Close();
                return;
            }
        }

        protected override void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ReturnedResult) 
                ReturnResult();
            base.OnFormClosing(sender, e);
        }

        public bool ReturnedResult = true;
        
        public void ReturnResult()
        {
            if (ParentForm != null && (ParentForm is BaseFormWithChooseNomenclatureCharacteristic))
            {
                Invoke((MethodInvoker)delegate()
                    {
                        (ParentForm as BaseFormWithChooseNomenclatureCharacteristic).ClosingChooseNomenclatureCharacteristicDialog();
                    });
            }
        }

        public void SetBarcodeReaction(BarcodeReceivedEventHandler chooseNomenclatureCharacteristicFromBarcode)
        {
            BarcodeFunc = chooseNomenclatureCharacteristicFromBarcode;
        }

        public void SetBarcodeText(string barcode)
        {
            edtBarcode.Text = barcode;
        }

        private void ChooseNomenclatureCharacteristicFromBarcode(string barcode)
        {
            if (barcode.Length > 0)
            {
                DbProductIdFromBarcodeResult getFromProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                if (getFromProductResult != null && getFromProductResult.ProductId != null && getFromProductResult.ProductId != Guid.Empty)
                {
                    FromProductId = getFromProductResult.ProductId;
                    NomenclatureId = getFromProductResult.NomenclatureId;
                    CharacteristicId = getFromProductResult.CharacteristicId;
                    QualityId = getFromProductResult.QualityId;
                    if (GetCountProducts())
                        Invoke((MethodInvoker)(() => DialogResult = DialogResult.OK));
                }
                else
                {
                    Shared.ShowMessageError(@"Ошибка! Штрих-код продукта не распознан!" + Environment.NewLine + @"Попробуйте еще раз или выберите номенклатуру (если возможно)");
                }
            }
            else
            {
                Shared.ShowMessageError(@"Ошибка! Штрих-код пустой!" + Environment.NewLine + @"Попробуйте еще раз или выберите номенклатуру (если возможно)");
            }
        }

        public bool CheckNomenclatureInNomenclatureList(Guid productId)
        {
            Guid? res = Db.GetProductNomenclature(productId);
            if ((res ?? Guid.Empty) == Guid.Empty)
            {
                Shared.SaveToLogError(@"Db.GetProductNomenclature is null", null, productId);
                return false;
            }
            else
            {
                if (ChooseNomenclatureList == null || ChooseNomenclatureList.Count == 0)
                {
                    Invoke((MethodInvoker)delegate()
                    {
                        GetNomenclatureList();
                    });
                }
                Shared.SaveToLogInformation(@"Db.GetProductNomenclature return " + res.ToString() + @"(ChooseNomenclatureList.Count = " + ChooseNomenclatureList.Count);

                var resFind = ChooseNomenclatureList.Find(n => n.NomenclatureId == (Guid)res);
                return resFind != null;
            }
        }

        public void btnAddBarcode_Click(object sender, EventArgs e)
        {
            Shared.SaveToLogInformation(@"Выбрано Добавить ШК: " + edtBarcode.Text);
            //ChooseNomenclatureCharacteristicFromBarcode(edtBarcode.Text);
            if (BarcodeFunc != null)
                BarcodeFunc.Invoke(edtBarcode.Text);
            else
                ChooseNomenclatureCharacteristicFromBarcode(edtBarcode.Text);
        }
    }
}