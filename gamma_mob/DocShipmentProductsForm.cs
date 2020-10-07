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
            , Guid characteristicId, Guid qualityId, Form parentForm, DocDirection docDirection, RefreshDocProductDelegate refreshDocOrder)
            : base(docShipmentOrderId, nomenclatureId, nomenclatureName
            , characteristicId, qualityId, parentForm, docDirection, refreshDocOrder)
        {
            
        }

        protected override BindingList<ProductBase> GetProducts()
        {
            //return Db.GetMovementGoodProducts(PlaceId, PersonId, NomenclatureId, CharacteristicId, QualityId, PlaceZoneId);
            return Db.DocShipmentOrderGoodProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, DocDirections);
        }

        protected override DbOperationProductResult RemovalProduct(Guid scanId)
        {
            return Db.DeleteProductFromMovementOnMovementID(scanId);
        }

        protected override DialogResult GetDialogResult(string number, string place)
        {
            return MessageBox.Show("Удалить из приказа продукт " + number + Environment.NewLine + "и вернуть продукт на передел " + place + "?"
                           , @"Операция с продуктом",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        }

        //protected override DataTable GetProducts()
        //{
        //    return Db.DocShipmentOrderGoodProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, DocDirections);
        //}

        //protected override DataTable RemovalRProducts()
        //{
        //    return Db.RemoveProductRFromOrder(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, Quantity);
        //}
    }
}