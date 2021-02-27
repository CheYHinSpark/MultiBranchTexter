using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application = System.Windows.Application;
using MultiBranchTexter.ViewModel;

namespace MultiBranchTexter.Model
{
    public class UpdateChecker
    {
        public static async Task<bool> CheckGitHubNewerVersion()
        {
            try
            {
                //Get all releases from GitHub
                //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
                GitHubClient client = new GitHubClient(new ProductHeaderValue("CheYHinSpark"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("CheYHinSpark", "MultiBranchTexter");

                //Setup the versions
                Version latestGitHubVersion = new Version(releases[0].TagName);
                Version tempV = Application.ResourceAssembly.GetName().Version;
                //只要前三位
                Version localVersion = new Version(tempV.Major, tempV.Minor, tempV.Build);
                //修改版本号请在MultiBranchTexter.csproj-->AssemblyVersion

                //Compare the Versions
                //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
                ViewModelFactory.Settings.NewVersionInfo = latestGitHubVersion.ToString();
                int versionComparison = localVersion.CompareTo(latestGitHubVersion);
                return versionComparison != 0;
            }
            catch { }
            return false;
        }
    }
}
