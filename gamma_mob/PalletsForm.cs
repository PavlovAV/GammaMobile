using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob
{
    public partial class PalletsForm : BaseFormWithToolBar
    {
        private PalletsForm()
        {
            Pallets = new BindingList<PalletListItem>();
            BSource = new BindingSource {DataSource = Pallets};
            InitializeComponent();
        }

        public PalletsForm(Form parentForm) : this()
        {
            ParentForm = parentForm;
        }

        private BindingSource BSource { get; set; }

        public PalletsForm(Form parentForm, Guid docOrderId, DocDirection docDirection) : this(parentForm)
        {
            DocOrderId = docOrderId;
            DocDirection = docDirection;
        }

        private void GetOrderPallets(Guid docOrderId)
        {
            UIServices.SetBusyState(this);
            Pallets = Db.GetOrderPallets(docOrderId);
            if (Pallets == null)
            {
                Shared.ShowMessageError(@"Не удалось получить информацию о скомплектованных паллетах");
                Close();
                return;
            }
            BSource.DataSource = Pallets;
            gridPallets.DataSource = BSource;
            UIServices.SetNormalState(this);
        }

        public void AddPalletToPallets(PalletListItem pallet)
        {
            if (Pallets != null)
                Pallets.Add(pallet);
        }

        private BindingList<PalletListItem> Pallets { get; set; }

        private Guid DocOrderId { get; set; }

        private DocDirection DocDirection { get; set; }

/*        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
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
*/
        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivateToolBar(new List<int>() { (int)Images.Back, (int)Images.Refresh, (int)Images.DocPlus, (int)Images.Edit, (int)Images.Remove, (int)Images.InfoProduct, (int)Images.RDP });

            /*            // установка иконок на кнопки
                        tbrMain.ImageList = ImgList;
                        btnBack.ImageIndex = (int)Images.Back;
                        btnRefresh.ImageIndex = (int)Images.Refresh;
                        btnAdd.ImageIndex = (int) Images.Add;
                        btnEdit.ImageIndex = (int) Images.Edit;
                        btnDelete.ImageIndex = (int) Images.Remove;*/
        }

        private void PalletsForm_Load(object sender, EventArgs e)
        {
            GetOrderPallets(DocOrderId);
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
                Width = 60,
                Format = "dd.MM.yy"
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Номер",
                MappingName = "Number",
                Width = 100
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Создал",
                MappingName = "Person",
                Width = 60
            });
            gridPallets.TableStyles.Add(tableStyle);
        }

        protected override void RemoveToolBarButton()
        {
            var pallet = Pallets[gridPallets.CurrentRowIndex];
            if (pallet == null) return;
            if (Shared.ShowMessageQuestion(@"Вы действительно хотите удалить паллету № " + pallet.Number) != DialogResult.Yes) return;
            UIServices.SetBusyState(this);
                var result = Db.DeletePallet(pallet.ProductId);
            UIServices.SetNormalState(this);
            if (String.IsNullOrEmpty(result))
            {
                Pallets.Remove(pallet);
                return;
            }
            Shared.ShowMessageError(result);
        }

        private void gridPallets_DoubleClick(object sender, EventArgs e)
        {
            EditToolBarButton();
        }

        protected override void NewToolBarButton()
        {
            var productId = Guid.NewGuid();
            /*var result = Db.CreateNewPallet(productId, DocOrderId);
            if (!String.IsNullOrEmpty(result))
            {
                Shared.ShowMessageError(result);
                return;
            }*/
            var pallet = new Pallet
                {
                    DocOrderId = DocOrderId,
                    ProductId = productId,
                    DocDirection = DocDirection,
                    Number = String.Empty,
                    IsConfirmed = false,
                    Items = new List<DocNomenclatureItem>()
                };
            OpenPallet(pallet);
        }

        protected override void EditToolBarButton()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            if (gridPallets == null || gridPallets.CurrentRowIndex < 0)
            {
                Shared.ShowMessageError(@"Список паллет пуст. Создайте паллету.");
                return;
            }
            var listItem = Pallets[gridPallets.CurrentRowIndex];
            if (listItem == null) return;
            var pallet = new Pallet
                {
                    ProductId = listItem.ProductId,
                    DocOrderId = DocOrderId,
                    DocDirection = DocDirection,
                    Number = listItem.Number
                };
            pallet.Items = Db.GetPalletItems(pallet.ProductId);
            OpenPallet(pallet);
        }

        private void OpenPallet(Pallet pallet)
        {
            UIServices.SetBusyState(this);
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
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

        protected override void RefreshToolBarButton()
        {
            GetOrderPallets(DocOrderId);
        }
        
    }
}