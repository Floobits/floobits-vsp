using System.Collections.Generic;

namespace Floobits.Common.Interfaces
{
    public interface IFactory
    {
        public IFile findFileByIoFile(File file);
        public IFile createFile(string path);
        public IDoc getDocument(IFile file);
        public IDoc getDocument(string relPath);
        public IFile createDirectories(string path);
        public IFile findFileByPath(string path);
        public IFile getOrCreateFile(string path);
        public void removeHighlightsForUser(int userID);
        public void removeHighlight(int userId, string path);
        public bool openFile(File file);
        public void clearHighlights();
        public void clearReadOnlyState();
        public HashSet<string> readOnlyBufferIds = new HashSet<string>();
    }
}

