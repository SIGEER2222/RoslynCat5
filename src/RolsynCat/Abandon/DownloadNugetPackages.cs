//using NuGet;

namespace RoslynCat.Abandon
{
    public static class DownloadNugetPackages
    {
        private static string installationDirectory = Directory.GetCurrentDirectory() + "//NugetPackages//packages";

        public static List<Assembly> LoadPackages(string packages)
        {
            List<Assembly> assemblies = new List<Assembly>();
            if (!string.IsNullOrWhiteSpace(packages))
            {

                string[] npackages = packages.Split(';');
                foreach (var item in npackages)
                {
                    string downloadItem = "";
                    string version = "";
                    if (item.Contains(','))
                    {
                        downloadItem = item.Split(',')[0];
                        version = item.Split(',')[1];
                    }
                    else
                    {
                        downloadItem = item;

                    }
                    var path = $"{installationDirectory}//{downloadItem}";

                    var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var assembly = Assembly.LoadFile(file);
                            assemblies.Add(assembly);
                        }
                        catch (Exception)
                        {
                            // throw
                        }
                    }

                }
            }
            return assemblies;
        }
        public static void DownloadAllPackages(string packages)
        {
            if (!string.IsNullOrWhiteSpace(packages))
            {
                string[] npackages = packages.Split(';');
                foreach (var item in npackages)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        string downloadItem = "";
                        string version = "";
                        if (item.Contains(','))
                        {
                            downloadItem = item.Split(',')[0];
                            version = item.Split(',')[1];
                        }
                        else
                        {
                            downloadItem = item;
                        }
                        if (!string.IsNullOrWhiteSpace(version))
                        {
                            DownloadPackage(downloadItem, version);
                        }
                        else
                        {
                            DownloadPackage(downloadItem, null);
                        }
                    }
                }
            }
        }
        public static void DownloadPackage(string packageName, string version)
        {
            string packageInstallationDirectory = installationDirectory + $"//{packageName}";
            if (!File.Exists($"{packageInstallationDirectory}//{packageName}.nuget"))
            {
                if (!Directory.Exists(packageInstallationDirectory))
                {
                    Directory.CreateDirectory(packageInstallationDirectory);
                }
                //https://www.nuget.org/api/v2/package/NuGet.Core/2.14.0
                string url = "https://packages.nuget.org/api/v2/package/" + $"{packageName}";
                if (!string.IsNullOrWhiteSpace(version))
                {
                    url += $"/{version}";
                }

                var wc = new System.Net.WebClient();
                wc.DownloadFile(url, $"{packageInstallationDirectory}//{packageName}.nuget");

                System.IO.Compression.ZipFile.ExtractToDirectory($"{packageInstallationDirectory}//{packageName}.nuget", packageInstallationDirectory, true);

            }

        }


    }
}
