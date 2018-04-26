using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocMovementProductsForm : DocProductsBaseForm
    {
        protected DocMovementProductsForm()
        {
            InitializeComponent();
        }

        public DocMovementProductsForm(int placeId, Guid personId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Guid? placeZoneId, Form parentForm, RefreshDocProductDelegate refreshDocOrder)
            : base(placeId, personId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, placeZoneId, parentForm, refreshDocOrder)
        {
            
        }

        protected override DataTable GetProducts()
        {
            return Db.GetMovementGoodProducts(PlaceId, PersonId, NomenclatureId, CharacteristicId, QualityId, PlaceZoneId);
        }

        protected override DataTable RemovalRProducts()
        {
            return Db.RemoveProductRFromMovement(PlaceId, PersonId, NomenclatureId, CharacteristicId, QualityId, PlaceZoneId, Quantity);
        }
    }
}