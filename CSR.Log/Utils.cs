using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSR.Logging
{
    public static class Utils
    {
        public static string AppSetting(string key, string fileName, string pathLocation)
        {
            //return ConfigurationManager.AppSettings[key];
            JObject Config = null;
            if (Config == null)
            {
                string path = GetConfigPath(fileName, pathLocation);
                Config = GetConfig(path);
            }
            if (Config != null && Config[key] != null)
            {
                return Config[key].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static List<JToken> AppSettingArray(string key, string fileName, string pathLocation)
        {
            JObject Config = null;
            if (Config == null)
            {
                string path = GetConfigPath(fileName, pathLocation);
                Config = GetConfig(path);
            }
            if (Config != null && Config[key] != null)
            {
                return Config[key].ToList(); ;
            }
            else
            {
                return new List<JToken>();
            }
        }

        private static JObject GetConfig(string filePath)
        {
            string json = string.Empty;
            using (StreamReader reader = new StreamReader(filePath))
            {
                json = reader.ReadToEnd();
            }
            var config = JObject.Parse(json);
            return config;
        }

        private static string GetConfigPath(string fileName, string pathLocation)
        {
            string file = String.Format("{0}.json", fileName);
            string pathFile = String.Format("{0}/JSON/{1}.json", pathLocation, fileName);
            bool existed = System.IO.File.Exists(file);
            if (existed) return file;
            else return pathFile;
        }

        public static string FullDayOrMonthOrYear(string number, int lengthFirst, string value)
        {
            string valReturn = "";
            if (number.Length > 1)
                return number;
            else
            {
                for (int i = 0; i < lengthFirst; i++)
                {
                    valReturn += value;
                }
                valReturn += number;
                return valReturn;
            }
        }
    }
}