using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace MultiBranchTexter.Model
{
    //mbtxt文件阅读器
    public class MBFileReader
    {
        public string Path;
        //根据路径创建
        public MBFileReader(string uri)
        {
            //确保格式统一
            Path = uri.Replace("/", "\\");
            //TODO检查文件是否存在
        }

        //开始读取，将返回一个已经连接完成的textNode的List
        public List<TextNodeWithLeftTop> Read()
        {
            if (!File.Exists(Path))
            { throw new System.Exception("文件不存在！"); }

            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs, Encoding.Default);

            List<TextNodeWithLeftTop> result = new List<TextNodeWithLeftTop>();
            string dirPath = Path.Substring(0, Path.LastIndexOf("\\") + 1);
            string line;
            //第0阶段，如果读到文件夹信息则更换dirPath
            while ((line = reader.ReadLine()) != null)
            {
                //去掉回车、空格、水平制表符和换行
                line.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                //如果遇到[Nodes]进入下一阶段
                if (line == "[Nodes]")
                { break; }
                if (line != "")
                {
                    if (Directory.Exists(line.Replace("/", "\\")))
                    { dirPath = line + "\\"; }
                }
            }
            //第1阶段，添加节点
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    //去掉回车、空格、水平制表符和换行
                    line.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                    //如果遇到[Connections]进入下一阶段
                    if (line == "[Connections]")
                    { break; }
                    if (line == "")
                    { continue; }
                    //切割一下
                    string[] vs = line.Split(",");
                    //判断是否存在这个文件
                    if (!File.Exists(dirPath + vs[0]))
                    {
                        //汇报已经发生错误
                        throw new System.Exception("文件不存在");
                    }
                    //尝试读取这个文件并转化为TextNode
                    FileStream txtFs = new FileStream(dirPath + vs[0], FileMode.Open, FileAccess.Read);
                    StreamReader txtReader = new StreamReader(txtFs, Encoding.Default);
                    TextNode node = new TextNode(vs[0], txtReader.ReadToEnd());
                    result.Add(new TextNodeWithLeftTop(node, double.Parse(vs[1]), double.Parse(vs[2])));
                    txtFs.Close();
                    txtReader.Close();
                }
                catch
                {
                    MessageBox.Show("读取失败！");
                    return null;
                }
            }
            //第2阶段，添加节点连接方式
            while ((line = reader.ReadLine()) != null)
            {
                //去掉回车、空格、水平制表符和换行
                line.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                if (line == "")
                { continue; }
                try
                {
                    //以-分隔字符串
                    string[] vs = line.Split("-");
                    if (vs[1] == "yn")//是否判断
                    {
                        string[] vs1 = vs[3].Split(",");
                        YesNoCondition condition = new YesNoCondition
                        { Question = vs[2] };
                        result[int.Parse(vs[0])].Node.endCondition = condition;
                        if (int.TryParse(vs1[0], out int i))
                        { TextNode.Link(result[int.Parse(vs[0])].Node, result[i].Node, true); }
                        if (int.TryParse(vs1[1], out int j))
                        { TextNode.Link(result[int.Parse(vs[0])].Node, result[j].Node, false); }
                    }
                    else if (vs[1] == "ma")//多选
                    {
                        MultiAnswerCondition condition = new MultiAnswerCondition
                        { Question = vs[2] };
                        result[int.Parse(vs[0])].Node.endCondition = condition;
                        for (int i = 3; i < vs.Length; i++)
                        {
                            string[] vs1 = vs[i].Split(",");
                            if (int.TryParse(vs1[1], out int j))
                            { TextNode.Link(result[int.Parse(vs[0])].Node, result[j].Node, vs1[0]); }
                        }
                    }
                    else//普通连接
                    {
                        TextNode.Link(result[int.Parse(vs[0])].Node,result[int.Parse(vs[1])].Node);
                    }
                }
                catch
                {
                    MessageBox.Show("读取失败！");
                    return null;
                }
            }
            //关闭流
            fs.Flush();
            reader.Close();
            fs.Close();
            Debug.WriteLine("读取文件" + Path + "成功");
            //结果
            return result;
        }
    }

    //mbtxt文件写入器
    public class MBFileWriter
    {
        public string Path;
        //根据路径创建
        public MBFileWriter(string uri)
        {
            //确保格式统一
            Path = uri.Replace("/", "\\");
            //TODO检查文件是否存在
        }

        public void Write(List<TextNodeWithLeftTop> nodes)
        {
            if (!File.Exists(Path))
            { throw new System.Exception("文件不存在！"); }

            FileInfo fileInfo = new FileInfo(Path);
            string dir = fileInfo.DirectoryName;

            FileStream fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //清空文件
            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(0);
            StreamWriter writer = new StreamWriter(fs, Encoding.Default);
            writer.WriteLine("[Nodes]");
            for (int i = 0;i < nodes.Count;i++)
            {
                writer.WriteLine(nodes[i].Node.Name + ","
                    + nodes[i].Left.ToString() + ","
                    + nodes[i].Top.ToString());
                FileStream nodeFs = new FileStream(dir + "\\" + nodes[i].Node.Name,
                    FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter nodeWriter = new StreamWriter(nodeFs, Encoding.Default);
                nodeWriter.Write(nodes[i].Node.Text);
                nodeFs.Flush();
                nodeWriter.Close();
                nodeFs.Close();
            }
            writer.WriteLine("[Connections]");
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Node.endCondition == null)
                {
                    for (int j = 0; j < nodes.Count;j++)
                    {
                        if (nodes[i].Node.PostNodes.Contains(nodes[j].Node))
                        {
                            writer.WriteLine(i.ToString() + "-" + j.ToString());
                            break;
                        }
                    }
                }
                else if (nodes[i].Node.endCondition is YesNoCondition)
                {
                    YesNoCondition ync = nodes[i].Node.endCondition as YesNoCondition;
                    string vs = i.ToString() + "-yn-" + ync.Question + "-";
                    string yes = "";
                    string no = "";
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        if (ync.YesNode == nodes[j].Node)
                        { yes = j.ToString(); }
                        if (ync.NoNode == nodes[j].Node)
                        { no = j.ToString(); }
                    }
                    writer.WriteLine(vs + yes + "," + no);
                }
                else if (nodes[i].Node.endCondition is MultiAnswerCondition)
                {
                    MultiAnswerCondition mac = nodes[i].Node.endCondition as MultiAnswerCondition;
                    string vs = i.ToString() + "-ma-" + mac.Question;
                    foreach (AnswerToNode atn in mac.AnswerToNodes)
                    {
                        string an = "-" + atn.Answer + ",";
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            if (atn.PostNode == nodes[j].Node)
                            {
                                an += j.ToString();
                                break;
                            }
                        }
                        vs += an;
                    }
                    writer.WriteLine(vs);
                }
            }
            fs.Flush();
            writer.Close();
            fs.Close();
        }
    }
}
