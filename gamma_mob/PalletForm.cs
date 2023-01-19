using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using gamma_mob.WCF;


namespace gamma_mob
{
    public partial class PalletForm : BaseForm
    {
        private PalletForm()
        {   
            Items = new BindingList<DocNomenclatureItem>();
            BSource = new BindingSource {DataSource = Items} ;
            InitializeComponent();
        }

        private PalletForm(Form parentForm) : this()
        {
            ParentForm = parentForm;
        }

        public PalletForm(Form parentForm, Pallet pallet) : this(parentForm)
        {
            DocOrderId = pallet.DocOrderId;
            ProductId = pallet.ProductId;
            var list = pallet.Items;
            if (Shared.LastQueryCompleted == false || list == null)
            {
                Shared.ShowMessageError(@"Не удалось получить информацию о текущем документе");
                Close();
                return;
            }
            Items = new BindingList<DocNomenclatureItem>(list);
            BSource.DataSource = Items;
            gridPalletItems.DataSource = BSource;
        }
        
        private Guid DocOrderId { get; set; }

        private BindingSource BSource { get; set; }
        private BindingList<DocNomenclatureItem> Items { get; set; }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 1:
                    DeleteItem();
                    break;
                case 2:
                    Print();
                    break;
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnPrint.ImageIndex = (int) Images.Print;
            btnDelete.ImageIndex = (int) Images.Remove;
            BarcodeFunc = BarcodeReaction;
            // Оформление грида
            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Номенклатура",
                MappingName = "ShortNomenclatureName",
                Width = 156
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Кол-во",
                MappingName = "CollectedQuantity",
                Width = 38
            });
            gridPalletItems.TableStyles.Add(tableStyle);
        }

        private Guid ProductId { get; set; }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
            UIServices.SetBusyState(this);
            AddProductByBarcode(barcode);
            UIServices.SetNormalState(this);
        }

        private void Print()
        {
            UIServices.SetBusyState(this);
            try
            {
                var client = new PrinterServiceClient();
                client.PrintPallet(ProductId.ToString());
            }
            catch (Exception)
            {
                Shared.ShowMessageError(@"Служба печати не доступна");
            }
            UIServices.SetNormalState(this);
        }

        private void DeleteItem()
        {
            if (!Items.Any() || gridPalletItems.CurrentRowIndex < 0) return;
            var item = Items[gridPalletItems.CurrentRowIndex];
            UIServices.SetBusyState(this);
            if (!Db.DeleteItemFromPallet(ProductId, item.NomenclatureId, item.CharacteristicId))
            {
                Shared.ShowMessageError(@"Не удалось связаться с базой");
            }
            else
            {
                Items.Remove(item);
            }
            UIServices.SetNormalState(this);
        }

        private void AddProductByBarcode(string barcode)
        {
            UIServices.SetBusyState(this);
            int quantity;
            using (var form = new SetPacksNumberDialog())
            {
                var dlgResult = form.ShowDialog();
                if (dlgResult != DialogResult.OK)
                {
                    Shared.ShowMessageInformation(@"Не было указано количество пачек");
                    UIServices.SetNormalState(this);
                    return;
                }
                quantity = form.Quantity;
            }
            var result = Db.AddItemToPallet(ProductId, DocOrderId, barcode, quantity);
            if (Shared.LastQueryCompleted == false)
            {
                Shared.ShowMessageError(@"Нет связи с базой");
            }      
            else if (result != null && !String.IsNullOrEmpty(result.ResultMessage))
            {
                    Shared.ShowMessageError(result.ResultMessage);
            }
            else
            {
                if (result != null)
                Invoke((UpdateInventarisationGridInvoker)(UpdateGrid),
                           new object[] { result.NomenclatureId, result.CharacteristicId, result.QualityId, result.NomenclatureName, 
                                result.ShortNomenclatureName, result.Quantity, null});
            }
            UIServices.SetNormalState(this);
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, int? productKindId)
        {
            DocNomenclatureItem item =
                Items.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId);
            if (item == null)
            {
                item = new DocNomenclatureItem
                {
                    NomenclatureId = nomenclatureId,
                    CharacteristicId = characteristicId,
                    QualityId = qualityId,
                    NomenclatureName = nomenclatureName,
                    ShortNomenclatureName = shortNomenclatureName,
                    CollectedQuantity = 0,
                };
                Items.Add(item);
                BSource.DataSource = Items;
            }
            item.CollectedQuantity += quantity;
            gridPalletItems.UnselectAll();
            int index = Items.IndexOf(item);
            gridPalletItems.CurrentRowIndex = index;
            gridPalletItems.Select(index);
        }
        
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            AddProductByBarcode(edtNumber.Text);
        }

        
        
    }
}