//using BugLog.UWP.Renderers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.UWP;
using Xamarin.Forms.Platform.UWP;

/*
[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace BugLog.UWP.Renderers
{
        public class CustomMapRenderer : MapRenderer
        {
          //  CustomMap customMap;
            MapControl mapControl;

            protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
            {
                base.OnElementChanged(e);

                if (e.NewElement != null)
                {
                   // customMap = e.NewElement as CustomMap;
                    mapControl = Control as MapControl;

                    UpdateTiles();
                }
            }
            /// <summary>
            /// Convert MapTileTemplate string to fit UWP HttpMapTileDataSource
            /// </summary>
            /// <param name="mapTileTemplate"></param>
            /// <returns></returns>
            private string GetTileTemplateForUWP(string mapTileTemplate)
            {
                return mapTileTemplate.Replace("{z}", "{zoomlevel}");
            }

            private void UpdateTiles()
            {
                Debug.WriteLine("BEGINING !");
               // HttpMapTileDataSource dataSource = new HttpMapTileDataSource(GetTileTemplateForUWP(customMap.MapTileTemplate));
                MapTileSource tileSource = new MapTileSource(dataSource);
                mapControl.TileSources.Add(tileSource);
                Debug.WriteLine("END !");
            }
        }
}
}
*/
