using System.Collections.Generic;

namespace MultiBranchTexter.Model
{
    /// <summary>
    /// textNode的结尾条件
    /// </summary>
    public class EndCondition
    {
        public string Question;

        public EndType EndType;

        public bool IsQuestion = true;

        public List<(string, string, string)> Answers = new List<(string, string, string)>();

        public EndCondition()
        {
            EndType = EndType.Single;
        }

        public EndCondition(EndType endType)
        {
            EndType = endType;
            if (EndType == EndType.Single)
            { Answers.Add(("", "", "")); }
            else if (EndType == EndType.YesNo)
            {
                Answers.Add(("yes", "", ""));
                Answers.Add(("no", "", ""));
            }
        }
    }
}