using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phoenix_prototype.config
{
    using Microsoft.VisualBasic;
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Windows;

    public static class ConfigurationManager
    {
        private static readonly string ConfigPath =
            Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        public static AppSettings Settings { get; private set; } = new();

        public static void Load()
        {
            if (!File.Exists(ConfigPath)) { 
                MessageBox.Show("Configuration file not found for " + ConfigPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
                

            string json = File.ReadAllText(ConfigPath);

            Settings = JsonSerializer.Deserialize<AppSettings>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new AppSettings();
        }
    }
}
