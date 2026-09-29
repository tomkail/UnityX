using System.IO;
using UnityEngine;

namespace UnityX.Versioning {
    public static class VersionControlX {

        // If this path contains a git repo.
        public static bool IsGitDirectory (string path) {
            return Directory.Exists (Path.Combine(path,".git"));
        }

        public static string gitDirectory {
            get {
                var currDir = Directory.GetCurrentDirectory();

                // Loop up through directories until we find the .git folder
                bool found = false;
                while(!found) {
                    found = IsGitDirectory(currDir);
                    if( !found ) {
                        // Go up a directory
                        currDir = Path.GetDirectoryName(currDir);

                        // Gone past C:\ to nothingness
                        if(string.IsNullOrEmpty(currDir)) {
                            return ReturnNullAndWarn("Tried to get git directory but no .git folder could be found in "+Directory.GetCurrentDirectory());
                        }
                    }
                }

                return Path.Combine(currDir, ".git");
            }
        }

        public static string GetGitBranch() {
            var gitDir = gitDirectory;
            if(gitDir == null) return ReturnNullAndWarn("git directory not found");

            // Find HEAD file that contains either:
            //  ref: refs/heads/2017.4
            // or a SHA itself
            var headFilePath = Path.Combine(gitDir, "HEAD");

            if( !File.Exists(headFilePath) ) {
                return ReturnNullAndWarn("Tried to get git branch but failed to find "+headFilePath);
            }

            // Get content of ref file - either a path to a file with a SHA, or the SHA itself
            var headFileContent = File.ReadAllText(headFilePath).Trim();

            // HEAD file contained a path to a ref file with a SHA?
            const string refColonHeader = "ref: ";
            if( headFileContent.StartsWith(refColonHeader) ) {
                // e.g. "ref: refs/heads/feature/my-branch" — the branch is everything after "refs/heads/"
                // (keep slashes; LastIndexOf('/') would drop "feature/").
                var refPath = headFileContent.Substring(refColonHeader.Length);
                const string headsPrefix = "refs/heads/";
                int headsPos = refPath.IndexOf(headsPrefix);
                if( headsPos >= 0 ) return refPath.Substring(headsPos + headsPrefix.Length);
                int pos = refPath.LastIndexOf("/") + 1;
                return refPath.Substring(pos);
            } else {
                return ReturnNullAndWarn("Tried to get git branch but headFileContent doesn't start with 'ref: '"+headFileContent);
            }
        }
        public static string GetGitSHA() {
            var gitDir = gitDirectory;
            if(gitDir == null) return ReturnNullAndWarn("git directory not found");

            // Find HEAD file that contains either:
            //  ref: refs/heads/2017.4
            // or a SHA itself
            var headFilePath = Path.Combine(gitDir, "HEAD");

            if( !File.Exists(headFilePath) )
                return ReturnNullAndWarn("Tried to get git SHA to put in Version object, but failed to find "+headFilePath);

            // Get content of ref file - either a path to a file with a SHA, or the SHA itself
            var headFileContent = File.ReadAllText(headFilePath).Trim();

            string gitSha;

            // HEAD file contained a path to a ref file with a SHA?
            const string refColonHeader = "ref: ";
            if( headFileContent.StartsWith(refColonHeader) ) {
                var refPath = headFileContent.Substring(refColonHeader.Length);
                refPath = Path.Combine(gitDir, refPath);

                if( !File.Exists(refPath) )
                    return ReturnNullAndWarn("Tried to get git SHA to put in Version object, but path of ref file could not be found: "+refPath);

                gitSha = File.ReadAllText(refPath).Trim();
            }

            // Not of the form "ref: path/to/ref/file", so assume it's the SHA itself.
            else {
                gitSha = headFileContent;
            }

            // Does it look like a git SHA? (git SHA-1 is 40 hex chars; the 42 upper bound here is a loose/arbitrary guard.)
            if( gitSha.Length < 6 || gitSha.Length > 42 || gitSha.Contains(" ") )
                return ReturnNullAndWarn("Tried to get git SHA to put in Version object, but got unexpected output: "+gitSha);

            return gitSha.Substring(0, 6);
        }

        static string ReturnNullAndWarn(string msg) {
            Debug.LogWarning("VersionControlX: "+msg);
            return null;
        }
    }
}
