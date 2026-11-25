using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CourseApp.classes.services
{
    public class JsonStorage : IStorage
    {
        private readonly JsonSerializerOptions _options;

        public JsonStorage()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
        }

        public List<T> Load<T>(string path)
        {
            if (!File.Exists(path)) return new List<T>();

            try
            {
                string json = File.ReadAllText(path);
                List<T>? items = JsonSerializer.Deserialize<List<T>>(json, _options);
                return items ?? new List<T>();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        public void Save<T>(string path, IEnumerable<T> items)
        {
            string json = JsonSerializer.Serialize(items, _options);
            File.WriteAllText(path, json);
        }
    }
}