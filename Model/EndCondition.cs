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

        private readonly EndType _endType;
        public EndType EndType { get { return _endType; } }

        public Dictionary<string, string> Answers = new Dictionary<string, string>();

        public EndCondition()
        { 
            _endType = EndType.Single;
            Answers.Add("", "");
        }

        public EndCondition(EndType endType)
        {
            _endType = endType;
            if (_endType == EndType.Single)
            { Answers.Add("", ""); }
            else if (_endType == EndType.YesNo)
            {
                Answers.Add("yes", "");
                Answers.Add("no", "");
            }
        }
    }
}