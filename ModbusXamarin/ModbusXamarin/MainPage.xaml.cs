using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ModbusXamarin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void ConfigHostClicked(object sender, EventArgs e)
        {
            
        }

        void ConfigIntClicked(object sender, EventArgs e)
        {

        }

        void LaunchCntrlClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ControlPage());
        }
    }
}
