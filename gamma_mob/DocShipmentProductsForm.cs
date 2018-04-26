using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocShipmentProductsForm : DocProductsBaseForm
    {
        protected DocShipmentProductsForm()
        {
            InitializeComponent();
        }

        public DocShipmentProductsForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm, DocDirection docDirection, RefreshDocProductDelegate refreshDocOrder)
            : base(docShipmentOrderId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, parentForm, docDirection, refreshDocOrder)
        {
            
        }

        protected override DataTable GetProducts()
        {
            return Db.DocShipmentOrderGoodProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, DocDirections);
        }

        protected override DataTable RemovalRProducts()
        {
            return Db.RemoveProductRFromOrder(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, Quantity);
        }
    }
}