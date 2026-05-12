using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CLIESC_CONVUNI_DOTNET_GR69.Views
{
    /// <summary>
    /// Controles y utilidades de estilo para el cliente de escritorio.
    /// </summary>
    public static class UITheme
    {
        // Paleta base (match maqueta)
        public static readonly Color BG_A = Color.FromArgb(126, 76, 229);  // #7E4CE5
        public static readonly Color BG_B = Color.FromArgb(196, 118, 255); // #C476FF

        public static readonly Color CARD_TOP = Color.FromArgb(175, 117, 255); // #AF75FF
        public static readonly Color CARD_BOTTOM = Color.FromArgb(229, 140, 236); // #E58CEC

        public static readonly Color BTN_A = Color.FromArgb(90, 124, 255);  // #5A7CFF
        public static readonly Color BTN_B = Color.FromArgb(138, 99, 232);  // #8A63E8

        // ============================================================
        // Fondo con gradiente
        // ============================================================
        public class GradientPanel : Panel
        {
            public Color Color1 { get; set; } = BG_A;
            public Color Color2 { get; set; } = BG_B;

            public GradientPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using var lg = new LinearGradientBrush(ClientRectangle, Color1, Color2, 45f);
                g.FillRectangle(lg, ClientRectangle);

                // Vignette muy sutil
                using var vignette = new LinearGradientBrush(
                    new Rectangle(0, 0, Width, Height),
                    Color.FromArgb(40, 0, 0, 0),
                    Color.FromArgb(0, 0, 0, 0),
                    90f);
                g.FillRectangle(vignette, ClientRectangle);
            }
        }

        // ============================================================
        // Tarjeta redondeada con gradiente, sombra y Region
        // ============================================================
        public class CardPanel : Panel
        {
            public int CornerRadius { get; set; } = 26;
            public Color TopColor { get; set; } = CARD_TOP;
            public Color BottomColor { get; set; } = CARD_BOTTOM;

            public CardPanel()
            {
                BackColor = Color.Transparent;
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
                Region?.Dispose();
                Region = new Region(path); // evita “puntas” blancas
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                int r = CornerRadius;

                using var path = RoundedRect(rect, r);

                // sombra
                using (var shadowPath = RoundedRect(new Rectangle(4, 10, rect.Width, rect.Height), r))
                using (var shadow = new SolidBrush(Color.FromArgb(70, 85, 44, 132)))
                    g.FillPath(shadow, shadowPath);

                // cuerpo
                using (var lg = new LinearGradientBrush(rect, TopColor, BottomColor, 90f))
                    g.FillPath(lg, path);

                // brillo superior sutil
                var top = new Rectangle(0, 0, rect.Width, rect.Height / 2);
                using (var gloss = new LinearGradientBrush(top,
                    Color.FromArgb(40, Color.White), Color.FromArgb(0, Color.White), 90f))
                {
                    g.SetClip(path);
                    g.FillRectangle(gloss, top);
                    g.ResetClip();
                }

                // borde tenue
                using var pen = new Pen(Color.FromArgb(110, 255, 255, 255), 1f);
                g.DrawPath(pen, path);
            }

            internal static GraphicsPath RoundedRect(Rectangle r, int radius)
            {
                int d = radius * 2;
                var p = new GraphicsPath();
                p.AddArc(r.X, r.Y, d, d, 180, 90);
                p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }
        }

        // ============================================================
        // Botón con gradiente, sin “línea” base, región redondeada
        // ============================================================
        public class GradientButton : Button
        {
            public int CornerRadius { get; set; } = 12;
            public Color Color1 { get; set; } = BTN_A;
            public Color Color2 { get; set; } = BTN_B;
            public bool Hover { get; private set; }

            public GradientButton()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
                ForeColor = Color.White;
                Cursor = Cursors.Hand;
                DoubleBuffered = true;

                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);

                MouseEnter += (_, __) => { Hover = true; Invalidate(); };
                MouseLeave += (_, __) => { Hover = false; Invalidate(); };
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using var path = CardPanel.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
                Region?.Dispose();
                Region = new Region(path);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using var path = CardPanel.RoundedRect(rect, CornerRadius);

                Color c1 = Hover ? Lighten(Color1, 0.08f) : Color1;
                Color c2 = Hover ? Lighten(Color2, 0.08f) : Color2;

                using (var lg = new LinearGradientBrush(rect, c1, c2, 0f))
                    g.FillPath(lg, path);

                // brillo arriba
                var top = new Rectangle(0, 0, rect.Width, rect.Height / 2);
                using (var gloss = new LinearGradientBrush(top,
                    Color.FromArgb(40, Color.White), Color.FromArgb(0, Color.White), 90f))
                {
                    g.SetClip(path);
                    g.FillRectangle(gloss, top);
                    g.ResetClip();
                }

                // borde tenue
                using var pen = new Pen(Color.FromArgb(70, 255, 255, 255), 1f);
                g.DrawPath(pen, path);

                TextRenderer.DrawText(
                    g, Text, Font, rect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // ============================================================
        // TextBox con placeholder real (EM_SETCUEBANNER)
        // ============================================================
        public class PlaceholderTextBox : TextBox
        {
            public string? Placeholder { get; set; }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                UpdateCue();
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                UpdateCue();
            }

            protected override void OnEnter(EventArgs e)
            {
                base.OnEnter(e);
                UpdateCue();
            }

            protected override void OnLeave(EventArgs e)
            {
                base.OnLeave(e);
                UpdateCue();
            }

            private void UpdateCue()
            {
                if (!IsHandleCreated) return;
                SendMessage(Handle, EM_SETCUEBANNER, (IntPtr)1, Placeholder ?? string.Empty);
            }

            private const int EM_SETCUEBANNER = 0x1501;

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        }

        // ============================================================
        // Avatar circular SIN aro (evita “doble borde”), con sombra
        // ============================================================
        public static void MakeCircleAvatar(PictureBox pb, bool drawRing = false)
        {
            pb.BackColor = Color.Transparent;
            pb.BorderStyle = BorderStyle.None;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.Padding = Padding.Empty;
            pb.Margin = Padding.Empty;

            void ApplyRegion()
            {
                if (pb.Width <= 0 || pb.Height <= 0) return;
                var rect = new Rectangle(0, 0, pb.Width - 1, pb.Height - 1);
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(rect);
                pb.Region?.Dispose();
                pb.Region = new Region(path);
            }

            // Región circular siempre actualizada
            pb.Resize += (_, __) => ApplyRegion();
            ApplyRegion();

            // Si no quieres aro, NO pintamos nada extra
            pb.Paint += (_, e) =>
            {
                if (!drawRing) return; // <- sin aro ni sombra

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var ring = new Pen(Color.FromArgb(240, 255, 255, 255), 6f)
                {
                    Alignment = System.Drawing.Drawing2D.PenAlignment.Inset
                };
                e.Graphics.DrawEllipse(ring, new Rectangle(0, 0, pb.Width - 1, pb.Height - 1));
            };
        }


        // ============================================================
        // Helpers
        // ============================================================
        private static Color Lighten(Color c, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            int r = (int)(c.R + (255 - c.R) * amount);
            int g = (int)(c.G + (255 - c.G) * amount);
            int b = (int)(c.B + (255 - c.B) * amount);
            return Color.FromArgb(c.A, r, g, b);
        }
    }
}
