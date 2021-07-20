using System.IO;
using UnityEngine;

public static class VersionControlX {
    public static string gitDirectory {
        get {
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
                        Debug.LogError("no .git folder could be found.");
                        return null;
                    }
                }
            }

            return Path.Combine(currDir, ".git");
        }
    }

    public static string GetGitBranch() {
        var gitDir = gitDirectory;

        // Find HEAD file that contains either:
        //  ref: refs/heads/2017.4
        // or a SHA itself
        var headFilePath = Path.Combine(gitDir, "HEAD");

        if( !File.Exists(headFilePath) ) {
            Debug.LogError("Tried to get git branch but failed to find "+headFilePath);
            return "???";
        }
        
        // Get content of ref file - either a path to a file with a SHA, or the SHA itself
        var headFileContent = File.ReadAllText(headFilePath).Trim();
        
        // HEAD file contained a path to a ref file with a SHA?
        const string refColonHeader = "ref: ";
        if( headFileContent.StartsWith(refColonHeader) ) {
            int pos = headFileContent.LastIndexOf("/") + 1;
            return headFileContent.Substring(pos, headFileContent.Length - pos);
        } else {
            return "???";
        }
    }
    public static string GetGitSHA() {
        string Error(string msg) {
            Debug.LogError("Tried to get git SHA to put in Version object, but "+msg);
            return null;
        }

        var gitDir = gitDirectory;

        // Find HEAD file that contains either:
        //  ref: refs/heads/2017.4
        // or a SHA itself
        var headFilePath = Path.Combine(gitDir, "HEAD");

        if( !File.Exists(headFilePath) )
            return Error("failed to find "+headFilePath);

        // Get content of ref file - either a path to a file with a SHA, or the SHA itself
        var headFileContent = File.ReadAllText(headFilePath).Trim();
        
        string gitSha;

        // HEAD file contained a path to a ref file with a SHA?
        const string refColonHeader = "ref: ";
        if( headFileContent.StartsWith(refColonHeader) ) {
            var refPath = headFileContent.Substring(refColonHeader.Length);
            refPath = Path.Combine(gitDir, refPath);
            
            if( !File.Exists(refPath) )
                return Error("path of ref file could not be found: "+refPath);
            
            gitSha = File.ReadAllText(refPath).Trim();
        }
        
        // Not of the form "ref: path/to/ref/file", so assume it's the SHA itself.
        else {
            gitSha = headFileContent;
        }

        // Does it look like a git SHA?
        if( gitSha.Length < 6 || gitSha.Length > 42 || gitSha.Contains(" ") )
            return Error("got unexpected output: "+gitSha);

        return gitSha.Substring(0, 6);
    }
        
}