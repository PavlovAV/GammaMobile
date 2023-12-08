using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.ComponentModel;

namespace gamma_mob
{
    public partial class DocShipmentProductsForm : DocProductsBaseForm
    {
        protected DocShipmentProductsForm()
        {
            InitializeComponent();
        }

        public DocShipmentProductsForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm, DocDirection docDirection, bool isMovementForOrder, OrderType orderType, RefreshDocProductDelegate refreshDocOrder, EndPointInfo startPointInfo, EndPointInfo endPointInfo, bool isEnableAddProductManual)
            : base(docShipmentOrderId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, parentForm, docDirection, isMovementForOrder, orderType, refreshDocOrder, startPointInfo, endPointInfo, isEnableAddProductManual)
        {
            
        }

        protected override BindingList<ProductBase> GetProducts()
        {
            //return Db.GetMovementGoodProducts(PlaceId, PersonId, NomenclatureId, CharacteristicId, QualityId, PlaceZoneId);
            return Db.DocShipmentOrderGoodProducts(DocId, NomenclatureId, CharacteristicId, QualityId, DocDirections, IsMovementForOrder, OrderType);
        }

        protected override DbOperationProductResult RemovalProduct(Guid scanId)
        {
            return Db.DeleteProductFromMovementOnMovementID(scanId);
        }

        protected override DialogResult GetDialogResult(string number, string place)
        {
            return Shared.ShowMessageQuestion("Удалить из приказа продукт " + number + Environment.NewLine + "и вернуть продукт на передел " + place + "?");
        }

        protected override DbOperationProductResult AddProductId(Guid? scanId, DbProductIdFromBarcodeResult getProductResult, EndPointInfo endPointInfo)
        {
            var addedProductIdToOrderResult = Db.AddProductIdToOrder(scanId, DocId, OrderType, Shared.PersonId, getProductResult.ProductId, DocDirection, endPointInfo, (int?)getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts, getProductResult.MeasureUnitId, getProductResult.FromProductId, getProductResult.FromPlaceId, getProductResult.FromPlaceZoneId);
            return addedProductIdToOrderResult == null ? null : (addedProductIdToOrderResult as DbOperationProductResult);
        }

    }
}