namespace MultiBranchTexter.ViewModel
{
    public class OutputViewModel : ViewModelBase
    {
        #region 字段

        #region 节点前后
        private string _nodeParPre;

        public string NodeParPre
        {
            get { return _nodeParPre; }
            set
            { _nodeParPre = value; RaisePropertyChanged("NodeParPre"); }
        }

        private string _nodeParPost;

        public string NodeParPost
        {
            get { return _nodeParPost; }
            set
            { _nodeParPost = value; RaisePropertyChanged("NodeParPost"); }
        }
        #endregion

        #region 注释
        private bool _outputComment;

        public bool OutputComment
        {
            get { return _outputComment; }
            set
            { _outputComment = value; RaisePropertyChanged("OutputComment"); }
        }

        private bool _ignoreEmptyComment;

        public bool IgnoreEmptyComment
        {
            get { return _ignoreEmptyComment; }
            set
            { _ignoreEmptyComment = value; RaisePropertyChanged("IgnoreEmptyComment"); }
        }

        private string _commentParPre;

        public string CommentParPre
        {
            get { return _commentParPre; }
            set
            { _commentParPre = value; RaisePropertyChanged("CommentParPre"); }
        }

        private string _commentParPost;

        public string CommentParPost
        {
            get { return _commentParPost; }
            set
            { _commentParPost = value; RaisePropertyChanged("CommentParPost"); }
        }
        #endregion

        #region 文本
        private string _contentParPre;

        public string ContentParPre
        {
            get { return _contentParPre; }
            set
            { _contentParPre = value; RaisePropertyChanged("ContentParPre"); }
        }

        private string _contentParPost;

        public string ContentParPost
        {
            get { return _contentParPost; }
            set
            { _contentParPost = value; RaisePropertyChanged("ContentParPost"); }
        }

        private bool _isCommentBefore;

        public bool IsCommentBefore
        {
            get { return _isCommentBefore; }
            set
            { _isCommentBefore = value; RaisePropertyChanged("IsCommentBefore"); }
        }
        #endregion

        #region 后继
        private bool _outputEnd;

        public bool OutputEnd
        {
            get { return _outputEnd; }
            set
            { _outputEnd = value; RaisePropertyChanged("OutputEnd"); }
        }

        private string _qPattern;

        public string QPattern
        {
            get { return _qPattern; }
            set
            { _qPattern = value; RaisePropertyChanged("QPattern"); }
        }

        private string _aPattern;

        public string APattern
        {
            get { return _aPattern; }
            set
            { _aPattern = value; RaisePropertyChanged("APattern"); }
        }

        private bool _ignoreEmptyAnswer;

        public bool IgnoreEmptyAnswer
        {
            get { return _ignoreEmptyAnswer; }
            set
            { _ignoreEmptyAnswer = value; RaisePropertyChanged("IgnoreEmptyAnswer"); }
        }
        #endregion

        #endregion

        public OutputViewModel()
        {
            NodeParPre = "";
            NodeParPost = "";
            OutputComment = false;
            IgnoreEmptyComment = true;
            CommentParPre = "";
            CommentParPost = "";
            ContentParPre = "";
            ContentParPost = "";
            IsCommentBefore = true;
            OutputEnd = false;
            IgnoreEmptyAnswer = true;
            QPattern = "##";
            APattern = "## @@";
        }
    }
}
