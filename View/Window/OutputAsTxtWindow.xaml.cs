using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// OutputAsTxtWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OutputAsTxtWindow : OutputWindowBase
    {
        public OutputAsTxtWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModelFactory.Output;
        }

        protected override void Output()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(SavePath);
            if (!directoryInfo.Exists)
            { directoryInfo.Create(); }

            string mainTitle = ViewModelFactory
                .Main.FileName[(ViewModelFactory.Main.FileName.LastIndexOf('\\') + 1)..]
                .Replace(".mbjson", "");

            HashSet<TextNode> nodes = ViewModelFactory.FCC.Nodes;
            foreach (TextNode node in nodes)
            {
                string result = ModStr(ViewModelFactory.Output.NodeParPre, node.Name, mainTitle);
                for (int i = 0; i < node.Fragments.Count; i++)
                {
                    string comment = "";
                    if (ViewModelFactory.Output.OutputComment)
                    {
                        if (!ViewModelFactory.Output.IgnoreEmptyComment
                            && node.Fragments[i].Comment != "")
                        {
                            comment = ModStr(ViewModelFactory.Output.CommentParPre, node.Name, mainTitle) +
                                node.Fragments[i].Comment +
                                ModStr(ViewModelFactory.Output.CommentParPost, node.Name, mainTitle);
                        }
                    }
                    string content = ModStr(ViewModelFactory.Output.ContentParPre, node.Name, mainTitle) +
                        node.Fragments[i].Content +
                        ModStr(ViewModelFactory.Output.ContentParPost, node.Name, mainTitle);
                    if (ViewModelFactory.Output.IsCommentBefore)
                    {
                        if (comment != "")
                        { result += comment + "\r\n" + content + "\r\n"; }
                        else
                        { result += content + "\r\n"; }
                    }
                    else
                    {
                        if (comment != "")
                        { result += content + "\r\n" + comment + "\r\n"; }
                        else
                        { result += content + "\r\n"; }
                    }
                }

                if (ViewModelFactory.Output.OutputEnd)
                {
                    result += ModStr(ViewModelFactory.Output.QPattern, node.Name, mainTitle, node.EndCondition.Question);
                    result += "\r\n";
                    for (int j = 0; j < node.EndCondition.Answers.Count; j++)
                    {
                        if (ViewModelFactory.Output.IgnoreEmptyAnswer && node.EndCondition.Answers[j].Item2 == "")
                        { continue; }
                        result += ModStr(ViewModelFactory.Output.APattern, node.Name, mainTitle,
                            node.EndCondition.Answers[j].Item1, node.EndCondition.Answers[j].Item2);
                        result += "\r\n";
                    }
                }

                result += ModStr(ViewModelFactory.Output.NodeParPost, node.Name, mainTitle);

                File.WriteAllText(SavePath + "\\" + node.Name + ".txt", result);
            }
            Process.Start("explorer.exe", SavePath);
            Debug.WriteLine("导出成功");
            this.Close();
        }

        private string ModStr(string str, string nodeName, string mainTitle)
        {
            string[] vs = str.Split("&&");
            string result = "";
            for (int i = 0; i < vs.Length; i++)
            {
                if (i > 0)
                { result += nodeName; }
                string[] vs1 = vs[i].Split("$$");
                for (int j = 0; j < vs1.Length; j++)
                {
                    if (j > 0)
                    { result += mainTitle; }
                    result += vs1[j].Replace("\\&", "&").Replace("\\$", "$");
                }
            }
            return result;
        }

        private string ModStr(string str, string nodeName, string mainTitle, string q)
        {
            string[] vs = str.Split("&&");
            string result = "";
            for (int i = 0; i < vs.Length; i++)
            {
                if (i > 0)
                { result += nodeName; }
                string[] vs1 = vs[i].Split("$$");
                for (int j = 0; j < vs1.Length; j++)
                {
                    if (j > 0)
                    { result += mainTitle; }
                    string[] vs2 = vs1[j].Split("##");
                    for (int k = 0; k < vs2.Length; k++)
                    {
                        if (k > 0)
                        { result += q; }
                        result += vs2[k].Replace("\\$", "$")
                            .Replace("\\#", "#")
                            .Replace("\\&", "&");
                    }
                }
            }
            return result;
        }

        private string ModStr(string str, string nodeName, string mainTitle, string q, string a)
        {
            string[] vs = str.Split("&&");
            string result = "";
            for (int i = 0; i < vs.Length; i++)
            {
                if (i > 0)
                { result += nodeName; }
                string[] vs1 = vs[i].Split("$$");
                for (int j = 0; j < vs1.Length; j++)
                {
                    if (j > 0)
                    { result += mainTitle; }
                    string[] vs2 = vs1[j].Split("##");
                    for (int k = 0; k < vs2.Length; k++)
                    {
                        if (k > 0)
                        { result += q; }
                        string[] vs3 = vs2[k].Split("@@");
                        for (int l = 0; l < vs3.Length; l++)
                        {
                            if (l > 0)
                            { result += a; }
                            result += vs3[l].Replace("\\$", "$")
                                .Replace("\\#", "#")
                                .Replace("\\@", "@")
                                .Replace("\\&", "&");
                        }
                    }
                }
            }
            return result;
        }
    }
}
