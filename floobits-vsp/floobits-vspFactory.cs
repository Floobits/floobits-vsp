using System;
using System.IO;
using Floobits.Common.Interfaces;
using Floobits.Utilities;

namespace Floobits.floobits_vsp
{
    public class VSPFactory : IFactory
    {
        VSPContext context;

        public VSPFactory(VSPContext context)
        {
            this.context = context;
        }

        public override IFile createFile(string path)
        {
            string abspath = context.absPath(path);
            string name = Path.GetFileName(abspath);
            string parentPath = Path.GetDirectoryName(abspath);
            try
            {
                Directory.CreateDirectory(parentPath);
            }
            catch (Exception e)
            {
                Flog.warn("Create directories error {0}", e);
                context.errorMessage("The Floobits plugin was unable to create directories for file.");
                return null;
            }

            try
            {
                File.Create(abspath);
            }
            catch (Exception e)
            {
                Flog.warn("Create file error {0}", e);
                context.errorMessage(string.Format("The Floobits plugin was unable to create file: {0}.", path));
                return null;
            }

            return new VSPFile(path);
        }

        public override IDoc getDocument(IFile file)
        {
            return null;
        }

        public override IDoc getDocument(string relPath)
        {
            return null;
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
            return new VSPFile(path);
        }

        public override IFile findFileByPath(string path)
        {
            string abspath = Path.GetFullPath(path);
            VSPFile file = null;
            if (File.Exists(abspath) || Directory.Exists(abspath))
            {
                file = new VSPFile(path);
            }
            return file;
        }

        public override IFile getOrCreateFile(string path)
        {
            return null;
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

