using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Vision
{
    public abstract class Storage
    {
        private static Storage Current;

        public static DirectoryNode Root => new DirectoryNode("/");
        public static string AbsoluteRoot => Current.GetAbsoluteRoot();

        public static void Init(Storage storage)
        {
            if(Current == null)
            {
                Logger.Log("Use Native Storage: " + storage.ToString());

                Current = storage;
            }
            else
            {
                Logger.Log("Already Inited");
            }
        }

        public static void Copy(Stream source, FileNode dist)
        {
            Current.InternalCopy(source, dist);
        }
        public static void Copy(Stream source, string dist)
        {
            Current.InternalCopy(source, new FileNode(dist));
        }
        protected abstract void InternalCopy(Stream source, FileNode dist);

        public static void Delete(string path)
        {
            Current.InternalDelete(new FileNode(path));
        }
        public static void Delete(FileNode path)
        {
            Current.InternalDelete(path);
        }
        protected abstract void InternalDelete(FileNode path);
        
        protected abstract string GetAbsoluteRoot();

        public static string GetAbsolutePath(string path)
        {
            return PathCombine(AbsoluteRoot, path);
        }
        
        public static Stream GetFileStream(FileNode node)
        {
            return Current.InternalGetFileStream(node);
        }
        protected abstract Stream InternalGetFileStream(FileNode node);

        public static FileNode NewFile(string path)
        {
            return Current.InternalNewFile(path);
        }
        protected abstract FileNode InternalNewFile(string path);

        public static DirectoryNode NewDirectory(string path)
        {
            return Current.InternalNewDirectory(path);
        }
        protected abstract DirectoryNode InternalNewDirectory(string path);

        public static string PathCombine(params string[] pathes)
        {
            return Current.InternalPathCombine(pathes);
        }
        protected virtual string InternalPathCombine(params string[] pathes)
        {
            string ret = "";
            foreach (string s in pathes)
            {
                ret += "/" + s.Trim('\\', '/');
            }
            return ret;
        }

        public static bool IsExist(StorageNode path)
        {
            return Current.InternalIsExist(path);
        }
        protected abstract bool InternalIsExist(StorageNode path);

        public static FileNode LoadResource(ManifestResource resource, bool overwrite = false)
        {
            Logger.Log("Load Resource: " + resource);
            var assembly = typeof(Core).GetTypeInfo().Assembly;

            FileNode node = Root.GetFile(resource.FileName);
            if (!overwrite && node.IsExist)
            {
                Logger.Log("resource finded! " + node.AbosolutePath);
            }
            else
            {
                if (overwrite)
                {
                    Logger.Log("resource is overwriting. copy to: " + node.AbosolutePath);
                    if (node.IsExist)
                    {
                        node.Delete();
                    }
                }
                else
                {
                    Logger.Log("resource not found. copy to : " + node.AbosolutePath);
                }

                using (Stream stream = assembly.GetManifestResourceStream(resource.Resource))
                {
                    Copy(stream, node);
                }
            }

            return node;
        }
    }

    public abstract class StorageNode
    {
        public string Path { get; set; }
        public string AbosolutePath
        {
            get
            {
                return Storage.GetAbsolutePath(Path);
            }
        }

        public abstract bool IsFile { get; }
        public abstract bool IsDirectory { get; }
        public abstract bool IsExist { get; }

        public override string ToString()
        {
            if (IsFile)
                return string.Format("File, {0}, AbsolutePath: {1}", Path, AbosolutePath);
            else
                return string.Format("Directory, {0}, AbsolutePath: {1}", Path, AbosolutePath);
        }
    }

    public class FileNode : StorageNode
    {
        public override bool IsFile => true;
        public override bool IsDirectory => false;
        public override bool IsExist => Storage.IsExist(this);

        public FileNode()
        {

        }

        public FileNode(string path)
        {
            Path = path;
        }

        public Stream Open()
        {
            return Storage.GetFileStream(this);
        }

        public void Delete()
        {
            Storage.Delete(this);
        }
    }

    public class DirectoryNode : StorageNode
    {
        public override bool IsFile => false;
        public override bool IsDirectory => true;
        public override bool IsExist => Storage.IsExist(this);

        public DirectoryNode()
        {

        }

        public DirectoryNode(string path)
        {
            Path = path;
        }

        public FileNode GetFile(string name)
        {
            return new FileNode(Storage.PathCombine(Path, name));
        }

        public DirectoryNode GetDirectory(string name)
        {
            return new DirectoryNode(Storage.PathCombine(Path, name));
        }

        public FileNode NewFile(string filename)
        {
            string newpath = Storage.PathCombine(Path, filename);

            if (new FileNode(newpath).IsExist)
            {
                return null;
            }

            return Storage.NewFile(newpath);
        }

        public DirectoryNode NewDirectory(string directoryName)
        {
            string newpath = Storage.PathCombine(Path, directoryName);

            if (new FileNode(newpath).IsExist)
            {
                return null;
            }

            return Storage.NewDirectory(newpath);
        }
    }
}
