namespace PresentationPointer
{
    partial class AboutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            applicationNameLabel = new Label();
            buildVersionLabel = new Label();
            copyrightLabel = new Label();
            repositoryLabel = new Label();
            repositoryLinkLabel = new LinkLabel();
            okButton = new Button();

            SuspendLayout();

            // applicationNameLabel
            applicationNameLabel.AutoSize = true;
            applicationNameLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            applicationNameLabel.Location = new Point(20, 20);
            applicationNameLabel.Name = "applicationNameLabel";
            applicationNameLabel.Size = new Size(200, 25);
            applicationNameLabel.TabIndex = 0;
            applicationNameLabel.Text = "PresentationPointer";

            // buildVersionLabel
            buildVersionLabel.AutoSize = true;
            buildVersionLabel.Location = new Point(20, 60);
            buildVersionLabel.Name = "buildVersionLabel";
            buildVersionLabel.Size = new Size(100, 15);
            buildVersionLabel.TabIndex = 1;
            buildVersionLabel.Text = "Build: -";

            // copyrightLabel
            copyrightLabel.AutoSize = true;
            copyrightLabel.Location = new Point(20, 85);
            copyrightLabel.Name = "copyrightLabel";
            copyrightLabel.Size = new Size(100, 15);
            copyrightLabel.TabIndex = 2;
            copyrightLabel.Text = "Copyright: -";

            // repositoryLabel
            repositoryLabel.AutoSize = true;
            repositoryLabel.Location = new Point(20, 120);
            repositoryLabel.Name = "repositoryLabel";
            repositoryLabel.Size = new Size(63, 15);
            repositoryLabel.TabIndex = 3;
            repositoryLabel.Text = "Repository:";

            // repositoryLinkLabel
            repositoryLinkLabel.AutoSize = true;
            repositoryLinkLabel.Location = new Point(20, 140);
            repositoryLinkLabel.Name = "repositoryLinkLabel";
            repositoryLinkLabel.Size = new Size(250, 15);
            repositoryLinkLabel.TabIndex = 4;
            repositoryLinkLabel.TabStop = true;
            repositoryLinkLabel.Text = "https://github.com/EARCE9000/ToolBox";
            repositoryLinkLabel.LinkClicked += RepositoryLinkLabel_LinkClicked;

            // okButton
            okButton.Location = new Point(240, 180);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 30);
            okButton.TabIndex = 5;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OkButton_Click;

            // AboutForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(340, 230);
            Controls.Add(okButton);
            Controls.Add(repositoryLinkLabel);
            Controls.Add(repositoryLabel);
            Controls.Add(copyrightLabel);
            Controls.Add(buildVersionLabel);
            Controls.Add(applicationNameLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "バージョン情報";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label applicationNameLabel;
        private Label buildVersionLabel;
        private Label copyrightLabel;
        private Label repositoryLabel;
        private LinkLabel repositoryLinkLabel;
        private Button okButton;
    }
}
