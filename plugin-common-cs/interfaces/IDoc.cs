using System.Collections.Generic;
using Floobits.Common.Dmp;


namespace Floobits.Common.Interfaces
{
    public abstract class IDoc
    {
        abstract public void removeHighlight(int userId, string path);
        abstract public void applyHighlight(string path, int userID, string username, bool stalking, bool force, List<List<int>> ranges);
        abstract public void save();
        abstract public string getText();
        abstract public void setText(string text);
        abstract public void setReadOnly(bool readOnly);
        abstract public bool makeWritable();
        abstract public IFile getVirtualFile();
        abstract public string patch(FlooPatchPosition[] positions);
    }
}

