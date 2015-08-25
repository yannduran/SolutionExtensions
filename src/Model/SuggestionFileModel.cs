﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SolutionExtensions
{
    public class SuggestionFileModel
    {
        public Dictionary<string, IEnumerable<IExtensionModel>> Extensions { get; set; }

        public static SuggestionFileModel FromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            string fileContent = File.ReadAllText(fileName);

            var obj = JObject.Parse(fileContent);
            SuggestionFileModel fileModel = new SuggestionFileModel
            {
                Extensions = new Dictionary<string, IEnumerable<IExtensionModel>>()
            };

            var fileBased = new List<IExtensionModel>();

            foreach (var ext in obj["fileBased"])
            {
                SuggestionModel model = new SuggestionModel
                {
                    Name = ((JProperty)ext).Name,
                    ProductId = ext.FirstOrDefault()?["productId"].ToString(),
                    Description = ext.FirstOrDefault()?["description"].ToString(),
                    FileTypes = ext.FirstOrDefault()?["fileTypes"].Values<string>().ToArray(),
                    Category = "File based"
                };

                fileBased.Add(model);
            }

            fileModel.Extensions.Add("File Based", fileBased);

            var general = new List<IExtensionModel>();

            foreach (var ext in obj["general"])
            {
                SuggestionModel model = new SuggestionModel
                {
                    Name = ((JProperty)ext).Name,
                    ProductId = ext.FirstOrDefault()?["productId"].ToString(),
                    Description = ext.FirstOrDefault()?["description"].ToString(),
                    Category = "General"
                };

                general.Add(model);
            }

            fileModel.Extensions.Add("General", general);

            return fileModel;
        }
    }
}
