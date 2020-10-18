using System;
using System.Linq;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutoComposer
{
    class Program
    {
        static void Main(string[] args)
        {
            mainAsync(args).Wait();
        }

        static async Task mainAsync(string[] args)
        {
            try
            {
                string batchFile = getCallFileName(args);
                if (string.IsNullOrWhiteSpace(batchFile))
                {
                    Log.Error("Batch file not found.");
                    return;
                }

                string newest = await getNewestVersion();
                Log.Debug($"Packagist version : {newest}");

                string composer = await getLocalVersion();
                Log.Debug($"Local version : {composer}");

                if (isUpdate(newest, composer))
                {
                    Log.Debug($"Call update start");
                    Directory.SetCurrentDirectory("..");
                    var process = Process.Start(batchFile);
                    process.WaitForExit();
                }
                else
                {
                    Log.Debug($"Not call update");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        private static async Task<string> getNewestVersion()
        {
            Log.Debug($"getNewestVersion Start");
            using (var client = new HttpClient())
            {
                var result1 = await client.GetAsync(@"https://repo.packagist.org/p/exceedone/exment.json"); // GET
                string jsonstring = await result1.Content.ReadAsStringAsync();

                var json = JObject.Parse(jsonstring);

                var packages = (JObject)json["packages"]["exceedone/exment"];

                var newest = packages.Properties()
                    .Where(
                        x =>
                        {
                            return !x.Name.StartsWith("dev-");
                        }
                    )
                    .OrderByDescending(
                        x => x.First["time"]
                    )
                .FirstOrDefault();

                Log.Debug($"getNewestVersion End");

                return newest.Name;
            }
        }

        private static async Task<string> getLocalVersion()
        {
            Log.Debug($"getLocalVersion Start");
            string composer_lock = null;
            using (StreamReader sr = new StreamReader(Path.Combine("..", "composer.lock"), System.Text.Encoding.UTF8))
            {
                composer_lock = await sr.ReadToEndAsync();
            }

            var json = JObject.Parse(composer_lock);

            var packages = (JArray)json["packages"];

            var exment = packages.Where(
                    x =>
                    {
                        return x?["name"]?.ToString() == "exceedone/exment";
                    }
                )
            .FirstOrDefault();

            Log.Debug($"getLocalVersion End");

            return exment?["version"]?.ToString();

        }

        private static bool isUpdate(string newest, string composer)
        {
            if (string.IsNullOrWhiteSpace(composer))
            {
                return false;
            }

            if (composer.StartsWith("dev-"))
            {
                return false;
            }

            newest = newest.Replace("v", "");
            composer = composer.Replace("v", "");

            return (new System.Version(composer)).CompareTo(new System.Version(newest)) < 0;
        }

        private static string getCallFileName(string[] args)
        {
            List<string> files = new List<string>();
            if(args.Length == 0)
            {
                files.Add("ExmentUpdateWindows.bat");
            }
            else
            {
                files.Add(args[0]);
            }

            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(currentDirectory);

            foreach (var file in files)
            {
                var path = Path.Combine("..", file);
                if (File.Exists(path))
                {
                    return file;
                }
            }

            return null;
        }
    }
}
