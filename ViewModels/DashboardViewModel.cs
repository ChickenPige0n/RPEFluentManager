using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPEFluentManager.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Wpf.Ui.Common.Interfaces;
using System.Xml;

namespace RPEFluentManager.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        public ObservableCollection<ChartData>? ChartList { get; set; }

        string ResourcePath;



        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }

        public DashboardViewModel() 
        {
            ChartList = new ObservableCollection<ChartData>();
            ResourcePath = SettingsHandler.GetSettings().ResourcePath;

            try
            {
                string[] folderPath111 = Directory.GetDirectories(ResourcePath, "*", SearchOption.AllDirectories);
            }catch (Exception ex)
            {
                return;
            }

            string[] folderPaths = Directory.GetDirectories(ResourcePath, "*", SearchOption.AllDirectories);

            foreach (string folderPath in folderPaths)
            {
                string infoFilePath = Path.Combine(folderPath, "info.txt");
                if (File.Exists(infoFilePath))
                {
                    string folderName = new DirectoryInfo(folderPath).Name;
                    if (folderName == "tmp")
                    {
                        continue;
                    }

                    ChartData cd = new ChartData();

                    
                    string infoContent = File.ReadAllText(infoFilePath);

                    string[] lines = infoContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        string[] infos = line.Split(new[] { ':' });
                        switch (infos[0].Trim())
                        {
                            case "Name":
                                cd.ChartName = infos[1].Trim();
                                break;

                            case "Path":
                                cd.ChartPath = infos[1].Trim();
                                break;

                            case "Picture":
                                cd.ImageSource = Path.Combine(folderPath,infos[1].Trim());
                                break;

                            case "Level":
                                cd.ChartDiff = infos[1].Trim();
                                break;

                            default:
                                break;
                        }
                    }

                    ChartList.Add(cd);
                }
            }

        }

        public partial class ChartData : ObservableObject
        {
            public string? ImageSource { get; set; }
            
            public string? ChartName { get; set; }
            public string? ChartDiff { get; set; }
            public string? ChartPath { get; set; }

            [ObservableProperty]
            public bool _isSelected;

            public ChartData()
            { 
                IsSelected = false;
            }
        }

        [RelayCommand]
        public void DelAutoSave(string p)
        {
            if (ChartList != null)
            {
                long totalSize = 0;
                foreach (ChartData cd in ChartList)
                {
                    if (cd.IsSelected)
                    {
                        string chartPath = Path.Combine(ResourcePath, cd.ChartPath);
                        

                        // 构造文件名模式
                        string pattern = $"AutoSave_*_{cd.ChartPath}.json";

                        // 获取目录中匹配模式的所有文件
                        string[] filePaths = Directory.GetFiles(chartPath, pattern);

                        // 逐个删除文件
                        foreach (string filePath in filePaths)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            totalSize += fileInfo.Length;
                            File.Delete(filePath);
                        }
                    }
                }

                string size = FormatFileSize(totalSize);

                makeMessageBox("喜报",$"删除了{size}的AutoSave文件!");
                
            }
        }


        public static string FormatFileSize(long fileSize)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(fileSize);

            if (bytes >= Math.Pow(byteConversion, 3)) // GB
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 3), 2), " GB");
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) // MB
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 2), 2), " MB");
            }
            else if (bytes >= byteConversion) // KB
            {
                return string.Concat(Math.Round(bytes / byteConversion, 2), " KB");
            }
            else // B
            {
                return string.Concat(bytes, " B");
            }
        }


        private void makeMessageBox(string title, string content)
        {
            Wpf.Ui.Controls.MessageBox msgbx = new Wpf.Ui.Controls.MessageBox();
            msgbx.Foreground = Brushes.Black;
            msgbx.Width = 200;
            msgbx.Height = 146;
            msgbx.ButtonLeftClick += (sender, e) => { msgbx.Close(); };
            msgbx.ButtonLeftName = "好的";
            msgbx.ButtonRightClick += (sender, e) => { msgbx.Close(); };
            msgbx.ButtonRightName = "取消";
            msgbx.Show(title, content);
        }


        bool selectionFlag = true;

        [RelayCommand]
        public void SelectAll(string p)
        {
            if (ChartList != null)
            {
                foreach (ChartData cd in ChartList)
                {
                    cd.IsSelected = selectionFlag;
                }
                selectionFlag = !selectionFlag;
            }
        }

    }
}
