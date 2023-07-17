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
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using RPEFluentManager.Views.Windows;
using System.Linq;
using RPEFluentManager.Views.Pages;
using Microsoft.Win32;
using System.Text;
using System.Reflection.Metadata;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;

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

                            case "Song":
                                cd.MusicFileName = infos[1].Trim();
                                break;

                            case "Chart":
                                cd.ChartFileName = infos[1].Trim();
                                break;

                            case "Composer":
                                cd.Composer = infos[1].Trim();
                                break;

                            case "Charter":
                                cd.Charter = infos[1].Trim();
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
            //曲绘绝对路径
            public string ImageSource { get; set; }
            public string MusicFileName { get; set; }
            public string ChartFileName { get; set; }
            
            public string ChartName { get; set; }
            public string ChartDiff { get; set; }
            public string ChartPath { get; set; }

            public string Composer { get; set; }
            public string Charter { get; set; }

            public string Illustrator { get; set; }

            [ObservableProperty]
            public bool _isSelected;

            [RelayCommand]
            public void DelChart()
            {
                Wpf.Ui.Controls.MessageBox msgbx = new Wpf.Ui.Controls.MessageBox();
                msgbx.Foreground = Brushes.Black;
                msgbx.Width = 250;
                msgbx.Height = 146;

                msgbx.ButtonLeftClick += (sender, e) =>
                {


                    var ResourcePath = SettingsHandler.GetSettings().ResourcePath;

                    var ChartListPath = Path.Combine(Path.GetDirectoryName(ResourcePath),"ChartList.txt");
                    try
                    {
                        Directory.Delete(Path.Combine(ResourcePath, ChartPath), true);
                    }
                    catch { }

                    string content = File.ReadAllText(ChartListPath);

                    List<string> splitted = content.Split(new char[] {'#'}).ToList();

                    for (int i = splitted.Count - 1; i >= 0; i--)
                    {
                        var infoContent = splitted[i];

                        string[] lines = infoContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            string[] infos = line.Split(new[] { ':' });
                            switch (infos[0].Trim())
                            {
                                case "Path":
                                    if (infos[1].Trim().Equals(ChartPath))
                                    {
                                        splitted.RemoveAt(i);
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }

                    }


                    StreamWriter sw = new StreamWriter(ChartListPath, false, new UTF8Encoding(false));
                    foreach (string infoContent in splitted)
                    {
                        sw.WriteLine("#");
                        sw.Write(infoContent);
                    }
                    sw.Close();


                    // 查找 MainWindow 实例
                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                    mainWindow?.RootNavigation.PageService?.GetPage<DashboardPage>()?.ViewModel.RemoveChartInfoByPath(ChartPath);




                    msgbx.Close();

                    makeMessageBox("喜报", $"删除了名为{ChartName}的谱面!");

                };
                msgbx.ButtonLeftName = "确定";

                msgbx.ButtonRightClick += (sender, e) =>
                {
                    msgbx.Close();
                };
                msgbx.ButtonRightName = "取消";

                msgbx.Foreground = Brushes.White;
                msgbx.Show("警告", "确定要删除这个谱面吗？");

            }

            [RelayCommand]
            public void EditChart(string p)
            {
                // 查找 MainWindow 实例
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                mainWindow?.RootNavigation.Navigate("chartedit");
                mainWindow?.RootNavigation.PageService?.GetPage<ChartEditPage>()?.ViewModel.SetData(this);


            }

            public ChartData()
            {

                ImageSource   = string.Empty;
                MusicFileName = string.Empty;
                ChartFileName = string.Empty;
                ChartName     = string.Empty;
                ChartDiff     = string.Empty;
                ChartPath     = string.Empty;
                Composer      = string.Empty;
                Charter       = string.Empty;
                Illustrator   = string.Empty;

                IsSelected = false;
            }
        }


        [RelayCommand]
        public void FixChartList(string p)
        {
            if (ChartList != null)
            {
                StringBuilder allInfo = new StringBuilder();
                foreach (ChartData cd in ChartList)
                {
                    string infoPath = Path.Combine(ResourcePath, cd.ChartPath, "info.txt");
                    allInfo.Append(File.ReadAllText(infoPath));
                }
                string chartListPath = Path.Combine(Path.GetDirectoryName(ResourcePath), "Chartlist.txt");
                if (File.Exists(chartListPath))
                {
                    Wpf.Ui.Controls.MessageBox msgbx = new Wpf.Ui.Controls.MessageBox();
                    msgbx.Foreground = Brushes.Black;
                    msgbx.Width = 250;
                    msgbx.Height = 146;

                    msgbx.ButtonLeftClick += (sender, e) => 
                    {
                        File.WriteAllText(chartListPath, allInfo.ToString());
                        msgbx.Close();
                    };
                    msgbx.ButtonLeftName = "确定";

                    msgbx.ButtonRightClick += (sender, e) => 
                    {
                        msgbx.Close();
                    };
                    msgbx.ButtonRightName = "取消";

                    msgbx.Foreground = Brushes.White;
                    msgbx.Show("警告", "发现已存在Chartlist.txt,确定覆盖吗？");
                    return;
                }
                File.WriteAllText(chartListPath, allInfo.ToString());

                makeMessageBox("成功", "已还原ChartList.txt");
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


        public static void makeMessageBox(string title, string content)
        {
            Wpf.Ui.Controls.MessageBox msgbx = new Wpf.Ui.Controls.MessageBox();
            msgbx.Foreground = Brushes.Black;
            msgbx.Width = 250;
            msgbx.Height = 146;
            msgbx.ButtonLeftClick += (sender, e) => { msgbx.Close(); };
            msgbx.ButtonLeftName = "好的";
            msgbx.ButtonRightClick += (sender, e) => { msgbx.Close(); };
            msgbx.ButtonRightName = "取消";
            msgbx.Foreground = Brushes.White;
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



                string[] folderPaths = Directory.GetDirectories(ResourcePath, "*", SearchOption.AllDirectories);

                foreach (string folderPath in folderPaths)
                {
                    string[] files = Directory.GetFiles(folderPath);
                    int fileCount = files.Length;

                    if(fileCount <= 0)
                    {
                        return;
                    }

                    var ext = Path.GetExtension(files[0]).ToLower();
                    string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                    if (fileCount == 1 && imageExtensions.Contains(ext))
                    {
                        Directory.Delete(folderPath, true);
                    }
                }
            }
        }

        public void RemoveChartInfoByPath(string path)
        {
            for (int i = ChartList.Count - 1; i >= 0; i--)
            {
                ChartData cd = ChartList[i];
                if (cd.ChartPath.Equals(path)){
                    cd.ImageSource = "";
                    ChartList.RemoveAt(i);
                }
            }
        }
    }
}
