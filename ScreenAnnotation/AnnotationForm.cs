using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScreenAnnotation
{
    public partial class AnnotationForm : Form
    {
        private List<TextPanel> textPanels = new();
        private List<Rectangle> rectangles = new();
        private List<ImageAnnotation> imageAnnotations = new();
        private Panel toolbarPanel;
        private Button addTextButton;
        private Button addArrowButton;
        private Button exitButton;
        private Button clearButton;
        private Button saveButton;
        private Button loadButton;
        private Button nextDisplayButton;
        private Button informationButton;
        private Button sleepButton;
        private Button settingButton;
        private Button captureButton;
        private ToolTip toolTip = new();
        private bool isDrawingRect = false;
        private Point rectStartPoint;
        private Rectangle? currentRect = null;
        private Image? arrowImage;
        private Image? speechBubbleImage;
        private Image? arrowButtonIcon;
        private Image? speechBubbleButtonIcon;
        private Image? exitButtonIcon;
        private Image? clearButtonIcon;
        private Image? saveButtonIcon;
        private Image? loadButtonIcon;
        private Image? displayChangeButtonIcon;
        private Image? informationButtonIcon;
        private Image? sleepButtonIcon;
        private Image? settingButtonIcon;
        private Image? captureButtonIcon;
        private ImageAnnotation? draggingImage = null;
        private TextPanel? draggingTextPanel = null;
        private Point dragOffset;
        private string lastSavedAnnotationSnapshot = string.Empty;
        private System.Windows.Forms.Timer? _placementTimer;
        private Screen? _lastKnownScreen;
        private Rectangle _lastKnownWorkingArea;

        private const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        private class TextPanel
        {
            public Panel Panel { get; set; }
            public TextBox TextBox { get; set; }
            public bool IsEditing { get; set; }

            public TextPanel(Panel panel, TextBox textBox)
            {
                Panel = panel;
                TextBox = textBox;
                IsEditing = true;
            }
        }

        private class ImageAnnotation
        {
            public Image Image { get; set; }
            public Point Location { get; set; }
            public Size Size { get; set; }
            public int Number { get; set; }

            public ImageAnnotation(Image image, Point location, Size size, int number)
            {
                Image = image;
                Location = location;
                Size = size;
                Number = number;
            }

            public Rectangle Bounds => new Rectangle(Location, Size);
        }

        public AnnotationForm()
        {
            InitializeComponent();
            Load += AnnotationForm_Load;
            Shown += AnnotationForm_Shown;

            // Load images from embedded resources
            var assembly = Assembly.GetExecutingAssembly();

            arrowImage = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.ArrowIcon.png", this);
            speechBubbleImage = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.DrawSpeechBubble.png", this);
            arrowButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.ArrowIconButton.png", this);
            speechBubbleButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.DrawSpeechBubbleButton.png", this);
            exitButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.ExitButton.png", this);
            clearButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.ClearButton.png", this);
            saveButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.SaveButton.png", this);
            loadButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.LoadButton.png", this);
            displayChangeButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.DisplayChange.png", this);
            informationButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.InformationButton.png", this);
            sleepButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.SleepButton.png", this);
            settingButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.SettingButton.png", this);
            captureButtonIcon = DpiAwareImageLoader.Load(assembly, "ScreenAnnotation.CaptureButton.png", this);

            // Setup form
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.None;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.DoubleBuffered = true;

            // Create toolbar panel with dark gray background
            toolbarPanel = new Panel
            {
                BackColor = Color.FromArgb(50, 50, 50),  // Dark gray
                Location = new Point(10, 10),
                Size = new Size(660, 60),
                Margin = new Padding(0)
            };
            this.Controls.Add(toolbarPanel);

            // Create exit button
            exitButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(5, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = exitButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            exitButton.FlatAppearance.BorderSize = 0;
            exitButton.Click += ExitButton_Click;
            toolbarPanel.Controls.Add(exitButton);

            // Create clear button
            clearButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(65, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = clearButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += ClearButton_Click;
            toolbarPanel.Controls.Add(clearButton);

            // Create save button
            saveButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(125, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = saveButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;
            toolbarPanel.Controls.Add(saveButton);

            // Create load button
            loadButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(185, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = loadButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = loadButtonIcon == null ? "L" : string.Empty
            };
            loadButton.FlatAppearance.BorderSize = 0;
            loadButton.Click += LoadButton_Click;
            toolbarPanel.Controls.Add(loadButton);

            // Create capture button
            captureButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(245, 5),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = captureButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = captureButtonIcon == null ? "C" : string.Empty
            };
            captureButton.FlatAppearance.BorderSize = 0;
            captureButton.Click += CaptureButton_Click;
            toolbarPanel.Controls.Add(captureButton);

            // Create add arrow button
            addArrowButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(305, 5),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = arrowButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            addArrowButton.FlatAppearance.BorderSize = 0;
            addArrowButton.Click += AddArrowButton_Click;
            toolbarPanel.Controls.Add(addArrowButton);

            // Create add text button
            addTextButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(365, 5),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = speechBubbleButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            addTextButton.FlatAppearance.BorderSize = 0;
            addTextButton.Click += AddTextButton_Click;
            toolbarPanel.Controls.Add(addTextButton);

            // Create next display button
            nextDisplayButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(425, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = displayChangeButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            nextDisplayButton.FlatAppearance.BorderSize = 0;
            nextDisplayButton.Click += NextDisplayButton_Click;
            toolbarPanel.Controls.Add(nextDisplayButton);

            // Create sleep button (Away/Break status)
            sleepButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(485, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = sleepButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            sleepButton.FlatAppearance.BorderSize = 0;
            sleepButton.Click += SleepButton_Click;
            toolbarPanel.Controls.Add(sleepButton);

            // Create setting button
            settingButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(545, 5),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = settingButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = settingButtonIcon == null ? "S" : string.Empty
            };
            settingButton.FlatAppearance.BorderSize = 0;
            settingButton.Click += SettingButton_Click;
            toolbarPanel.Controls.Add(settingButton);

            // Create information button
            informationButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(605, 5),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = informationButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = informationButtonIcon == null ? "i" : string.Empty
            };
            informationButton.FlatAppearance.BorderSize = 0;
            informationButton.Click += InformationButton_Click;
            toolbarPanel.Controls.Add(informationButton);

            // ToolTips
            toolTip.SetToolTip(exitButton,        Properties.Resources.Tooltip_Exit);
            toolTip.SetToolTip(clearButton,       Properties.Resources.Tooltip_Clear);
            toolTip.SetToolTip(saveButton,        Properties.Resources.Tooltip_Save);
            toolTip.SetToolTip(loadButton,        Properties.Resources.Tooltip_Load);
            toolTip.SetToolTip(addArrowButton,    Properties.Resources.Tooltip_AddArrow);
            toolTip.SetToolTip(addTextButton,     Properties.Resources.Tooltip_AddText);
            toolTip.SetToolTip(nextDisplayButton, Properties.Resources.Tooltip_NextDisplay);
            toolTip.SetToolTip(sleepButton,       Properties.Resources.Tooltip_Sleep);
            toolTip.SetToolTip(settingButton,     Properties.Resources.Tooltip_Settings);
            toolTip.SetToolTip(informationButton, Properties.Resources.Tooltip_Information);
            toolTip.SetToolTip(captureButton,     Properties.Resources.Tooltip_Capture);

            // Mouse events
            this.MouseDown += AnnotationForm_MouseDown;
            this.MouseMove += AnnotationForm_MouseMove;
            this.MouseUp += AnnotationForm_MouseUp;
            this.DoubleClick += AnnotationForm_DoubleClick;

            // Resize event to update button positions
            this.Resize += AnnotationForm_Resize;

            this.Cursor = Cursors.Default;
            lastSavedAnnotationSnapshot = CreateCurrentAnnotationSnapshot();
        }

        private void AnnotationForm_Resize(object? sender, EventArgs e)
        {
            // Recalculate toolbar panel position based on current client area height
            int toolbarY = this.ClientSize.Height - toolbarPanel.Height - 10;
            toolbarPanel.Location = new Point(10, Math.Max(0, toolbarY));
        }

        private void ApplyToolbarScale(int scalePercent)
        {
            // scalePercent: 50, 75, or 100
            float scale = scalePercent / 100f;
            int btnSize = (int)(50 * scale);
            int btnPadding = (int)(5 * scale);
            int btnStep = (int)(60 * scale);
            int panelHeight = btnSize + btnPadding * 2;
            int panelWidth = btnStep * 10 + btnSize + btnPadding * 2;

            toolbarPanel.Size = new Size(panelWidth, panelHeight);

            // Pair each button with its source icon image
            var buttonIconPairs = new (Button btn, Image? icon)[]
            {
                (exitButton,        exitButtonIcon),
                (clearButton,       clearButtonIcon),
                (saveButton,        saveButtonIcon),
                (loadButton,        loadButtonIcon),
                (captureButton,     captureButtonIcon),
                (addArrowButton,    arrowButtonIcon),
                (addTextButton,     speechBubbleButtonIcon),
                (nextDisplayButton, displayChangeButtonIcon),
                (sleepButton,       sleepButtonIcon),
                (settingButton,     settingButtonIcon),
                (informationButton, informationButtonIcon),
            };

            for (int i = 0; i < buttonIconPairs.Length; i++)
            {
                var (btn, icon) = buttonIconPairs[i];
                btn.Size = new Size(btnSize, btnSize);
                btn.Location = new Point(btnPadding + i * btnStep, btnPadding);

                // Scale the icon to fit exactly inside the button with a small inset
                int iconSize = Math.Max(1, btnSize - 4);
                btn.Image = icon != null ? ResizeImage(icon, iconSize, iconSize) : null;
                btn.ImageAlign = ContentAlignment.MiddleCenter;
            }
        }

        /// <summary>高品質リサンプリングで画像を指定サイズにリサイズする。</summary>
        private static Bitmap ResizeImage(Image source, int width, int height)
        {
            var dest = new Bitmap(width, height);
            using var g = Graphics.FromImage(dest);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawImage(source, new Rectangle(0, 0, width, height));
            return dest;
        }

        private void AddArrowButton_Click(object? sender, EventArgs e)
        {
            if (arrowImage != null)
            {
                // Calculate next number
                int nextNumber = imageAnnotations.Count + 1;

                // Add arrow image at the center of the screen (96x96)
                Point centerLocation = new Point(
                    this.Width / 2 - 48,
                    this.Height / 2 - 48
                );
                imageAnnotations.Add(new ImageAnnotation(arrowImage, centerLocation, new Size(96, 96), nextNumber));
                Invalidate();
            }
        }

        private void AddTextButton_Click(object? sender, EventArgs e)
        {
            // Create new text panel in the center of the screen
            Point centerLocation = new Point(
                this.Width / 2 - 125,
                this.Height / 2 - 75
            );
            CreateTextPanel(centerLocation);
        }

        private void LoadButton_Click(object? sender, EventArgs e)
        {
            LoadFromJson();
        }

        private void CaptureButton_Click(object? sender, EventArgs e)
        {
            try
            {
                Rectangle formBounds = this.Bounds;
                Screen targetScreen = Screen.FromControl(this);
                int maxOverlapArea = -1;

                foreach (var screen in Screen.AllScreens)
                {
                    Rectangle overlap = Rectangle.Intersect(screen.Bounds, formBounds);
                    int overlapArea = Math.Max(0, overlap.Width) * Math.Max(0, overlap.Height);
                    if (overlapArea > maxOverlapArea)
                    {
                        maxOverlapArea = overlapArea;
                        targetScreen = screen;
                    }
                }

                Rectangle captureBounds = TryGetDisplayBoundsInPixels(targetScreen, out Rectangle pixelBounds)
                    ? pixelBounds
                    : targetScreen.Bounds;

                using var bitmap = new Bitmap(captureBounds.Width, captureBounds.Height);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(captureBounds.Left, captureBounds.Top, 0, 0, captureBounds.Size, CopyPixelOperation.SourceCopy);
                }

                string screenshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
                Directory.CreateDirectory(screenshotsDirectory);

                string filePath = Path.Combine(screenshotsDirectory, $"ScreenAnnotation_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                MessageBox.Show(string.Format(Properties.Resources.Msg_CaptureComplete, filePath), Properties.Resources.Title_CaptureComplete, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.Msg_CaptureError, ex.Message), Properties.Resources.Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool TryGetDisplayBoundsInPixels(Screen screen, out Rectangle bounds)
        {
            DEVMODE mode = new DEVMODE();
            mode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref mode))
            {
                bounds = new Rectangle(mode.dmPositionX, mode.dmPositionY, mode.dmPelsWidth, mode.dmPelsHeight);
                return true;
            }

            bounds = Rectangle.Empty;
            return false;
        }

        private void LoadFromJson()
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (openDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                string json = System.IO.File.ReadAllText(openDialog.FileName);
                var data = JsonSerializer.Deserialize<AnnotationSaveData>(json);
                if (data == null)
                {
                    MessageBox.Show(Properties.Resources.Msg_LoadParseError, Properties.Resources.Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ClearAll();

                // Restore arrows
                foreach (var arrow in data.Arrows)
                {
                    if (arrowImage != null)
                    {
                        imageAnnotations.Add(new ImageAnnotation(arrowImage, new Point(arrow.X, arrow.Y), new Size(96, 96), arrow.Number));
                    }
                }
                RenumberArrows();

                // Restore bubbles
                foreach (var bubble in data.Bubbles)
                {
                    CreateTextPanel(new Point(bubble.X, bubble.Y), bubble.Text);
                }

                Invalidate();
                lastSavedAnnotationSnapshot = CreateCurrentAnnotationSnapshot();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.Msg_LoadError, ex.Message), Properties.Resources.Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NextDisplayButton_Click(object? sender, EventArgs e)
        {
            MoveToNextDisplay();
        }

        private void InformationButton_Click(object? sender, EventArgs e)
        {
            bool wasTopMost = TopMost;
            TopMost = false;

            try
            {
                using var aboutForm = new AboutForm();
                aboutForm.ShowDialog(this);
            }
            finally
            {
                TopMost = wasTopMost;
            }
        }

        private void SleepButton_Click(object? sender, EventArgs e)
        {
            // Get the screen that contains this form
            Screen? currentScreen = Screen.FromControl(this);

            // Show away/break status window on the same screen
            var statusWindow = new StatusWindow(currentScreen, StatusType.Away);
            statusWindow.ShowDialog(this);
        }

        private void SettingButton_Click(object? sender, EventArgs e)
        {
            bool wasTopMost = TopMost;
            TopMost = false;

            try
            {
                ShowSleepTextSettingsDialog();
            }
            finally
            {
                TopMost = wasTopMost;
            }
        }

        private void ShowSleepTextSettingsDialog()
        {
            using var form = new Form
            {
                Text = Properties.Resources.Settings_Title,
                Width = 640,
                Height = 360,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var titleLabel = new Label { Text = Properties.Resources.Settings_SleepTextLabel, Left = 16, Top = 16, Width = 180 };
            var lbl1 = new Label { Text = "1)", Left = 16, Top = 50, Width = 40 };
            var txt1 = new TextBox { Left = 64, Top = 46, Width = 330, MaxLength = 10, Text = Properties.Settings.Default.SleepWord1 };
            var toggle1 = new CheckBox { Left = 410, Top = 48, Width = 200, Text = Properties.Resources.Settings_ShowResumeTime, Checked = Properties.Settings.Default.ShowResumeTime1 };
            var lbl2 = new Label { Text = "2)", Left = 16, Top = 90, Width = 40 };
            var txt2 = new TextBox { Left = 64, Top = 86, Width = 330, MaxLength = 10, Text = Properties.Settings.Default.SleepWord2 };
            var toggle2 = new CheckBox { Left = 410, Top = 88, Width = 200, Text = Properties.Resources.Settings_ShowResumeTime, Checked = Properties.Settings.Default.ShowResumeTime2 };
            var lbl3 = new Label { Text = "3)", Left = 16, Top = 130, Width = 40 };
            var txt3 = new TextBox { Left = 64, Top = 126, Width = 330, MaxLength = 10, Text = Properties.Settings.Default.SleepWord3 };
            var toggle3 = new CheckBox { Left = 410, Top = 128, Width = 200, Text = Properties.Resources.Settings_ShowResumeTime, Checked = Properties.Settings.Default.ShowResumeTime3 };

            var scaleLabel = new Label { Text = Properties.Resources.Settings_ToolbarScaleLabel, Left = 16, Top = 180, Width = 160 };
            int savedScale = Properties.Settings.Default.ToolbarScale;
            var scale50 = new RadioButton { Text = "50%", Left = 190, Top = 178, Width = 60, Checked = savedScale == 50 };
            var scale75 = new RadioButton { Text = "75%", Left = 260, Top = 178, Width = 60, Checked = savedScale == 75 };
            var scale100 = new RadioButton { Text = "100%", Left = 330, Top = 178, Width = 65, Checked = savedScale == 100 || (savedScale != 50 && savedScale != 75) };

            var okButton = new Button { Text = Properties.Resources.Settings_Save, Left = 450, Top = 260, Width = 75, Height = 32, DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = Properties.Resources.Settings_Cancel, Left = 535, Top = 260, Width = 75, Height = 32, DialogResult = DialogResult.Cancel };

            form.Controls.Add(titleLabel);
            form.Controls.Add(lbl1);
            form.Controls.Add(txt1);
            form.Controls.Add(toggle1);
            form.Controls.Add(lbl2);
            form.Controls.Add(txt2);
            form.Controls.Add(toggle2);
            form.Controls.Add(lbl3);
            form.Controls.Add(txt3);
            form.Controls.Add(toggle3);
            form.Controls.Add(scaleLabel);
            form.Controls.Add(scale50);
            form.Controls.Add(scale75);
            form.Controls.Add(scale100);
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);
            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Properties.Settings.Default.SleepWord1 = txt1.Text.Trim();
            Properties.Settings.Default.SleepWord2 = txt2.Text.Trim();
            Properties.Settings.Default.SleepWord3 = txt3.Text.Trim();
            Properties.Settings.Default.ShowResumeTime1 = toggle1.Checked;
            Properties.Settings.Default.ShowResumeTime2 = toggle2.Checked;
            Properties.Settings.Default.ShowResumeTime3 = toggle3.Checked;

            int newScale = scale50.Checked ? 50 : scale75.Checked ? 75 : 100;
            Properties.Settings.Default.ToolbarScale = newScale;
            Properties.Settings.Default.Save();

            ApplyToolbarScale(newScale);
            AnnotationForm_Resize(this, EventArgs.Empty);
        }

        private void AnnotationForm_Load(object? sender, EventArgs e)
        {
            ApplyToolbarScale(Properties.Settings.Default.ToolbarScale);

            // Start a 1-second timer that continuously monitors which screen the
            // window is on and corrects its bounds and toolbar position automatically.
            _placementTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _placementTimer.Tick += PlacementTimer_Tick;
            _placementTimer.Start();
        }

        private void AnnotationForm_Shown(object? sender, EventArgs e)
        {
            PositionOnPreferredScreen();
        }

        private void PlacementTimer_Tick(object? sender, EventArgs e)
        {
            // Determine which screen the centre of the window is currently on.
            Screen current = Screen.FromPoint(
                new Point(this.Left + this.Width / 2, this.Top + this.Height / 2));

            // If the screen or its working area has changed, refit the window.
            if (!ReferenceEquals(current, _lastKnownScreen) ||
                current.WorkingArea != _lastKnownWorkingArea)
            {
                _lastKnownScreen = current;
                _lastKnownWorkingArea = current.WorkingArea;
                ApplyScreenPlacement(current, persistSelection: false);
            }
        }

        private void PositionOnPreferredScreen()
        {
            string? savedDeviceName = Properties.Settings.Default.LastDisplayDeviceName;
            if (!string.IsNullOrWhiteSpace(savedDeviceName))
            {
                Screen? savedScreen = Screen.AllScreens.FirstOrDefault(screen =>
                    string.Equals(screen.DeviceName, savedDeviceName, StringComparison.OrdinalIgnoreCase));

                if (savedScreen != null)
                {
                    ApplyScreenPlacement(savedScreen, persistSelection: false);
                    return;
                }
            }

            ApplyScreenPlacement(Screen.FromPoint(Cursor.Position), persistSelection: false);
        }

        private void ApplyScreenPlacement(Screen screen, bool persistSelection)
        {
            this.WindowState = FormWindowState.Normal;
            this.Bounds = screen.WorkingArea;

            // Re-generate icon bitmaps at the current toolbar scale so they
            // render correctly regardless of the target monitor's DPI/resolution.
            ApplyToolbarScale(Properties.Settings.Default.ToolbarScale);
            AnnotationForm_Resize(this, EventArgs.Empty);

            // Update cache so the timer does not immediately re-trigger.
            _lastKnownScreen = screen;
            _lastKnownWorkingArea = screen.WorkingArea;

            if (persistSelection)
            {
                Properties.Settings.Default.LastDisplayDeviceName = screen.DeviceName;
                Properties.Settings.Default.Save();
            }
        }

        private void MoveToNextDisplay()
        {
            Screen[] screens = Screen.AllScreens;

            if (screens.Length <= 1)
            {
                return;
            }

            // Get the screen that contains most of the form
            Screen currentScreen = Screen.FromPoint(new Point(this.Left + this.Width / 2, this.Top + this.Height / 2));
            int currentDisplayIndex = Array.IndexOf(screens, currentScreen);

            if (currentDisplayIndex == -1)
            {
                currentDisplayIndex = 0;
            }

            // Move to next display
            int nextIndex = (currentDisplayIndex + 1) % screens.Length;
            Screen nextScreen = screens[nextIndex];

            ApplyScreenPlacement(nextScreen, persistSelection: true);
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            _placementTimer?.Stop();
            var result = MessageBox.Show(
                Properties.Resources.Msg_ExitConfirm,
                Properties.Resources.Title_ExitConfirm,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.OK)
            {
                this.Close();
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            if (textPanels.Count == 0 && imageAnnotations.Count == 0)
            {
                ClearAll();
                return;
            }

            if (IsCurrentAnnotationSaved())
            {
                ClearAll();
                return;
            }

            var result = MessageBox.Show(
                Properties.Resources.Msg_ClearConfirm,
                Properties.Resources.Title_ClearConfirm,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.OK)
            {
                ClearAll();
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            SaveAsJson();
        }

        private void SaveAsJson()
        {
            if (textPanels.Count == 0 && imageAnnotations.Count == 0)
            {
                MessageBox.Show(Properties.Resources.Msg_NoAnnotationsToSave, Properties.Resources.Title_Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"annotation_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                var data = CreateCurrentSaveData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = JsonSerializer.Serialize(data, options);
                System.IO.File.WriteAllText(saveDialog.FileName, json);
                lastSavedAnnotationSnapshot = CreateCurrentAnnotationSnapshot();
                MessageBox.Show(string.Format(Properties.Resources.Msg_SaveComplete, saveDialog.FileName), Properties.Resources.Title_SaveComplete, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.Msg_SaveError, ex.Message), Properties.Resources.Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private AnnotationSaveData CreateCurrentSaveData()
        {
            return new AnnotationSaveData
            {
                Generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Arrows = imageAnnotations.Select(a => new ArrowData
                {
                    Number = a.Number,
                    X = a.Location.X,
                    Y = a.Location.Y
                }).ToList(),
                Bubbles = textPanels.Select(t => new BubbleData
                {
                    X = t.Panel.Location.X,
                    Y = t.Panel.Location.Y,
                    Text = t.TextBox.Text
                }).ToList()
            };
        }

        private string CreateCurrentAnnotationSnapshot()
        {
            var snapshot = CreateCurrentSaveData();
            return JsonSerializer.Serialize(new { snapshot.Arrows, snapshot.Bubbles });
        }

        private bool IsCurrentAnnotationSaved()
        {
            return string.Equals(lastSavedAnnotationSnapshot, CreateCurrentAnnotationSnapshot(), StringComparison.Ordinal);
        }

        private void AnnotationForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if clicking on an image (check in reverse order to prioritize top images)
                for (int i = imageAnnotations.Count - 1; i >= 0; i--)
                {
                    var img = imageAnnotations[i];
                    if (img.Bounds.Contains(e.Location))
                    {
                        draggingImage = img;
                        dragOffset = new Point(e.X - img.Location.X, e.Y - img.Location.Y);
                        this.Cursor = Cursors.SizeAll;
                        return;
                    }
                }

                // If not clicking on an image and in rect drawing mode
                if (isDrawingRect)
                {
                    rectStartPoint = e.Location;
                }
            }
        }

        private void AnnotationForm_DoubleClick(object? sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
            {
                // Check if double-clicking on an image to delete it
                for (int i = imageAnnotations.Count - 1; i >= 0; i--)
                {
                    var img = imageAnnotations[i];
                    if (img.Bounds.Contains(me.Location))
                    {
                        imageAnnotations.RemoveAt(i);
                        RenumberArrows();
                        Invalidate();
                        return;
                    }
                }
            }
        }

        private void RenumberArrows()
        {
            for (int i = 0; i < imageAnnotations.Count; i++)
            {
                imageAnnotations[i].Number = i + 1;
            }
        }

        private void AnnotationForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // If dragging an image
                if (draggingImage != null)
                {
                    draggingImage.Location = new Point(e.X - dragOffset.X, e.Y - dragOffset.Y);
                    Invalidate();
                    return;
                }

                // If drawing a rectangle
                if (isDrawingRect)
                {
                    int x = Math.Min(rectStartPoint.X, e.X);
                    int y = Math.Min(rectStartPoint.Y, e.Y);
                    int width = Math.Abs(e.X - rectStartPoint.X);
                    int height = Math.Abs(e.Y - rectStartPoint.Y);
                    currentRect = new Rectangle(x, y, width, height);
                    Invalidate();
                }
            }
        }

        private void AnnotationForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // If was dragging an image
                if (draggingImage != null)
                {
                    draggingImage = null;
                    this.Cursor = Cursors.Default;
                }

                // If was drawing a rectangle
                if (isDrawingRect && currentRect.HasValue)
                {
                    rectangles.Add(currentRect.Value);
                    currentRect = null;
                    isDrawingRect = false;
                    this.Cursor = Cursors.Default;
                    Invalidate();
                }
            }
        }

        private void CreateTextPanel(Point location)
        {
            CreateTextPanel(location, string.Empty);
        }

        private void CreateTextPanel(Point location, string initialText)
        {
            // Create panel with speech bubble background
            var panel = new Panel
            {
                Location = location,
                Size = new Size(250, 150),
                BackgroundImage = speechBubbleImage,
                BackgroundImageLayout = ImageLayout.Stretch,
                Cursor = Cursors.SizeAll
            };

            // Create text box
            var textBox = new TextBox
            {
                Location = new Point(20, 20),
                Width = 210,
                Height = 110,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.LightYellow,
                ForeColor = Color.Red,
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ReadOnly = false
            };

            var textPanel = new TextPanel(panel, textBox);
            textPanels.Add(textPanel);

            panel.Controls.Add(textBox);
            this.Controls.Add(panel);
            panel.BringToFront();

            // Add mouse events for panel dragging
            panel.MouseDown += (s, e) => TextPanel_MouseDown(textPanel, e);
            panel.MouseMove += (s, e) => TextPanel_MouseMove(textPanel, e);
            panel.MouseUp += (s, e) => TextPanel_MouseUp(textPanel, e);

            // Add mouse events for textbox dragging (when read-only)
            textBox.MouseDown += (s, e) => TextBox_MouseDown(textPanel, e);
            textBox.MouseMove += (s, e) => TextBox_MouseMove(textPanel, e);
            textBox.MouseUp += (s, e) => TextBox_MouseUp(textPanel, e);

            // Add double-click event to delete the text panel (on panel background)
            panel.DoubleClick += (s, e) => DeleteTextPanel(textPanel);
            textBox.DoubleClick += (s, e) =>
            {
                if (!textPanel.IsEditing)
                {
                    EnterEditMode(textPanel);
                }
            };

            // Add click event on form to exit edit mode
            textBox.LostFocus += (s, e) => ExitEditMode(textPanel);

            if (!string.IsNullOrEmpty(initialText))
            {
                textBox.Text = initialText;
                ExitEditMode(textPanel);
            }
            else
            {
                textBox.Focus();
            }
        }

        private void TextBox_MouseDown(TextPanel textPanel, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && textPanel.TextBox.ReadOnly)
            {
                EnterEditMode(textPanel);
                return;
            }
        }

        private void TextBox_MouseMove(TextPanel textPanel, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && draggingTextPanel == textPanel && textPanel.TextBox.ReadOnly)
            {
                Point newLocation = textPanel.Panel.Location;
                newLocation.X += e.X - (dragOffset.X - 20);
                newLocation.Y += e.Y - (dragOffset.Y - 20);
                textPanel.Panel.Location = newLocation;
                dragOffset = new Point(e.X + 20, e.Y + 20);
            }
        }

        private void TextBox_MouseUp(TextPanel textPanel, MouseEventArgs e)
        {
            if (draggingTextPanel == textPanel)
            {
                draggingTextPanel = null;
            }
        }

        private void TextPanel_MouseDown(TextPanel textPanel, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draggingTextPanel = textPanel;
                dragOffset = e.Location;
            }
        }

        private void TextPanel_MouseMove(TextPanel textPanel, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && draggingTextPanel == textPanel)
            {
                Point newLocation = textPanel.Panel.Location;
                newLocation.X += e.X - dragOffset.X;
                newLocation.Y += e.Y - dragOffset.Y;
                textPanel.Panel.Location = newLocation;
            }
        }

        private void TextPanel_MouseUp(TextPanel textPanel, MouseEventArgs e)
        {
            if (draggingTextPanel == textPanel)
            {
                draggingTextPanel = null;
            }
        }

        private void EnterEditMode(TextPanel textPanel)
        {
            textPanel.IsEditing = true;
            textPanel.TextBox.ReadOnly = false;
            textPanel.TextBox.BackColor = Color.LightYellow;
            textPanel.TextBox.Focus();
        }

        private void ExitEditMode(TextPanel textPanel)
        {
            textPanel.IsEditing = false;
            textPanel.TextBox.ReadOnly = true;
            textPanel.TextBox.BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Draw images with numbers
            foreach (var imgAnnotation in imageAnnotations)
            {
                // Draw arrow image
                e.Graphics.DrawImage(imgAnnotation.Image, 
                    new Rectangle(imgAnnotation.Location, imgAnnotation.Size));

                // Draw number in center of the rectangular part (left 60% of the arrow) + 10px to the right
                string numberText = imgAnnotation.Number.ToString();
                using (var font = new Font("Segoe UI", 24, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var textSize = e.Graphics.MeasureString(numberText, font);
                    // Position text in the center of the left 60% of the arrow (the rectangular part) + 10px right
                    float rectPartWidth = imgAnnotation.Size.Width * 0.6f;
                    var textLocation = new PointF(
                        imgAnnotation.Location.X + (rectPartWidth - textSize.Width) / 2 + 10,
                        imgAnnotation.Location.Y + (imgAnnotation.Size.Height - textSize.Height) / 2
                    );
                    e.Graphics.DrawString(numberText, font, brush, textLocation);
                }
            }

            // Draw rectangles
            using (var rectPen = new Pen(Color.Red, 3))
            {
                foreach (var rect in rectangles)
                {
                    e.Graphics.DrawRectangle(rectPen, rect);
                }

                // Draw current rectangle being drawn
                if (currentRect.HasValue)
                {
                    e.Graphics.DrawRectangle(rectPen, currentRect.Value);
                }
            }
        }

        private void ClearAll()
        {
            // Remove all text panels
            foreach (var textPanel in textPanels)
            {
                this.Controls.Remove(textPanel.Panel);
                textPanel.Panel.Dispose();
            }
            textPanels.Clear();

            rectangles.Clear();
            imageAnnotations.Clear();
            currentRect = null;
            Invalidate();
        }

        private Rectangle GetTotalDisplayBounds()
        {
            Screen[] screens = Screen.AllScreens;
            if (screens.Length == 0)
            {
                return Screen.PrimaryScreen?.Bounds ?? Rectangle.Empty;
            }

            int minX = screens[0].Bounds.X;
            int minY = screens[0].Bounds.Y;
            int maxX = screens[0].Bounds.Right;
            int maxY = screens[0].Bounds.Bottom;

            foreach (var screen in screens)
            {
                minX = Math.Min(minX, screen.Bounds.X);
                minY = Math.Min(minY, screen.Bounds.Y);
                maxX = Math.Max(maxX, screen.Bounds.Right);
                maxY = Math.Max(maxY, screen.Bounds.Bottom);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private void DeleteTextPanel(TextPanel textPanel)
        {
            // Remove from controls
            this.Controls.Remove(textPanel.Panel);

            // Remove from list
            textPanels.Remove(textPanel);

            // Dispose
            textPanel.Panel.Dispose();

            // Redraw
            Invalidate();
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void Clear_Click(object? sender, EventArgs e)
        {
            ClearAll();
        }
    }

    // ---- JSON save/load data models ----

    internal class AnnotationSaveData
    {
        [JsonPropertyName("generated")]
        public string Generated { get; set; } = string.Empty;

        [JsonPropertyName("arrows")]
        public List<ArrowData> Arrows { get; set; } = new();

        [JsonPropertyName("bubbles")]
        public List<BubbleData> Bubbles { get; set; } = new();
    }

    internal class ArrowData
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    internal class BubbleData
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
