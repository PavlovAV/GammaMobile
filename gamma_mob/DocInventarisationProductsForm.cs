using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocInventarisationProductsForm : DocProductsBaseForm
    {
        protected DocInventarisationProductsForm()
        {
            InitializeComponent();
        }

        public DocInventarisationProductsForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm, RefreshDocProductDelegate RefreshDocInventarisation)
            : base(docShipmentOrderId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, parentForm, RefreshDocInventarisation)
        {
            
        }

        //protected override DataTable GetProducts()
        //{
        //    return Db.DocInventarisationNomenclatureProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId);
        //}

        protected override DataTable RemovalRProducts()
        {
            return Db.RemoveProductRFromInventarisation(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, Quantity);
        }
    }
}