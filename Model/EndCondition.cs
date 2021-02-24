using System;
using System.Collections.Generic;
using System.Text;

namespace MultiBranchTexter.Model
{
    /// <summary>
    /// textNode的结尾条件
    /// </summary>
    public class EndCondition
    {
        public string Question;

        public EndType EndType;

        public Dictionary<string, string> Answers = new Dictionary<string, string>();

        public EndCondition()
        {
            EndType = EndType.Single;
            Answers.Add("", "");
        }

        public EndCondition(EndType endType)
        {
            EndType = endType;
            if (EndType == EndType.Single)
            { Answers.Add("", ""); }
            else if (EndType == EndType.YesNo)
            {
                Answers.Add("yes", "");
                Answers.Add("no", "");
            }
        }
    }
}