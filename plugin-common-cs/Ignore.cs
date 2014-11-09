using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Floobits.Common.Interfaces;
using Floobits.Utilities;
/// NGIT?


namespace Floobits.Common
{
    public class Ignore
    {
        static HashSet<string> IGNORE_FILES = new HashSet<string> {".gitignore", ".hgignore", ".flignore", ".flooignore"};
        static List<string> DEFAULT_IGNORES = new List<string> {"extern", "node_modules", "tmp", "vendor", ".idea/workspace.xml", ".idea/misc.xml"};
        static int MAX_FILE_SIZE = 1024 * 1024 * 5;
        public IFile file;
        public string stringPath;
        public Dictionary<string, Ignore> children = new Dictionary<string, Ignore>();
        public List<IFile> files = new List<IFile>();
        public int size = 0;
#if LATER
        private IgnoreNode ignoreNode = new IgnoreNode();
#endif

        public static Ignore BuildIgnore(IFile virtualFile) {
            Ignore ig = new Ignore(virtualFile);
#if LATER
            // TODO: add more hard-coded ignores
            ig.ignoreNode.addRule(new IgnoreRule(".idea/workspace.xml"));
            ig.ignoreNode.addRule(new IgnoreRule(".idea/misc.xml"));
            ig.ignoreNode.addRule(new IgnoreRule(".git"));
            ig.ignoreNode.addRule(new IgnoreRule(".svn"));
            ig.ignoreNode.addRule(new IgnoreRule(".hg"));
            ig.recurse(ig, virtualFile.getPath());
#endif
            return ig;
        }

        public static void writeDefaultIgnores(IContext context) {
            Flog.log("Creating default ignores.");
            string path = FilenameUtils.concat(context.colabDir, ".flooignore");

            try {
                if (File.Exists(path)) {
                    return;
                }
                File.WriteAllLines(path, DEFAULT_IGNORES);
            } catch (IOException e) {
                Flog.warn(e.ToString());
            }
        }

        public static bool isIgnoreFile(IFile virtualFile) {
            return virtualFile != null && IGNORE_FILES.Contains(virtualFile.getName()) && virtualFile.isValid();
        }

        public bool isIgnored(IFile f, string absPath, string relPath, bool isDir) {
            if (isFlooIgnored(f, absPath)) {
                Flog.log("Ignoring %s just because.", absPath);
                return true;
            }
            relPath = FilenameUtils.separatorsToUnix(relPath);
            return !relPath.Equals(stringPath) && isGitIgnored(relPath, isDir);
        }

        private Ignore(IFile virtualFile) {
            file = virtualFile;
            stringPath = FilenameUtils.separatorsToUnix(virtualFile.getPath());
            Flog.debug("Initializing ignores for %s", file);
            foreach (IFile vf in file.getChildren()) {
                addRules(vf);
            }
        }

        public void addRules(IFile virtualFile) {
            if (!isIgnoreFile(virtualFile)) {
                return;
            }
#if LATER
            InputStream inputStream = virtualFile.getInputStream();
            if (inputStream != null) {
                try {
                    ignoreNode.parse(inputStream);
                } catch (IOException e) {
                    Flog.warn(e);
                }
#endif
        }

        public void recurse(Ignore root, string rootPath) {
#if LATER
            @SuppressWarnings("UnsafeVfsRecursion") IFile[] fileChildren = file.getChildren();
            for (IFile file : fileChildren) {
                String absPath = file.getPath();
                if (isFlooIgnored(file, absPath))  {
                    continue;
                }
                String relPath = FilenameUtils.separatorsToUnix(Utils.toProjectRelPath(absPath, rootPath));

                Boolean isDir = file.isDirectory();
                if (root.isGitIgnored(relPath, isDir)) {
                    continue;
                }

                if (isDir) {
                    Ignore child = new Ignore(file);
                    children.put(file.getName(), child);
                    child.recurse(root, rootPath);
                    continue;
                }

                files.add(file);
                size += file.getLength();
            }
#endif
        }

        /**
        * @param path
        *            the rel path to test. The path must be relative to this ignore
        *            node's own repository path, and in repository path format
        *            (uses '/' and not '\').
        **/
        private bool isGitIgnored(string path, bool isDir) {
#if LATER
            IgnoreNode.MatchResult ignored = ignoreNode.isIgnored(path, isDir);
            switch (ignored) {
                case IGNORED:
                    Flog.log("Ignoring %s because it is ignored by git.", path);
                    return true;
                case NOT_IGNORED:
                    return false;
                case CHECK_PARENT:
                    break;
            }
            String[] split = path.split("/", 2);
            if (split.length != 2) {
                return false;
            }
            String nextName = split[0];
            path = split[1];
            Ignore ignore = children.get(nextName);
            return ignore != null && ignore.isGitIgnored(path, isDir);
#endif
            return false;
        }

        public bool isFlooIgnored(IFile virtualFile, string absPath) {
            if (!file.isValid()) {
                return true;
            }
            if (virtualFile.isSpecial() || virtualFile.isSymLink()) {
                Flog.log("Ignoring %s because it is special or a symlink.", absPath);
                return true;
            }
            if (!virtualFile.isDirectory() && virtualFile.getLength() > MAX_FILE_SIZE) {
                Flog.log("Ignoring %s because it is too big (%s)", absPath, virtualFile.getLength());
                return true;
            }
            return false;
        }
#if LATER
        @Override
        public int compareTo(@NotNull Ignore ignore) {
            return ignore.size - size;
        }
#endif
    }
}
