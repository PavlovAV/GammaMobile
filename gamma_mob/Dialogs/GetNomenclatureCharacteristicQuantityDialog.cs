using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using gamma_mob.Models;
using gamma_mob.Common;
using OpenNETCF.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace gamma_mob.Dialogs
{
    public partial class GetNomenclatureCharacteristicQuantityDialog : BaseFormWithChooseEndpoint
    {
        public GetNomenclatureCharacteristicQuantityDialog()
        {
            InitializeComponent();
            if (!Shared.IsScanGroupPackOnlyFromProduct && Shared.IsAvailabilityChoiseNomenclatureForMovingGroupPack)
            {
                gridChoose.Visible = true;
                //label4.Text = "Выберите номенклатуру или отсканируйте паллету, из которой упаковка/коробка, или зону стеллажа"; ;
            }
            else
            {
                //gridChoose.Visible = false;
                //label4.Text = "Отсканируйте паллету, из которой упаковка/коробка, или зону стеллажа"; ;
                //btnOK.Text = "Отмена";
            }
        }

        public GetNomenclatureCharacteristicQuantityDialog(Form parentForm, NomenclatureCharacteristicQuantityDialogParameter parameter)
            : this()
        {
            ParentForm = parentForm;
            CheckExistMovementToZone = parameter.CheckExistMovementToZone;
            //StartPointInfo = startPointInfo;
            EndPointInfo = parameter.EndPointInfo;
            StartPointInfo = parameter.StartPointInfo;
            isFilteringOnNomenclature = parameter.IsFilteringOnNomenclature;
            isFilteringOnEndpoint = parameter.IsFilteringOnEndpoint;
            //NomenclatureGoods = parameter.NomenclatureGoods;
            NomenclatureId = parameter.NomenclatureId ?? Guid.Empty;
            CharacteristicId = parameter.CharacteristicId ?? Guid.Empty;
            QualityId = parameter.QualityId ?? Guid.Empty;
            ProductKindId = parameter.ProductKindId ?? (byte)ProductKind.ProductMovement;
            NomenclatureGoods = parameter.NomenclatureGoods ?? (parameter.NomenclatureId != null ? new List<Nomenclature>() { new Nomenclature() { NomenclatureId = parameter.NomenclatureId ?? Guid.Empty, CharacteristicId = parameter.CharacteristicId ?? Guid.Empty, QualityId = parameter.QualityId ?? Guid.Empty } } : new List<Nomenclature>());
            ValidUntilDate = parameter.ValidUntilDate;
            //Barcode = barcode;
            //FromPlaceId = fromPlaceId;
            //FromPlaceZoneId = fromPlaceZone == null ? Guid.Empty : fromPlaceZone.PlaceZoneId;
            //заполнить выбор Откуда
            CreateFromPlaceZoneList();
            if (CreateNomenclatureList())
            {

                gridChoose.CurrentCell = new DataGridCell(0, 0);
                if (gridChoose.VisibleRowCount > 0)
                {
                    gridChoose.Select(0);
                    SetNomenclatureFromRow(0);
                }
                else
                {
                    gridChoose.UnselectAll();
                    SetNomenclatureFromRow(-1);
                }
                if (cmbFromPlaceZones.SelectedItem != null)
                    FromPlaceZoneChanged((cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceId, (cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceZoneId);
            }
            
            CountProducts = parameter.Quantity;
            CountFractionalProducts = parameter.QuantityFractional < 10 ? parameter.QuantityFractional * 100 : parameter.QuantityFractional < 100 ? parameter.QuantityFractional * 10 : parameter.QuantityFractional;
            if ((parameter.MeasureUnitId ?? Guid.Empty) != Guid.Empty)
            {
                MeasureUnitId = (Guid)parameter.MeasureUnitId;
                quantityMeasureUnit.Enabled = true;
                quantityMeasureUnit.SetMeasureQuantityDefaultMeasure((Guid)parameter.MeasureUnitId, parameter.Quantity, parameter.QuantityFractional);
            }
            else
            {
                MeasureUnitId = parameter.MeasureUnit.MeasureUnitID;
                quantityMeasureUnit.Enabled = false;
                quantityMeasureUnit.SetMeasureQuantityLocked(parameter.MeasureUnit, parameter.Quantity, parameter.QuantityFractional);
            }
            
            init = false;
            //if (startPointInfo == null)
            //    Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog ('null', 'null')");
            //else
            //    Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog ('" + startPointInfo.PlaceId + "', '" + startPointInfo.PlaceZoneId + "')");
        }
        /*
        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, bool setIsFilteringOnNomenclature, bool setIsFilteringOnEndpoint, List<Nomenclature> nomenclatureGoods)
            : this()
        {
            //StartPointInfo = startPointInfo;
            EndPointInfo = endPointInfo;
            StartPointInfo = startPointInfo;
            isFilteringOnNomenclature = setIsFilteringOnNomenclature;
            isFilteringOnEndpoint = setIsFilteringOnEndpoint;
            NomenclatureGoods = nomenclatureGoods;
            Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog (fromPlaceId, fromPlaceZone, parentForm, isFilteringOnNomenclature, isFilteringOnEndpoint, nomenclatureGoods)");
            //Barcode = barcode;
            //FromPlaceId = fromPlaceId;
            //FromPlaceZoneId = fromPlaceZone == null ? Guid.Empty : fromPlaceZone.PlaceZoneId;
            //заполнить выбор Откуда
            CreateFromPlaceZoneList();
            if (!isFilteringOnNomenclature && CreateNomenclatureList())
            {
                
                gridChoose.CurrentCell = new DataGridCell(0, 0);
                if (gridChoose.VisibleRowCount > 0)
                {
                    gridChoose.Select(0);
                    SetNomenclatureFromRow(0);
                }
                else
                {
                    gridChoose.UnselectAll();
                    SetNomenclatureFromRow(-1);
                }
                if (cmbFromPlaceZones.SelectedItem != null)
                    FromPlaceZoneChanged((cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceId, (cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceZoneId);
            }
            init = false;
            if (startPointInfo == null)
                Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog ('null', 'null')");
            else
                Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog ('" + startPointInfo.PlaceId + "', '" + startPointInfo.PlaceZoneId + "')");
        }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo)
            : this(startPointInfo, endPointInfo, false, false)
        { }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, bool isFilteringOnNomenclature, bool isFilteringOnEndpoint)
            : this(startPointInfo, endPointInfo, false, false, null)
        { }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm, bool isFilteringOnNomenclature, bool isFilteringOnEndpoint)
            : this(startPointInfo, endPointInfo, parentForm, isFilteringOnNomenclature, isFilteringOnEndpoint, null)
        { }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm, bool isFilteringOnNomenclature, bool isFilteringOnEndpoint, List<Nomenclature> nomenclatureGoods)
            : this(startPointInfo, endPointInfo, parentForm, isFilteringOnNomenclature, isFilteringOnEndpoint, nomenclatureGoods, false)
        { }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm, bool isFilteringOnNomenclature, bool isFilteringOnEndpoint, List<Nomenclature> nomenclatureGoods, bool checkExistMovementToZone)
            : this(startPointInfo, endPointInfo, isFilteringOnNomenclature, isFilteringOnEndpoint, nomenclatureGoods)
        {
            ParentForm = parentForm;
            CheckExistMovementToZone = checkExistMovementToZone;
            Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog (fromPlaceId, fromPlaceZone, '" + parentForm.Name + "')");
        }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm)
            : this(startPointInfo, endPointInfo, parentForm, false, false)
        { }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, byte? productKindId)
            : this(startPointInfo, endPointInfo, parentForm, true, startPointInfo != null && startPointInfo.PlaceId > 0, (nomenclatureId != null ? new List<Nomenclature>() { new Nomenclature() { NomenclatureId = nomenclatureId ?? Guid.Empty, CharacteristicId = characteristicId ?? Guid.Empty, QualityId = qualityId ?? Guid.Empty } } : new List<Nomenclature>()))
        {
            NomenclatureId = nomenclatureId ?? Guid.Empty;
            CharacteristicId = characteristicId ?? Guid.Empty;
            QualityId = qualityId ?? Guid.Empty;
            ProductKindId = productKindId ?? (byte)ProductKind.ProductMovement;
            //gridChoose.Enabled = false;
            if (isFilteringOnNomenclature && CreateNomenclatureList())
            {
                gridChoose.CurrentCell = new DataGridCell(0, 0);
                if (gridChoose.VisibleRowCount > 0)
                {
                    gridChoose.Select(0);
                    SetNomenclatureFromRow(0);
                    btnOK.Enabled = NomenclatureId != Guid.Empty;
                }
                else
                {
                    gridChoose.UnselectAll();
                    SetNomenclatureFromRow(-1);
                }

            }
            //            isFilteringOnNomenclature = true;
            Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog (fromPlaceId, fromPlaceZone, parentForm, '" + NomenclatureId + "', '" + CharacteristicId + "', '" + QualityId + "')");
        }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, byte? productKindId, MeasureUnit measureUnit, int quantity, int? quantityFractional)
            : this(startPointInfo, endPointInfo, parentForm, nomenclatureId, characteristicId, qualityId, productKindId)
        {
            //quantityMeasureUnit.Value = quantity;
            //quantityMeasureUnit.MeasureUnitId = MeasureUnitId;
            MeasureUnitId = measureUnit.MeasureUnitID;
            CountProducts = quantity;
            CountFractionalProducts = quantityFractional < 10 ? quantityFractional * 100 : quantityFractional < 100 ? quantityFractional * 10 : quantityFractional; 
            quantityMeasureUnit.Enabled = false;
            quantityMeasureUnit.SetMeasureQuantityLocked(measureUnit, quantity, quantityFractional);
            Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog (fromPlaceId, fromPlaceZone, parentForm, nomenclatureId, characteristicId, qualityId, '" + measureUnit.MeasureUnitID + "', '" + quantity + "')");
        }

        public GetNomenclatureCharacteristicQuantityDialog(EndPointInfo startPointInfo, EndPointInfo endPointInfo, Form parentForm, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, byte? productKindId, Guid measureUnitId, int quantity, int? quantityFractional)
            : this(startPointInfo, endPointInfo, parentForm, nomenclatureId, characteristicId, qualityId, productKindId)
        {
            //quantityMeasureUnit.Value = quantity;
            //quantityMeasureUnit.MeasureUnitId = MeasureUnitId;
            MeasureUnitId = measureUnitId;
            CountProducts = quantity;
            CountFractionalProducts = quantityFractional < 10 ? quantityFractional * 100 : quantityFractional < 100 ? quantityFractional * 10 : quantityFractional; 
            quantityMeasureUnit.Enabled = true;
            quantityMeasureUnit.SetMeasureQuantityDefaultMeasure(measureUnitId, quantity, quantityFractional);
            Shared.SaveToLogInformation("Open GetNomenclatureCharacteristicQuantityDialog (fromPlaceId, fromPlaceZone, parentForm, nomenclatureId, characteristicId, qualityId, measureUnitId = '" + measureUnitId + "', '" + quantity + "')");
        }
        */
        public void SetGridRowHeight(DataGrid dg, int nRow, int cy)
        {
            ArrayList arrRows = ((ArrayList)(dg.GetType().GetField("m_rlrow", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(dg)));
            object row = arrRows[nRow];
            row.GetType().GetField("m_cy", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).SetValue(row, cy);
            dg.Invalidate();
        }

        private bool init = true;

        //public Guid FromPlaceZoneId { get; set; }
        //public int FromPlaceId { get; set; }
        private Guid _nomenclatureId { get; set; }
        public Guid NomenclatureId 
        {
            get { return _nomenclatureId; } 
            set
            {
                _nomenclatureId = value;
                btnOK.Enabled = gridChoose.VisibleRowCount > 0 && NomenclatureId != Guid.Empty;
            }
        
        }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public byte? ProductKindId { get; private set; }
        public int CountProducts { get; private set; }
        public int? CountFractionalProducts { get; private set; }
        public Guid MeasureUnitId { get; private set; }
        public Guid? BaseMeasureUnitId { get; private set; }
        public decimal CountProductsInBaseMeasureUnit { get; set; }
        public int? BaleWeight { get; private set; }
        private DateTime? _validUntilDate { get; set; }
        public DateTime? ValidUntilDate 
        {
            get { return _validUntilDate; } 
            set
            {
                _validUntilDate = value;
                label1.Text = "Номенклатура" + (_validUntilDate == null ? "" : String.Format("-годен до {0:dd.MM.yy}", _validUntilDate));
            }
        }
        private bool isFilteringOnNomenclature = false;
        private bool isFilteringOnEndpoint = false;
        private List<Nomenclature> NomenclatureGoods { get; set; }
        //private string Barcode { get; set; }
        private bool CheckExistMovementToZone = false;

        private BindingSource BSourceNomenclature { get; set; }
        private BindingList<ChooseNomenclatureItem> ChooseNomenclatureList { get; set; }

        private BindingSource BSourcePlaceZone { get; set; }
        private List<PlaceZone> ChoosePlaceZoneList { get; set; }
        //private PlaceZone _selectedPlaceZone { get; set; }
        //public PlaceZone SelectedPlaceZone 
        //{
        //    get { return _selectedPlaceZone; }
        //    set
        //    {
        //        _selectedPlaceZone = value;
        //        FromPlaceZoneId = value == null ? Guid.Empty : value.PlaceZoneId;
        //    }
        //}

//        private BindingSource BSourceMeasureUnit { get; set; }
//        private List<MeasureUnit> ChooseMeasureUnitList { get; set; }
        //private MeasureUnit _selectedMeasureUnit { get; set; }
        //private MeasureUnit SelectedMeasureUnit
        //{
        //    get { return _selectedMeasureUnit; }
        //    set
        //    {
        //        _selectedMeasureUnit = value;
        //        MeasureUnitId = value == null ? Guid.Empty : value.MeasureUnitID ;
        //    }
        //}

        //private void GetPlaceZoneList(int placeId)
        //{
        //    List<PlaceZone> list = Shared.PlaceZones.FindAll(z => z.PlaceId == placeId && z.IsValid);
        //    ChoosePlaceZoneList = list;
        //    if (BSource == null)
        //        BSource = new BindingSource { DataSource = ChoosePlaceZoneList };
        //    else
        //    {
        //        BSource.DataSource = ChoosePlaceZoneList;
        //    }
        //    cmbFromPlaceZones.DataSource = BSource;
        //}

        private void GetNomenclatureList()
        {
            //List<ChooseNomenclatureItem> list = Shared.Barcodes1C.GetNomenclaturesFromBarcodeInBarcodes(Barcode);
            ChooseNomenclatureList = new BindingList<ChooseNomenclatureItem>();
            //////if (StartPointInfo.PlaceId != 0)
            //if (NomenclatureId != Guid.Empty)
            //{
            //    ChooseNomenclatureList = Db.GetNomenclatureCharacteristicQualityFromId(NomenclatureId, CharacteristicId, QualityId);
            //}
            //else
            {
                var chooseNomenclatureList = Db.GetNomenclatureInPlaceZone(StartPointInfo.PlaceId, StartPointInfo.PlaceZoneId ?? Guid.Empty, isFilteringOnNomenclature, NomenclatureId, CharacteristicId, QualityId);
                if (NomenclatureGoods != null && NomenclatureGoods.Count > 0)
                {
                    ChooseNomenclatureList.Clear();
                    foreach (var item in NomenclatureGoods)
                    {
                        var nomenclature = chooseNomenclatureList.FirstOrDefault(n => n.NomenclatureId == item.NomenclatureId && n.CharacteristicId == item.CharacteristicId && n.QualityId == item.QualityId);
                        if (nomenclature != null)
                            ChooseNomenclatureList.Add(nomenclature);
                    }
                }
                else
                    ChooseNomenclatureList = chooseNomenclatureList;
            }
            if (BSourceNomenclature == null)
                BSourceNomenclature = new BindingSource { DataSource = ChooseNomenclatureList };
            else
            {
                BSourceNomenclature.DataSource = ChooseNomenclatureList;
            }
            gridChoose.DataSource = BSourceNomenclature;
        }

        private bool CreateNomenclatureList()
        {
            GetNomenclatureList();
            var tableStyle = new DataGridTableStyle { MappingName = BSourceNomenclature.GetListName(null) };
            var columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Наименование";
            columnStyle.MappingName = "Name";
            columnStyle.Width = 200;
            tableStyle.GridColumnStyles.Add(columnStyle);
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "NomenclatureId",
                MappingName = "NomenclatureId",
                Width = 0,
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "CharacteristicId",
                MappingName = "CharacteristicId",
                Width = 0,
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "QualityId",
                MappingName = "QualityId",
                Width = 0,
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "ProductKindId",
                MappingName = "ProductKindId",
                Width = 0,
            });
            gridChoose.TableStyles.Add(tableStyle);
            //columnStyle.TextBox.Multiline=true;
            //columnStyle.TextBox.WordWrap = true;

            RefreshGridRowHeight();
            return true;
        }

        private void GetFromPlaceZoneList(EndPointInfo startPointInfo)
        {
            List<PlaceZone> list = new List<PlaceZone>();
            if (startPointInfo == null)
            {
                var place = Shared.Warehouses;
                list.Add(new PlaceZone() { PlaceId = 0, PlaceZoneId = Guid.Empty, Name = "Выберите ...", IsValid = true });
                foreach (var placeZone in place.Where(z => z.WarehouseId != 0).Select(z => new PlaceZone() { PlaceId = z.WarehouseId, PlaceZoneId = Guid.Empty, Name = z.WarehouseName, IsValid = true }))
                    list.Add(placeZone);
                //list = place.Where(z => z.WarehouseId != 0).Select(z => new PlaceZone() { PlaceId = z.WarehouseId, PlaceZoneId = Guid.Empty, Name = z.WarehouseName, IsValid = true }).ToList();
                
            }
            else
            {
                var place = Shared.Warehouses.Find(w => w.WarehouseId == startPointInfo.PlaceId);
                if ((place.WarehouseZones != null && place.WarehouseZones.Count > 0))
                    if (isFilteringOnEndpoint)
                        list = Shared.PlaceZones.FindAll(z => z.PlaceId == startPointInfo.PlaceId && z.IsValid && z.PlaceZoneId == startPointInfo.PlaceZoneId).ToList();
                    else
                    {
                        list.Add(new PlaceZone() { PlaceId = 0, PlaceZoneId = Guid.Empty, Name = "Выберите ...", IsValid = true });
                        foreach (var placeZone in Shared.PlaceZones.FindAll(z => z.PlaceId == startPointInfo.PlaceId && z.IsValid))
                            list.Add(placeZone);
                    }
                else
                    list.Add(new PlaceZone() { PlaceId = startPointInfo.PlaceId, PlaceZoneId = Guid.Empty, Name = "Передел " + startPointInfo.PlaceName, IsValid = true });
                /*if ((place.WarehouseZones != null && place.WarehouseZones.Count > 0))
                    list = Shared.PlaceZones.FindAll(z => z.PlaceId == startPointInfo.PlaceId && z.IsValid && (!isFilteringOnEndpoint || (isFilteringOnEndpoint && (z.PlaceZoneId == startPointInfo.PlaceZoneId || z.PlaceZoneParentId == startPointInfo.PlaceZoneId))));
                else
                    list.Add(new PlaceZone() { PlaceId = startPointInfo.PlaceId, PlaceZoneId = Guid.Empty, Name = "Передел " + startPointInfo.PlaceName, IsValid = true });*/
            }
            ChoosePlaceZoneList = list;
            if (BSourcePlaceZone == null)
                BSourcePlaceZone = new BindingSource { DataSource = ChoosePlaceZoneList };
            else
                BSourcePlaceZone.DataSource = ChoosePlaceZoneList;
            cmbFromPlaceZones.DataSource = BSourcePlaceZone;
            
        }

        private bool CreateFromPlaceZoneList()
        {
            GetFromPlaceZoneList(StartPointInfo);
            cmbFromPlaceZones.DisplayMember = "Name";
            cmbFromPlaceZones.ValueMember = "PlaceZoneId";
            //var selectedPlaceZone = fromPlaceZone == null ? list[0] : list.Find(z => z.PlaceZoneId == FromPlaceZoneId);
            //cmbFromPlaceZones.SelectedItem = selectedPlaceZone;
            //cmbFromPlaceZones.SelectedItem = startPointInfo == null ? list.First(l => l.PlaceId == 0) : startPointInfo.PlaceZoneId == null ? list[0] : list.Find(z => z.PlaceZoneId == (startPointInfo.PlaceZoneId == null ? Guid.Empty : startPointInfo.PlaceZoneId));
            //cmbFromPlaceZones.SelectedIndex = cmbFromPlaceZones.Items.Count > 0 ? cmbFromPlaceZones.Items.Count - 1 : 0;
            if (StartPointInfo == null)
                SetPlaceZone(0, Guid.Empty);
            else if (StartPointInfo != null && (StartPointInfo.PlaceZoneId ?? Guid.Empty) == Guid.Empty)
            {
                cmbFromPlaceZones.SelectedIndex = 0;
                //SetPlaceZone(startPointInfo.PlaceId, Guid.Empty);
                ////FromPlaceZoneChanged((cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceId,(cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceZoneId)
            }
            else
            {
                for (int i = 0; i < cmbFromPlaceZones.Items.Count; i++)
                {
                    if ((cmbFromPlaceZones.Items[i] as PlaceZone).PlaceZoneId == StartPointInfo.PlaceZoneId)
                        cmbFromPlaceZones.SelectedIndex = i;
                }
                ////cmbFromPlaceZones.SelectedIndex = cmbFromPlaceZones.Items.Count > 0 && startPointInfo.PlaceZoneId != null ? cmbFromPlaceZones.Items.IndexOf() : 0;
                ////SetPlaceZone(startPointInfo.PlaceId, startPointInfo.PlaceZoneId == null ? Guid.Empty : (Guid)startPointInfo.PlaceZoneId);
                //FromPlaceZoneChanged(startPointInfo.PlaceId, startPointInfo.PlaceZoneId == null ? Guid.Empty : (Guid)startPointInfo.PlaceZoneId);
            }
            return true;
        }

        private bool RefreshGridRowHeight()
        {
            for (int i = 0; i < gridChoose.BindingContext[gridChoose.DataSource].Count; i++)
            {
                SetGridRowHeight(gridChoose, i, (int)gridChoose.Font.Size * (3 * 2 + 1));
            }
            ScrollBar sb = (ScrollBar)typeof(DataGrid).GetField("m_sbVert",
            BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(gridChoose);
            sb.Visible = true;
            return true;
        }
        //private bool setNomenclaturId()
        //{
        //    var good = ChooseNomenclatureList[gridChoose.CurrentRowIndex];
        //    if (good == null || gridChoose.Visible == false)
        //    {
        //        return false;
        //    }
        //    NomenclatureId = good.NomenclatureId;
        //    CharacteristicId = good.CharacteristicId;
        //    QualityId = good.QualityId;
        //    return GetCountProducts();
        //}
        
        private bool GetCountProducts()
        {
            using (var form = new SetCountProductsDialog())
            {
                DialogResult result = form.ShowDialog();
                Invoke((MethodInvoker)Activate);
                if (result != DialogResult.OK || form.Quantity == 0)
                {
                    Shared.ShowMessageInformation(@"Не указано количество продукта. Продукт не добавлен!");
                    return false;
                }
                else
                {
                    CountProducts = form.Quantity;
                    Shared.SaveToLogInformation(@"Установлено кол-во " + form.Quantity);
                }
            }
            return true;
        }

        private bool checkSetBaleWeight()
        {
            if (EndPointInfo != null && EndPointInfo.PlaceGroupId == 0 && quantityMeasureUnit != null && quantityMeasureUnit.SelectedItem != null && ((MeasureUnit)quantityMeasureUnit.SelectedItem).Numerator == 0)
            {
                using (var form = new SetCountProductsDialog(800, " кг.", "Укажите вес кипы"))
                {
                    DialogResult result = form.ShowDialog();
                    Invoke((MethodInvoker)Activate);
                    if (result != DialogResult.OK)
                    {
                        BaleWeight = null;
                        Shared.ShowMessageInformation(@"Вес кипы не указан. Будет добавлен с нулевым весом.");
                        //return false;
                    }
                    else
                    {
                        BaleWeight = form.Quantity;
                        Shared.SaveToLogInformation(@"Установлен вес кипы " + form.Quantity + " кг.");
                    }
                }
            }
            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (StartPointInfo == null || StartPointInfo.PlaceId == 0)
                Shared.ShowMessageInformation("Выберите склад Откуда!");
            else if (NomenclatureId == Guid.Empty)
                Shared.ShowMessageInformation("Выберите номенклатуру!");
            else if (EndPointInfo != null && EndPointInfo.IsAvailabilityPlaceZoneId && !EndPointInfo.IsSettedDefaultPlaceZoneId)
                base.ChooseEndPoint(this.ChoosePlaceZone, CheckExistMovementToZone);//, new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = false, getProductResult = getProductResult });
            else 
            {
                if (checkSetBaleWeight())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            //if (setNomenclaturId())
                //DialogResult = DialogResult.OK;
            //else
            //    DialogResult = DialogResult.Cancel;
            //Close();
            //if ((ParentForm is BaseFormWithChooseEndpoint))
            //    //if ((Parent as BaseFormWithChooseEndpoint).ChooseEndPointForm != null)
            //{
            //    Invoke((MethodInvoker)(() => (ParentForm as BaseFormWithChooseEndpoint).TopMost = true));
            //    Invoke((MethodInvoker)delegate()
            //    {
            //        (ParentForm as BaseFormWithChooseEndpoint).BringToFrontChooseEndPointForm();
            //    });
            //}
        }

        private void ChoosePlaceZone(EndPointInfo param)
        {
            base.ReturnPlaceZoneBeforeChoosedPlaceZone -= this.ChoosePlaceZone;
            //BarcodeFunc = this.BarcodeReaction;
            if (param != null)
            {
                EndPointInfo = param;
                //AddProductByBarcode(param.barcode, param.endPointInfo, param.fromBuffer, param.getProductResult);
                if (checkSetBaleWeight())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void gridChoose_DoubleClick(object sender, EventArgs e)
        {
            //if (setNomenclaturId())
                DialogResult = DialogResult.OK;
            //else
            //    DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = ChooseNomenclatureCharacteristicFromBarcode;
            //if (!CreateNomenclatureList())
            //{
            //    DialogResult = DialogResult.Abort;
            //    Close();
            //    return;
            //}
        }

        protected override void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ReturnedResult && DialogResult == DialogResult.OK)
            {
                if (quantityMeasureUnit == null || quantityMeasureUnit.MeasureUnit == null)
                {
                    Db.AddMessageToLog((quantityMeasureUnit == null ? "quantityMeasureUnit == null" : "quantityMeasureUnit.MeasureUnit == null") + "; NomenclatureID == " + NomenclatureId.ToString() + "; StartPointInfo.PlaceZoneId == " + StartPointInfo == null ? "StartPointInfo is null" : StartPointInfo.PlaceZoneId == null ? "StartPointInfo.PlaceZoneId is null" : StartPointInfo.PlaceZoneId.ToString());
                    DialogResult = DialogResult.Cancel;
                }
                else
                    ReturnResult();
            }
            base.OnFormClosing(sender, e);
        }

        public bool ReturnedResult = true;
        
        public void ReturnResult()
        {
            CountProducts = quantityMeasureUnit.Value;
            CountFractionalProducts = quantityMeasureUnit.ValueFractional < 10 ? quantityMeasureUnit.ValueFractional * 100 : quantityMeasureUnit.ValueFractional < 100 ? quantityMeasureUnit.ValueFractional * 10 : quantityMeasureUnit.ValueFractional;
            MeasureUnitId = quantityMeasureUnit.MeasureUnit.MeasureUnitID;
            if (quantityMeasureUnit.MeasureUnit.MeasureUnitID == null || quantityMeasureUnit.MeasureUnit.MeasureUnitID == Guid.Empty || quantityMeasureUnit.MeasureUnit.MeasureUnitID == new Guid("00000000-0000-0000-0000-000000000000"))
                Db.AddMessageToLog("quantityMeasureUnit.MeasureUnit.MeasureUnitID == 00000000-0000-0000-0000-000000000000; NomenclatureID == " + NomenclatureId.ToString() + "; StartPointInfo.PlaceZoneId == " + StartPointInfo == null ? "StartPointInfo is null" : StartPointInfo.PlaceZoneId == null ? "StartPointInfo.PlaceZoneId is null" : StartPointInfo.PlaceZoneId.ToString());
            //CountProductsInBaseMeasureUnit = quantityMeasureUnit.MeasureUnit.Numerator == 0 ? baleWeight : quantityMeasureUnit.ValueInBaseMeasureUnit; //Convert.ToInt32(quantityMeasureUnit.Value * quantityMeasureUnit.MeasureUnit.Coefficient);
            BaseMeasureUnitId = quantityMeasureUnit.MeasureUnit.BaseMeasureUnitID;
            CountProductsInBaseMeasureUnit = quantityMeasureUnit.ValueInBaseMeasureUnit; //Convert.ToInt32(quantityMeasureUnit.Value * quantityMeasureUnit.MeasureUnit.Coefficient);
            //CountProducts = numericUpDownWithButtons.Value;
            if (ParentForm != null && (ParentForm is BaseFormWithProducts))
            {
                Invoke((MethodInvoker)delegate()
                    {
                        (ParentForm as BaseFormWithProducts).ClosingGetNomenclatureCharacteristicQuantityDialog();
                    });
            }
        }

        public void SetBarcodeReaction(BarcodeReceivedEventHandler chooseNomenclatureCharacteristicFromBarcode)
        {
            BarcodeFunc = chooseNomenclatureCharacteristicFromBarcode;
        }

        public void SetBarcodeText(string barcode)
        {
            //edtBarcode.Text = barcode;
        }

        private void ChooseNomenclatureCharacteristicFromBarcode(string barcode)
        {
            if (barcode.Length > 0)
            {
                var placeZones = ChoosePlaceZoneList.Any(p => p.PlaceId == 0)
                    ? Shared.PlaceZones.FindAll(p => p.Barcode == barcode && p.IsValid) // && (StartPointInfo == null || (StartPointInfo != null && p.PlaceId == StartPointInfo.PlaceId)));
                    : ChoosePlaceZoneList.FindAll(p => p.Barcode == barcode); // && (StartPointInfo == null || (StartPointInfo != null && p.PlaceId == StartPointInfo.PlaceId)));
                if (placeZones != null && placeZones.Count > 0)
                {
                    if (StartPointInfo != null && StartPointInfo.PlaceId != 0 && placeZones.Find(p => (StartPointInfo == null || (StartPointInfo != null && p.PlaceId == StartPointInfo.PlaceId))) == null)
                        Shared.ShowMessageError(@"Найденная по ШК " + barcode + " зона не принадлежит складу отгрузки!" + Environment.NewLine + "Невозможно определить зону склада");
                    else if (placeZones.Count > 1)
                        Shared.ShowMessageError(@"По ШК " + barcode + " найдено несколько зон!" + Environment.NewLine + "Невозможно определить зону склада");
                    else
                    {
                        if (this.InvokeRequired)
                        {
                            MethodInvoker del = delegate
                            {
                                GetFromPlaceZoneList(new EndPointInfo(placeZones[0].PlaceId, placeZones[0].PlaceZoneId));
                            };
                            this.Invoke(del);
                        }
                        else GetFromPlaceZoneList(new EndPointInfo(placeZones[0].PlaceId, placeZones[0].PlaceZoneId));
                        
                        //if (StartPointInfo == null)
                        //    SetPlaceZone(0, Guid.Empty);
                        //else if (StartPointInfo != null && (StartPointInfo.PlaceZoneId ?? Guid.Empty) == Guid.Empty)
                        //{
                        //    Invoke((MethodInvoker)(() => cmbFromPlaceZones.SelectedIndex = 0));
                        //    //SetPlaceZone(startPointInfo.PlaceId, Guid.Empty);
                        //    ////FromPlaceZoneChanged((cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceId,(cmbFromPlaceZones.SelectedItem as PlaceZone).PlaceZoneId)
                        //}
                        //else
                        {
                            for (int i = 0; i < cmbFromPlaceZones.Items.Count; i++)
                            {
                                if ((cmbFromPlaceZones.Items[i] as PlaceZone).PlaceZoneId == placeZones[0].PlaceZoneId)
                                    Invoke((MethodInvoker)(() => cmbFromPlaceZones.SelectedIndex = i));
                            }
                            ////cmbFromPlaceZones.SelectedIndex = cmbFromPlaceZones.Items.Count > 0 && startPointInfo.PlaceZoneId != null ? cmbFromPlaceZones.Items.IndexOf() : 0;
                            ////SetPlaceZone(startPointInfo.PlaceId, startPointInfo.PlaceZoneId == null ? Guid.Empty : (Guid)startPointInfo.PlaceZoneId);
                            //FromPlaceZoneChanged(startPointInfo.PlaceId, startPointInfo.PlaceZoneId == null ? Guid.Empty : (Guid)startPointInfo.PlaceZoneId);
                        }
                        /*if (ChoosePlaceZoneList.Any(p => p.PlaceId == 0))
                        {
                            ChoosePlaceZoneList = Shared.PlaceZones.FindAll(z => z.PlaceId == placeZones[0].PlaceId && z.IsValid);
                        }
                        BSourcePlaceZone.DataSource = ChoosePlaceZoneList;
                        cmbFromPlaceZones.DataSource = BSourcePlaceZone;
                        Invoke((MethodInvoker)cmbFromPlaceZones.Update);
                        var placeZone = ChoosePlaceZoneList.FirstOrDefault(p => p.PlaceZoneId == placeZones[0].PlaceZoneId);
                        cmbFromPlaceZones.SelectedItem = placeZone;
                        if (this.InvokeRequired)
                        {
                            MethodInvoker del = delegate
                            {
                                FromPlaceZoneChanged(placeZone.PlaceId, placeZone.PlaceZoneId);
                            };
                            this.Invoke(del);
                        }
                        else FromPlaceZoneChanged(placeZone.PlaceId, placeZone.PlaceZoneId);*/
                    }
                }
                else if (Shared.Warehouses.Any(p => p.Barcode == barcode && (StartPointInfo == null || (StartPointInfo != null && p.WarehouseId == StartPointInfo.PlaceId))))
                {
                        Shared.ShowMessageError(@"По ШК " + barcode + " найден передел " + Shared.Warehouses.FirstOrDefault(p => p.Barcode == barcode && (StartPointInfo == null || (StartPointInfo != null && p.WarehouseId == StartPointInfo.PlaceId))) 
                            + Environment.NewLine + "Выберите (или отсканируйте) зону!");
                }
                else
                {
                    bool e = false;
                    Invoke(
                        (MethodInvoker)
                    (() => e = gridChoose.Enabled));

                    if (e)
                    {
                        DbProductIdFromBarcodeResult getFromProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                        if (getFromProductResult != null && getFromProductResult.ProductId != null)// && getFromProductResult.ProductId != Guid.Empty)
                        {
                            var r = getFromProductResult.ProductId != Guid.Empty;
                            if (gridChoose.VisibleRowCount > 0)
                            {
                                var nomenclature = getFromProductResult.CharacteristicId != Guid.Empty
                                    ? ChooseNomenclatureList.FirstOrDefault(n => n.NomenclatureId == getFromProductResult.NomenclatureId && n.CharacteristicId == getFromProductResult.CharacteristicId)
                                    : ChooseNomenclatureList.FirstOrDefault(n => n.NomenclatureId == getFromProductResult.NomenclatureId);
                                if (nomenclature != null)
                                {
                                    var rowIndex = ChooseNomenclatureList.IndexOf(nomenclature);
                                    if (rowIndex != 0)
                                    {
                                        ChooseNomenclatureList.Remove(nomenclature);
                                        ChooseNomenclatureList.Insert(0,nomenclature);
                                    }
                                    //gridChoose.Focus();
                                    gridChoose.UnselectAll();
                                    gridChoose.CurrentCell = new DataGridCell(0,0);
                                    gridChoose.Select(0);
                                    SetNomenclatureFromRow(0);
                                }
                            }
                        }
                        else
                        {
                            Shared.ShowMessageError(@"Ошибка! Штрих-код продукта не распознан!" + Environment.NewLine + @"Попробуйте еще раз или выберите номенклатуру (если возможно)");
                        }
                    }
                }
            }
            else
            {
                Shared.ShowMessageError(@"Ошибка! Штрих-код пустой!" + Environment.NewLine + @"Попробуйте еще раз или выберите номенклатуру (если возможно)");
            }
        }

        public bool CheckNomenclatureInNomenclatureList(Guid productId)
        {
            Guid? res = Db.GetProductNomenclature(productId);
            if ((res ?? Guid.Empty) == Guid.Empty)
            {
                Shared.SaveToLogError(@"Db.GetProductNomenclature is null", null, productId);
                return false;
            }
            else
            {
                //if (ChooseNomenclatureList == null || ChooseNomenclatureList.Count == 0)
                //{
                //    Invoke((MethodInvoker)delegate()
                //    {
                //        GetNomenclatureList();
                //    });
                //}
                //Shared.SaveToLogInformation(@"Db.GetProductNomenclature return " + res.ToString() + @"(ChooseNomenclatureList.Count = " + ChooseNomenclatureList.Count);

                //var resFind = ChooseNomenclatureList.Find(n => n.NomenclatureId == (Guid)res);
                return false;// resFind != null;
            }
        }

        public void btnAddBarcode_Click(object sender, EventArgs e)
        {
            //Shared.SaveToLogInformation(@"Выбрано Добавить ШК: " + edtBarcode.Text);
            ////ChooseNomenclatureCharacteristicFromBarcode(edtBarcode.Text);
            //if (BarcodeFunc != null)
                //BarcodeFunc.Invoke(edtBarcode.Text);
            //else
                //ChooseNomenclatureCharacteristicFromBarcode(edtBarcode.Text);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SetPlaceZone(int placeId, Guid placeZoneId)
        {
            //FromPlaceId = placeId;
            //FromPlaceZoneId = placeZoneId;
            StartPointInfo = new EndPointInfo(placeId, placeZoneId);
        }

        private void FromPlaceZoneChanged(int placeId, Guid placeZoneId)
        {
            if (/*!init*/ placeId != 0 || ( StartPointInfo != null && StartPointInfo.PlaceId != 0 && placeId == 0))
            {
                SetPlaceZone(placeId, placeZoneId);
                GetNomenclatureList();
                RefreshGridRowHeight();
                gridChoose.CurrentCell = new DataGridCell(0, 0);
                if (gridChoose.VisibleRowCount > 0)
                {
                    gridChoose.Select(0);
                    SetNomenclatureFromRow(0);
                }
                else
                {
                    gridChoose.UnselectAll();
                    SetNomenclatureFromRow(-1);
                }
            }
            btnOK.Enabled = gridChoose.VisibleRowCount > 0 && NomenclatureId != Guid.Empty;
        }
        
        private void cmbFromPlaceZones_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!init)
            {
                var cmb = (sender as ComboBox).SelectedItem as PlaceZone;
                if (cmb.PlaceZoneId == Guid.Empty && Shared.Warehouses.Any(w => w.WarehouseId == cmb.PlaceId && w.WarehouseZones.Count > 0))
                {
                    GetFromPlaceZoneList(new EndPointInfo(cmb.PlaceId));
                }
                else
                    FromPlaceZoneChanged(cmb.PlaceId, cmb.PlaceZoneId);
                //FromPlaceZoneChanged((cmb.SelectedItem as PlaceZone).PlaceId, (cmb.SelectedItem as PlaceZone).PlaceZoneId);
            }
        }

        private void SetNomenclatureFromRow(int rowIndex)
        {
            {
                if (rowIndex >= 0 && ChooseNomenclatureList.Count > 0)
                {
                    var nomenclature = ChooseNomenclatureList[rowIndex];
                    if (!isFilteringOnNomenclature)
                    {
                        NomenclatureId = nomenclature.NomenclatureId;
                        CharacteristicId = nomenclature.CharacteristicId;
                        QualityId = nomenclature.QualityId;
                        ProductKindId = nomenclature.ProductKindId;
                    }
                    //GetMeasureUnitList(nomenclature.MeasureUnits);
                    quantityMeasureUnit.FillMeasureUnitList(nomenclature.MeasureUnits);
                }
                else
                {
                    NomenclatureId = Guid.Empty;
                    CharacteristicId = Guid.Empty;
                    QualityId = Guid.Empty;
                    ProductKindId = 0;
                    //GetMeasureUnitList(string.Empty);
                    quantityMeasureUnit.FillMeasureUnitList(string.Empty);
                }
            }
        }

        private void gridChoose_CurrentCellChanged(object sender, EventArgs e)
        {
            if (!init)
            {
                var currentRowIndex = (sender as DataGrid).CurrentRowIndex;
                if (currentRowIndex >= 0)
                    SetNomenclatureFromRow(currentRowIndex);
                
                //FromPlaceZoneId = (cmb.SelectedItem as PlaceZone).PlaceZoneId;
                //GetNomenclatureList();
                //RefreshGridRowHeight();
            }
        }

        //private void GetMeasureUnitList(string measureUnits)
        //{
        //    //measureUnits = "<Root><MeasureUnit>  <MeasureUnitID>9574BDF9-8AB4-11EA-9438-0015B2A9C22A</MeasureUnitID>  <Name>т</Name>  <Coefficient>1.000</Coefficient></MeasureUnit><MeasureUnit>	<MeasureUnitID>9574BDF9-8AB4-11EA-9438-0015B2A9C22A</MeasureUnitID>	<Name>т</Name>	<Coefficient>1.000</Coefficient></MeasureUnit></Root>";

        //    XmlSerializer serializer = new XmlSerializer(typeof(List<MeasureUnit>),
        //    new XmlRootAttribute("Root"));
        //    var list = measureUnits == String.Empty ? new List<MeasureUnit>() : (List<MeasureUnit>)serializer.Deserialize(new StringReader(measureUnits));

        //    //List<MeasureUnit> list = new List<MeasureUnit>();
        //    //var measureUnit = Shared.Warehouses.Find(w => w.WarehouseId == fromPlaceId);
        //    //if (place.WarehouseZones != null && place.WarehouseZones.Count > 0)
        //    //    list = Shared.PlaceZones.FindAll(z => z.PlaceId == fromPlaceId && z.IsValid);
        //    //else
        //    //    list.Add(new MeasureUnit() { Id = fromPlaceId, Name = "Передел" });
        //    ChooseMeasureUnitList = list;
        //    if (BSourceMeasureUnit == null)
        //        BSourceMeasureUnit = new BindingSource { DataSource = ChooseMeasureUnitList };
        //    else
        //        BSourceMeasureUnit.DataSource = ChooseMeasureUnitList;
        //    quantityMeasureUnit./*cmbMeasureUnits.*/DataSource = BSourceMeasureUnit;
        //    quantityMeasureUnit./*cmbMeasureUnits.*/DisplayMember = "Name";
        //    quantityMeasureUnit./*cmbMeasureUnits.*/ValueMember = "MeasureUnitID";
        //    //SelectedMeasureUnit = ChooseMeasureUnitList[0];//measureUnit == null ? list[0] : list.Find(z => z.PlaceZoneId == FromPlaceZoneId);
        //    //cmbMeasureUnits.SelectedItem = SelectedMeasureUnit;
        //    if (ChooseMeasureUnitList.Count > 0)
        //        quantityMeasureUnit./*cmbMeasureUnits.*/SelectedItem = ChooseMeasureUnitList[0];
        //    else
        //        quantityMeasureUnit./*cmbMeasureUnits.*/SelectedItem = null;
        //}

        //private void cmbMeasureUnits_SelectedValueChanged(object sender, EventArgs e)
        //{
        //    var cmb = sender as ComboBox;
        //    MeasureUnitId = (cmb.SelectedItem as MeasureUnit).MeasureUnitID;
        //}

        //private void btnUP_Click(object sender, EventArgs e)
        //{
        //    edtQuantity.Text = (Convert.ToInt32(edtQuantity.Text) + 1).ToString();
        //}

        //private void btnDown_Click(object sender, EventArgs e)
        //{
        //    edtQuantity.Text = (Convert.ToInt32(edtQuantity.Text) - 1).ToString();
        //}

    }
}