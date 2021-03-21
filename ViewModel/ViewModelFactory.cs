using System;
using System.Collections.Generic;
using System.Windows;

namespace MultiBranchTexter.ViewModel
{
    /// <summary>
    /// 为单例ViewModel定位
    /// </summary>
    public class ViewModelFactory
    {
        public static object GetViewModel(Type vm_type)
        {
            Dictionary<string, object> dic = Application.Current.Properties["ViewModelMap"] as Dictionary<string, object>;
            if (dic.ContainsKey(vm_type.FullName))
            {
                return dic[vm_type.FullName];
            }
            else
            {
                object new_vm = Activator.CreateInstance(vm_type);
                dic.Add(vm_type.FullName, new_vm);
                return new_vm;
            }
        }

        public static void SetViewModel(Type vm_type, ViewModelBase vm)
        {
            Dictionary<string, object> dic = Application.Current.Properties["ViewModelMap"] as Dictionary<string, object>;
            if (dic.ContainsKey(vm_type.FullName))
            { dic[vm_type.FullName] = vm; }
            else
            { dic.Add(vm_type.FullName, vm); }
        }

        /// <summary> 获得唯一的MainViewModel </summary>
        public static MainViewModel Main
        { get { return GetViewModel(typeof(MainViewModel)) as MainViewModel; } }

        /// <summary> 获得唯一的FCCViewModel </summary>
        public static FCCViewModel FCC
        { get { return GetViewModel(typeof(FCCViewModel)) as FCCViewModel; } }

        /// <summary> 获得唯一的SettingViewModel </summary>
        public static SettingViewModel Settings
        { get { return GetViewModel(typeof(SettingViewModel)) as SettingViewModel; } }

        /// <summary> 获得唯一的OutputViewModel </summary>
        public static OutputViewModel Output
        { get { return GetViewModel(typeof(OutputViewModel)) as OutputViewModel; } }
    }
}