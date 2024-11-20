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

        public DocInventarisationProductsForm(Guid docId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, Form parentForm, bool isEnableAddProductManual)
            : base(docId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, productKindId, parentForm, isEnableAddProductManual)
        {

        }

        protected override BindingList<ProductBase> GetProducts()
        {
            return Db.DocInventarisationNomenclatureProducts(DocId, NomenclatureId, CharacteristicId, QualityId);
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