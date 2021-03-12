using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MultiBranchTexter.Model
{
    public class IniFile
    {
        private readonly string fileName;//必须是完整路径
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileInt(
            string lpAppName,// 指向包含 Section 名称的字符串地址 
            string lpKeyName,// 指向包含 Key 名称的字符串地址 
            int Default,// 如果 Key 值没有找到，则返回缺省的值是多少 
            string lpFileName
        );

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
            string lpAppName,// 指向包含 Section 名称的字符串地址 
            string lpKeyName,// 指向包含 Key 名称的字符串地址 
            string Default,// 如果 Key 值没有找到，则返回缺省的字符串的地址 
            StringBuilder lpReturnedString,// 返回字符串的缓冲区地址 
            int nSize,// 缓冲区的长度 
            string lpFileName
        );

        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(
            string lpAppName,// 指向包含 Section 名称的字符串地址 
            string lpKeyName,// 指向包含 Key 名称的字符串地址 
            string lpString,// 要写的字符串地址 
            string lpFileName
        );

        public IniFile(string filename)
        {
            fileName = filename;
            FileInfo fi = new FileInfo(filename);
            if (!fi.Exists)//写入默认配置
            {
                try
                {
                    StreamWriter sw = new StreamWriter(new FileStream("Settings.ini", FileMode.Create));
                    sw.Write("[Settings]\n" +
                        "IsDarkMode=false\n" +
                        "AllowDoubleEnter=false\n" +
                        "CountOpChar=false\n" +
                        "DoubleSaveAwake=true\n" +
                        "SideWidth=10\n" +
                        "[Color]\n" +
                        "Red=238\n" +
                        "Green=170\n" +
                        "Blue=22");
                    sw.Flush();
                    sw.Close();
                }
                catch { }
            }
        }

        public int GetInt(string section, string key, int def)
        {
            return GetPrivateProfileInt(section, key, def, fileName);
        }

        public string GetString(string section, string key, string def)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, temp, 1024, fileName);
            return temp.ToString();
        }

        public bool GetBool(string section, string key, bool def)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def.ToString(), temp, 1024, fileName);
            return temp.ToString() == "true" || temp.ToString() == "True";
        }

        public void WriteInt(string section, string key, int iVal)
        {
            WritePrivateProfileString(section, key, iVal.ToString(), fileName);
        }

        public void WriteString(string section, string key, string strVal)
        {
            WritePrivateProfileString(section, key, strVal, fileName);
        }

        public void WriteBool(string section, string key, bool bVal)
        {
            WritePrivateProfileString(section, key, bVal.ToString(), fileName);
        }

        public void DelKey(string section, string key)
        {
            WritePrivateProfileString(section, key, null, fileName);
        }

        public void DelSection(string section)
        {
            WritePrivateProfileString(section, null, null, fileName);
        }
    }
}
