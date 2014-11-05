using System.Collections.Generic;

namespace Floobits.Common.Interfaces
{
    public abstract class IFactory
    {
        abstract public IFile findFileByIoFile(string filename);
        abstract public IFile createFile(string path);
        abstract public IDoc getDocument(IFile file);
        abstract public IDoc getDocument(string relPath);
        abstract public IFile createDirectories(string path);
        abstract public IFile findFileByPath(string path);
        abstract public IFile getOrCreateFile(string path);
        abstract public void removeHighlightsForUser(int userID);
        abstract public void removeHighlight(int userId, string path);
        abstract public bool openFile(string filename);
        abstract public void clearHighlights();
        abstract public void clearReadOnlyState();
        public HashSet<string> readOnlyBufferIds = new HashSet<string>();
    }
}

