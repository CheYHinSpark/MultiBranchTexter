using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 帮助寻找控件的类
    /// </summary>
    public static class ControlTreeHelper
    {
        /// <summary>
        /// 递归找到目标类型的父控件
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="reference">依赖对象</param>
        /// <returns></returns>
        public static T FindParentOfType<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject dObj = VisualTreeHelper.GetParent(reference);
            if (dObj == null)
                throw new Exception("无法找到目标类型为" + typeof(T).ToString() + "的父控件");
            if (dObj.GetType() == typeof(T))
            {
                //Debug.WriteLine("找到目标控件" + typeof(T).ToString());//这句出现得太多了。。。
                return (T)dObj; 
            }
            else
                return FindParentOfType<T>(dObj);
        }
    }
}
