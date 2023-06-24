using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;

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

    }
}
