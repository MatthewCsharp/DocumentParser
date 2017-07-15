using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tesseract;

namespace DocumentParser.Classes {
    public class ParseImage {
        public async Task<List<string>> Execute(string filePath) {
            var dataList = new List<string>();
            try {
                using (var engine = new TesseractEngine("C:\\tessdata", "eng", EngineMode.Default)) {
                    try {
                        using (var img = Pix.LoadFromFile(filePath)) {
                            try {
                                await Task.Run(() => {
                                    using (var page = engine.Process(img)) {
                                        var pageTxt = page.GetText();

                                        MainWindow.AppendHistory("Hover for parsed content", HistoryType.Status, pageTxt.Replace("\n\n", "\n"));
                                        var data = pageTxt;
                                        var soNumber = GetSoNumber(data);
                                        var customerCode = GetCustomerCode(data);

                                        dataList.Add(soNumber);
                                        dataList.Add(customerCode);

                                        File.Delete(filePath);
                                        MainWindow.AppendHistory("original deleted", HistoryType.Status);
                                    }
                                });
                            }
                            catch (Exception x) {
                                MainWindow.AppendHistory("Error: " + x.Message + "\n", HistoryType.Error);
                                MainWindow.AppendHistory("InnerException: " + x.InnerException + "\n", HistoryType.Error);
                            }
                        }
                    }
                    catch (Exception x) {
                        MainWindow.AppendHistory("Error: " + x.Message + "\n", HistoryType.Error);
                        MainWindow.AppendHistory("InnerException: " + x.InnerException + "\n", HistoryType.Error);
                    }
                }
            }
            catch (Exception x) {
                MainWindow.AppendHistory("Error: " + x.Message + "\n", HistoryType.Error);
                MainWindow.AppendHistory("InnerException: " + x.InnerException + "\n", HistoryType.Error);
            }

            return dataList;
        }

        private string GetSoNumber(string data) {
            try {
                var soIndex = data.IndexOf("\n");

                var soNumber = data.Substring(soIndex + 1, 7);
                var stripedSoNum = soNumber.Substring(2, soNumber.Length - 2);

                int n;
                var isNumeric = int.TryParse(stripedSoNum, out n);

                if (stripedSoNum.Length == 5 && isNumeric) {
                    return stripedSoNum;
                }
            }
            catch (Exception x) {
                MainWindow.AppendHistory("Could not gather SO Number from page data.", HistoryType.Warning);
            }
            return "UNDETECTED";
        }

        private string GetCustomerCode(string data) {
            try {
                var splitData = data.Split('\n');

                foreach (var s in splitData) {
                    if (s.Length > 13 && LevenshteinDistance.Compute("customer code", s.Substring(0, 13).ToLower()) < 9) {
                        // Find the word that resembles 'code'
                        var splitLine = s.Split(' ');

                        for (var i = 0; i < splitLine.Length; i++) {
                            if (splitLine[i].ToLower().Contains("code")) {
                                var endOfCodeIndex = s.IndexOf(splitLine[i]);
                                endOfCodeIndex += splitLine[i].Length;
                                var code = s.Substring(endOfCodeIndex);

                                if (code.Contains("—")) {
                                    code = code.Replace("—", "-");

                                    var dashIndex = code.IndexOf("-");
                                    if (code[dashIndex - 1] == ' ') {
                                        code = code.Remove(dashIndex - 1, 1);
                                    }
                                }

                                return code;
                            }
                        }
                    }
                }
            }
            catch (Exception x) {
                MainWindow.AppendHistory("Could not gather Customer Code from page data.", HistoryType.Warning);
            }
            return "UNDETECTED";
        }
    }
}