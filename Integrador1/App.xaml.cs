using Microsoft.Extensions.DependencyInjection;

namespace Integrador1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
        }

    }
}