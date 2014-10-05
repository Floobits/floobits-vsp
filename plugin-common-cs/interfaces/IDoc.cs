using System.Collections.Generic;
using Floobits.Common.Dmp;


namespace Floobits.Common.Interfaces
{
    public interface IDoc
    {
        public void removeHighlight(int userId, string path);
        public void applyHighlight(string path, int userID, string username, bool stalking, bool force, List<List<int>> ranges);
        public void save();
        public string getText();
        public void setText(string text);
        public void setReadOnly(bool readOnly);
        public bool makeWritable();
        public IFile getVirtualFile();
        public string patch(FlooPatchPosition[] positions);
    }
}

