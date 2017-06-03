﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision.Windows
{
    public class WindowsStorage : Storage
    {
        string absolutePath;

        public WindowsStorage()
        {
            absolutePath = Path.Combine(Environment.CurrentDirectory, "Vision");
            DirectoryInfo di = new DirectoryInfo(absolutePath);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        protected override string GetAbsoluteRoot()
        {
            return absolutePath;
        }

        protected override void InternalCopy(Stream source, FileNode dist)
        {
            using(Stream output = dist.Open())
            {
                source.CopyTo(output);
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
            if (path.IsFile)
            {
                return File.Exists(path.AbosolutePath);
            }
            return Directory.Exists(path.AbosolutePath);
        }

        protected override DirectoryNode InternalCreateDirectory(DirectoryNode node)
        {
            Directory.CreateDirectory(node.AbosolutePath);
            return node;
        }

        protected override FileNode InternalCreateFile(FileNode node)
        {
            using(File.Create(node.AbosolutePath))
            {
            }
            return node;
        }

        protected override string InternalPathCombine(params string[] pathes)
        {
            return base.InternalPathCombine(pathes).TrimStart('/').Replace("/", "\\");
        }

        protected override char[] GetInvalidPathChars()
        {
            List<char> c = new List<char>();
            c.AddRange(Path.GetInvalidPathChars());
            c.Add(Path.VolumeSeparatorChar);
            return c.ToArray();
        }
    }
}
