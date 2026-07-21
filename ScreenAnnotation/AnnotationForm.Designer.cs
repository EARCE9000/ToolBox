namespace ScreenAnnotation
{
    partial class AnnotationForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // Form
            this.Text = "ScreenAnnotation";
            this.Name = "AnnotationForm";
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = Screen.PrimaryScreen.Bounds;
        }
    }
}
