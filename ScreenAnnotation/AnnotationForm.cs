using System.Text;

namespace ScreenAnnotation
{
    public partial class AnnotationForm : Form
    {
        private List<TextPanel> textPanels = new();
        private List<Rectangle> rectangles = new();
        private List<ImageAnnotation> imageAnnotations = new();
        private Button addTextButton;
        private Button addArrowButton;
        private Button exitButton;
        private Button clearButton;
        private Button saveButton;
        private Button nextDisplayButton;
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
        private Image? displayChangeButtonIcon;
        private ImageAnnotation? draggingImage = null;
        private TextPanel? draggingTextPanel = null;
        private Point dragOffset;

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

            // Load images from embedded resources
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.ArrowIcon.png"))
            {
                if (stream != null)
                {
                    arrowImage = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.DrawSpeechBubble.png"))
            {
                if (stream != null)
                {
                    speechBubbleImage = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.ArrowIconButton.png"))
            {
                if (stream != null)
                {
                    arrowButtonIcon = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.DrawSpeechBubbleButton.png"))
            {
                if (stream != null)
                {
                    speechBubbleButtonIcon = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.ExitButton.png"))
            {
                if (stream != null)
                {
                    exitButtonIcon = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.ClearButton.png"))
            {
                if (stream != null)
                {
                    clearButtonIcon = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.SaveButton.png"))
            {
                if (stream != null)
                {
                    saveButtonIcon = Image.FromStream(stream);
                }
            }

            using (var stream = assembly.GetManifestResourceStream("ScreenAnnotation.DisplayChange.png"))
            {
                if (stream != null)
                {
                    displayChangeButtonIcon = Image.FromStream(stream);
                }
            }

            // Setup form
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.DoubleBuffered = true;

            // Create exit button (on the first display)
            exitButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(10, 10),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = exitButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            exitButton.FlatAppearance.BorderSize = 0;
            exitButton.Click += ExitButton_Click;
            this.Controls.Add(exitButton);

            // Create clear button
            clearButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(70, 10),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = clearButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += ClearButton_Click;
            this.Controls.Add(clearButton);

            // Create save button
            saveButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(130, 10),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = saveButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            // Create add arrow button
            addArrowButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(190, 10),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = arrowButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            addArrowButton.FlatAppearance.BorderSize = 0;
            addArrowButton.Click += AddArrowButton_Click;
            this.Controls.Add(addArrowButton);

            // Create add text button
            addTextButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(250, 10),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = speechBubbleButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            addTextButton.FlatAppearance.BorderSize = 0;
            addTextButton.Click += AddTextButton_Click;
            this.Controls.Add(addTextButton);

            // Create next display button
            nextDisplayButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(310, 10),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = displayChangeButtonIcon,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            nextDisplayButton.FlatAppearance.BorderSize = 0;
            nextDisplayButton.Click += NextDisplayButton_Click;
            this.Controls.Add(nextDisplayButton);

            // Mouse events
            this.MouseDown += AnnotationForm_MouseDown;
            this.MouseMove += AnnotationForm_MouseMove;
            this.MouseUp += AnnotationForm_MouseUp;
            this.DoubleClick += AnnotationForm_DoubleClick;

            // Resize event to update button positions
            this.Resize += AnnotationForm_Resize;

            this.Cursor = Cursors.Default;
        }

        private void AnnotationForm_Resize(object? sender, EventArgs e)
        {
            // Recalculate button positions based on current form height
            int buttonBaseY = this.Height - 60;
            exitButton.Location = new Point(10, buttonBaseY);
            clearButton.Location = new Point(70, buttonBaseY);
            saveButton.Location = new Point(130, buttonBaseY);
            addArrowButton.Location = new Point(190, buttonBaseY);
            addTextButton.Location = new Point(250, buttonBaseY);
            nextDisplayButton.Location = new Point(310, buttonBaseY);
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

        private void NextDisplayButton_Click(object? sender, EventArgs e)
        {
            MoveToNextDisplay();
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

            // Temporarily disable maximized state to change location and size
            FormWindowState previousState = this.WindowState;
            this.WindowState = FormWindowState.Normal;

            // Set location to next screen
            this.Location = nextScreen.WorkingArea.Location;

            // Set size to match the next screen's working area
            this.Size = nextScreen.WorkingArea.Size;

            // Restore to Maximized state
            this.WindowState = previousState;
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "終了してもよろしいですか？",
                "ScreenAnnotation - 終了確認",
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
            var result = MessageBox.Show(
                "全ての注釈をクリアしてもよろしいですか？",
                "ScreenAnnotation - クリア確認",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.OK)
            {
                // Clear all annotations
                textPanels.Clear();
                imageAnnotations.Clear();
                rectangles.Clear();

                // Remove all controls except buttons
                var buttonsToKeep = new[] { exitButton, clearButton, saveButton, addArrowButton, addTextButton };
                var controlsToRemove = this.Controls.Cast<Control>()
                    .Where(c => !buttonsToKeep.Contains(c))
                    .ToList();

                foreach (var control in controlsToRemove)
                {
                    this.Controls.Remove(control);
                    control.Dispose();
                }

                this.Invalidate();
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            SaveAsMarkdown();
        }

        private void SaveAsMarkdown()
        {
            if (textPanels.Count == 0)
            {
                MessageBox.Show("No annotations to save.", "ScreenAnnotation - 情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveDialog = new SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
                DefaultExt = "md",
                FileName = $"annotation_{DateTime.Now:yyyyMMdd_HHmmss}.md"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("# Annotations");
                        sb.AppendLine();
                        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        sb.AppendLine();

                        int index = 1;
                        foreach (var textPanel in textPanels)
                        {
                            sb.AppendLine($"## Annotation {index}");
                            sb.AppendLine();
                            sb.AppendLine($"```");
                            sb.AppendLine(textPanel.TextBox.Text);
                            sb.AppendLine($"```");
                            sb.AppendLine();
                            index++;
                        }

                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString());
                        MessageBox.Show($"Saved to {saveDialog.FileName}", "ScreenAnnotation - 保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "ScreenAnnotation - エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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

            textBox.Focus();
        }

        private void TextBox_MouseDown(TextPanel textPanel, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && textPanel.TextBox.ReadOnly)
            {
                draggingTextPanel = textPanel;
                dragOffset = new Point(e.X + 20, e.Y + 20); // Adjust for textbox offset in panel
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
                return Screen.PrimaryScreen.Bounds;
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
}
