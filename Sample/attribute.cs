using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Sample
{
    public partial class attribute : Form
    {
        public attribute()
        {
            InitializeComponent();
        }

        private IFeatureLayer _current;
        public IFeatureLayer current {

            get { return _current; }
            set { _current = value;}
        }

        public void init(){
            if (_current == null) return;
            IFeature feature = null;
            DataTable table = new DataTable();
            DataRow row = null;
            DataColumn caol = null;
            IField field = null;
            for (int i =0;i<_current.FeatureClass.Fields.FieldCount;i++){
                caol = new DataColumn();
                field = _current.FeatureClass.Fields.get_Field(i);
                caol.ColumnName = field.AliasName;
                caol.DataType = Type.GetType("System.Object");
                table.Columns.Add(caol);
            }
            IFeatureCursor cursor = _current.Search(null,true);
            feature = cursor.NextFeature();

            while(feature != null){
            
                row = table.NewRow();
                for (int j=0;j<table.Columns.Count;j++){
                    row[j] = feature.get_Value(j);
                }
                table.Rows.Add(row);
                feature = cursor.NextFeature();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);         
            dataGridView1.DataSource = table;
            
    }

    }
}
