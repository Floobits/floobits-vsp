using System;
using System.IO;

namespace Floobits.Common.Interfaces
{
    public interface IFile
    {
        public string getPath();
        public bool rename(Object obj, string name);
        public IFile makeFile(string name);
        public bool move(Object obj, IFile d);
        public bool delete(Object obj);
        public IFile[] getChildren();
        public string getName();
        public long getLength();
        public bool exists();
        public bool isDirectory();
        public bool isSpecial();
        public bool isSymLink();
        public bool isValid();
        public byte[] getBytes();
        public bool setBytes(byte[] bytes);
        public void refresh();
        public bool createDirectories(string dir);
        public StreamReader getInputStream();
    }
}
