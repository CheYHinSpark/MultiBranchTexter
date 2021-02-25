using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MultiBranchTexter.Model
{
    public class UpdateChecker
    {
        public static async Task CheckGitHubNewerVersion()
        {
            try
            {
                //Get all releases from GitHub
                //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
                GitHubClient client = new GitHubClient(new ProductHeaderValue("CheYHinSpark"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("CheYHinSpark", "MultiBranchTexter");

                //Setup the versions
                Version latestGitHubVersion = new Version(releases[0].TagName);
                Version localVersion = new Version("1.0.0"); //Replace this with your local version. 
                                                             //Only tested with numeric values.

                //Compare the Versions
                //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
                int versionComparison = localVersion.CompareTo(latestGitHubVersion);
                if (versionComparison != 0)
                {
                    Process.Start("explorer.exe", "https://github.com/CheYHinSpark/MultiBranchTexter");
                }
                else
                {
                    MessageBox.Show("当前已是最新版本");
                }
            }
            catch { }
        }
    }
}
