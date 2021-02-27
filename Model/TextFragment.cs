using System;
using System.Collections.Generic;
using System.Text;

namespace MultiBranchTexter.Model
{
    public class TextFragment
    {
        public string Comment;
        public string Content;
        public TextFragment() { }
        public TextFragment(string newContent)
        { Content = newContent; }
    }

    [Obsolete]
    public class OldTextFragment
    {
        public List<string> Operations = new List<string>();
        public string Content;
        public OldTextFragment() { }
        public TextFragment ToTextFragment()
        {
            string newComment = "";
            for (int i = 0;i<Operations.Count;i++)
            {
                newComment += Operations[i];
            }
            return new TextFragment()
            {
                Comment = newComment,
                Content = Content
            };
        }
    }
}
