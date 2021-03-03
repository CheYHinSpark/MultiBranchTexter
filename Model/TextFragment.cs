using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MultiBranchTexter.Model
{
    public class TextFragment : INotifyPropertyChanged
    {
        #region 实现接口，可以绑定
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region 核心数据
        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == null)
                { value = ""; }
                _comment = value; 
                ShouldRecount = true; 
                RaisePropertyChanged("Comment");
            }
        }
        private string _content;
        public string Content
        {
            get { return _content; }
            set 
            {
                if (value == null)
                { value = ""; }
                _content = value; 
                ShouldRecount = true; 
                RaisePropertyChanged("Content");
            }
        }
        #endregion

        #region 统计用
        [JsonIgnore]
        public bool ShouldRecount = true;//是否要重新统计字数
        [JsonIgnore]
        private Tuple<int, int> charWordCount = new Tuple<int, int>(0, 0);//字数统计
        #endregion

        public TextFragment() { }
        public TextFragment(string newContent)
        { Content = newContent; }

        public Tuple<int, int> CountCharWord()
        {
            if (ShouldRecount)
            {
                ShouldRecount = false;
                int charCount = 0;
                int wordCount = 0;
                bool isInLetterWords = false;//表示是否正在字母文字的单词中
                if (Content != null)
                {
                    foreach (char ch in Content)
                    {
                        if (char.IsWhiteSpace(ch))
                        { isInLetterWords = false; continue; }
                        charCount++;

                        //如果这是一个字母
                        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')//基本英文字母
                            || (ch >= 0100 && ch <= 017F) || (ch >= 0180 && ch <= 024F)//拉丁文扩展
                            )
                        {
                            if (!isInLetterWords)
                            {
                                isInLetterWords = true;
                                wordCount++;
                            }
                            continue;
                        }
                        else if (isInLetterWords)
                        { isInLetterWords = false; }

                        //如果是汉字
                        if ((0x4e00 <= ch && ch <= 0x9fff)
                            || (0x3400 <= ch && ch <= 0x4dff)
                            || (0x20000 <= ch && ch <= 0x2a6df)
                            || (0xf900 <= ch && ch <= 0xfaff)
                            || (0x2f800 <= ch && ch <= 0x2fa1f))
                        { wordCount++; }
                    }
                }
                if (ViewModelFactory.Settings.CountOpChar)
                {
                    isInLetterWords = false;
                    if (Comment != null)
                    {
                        foreach (char ch in Comment)
                        {
                            if (char.IsWhiteSpace(ch))
                            { isInLetterWords = false; continue; }
                            charCount++;

                            //如果这是一个字母
                            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')//基本英文字母
                                || (ch >= 0100 && ch <= 017F) || (ch >= 0180 && ch <= 024F)//拉丁文扩展
                                )
                            {
                                if (!isInLetterWords)
                                {
                                    isInLetterWords = true;
                                    wordCount++;
                                }
                                continue;
                            }
                            else if (isInLetterWords)
                            { isInLetterWords = false; }

                            //如果是汉字
                            if ((0x4e00 <= ch && ch <= 0x9fff)
                                || (0x3400 <= ch && ch <= 0x4dff)
                                || (0x20000 <= ch && ch <= 0x2a6df)
                                || (0xf900 <= ch && ch <= 0xfaff)
                                || (0x2f800 <= ch && ch <= 0x2fa1f))
                            { wordCount++; }
                        }
                    }
                }
                charWordCount = new Tuple<int, int>(charCount, wordCount);
            }
            return charWordCount;
        }

        /// <summary> 
        /// 将Comments转换为一个operation的list
        /// 游戏开发导出JSON使用
        /// </summary>
        public OperationTextFragment ToOperation()
        {
            if (Comment == null)
            { Comment = ""; }
            string[] preOps = Comment.Split(new char[] { '\r', '\n' });
            List<string> vs = new List<string>();
            for (int i =0;i<preOps.Length;i++)
            {
                string p = preOps[i].Replace(" ", "").Replace("\t", "");
                if (p != "")
                { vs.Add(p); }
            }
            return new OperationTextFragment() { Content = Content, Operations = vs };
        }
    }

    public class OperationTextFragment
    {
        public List<string> Operations = new List<string>();
        public string Content;
        public OperationTextFragment() { }
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
