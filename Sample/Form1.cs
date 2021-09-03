using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using System.IO;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.esriSystem;


namespace Sample
{
    public partial class Form1 : Form
    {
        #region 变量定义
        private measure frmMeasureResult = null;             //量算结果窗体
        private INewLineFeedback pNewLineFeedback;           //追踪线对象
        private INewPolygonFeedback pNewPolygonFeedback;     //追踪面对象
        private IPoint pPointPt = null;                      //鼠标点击点
        private IPoint pMovePt = null;                       //鼠标移动时的当前点
        private double dToltalLength = 0;                    //量测总长度
        private double dSegmentLength = 0;                   //片段距离
        private IPointCollection pAreaPointCol = new MultipointClass();  //面积量算时画的点进行存储； 

        private string sMapUnits = "未知单位";             //地图单位变量
        private object missing = Type.Missing;

        # endregion

        # region 另存为
        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //封装好的方法
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(axMapControl1.Object);
            command.OnClick();
        }
        #endregion
        public Form1()
        {
            InitializeComponent();
            axTOCControl1.SetBuddyControl(axMapControl1);

            axMapControl2.Extent = axMapControl1.FullExtent;

        }
        # region 数据加载
        private void 加载mxdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog p0 = new OpenFileDialog();
            p0.CheckFileExists = true;
            p0.Title = "开打";
            p0.Filter = "ArcMap文档(*.mxd)|*.mxd;|ArcMap模板(*.mxt)|*.mxt;|发布地图文件(*.pmf)|*.pmf;|所有地图格式(*.mxd;*.mxt;*.pmf)|*.mxd;*.mxt;*.pmf";
            p0.Multiselect = false;
            p0.RestoreDirectory = true;

            if (p0.ShowDialog() == DialogResult.OK)
            {

                string name = p0.FileName;
                if (name == "")
                {

                    return;
                }
                if (axMapControl1.CheckMxFile(name)) //  直接加载filename
                {

                    axMapControl1.LoadMxFile(name);
                }
                else
                {
                    MessageBox.Show(name);
                    return;
                }

            }
        }

        private void 加载shpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
             OpenFileDialog p0 = new OpenFileDialog();
             p0.CheckFileExists = true;
             p0.Title = "打开shp文件";
             p0.Filter = "Shapefile文件(*.shp)|*.shp;";
             p0.Multiselect = false;
             p0.RestoreDirectory = true;
             p0.ShowDialog();

             IWorkspaceFactory wsf;
             IFeatureWorkspace fws;
             IFeatureLayer fl;
             string filename = p0.FileName;

             if (filename == "") return;

             int pIndex = filename.LastIndexOf("\\");
             string path = filename.Substring(0, pIndex);
             string name = filename.Substring(pIndex + 1);

             wsf = new ShapefileWorkspaceFactory();
             fws = (IFeatureWorkspace)wsf.OpenFromFile(path, 0);

             IFeatureClass fclass = fws.OpenFeatureClass(name);
             fl = new FeatureLayer();
             fl.FeatureClass = fclass;
             fl.Name = fl.FeatureClass.AliasName;

             axMapControl1.Map.AddLayer(fl);
             axMapControl1.ActiveView.Refresh();
             */

            IWorkspaceFactory pWF = new ShapefileWorkspaceFactory();
            OpenFileDialog dia = new OpenFileDialog();

            dia.Title = "打开SHP文件";
            try
            {
                if (dia.ShowDialog() == DialogResult.OK)
                {


                    string path = System.IO.Path.GetDirectoryName(dia.FileName);
                    string name = System.IO.Path.GetFileName(dia.FileName);


                    /*IWorkspace pw = pWF.OpenFromFile(path, 0);
                    IFeatureClass pFC = (pw as IFeatureWorkspace).OpenFeatureClass(name);
                    IFeatureLayer pFL = new FeatureLayer();
                    pFL.FeatureClass = pFC;
                    pFL.Name = pFL.FeatureClass.AliasName;
                    axMapControl1.AddLayer(pFL);
                    axMapControl1.ActiveView.Refresh();*/

                    axMapControl1.AddShapeFile(path, name);
                }
            }
            catch
            {
                MessageBox.Show("SHP文件选择错误！");
            }



        }

        private void 加载栅格文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RasterWorkspaceFactory raster = new RasterWorkspaceFactory();
            OpenFileDialog pd = new OpenFileDialog();
            pd.Title = "打开栅格文件";
            pd.Filter = "TIF文件 *.tif|*.tif;|所有文件*.*|*.*";

            if (pd.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    string path = System.IO.Path.GetDirectoryName(pd.FileName);
                    string name = System.IO.Path.GetFileName(pd.FileName);
                    IRasterWorkspace pRW = (IRasterWorkspace)raster.OpenFromFile(path, 0); // 创建工作空间
                    RasterDataset pRD = (RasterDataset)pRW.OpenRasterDataset(name);
                    IRasterLayer pRL = new RasterLayer();

                    //栅格金字塔判断
                    IRasterPyramid3 pRP3 = (IRasterPyramid3)pRD;
                    if (pRP3 != null)
                    {
                        if (!pRP3.Present)
                        {
                            pRP3.Create();
                        }
                    }

                    
                    pRL.CreateFromDataset(pRD);
                    axMapControl1.AddLayer(pRL);
                    axMapControl1.ActiveView.Refresh();

                }
                catch
                {
                    MessageBox.Show("文件错误");
                }
            }

        }

        private void 分层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog pd = new OpenFileDialog();
            pd.Filter = "CAD文件 *.dwg|*.dwg";
            pd.Title = "打开cad文件";

            IWorkspaceFactory pWF = new CadWorkspaceFactory();
            if (pd.ShowDialog() == DialogResult.OK)
            {

                //分图层CAD显示
                string path = System.IO.Path.GetDirectoryName(pd.FileName);
                string name = System.IO.Path.GetFileName(pd.FileName);
                IFeatureWorkspace pFW = pWF.OpenFromFile(path, 0) as IFeatureWorkspace;
                //多边形
                /*
                string polygonname = name + ":Polygon";
                IFeatureClass fc1 = pFW.OpenFeatureClass(polygonname);
                FeatureLayer fl1 = new FeatureLayer();
                fl1.FeatureClass = fc1;
                fl1.Name = fl1.FeatureClass.AliasName;
                axMapControl1.AddLayer(fl1);

                string polylinename = name + ":Polyline";
                IFeatureClass fc2 = pFW.OpenFeatureClass(polylinename);
                FeatureLayer fl2 = new FeatureLayer();
                fl2.FeatureClass = fc2;
                fl2.Name = fl2.FeatureClass.AliasName;
                axMapControl1.AddLayer(fl2);

                string pointname = name + ":Point";
                IFeatureClass fc3 = pFW.OpenFeatureClass(pointname);
                FeatureLayer fl3 = new FeatureLayer();
                fl3.FeatureClass = fc3;
                fl3.Name = fl2.FeatureClass.AliasName;
                axMapControl1.AddLayer(fl3);*/

                string[] names = { ":Polyline", ":Point", ":Point" };
                foreach (string i in names) {
                    IFeatureClass fc1= pFW.OpenFeatureClass(name+i);
                    FeatureLayer fl1 = new FeatureLayer();
                    fl1.FeatureClass = fc1;
                    fl1.Name = fl1.FeatureClass.AliasName;
                    axMapControl1.AddLayer(fl1);
                    
                }



            }
            axMapControl1.ActiveView.Refresh();
        }

        private void 整层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog pd = new OpenFileDialog();
            pd.Filter = "CAD文件 *.dwg|*.dwg";
            pd.Title = "打开cad文件";
            IWorkspaceFactory pWF = new CadWorkspaceFactory();
            if (pd.ShowDialog() == DialogResult.OK)
            {
                if (pd.FileName == "") return;
                //分图层CAD显示
                string path = System.IO.Path.GetDirectoryName(pd.FileName);
                string name = System.IO.Path.GetFileName(pd.FileName);

                IFeatureWorkspace pFW = pWF.OpenFromFile(path, 0) as IFeatureWorkspace;
                IFeatureDataset pFD = pFW.OpenFeatureDataset(name);
                IFeatureClassContainer pFCC = pFD as IFeatureClassContainer; //用container管理dataset中每个要素类

                //遍历cad数据类
                for (int i = 0; i < pFCC.ClassCount; i++)
                {
                    IFeatureClass pFC = pFCC.get_Class(i);

                    if (pFC.FeatureType == esriFeatureType.esriFTCoverageAnnotation)//判断是否为注记层
                    {
                        IFeatureLayer pFL = new CadAnnotationLayerClass();
                        pFL.Name = pFC.AliasName;
                        pFL.FeatureClass = pFC;
                        axMapControl1.AddLayer(pFL);
                    }
                    else
                    {
                        IFeatureLayer pFL = new FeatureLayerClass();
                        pFL.Name = pFC.AliasName;
                        pFL.FeatureClass = pFC;
                        axMapControl1.AddLayer(pFL);
                    }
                }


            }
            axMapControl1.ActiveView.Refresh();
        }

        private void 栅格ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory pWF = new CadWorkspaceFactoryClass();
            OpenFileDialog dia = new OpenFileDialog();
            dia.Filter = "CAD文件 *.dwg|*.dwg";
            dia.Title = "打开cad文件";
            if (dia.ShowDialog() == DialogResult.OK)
            {
                string path = System.IO.Path.GetDirectoryName(dia.FileName);
                string name = System.IO.Path.GetFileName(dia.FileName);

                IWorkspace pW = pWF.OpenFromFile(path, 0);
                ICadDrawingWorkspace pCDW = (ICadDrawingWorkspace)pW;

                //获取数据集
                ICadDrawingDataset pCDD = pCDW.OpenCadDrawingDataset(name);
                ICadLayer pCL = new CadLayerClass();
                pCL.CadDrawingDataset = pCDD;
                pCL.Name = name;
                axMapControl1.AddLayer(pCL);
                axMapControl1.ActiveView.Refresh();
            }
        }

        private void mdbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dia = new OpenFileDialog();
            dia.Filter = "个人数据库*.mdb|*.mdb";
            if (dia.ShowDialog() == DialogResult.OK)
            {
                AccessWorkspaceFactory pAW = new AccessWorkspaceFactory();
                IWorkspace pW = pAW.OpenFromFile(dia.FileName, 0);
                Sample.tool.AddAllDataset(pW, axMapControl1);


            }
        }

        private void gdbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dia = new FolderBrowserDialog();
            if (dia.ShowDialog() == DialogResult.OK)
            {
                FileGDBWorkspaceFactory pGDB = new FileGDBWorkspaceFactoryClass();
                IWorkspace pW = pGDB.OpenFromFile(dia.SelectedPath, 0);
                Sample.tool.AddAllDataset(pW, axMapControl1);



            }

        }

        private void aDDXYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dia = new OpenFileDialog();
            dia.Filter = "文本文件 *.txt|*.txt";
            if (dia.ShowDialog() == DialogResult.OK) {
                addxy add = new addxy();
                add.show(axMapControl1,dia.FileName);

            }
            
        }
        # endregion

        # region 保存
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string mxdfilename = axMapControl1.DocumentFilename;
                IMapDocument pMD = new MapDocumentClass();
                if (mxdfilename != null && axMapControl1.CheckMxFile(mxdfilename))
                {
                    if (pMD.get_IsReadOnly(mxdfilename))
                    {
                        MessageBox.Show("文件只读");
                        pMD.Close();
                        return;
                    }
                }
                else
                {

                    SaveFileDialog dia = new SaveFileDialog();
                    dia.OverwritePrompt = true;
                    if (dia.ShowDialog() == DialogResult.OK)
                    {
                        mxdfilename = dia.FileName;
                    }
                    else return;
                }

                pMD.New(mxdfilename);
                pMD.ReplaceContents(axMapControl1.Map as IMxdContents);
                pMD.Save(pMD.UsesRelativePaths, true);
                pMD.Close();
                MessageBox.Show("保存成功");
            }
            catch {
                MessageBox.Show("出错");
            }

        }
        #endregion


        #region 放大
        private void 选择放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnvelope pE = axMapControl1.Extent;
            pE.Expand(0.5, 0.5, true);
            axMapControl1.Extent = pE;
            axMapControl1.ActiveView.Refresh();
        }
        # endregion

        # region 缩小
        private void 选择缩小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnvelope pE = axMapControl1.Extent;
            pE.Expand(2, 2, true);
            axMapControl1.Extent = pE;
            axMapControl1.ActiveView.Refresh();


        }
        # endregion


        #region 拉框放大
        string pMouseOperate = null;
        private void 拉框放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "ZoomIn";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomIn;

            

        }
        # endregion

        #region 拉框缩小
        private void 拉框缩小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "ZoomOut";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomOut;

        }
        # endregion

        

        #region mousedown
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            pPointPt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
            if (e.button == 1) {
                IActiveView pAV = axMapControl1.ActiveView;
                IEnvelope pE = new EnvelopeClass();

                switch(pMouseOperate){

                    case "ZoomIn":
                        pE = axMapControl1.TrackRectangle();
                        if (pE==null || pE.IsEmpty || pE.Width==0||pE.Height==0) return;
                        pAV.Extent = pE;
                        pAV.Refresh();
                        break;

                    case "ZoomOut":
                        pE = axMapControl1.TrackRectangle();
                        if (pE==null || pE.IsEmpty || pE.Width==0||pE.Height==0) return;
                        double dWidth = pAV.Extent.Width * pAV.Extent.Width / pE.Width;
                        double dHeight = pAV.Extent.Height * pAV.Extent.Height / pE.Height;
                        double dXmin = pAV.Extent.XMin -
                                        ((pE.XMin - pAV.Extent.XMin) * pAV.Extent.Width /
                                        pE.Width);
                        double dYmin = pAV.Extent.YMin -
                                        ((pE.YMin - pAV.Extent.YMin) * pAV.Extent.Height /
                                        pE.Height);
                        double dXmax = dXmin + dWidth;
                        double dYmax = dYmin + dHeight;
                        pE.PutCoords(dXmin, dYmin, dXmax, dYmax);

                        pAV.Extent = pE;
                        pAV.Refresh();
                        break;

                    case "Pan":
                        axMapControl1.Pan();
                        break;

                    case "MeasureLength":
                        if (pNewLineFeedback == null)
                        {
                            pNewLineFeedback = new NewLineFeedbackClass();
                            pNewLineFeedback.Display = (axMapControl1.Map as IActiveView).ScreenDisplay;

                            pNewLineFeedback.Start(pPointPt);
                            dToltalLength = 0;
                        }
                        else {
                            pNewLineFeedback.AddPoint(pPointPt);
                        }
                        if (dSegmentLength != 0) {
                            dToltalLength += dSegmentLength;
                        }
                        break;

                    case "MeasureArea":
                        if (pNewPolygonFeedback == null)
                        {
                            //实例化追踪面对象
                            pNewPolygonFeedback = new NewPolygonFeedback();
                            pNewPolygonFeedback.Display = (axMapControl1.Map as IActiveView).ScreenDisplay;
                            pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount);
                            //开始绘制多边形
                            pNewPolygonFeedback.Start(pPointPt);
                            pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);
                        }
                        else
                        {
                            pNewPolygonFeedback.AddPoint(pPointPt);
                            pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);
                        }
                        break;    
                    default:
                        break;

                }
            }
            else if (e.button == 2) {
                pMouseOperate = "";
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;

            }
        }
        #endregion

        # region 视图显示
        private void 漫游ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "Pan";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPan;
        }

        private void 全图显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.Extent = axMapControl1.FullExtent;

        }

        private void 前视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IExtentStack pES = axMapControl1.ActiveView.ExtentStack;
            if (pES.CanUndo()) {
                pES.Undo();
                前视图ToolStripMenuItem.Enabled = true;
                if (!pES.CanUndo()) {
                    前视图ToolStripMenuItem.Enabled = false;
                }
            }
            axMapControl1.Refresh();
        }

        private void 后视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IExtentStack pES = axMapControl1.ActiveView.ExtentStack;
            if (pES.CanRedo())
            {
                pES.Redo();
                后视图ToolStripMenuItem.Enabled = true;
                if (!pES.CanRedo())
                {
                    后视图ToolStripMenuItem.Enabled = false;
                }
            }
            axMapControl1.Refresh();
        }

        # endregion

        #region 书签
        private void 添加书签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bookmark1 bookmark = new bookmark1();
            bookmark.ShowDialog();
            IActiveView pAV = axMapControl1.Map as IActiveView;
            IAOIBookmark pAOI = new AOIBookmarkClass();
            pAOI.Location = pAV.Extent;
            string name = string.Empty;
            Boolean check = bookmark.Check;
            if (check) {
                name = bookmark.Bookmark;
            }
            if (string.IsNullOrEmpty(name)) return;

            IMapBookmarks pMB = axMapControl1.Map as IMapBookmarks;
            IEnumSpatialBookmark pESB = pMB.Bookmarks;
            pESB.Reset();
            ISpatialBookmark pSB = pESB.Next();
            while (pSB != null) {
                if (name == pSB.Name) {
                    DialogResult result = MessageBox.Show("书签名字重复，将覆盖");
                    if (result == DialogResult.OK) {
                        pMB.RemoveBookmark(pSB);
                    }
                    else if (result == DialogResult.No)
                    {
                        bookmark.ShowDialog();
                    }
                    else {
                        return;
                    }
                }
                pSB = pESB.Next();
            }
            pAOI.Name = name;
            pMB.AddBookmark(pAOI);


        }

        private void 管理书签ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            bookmark2 bookmark2 = new bookmark2(axMapControl1.Map);
            bookmark2.ShowDialog();

        }

        # endregion



        # region 量测


        //关闭的响应事件
        private void frmMeasureResult_frmColsed()
        { 
            //清空线
            if (pNewLineFeedback != null) {
                pNewLineFeedback.Stop();
                pNewLineFeedback = null;
            }

            if (pNewPolygonFeedback != null) {
                pNewPolygonFeedback.Stop();
                pNewPolygonFeedback = null;
                pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount);
            }

            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null); //清空量算

            pMouseOperate = string.Empty;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }

        private void 长度ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "MeasureLength";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (frmMeasureResult == null || frmMeasureResult.IsDisposed)
            {
                frmMeasureResult = new measure();
                frmMeasureResult.frmClosed += new measure.FormClosedEventHandler(frmMeasureResult_frmColsed);
                frmMeasureResult.label1.Text = "";
                frmMeasureResult.Text = "距离量测";
                frmMeasureResult.Show();
            }
            else {
                frmMeasureResult.Activate();
            }
        }

        private void 面积ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "MeasureArea";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (frmMeasureResult == null || frmMeasureResult.IsDisposed)
            {
                frmMeasureResult = new measure();
                frmMeasureResult.frmClosed += new measure.FormClosedEventHandler(frmMeasureResult_frmColsed);
                frmMeasureResult.label1.Text = "";
                frmMeasureResult.Text = "面积量测";
                frmMeasureResult.Show();
            }
            else
            {
                frmMeasureResult.Activate();
            }
        }
        # endregion

        # region 要素选择
        private void 选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            ControlsSelectFeaturesTool select = new ControlsSelectFeaturesToolClass();
            select.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = select as ITool;
        }




        private void 缩放指选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            ControlsZoomToSelectedCommand select = new ControlsZoomToSelectedCommandClass();
            select.OnCreate(axMapControl1.Object);
            select.OnClick();
        }

        private void 清除选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            ControlsClearSelectionCommand clean = new ControlsClearSelectionCommandClass();
            clean.OnCreate(axMapControl1.Object);
            clean.OnClick();
        }
        # endregion


        # region tool

        private string GetMapUnit(esriUnits _esriMapUnit)
        {
            sMapUnits = string.Empty;
            switch (_esriMapUnit)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "厘米";
                    break;
                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "十进制";
                    break;
                case esriUnits.esriDecimeters:
                    sMapUnits = "分米";
                    break;
                case esriUnits.esriFeet:
                    sMapUnits = "尺";
                    break;
                case esriUnits.esriInches:
                    sMapUnits = "英寸";
                    break;
                case esriUnits.esriKilometers:
                    sMapUnits = "千米";
                    break;
                case esriUnits.esriMeters:
                    sMapUnits = "米";
                    break;
                case esriUnits.esriMiles:
                    sMapUnits = "英里";
                    break;
                case esriUnits.esriMillimeters:
                    sMapUnits = "毫米";
                    break;
                case esriUnits.esriNauticalMiles:
                    sMapUnits = "海里";
                    break;
                case esriUnits.esriPoints:
                    sMapUnits = "点";
                    break;
                case esriUnits.esriUnitsLast:
                    sMapUnits = "UnitsLast";
                    break;
                case esriUnits.esriUnknownUnits:
                    sMapUnits = "未知单位";
                    break;
                case esriUnits.esriYards:
                    sMapUnits = "码";
                    break;
                default:
                    break;
            }
            return sMapUnits;
        }

        private void DrawRectangle(IEnvelope pEn)
        {
            //消除先前存在矩形框
            IGraphicsContainer pGC = axMapControl2.Map as IGraphicsContainer;
            IActiveView pAV = pGC as IActiveView;
            pGC.DeleteAllElements();

            //当前视图
            IRectangleElement pRect = new RectangleElementClass();
            IElement pE = pRect as IElement;
            pE.Geometry = pEn;

            //设置矩形框
            IRgbColor color = new RgbColorClass();
            color = GetRgbColor(255, 0, 0);
            color.Transparency = 255;

            ILineSymbol line = new SimpleLineSymbolClass();
            line.Width = 2;
            line.Color = color;

            IFillSymbol fill = new SimpleFillSymbolClass();
            color = new RgbColorClass();
            color.Transparency = 0;
            fill.Color = color;
            fill.Outline = line;

            //添加矩形框
            IFillShapeElement pFSE = pE as IFillShapeElement;
            pFSE.Symbol = fill;
            pGC.AddElement(pFSE as IElement, 0);
            pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);


        }

        private IRgbColor GetRgbColor(int intR, int intG, int intB)
        {
            IRgbColor pRgbColor = null;
            if (intR < 0 || intR > 255 || intG < 0 || intG > 255 || intB < 0 || intB > 255)
            {
                return pRgbColor;
            }
            pRgbColor = new RgbColorClass();
            pRgbColor.Red = intR;
            pRgbColor.Green = intG;
            pRgbColor.Blue = intB;
            return pRgbColor;
        }

        private void sychronizeEaleEye()
        {
            if (axMapControl2.LayerCount >= 0)
            {
                axMapControl2.ClearLayers();
            }
            IMap map = axMapControl1.Map;
            for (int i = 0; i < map.LayerCount; i++) {
                axMapControl2.Map.AddLayer(map.get_Layer(i));
            }
            axMapControl1.Extent = axMapControl2.FullExtent;
            axMapControl2.Refresh();
        }

        # endregion

        # region 事件


        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            sMapUnits = GetMapUnit(axMapControl1.Map.MapUnits);
            toolStripStatusLabel1.Text = String.Format("当前坐标：X = {0:#.###} Y = {1:#.###} {2}", e.mapX, e.mapY, sMapUnits);
            pMovePt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);//转换为地理坐标
            
            if (pMouseOperate == "MeasureLength") {
                if (pNewLineFeedback != null) pNewLineFeedback.MoveTo(pMovePt);
                double deltax = 0;
                double deltay = 0;
                if ((pPointPt!=null) && (pNewLineFeedback != null)){
                    deltax = pMovePt.X - pPointPt.X;
                    deltay = pMovePt.Y - pPointPt.Y;
                    dSegmentLength = Math.Round(Math.Sqrt((deltax * deltax) + (deltay * deltay)));
                    dToltalLength += dSegmentLength;
                    if (frmMeasureResult != null) {
                        frmMeasureResult.label1.Text = String.Format("当前线段长度：{0:.###}{1};\r\n总长度为: {2:.###}{1}", dSegmentLength, sMapUnits, dToltalLength);
                        dToltalLength -= dSegmentLength;
                    }
                    frmMeasureResult.frmClosed += new measure.FormClosedEventHandler(frmMeasureResult_frmColsed);
                }
            }

            if (pMouseOperate == "MeasureArea")
            {
                if (pNewPolygonFeedback != null) pNewPolygonFeedback.MoveTo(pPointPt);
                IPointCollection pPointsCol = new Polygon();
                IPolygon polygon = new PolygonClass();
                IGeometry pGeo = null;
                ITopologicalOperator PTopo = null;
                for (int i = 0; i <= pAreaPointCol.PointCount-1; i++)
                {
                    pPointsCol.AddPoint(pAreaPointCol.get_Point(i), ref missing, ref missing);
                }
                pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);
                if (pPointsCol.PointCount < 3) return;
                polygon = pPointsCol as IPolygon;

                if (polygon != null) {
                    polygon.Close();
                    pGeo = polygon as IGeometry;
                    PTopo = pGeo as ITopologicalOperator;

                    PTopo.Simplify();
                    pGeo.Project(axMapControl1.Map.SpatialReference);
                    IArea area = pGeo as IArea;
                    frmMeasureResult.label1.Text = String.Format(
                        "总面积为：{0:.####}平方{1};\r\n总长度为：{2:.####}{1}",
                        area.Area, sMapUnits, polygon.Length);
                    polygon = null;
                }

            }
        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            if (pMouseOperate == "MeasureLength")
            {
                if (frmMeasureResult != null) {
                    frmMeasureResult.label1.Text = "线段总长度为：" + dToltalLength + sMapUnits;

                }
                if (pNewLineFeedback != null) {
                    pNewLineFeedback.Stop();
                    pNewLineFeedback = null;

                }
                (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                dToltalLength = 0;
                dSegmentLength = 0;
            }

            if (pMouseOperate == "MeasureArea")
            {
                if (pNewPolygonFeedback != null)
                {
                    pNewPolygonFeedback.Stop();
                    pNewPolygonFeedback = null;
                    //清空所画的线对象
                    (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                }
                pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount); //清空点集中所有点
            }
            
        }

        # region 鹰眼
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            sychronizeEaleEye();
            
        }
        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            IEnvelope pE = e.newEnvelope as IEnvelope;
            DrawRectangle(pE);
        }
        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (axMapControl2.LayerCount > 0) {
                if (e.button == 1) {
                    IPoint point = new PointClass();
                    point.PutCoords(e.mapX, e.mapY);
                    Envelope pEnvelope = axMapControl1.Extent as Envelope ;
                    pEnvelope.CenterAt(point);
                    axMapControl1.Extent = pEnvelope as IEnvelope;
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                }
                else if (e.button == 2) {
                    IEnvelope envelop = axMapControl2.TrackRectangle();
                    axMapControl1.Extent = envelop;
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }
            }
        }
        //框的拖动
        private void axMapControl2_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.button != 1) return;
            IPoint point = new PointClass();
            point.PutCoords(e.mapX, e.mapY);
            axMapControl1.CenterAt(point);
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        # endregion

        # region page同步

        private void copy()
        {
            IObjectCopy pOb = new ObjectCopyClass();
            object copefrom = axMapControl1.Map;
            object target = pOb.Copy(copefrom);
            object copeto = axPageLayoutControl1.ActiveView.FocusMap;
            pOb.Overwrite(target, ref copeto);
            axPageLayoutControl1.ActiveView.Refresh();
        }
        private void axMapControl1_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            //获取page当前视图
            IActiveView page = axPageLayoutControl1.ActiveView.FocusMap as IActiveView;
            IDisplayTransformation trans = page.ScreenDisplay.DisplayTransformation;
            trans.VisibleBounds = axMapControl1.Extent;
            axPageLayoutControl1.ActiveView.Refresh();
            copy();
        }
        
        # endregion
        # endregion

        # region TOCC右键

        IFeatureLayer pFL = new FeatureLayerClass(); //选中的图层
        attribute form = null;

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 2) {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap map = null;
                object unk = null;
                object data = null;
                ILayer layer = null;
                axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref layer, ref unk, ref data);
                pFL = layer as IFeatureLayer;
                if (item == esriTOCControlItem.esriTOCControlItemLayer && pFL != null) {

                    contextMenuStrip1.Show(Control.MousePosition);

                }
            }
            
        }



        private void 属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (form == null || form.IsDisposed)
            {
                form = new attribute();
            }
            form.current = pFL;
            form.init();
            form.ShowDialog();
        }

        private void 缩放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pFL == null) return;
            (axMapControl1.Map as IActiveView).Extent = pFL.AreaOfInterest;
            (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

            
        }





        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pFL == null) return;
            DialogResult result = MessageBox.Show("是否删除[" + pFL.Name + "]图层","提示", MessageBoxButtons.OKCancel,MessageBoxIcon.Information);
            if (result == DialogResult.OK) {
                axMapControl1.Map.DeleteLayer(pFL);

            }
            axMapControl1.ActiveView.Refresh();
        }



        # endregion

        private void 图形查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.Map.FeatureSelection.Clear();

            IGraphicsContainer pGC = axMapControl1.Map as IGraphicsContainer;
            pGC.Reset();
            IElement element = pGC.Next();
            IGeometry pG = element.Geometry;
            axMapControl1.Map.SelectByShape(pG, null, false);
            pGC.DeleteAllElements();
            axMapControl1.ActiveView.Refresh();


        }




    }
}