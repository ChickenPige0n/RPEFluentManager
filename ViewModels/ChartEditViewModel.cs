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
        private void GenInfoCsv()
        {
            if (ChartData != null)
            {
                string csvPath = Path.Combine(SettingsHandler.GetSettings().ResourcePath,ChartData.ChartPath,"info.csv");
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
                System.Diagnostics.Process.Start("explorer.exe", $"/select,{csvPath}");

            }
        }
    }
}
