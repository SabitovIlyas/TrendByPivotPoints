using System.Text;

namespace ProjectsManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
