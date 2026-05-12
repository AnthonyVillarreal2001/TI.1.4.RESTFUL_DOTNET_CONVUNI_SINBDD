using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CLIESC_CONVUNI_DOTNET_GR69.Controllers;

namespace CLIESC_CONVUNI_DOTNET_GR69.Views
{
    public partial class LoginForm : Form
    {
        private Panel card;
        private TextBox txtUsuario, txtPassword;
        private Button btnLogin;
        private Label lblCredits;
        private ConversionController _controller;

        // Ruta a la imagen de fondo
        private readonly string _bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", "sullivan.jpg");

        public LoginForm()
        {
            _controller = new ConversionController();
            InitializeDesign();
        }

        private void InitializeDesign()
        {
            this.Text = "Conversor Sullivan – Login";
            this.ClientSize = new Size(500, 420);
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
                this.BackColor = Color.FromArgb(91, 44, 142); // púrpura oscuro
            }

            // Tarjeta blanca semitransparente
            card = new Panel
            {
                Size = new Size(340, 300),
                Location = new Point((ClientSize.Width - 340) / 2, 40),
                BackColor = Color.FromArgb(240, 255, 255, 255), // 95% opacidad
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 340, 300, 20, 20))
            };
            Controls.Add(card);

            // Icono puerta (🚪)
            var icono = new Label
            {
                Text = "🚪",
                Font = new Font("Segoe UI", 24),
                AutoSize = true,
                Location = new Point(150, 15),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(139, 92, 246) // púrpura claro
            };
            card.Controls.Add(icono);

            // Título
            var titulo = new Label
            {
                Text = "¡Bienvenido!",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(91, 44, 142),
                AutoSize = true,
                Location = new Point(85, 50),
                BackColor = Color.Transparent
            };
            card.Controls.Add(titulo);

            // Caja Usuario
            var borderUser = CreaBordeTextBox(new Point(60, 95));
            card.Controls.Add(borderUser);
            txtUsuario = CreaTextBoxDentro(borderUser, "Usuario");

            // Caja Password
            var borderPass = CreaBordeTextBox(new Point(60, 135));
            card.Controls.Add(borderPass);
            txtPassword = CreaTextBoxDentro(borderPass, "Contraseña");
            txtPassword.UseSystemPasswordChar = true;

            // Botón INGRESAR con degradado
            btnLogin = new GradientButton2
            {
                Text = "INGRESAR",
                Location = new Point(105, 180),
                Size = new Size(130, 40),
                Color1 = Color.FromArgb(139, 92, 246),   // púrpura claro
                Color2 = Color.FromArgb(20, 184, 166)    // verde azulado
            };
            btnLogin.Click += async (s, e) => await OnLoginClick();
            card.Controls.Add(btnLogin);

            // Créditos (fuera de la tarjeta)
            lblCredits = new Label
            {
                Text = "👾 Hecho por: Ariel R. y Anthony V.",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                AutoSize = true,
                Location = new Point((ClientSize.Width - 250) / 2, ClientSize.Height - 35),
                BackColor = Color.Transparent
            };
            Controls.Add(lblCredits);

            this.AcceptButton = btnLogin;
        }

        private async Task OnLoginClick()
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Completa ambos campos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "INGRESANDO...";
            try
            {
                var loginResult = await _controller.LoginAsync(user, pass);
                if (loginResult.Success)
                {
                    // Token (si lo hubiera) se podría guardar, aquí no es necesario.
                    var main = new MainForm(user, _controller);
                    main.Show();
                    this.Hide();
                    // Limpiar campos
                    txtUsuario.Text = "Usuario";
                    txtPassword.Text = "Contraseña";
                }
                else
                {
                    MessageBox.Show(loginResult.Message ?? "Credenciales incorrectas", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "INGRESAR";
                txtPassword.Text = "";
            }
        }

        // --- Métodos auxiliares de bordes redondeados ---
        private Panel CreaBordeTextBox(Point location)
        {
            return new Panel
            {
                Size = new Size(220, 30),
                Location = location,
                BackColor = Color.FromArgb(139, 92, 246), // borde púrpura claro
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 220, 30, 10, 10))
            };
        }

        private TextBox CreaTextBoxDentro(Panel border, string placeholder)
        {
            var tb = new TextBox
            {
                Location = new Point(2, 2),
                Size = new Size(216, 26),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            tb.GotFocus += (s, e) => { if (tb.Text == placeholder) tb.Text = ""; };
            tb.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = placeholder; };
            tb.Text = placeholder;
            border.Controls.Add(tb);
            return tb;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }

    // Botón degradado simple
    public class GradientButton2 : Button
    {
        public Color Color1 { get; set; } = Color.FromArgb(139, 92, 246);
        public Color Color2 { get; set; } = Color.FromArgb(20, 184, 166);

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var lg = new LinearGradientBrush(rect, Color1, Color2, LinearGradientMode.Horizontal))
            using (var path = RoundedRect(rect, 12))
            {
                pevent.Graphics.FillPath(lg, path);
            }
            TextRenderer.DrawText(pevent.Graphics, Text, Font, rect, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}