using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using gamma_mob.Models;
using System.Xml.Serialization;
using System.IO;

namespace gamma_mob.Common
{
    public class QuantityMeasureUnit : Panel
    {
        private System.Windows.Forms.Panel panelQuantity;
        private System.Windows.Forms.Panel panelEmptyTop;
        private System.Windows.Forms.Panel panelEmptyBottom;
        private System.Windows.Forms.Panel panelMeasureUnit;
        private NumericUpDownWithButtons numericUpDownWithButtons;
        private System.Windows.Forms.Label lblMeasureUnit;
        public System.Windows.Forms.ComboBox cmbMeasureUnits;
        
        private System.Drawing.Font _font {get; set; }
        public override System.Drawing.Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                numericUpDownWithButtons.Font = value;
                cmbMeasureUnits.Font = value;
            }
        }
        
        public int Value
        { get { return numericUpDownWithButtons.Value; } }

        public int ValueFractional
        { get { return numericUpDownWithButtons.ValueFractional; } }

        public decimal ValueInBaseMeasureUnit
        { get { return numericUpDownWithButtons.Value * MeasureUnit.Coefficient; } }
        
        public MeasureUnit MeasureUnit { get; private set; }
        //public decimal Coefficient { get; private set; }

        public object DataSource { 
            get { return cmbMeasureUnits.DataSource; }
            set { cmbMeasureUnits.DataSource = value; }
        }
        public string DisplayMember {
            get { return cmbMeasureUnits.DisplayMember; }
            set { cmbMeasureUnits.DisplayMember = value; }
        }
        public int SelectedIndex {
            get { return cmbMeasureUnits.SelectedIndex; }
            set { cmbMeasureUnits.SelectedIndex = value; } 
        }
        public object SelectedValue {
            get { return cmbMeasureUnits.SelectedValue; }
            set { cmbMeasureUnits.SelectedValue = value; }
        }
        public string ValueMember {
            get { return cmbMeasureUnits.ValueMember; }
            set { cmbMeasureUnits.ValueMember = value; }
        }

        public object SelectedItem {
            get { return cmbMeasureUnits.SelectedItem; }
            set 
            { 
                cmbMeasureUnits.SelectedItem = value;
                if (value == null)
                    MeasureUnit = null;
            }
        }

        public int MinEditWidth
        {
            get { return numericUpDownWithButtons.MinEditWidth; }
            set { numericUpDownWithButtons.MinEditWidth = value; }
        }

        public int MaxEditWidth 
        {
            get { return numericUpDownWithButtons.MaxEditWidth; }
            set { numericUpDownWithButtons.MaxEditWidth = value; }
        }

        private MeasureUnit _defaultMeasureUnit { get; set; }
        public MeasureUnit DefaultMeasureUnit
        {
            get { return _defaultMeasureUnit; }
            private set { _defaultMeasureUnit = value; }
        }

        public QuantityMeasureUnit()
        {
            this.panelQuantity = new System.Windows.Forms.Panel();
            this.panelEmptyTop = new System.Windows.Forms.Panel();
            this.panelEmptyBottom = new System.Windows.Forms.Panel();
            this.panelMeasureUnit = new System.Windows.Forms.Panel();
            this.numericUpDownWithButtons = new gamma_mob.Common.NumericUpDownWithButtons();
            this.lblMeasureUnit = new System.Windows.Forms.Label();
            this.cmbMeasureUnits = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            this.Controls.Add(this.panelQuantity);
            //this.Controls.Add(this.panelEmpty);
            this.Controls.Add(this.panelMeasureUnit);
            this.Location = new System.Drawing.Point(1, 2);
            this.Name = "pnlQuantityMeasureUnit";
            this.Size = new System.Drawing.Size(160, 62);
            // 
            // panelQuantity
            // 
            this.panelQuantity.Controls.Add(this.numericUpDownWithButtons);
            this.panelQuantity.Controls.Add(this.panelEmptyTop);
            this.panelQuantity.Controls.Add(this.panelEmptyBottom);
            this.panelQuantity.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelQuantity.Location = new System.Drawing.Point(0, 0);
            this.panelQuantity.Name = "panelQuantity";
            this.panelQuantity.Size = new System.Drawing.Size(150, 29);
            this.panelQuantity.SendToBack();
            // 
            // numericUpDownWithButtons
            // 
            this.numericUpDownWithButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownWithButtons.Location = new System.Drawing.Point(0, 0);
            this.numericUpDownWithButtons.Name = "numericUpDownWithButtons";
            this.numericUpDownWithButtons.Size = new System.Drawing.Size(145, 27);
            // 
            // panelEmptyTop
            // 
            this.panelEmptyTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelEmptyTop.Location = new System.Drawing.Point(0, 30);
            this.panelEmptyTop.Name = "panelEmptyTop";
            this.panelEmptyTop.Size = new System.Drawing.Size(150, 2);
            //this.panelEmpty.SendToBack();
            // 
            // panelEmpty
            // 
            this.panelEmptyBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEmptyBottom.Location = new System.Drawing.Point(0, 30);
            this.panelEmptyBottom.Name = "panelEmptyBottom";
            this.panelEmptyBottom.Size = new System.Drawing.Size(150, 2);
            // 
            // panelMeasureUnit
            // 
            this.panelMeasureUnit.Controls.Add(this.lblMeasureUnit);
            this.panelMeasureUnit.Controls.Add(this.cmbMeasureUnits);
            this.panelMeasureUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMeasureUnit.Location = new System.Drawing.Point(0, 35);
            this.panelMeasureUnit.Name = "panelMeasureUnit";
            this.panelMeasureUnit.Size = new System.Drawing.Size(150, 29);
            //this.panel1.SendToBack();
            // 
            // lblMeasureUnit
            // 
            this.lblMeasureUnit.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblMeasureUnit.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
            this.lblMeasureUnit.Location = new System.Drawing.Point(0, 0);
            this.lblMeasureUnit.Name = "lblMeasureUnit";
            this.lblMeasureUnit.Size = new System.Drawing.Size(42, 29);
            this.lblMeasureUnit.Text = "Ед.изм";
            this.lblMeasureUnit.SendToBack();
            // 
            // cmbMeasureUnits
            // 
            this.cmbMeasureUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbMeasureUnits.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.cmbMeasureUnits.Location = new System.Drawing.Point(0, 35);
            this.cmbMeasureUnits.Name = "cmbMeasureUnits";
            this.cmbMeasureUnits.Size = new System.Drawing.Size(80, 29);
            this.cmbMeasureUnits.TabIndex = 1;
            this.cmbMeasureUnits.SelectedValueChanged += new System.EventHandler(this.cmbMeasureUnits_SelectedValueChanged);
            MinEditWidth = 50;
            MaxEditWidth = 80;
            
            this.ResumeLayout(false);
            ChooseMeasureUnitList = new List<MeasureUnit>();
        }
        
        public void SetMeasureQuantityLocked(MeasureUnit measureUnit, int quantity, int? quantityFeactional)
        {
            List<MeasureUnit> list = new List<MeasureUnit>();
            list.Add(measureUnit);
            FillMeasureUnitList(list);
            numericUpDownWithButtons.IsInteger = measureUnit.IsInteger;
            numericUpDownWithButtons.Value = quantity;
            numericUpDownWithButtons.ValueFractional = quantityFeactional ?? 0;
        }
        
        public void SetMeasureQuantityDefaultMeasure(Guid measureUnitId, int quantity, int? quantityFeactional)
        {
            var measure = ChooseMeasureUnitList.Find(l => l.MeasureUnitID == measureUnitId);
            if (measure == null)
            {
                measure = new MeasureUnit() { MeasureUnitID = measureUnitId, Name = "Test", Numerator = 1, Denominator = 1, IsInteger = false };
                ChooseMeasureUnitList.Add(measure);
            }
            DefaultMeasureUnit = measure;
            var i = ChooseMeasureUnitList.FindIndex(l => l.MeasureUnitID == measureUnitId);
            if (cmbMeasureUnits.Items.Count > 0)
                cmbMeasureUnits.SelectedIndex = i;
            numericUpDownWithButtons.IsInteger = measure.IsInteger;
            numericUpDownWithButtons.Value = quantity;
            numericUpDownWithButtons.ValueFractional = quantityFeactional ?? 0;
        }

        private void cmbMeasureUnits_SelectedValueChanged(object sender, EventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb.SelectedItem != null)
            {
                MeasureUnit = (cmb.SelectedItem as MeasureUnit);
                DefaultMeasureUnit = MeasureUnit;
                //Coefficient = (cmb.SelectedItem as MeasureUnit).Coefficient;
            }
        }

        private BindingSource BSourceMeasureUnit { get; set; }
        private List<MeasureUnit> ChooseMeasureUnitList { get; set; }

        public void FillMeasureUnitList(string measureUnits)
        {
            //measureUnits = "<Root><MeasureUnit>  <MeasureUnitID>9574BDF9-8AB4-11EA-9438-0015B2A9C22A</MeasureUnitID>  <Name>т</Name>  <Coefficient>1.000</Coefficient></MeasureUnit><MeasureUnit>	<MeasureUnitID>9574BDF9-8AB4-11EA-9438-0015B2A9C22A</MeasureUnitID>	<Name>т</Name>	<Coefficient>1.000</Coefficient></MeasureUnit></Root>";

            XmlSerializer serializer = new XmlSerializer(typeof(List<MeasureUnit>),
            new XmlRootAttribute("Root"));
            var list = measureUnits == String.Empty ? new List<MeasureUnit>() : (List<MeasureUnit>)serializer.Deserialize(new StringReader(measureUnits));
            FillMeasureUnitList(list);
        }

        public void FillMeasureUnitList(List<MeasureUnit> list)
        {
            ChooseMeasureUnitList = list;
            if (BSourceMeasureUnit == null)
                BSourceMeasureUnit = new BindingSource { DataSource = ChooseMeasureUnitList };
            else
                BSourceMeasureUnit.DataSource = ChooseMeasureUnitList;
            cmbMeasureUnits.DataSource = BSourceMeasureUnit;
            cmbMeasureUnits.DisplayMember = "Name";
            cmbMeasureUnits.ValueMember = "MeasureUnitID";
            //SelectedMeasureUnit = ChooseMeasureUnitList[0];//measureUnit == null ? list[0] : list.Find(z => z.PlaceZoneId == FromPlaceZoneId);
            //cmbMeasureUnits.SelectedItem = SelectedMeasureUnit;
            if (ChooseMeasureUnitList.Count > 0)
            {
                if (DefaultMeasureUnit == null)
                    cmbMeasureUnits.SelectedIndex = 0;
                else
                {
                    cmbMeasureUnits.SelectedIndex = ChooseMeasureUnitList.FindIndex(l => l.MeasureUnitID == DefaultMeasureUnit.MeasureUnitID);
                    //cmbMeasureUnits.SelectedItem = DefaultMeasureUnit;
                }
            }
            else
                cmbMeasureUnits.SelectedItem = null;
        }
    }
}
