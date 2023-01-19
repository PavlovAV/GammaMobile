using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.ComponentModel;

namespace gamma_mob
{
    public partial class DocInventarisationProductsForm : DocProductsBaseForm
    {
        protected DocInventarisationProductsForm()
        {
            InitializeComponent();
        }

        public DocInventarisationProductsForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm)
            : base(docShipmentOrderId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, parentForm)
        {

        }

        protected override BindingList<ProductBase> GetProducts()
        {
            return Db.DocInventarisationNomenclatureProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId);
        }        

        protected override DbOperationProductResult RemovalProduct(Guid scanId)
        {
            Shared.SaveToLogInformation(@"Start RemovalProduct in DocInventarisationProductsForm: scanId = " + scanId.ToString());
            return Db.DeleteProductFromInventarisationOnInvProductID(scanId);
        }

        protected override DialogResult GetDialogResult(string number, string place)
        {
            return Shared.ShowMessageQuestion("Отменить инвентаризацию " + number + "?");
        }
    }
}