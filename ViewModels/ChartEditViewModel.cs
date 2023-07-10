using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;
using RPEFluentManager.Models;
using System.IO;
using RPEFluentManager.Views.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Text.Unicode;
using System.Net;
using System.Net.Sockets;

namespace RPEFluentManager.ViewModels
{
    public partial class ChartEditViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private DashboardViewModel.ChartData _chartData;

        [ObservableProperty]
        private string _density;

        [ObservableProperty]
        private string _parent;

        [ObservableProperty]
        private string _child;

        [ObservableProperty]
        private string _parentStartTime;
        
        [ObservableProperty]
        private string _parentEndTime;

        [ObservableProperty]
        private string _fileServerPort;


        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }

        public void SetData(DashboardViewModel.ChartData data)
        {
            ChartData = data;
        }

        public ChartEditViewModel()
        {
            _chartData = new DashboardViewModel.ChartData();
        }

        [RelayCommand]
        private void OnGenInfo()
        {
            GenInfoCsv(true, ChartData);
        }

        private static void GenInfoCsv(bool openExplorer, DashboardViewModel.ChartData ChartData)
        {
            if (ChartData != null)
            {
                string csvPath = Path.Combine(SettingsHandler.GetSettings().ResourcePath, ChartData.ChartPath, "info.csv");
                string aspectRatio = true ? "1.777778" : "1.333332";
                File.WriteAllText(csvPath, "Chart,Music,Image,AspectRatio,ScaleRatio,GlobalAlpha,Name,Level,Illustrator,Designer\n谱面, 音乐, 图片, 宽高比, 按键缩放, 背景变暗, 名称, 等级, 曲绘, 谱师\n"
                    + ChartData.ChartFileName + ", "
                    + ChartData.MusicFileName + ", "
                    + Path.GetFileName(ChartData.ImageSource) + ", "
                    + aspectRatio + ", 8.00E+03, 0.6, "
                    + ChartData.ChartPath + ", "
                    + ChartData.Composer + ", "
                    + ChartData.Illustrator + ", "
                    + ChartData.Charter);
                if (openExplorer) System.Diagnostics.Process.Start("explorer.exe", $"/select,{csvPath}");

            }
        }

        [RelayCommand]
        public void PackPEZ()
        {
            PackPEZ(true, ChartData);
        }


        public static void PackPEZ(bool openExplorer, DashboardViewModel.ChartData ChartData)
        {
            if (ChartData != null)
            {
                var resPath = SettingsHandler.GetSettings().ResourcePath;

                string absChartPath = Path.Combine(resPath, ChartData.ChartPath);

                List<string> absFilePaths = new List<string> { };

                GenInfoCsv(false, ChartData);
                absFilePaths.Add(Path.Combine(absChartPath, "info.csv"));
                absFilePaths.Add(ChartData.ImageSource);
                absFilePaths.Add(Path.Combine(absChartPath, ChartData.ChartFileName));
                absFilePaths.Add(Path.Combine(absChartPath, ChartData.MusicFileName));
                absFilePaths.Add(Path.Combine(absChartPath, "info.txt"));

                RPEChart? chart = JsonConvert.DeserializeObject<RPEChart>(File.ReadAllText(Path.Combine(absChartPath, ChartData.ChartFileName)));
                if (chart == null) return;

                foreach (JudgeLineListItem line in chart.judgeLineList)
                {
                    if (line.Texture.Trim('\0').Trim() == "line.png") continue;

                    absFilePaths.Add(Path.Combine(absChartPath, line.Texture));
                }

                var pezPath = Path.Combine(resPath, $"{ChartData.ChartPath}.pez");


                if (File.Exists(pezPath)) File.Delete(pezPath);

                using (var archive = ZipFile.Open(pezPath, ZipArchiveMode.Create))
                {
                    foreach (string absFilePath in absFilePaths)
                    {
                        archive.CreateEntryFromFile(absFilePath.Trim('\0'), Path.GetFileName(absFilePath.Trim('\0')));
                    }
                    archive.Dispose();
                }
                if (openExplorer) System.Diagnostics.Process.Start("explorer.exe", $"/select,{pezPath}");
            }
        }


        [RelayCommand]
        private void GenParentEvent()
        {
            if (ChartData != null)
            {
                var resPath = SettingsHandler.GetSettings().ResourcePath;
                string absChartPath = Path.Combine(resPath, ChartData.ChartPath);
                RPEChart? chart = JsonConvert.DeserializeObject<RPEChart>(File.ReadAllText(Path.Combine(absChartPath, ChartData.ChartFileName)));

                if (chart == null)
                {
                    DashboardViewModel.makeMessageBox("错误", "读取谱面失败");
                    return;
                }


                int childIdx;
                int parentIdx;
                int density;
                if (!int.TryParse(Child, out childIdx) || !int.TryParse(Parent, out parentIdx) || !int.TryParse(Density,out density))
                {
                    DashboardViewModel.makeMessageBox("错误", "读取父子线号失败");
                    return;
                }

                EventLayersItem child = chart.judgeLineList[childIdx].eventLayers[0];
                EventLayersItem parent = chart.judgeLineList[parentIdx].eventLayers[0];

                Time? startTime = Time.Parse(ParentStartTime);
                Time? endTime = Time.Parse(ParentEndTime);
                if( startTime == null||endTime == null )
                {
                    DashboardViewModel.makeMessageBox("错误", "读取时间失败");
                    return;
                }

                (int a,int b) = child.moveXEvents.GetEventIndexRangeByTime(startTime, endTime);
                child.moveXEvents.CutEventInRange(a, b, int.Parse(Density));
                (int c, b) = child.moveYEvents.GetEventIndexRangeByTime(startTime, endTime);
                child.moveYEvents.CutEventInRange(c, b, int.Parse(Density));
                int Count = (int)((endTime - startTime) * density);


                double curTime;
                double curParentX;
                double curParentY;
                double curParentR;
                double curChildX;
                double curChildY;

                int idx = 0;
                for ( ; idx < Count; idx++)
                {
                    curTime = (double)startTime + ((double)idx / (double)density);
                    curParentX = parent.moveXEvents.GetValByTime(curTime);
                    curParentY = parent.moveYEvents.GetValByTime(curTime);
                    curParentR = -parent.rotateEvents.GetValByTime(curTime);
                    curParentR = (curParentR % 360) * Math.PI / 180.0;

                    curChildX = child.moveXEvents[a + idx].start;
                    curChildY = child.moveYEvents[c + idx].start;

                    child.moveXEvents[a + idx].start = (float)((curChildX * Math.Cos(curParentR)) - (curChildY * Math.Sin(curParentR))) + (float)curParentX;
                    child.moveYEvents[c + idx].start = (float)((curChildX * Math.Sin(curParentR)) + (curChildY * Math.Cos(curParentR))) + (float)curParentY;

                    if (idx != 0) child.moveXEvents[a + idx - 1].end = child.moveXEvents[a + idx].start;
                    if (idx != 0) child.moveYEvents[c + idx - 1].end = child.moveYEvents[c + idx].start;
                }
                
                StreamWriter sw = new StreamWriter(Path.Combine(absChartPath, ChartData.ChartFileName), false, new UTF8Encoding(false));
                sw.Write(JsonConvert.SerializeObject(chart, formatting:Formatting.Indented));
                sw.Close();
            }
        }




        public ChartFileServer? server;

        [RelayCommand]
        private void StartServer()
        {
            server = new ChartFileServer(ChartData, int.Parse(FileServerPort));
            server.Start();
            DashboardViewModel.makeMessageBox("提示", "服务器已经开启");
        }
        [RelayCommand]
        private void StopServer()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }

            DashboardViewModel.makeMessageBox("提示", "服务器已经关闭");
        }
    }   
}
