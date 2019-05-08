using KanbanReporter.Business.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace KanbanReporterCmd
{
    internal class ConsoleSettings : ISettings
    {
        private string _settingsFilePath;
        private JObject _json;

        public ConsoleSettings(string[] args)
        {
            if (!args.Contains("--settings-file") && !args.Contains("-s"))
            {
                Console.WriteLine("ERROR: Required argument missing: --settings-file [settings file path]");
                return;
            }

            _settingsFilePath = string.Empty;
            int i;
            for (i = 0; args[i] != "--settings-file" && args[i] != "-s"; i++);

            if(i + 1 == args.Length)
            {
                Console.WriteLine("ERROR: No argument provided for settings file path");
                return;
            }

            _settingsFilePath = args[i + 1];
            if(!File.Exists(_settingsFilePath))
            {
                Console.WriteLine("ERROR: Settings file path does not point to a file");
                return;
            }

            Console.WriteLine("Settings file accepted");
        }

        public string this[string key]
        {
            get
            {
                if (_json == null)
                    _json = JObject.Parse(File.ReadAllText(_settingsFilePath));

                if(_json.ContainsKey(key))
                    return _json[key].Value<string>();

                return string.Empty;
            }
        }
    }
}