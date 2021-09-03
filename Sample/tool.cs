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

//封装一些常用操作
namespace Sample
{
    class tool
    {
        //空间数据库数据加载
        public static void AddAllDataset(IWorkspace pW, AxMapControl axmap) {
            IEnumDataset pED = pW.get_Datasets(esriDatasetType.esriDTAny);
            pED.Reset();

            IDataset pD = pED.Next();

            while (pD != null) {

                if (pD is IFeatureDataset) {
                    IFeatureWorkspace pFW = pW as IFeatureWorkspace;
                    IFeatureDataset pFD = pFW.OpenFeatureDataset(pD.Name);
                    IEnumDataset pED1 = pFD.Subsets;
                    pED1.Reset();
                    IGroupLayer pGL = new GroupLayerClass();
                    pGL.Name = pFD.Name;
                    IDataset pD1 = pED1.Next();

                    while (pD1 != null) {
                        if (pD1 is IFeatureClass) {
                            IFeatureLayer pFL = new FeatureLayerClass();
                            pFL.FeatureClass = pFW.OpenFeatureClass(pD1.Name);
                            if (pFL.FeatureClass != null) {
                                pFL.Name = pFL.FeatureClass.AliasName;
                                pGL.Add(pFL);
                                axmap.AddLayer(pFL);
                            }

                        }
                        pD1 = pED1.Next();
                    }

                }
                else if (pD is IFeatureClass) {
                    IFeatureWorkspace pFW = pW as IFeatureWorkspace;
                    IFeatureLayer pFL = new FeatureLayerClass();
                    pFL.FeatureClass = pFW.OpenFeatureClass(pD.Name);
                    pFL.Name = pFL.FeatureClass.AliasName;
                    axmap.AddLayer(pFL);
                }
                else if (pD is IRasterDataset) {
                    IRasterWorkspaceEx pRW = pW as IRasterWorkspaceEx;
                    IRasterDataset pRD = pRW.OpenRasterDataset(pD.Name);
                    IRasterPyramid3 pRP3 = (IRasterPyramid3)pRD;
                    if (pRP3 != null)
                    {
                        if (!pRP3.Present)
                        {
                            pRP3.Create();
                        }
                    }
                    IRasterLayer pRL = new RasterLayerClass();
                    pRL.CreateFromDataset(pRD);
                    axmap.AddLayer(pRL);


                }
                pD = pED.Next();

            }
            axmap.ActiveView.Refresh();
        }
    }
}
