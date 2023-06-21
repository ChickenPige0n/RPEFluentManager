using Wpf.Ui.Common.Interfaces;
using RPEFluentManager.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using System;
using System.Reflection;

namespace RPEFluentManager.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {
        public ViewModels.DashboardViewModel ViewModel
        {
            get;
        }

        public DashboardPage(ViewModels.DashboardViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }

        private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Type scrollViewerType = typeof(ScrollViewer);
            MethodInfo onMouseWheelMethod = scrollViewerType.GetMethod("OnMouseWheel", BindingFlags.Instance | BindingFlags.NonPublic);

            if (onMouseWheelMethod != null)
            {
                object[] parameters = { e };
                onMouseWheelMethod.Invoke(DynScr, parameters);
            }
        }


    }
}