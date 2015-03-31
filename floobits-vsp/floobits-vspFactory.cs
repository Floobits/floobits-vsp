using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Floobits.Common;
using Floobits.Common.Interfaces;
using Floobits.Utilities;

namespace Floobits.floobits_vsp
{
    public class VSPFactory : IFactory
    {
        VSPContext context;
        Dictionary<string, VSPDoc> docs;
        Dictionary<string, VSPFile> files;

        public VSPFactory(VSPContext context)
        {
            this.context = context;
            docs = new Dictionary<string, VSPDoc>();
            files = new Dictionary<string, VSPFile>();
        }

        public override IFile createFile(string path)
        {
            string abspath = context.absPath(path);
            string name = Path.GetFileName(abspath);
            string parentPath = Path.GetDirectoryName(abspath);
            VSPFile file = null;
            try
            {
                Directory.CreateDirectory(parentPath);
            }
            catch (Exception e)
            {
                Flog.warn("Create directories error {0}", e);
                context.errorMessage("The Floobits plugin was unable to create directories for file.");
            }

            try
            {
                File.Create(abspath);
                file = trackFile(path);
            }
            catch (Exception e)
            {
                Flog.warn("Create file error {0}", e);
                context.errorMessage(string.Format("The Floobits plugin was unable to create file: {0}.", path));
            }

            return file;
        }

        public VSPDoc trackDocument(IWpfTextView tv)
        {
            VSPDoc doc = new VSPDoc(tv, context);
            docs.Add(FilenameUtils.normalize(doc.getPath()), doc);
            return doc;
        }

        public void untrackDocument(IWpfTextView tv)
        {
            foreach (KeyValuePair<string, VSPDoc> pair in docs)
            {
                if (pair.Value == tv)
                {
                    untrackDocument(pair.Key);
                    break;
                }
            }
        }

        public void untrackDocument(string path)
        {
            docs.Remove(FilenameUtils.normalize(path));
        }

        public VSPFile trackFile(string path)
        {
            VSPFile file = new VSPFile(path, context);
            files.Add(FilenameUtils.normalize(path), file);
            return file;
        }

        public void untrackFile(string path)
        {
            files.Remove(FilenameUtils.normalize(path));
        }

        public void retrackFile(string oldpath, VSPFile file)
        {
            files.Remove(FilenameUtils.normalize(oldpath));
            files.Remove(FilenameUtils.normalize(file.getPath()));
            files.Add(FilenameUtils.normalize(file.getPath()), file);
        }

        public override IDoc getDocument(IFile file)
        {
            return getDocument(file.getPath());
        }

        public override IDoc getDocument(string relPath)
        {
            VSPDoc d = null;
            docs.TryGetValue(FilenameUtils.normalize(relPath), out d);
            return d;
        }

        public override IFile createDirectories(string path)
        {
            string abspath = context.absPath(path);
            try
            {
                Directory.CreateDirectory(abspath);
            }
            catch (Exception e)
            {
                Flog.warn("Create directories error {0}", e);
                context.errorMessage(string.Format("The Floobits plugin was unable to create directories: {0}.", path));
                return null;
            }
            return trackFile(path);
        }

        public override IFile findFileByPath(string path)
        {
            string abspath = FilenameUtils.normalize(path);
            VSPFile file = null;
            if (!files.TryGetValue(abspath, out file))
            {
                if (File.Exists(abspath) || Directory.Exists(abspath))
                {
                    file = trackFile(path);
                }
            }
            return file;
        }

        public override IFile getOrCreateFile(string path)
        {
            IFile fileByPath = findFileByPath(path);
            if (fileByPath != null)
            {
                return fileByPath;
            }
            return createFile(path);
        }

        public override void removeHighlightsForUser(int userID)
        {

        }

        public override void removeHighlight(int userId, string path)
        {

        }

        public override bool openFile(string filename)
        {
            return false;
        }

        public override void clearHighlights()
        {

        }

        public override void clearReadOnlyState()
        {

        }
    }
}

