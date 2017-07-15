using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DocumentParser.Classes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DocumentParser {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static ListBox HistoryListBox;

        private static bool _noFilesWasLastMsg;
        private DispatcherTimer _dispatcherTimer;
        public ProcessDocument ProcessDocument;

        public MainWindow() {
            InitializeComponent();
            InitializeSettings();
            RequiredDependenciesTooltip();

            HistoryListBox = HistoryList;
            ProcessDocument = new ProcessDocument();

            BeginExecution();
        }

        public static Settings Settings { get; set; }

        private void BeginExecution() {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += (sender, args) => { ProcessDocument.Process(); };
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            _dispatcherTimer.Start();
        }

        private void InitializeSettings() {
            Settings = new Settings();

            ScanFolderTxt.Text = Settings.ScanFolder;
            ProcessedFolderTxt.Text = Settings.ProcessedFolder;
        }

        private void RequiredDependenciesTooltip() {
            var terreractLink = "https://github.com/charlesw/tesseract";
            var ghostScriptLink = "https://www.ghostscript.com/download/gsdnld.html";

            RequiredDependenciesTxt.ToolTip = $"Tesseract Dlls: {terreractLink}\nGhostscript: {ghostScriptLink}";

            CreatedBy.ToolTip = "matt@csharp.com.au";
        }

        private void ShutdownApplication(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void TopbarMouseDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void BrowseFolder(object sender, RoutedEventArgs e) {
            var btn = (Button) sender;

            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                if (btn.Tag.ToString() == "ScanFolder") {
                    ScanFolderTxt.Text = dialog.FileName;
                }
                else {
                    ProcessedFolderTxt.Text = dialog.FileName;
                }

                Settings.UpdateSetting(btn.Tag.ToString(), dialog.FileName);
            }
        }

        private void ToggleRunning(object sender, RoutedEventArgs e) {
            Settings.Running = !Settings.Running;
            Settings.UpdateSetting("Running", Settings.Running.ToString());

            if (Settings.Running) {
                StatusTxt.Text = "Running";
                StatusTxt.Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#C7F464");
            }
            else {
                StatusTxt.Text = "Stopped";
                StatusTxt.Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#C02942");
            }
        }

        public static void AppendHistory(string msg, HistoryType type, string tooltip = "") {
            Brush color = Brushes.Black;

            if (type == HistoryType.NoFiles) {
                if (_noFilesWasLastMsg) {
                    return;
                }
                _noFilesWasLastMsg = true;
            }
            else {
                _noFilesWasLastMsg = false;
            }

            switch (type) {
                case HistoryType.Status:
                    color = Brushes.Black;
                    break;
                case HistoryType.Success:
                    color = Brushes.Green;
                    break;
                case HistoryType.Error:
                    color = (SolidColorBrush) new BrushConverter().ConvertFrom("#C02942");
                    break;
                case HistoryType.Warning:
                    color = (SolidColorBrush) new BrushConverter().ConvertFrom("#CC4D4D");
                    break;
                case HistoryType.NoFiles:
                    color = Brushes.Black;
                    break;
            }

            try {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    var txt = new TextBlock {Text = msg, Foreground = color};
                    if (tooltip != "") {
                        txt.ToolTip = tooltip;
                    }

                    HistoryListBox.Items.Add(txt);


                    HistoryListBox.SelectedIndex = HistoryListBox.Items.Count - 1;
                    HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                }));
            }
            catch (Exception e) {
                // Ignored
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }

    public enum HistoryType {
        Status,
        Success,
        Warning,
        Error,
        NoFiles
    }
}