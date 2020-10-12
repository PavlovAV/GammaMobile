using System;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using OpenNETCF.ComponentModel;
using System.Data;
using gamma_mob.Models;
using gamma_mob.Common;
using System.ComponentModel;

namespace gamma_mob.Common
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm(DateTime startDate, DateTime endDate, DataTable table)
        {
            InitializeComponent();
            StartDate = startDate;
            EndDate = endDate;
            Table = table;
            bkgndWorker = new BackgroundWorker();
            bkgndWorker.DoWork += new DoWorkEventHandler(bkgndWorker_DoWork);
            bkgndWorker.ProgressChanged += new ProgressChangedEventHandler(bkgndWorker_ProgressChanged);
            bkgndWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkgndWorker_RunWorkerCompleted);
            
        }

        public BackgroundWorker bkgndWorker { get; set; }
        private DateTime StartDate { get; set; }
        private DateTime EndDate { get; set; }
        private DataTable Table { get; set; }
        public DateTime ret { get; private set; }

        private void bkgndWorker_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            this.label1.Text = @"Выполнено " + e.ProgressPercentage.ToString() + @"%";
            this.progressBar1.Value = e.ProgressPercentage;
        }

        public void bkgndWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            int index = 0;
            int count = Table.Rows.Count;
            
            ret = StartDate;
            DateTime previousDate = ret;
            worker.ReportProgress(0);
            System.Threading.Thread.Sleep(100);
            int percent_old = 0;
            
            try
            {
                foreach (DataRow row in Table.Rows)
                {
                    var percent = Convert.ToInt32((index * 100) / count);
                    if (percent_old != percent)
                    {
                        worker.ReportProgress(percent);
                        percent_old = percent;
                    }
                    if (Db.UpdateBarcodes1C(
                         new CashedBarcode
                         {
                             DateChange = Convert.ToDateTime(row["DateChange"]),
                             TypeChange = Convert.ToInt32(row["TypeChange"]),
                             Barcode = row["Barcode"].ToString(),
                             Name = row["Name"].ToString(),
                             NomenclatureId = row.IsNull("NomenclatureID") ? new Guid() : new Guid(row["NomenclatureID"].ToString()),
                             CharacteristicId = row.IsNull("CharacteristicID") ? new Guid() : new Guid(row["CharacteristicID"].ToString()),
                             QualityId = row.IsNull("QualityID") ? new Guid() : new Guid(row["QualityID"].ToString()),
                             MeasureUnitId = row.IsNull("MeasureUnitID") ? new Guid() : new Guid(row["MeasureUnitID"].ToString()),
                             BarcodeId = new Guid(row["BarcodeID"].ToString()),
                             Number = row["Number"].ToString(),
                             KindId = row.IsNull("KindID") ? null : (int?)Convert.ToInt32(row["KindID"])
                         }))
                    {
                        if (previousDate != Convert.ToDateTime(row["DateChange"]))
                        {
                            ret = previousDate;//возвращаем время последней удачно записанной строки с предыдущим временем (чтобы если несколько изменений в один момент и произойдет сбой, то все эти записи этого момента кешировались повторно) 
                            previousDate = Convert.ToDateTime(row["DateChange"]);
                        }
                    }
                    else
                        break; ;//ошибка при обновлении текущей строки - дата последнего обновления есть дата предыдущей строки с дытой, отличной от текущей
                    index++;
                }
            }
            catch (Exception ex)
            { }
        }

        private void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (bkgndWorker.IsBusy)
            {
                bkgndWorker.CancelAsync();
                e.Cancel = true;
                DialogResult = DialogResult.OK;
            }
        }

        void bkgndWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void ProgressBarForm_Load(object sender, EventArgs e)
        {
            bkgndWorker.WorkerReportsProgress = true;
            bkgndWorker.WorkerSupportsCancellation = true;
            bkgndWorker.RunWorkerAsync();
        }
    }
}