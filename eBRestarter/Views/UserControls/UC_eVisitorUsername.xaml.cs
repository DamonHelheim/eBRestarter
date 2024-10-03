using System;
using System.Windows;
using System.Windows.Controls;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Classes.EBEvents;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_eVisitorUsername : UserControl
    {
        public event EventHandler<NameEnteredEventArgs>? NameEntered;
        public EVisitorUsernameViewModel EVisitorUsernameViewModel { get; } = new();

        private readonly bool _valueChanged = false;

        public UC_eVisitorUsername()
        {
            InitializeComponent();

            DataContext = EVisitorUsernameViewModel;

            _valueChanged = true;
        }


        private void BtnUserNameApply_Click(object sender, RoutedEventArgs e)
        {
            string name = tb_PleaseEnterYourUsernameHere.Text;           

            NameEntered?.Invoke(this, new NameEnteredEventArgs(name));
        }

    }
}
