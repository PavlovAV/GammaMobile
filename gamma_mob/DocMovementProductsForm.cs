using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.ComponentModel;

namespace gamma_mob
{
    public partial class DocMovementProductsForm : DocProductsBaseForm
    {
        protected DocMovementProductsForm()
        {
            InitializeComponent();
        }

        public DocMovementProductsForm(int placeId, Guid personId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Guid? placeZoneId, Form parentForm, bool isEnableAddProductManual)
            : base(placeId, personId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, placeZoneId, parentForm, isEnableAddProductManual)
        {
            
        }

        protected override BindingList<ProductBase> GetProducts()
        {
            return Db.GetMovementGoodProducts(PlaceId, PersonId, NomenclatureId, CharacteristicId, QualityId, PlaceZoneId);
        }

        protected override DbOperationProductResult RemovalProduct(Guid scanId)
        {
            Shared.SaveToLogInformation(@"Start RemovalProduct in DocMovementProductsForm: scanId = " + scanId.ToString());
            return Db.DeleteProductFromMovementOnMovementID(scanId);
        }
        
        protected override DialogResult GetDialogResult(string number, string place)
        {
            return Shared.ShowMessageQuestion("Удалить перемещение продукта " + number + Environment.NewLine + "и вернуть продукт на передел " + place + "?");
        }
    }
}