using System;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using gamma_mob.Models;

namespace gamma_mob
{
    public partial class DocInventarisationListForm : BaseForm
    {
        private DocInventarisationListForm()
        {
            InitializeComponent();
        }

        public DocInventarisationListForm(Form parentForm, DocDirection docDirection) : this()
        {
            ParentForm = parentForm;
            DocDirection = docDirection;
            if (Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone != null && (DateTime.Now - Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone).TotalHours > 1)
                MessageBox.Show("Локальная база штрих-кодов устарела! " + Environment.NewLine
                    + "Последнее обновление - " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine
                    + "Перезапустите программу для загрузки штрих-кодов!");
        }

        private DocDirection DocDirection { get; set; }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 1:
                    EditDocInventarisation();
                    break;
                case 2:
                    GetDocInventarisations();
                    break;
                case 3:
                    NewInventarisation();
                    break;
                case 4:
                    var InfoProduct = new InfoProductForm(this);
                    //BarcodeFunc = null;
                    DialogResult result = InfoProduct.ShowDialog();
                    //Invoke((MethodInvoker)Activate);
                    //BarcodeFunc = BarcodeReaction;
                    break;
            }
        }

        private BindingSource BSource { get; set; }

        private void DocInventarisationListForm_Load(object sender, EventArgs e)
        {
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnEdit.ImageIndex = (int)Images.Edit;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnInfoProduct.ImageIndex = (int)Images.InfoProduct;
            GetDocInventarisations();
            gridInventarisations.DataSource = BSource;
            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "DocInventarisationID",
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
                Width = 50
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Склад",
                MappingName = "PlaceName",
                Width = 120
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "СкладНомер",
                MappingName = "PlaceID",
                Width = 0
            });
            gridInventarisations.TableStyles.Add(tableStyle);
        }

        private void GetDocInventarisations()
        {
            if (BSource == null)
                BSource = new BindingSource(Db.GetInventarisations(), null);
            else
            {
                BSource.DataSource = Db.GetInventarisations();
            }
        }

        private void EditDocInventarisation()
        {
            Cursor.Current = Cursors.WaitCursor;
            int row = gridInventarisations.CurrentRowIndex;
            var id = new Guid(gridInventarisations[row, 0].ToString());
            var form = new DocInventarisationForm(id, this,
                gridInventarisations[row, 2] == null ? "" : gridInventarisations[row, 2].ToString(), DocDirection.DocInventarisation, gridInventarisations[row, 4] == null ? 0 : Convert.ToInt32(gridInventarisations[row, 4]));
            form.Show();
            if (!form.IsDisposed && form.Enabled)
                Hide();
            Cursor.Current = Cursors.Default;
        }

        private void DocInventarisationListForm_DoubleClick(object sender, EventArgs e)
        {
            EditDocInventarisation();
        }

        private void NewInventarisation()
        {
            Cursor.Current = Cursors.WaitCursor;
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет сети, повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                Cursor.Current = Cursors.Default;
                return;
            }
            EndPointInfo endPointInfo;
            using (var form = new ChooseEndPointDialog())
            {
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK) return;
                endPointInfo = form.EndPointInfo;
            }
            DocInventarisation doc = Db.CreateNewInventarisation(Shared.PersonId, endPointInfo.PlaceId);
            if (doc == null)
            {
                MessageBox.Show(@"Не удалось создать документ в базе");
                Cursor.Current = Cursors.Default;
                return;
            }
            var inventarisationForm = new DocInventarisationForm(doc.DocInventarisationId, this, doc.Number, DocDirection.DocInventarisation, endPointInfo.PlaceId);
            //inventarisationform.Show();
            //if (!inventarisationform.IsDisposed && inventarisationform.Enabled)
            //    Hide();
            DialogResult resultInventarisationForm = inventarisationForm.ShowDialog();
            
            Cursor.Current = Cursors.Default;
        }

        private void gridInventarisations_DoubleClick(object sender, EventArgs e)
        {
            EditDocInventarisation();
        }

        protected override void FormLoad(object sender, EventArgs e)
        {           
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnAdd.ImageIndex = (int) Images.DocPlus;
        }

        private void gridInventarisations_CurrentCellChanged(object sender, EventArgs e)
        {
            gridInventarisations.Select(gridInventarisations.CurrentRowIndex);
        }

        
    }
}