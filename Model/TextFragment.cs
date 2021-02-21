using System;
using System.Collections.Generic;
using System.Text;

namespace MultiBranchTexter.Model
{
    public class TextFragment
    {
        public List<string> Operations;
        public string Content;
        public TextFragment() { }
        public TextFragment(string newContent)
        { Content = newContent; }
    }
}
