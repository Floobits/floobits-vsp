using System;
using System.IO;
using System.Collections.Generic;
using Floobits.Common;
using Floobits.Common.Dmp;
using Floobits.Common.Interfaces;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Floobits.floobits_vsp
{
    public class VSPDoc : IDoc
    {
        IWpfTextView textView;
        ITextBuffer textBuffer;
        ITextDocument document;
        VSPContext context;

        public VSPDoc(IWpfTextView textView, VSPContext context)
        {
            this.textView = textView;
            this.textBuffer = textView.TextBuffer;
            this.document = textBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
            this.context = context;
        }

        public string getPath()
        {
            return document.FilePath;
        }

        public IWpfTextView getTextView()
        {
            return textView;
        }

        override public void removeHighlight(int userId, string path)
        {

        }

        override public void applyHighlight(string path, int userID, string username, bool stalking, bool force, List<List<int>> ranges)
        {

        }

        override public void save()
        {
            document.Save();
        }

        override public string getText()
        {
            return textBuffer.CurrentSnapshot.GetText();
        }

        override public void setText(string text)
        {
            context.mainThread(delegate
            {
                textBuffer.Replace(new Span(0, textBuffer.CurrentSnapshot.Length), text);
            });
        }

        override public void setReadOnly(bool readOnly)
        {
         
        }

        override public bool makeWritable()
        {
            return false;
        }

        override public IFile getVirtualFile()
        {
            return context.GetVSPFactory().findFileByPath(getPath());
        }

        override public string patch(FlooPatchPosition[] positions)
        {
            return "";
        }
    }
}

