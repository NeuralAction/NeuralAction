using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using OS = Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Vision.Android
{
    public class AndroidStorage : Storage
    {
        string absolutepath;
        public AndroidStorage()
        {
            //absolutepath = InternalPathCombine((((AndroidCv)Core.Cv).AppContext).FilesDir.AbsolutePath, "Vision");
            absolutepath = "/sdcard/Vision";
            DirectoryInfo di = new DirectoryInfo(absolutepath);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        protected override string GetAbsoluteRoot()
        {
            return absolutepath;
        }

        protected override void InternalCopy(Stream source, FileNode dist)
        {
            FileInfo fi = new FileInfo(dist.AbosolutePath);
            using (FileStream target = fi.Open(FileMode.OpenOrCreate))
            {
                source.CopyTo(target);
            }
        }

        protected override void InternalDelete(FileNode path)
        {
            File.Delete(path.AbosolutePath);
        }

        protected override Stream InternalGetFileStream(FileNode node)
        {
            return File.Open(node.AbosolutePath, FileMode.Open);
        }

        protected override bool InternalIsExist(StorageNode path)
        {
            if (path.IsDirectory)
            {
                return Directory.Exists(path.AbosolutePath);
            }
            return File.Exists(path.AbosolutePath);
        }

        protected override DirectoryNode InternalCreateDirectory(DirectoryNode path)
        {
            Directory.CreateDirectory(path.AbosolutePath);
            return path;
        }

        protected override FileNode InternalCreateFile(FileNode path)
        {
            using (File.Create(path.AbosolutePath))
            {

            }
            return path;
        }

        protected override char[] GetInvalidPathChars()
        {
            return Path.GetInvalidFileNameChars();
        }
    }
}