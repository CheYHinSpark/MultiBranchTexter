using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class FCCViewModel: ViewModelBase
    {
        #region 字段
        private Visibility _searchBoxVisibility;
        public Visibility SearchBoxVisibility
        {
            get { return _searchBoxVisibility; }
            set
            { _searchBoxVisibility = value; RaisePropertyChanged("SearchBoxVisibility"); }
        }
        private ObservableCollection<NodeButton> _selectedNodes;
        public ObservableCollection<NodeButton> SelectedNodes
        {
            get { return _selectedNodes; }
            set
            { _selectedNodes = value; RaisePropertyChanged("SelectedNodes"); }
        }
        private ObservableCollection<NodeButton> _searchedNodes;
        public ObservableCollection<NodeButton> SearchedNodes
        {
            get { return _searchedNodes; }
            set
            { _searchedNodes = value; RaisePropertyChanged("SearchedNodes"); }
        }
        #endregion

        #region 命令
        public ICommand NewNodeCommand => new RelayCommand((t) =>
        {
            FlowChartContainer fcc = (Application.Current.MainWindow as MainWindow).GetFCC();
            Point point = Mouse.GetPosition(fcc.container);
            NodeButton newNode = new NodeButton(new TextNode(fcc.GetNewName()));
            newNode.SetParent(fcc.container);
            fcc.container.Children.Add(newNode);
            Canvas.SetLeft(newNode, Math.Max(0, point.X - 50));
            Canvas.SetTop(newNode, Math.Max(0, point.Y - 25));
            Debug.WriteLine("新建节点成功");
            fcc.IsModified = "*";
        });

        public ICommand UniteXCommand => new RelayCommand((sender) =>
        {
            string mode = (string)sender;
            double nX = Canvas.GetLeft(SelectedNodes[0]);
            if (mode == "avg")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                { nX += Canvas.GetLeft(SelectedNodes[i]); }
                nX /= SelectedNodes.Count;
            }
            else if (mode == "min")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetLeft(SelectedNodes[i]);
                    if (nX > temp)
                    { nX = temp; }
                }
            }
            else
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetLeft(SelectedNodes[i]);
                    if (nX < temp)
                    { nX = temp; }
                }
            }
            //统一
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Canvas.SetLeft(SelectedNodes[i], nX);
                SelectedNodes[i].UpdatePostLines();
                SelectedNodes[i].UpdatePreLines();
            }
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
        });

        public ICommand UniteYCommand => new RelayCommand((sender) =>
        {
            string mode = (string)sender;
            double nY = Canvas.GetTop(SelectedNodes[0]);
            if (mode == "avg")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                { nY += Canvas.GetTop(SelectedNodes[i]); }
                nY /= SelectedNodes.Count;
            }
            else if (mode == "min")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetTop(SelectedNodes[i]);
                    if (nY > temp)
                    { nY = temp; }
                }
            }
            else
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetTop(SelectedNodes[i]);
                    if (nY < temp)
                    { nY = temp; }
                }
            }
            //统一
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Canvas.SetTop(SelectedNodes[i], nY);
                SelectedNodes[i].UpdatePostLines();
                SelectedNodes[i].UpdatePreLines();
            }
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
        });

        public ICommand DeleteCommand => new RelayCommand((sender) =>
        {
            if (SelectedNodes.Count == 0)
            { return; }
            Debug.WriteLine("准备删除节点");
            int n = SelectedNodes.Count;
            MessageBoxResult warnResult = MessageBox.Show
                (
                Application.Current.MainWindow,
                "你即将删除" + n.ToString()
                + "个节点！\n这将同时断开这些节点的所有连接线，并且此操作不可撤销！",
                "警告",
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            while (SelectedNodes.Count > 0)
            {
                SelectedNodes[0].Delete();
                SelectedNodes.RemoveAt(0);
            }
            SelectedNodes.Clear();
            Debug.WriteLine("删除了" + n.ToString() + "个节点");
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
        });

        public ICommand StartSearchCommand => new RelayCommand((t) =>
        { SearchBoxVisibility = Visibility.Visible; });
        #endregion

        public FCCViewModel()
        {
            SearchBoxVisibility = Visibility.Hidden;
            SelectedNodes = new ObservableCollection<NodeButton>();
            SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged; 
            SearchedNodes = new ObservableCollection<NodeButton>();
            SearchedNodes.CollectionChanged += SearchedNodes_CollectionChanged; ;
        }

        private void SearchedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        { RaisePropertyChanged("SearchedNodes"); }

        private void SelectedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        { RaisePropertyChanged("SelectedNodes"); }

        #region 方法
        public void ClearSelection()
        {
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                if (SearchedNodes.Contains(SelectedNodes[i]))
                { SelectedNodes[i].NodeState = NodeState.Searched; }
                else
                { SelectedNodes[i].NodeState = NodeState.Normal; }
            }
            SelectedNodes.Clear();
        }

        public void ClearSearch()
        {
            for (int i = 0; i < SearchedNodes.Count; i++)
            {
                if (SelectedNodes.Contains(SearchedNodes[i]))
                { SearchedNodes[i].NodeState = NodeState.Selected; }
                else
                { SearchedNodes[i].NodeState = NodeState.Normal; }
            }
            SearchedNodes.Clear();
        }
        #endregion
    }
}
