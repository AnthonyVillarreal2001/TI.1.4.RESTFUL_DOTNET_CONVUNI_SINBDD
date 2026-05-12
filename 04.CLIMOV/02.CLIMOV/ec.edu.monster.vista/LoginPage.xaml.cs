using CommunityToolkit.Maui.Alerts;

namespace _02.CLIMOV.Vista
{
    public partial class LoginPage : ContentPage
    {
        // Credenciales quemadas
        private const string USERNAME_VALIDO = "MONSTER";
        private const string PASSWORD_VALIDO = "MONSTER9";

        public LoginPage()
        {
            InitializeComponent();

            // Limpiar sesión previa para debug
#if DEBUG
            Preferences.Remove("isLoggedIn");
            Preferences.Remove("username");
#endif
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Obtener valores
            string username = EntryUsuario.Text?.Trim();
            string password = EntryPassword.Text?.Trim();

            // Validar campos vacíos
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await MostrarToast("⚠️ Por favor complete todos los campos");
                return;
            }

            // Mostrar loading
            MostrarLoading(true);

            // Simular validación (puedes agregar delay para efecto)
            await Task.Delay(800);

            // Validar credenciales
            if (username == USERNAME_VALIDO && password == PASSWORD_VALIDO)
            {
                // Login exitoso
                MostrarLoading(false);

                // Mostrar toast de éxito
                await MostrarToast("✅ Inicio de sesión exitoso");

                // Guardar sesión (opcional)
                Preferences.Set("isLoggedIn", true);
                Preferences.Set("username", username);

                // Pequeño delay para que se vea el toast
                await Task.Delay(500);

                // Navegar a página de conversión
                await Shell.Current.GoToAsync("//ConversionPage");

                // Limpiar campos
                LimpiarCampos();
            }
            else
            {
                // Login fallido
                MostrarLoading(false);
                await MostrarToast("❌ Usuario o contraseña incorrectos");

                // Limpiar contraseña por seguridad
                EntryPassword.Text = string.Empty;
            }
        }

        private async Task MostrarToast(string mensaje)
        {
            var toast = Toast.Make(mensaje, CommunityToolkit.Maui.Core.ToastDuration.Short, 16);
            await toast.Show();
        }

        private void MostrarLoading(bool mostrar)
        {
            LoadingIndicator.IsRunning = mostrar;
            LoadingIndicator.IsVisible = mostrar;
            BtnLogin.IsEnabled = !mostrar;
            BtnLogin.Text = mostrar ? "INGRESANDO..." : "INGRESAR";
        }

        private void LimpiarCampos()
        {
            EntryUsuario.Text = string.Empty;
            EntryPassword.Text = string.Empty;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LimpiarCampos();
        }
    }
}
