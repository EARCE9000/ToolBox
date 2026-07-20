using System.Diagnostics;
using System.Reflection;

namespace PresentationPointer
{
    public partial class AboutForm : Form
    {
        private const string RepositoryUrl = "https://github.com/EARCE9000/ToolBox";

        public AboutForm()
        {
            InitializeComponent();
            Load += AboutForm_Load;
        }

        private void AboutForm_Load(object? sender, EventArgs e)
        {
            // Application name
            applicationNameLabel.Text = "PresentationPointer";

            // Build version
            string buildTimestamp = Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == "BuildTimestamp")?.Value
                ?? Application.ProductVersion;
            buildVersionLabel.Text = $"Build: {buildTimestamp}";

            // Copyright
            string copyrightText = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
                ?? "EARCE.NET";
            copyrightLabel.Text = copyrightText;

            // Repository link
            repositoryLinkLabel.Text = RepositoryUrl;
        }

        private void RepositoryLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = RepositoryUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                // Ignore browser launch failures
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
