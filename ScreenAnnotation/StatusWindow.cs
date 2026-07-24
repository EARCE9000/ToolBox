using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace ScreenAnnotation
{
    public enum StatusType
    {
        Away,      // 只今、離席中
        BreakTime, // Break Time
        LunchBreak // Lunch Break
    }

    public enum ResumeTimeAdjustDirection
    {
        None,
        Up,
        Down
    }

    public partial class StatusWindow : Form
    {
        private Screen? targetScreen;
        private StatusType statusType;
        private Rectangle statusTextBounds = Rectangle.Empty;
        private Rectangle closeButtonBounds = Rectangle.Empty;
        private Rectangle resumeTimeBounds = Rectangle.Empty;
        private Rectangle resumeTimeUpButtonBounds = Rectangle.Empty;
        private Rectangle resumeTimeDownButtonBounds = Rectangle.Empty;
        private Image? escapeButtonIcon;
        private Image? countUpButtonIcon;
        private Image? countDownButtonIcon;
        private Image? clockBackground;
        private Image? clockHourHand;
        private Image? clockMinuteHand;
        private Image? clockSecondHand;
        private System.Windows.Forms.Timer? clockTimer;
        private System.Windows.Forms.Timer? resumeTimeHoldTimer;
        private DateTime resumeTime;
        private DateTime holdStartTime;
        private bool isLongPressActive;
        private ResumeTimeAdjustDirection holdDirection = ResumeTimeAdjustDirection.None;


        // Constants for DPI scaling (FHD baseline: 1920x1080)
        private const float BASELINE_DPI = 96f; // Standard DPI
        private const float FHD_WIDTH = 1920f;
        private const float FHD_HEIGHT = 1080f;

        public StatusWindow(Screen? screen = null, StatusType type = StatusType.Away)
        {
            InitializeComponent();

            targetScreen = screen;
            statusType = type;
            // Set default resume time to 15 minutes from now
            resumeTime = DateTime.Now.AddMinutes(15);

            // Load escape button icon from embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.EscapeButton.png"))
            {
                if (stream != null)
                {
                    escapeButtonIcon = Image.FromStream(stream);
                }
            }

            LoadClockImage("ScreenAnnotation.CountUpButton.png", out countUpButtonIcon);
            LoadClockImage("ScreenAnnotation.CountDownButton.png", out countDownButtonIcon);

            // Load clock images from embedded resources
            LoadClockImage("ScreenAnnotation.Clock_BackGround.png", out clockBackground);
            LoadClockImage("ScreenAnnotation.Clock_Hour.png", out clockHourHand);
            LoadClockImage("ScreenAnnotation.Clock_Minite.png", out clockMinuteHand);
            LoadClockImage("ScreenAnnotation.Clock_Second.png", out clockSecondHand);

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
            this.MouseDown += StatusWindow_MouseDown;
            this.MouseUp += StatusWindow_MouseUp;
            this.MouseLeave += StatusWindow_MouseLeave;
            this.Cursor = Cursors.Hand;

            // Setup timer for clock animation (update every 100ms for smooth second hand movement)
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 100; // 100ms for smooth animation
            clockTimer.Tick += (sender, e) => this.Invalidate();
            clockTimer.Start();

            resumeTimeHoldTimer = new System.Windows.Forms.Timer();
            resumeTimeHoldTimer.Interval = 120;
            resumeTimeHoldTimer.Tick += ResumeTimeHoldTimer_Tick;

            // Initialize UI controls
            InitializeControls();

            // Ensure correct positioning when form is shown
            this.Shown += StatusWindow_Shown;

            // Position on target screen
            PositionOnTargetScreen();
        }

        private void InitializeControls()
        {
            // Update time every second via Invalidate for OnPaint rendering
            var timeUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            timeUpdateTimer.Tick += (sender, e) =>
            {
                this.Invalidate();
            };
            timeUpdateTimer.Start();
        }

        private float PixelsToPoints(float pixelSize, float scaleX, float scaleY)
        {
            // Convert pixels to points: Use average of X and Y scale for responsive sizing
            // Points = Pixels × (DPI / 96) where DPI = 96 is the standard baseline
            // For scaled coordinate system: pixelSize × scale ÷ 1.333 (72/96)
            float scale = (scaleX + scaleY) / 2f;
            return (pixelSize * scale) / 1.333f;
        }

        private void StatusWindow_Shown(object? sender, EventArgs e)
        {
            // Re-apply position after form is shown to ensure correct screen placement
            PositionOnTargetScreen();
            this.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Stop timer on close
            if (clockTimer != null)
            {
                clockTimer.Stop();
                clockTimer.Dispose();
            }
            if (resumeTimeHoldTimer != null)
            {
                resumeTimeHoldTimer.Stop();
                resumeTimeHoldTimer.Dispose();
            }
            // Dispose images
            escapeButtonIcon?.Dispose();
            countUpButtonIcon?.Dispose();
            countDownButtonIcon?.Dispose();
            clockBackground?.Dispose();
            clockHourHand?.Dispose();
            clockMinuteHand?.Dispose();
            clockSecondHand?.Dispose();
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
                StatusType.BreakTime => GetSettingOrDefault(Properties.Settings.Default.SleepWord2, "Break Time"),
                StatusType.LunchBreak => GetSettingOrDefault(Properties.Settings.Default.SleepWord3, "Lunch Break"),
                _ => GetSettingOrDefault(Properties.Settings.Default.SleepWord1, "只今、離席中")
            };
        }

        private static string GetSettingOrDefault(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
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

            // Draw beige background
            using (var brush = new SolidBrush(Color.FromArgb(230, 220, 200)))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            // Calculate positions and sizes
            float scaleX = this.ClientSize.Width / FHD_WIDTH;
            float scaleY = this.ClientSize.Height / FHD_HEIGHT;
            float statusFontSize = PixelsToPoints(72, scaleX, scaleY);
            float labelFontSize = PixelsToPoints(22, scaleX, scaleY);
            float timeFontSize = PixelsToPoints(48, scaleX, scaleY);

            // Draw status text (只今、離席中) at X:10%, Y:30%
            int statusLeft = (int)(this.ClientSize.Width * 0.1);
            int statusTop = (int)(this.ClientSize.Height * 0.3);
            using (var font = new Font("Segoe UI", statusFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(100, 80, 50)))
            {
                e.Graphics.DrawString(GetStatusText(), font, brush, statusLeft, statusTop);
            }
            statusTextBounds = new Rectangle(statusLeft, statusTop, (int)(this.ClientSize.Width * 0.5), (int)(this.ClientSize.Height * 0.2));

            // Draw current time label (現在時刻) at X:10%, Y:68%
            int currentTimeLabelLeft = (int)(this.ClientSize.Width * 0.1);
            int currentTimeLabelTop = (int)(this.ClientSize.Height * 0.68);
            using (var font = new Font("Segoe UI", labelFontSize, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(100, 80, 50)))
            {
                e.Graphics.DrawString("現在時刻", font, brush, currentTimeLabelLeft, currentTimeLabelTop);
            }

            // Draw current time value (HH:mm) at X:10%, Y:72%
            int currentTimeLeft = (int)(this.ClientSize.Width * 0.1);
            int currentTimeTop = (int)(this.ClientSize.Height * 0.72);
            using (var font = new Font("Segoe UI", timeFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(100, 80, 50)))
            {
                e.Graphics.DrawString(DateTime.Now.ToString("HH:mm"), font, brush, currentTimeLeft, currentTimeTop);
            }

            // Draw resume time label (再開時刻) at X:25%, Y:68%
            int resumeTimeLabelLeft = (int)(this.ClientSize.Width * 0.25);
            int resumeTimeLabelTop = (int)(this.ClientSize.Height * 0.68);
            using (var font = new Font("Segoe UI", labelFontSize, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(200, 100, 50)))
            {
                e.Graphics.DrawString("再開時刻", font, brush, resumeTimeLabelLeft, resumeTimeLabelTop);
            }

            // Draw resume time value (HH:mm) at X:25%, Y:72%
            int resumeTimeLeft = (int)(this.ClientSize.Width * 0.25);
            int resumeTimeTop = (int)(this.ClientSize.Height * 0.72);
            string resumeText = resumeTime.ToString("HH:mm");
            SizeF resumeTextSize;
            using (var font = new Font("Segoe UI", timeFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(200, 100, 50)))
            {
                e.Graphics.DrawString(resumeText, font, brush, resumeTimeLeft, resumeTimeTop);
                resumeTextSize = e.Graphics.MeasureString(resumeText, font);
            }

            int resumeTextWidth = (int)Math.Ceiling(resumeTextSize.Width);
            int resumeTextHeight = (int)Math.Ceiling(resumeTextSize.Height);
            resumeTimeBounds = new Rectangle(resumeTimeLeft, resumeTimeTop, resumeTextWidth, resumeTextHeight);

            int spinnerGap = 6;
            int spinnerLeft = resumeTimeBounds.Right + spinnerGap;
            int spinnerTotalHeight = Math.Max(23, (resumeTextHeight - 12) / 2);
            int spinnerTop = resumeTimeTop + Math.Max(0, (resumeTextHeight - spinnerTotalHeight) / 2);
            int spinnerWidth = Math.Max(13, (int)(spinnerTotalHeight * 0.8f));
            int upperHeight = spinnerTotalHeight / 2;
            int lowerHeight = spinnerTotalHeight - upperHeight;

            resumeTimeUpButtonBounds = new Rectangle(spinnerLeft, spinnerTop, spinnerWidth, upperHeight);
            resumeTimeDownButtonBounds = new Rectangle(spinnerLeft, spinnerTop + upperHeight, spinnerWidth, lowerHeight);

            DrawResumeAdjustSpinner(e.Graphics);

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

            // Draw clock icon - right side, centered
            DrawClockIcon(e.Graphics);
        }

        private void DrawClockIcon(Graphics g)
        {
            // Position for clock on the right side - centered at 3/4 on X-axis, 1/2 on Y-axis
            int clockCenterX = (int)(this.ClientSize.Width * 0.75);
            int clockCenterY = this.ClientSize.Height / 2;
            const int clockSize = 640; // Fixed size 640px

            if (clockBackground != null)
            {
                // Draw clock background
                g.DrawImage(clockBackground, clockCenterX - clockSize / 2, clockCenterY - clockSize / 2, clockSize, clockSize);
            }

            // Draw clock hands with rotation
            DateTime now = DateTime.Now;

            // Hour hand - rotates 30 degrees per hour + 0.5 degrees per minute + milliseconds contribution
            double hourAngle = (now.Hour % 12) * 30.0 + now.Minute * 0.5 + now.Second * (0.5 / 60.0) + now.Millisecond * (0.5 / 60000.0);
            DrawRotatedImage(g, clockHourHand, clockCenterX, clockCenterY, clockSize, hourAngle);

            // Minute hand - rotates 6 degrees per minute + milliseconds contribution
            double minuteAngle = now.Minute * 6.0 + now.Second * 0.1 + now.Millisecond * (0.1 / 1000.0);
            DrawRotatedImage(g, clockMinuteHand, clockCenterX, clockCenterY, clockSize, minuteAngle);

            // Second hand - rotates 6 degrees per second + milliseconds contribution (smooth movement)
            double secondAngle = now.Second * 6.0 + now.Millisecond * (6.0 / 1000.0);
            DrawRotatedImage(g, clockSecondHand, clockCenterX, clockCenterY, clockSize, secondAngle);
        }

        private void DrawRotatedImage(Graphics g, Image? image, int centerX, int centerY, int size, double angleInDegrees)
        {
            if (image == null) return;

            // Save the current graphics state
            var state = g.Save();

            // Translate to center point
            g.TranslateTransform(centerX, centerY);

            // Rotate by the specified angle (clockwise, so negative)
            g.RotateTransform((float)angleInDegrees);

            // Draw the image centered at the origin, then translate back
            g.DrawImage(image, -size / 2, -size / 2, size, size);

            // Restore the graphics state
            g.Restore(state);
        }

        private void LoadClockImage(string resourceName, out Image? image)
        {
            image = null;
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        image = Image.FromStream(stream);
                    }
                }
            }
            catch
            {
                // Resource not found or failed to load, image remains null
            }
        }

        private void DrawResumeAdjustSpinner(Graphics g)
        {
            Rectangle spinnerBounds = Rectangle.Union(resumeTimeUpButtonBounds, resumeTimeDownButtonBounds);

            using (var buttonBrush = new SolidBrush(Color.FromArgb(245, 245, 245)))
            {
                g.FillRectangle(buttonBrush, spinnerBounds);
            }

            using (var borderPen = new Pen(Color.FromArgb(150, 150, 150), 1))
            {
                g.DrawRectangle(borderPen, spinnerBounds);
                g.DrawLine(borderPen, spinnerBounds.Left, resumeTimeDownButtonBounds.Top, spinnerBounds.Right - 1, resumeTimeDownButtonBounds.Top);
            }

            DrawResumeAdjustGlyph(g, resumeTimeUpButtonBounds, countUpButtonIcon, true);
            DrawResumeAdjustGlyph(g, resumeTimeDownButtonBounds, countDownButtonIcon, false);
        }

        private void DrawResumeAdjustGlyph(Graphics g, Rectangle bounds, Image? image, bool isUp)
        {
            _ = image;

            int centerX = bounds.Left + bounds.Width / 2;
            int centerY = bounds.Top + bounds.Height / 2;
            int triangleHalfWidth = Math.Max(4, bounds.Width / 5);
            int triangleHalfHeight = Math.Max(3, bounds.Height / 6);

            Point[] triangle = isUp
                ?
                [
                    new Point(centerX, centerY - triangleHalfHeight),
                    new Point(centerX - triangleHalfWidth, centerY + triangleHalfHeight),
                    new Point(centerX + triangleHalfWidth, centerY + triangleHalfHeight)
                ]
                :
                [
                    new Point(centerX, centerY + triangleHalfHeight),
                    new Point(centerX - triangleHalfWidth, centerY - triangleHalfHeight),
                    new Point(centerX + triangleHalfWidth, centerY - triangleHalfHeight)
                ];

            using (var triangleBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                g.FillPolygon(triangleBrush, triangle);
            }
        }

        private void AdjustResumeTime(int minutes)
        {
            resumeTime = resumeTime.AddMinutes(minutes);
            this.Invalidate();
        }

        private void ResumeTimeHoldTimer_Tick(object? sender, EventArgs e)
        {
            if (holdDirection == ResumeTimeAdjustDirection.None)
            {
                return;
            }

            if (!isLongPressActive)
            {
                if ((DateTime.Now - holdStartTime).TotalMilliseconds < 450)
                {
                    return;
                }
                isLongPressActive = true;
            }

            AdjustResumeTime(holdDirection == ResumeTimeAdjustDirection.Up ? 5 : -5);
        }

        private void StopResumeTimeHold()
        {
            resumeTimeHoldTimer?.Stop();
            holdDirection = ResumeTimeAdjustDirection.None;
            isLongPressActive = false;
        }

        private void StatusWindow_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (resumeTimeUpButtonBounds.Contains(e.Location))
            {
                holdDirection = ResumeTimeAdjustDirection.Up;
            }
            else if (resumeTimeDownButtonBounds.Contains(e.Location))
            {
                holdDirection = ResumeTimeAdjustDirection.Down;
            }
            else
            {
                return;
            }

            holdStartTime = DateTime.Now;
            isLongPressActive = false;
            resumeTimeHoldTimer?.Start();
        }

        private void StatusWindow_MouseUp(object? sender, MouseEventArgs e)
        {
            if (holdDirection == ResumeTimeAdjustDirection.None)
            {
                return;
            }

            ResumeTimeAdjustDirection direction = holdDirection;
            bool wasLongPress = isLongPressActive;
            StopResumeTimeHold();

            if (wasLongPress)
            {
                return;
            }

            if (direction == ResumeTimeAdjustDirection.Up && resumeTimeUpButtonBounds.Contains(e.Location))
            {
                AdjustResumeTime(1);
            }
            else if (direction == ResumeTimeAdjustDirection.Down && resumeTimeDownButtonBounds.Contains(e.Location))
            {
                AdjustResumeTime(-1);
            }
        }

        private void StatusWindow_MouseLeave(object? sender, EventArgs e)
        {
            StopResumeTimeHold();
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

            // Check if clicked on resume time - allow editing
            if (resumeTimeBounds.Contains(e.Location))
            {
                ShowResumeTimeDialog();
                return;
            }

            // Check if clicked on close button
            if (closeButtonBounds.Contains(e.Location))
            {
                this.Close();
                return;
            }
        }

        private void ShowResumeTimeDialog()
        {
            // Create a simple time picker dialog
            using (var form = new Form())
            {
                form.Text = "再開時間を設定";
                form.Width = 300;
                form.Height = 150;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label() { Text = "時刻 (HH:mm):", Left = 10, Top = 10, Width = 270 };
                var textBox = new TextBox() { Text = resumeTime.ToString("HH:mm"), Left = 10, Top = 35, Width = 270 };
                var okButton = new Button() { Text = "OK", Left = 130, Top = 70, Width = 75, DialogResult = DialogResult.OK };
                var cancelButton = new Button() { Text = "キャンセル", Left = 210, Top = 70, Width = 75, DialogResult = DialogResult.Cancel };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(okButton);
                form.Controls.Add(cancelButton);
                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (DateTime.TryParseExact(textBox.Text, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var time))
                    {
                        resumeTime = DateTime.Now.Date.Add(time.TimeOfDay);
                        this.Invalidate();
                    }
                }
            }
        }
    }
}
