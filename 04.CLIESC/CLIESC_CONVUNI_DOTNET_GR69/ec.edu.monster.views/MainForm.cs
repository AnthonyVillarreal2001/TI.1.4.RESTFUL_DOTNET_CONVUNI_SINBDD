using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CLIESC_CONVUNI_DOTNET_GR69.Controllers;
using CLIESC_CONVUNI_DOTNET_GR69.Models;

namespace CLIESC_CONVUNI_DOTNET_GR69.Views
{
    public partial class MainForm : Form
    {
        private readonly string _user;
        private readonly ConversionController _controller;

        // Controles dinámicos
        private ComboBox cmbCategory, cmbType;
        private TextBox txtValue;
        private Button btnConvert, btnLogout, fabClear;
        private Label lblResult, lblCredits;

        // Diccionario categoría -> lista de tipos con (texto, clave, from, to)
        private readonly Dictionary<string, List<ConvItem>> _conversions = new()
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

        private string _bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", "sullivan.jpg");

        public MainForm(string user, ConversionController controller)
        {
            _user = user;
            _controller = controller;
            InitializeDesign();
        }

        private void InitializeDesign()
        {
            this.Text = "Conversión de Unidades – Sullivan Elegance";
            this.ClientSize = new Size(700, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            // Fondo con imagen
            if (File.Exists(_bgPath))
            {
                this.BackgroundImage = Image.FromFile(_bgPath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                this.BackColor = Color.FromArgb(91, 44, 142);
            }

            // Header
            var header = new Panel
            {
                Size = new Size(660, 45),
                Location = new Point(20, 15),
                BackColor = Color.FromArgb(230, 255, 255, 255),
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 660, 45, 15, 15))
            };
            Controls.Add(header);
            header.Controls.Add(new Label
            {
                Text = "Conversión de Unidades",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(91, 44, 142),
                Location = new Point(20, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            btnLogout = new Button
            {
                Text = "Cerrar sesión",
                Location = new Point(550, 9),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { new LoginForm().Show(); this.Close(); };
            header.Controls.Add(btnLogout);

            // Bloque 1: Categoría (azul)
            var catCard = CrearTarjeta(new Rectangle(20, 75, 660, 75), Color.White, Color.FromArgb(59, 130, 246), "📋 Categoría de Conversión");
            Controls.Add(catCard);
            cmbCategory = new ComboBox
            {
                Location = new Point(15, 42),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbCategory.Items.AddRange(_conversions.Keys.ToArray());
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => UpdateTypes();
            catCard.Controls.Add(cmbCategory);

            // Bloque 2: Tipo (naranja-rojo)
            var typeCard = CrearTarjeta(new Rectangle(20, 165, 660, 75), Color.White, Color.FromArgb(249, 115, 22), "🔄 Tipo de Conversión");
            Controls.Add(typeCard);
            cmbType = new ComboBox
            {
                Location = new Point(15, 42),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            typeCard.Controls.Add(cmbType);
            UpdateTypes();

            // Bloque 3: Valor + resultado (verde)
            var valueCard = CrearTarjeta(new Rectangle(20, 255, 660, 160), Color.White, Color.FromArgb(16, 185, 129), "✏️ Ingrese el Valor");
            Controls.Add(valueCard);

            txtValue = new TextBox
            {
                Location = new Point(15, 42),
                Width = 200,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtValue.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    e.Handled = true;
            };
            valueCard.Controls.Add(txtValue);

            btnConvert = new Button
            {
                Text = "CONVERTIR",
                Location = new Point(230, 42),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(20, 184, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnConvert.FlatAppearance.BorderSize = 0;
            btnConvert.Click += async (s, e) => await Convertir();
            valueCard.Controls.Add(btnConvert);

            lblResult = new Label
            {
                Text = "Resultado: ---",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 118, 110),
                Location = new Point(15, 95),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            valueCard.Controls.Add(lblResult);

            // FAB limpiar
            fabClear = new Button
            {
                Text = "🗑️",
                Font = new Font("Segoe UI", 16),
                Size = new Size(50, 50),
                Location = new Point(ClientSize.Width - 70, ClientSize.Height - 70),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(249, 115, 22),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            fabClear.FlatAppearance.BorderSize = 0;
            fabClear.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 50, 50, 25, 25));
            fabClear.Click += (s, e) =>
            {
                txtValue.Clear();
                lblResult.Text = "Resultado: ---";
            };
            Controls.Add(fabClear);
            fabClear.BringToFront();

            // Créditos
            lblCredits = new Label
            {
                Text = "👾 Hecho por: Ariel R. y Anthony V.",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                AutoSize = true,
                Location = new Point((ClientSize.Width - 250) / 2, ClientSize.Height - 25),
                BackColor = Color.Transparent
            };
            Controls.Add(lblCredits);
        }

        private void UpdateTypes()
        {
            if (cmbCategory.SelectedItem is string cat && _conversions.TryGetValue(cat, out var items))
            {
                cmbType.DataSource = items;
                cmbType.DisplayMember = "Text";
                cmbType.ValueMember = "TypeKey";
            }
        }

        private async Task Convertir()
        {
            if (cmbType.SelectedItem is not ConvItem item)
            {
                MessageBox.Show("Seleccione un tipo de conversión.");
                return;
            }
            if (!double.TryParse(txtValue.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                MessageBox.Show("Ingrese un número válido.");
                return;
            }

            btnConvert.Enabled = false;
            btnConvert.Text = "CONVIRTIENDO...";
            try
            {
                var req = new ConversionRequest { Type = item.TypeKey, Value = val };
                var res = await _controller.ConvertAsync(req);
                if (res.Error != null)
                {
                    lblResult.Text = $"Error: {res.Error}";
                }
                else
                {
                    lblResult.Text = $"{val:F4} {item.From} → {res.Value:F4} {item.To}";
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnConvert.Enabled = true;
                btnConvert.Text = "CONVERTIR";
            }
        }

        private Panel CrearTarjeta(Rectangle bounds, Color back, Color headerColor, string headerText)
        {
            Panel pnl = new Panel
            {
                Location = bounds.Location,
                Size = bounds.Size,
                BackColor = back,
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, bounds.Width, bounds.Height, 20, 20))
            };
            Panel header = new Panel
            {
                Size = new Size(bounds.Width, 30),
                Location = new Point(0, 0),
                BackColor = headerColor
            };
            header.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, bounds.Width, 30, 20, 20));
            header.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Font f = new Font("Segoe UI", 10, FontStyle.Bold))
                using (Brush b = new SolidBrush(Color.White))
                    e.Graphics.DrawString(headerText, f, b, new PointF(15, 6));
            };
            pnl.Controls.Add(header);
            return pnl;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private class ConvItem
        {
            public string Text { get; }
            public string TypeKey { get; }
            public string From { get; }
            public string To { get; }

            public ConvItem(string text, string typeKey, string from, string to)
            {
                Text = text; TypeKey = typeKey; From = from; To = to;
            }
            public override string ToString() => Text;
        }
    }
}