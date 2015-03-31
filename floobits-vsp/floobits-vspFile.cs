using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Floobits.Common;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    public class ByFileName : IComparer<string>
    {
        CaseInsensitiveComparer caseiComp = new CaseInsensitiveComparer();
        public int Compare(string x, string y)
        {
            return caseiComp.Compare(x, y);
        }
    }

    public class VSPFile : IFile
    {
        string file_path;
        VSPContext context;

        public VSPFile(string path, VSPContext context)
        {
            file_path = path;
            this.context = context;
        }

        public override bool Equals(object other)
        {
            return this.getPath() == (other as IFile).getPath();
        }

        public override int GetHashCode()
        {
            return getPath().GetHashCode();
        }

        public override string getPath()
        {
            return FilenameUtils.normalize(file_path);
        }

        public override bool rename(string name)
        {
            try
            {
                string old = getPath();
                File.Move(file_path, name);
                context.GetVSPFactory().retrackFile(old, this);
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
                context.GetVSPFactory().untrackFile(getPath());
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
