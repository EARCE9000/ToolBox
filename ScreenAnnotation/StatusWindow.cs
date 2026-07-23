using System.Diagnostics;
using System.Reflection;

namespace ScreenAnnotation
{
    public enum StatusType
    {
        Away,      // 只今、離席中
        BreakTime, // Break Time
        LunchBreak // Lunch Break
    }

    public partial class StatusWindow : Form
    {
        private Screen? targetScreen;
        private StatusType statusType;
        private Rectangle statusTextBounds = Rectangle.Empty;
        private Rectangle closeButtonBounds = Rectangle.Empty;
        private Image? escapeButtonIcon;

        public StatusWindow(Screen? screen = null, StatusType type = StatusType.Away)
        {
            InitializeComponent();

            targetScreen = screen;
            statusType = type;

            // Load escape button icon from embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.EscapeButton.png"))
            {
                if (stream != null)
                {
                    escapeButtonIcon = Image.FromStream(stream);
                }
            }

            this.Text = "フッ";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(230, 220, 200);  // Beige color
            this.DoubleBuffered = true;

            // Allow keyboard input to close
            this.KeyPreview = true;
            this.KeyDown += StatusWindow_KeyDown;

            // Allow mouse click to change status or close
            this.MouseClick += StatusWindow_MouseClick;
            this.Cursor = Cursors.Hand;

            // Ensure correct positioning when form is shown
            this.Shown += StatusWindow_Shown;

            // Position on target screen
            PositionOnTargetScreen();
        }

        private void StatusWindow_Shown(object? sender, EventArgs e)
        {
            // Re-apply position after form is shown to ensure correct screen placement
            PositionOnTargetScreen();
            this.Invalidate();
        }

        private void PositionOnTargetScreen()
        {
            if (targetScreen != null)
            {
                // Set location to target screen's working area
                this.Bounds = targetScreen.WorkingArea;
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                // Fallback to primary screen
                Screen primaryScreen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
                this.Bounds = primaryScreen.WorkingArea;
                this.WindowState = FormWindowState.Normal;
            }
        }

        private string GetStatusText()
        {
            return statusType switch
            {
                StatusType.BreakTime => "Break Time",
                StatusType.LunchBreak => "Lunch Break",
                _ => "只今、離席中"
            };
        }

        private void CycleStatusType()
        {
            statusType = statusType switch
            {
                StatusType.Away => StatusType.BreakTime,
                StatusType.BreakTime => StatusType.LunchBreak,
                _ => StatusType.Away
            };
            this.Invalidate();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "StatusWindow";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Beige background
            using (var brush = new SolidBrush(Color.FromArgb(230, 220, 200)))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            // Font for text
            var statusFont = new Font("Segoe UI", 60, FontStyle.Bold);
            var labelFont = new Font("Segoe UI", 20, FontStyle.Regular);
            var timeFont = new Font("Segoe UI", 48, FontStyle.Bold);

            // Text color
            var brownColor = Color.FromArgb(100, 80, 50);
            var orangeColor = Color.FromArgb(200, 100, 50);

            // Draw status text (e.g., 只今、離席中) - clickable
            string statusText = GetStatusText();
            SizeF statusSize = e.Graphics.MeasureString(statusText, statusFont);
            PointF statusPoint = new PointF(60, 150);
            e.Graphics.DrawString(statusText, statusFont, new SolidBrush(brownColor), statusPoint);
            statusTextBounds = new Rectangle((int)statusPoint.X, (int)statusPoint.Y, (int)statusSize.Width, (int)statusSize.Height);

            // Draw current time section
            e.Graphics.DrawString("現在時刻", labelFont, new SolidBrush(brownColor), new PointF(60, 320));
            string currentTime = DateTime.Now.ToString("HH:mm");
            e.Graphics.DrawString(currentTime, timeFont, new SolidBrush(brownColor), new PointF(60, 360));

            // Draw resume time section
            e.Graphics.DrawString("再開時間", labelFont, new SolidBrush(orangeColor), new PointF(350, 320));
            string resumeTime = DateTime.Now.AddMinutes(30).ToString("HH:mm");
            e.Graphics.DrawString(resumeTime, timeFont, new SolidBrush(orangeColor), new PointF(350, 360));

            // Draw close button (top right)
            int buttonSize = 50;
            int buttonX = this.ClientSize.Width - buttonSize - 20;
            int buttonY = 20;
            closeButtonBounds = new Rectangle(buttonX, buttonY, buttonSize, buttonSize);

            // Draw close button - use image if available, otherwise draw X mark
            if (escapeButtonIcon != null)
            {
                // Draw the escape button image
                e.Graphics.DrawImage(escapeButtonIcon, closeButtonBounds);
            }
            else
            {
                // Fallback: Draw button with background
                using (var buttonBrush = new SolidBrush(Color.FromArgb(200, 100, 50)))
                {
                    e.Graphics.FillRectangle(buttonBrush, closeButtonBounds);
                }

                // Draw button border
                using (var buttonPen = new Pen(Color.FromArgb(150, 70, 30), 2))
                {
                    e.Graphics.DrawRectangle(buttonPen, closeButtonBounds);
                }

                // Draw X mark on button
                using (var xPen = new Pen(Color.White, 3))
                {
                    int margin = 10;
                    e.Graphics.DrawLine(xPen, buttonX + margin, buttonY + margin, buttonX + buttonSize - margin, buttonY + buttonSize - margin);
                    e.Graphics.DrawLine(xPen, buttonX + buttonSize - margin, buttonY + margin, buttonX + margin, buttonY + buttonSize - margin);
                }
            }

            // Draw clock icon
            DrawClockIcon(e.Graphics);

            // Cleanup
            statusFont.Dispose();
            labelFont.Dispose();
            timeFont.Dispose();
        }

        private void DrawClockIcon(Graphics g)
        {
            // Draw large circle with light green background
            int centerX = this.ClientSize.Width - 200;
            int centerY = this.ClientSize.Height / 2 - 50;
            int radius = 120;

            // Outer light green circle
            using (var brush = new SolidBrush(Color.FromArgb(200, 230, 200)))
            {
                g.FillEllipse(brush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // Inner beige circle
            int innerRadius = 100;
            using (var brush = new SolidBrush(Color.FromArgb(230, 220, 200)))
            {
                g.FillEllipse(brush, centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2);
            }

            // Draw clock markers and hands
            using (var pen = new Pen(Color.FromArgb(100, 120, 80), 2))
            {
                // Draw hour markers
                for (int i = 0; i < 12; i++)
                {
                    double angle = (i * 30 - 90) * Math.PI / 180;
                    int x1 = (int)(centerX + (innerRadius - 15) * Math.Cos(angle));
                    int y1 = (int)(centerY + (innerRadius - 15) * Math.Sin(angle));
                    int x2 = (int)(centerX + (innerRadius - 5) * Math.Cos(angle));
                    int y2 = (int)(centerY + (innerRadius - 5) * Math.Sin(angle));
                    g.DrawLine(pen, x1, y1, x2, y2);
                }

                // Draw clock hands
                DateTime now = DateTime.Now;
                double hourAngle = ((now.Hour % 12) * 30 + now.Minute * 0.5 - 90) * Math.PI / 180;
                double minuteAngle = (now.Minute * 6 - 90) * Math.PI / 180;

                // Hour hand
                int hx = (int)(centerX + 60 * Math.Cos(hourAngle));
                int hy = (int)(centerY + 60 * Math.Sin(hourAngle));
                g.DrawLine(pen, centerX, centerY, hx, hy);

                // Minute hand
                int mx = (int)(centerX + 80 * Math.Cos(minuteAngle));
                int my = (int)(centerY + 80 * Math.Sin(minuteAngle));
                g.DrawLine(pen, centerX, centerY, mx, my);
            }

            // Draw center dot
            using (var brush = new SolidBrush(Color.FromArgb(100, 120, 80)))
            {
                g.FillEllipse(brush, centerX - 4, centerY - 4, 8, 8);
            }
        }

        private void StatusWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            // Close on Esc
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void StatusWindow_MouseClick(object? sender, MouseEventArgs e)
        {
            // Check if clicked on status text - cycle status type
            if (statusTextBounds.Contains(e.Location))
            {
                CycleStatusType();
                return;
            }

            // Check if clicked on close button
            if (closeButtonBounds.Contains(e.Location))
            {
                this.Close();
                return;
            }
        }
    }
}
