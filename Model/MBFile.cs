using System.Collections.Generic;
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
            //检查文件是否存在
        }

        //开始读取，将返回一个已经连接完成的textNode的List
        public List<TextNode> Read()
        {
            if (!File.Exists(Path))
            {
                MessageBox.Show("文件不存在！");
                return null;
            }
            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs, Encoding.Default);

            List<TextNode> result = new List<TextNode>();
            string line = "";
            string dirPath = Path.Substring(0, Path.LastIndexOf("\\") + 1);
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
                //去掉回车、空格、水平制表符和换行
                line.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                //如果遇到[Connections]进入下一阶段
                if (line == "[Connections]")
                { break; }
                if (line == "")
                { continue; }
                //判断是否存在这个文件
                if (!File.Exists(dirPath + line))
                {
                    //汇报已经发生错误
                    continue;
                }
                //尝试读取这个文件并转化为TextNode
                FileStream txtFs = new FileStream(dirPath + line, FileMode.Open, FileAccess.Read);
                StreamReader txtReader = new StreamReader(txtFs, Encoding.Default);
                TextNode node = new TextNode(line, txtReader.ReadToEnd());
                result.Add(node);
                txtFs.Close();
                txtReader.Close();
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
                        {
                            Question = vs[2]
                        };
                        result[int.Parse(vs[0])].endCondition = condition;
                        TextNode.Link(result[int.Parse(vs[0])], result[int.Parse(vs1[0])], true);
                        TextNode.Link(result[int.Parse(vs[0])], result[int.Parse(vs1[1])], false);
                    }
                    else//普通连接
                    {
                        TextNode.Link(result[int.Parse(vs[0])],result[int.Parse(vs[1])]);
                    }
                    //以,分隔后一段字符串

                    //为Node添加postNode
                    //for (int i = 0; i < vs1.Length; i++)
                    //{
                    //    postIndex = int.Parse(vs1[i]);
                    //    result[preIndex].AddPostNode(result[postIndex]);
                    //    result[postIndex].AddPreNode(result[preIndex]);
                    //}
                }
                catch
                {
                    MessageBox.Show("读取失败！");
                    return null;
                }
            }
            //关闭流
            reader.Close();
            fs.Close();
            //结果
            return result;
        }
    }

    //mbtxt文件写入器
    public class MBFileWriter
    { }
}
