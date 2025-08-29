using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Security.Policy;
using Ukagaka.Classes.Cocoa.AppKit;
using System.Collections;
using System.Windows.Documents;
using Ukagaka;

namespace Utils
{
    public class File
    {

        private string pathname = "";
        private string parent = "";
        private string child = "";
        private string filePath = "";
        private File parentFile;


        public File()
        {

        }


        public File(string pathname)
        {

            this.pathname = pathname;
            this.filePath = this.pathname;
        }

        public File(string parent, string child)
        {

            this.parent = parent;
            this.child = child;
            this.pathname = parent + "/"+ child;
            parentFile = new File(parent);

            this.filePath = this.pathname;
        }


        public File(string parent,string path, string child)
        {
            parentFile = new File(parent);

            this.parent = parent + "/" + path;
            this.child = child;
            this.pathname = this.parent + "/" + child;


            this.filePath = this.pathname;
        }


        public File(File parent, string child)
        {

            this.parent = parent.pathname;
            parentFile = parent;

            this.child = child;
            this.pathname = this.parent + "/" + this.child;
            this.filePath = this.pathname;


        }



        public File GetFile(string pathname)
        {
            File file = new File();
            file.pathname = pathname;
            file.filePath = pathname;
            return file;
        }

        public File GetFile(string parent, string child)
        {
            File file = new File();
            file.parent = parent;
            file.child = child;
            file.pathname = parent + child;
            file.filePath = pathname;
            return file;
        }

        public File GetFile(File parent, string child)
        {
            File file = new File();
            file.parent = parent.parent;
            file.child = child;
            file.pathname = file.parent + file.child;
            file.filePath = pathname;
            return file;
        }



        public string GetFullName()
        {
             return filePath;
        }


        public string GetName()
        {
            FileInfo fileInfo = new FileInfo(this.filePath);
             
            return fileInfo.Name;
        }
        //返回由此抽象路径名表示的文件或目录的名称。
        public string GetParent()
        {
            FileInfo fileInfo = new FileInfo(filePath);

            return fileInfo.Directory.Parent.FullName;
        }

         

        //返回此抽象路径名的父路径名的路径名字符串，如果此路径名没有指定父目录，则返回 null。
        public File GetParentFile()
        {
            if (this.parentFile == null)
            {
                return new File(filePath);
            }
            return this.parentFile;
        }
        //返回此抽象路径名的父路径名的抽象路径名，如果此路径名没有指定父目录，则返回 null。
        public string GetPath()
        {
            return this.filePath;
        }
        // 将此抽象路径名转换为一个路径名字符串。
        public bool IsAbsolute()
        {
            FileInfo fileInfo = new FileInfo(filePath);

            return false;
        }
        //测试此抽象路径名是否为绝对路径名。
        public string GetAbsolutePath()
        {

            return "";
        }
        //返回抽象路径名的绝对路径名字符串。

        public bool CanRead()
        {

            return true;
        }
        //测试应用程序是否可以读取此抽象路径名表示的文件。

        public bool CanWrite()
        {

            return true;
        }
        //测试应用程序是否可以修改此抽象路径名表示的文件。

        public bool Exists()
        {
             
            bool exists = false;
             
            FileInfo fileInfo = new FileInfo(filePath);
            exists = fileInfo.Exists;


            DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

            exists |= directoryInfo.Exists;


            return exists;
        }
        //测试此抽象路径名表示的文件或目录是否存在。

        public bool IsDirectory()
        {
             DirectoryInfo fileInfo = new DirectoryInfo(filePath);

            return fileInfo.Exists;
        }
        //测试此抽象路径名表示的文件是否是一个目录。
        public bool IsFile()
        {
            FileInfo fileInfo = new FileInfo(filePath);

            return fileInfo.Exists;
        }
        //测试此抽象路径名表示的文件是否是一个标准文件。
        public DateTime LastModified()
        {
            FileInfo fileInfo = new FileInfo(filePath);

            return fileInfo.LastWriteTime;
        }
        //返回此抽象路径名表示的文件最后一次被修改的时间。
        public double Length()
        {
            FileInfo fileInfo = new FileInfo(filePath);

            return fileInfo.Length;
        }
        //返回由此抽象路径名表示的文件的长度。
        public void CreateNewFile()
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists == false)
            {
                fileInfo.Create();
            }


        }
        //   当且仅当不存在具有此抽象路径名指定的名称的文件时，原子地创建由此抽象路径名指定的一个新的空文件。
        public bool Delete()
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == false)
            {
                return false;
            }
            fileInfo.Delete();
            return true;
        }
        //   删除此抽象路径名表示的文件或目录。
        public void DeleteOnExit()
        {


        }
        //    在虚拟机终止时，请求删除此抽象路径名表示的文件或目录。
        public string[] List()
        {

            return Directory.GetFileSystemEntries(this.filePath);

            /*

            ArrayList list = new ArrayList();
            DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                list.Add(file.FullName);
            }

            foreach (DirectoryInfo subdirectory in directoryInfo.GetDirectories())
            {
                list.Add(subdirectory.FullName);
            }
             
            return (string[])list.ToArray();
            */
        }
        //返回由此抽象路径名所表示的目录中的文件和目录的名称所组成字符串数组。
        /* public [string] list(FilenameFilter  filter){



         }
         */
        //返回由包含在目录中的文件和目录的名称所组成的字符串数组，这一目录是通过满足指定过滤器的抽象路径名来表示的。
        public File[] ListFiles()
        {
            List<File> list = new List<File>();

            string normalizedPath = Path.GetFullPath(
             filePath.Replace('/', Path.DirectorySeparatorChar)
             .Replace('\\', Path.DirectorySeparatorChar));

            DirectoryInfo directoryInfo = new DirectoryInfo(normalizedPath);
            foreach (DirectoryInfo dirInfo in directoryInfo.GetDirectories())
            {
                list.Add(new File(dirInfo.FullName));
            }
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                list.Add(new File(fileInfo.FullName));
            }

            return list.ToArray();
        }
        //返回一个抽象路径名数组，这些路径名表示此抽象路径名所表示目录中的文件。
        /* public [File] listFiles(FileFilter  filter){


         }*/
        //返回表示此抽象路径名所表示目录中的文件和目录的抽象路径名数组，这些路径名满足特定过滤器。
        public bool Mkdir()
        {
            DirectoryInfo fileInfo = new DirectoryInfo(filePath);
            try
            {
                fileInfo.Create();
                return true;
            }
            catch
            {
                return false;
            }
        }
        //    创建此抽象路径名指定的目录。
        public bool Mkdirs()
        {
             
            try
            {
                DirectoryInfo fileInfo = new DirectoryInfo(filePath);
                string parentDirPath = fileInfo.FullName;
                if (Directory.Exists(parentDirPath) == false) // 如果父亲文件夹不存在则创建
                {
                    Directory.CreateDirectory(parentDirPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
  
        }
        //    创建此抽象路径名指定的目录，包括创建必需但不存在的父目录。
        public bool RenameTo(File t)
        {
            try
            {
                string newFilePath = t.GetPath();
                System.IO.File.Move(filePath, newFilePath);
                return true;
            }
            catch
            {
                return false;
            }




        }
        //  重新命名此抽象路径名表示的文件。
        public bool SetLastModified(DateTime time)
        {

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.LastWriteTime = time;
                return true;
            }
            catch
            {
                return false;
            }


        }
        //   设置由此抽象路径名所指定的文件或目录的最后一次修改时间。
        public bool SetReadOnly()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.IsReadOnly = true;
                return true;
            }
            catch
            {
                return false;
            }

        }
        //   标记此抽象路径名指定的文件或目录，以便只可对其进行读操作。
        public File CreateTempFile(string prefix, string uffix, File directory)
        {
            File file = new File(directory, prefix + uffix);
            try
            {
                file.CreateNewFile();




            }
            catch
            {

            }
            return file;

        }
        //    在指定目录中创建一个新的空文件，使用给定的前缀和后缀字符串生成其名称。
        public File CreateTempFile(string prefix,string uffix)
        {
            File file = new File(this, prefix + uffix);
            try
            {
                file.CreateNewFile();




            }
            catch
            {

            }
            return file;
        }
        //   在默认临时文件目录中创建一个空文件，使用给定前缀和后缀生成其名称。
        public int CompareTo(File pathname)
        {
            return this.GetPath().CompareTo(pathname.GetPath()) ;
        }
        //   按字母顺序比较两个抽象路径名。
        public int CompareTo(object o)
        {

            return this.GetPath().CompareTo(o);
        }
        //   按字母顺序比较抽象路径名与给定对象。
        public bool Equals(object obj)
        {
            
            return ((object)this).Equals(obj);
        }
        //测试此抽象路径名与给定对象是否相等。
        public string Tostring()
        {

            return ((object)this).ToString();
        }
        //返回此抽象路径名的路径名字符串。


    }
}
