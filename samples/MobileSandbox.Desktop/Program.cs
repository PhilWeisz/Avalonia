using System;
using Avalonia;

namespace MobileSandbox.Desktop
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args) =>
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

        /// <summary>
        /// This method is needed for IDE previewer infrastructure
        /// </summary>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                UseCompositor = true,
            })
            .AfterSetup(builder =>
            {
                builder.Instance!.AttachDevTools(new Avalonia.Diagnostics.DevToolsOptions()
                {
                    StartupScreenIndex = 1,
                });
            })
                .LogToTrace();
    }
}
