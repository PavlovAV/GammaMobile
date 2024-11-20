using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.ComponentModel;

namespace gamma_mob
{
    public partial class PalletItemProductsForm : DocProductsBaseForm
    {
        public PalletItemProductsForm()
        {
            InitializeComponent();
        }

        public PalletItemProductsForm(Guid productId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, Form parentForm, RefreshPalletItemsDelegate refreshPalletItems)
            : base(productId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, productKindId, parentForm, refreshPalletItems)
        {
        }

        protected override BindingList<ProductBase> GetProducts()
        {
            //return Db.GetMovementGoodProducts(PlaceId, PersonId, NomenclatureId, CharacteristicId, QualityId, PlaceZoneId);
            return Db.PalletItemProducts(ProductId, NomenclatureId, CharacteristicId, QualityId);
        }

        protected override DbOperationProductResult RemovalProduct(Guid scanId)
        {
            return Db.DeleteProductItemFromPalletOnMovementID(scanId);
        }

        protected override DialogResult GetDialogResult(string message, string place)
        {
            return Shared.ShowMessageQuestion("Удалить из паллеты " + message + Environment.NewLine + "и вернуть это кол-во на передел " + place + "?");
        }
    }
}
