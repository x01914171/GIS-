using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Windows.Forms;
namespace Sample
{

    class addxy
    {
        List<string> pColumns = new List<string>();
        struct CPoint {
            public string Name;
            public double X;
            public double Y;
        }

        //读取文件
        private List<CPoint> GetPoints(string surveyDataFullName)
        {
            try
            {
                List<CPoint> pList = new List<CPoint>();
                char[] charArray = new char[] { ',', ' ', '\t' };   //常用的分隔符为逗号、空格、制位符
                //文本信息读取
                FileStream fs = new FileStream(surveyDataFullName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string strLine = sr.ReadLine();
                if (strLine != null)
                {
                    string[] strArray = strLine.Split(charArray);
                    if (strArray.Length > 0)
                    {
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            pColumns.Add(strArray[i]);
                        }
                    }

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        //点信息的读取
                        strArray = strLine.Split(charArray);
                        CPoint pCPoint = new CPoint();
                        pCPoint.Name = strArray[0].Trim();
                        pCPoint.X = Convert.ToDouble(strArray[1]);
                        pCPoint.Y = Convert.ToDouble(strArray[2]);

                        pList.Add(pCPoint);
                    }
                }
                else
                {
                    return null;
                }
                sr.Close();
                return pList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }



        private IFeatureLayer CreateShpFromFile(List<CPoint> points, string filePath)
        {
            int index = filePath.LastIndexOf('\\');
            string folder = filePath.Substring(0, index);
            string shapeName = filePath.Substring(index + 1);

            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pFW = (IFeatureWorkspace)pWF.OpenFromFile(folder, 0);
            IFields pFs = new FieldsClass();
            IFieldsEdit pFsE = pFs as IFieldsEdit;

            IField pf = new FieldClass();
            IFieldEdit pFE = pf as IFieldEdit;

            pFE.Name_2 = "Shape";
            pFE.Type_2 = esriFieldType.esriFieldTypeGeometry;
            IGeometryDef pGD = new GeometryDefClass();
            IGeometryDefEdit pGDE = pGD as IGeometryDefEdit;
            pGDE.GeometryType_2 = esriGeometryType.esriGeometryPoint;


            //定义坐标系
            ISpatialReferenceFactory pSRF = new SpatialReferenceEnvironment();
            ISpatialReference pSR = pSRF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_Beijing1954);
            pGDE.SpatialReference_2 = pSR;

            pFE.GeometryDef_2 = pGD;
            pFsE.AddField(pf);

            IFeatureClass pFC = pFW.CreateFeatureClass(shapeName, pFs, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            IPoint point = new PointClass();

            for(int i =0;i<points.Count;i++){

                point.X = points[i].X;
                point.Y = points[i].Y;
                IFeature pF = pFC.CreateFeature();
                pF.Shape = point;
                pF.Store();
                
            }
            IFeatureLayer pFL = new FeatureLayerClass();
            pFL.Name = shapeName;
            pFL.FeatureClass = pFC;
            return pFL;




        }

        public void show(AxMapControl axmap,string filename) {
            List<CPoint> points = GetPoints(filename);

            if (points ==null){
                MessageBox.Show("文件为空");
            }else{
                IFeatureLayer pFL = CreateShpFromFile(points, filename);
                axmap.AddLayer(pFL);
                axmap.ActiveView.Refresh();
            }
        }
    }
}