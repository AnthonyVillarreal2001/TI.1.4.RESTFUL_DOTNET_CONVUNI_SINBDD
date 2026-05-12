using _02.CLIMOV.Servicio;
using System.Globalization;

namespace _02.CLIMOV.Vista;

public partial class ConversionPage : ContentPage
{
    private readonly IRestService _restService;
    private string _categoriaActual = string.Empty;
    private string _tipoClave = string.Empty;
    private string _unidadOrigen = string.Empty;
    private string _unidadDestino = string.Empty;

    private readonly Dictionary<string, List<ItemConversion>> _conversionesPorCategoria = new()
    {
        ["📏 Longitud"] = new()
        {
            new("Metros → Pies", "MetroAPie", "m", "ft"),
            new("Kilómetros → Millas", "KilometroAMilla", "km", "mi"),
            new("Centímetros → Pulgadas", "CentimetroAPulgada", "cm", "in"),
            new("Pulgadas → Centímetros", "PulgadaACentimetro", "in", "cm"),
            new("Pies → Metros", "PieAMetro", "ft", "m")
        },
        ["⚖️ Peso"] = new()
        {
            new("Kilogramos → Libras", "KilogramoALibra", "kg", "lb"),
            new("Gramos → Onzas", "GramoAOnza", "g", "oz"),
            new("Toneladas → Kilogramos", "ToneladaAKilogramo", "ton", "kg"),
            new("Libras → Kilogramos", "LibraAKilogramo", "lb", "kg"),
            new("Onzas → Gramos", "OnzaAGramo", "oz", "g")
        },
        ["🌡️ Temperatura"] = new()
        {
            new("Celsius → Fahrenheit", "CelsiusAFahrenheit", "°C", "°F"),
            new("Celsius → Kelvin", "CelsiusAKelvin", "°C", "K"),
            new("Celsius → Rankine", "CelsiusARankine", "°C", "°Ra"),
            new("Celsius → Réaumur", "CelsiusAReaumur", "°C", "°Re"),
            new("Fahrenheit → Celsius", "FahrenheitACelsius", "°F", "°C")
        }
    };

    public ConversionPage()
    {
        InitializeComponent();
        _restService = new RestService();
        PickerCategoria.SelectedIndex = 0;
        ActualizarUsuario();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ActualizarUsuario();
    }

    private void ActualizarUsuario()
    {
        var username = Preferences.Get("username", "Usuario");
        LabelUsuario.Text = $"Bienvenido, {username}";
    }

    private void OnCategoriaChanged(object? sender, EventArgs e)
    {
        if (PickerCategoria.SelectedItem is not string cat) return;
        _categoriaActual = cat;
        var items = _conversionesPorCategoria[cat];
        PickerTipo.ItemsSource = items;
        PickerTipo.SelectedIndex = 0;
        OcultarResultado();
        EntryValor.Text = string.Empty;
    }

    private void OnTipoChanged(object? sender, EventArgs e)
    {
        if (PickerTipo.SelectedItem is ItemConversion item)
        {
            _tipoClave = item.Clave;
            _unidadOrigen = item.From;
            _unidadDestino = item.To;
            OcultarResultado();
            EntryValor.Text = string.Empty;
        }
    }

    private async void OnConvertirClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryValor.Text))
        {
            await DisplayAlert("⚠️ Error", "Ingrese un valor", "OK");
            return;
        }
        if (!double.TryParse(EntryValor.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double valor))
        {
            await DisplayAlert("⚠️ Error", "Número no válido", "OK");
            return;
        }
        if (PickerTipo.SelectedIndex == -1)
        {
            await DisplayAlert("⚠️ Error", "Seleccione un tipo de conversión", "OK");
            return;
        }

        try
        {
            MostrarLoading(true);
            OcultarResultado();
            var resultado = await _restService.ConvertirAsync(_tipoClave, valor);
            MostrarResultado(resultado, valor);
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Error", $"Error: {ex.Message}", "OK");
        }
        finally
        {
            MostrarLoading(false);
        }
    }

    private void OnLimpiarClicked(object? sender, EventArgs e)
    {
        EntryValor.Text = string.Empty;
        OcultarResultado();
        PickerCategoria.SelectedIndex = 0;
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("🚪 Cerrar Sesión", "¿Está seguro?", "Sí", "Cancelar");
        if (confirmar)
        {
            Preferences.Remove("isLoggedIn");
            Preferences.Remove("username");
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private void MostrarLoading(bool mostrar)
    {
        LoadingIndicator.IsRunning = mostrar;
        LoadingIndicator.IsVisible = mostrar;
        BtnConvertir.IsEnabled = !mostrar;
        PickerCategoria.IsEnabled = !mostrar;
        PickerTipo.IsEnabled = !mostrar;
        EntryValor.IsEnabled = !mostrar;
        BtnConvertir.Text = mostrar ? "CONVIRTIENDO..." : "CONVERTIR";
    }

    private void MostrarResultado(double resultado, double valorOriginal)
    {
        LabelResultado.Text = $"{resultado:F2} {_unidadDestino}";
        LabelDetalles.Text = $"{valorOriginal:F2} {_unidadOrigen} = {resultado:F2} {_unidadDestino}";
        FrameResultado.IsVisible = true;
    }

    private void OcultarResultado() => FrameResultado.IsVisible = false;

    // Clase interna para el ComboBox
    private record ItemConversion(string Texto, string Clave, string From, string To)
    {
        public override string ToString() => Texto;
    }
}