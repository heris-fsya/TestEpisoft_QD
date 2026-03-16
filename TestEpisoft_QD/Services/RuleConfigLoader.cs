using System;
using System.Collections.Generic;
using System.IO;

namespace TestEpisoft_QD.Services
{
    public class RuleConfigLoader
    {
        public Dictionary<string, int> Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Fichier de configuration introuvable : " + path);
            }

            var config = new Dictionary<string, int>();

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('=');

                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = int.Parse(parts[1].Trim());

                config[key] = value;
            }

            return config;
        }
    }
}