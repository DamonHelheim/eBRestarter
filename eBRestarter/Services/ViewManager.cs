using eBRestarter.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;
using System.Windows;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Views.UserControls;

namespace eBRestarter.Services
{
    public class ViewManager
    {
        public static EBRestarterMainWindow? EBRestarterMainWindow { get; set; }
        public static EBRestarterMainViewModel? EBRestarterMainViewModel { get; set; }
        public static ServiceProvider? ServiceProvider { get; set; }

        public static void InitViewManagerData(EBRestarterMainWindow ieBRestarterMainWindow, ServiceProvider? iServiceProvider)
        {
            EBRestarterMainWindow = ieBRestarterMainWindow;

            EBRestarterMainViewModel = ieBRestarterMainWindow.EBRestarterMainViewModel;

            ServiceProvider = iServiceProvider;
        }

        public static void ShowPageOnMainView<T>(bool iFullPage = false) where T : UserControl
        {
            try
            {
                T pageService = ServiceProvider?.GetService<T>()!;

                if (pageService is null) return;

                if (pageService is UserControl userControlPage)
                {
                    
                    Show(userControlPage, iFullPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //public static void LinkToEvent<T>(UC_MainSettingsView uc_MainSettingsView2) where T : UserControl
        //{
        //    try
        //    {
        //        T pageService = ServiceProvider?.GetService<T>()!;

        //        if (pageService is null) return;

        //        if (pageService is UC_MainSettingsView uc_MainSettingsView)
        //        {
        //            uc_MainSettingsView = uc_MainSettingsView2;

        //            if (pageService is UserControl userControlPage)
        //            {
        //                EBRestarterMainWindow!.AnimatedContentControl.PagePlace.Content = uc_MainSettingsView;
        //                Show(userControlPage, false);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}


        public static void LinkToAPIAccess<T>(bool iFullPage = false) where T : UserControl
        {
            try
            {
                T pageService = ServiceProvider?.GetService<T>()!;

                if (pageService is null) return;

                if (pageService is UC_OptionsView uc_OptionsView)
                {
                    if (pageService is UserControl userControlPage)
                    {
                        EBRestarterMainWindow!.AnimatedContentControl.PagePlace.Content = uc_OptionsView.OptionsTabControl.SelectedIndex = 1;
                        Show(userControlPage, iFullPage);                       
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       

        public static void Show(UserControl iPage, bool iFullPage = false)
        {
            EBRestarterMainWindow!.AnimatedContentControl.PagePlace.Content = null;
            EBRestarterMainWindow!.AnimatedContentControl.PagePlace.Content = iPage;

            EBRestarterMainWindow!.AnimatedContentControl.MetroTabItem.IsSelected = false;
            EBRestarterMainWindow!.AnimatedContentControl.MetroTabItem.IsSelected = true;

        }

        public static void Show2(UC_OptionsView uc_OptionsView, bool iFullPage = false)
        {
            EBRestarterMainWindow!.AnimatedContentControl.PagePlace.Content = null;
            EBRestarterMainWindow!.AnimatedContentControl.PagePlace.Content = uc_OptionsView.OptionsTabControl.SelectedIndex = 1;

            EBRestarterMainWindow!.AnimatedContentControl.MetroTabItem.IsSelected = false;
            EBRestarterMainWindow!.AnimatedContentControl.MetroTabItem.IsSelected = true;
        }

    }
}
