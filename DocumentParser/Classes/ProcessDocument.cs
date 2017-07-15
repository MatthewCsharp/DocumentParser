using System;
using System.IO;
using System.Linq;

namespace DocumentParser.Classes {
    public class ProcessDocument {
        private static CleanImage _cleanImage;
        private static ParseImage _parseImage;

        private bool _currentlyRunning;

        public ProcessDocument() {
            _cleanImage = new CleanImage();
            _parseImage = new ParseImage();
        }

        public async void Process() {
            if (!MainWindow.Settings.Running || _currentlyRunning) {
                return;
            }
            _currentlyRunning = true;


            try {
                var filePath = CreateDateFolder();

                var files = Directory.GetFiles(MainWindow.Settings.ScanFolder).Where(f => Path.GetExtension(f) == ".pdf").ToList();

                if (files.Count != 0) {
                    MainWindow.AppendHistory("Total files in " + MainWindow.Settings.ScanFolder + ": " + files.Count, HistoryType.Status);

                    foreach (var file in files) {
                        try {
                            var cleanedImgPath = await _cleanImage.Execute(file);
                            var dataList = await _parseImage.Execute(cleanedImgPath);

                            var fileName = $"{dataList[1]} {dataList[0]}";

                            var needToRename = File.Exists($"{filePath}/{fileName}.pdf");
                            var iteration = 1;
                            while (needToRename) {
                                var newFileName = fileName + $"({iteration})";
                                needToRename = File.Exists($"{filePath}/{newFileName}.pdf");
                                iteration++;

                                if (!needToRename) {
                                    fileName = newFileName;
                                }
                            }

                            File.Move(file, $"{filePath}/{fileName}.pdf");
                            MainWindow.AppendHistory(file + " had been processed.", HistoryType.Success);
                        }
                        catch (Exception x) {
                            if (x.Message == "The process cannot access the file because it is being used by another process.\n") {
                                MainWindow.AppendHistory($"The file: {Path.GetFileName(file)} is being used and cannot be processed", HistoryType.Error);
                            }
                            else {
                                MainWindow.AppendHistory(x.Message, HistoryType.Error);
                            }
                        }
                    }
                }
                else {
                    MainWindow.AppendHistory("Zero files to process. Waiting...", HistoryType.NoFiles);
                }
            }
            catch (Exception e) {
                MainWindow.AppendHistory(e.Message, HistoryType.Error);
            }
            finally {
                _currentlyRunning = false;
            }
        }

        public string CreateDateFolder() {
            var yearName = DateTime.Now.Year;
            var monthNumber = DateTime.Now.Month;
            var monthName = DateTime.Now.ToString("MMMM").ToUpper();
            var combinedMonthFolder = $"{monthNumber} {monthName}";

            var filePath = $"{MainWindow.Settings.ProcessedFolder}/{yearName}/{combinedMonthFolder}";
            Directory.CreateDirectory(filePath);

            return filePath;
        }
    }
}