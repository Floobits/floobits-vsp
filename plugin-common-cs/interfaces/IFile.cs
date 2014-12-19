using System;
using System.IO;

namespace Floobits.Common.Interfaces
{
    public abstract class IFile
    {
        abstract public string getPath();
        abstract public bool rename(string name);
        abstract public bool move(IFile d);
        abstract public bool delete();
        abstract public IFile[] getChildren();
        abstract public string getName();
        abstract public long getLength();
        abstract public bool exists();
        abstract public bool isDirectory();
        abstract public bool isSpecial();
        abstract public bool isSymLink();
        abstract public bool isValid();
        abstract public byte[] getBytes();
        abstract public bool setBytes(byte[] bytes);
        abstract public void refresh();
        abstract public bool createDirectories(string dir);
        abstract public StreamReader getInputStream();
    }
}
