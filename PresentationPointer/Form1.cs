using System.Reflection;

namespace PresentationPointer
{
    public partial class Form1 : Form
    {
        private Image? currentImage;
        private ImageType currentImageType = ImageType.Arrow;
        private int currentSize = 256;
        private int currentRotation = 0;
        private Point imageLocation;
        private Point dragStart;
        private bool isDragging = false;

        private readonly TextBox speechBubbleTextBox = new();
        private string speechBubbleText = string.Empty;
        private bool isSpeechTextConfirmed = false;
        private const int MaxSpeechCharsPerLine = 8;
        private const int MaxSpeechLines = 6;

        public enum ImageType { Arrow, SpeechBubble }

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;

            this.Opacity = 1.0D;
            if (this.Size.Width < 200 || this.Size.Height < 200)
            {
                this.Size = new System.Drawing.Size(300, 300);
            }

            InitializeSpeechBubbleEditor();
            contextMenuStrip.Opening += ContextMenuStrip_Opening;
            UpdateInfoMenuTexts();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // ignore icon load failure
            }

            // Load initial image
            currentImageType = ImageType.Arrow;
            currentSize = 256;
            currentRotation = 0;
            imageLocation = new Point((ClientSize.Width - currentSize) / 2, (ClientSize.Height - currentSize) / 2);

            LoadImage();

            ClientSize = new Size(currentSize, currentSize);
            CenterToScreen();

            // Mouse events for dragging / speech text edit
            MouseDown += Form1_MouseDown;
            MouseMove += Form1_MouseMove;
            MouseUp += Form1_MouseUp;
            MouseDoubleClick += Form1_MouseDoubleClick;

            UpdateContextMenuChecks();
        }

        private void LoadImage()
        {
            currentImage?.Dispose();

            string resourceName = currentImageType == ImageType.Arrow 
                ? "PresentationPointer.ArrowIcon.png"
                : "PresentationPointer.DrawSpeechBubble.png";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception($"Resource '{resourceName}' not found");

                var originalImage = Image.FromStream(stream);
                currentImage = ResizeAndRotateImage(originalImage, currentSize, currentRotation);
            }

            Invalidate();
        }

        private Image ResizeAndRotateImage(Image original, int size, int rotation)
        {
            var resized = new Bitmap(size, size);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, size, size);
            }

            if (rotation == 0)
                return resized;

            var rotated = new Bitmap(size, size);
            using (var g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(size / 2f, size / 2f);
                g.RotateTransform(rotation);
                g.TranslateTransform(-size / 2f, -size / 2f);
                g.DrawImage(resized, 0, 0);
            }

            resized.Dispose();
            return rotated;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (currentImage != null)
            {
                e.Graphics.DrawImage(currentImage, 0, 0, currentSize, currentSize);
            }

        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (speechBubbleTextBox.Visible && !speechBubbleTextBox.Bounds.Contains(e.Location) && !speechBubbleTextBox.ReadOnly)
            {
                HideSpeechBubbleEditor();
                return;
            }

            isDragging = true;
            dragStart = e.Location;
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int offsetX = e.X - dragStart.X;
                int offsetY = e.Y - dragStart.Y;
                Location = new Point(Location.X + offsetX, Location.Y + offsetY);
            }
        }

        private void Form1_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Image_Arrow_Click(object? sender, EventArgs e)
        {
            currentImageType = ImageType.Arrow;
            speechBubbleTextBox.Visible = false;
            LoadImage();
            UpdateContextMenuChecks();
        }

        private void Image_SpeechBubble_Click(object? sender, EventArgs e)
        {
            currentImageType = ImageType.SpeechBubble;
            UpdateSpeechBubbleEditorLayout();
            speechBubbleTextBox.Text = speechBubbleText;
            speechBubbleTextBox.ReadOnly = true;
            speechBubbleTextBox.BackColor = Color.FromArgb(255, 255, 255);
            speechBubbleTextBox.Visible = !string.IsNullOrWhiteSpace(speechBubbleText);
            LoadImage();
            UpdateContextMenuChecks();
        }

        private void Size_128_Click(object? sender, EventArgs e)
        {
            SetSize(128);
        }

        private void Size_256_Click(object? sender, EventArgs e)
        {
            SetSize(256);
        }

        private void Size_512_Click(object? sender, EventArgs e)
        {
            SetSize(512);
        }

        private void SetSize(int size)
        {
            // Center the new size on the old center
            Point oldCenter = new Point(Location.X + currentSize / 2, Location.Y + currentSize / 2);

            currentSize = size;
            LoadImage();

            ClientSize = new Size(currentSize, currentSize);
            Location = new Point(oldCenter.X - currentSize / 2, oldCenter.Y - currentSize / 2);
            UpdateSpeechBubbleEditorLayout();
            UpdateContextMenuChecks();
        }

        private void Rotate_0_Click(object? sender, EventArgs e)
        {
            SetRotation(0);
        }

        private void Rotate_90_Click(object? sender, EventArgs e)
        {
            SetRotation(90);
        }

        private void Rotate_180_Click(object? sender, EventArgs e)
        {
            SetRotation(180);
        }

        private void Rotate_270_Click(object? sender, EventArgs e)
        {
            SetRotation(270);
        }

        private void SetRotation(int rotation)
        {
            currentRotation = rotation;
            LoadImage();
            UpdateSpeechBubbleEditorLayout();
            UpdateContextMenuChecks();
        }

        private void Form1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currentImageType != ImageType.SpeechBubble)
            {
                return;
            }

            isDragging = false;
            ShowSpeechBubbleEditor();
        }

        private void InitializeSpeechBubbleEditor()
        {
            speechBubbleTextBox.Multiline = true;
            speechBubbleTextBox.AcceptsReturn = false;
            speechBubbleTextBox.AcceptsTab = false;
            speechBubbleTextBox.Visible = false;
            speechBubbleTextBox.BorderStyle = BorderStyle.None;
            speechBubbleTextBox.BackColor = Color.FromArgb(245, 245, 245);
            speechBubbleTextBox.ForeColor = Color.FromArgb(0xB2, 0x00, 0x00);
            speechBubbleTextBox.WordWrap = true;
            speechBubbleTextBox.TextChanged += SpeechBubbleTextBox_TextChanged;
            speechBubbleTextBox.Leave += SpeechBubbleTextBox_Leave;
            speechBubbleTextBox.GotFocus += SpeechBubbleTextBox_GotFocus;
            speechBubbleTextBox.MouseDoubleClick += SpeechBubbleTextBox_MouseDoubleClick;
            Controls.Add(speechBubbleTextBox);
        }

        private void ShowSpeechBubbleEditor()
        {
            isSpeechTextConfirmed = false;
            UpdateSpeechBubbleEditorLayout();
            speechBubbleTextBox.Text = speechBubbleText;
            speechBubbleTextBox.ReadOnly = false;
            speechBubbleTextBox.BackColor = Color.FromArgb(245, 245, 245);
            speechBubbleTextBox.Visible = true;
            speechBubbleTextBox.BringToFront();
            speechBubbleTextBox.Focus();
            speechBubbleTextBox.SelectionStart = speechBubbleTextBox.TextLength;
        }

        private void HideSpeechBubbleEditor()
        {
            if (!speechBubbleTextBox.Visible)
            {
                return;
            }

            speechBubbleText = speechBubbleTextBox.Text;
            isSpeechTextConfirmed = !string.IsNullOrWhiteSpace(speechBubbleText);
            speechBubbleTextBox.ReadOnly = true;
            speechBubbleTextBox.BackColor = Color.FromArgb(255, 255, 255);
            speechBubbleTextBox.Visible = isSpeechTextConfirmed;
            speechBubbleTextBox.SelectionLength = 0;
            speechBubbleTextBox.SelectionStart = 0;
            if (ReferenceEquals(ActiveControl, speechBubbleTextBox))
            {
                ActiveControl = null;
                Focus();
            }
            Invalidate();
        }

        private void SpeechBubbleTextBox_Leave(object? sender, EventArgs e)
        {
            HideSpeechBubbleEditor();
        }

        private void SpeechBubbleTextBox_GotFocus(object? sender, EventArgs e)
        {
            if (!speechBubbleTextBox.ReadOnly)
            {
                return;
            }

            ActiveControl = null;
            Focus();
        }

        private void SpeechBubbleTextBox_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currentImageType != ImageType.SpeechBubble || !speechBubbleTextBox.ReadOnly)
            {
                return;
            }

            ShowSpeechBubbleEditor();
        }

        private void SpeechBubbleTextBox_TextChanged(object? sender, EventArgs e)
        {
            string singleLine = speechBubbleTextBox.Text.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
            int maxChars = MaxSpeechCharsPerLine * MaxSpeechLines;

            if (singleLine.Length > maxChars)
            {
                singleLine = singleLine[..maxChars];
            }

            if (speechBubbleTextBox.Text == singleLine)
            {
                return;
            }

            int selectionStart = speechBubbleTextBox.SelectionStart;
            speechBubbleTextBox.Text = singleLine;
            speechBubbleTextBox.SelectionStart = Math.Min(selectionStart, speechBubbleTextBox.TextLength);
        }

        private Rectangle GetSpeechTextBounds()
        {
            if (currentSize <= 128)
            {
                int leftSmall = (int)(currentSize * 0.08);
                int topSmall = (int)(currentSize * 0.13);
                int widthSmall = (int)(currentSize * 0.84);
                int heightSmall = (int)(currentSize * 0.76);
                return new Rectangle(leftSmall, topSmall, widthSmall, heightSmall);
            }

            int left = (int)(currentSize * 0.10);
            int top = (int)(currentSize * 0.14);
            int width = (int)(currentSize * 0.80);
            int height = (int)(currentSize * 0.74);
            return new Rectangle(left, top, width, height);
        }

        private Font CreateSpeechBubbleFont()
        {
            const float baseSize = 512f;
            const float baseFontSize = 60f;

            float fontSize = baseFontSize * (currentSize / baseSize);
            if (currentSize <= 128)
            {
                fontSize *= 0.85f;
            }

            fontSize = Math.Max(10f, fontSize);
            return new Font("MS Gothic", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        private void UpdateSpeechBubbleEditorLayout()
        {
            Rectangle bounds = GetSpeechTextBounds();
            speechBubbleTextBox.Bounds = bounds;
            speechBubbleTextBox.Font = CreateSpeechBubbleFont();
        }

        private void ContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateContextMenuChecks();
            UpdateInfoMenuTexts();
        }

        private void UpdateContextMenuChecks()
        {
            arrowIconToolStripMenuItem.Checked = currentImageType == ImageType.Arrow;
            speechBubbleToolStripMenuItem.Checked = currentImageType == ImageType.SpeechBubble;

            size128ToolStripMenuItem.Checked = currentSize == 128;
            size256ToolStripMenuItem.Checked = currentSize == 256;
            size512ToolStripMenuItem.Checked = currentSize == 512;

            rotate0ToolStripMenuItem.Checked = currentRotation == 0;
            rotate90ToolStripMenuItem.Checked = currentRotation == 90;
            rotate180ToolStripMenuItem.Checked = currentRotation == 180;
            rotate270ToolStripMenuItem.Checked = currentRotation == 270;
        }

        private void UpdateInfoMenuTexts()
        {
            string buildTimestamp = Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == "BuildTimestamp")?.Value
                ?? Application.ProductVersion;

            string buildDate = buildTimestamp.Split(' ')[0];
            versionToolStripMenuItem.Text = $"Build: {buildDate}";

            string copyrightText = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
                ?? "EARCE.NET";
            copyrightToolStripMenuItem.Text = $"Copyright: {copyrightText}";
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
