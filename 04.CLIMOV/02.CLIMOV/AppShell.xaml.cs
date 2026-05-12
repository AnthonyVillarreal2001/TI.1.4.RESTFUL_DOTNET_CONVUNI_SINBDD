using _02.CLIMOV.Vista;

namespace _02.CLIMOV
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("ConversionPage", typeof(ConversionPage));
        }
    }
}
