using System;
using System.Windows.Forms;

namespace gamma_mob
{
    public partial class FindDocOrder : Form
    {
        private FindDocOrder()
        {
            InitializeComponent();
        }

        public FindDocOrder(BindingSource bindingSource): this()
        {
            gridDocOrders.DataSource = bindingSource;
        }

        private void FindDocOrder_Load(object sender, EventArgs e)
        {
            
            
        }

        private void gridDocOrders_DoubleClick(object sender, EventArgs e)
        {
            //BindingSource bSource = (BindingSource)gridDocOrders.DataSource;
            //bSource.Position = gridDocOrders.CurrentRowIndex;
            Close();
        }      
    }
}