using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocInventarisationNomenclatureProductsForm : BaseForm
    {
        private DocInventarisationNomenclatureProductsForm()
        {
            InitializeComponent();
            var tableStyle = new DataGridTableStyle();
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "Number",
                HeaderText = "Номер",
                Width = 150
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "Quantity",
                HeaderText = "Кол-во",
                Width = 80,
                Format = "0.###"
            });
            gridProducts.TableStyles.Add(tableStyle);
        }

        public DocInventarisationNomenclatureProductsForm(Guid docInventarisationId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Form parentForm) : this()
        {
            lblNomenclature.Text = nomenclatureName;
            ParentForm = parentForm;
            DataTable table = Db.DocInventarisationNomenclatureProducts(docInventarisationId, nomenclatureId, characteristicId);
            if (!Shared.LastQueryCompleted)
            {
                MessageBox.Show(@"Не удалось получить информацию о продукции");
                Close();
                return;
            }
            if (table != null)
                gridProducts.DataSource = table;
        }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
        }
    }
}