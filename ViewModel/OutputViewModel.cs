namespace MultiBranchTexter.ViewModel
{
    public class OutputViewModel : ViewModelBase
    {
        private bool _outputComment;

        public bool OutputComment
        {
            get { return _outputComment; }
            set
            { _outputComment = value; RaisePropertyChanged("OutputComment"); }
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

        private string _commentLinePre;

        public string CommentLinePre
        {
            get { return _commentLinePre; }
            set
            { _commentLinePre = value; RaisePropertyChanged("CommentLinePre"); }
        }

        private string _commentLinePost;

        public string CommentLinePost
        {
            get { return _commentLinePost; }
            set
            { _commentLinePost = value; RaisePropertyChanged("CommentLinePost"); }
        }

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

        public OutputViewModel()
        {
            OutputComment = false;
            CommentParPre = "";
            CommentParPost = "";
            CommentLinePre = "";
            CommentLinePost = "";
            ContentParPre = "";
            ContentParPost = "";
            IsCommentBefore = true;
        }
    }
}
