using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace phoenix_prototype
{

    class WindowMetadata
    {
        public string windowsName {  get; set; }
        public double top {  get; set; }
        public double left { get; set; }
        public double height { get; set; }
        public double width { get; set; }


        public WindowMetadata(string windowsName, double top, double left, double height, double width) {
            this.windowsName = windowsName;
            this.top = top;
            this.left = left;
            this.height = height;
            this.width = width;
        }
    }

    internal class ViewManager
    {

        public static List<WindowMetadata> PrepareDefaultLayout()
        {
            List<WindowMetadata> windowMetadataList = new();
            windowMetadataList.Add(new WindowMetadata("_Watchlist", 76, -0.8, 425.6, 396));
            windowMetadataList.Add(new WindowMetadata("_Orders", 498.40000000000003, 0, 326.40000000000003, 397.6));
            windowMetadataList.Add(new WindowMetadata("_Notifications", 72.8, 1180, 753.6, 358.40000000000003));
            windowMetadataList.Add(new WindowMetadata("_Search", 73.60000000000001, 388.8, 450.40000000000003, 802.4000000000001));
            windowMetadataList.Add(new WindowMetadata("_News", 522.4, 389.6, 304, 800));

            return windowMetadataList;
        }

        public static bool ExportLayoutToFile(List<Window> windowsList, string filePath)
        {
            List<WindowMetadata> windowMetadataList = new();
            if (windowsList.Count != 0)
            {
                foreach (Window win in windowsList)
                {
                    windowMetadataList.Add(new WindowMetadata(win.Name, win.Top, win.Left, win.Height, win.Width));
                }
            }

            //TODO: Save the list (json format) on a file

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(windowMetadataList, options);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }


        public static List<WindowMetadata> ImportLayoutFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<WindowMetadata> listOfWindowMetadata = JsonSerializer.Deserialize<List<WindowMetadata>>(json);
                if (listOfWindowMetadata != null && listOfWindowMetadata.Count > 0)
                {
                    return listOfWindowMetadata;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new();
            }
            return new();
        }

    }


}
