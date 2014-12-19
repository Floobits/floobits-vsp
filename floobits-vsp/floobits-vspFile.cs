using System;
using System.IO;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    public class VSPFile : IFile
    {
        string file_path;

        public VSPFile(string path)
        {
            file_path = path;
        }

        public override string getPath()
        {
            return Path.GetFullPath(file_path);
        }

        public override bool rename(string name)
        {
            try
            {
                File.Move(file_path, name);
                file_path = name;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool move(IFile d)
        {
            return rename(d.getPath());
        }

        public override bool delete()
        {
            try
            {
                File.Delete(file_path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override IFile[] getChildren()
        {
            return null;
        }

        public override string getName()
        {
            return Path.GetFileName(file_path);
        }

        public override long getLength()
        {
            FileInfo fi = new FileInfo(file_path);
            return fi.Length;
        }

        public override bool exists()
        {
            return File.Exists(getPath());
        }

        public override bool isDirectory()
        {
            return File.GetAttributes(getPath()).HasFlag(FileAttributes.Directory);
        }

        public override bool isSpecial()
        {
            return false;
        }

        public override bool isSymLink()
        {
            return false;
        }

        public override bool isValid()
        {
            return exists();
        }

        public override byte[] getBytes()
        {
            return File.ReadAllBytes(getPath());
        }

        public override bool setBytes(byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(getPath(), bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void refresh()
        {

        }

        public override bool createDirectories(string dir)
        {
            try
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override StreamReader getInputStream()
        {
            return null;
        }

    }
}
