using System;
using System.IO;
using System.Threading.Tasks;
using FredsImageMagickScripts;
using ImageMagick;

namespace DocumentParser.Classes {
    public class CleanImage {
        public async Task<string> Execute(string filePath) {
            try {
                var path = Path.GetTempPath() + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".tif";
                MainWindow.AppendHistory("Document found. ", HistoryType.Status);
                MagickImage img = null;

                var settings = new MagickReadSettings {Density = new Density(300, 300)};
                MainWindow.AppendHistory($"Splitting pdf into seperate files...", HistoryType.Status);
                await Task.Run(() => {
                    using (var images = new MagickImageCollection()) {
                        images.Read(filePath, settings);
                        MainWindow.AppendHistory($"Total images: {images.Count}", HistoryType.Status);
                        images[0].Write(path);
                    }
                });


                await Task.Run(() => {
                    using (var i = new MagickImage(path)) {
                        var offset = new TextCleanerCropOffset {Bottom = 2600, Left = 1500, Right = 100, Top = 250};

                        var script = new TextCleanerScript();
                        script.SmoothingThreshold = new Percentage(70);
                        script.Unrotate = true;
                        script.MakeGray = true;
                        script.CropOffset = offset;

                        MainWindow.AppendHistory($"Cleaning up image for accurate OCR", HistoryType.Status);

                        MagickImage returnedImg = null;

                        returnedImg = script.Execute(i);

                        returnedImg.Write(path);
                    }
                });
                return path;
            }
            catch (Exception x) {
                MainWindow.AppendHistory(x.Message, HistoryType.Error);
                return null;
            }
        }
    }
}