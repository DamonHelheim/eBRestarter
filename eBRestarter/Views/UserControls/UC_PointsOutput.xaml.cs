using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.UserControls
{
    public partial class UC_PointsOutput : UserControl
    {
        public PointsOutputViewModel PointsOutputViewModel { get; private set; } = new();
        
        public UC_PointsOutput()
        {
            InitializeComponent();

            InitializePointsOutputAgain();
        }

        private void InitializePointsOutputAgain()
        {
            Task.Run(InitializePointsOutput);
        }

        private async Task InitializePointsOutput()
        {
            await Task.Run(async () => {
            
                while (true)
                {
                    if (DateTime.Now.ToString("t").Equals("00:00") is true)
                    {
                        PointsOutputViewModel = new();

                        while(DateTime.Now.ToString("t").Equals("00:00") is true)
                        {
                            await Task.Delay(1000);
                        }
                    }

                    await Task.Delay(1000);
                }              

            });
        }
    }
}
