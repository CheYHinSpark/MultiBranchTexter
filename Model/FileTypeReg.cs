using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Permissions;

namespace MultiBranchTexter.Model
{

    /// <summary>
    /// 文件类型注册信息
    /// </summary>
    public class FileTypeRegInfo
    {
        /// <summary> 扩展名 </summary>  
        public string ExtendName;  //".mbjson"  
        /// <summary> 说明 </summary>  
        public string Description; //"多分支导航文件"  
        /// <summary> 关联的图标 </summary>  
        public string IconPath;
        /// <summary> 应用程序路径 </summary>  
        public string ExePath;

        public FileTypeRegInfo(string extendName)
        {
            this.ExtendName = extendName;
        }
    }

    /// <summary>  
    /// 注册自定义的文件类型。  
    /// </summary>  
    public static class FileTypeRegister
    {
        /// <summary>  
        /// 使文件类型与对应的图标及应用程序关联起来
        /// </summary>          
        // .NET6中，这个已经过时 [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        public static void RegisterFileType(FileTypeRegInfo regInfo)
        {
            //HKEY_CLASSES_ROOT/.mbjson
            RegistryKey fileTypeKey = Registry.ClassesRoot.CreateSubKey(regInfo.ExtendName);
            string relationName = regInfo.ExtendName[1..].ToUpper() + "_FileType";
            fileTypeKey.SetValue("", relationName);
            fileTypeKey.Close();

            //HKEY_CLASSES_ROOT/MBJSON_FileType
            RegistryKey relationKey = Registry.ClassesRoot.CreateSubKey(relationName);
            relationKey.SetValue("", regInfo.Description);

            //HKEY_CLASSES_ROOT/MBJSON_FileType/Shell/DefaultIcon
            RegistryKey iconKey = relationKey.CreateSubKey("DefaultIcon");
            iconKey.SetValue("", regInfo.IconPath);

            //HKEY_CLASSES_ROOT/MBJSON_FileType/Shell
            RegistryKey shellKey = relationKey.CreateSubKey("Shell");

            //HKEY_CLASSES_ROOT/MBJSON_FileType/Shell/Open
            RegistryKey openKey = shellKey.CreateSubKey("Open");

            //HKEY_CLASSES_ROOT/MBJSON_FileType/Shell/Open/Command
            RegistryKey commandKey = openKey.CreateSubKey("Command");
            commandKey.SetValue("", regInfo.ExePath + " %1"); // " %1"表示将被双击的文件的路径传给目标应用程序
            relationKey.Close();
        }

        /// <summary>  
        /// 更新指定文件类型关联信息  
        /// </summary>      
        public static bool UpdateFileTypeRegInfo(FileTypeRegInfo regInfo)
        {
            if (FileTypeRegistered(regInfo.ExtendName) == null)
            {
                return false;
            }

            string extendName = regInfo.ExtendName;
            string relationName = extendName[1..].ToUpper() + "_FileType";
            RegistryKey relationKey = Registry.ClassesRoot.OpenSubKey(relationName, true);
            relationKey.SetValue("", regInfo.Description);
            RegistryKey iconKey = relationKey.OpenSubKey("DefaultIcon", true);
            iconKey.SetValue("", regInfo.IconPath);
            RegistryKey shellKey = relationKey.OpenSubKey("Shell");
            RegistryKey openKey = shellKey.OpenSubKey("Open");
            RegistryKey commandKey = openKey.OpenSubKey("Command", true);
            commandKey.SetValue("", regInfo.ExePath + " %1");
            relationKey.Close();
            return true;
        }

        /// <summary>  
        /// 获取指定文件类型关联信息  
        /// </summary>          
        public static FileTypeRegInfo GetFileTypeRegInfo(string extendName)
        {
            if (FileTypeRegistered(extendName) == null)
            {
                return null;
            }
            FileTypeRegInfo regInfo = new FileTypeRegInfo(extendName);

            string relationName = extendName[1..].ToUpper() + "_FileType";
            RegistryKey relationKey = Registry.ClassesRoot.OpenSubKey(relationName);
            regInfo.Description = relationKey.GetValue("").ToString();
            RegistryKey iconKey = relationKey.OpenSubKey("DefaultIcon");
            regInfo.IconPath = iconKey.GetValue("").ToString();
            RegistryKey shellKey = relationKey.OpenSubKey("Shell");
            RegistryKey openKey = shellKey.OpenSubKey("Open");
            RegistryKey commandKey = openKey.OpenSubKey("Command");
            string temp = commandKey.GetValue("").ToString();
            regInfo.ExePath = temp[0..^3];
            return regInfo;
        }

        /// <summary>  
        /// 指定文件类型是否已经注册  
        /// </summary>
        /// <returns>true表示正是本人被注册，false表示不是本人，null表示没有注册</returns>
        public static bool? FileTypeRegistered(string extendName)
        {
            RegistryKey softwareKey = Registry.ClassesRoot.OpenSubKey(extendName);
            if (softwareKey == null)
            {
                return null;
            }
            try
            {
                string relationName = extendName[1..].ToUpper() + "_FileType";
                RegistryKey commandKey = Registry.ClassesRoot
                    .OpenSubKey(relationName)
                    .OpenSubKey("Shell")
                    .OpenSubKey("Open")
                    .OpenSubKey("Command");
                string ExePath = commandKey.GetValue("").ToString()[0..^3];
                return ExePath == Process.GetCurrentProcess().MainModule.FileName;
            }
            catch { }
            return false;
        }
    }
}
