using System.Collections.Generic;

namespace MultiBranchTexter.Model
{
    public class TextNode
    {
        // 文件名
        public string Name = "";
        // 文字内容
        public string Text = "";
        // 前节点
        private List<TextNode> preNodes = new List<TextNode>();
        // 后节点
        private List<TextNode> postNodes = new List<TextNode>();
        

        public TextNode() { }
        // TODO
        public TextNode(string name, string text)
        {
            Name = name;
            Text = text;
        }

        #region 添加与删除前后节点
        #region 静态方法
        public static void Link(TextNode pre, TextNode post)
        {
            pre.AddPostNode(post);
            post.AddPreNode(pre);
        }
        public static void UnLink(TextNode pre, TextNode post)
        {
            pre.DeletePostNode(post);
            post.DeletePreNode(pre);
        }
        #endregion

        public void AddPreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Add(node);
        }
        public void AddPostNode(TextNode node)
        {
            //TODO 判断是否已经存在
            postNodes.Add(node);
        }
        public void DeletePreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Remove(node);
        }
        public void DeletePostNode(TextNode node)
        {
            //TODO 判断是否已经存在
            postNodes.Remove(node);
        }
        #endregion

        /// <summary>
        /// 得到postNodes在参数List中的指标，目前是根据内存地址判断
        /// </summary>
        /// <param name="textNodes"></param>
        /// <returns></returns>
        public List<int> GetPostNodeIndex(List<TextNode> textNodes)
        {
            List<int> vs = new List<int>();
            for (int i = 0; i < textNodes.Count; i++)
            {
                if (postNodes.Contains(textNodes[i]))
                {
                    vs.Add(i);
                }
                //for (int j = 0; j < postNodes.Count; j++)
                //{
                //    if (textNodes[i].Name == postNodes[j].Name)
                //    { vs.Add(i); }
                //}
            }
            return vs;
        }
    }
}
