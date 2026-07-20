namespace PresentationPointer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ContextMenuStrip
            contextMenuStrip = new ContextMenuStrip(components);

            // Image menu
            imageToolStripMenuItem = new ToolStripMenuItem("モード");
            arrowIconToolStripMenuItem = new ToolStripMenuItem("矢印", null, Image_Arrow_Click);
            speechBubbleToolStripMenuItem = new ToolStripMenuItem("吹き出し", null, Image_SpeechBubble_Click);
            imageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 
                arrowIconToolStripMenuItem,
                speechBubbleToolStripMenuItem
            });

            // Size menu
            sizeToolStripMenuItem = new ToolStripMenuItem("サイズ");
            size128ToolStripMenuItem = new ToolStripMenuItem("128x128", null, Size_128_Click);
            size256ToolStripMenuItem = new ToolStripMenuItem("256x256", null, Size_256_Click);
            size512ToolStripMenuItem = new ToolStripMenuItem("512x512", null, Size_512_Click);
            sizeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                size128ToolStripMenuItem,
                size256ToolStripMenuItem,
                size512ToolStripMenuItem
            });

            // Rotation menu
            rotationToolStripMenuItem = new ToolStripMenuItem("方向");
            rotate0ToolStripMenuItem = new ToolStripMenuItem("右上", null, Rotate_0_Click);
            rotate90ToolStripMenuItem = new ToolStripMenuItem("右下", null, Rotate_90_Click);
            rotate180ToolStripMenuItem = new ToolStripMenuItem("左下", null, Rotate_180_Click);
            rotate270ToolStripMenuItem = new ToolStripMenuItem("左上", null, Rotate_270_Click);
            rotationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                rotate0ToolStripMenuItem,
                rotate90ToolStripMenuItem,
                rotate180ToolStripMenuItem,
                rotate270ToolStripMenuItem
            });

            var separatorToolStripMenuItem = new ToolStripSeparator();
            var aboutToolStripMenuItem = new ToolStripMenuItem("バージョン情報", null, About_Click);
            var infoSeparatorToolStripMenuItem = new ToolStripSeparator();
            var exitToolStripMenuItem = new ToolStripMenuItem("終了", null, Exit_Click);

            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
                imageToolStripMenuItem,
                sizeToolStripMenuItem,
                rotationToolStripMenuItem,
                separatorToolStripMenuItem,
                aboutToolStripMenuItem,
                infoSeparatorToolStripMenuItem,
                exitToolStripMenuItem
            });

            // Form1
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 300);
            ContextMenuStrip = contextMenuStrip;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PresentationPointer";
            TopMost = true;

            // Transparent background
            TransparencyKey = Color.Magenta;
            BackColor = Color.Magenta;
        }

        #endregion

        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem imageToolStripMenuItem;
        private ToolStripMenuItem arrowIconToolStripMenuItem;
        private ToolStripMenuItem speechBubbleToolStripMenuItem;
        private ToolStripMenuItem sizeToolStripMenuItem;
        private ToolStripMenuItem size128ToolStripMenuItem;
        private ToolStripMenuItem size256ToolStripMenuItem;
        private ToolStripMenuItem size512ToolStripMenuItem;
        private ToolStripMenuItem rotationToolStripMenuItem;
        private ToolStripMenuItem rotate0ToolStripMenuItem;
        private ToolStripMenuItem rotate90ToolStripMenuItem;
        private ToolStripMenuItem rotate180ToolStripMenuItem;
        private ToolStripMenuItem rotate270ToolStripMenuItem;
    }
}
