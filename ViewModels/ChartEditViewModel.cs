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

namespace RPEFluentManager.ViewModels
{
    public partial class ChartEditViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private DashboardViewModel.ChartData _chartData;

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
            GenInfoCsv(true);
        }

        private void GenInfoCsv(bool openExplorer)
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
        private void PackPEZ()
        {
            if (ChartData != null)
            {
                var resPath = SettingsHandler.GetSettings().ResourcePath;

                string absChartPath = Path.Combine(resPath, ChartData.ChartPath);

                List<string> absFilePaths = new List<string> { };

                GenInfoCsv(false);
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

                    absFilePaths.Add(Path.Combine(absChartPath,line.Texture));
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

                System.Diagnostics.Process.Start("explorer.exe", $"/select,{pezPath}");

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


                

                StreamWriter sw = new StreamWriter(Path.Combine(absChartPath, ChartData.ChartFileName), false, new UTF8Encoding(false));
                sw.Write(JsonConvert.SerializeObject(chart, formatting:Formatting.Indented));
                sw.Close();
            }
        }
    }   
}
