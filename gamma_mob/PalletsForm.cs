using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob
{
    public partial class PalletsForm : BaseForm
    {
        private PalletsForm()
        {
            Pallets = new BindingList<PalletListItem>();
            BSource = new BindingSource {DataSource = Pallets};
            InitializeComponent();
        }

        private PalletsForm(Form parentForm) : this()
        {
            ParentForm = parentForm;
        }

        private BindingSource BSource { get; set; }

        public PalletsForm(Form parentForm, Guid docOrderId) : this(parentForm)
        {
            DocOrderId = docOrderId;
            GetOrderPallets(docOrderId);
        }

        private void GetOrderPallets(Guid docOrderId)
        {
            UIServices.SetBusyState(this);
            Pallets = Db.GetOrderPallets(docOrderId);
            if (Pallets == null)
            {
                MessageBox.Show(@"Не удалось получить информацию о скомплектованных паллетах");
                Close();
                return;
            }
            BSource.DataSource = Pallets;
            gridPallets.DataSource = BSource;
            UIServices.SetNormalState(this);
        }

        private BindingList<PalletListItem> Pallets { get; set; }

        private Guid DocOrderId { get; set; }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 1:
                    NewPallet();
                    break;
                case 2:
                    EditPallet();
                    break;
                case 3:
                    DeletePallet();
                    break;
                case 4:
                    GetOrderPallets(DocOrderId);
                    break;
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            // установка иконок на кнопки
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnAdd.ImageIndex = (int) Images.Add;
            btnEdit.ImageIndex = (int) Images.Edit;
            btnDelete.ImageIndex = (int) Images.Remove;
            // Оформление грида
            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "ProductId",
                Width = -1
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Дата",
                MappingName = "Date",
                Width = 80
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Номер",
                MappingName = "Number",
                Width = 140
            });
            gridPallets.TableStyles.Add(tableStyle);
        }

        private void DeletePallet()
        {
            var pallet = Pallets[gridPallets.CurrentRowIndex];
            if (pallet == null) return;
            if (MessageBox.Show(@"Вы действительно хотите удалить паллету № " + pallet.Number, @"Удаление паллеты", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            UIServices.SetBusyState(this);
                var result = Db.DeletePallet(pallet.ProductId);
            UIServices.SetNormalState(this);
            if (String.IsNullOrEmpty(result))
            {
                Pallets.Remove(pallet);
                return;
            }
            MessageBox.Show(result);
        }

        private void gridPallets_DoubleClick(object sender, EventArgs e)
        {
            EditPallet();
        }

        private void NewPallet()
        {
            var productId = Guid.NewGuid();
            var result = Db.CreateNewPallet(productId, DocOrderId);
            if (!String.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
                return;
            }
            var pallet = new Pallet
                {
                    DocOrderId = DocOrderId,
                    ProductId = productId,
                    IsConfirmed = false,
                    Items = new List<DocNomenclatureItem>()
                };
            OpenPallet(pallet);
        }

        private void EditPallet()
        {
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с сервером");
                return;
            }
            var listItem = Pallets[gridPallets.CurrentRowIndex];
            if (listItem == null) return;
            var pallet = new Pallet
                {
                    ProductId = listItem.ProductId,
                    DocOrderId = DocOrderId,
                };
            pallet.Items = Db.GetPalletItems(pallet.ProductId);
            OpenPallet(pallet);
        }

        private void OpenPallet(Pallet pallet)
        {
            UIServices.SetBusyState(this);
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с сервером");
                return;
            }
            var form = new PalletForm(this, pallet);
            if (!form.IsDisposed)
            {
                form.Show();
                if (form.Enabled)
                    Hide();
            }
            UIServices.SetNormalState(this);
        }
        
    }
}