using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace Sample
{
    public partial class bookmark2 : Form
    {
        private IMap currentMap = null;
        Dictionary<String,ISpatialBookmark> pD = new Dictionary<string,ISpatialBookmark>();
        IMapBookmarks pMB = null;

        public bookmark2( IMap pM)
        {
            InitializeComponent();
            currentMap = pM;
            InitControl();
            

        }
        //初始化目录树
        private void InitControl() {
            pMB = currentMap as IMapBookmarks;
            IEnumSpatialBookmark pESB = pMB.Bookmarks;
            pESB.Reset();
            ISpatialBookmark pSB = pESB.Next();
            string name = string.Empty;
            if (pSB == null)
            {
                button1.Enabled = false;
            }
            else {
                button1.Enabled = true;

            }
            while (pSB != null) {
                name = pSB.Name;
                treeView1.Nodes.Add(name);
                pD.Add(name, pSB);
                pSB = pESB.Next();
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode select = treeView1.SelectedNode;
            ISpatialBookmark pSB = pD[select.Text];

            pSB.ZoomTo(currentMap);
            IActiveView pAV = currentMap as IActiveView;
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode select = treeView1.SelectedNode;
            ISpatialBookmark pSB = pD[select.Text];

            pMB.RemoveBookmark(pSB);
            pD.Remove(select.Text);
            treeView1.Nodes.Remove(select);
            treeView1.Refresh();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }
}
