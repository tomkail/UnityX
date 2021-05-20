using System.IO;
using UnityEngine;

public static class VersionControlX {
    public static string GetGitSHA() {
        var currDir = Directory.GetCurrentDirectory();

        // Loop up through directories until we find the .git folder
        bool found = false;
        while(!found) {
            found = Directory.Exists(Path.Combine(currDir, ".git"));
            if( !found ) {
                // Go up a directory
                currDir = Path.GetDirectoryName(currDir);

                // Gone past C:\ to nothingness
                if( currDir == "" || currDir == null ) {
                    return GitError("no .git folder could be found.");
                }
            }
        }

        // Enter .git directory
        var gitDir = Path.Combine(currDir, ".git");

        // Find HEAD file that contains either:
        //  ref: refs/heads/2017.4
        // or a SHA itself
        var headFilePath = Path.Combine(gitDir, "HEAD");

        if( !File.Exists(headFilePath) )
            return GitError("failed to find "+headFilePath);

        // Get content of ref file - either a path to a file with a SHA, or the SHA itself
        var headFileContent = File.ReadAllText(headFilePath).Trim();
        
        string gitSha;

        // HEAD file contained a path to a ref file with a SHA?
        const string refColonHeader = "ref: ";
        if( headFileContent.StartsWith(refColonHeader) ) {
            var refPath = headFileContent.Substring(refColonHeader.Length);
            refPath = Path.Combine(gitDir, refPath);
            
            if( !File.Exists(refPath) )
                return GitError("path of ref file could not be found: "+refPath);
            
            gitSha = File.ReadAllText(refPath).Trim();
        }
        
        // Not of the form "ref: path/to/ref/file", so assume it's the SHA itself.
        else {
            gitSha = headFileContent;
        }

        // Does it look like a git SHA?
        if( gitSha.Length < 6 || gitSha.Length > 42 || gitSha.Contains(" ") )
            return GitError("got unexpected output: "+gitSha);

        return gitSha.Substring(0, 6);
    }
        
    static string GitError(string msg) {
        Debug.LogError("Tried to get git SHA to put in Version object, but "+msg);
        return null;
    }
}