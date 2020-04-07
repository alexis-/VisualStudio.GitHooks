#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Modified On:  2020/04/05 00:08
// Modified By:  Alexis

#endregion




namespace VisualStudio.GitHooks
{
  using System;
  using System.IO;
  using System.Threading;
  using Extensions;

  public static class GitHooks
  {
    #region Constants & Statics

    public const string GitHooksFolderName     = "hooks";
    public const string GitRootName            = ".git";
    public const string GitHooksConfigFileName = ".githooks";

    #endregion




    #region Methods

    public static DirectoryInfo FindGitRoot(this EnvDTE.DTE dte)
    {
      Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
      dte.ThrowIfArgumentNull(nameof(dte));

      var sol = dte.Solution;

      sol.ThrowIfNull("DTE.Solution is null");
      sol.FullName.ThrowIfNullOrWhitespace("DTE.Solution.FullName (path) is empty or null");

      var solDir = new DirectoryInfo(Path.GetDirectoryName(sol.FullName));

      solDir.ThrowIfMissing($"Solution '{sol.FullName}' doesn't exist.");

      return solDir.FindInParent(di => di.Contains(GitRootName));
    }

    public static bool ApplyGitHooks(
      this DirectoryInfo gitRoot,
      bool               throwIfMissing = false,
      CancellationToken  cs             = default)
    {
      // Validate paths
      if (gitRoot.Contains(GitRootName) == false)
        throw new ArgumentException($"The directory '{gitRoot.FullName}' is not the root of a git repository.");

      if (gitRoot.Contains(GitHooksConfigFileName) == false)
        return false;

      // Get .git/hooks directory
      var gitHookDir = new DirectoryInfo(Path.Combine(gitRoot.FullName, GitRootName, GitHooksFolderName));

      if (throwIfMissing)
        gitHookDir.ThrowIfMissing(".git/hooks directory is missing");

      gitHookDir.EnsureExists();

      // Copy user's hooks in .git/hooks
      var gitHooksLines = File.ReadAllLines(Path.Combine(gitRoot.FullName, GitHooksConfigFileName));

      foreach (var gitHooksLine in gitHooksLines)
        try
        {
          if (cs.IsCancellationRequested)
            return false;

          var userGitHookDir = new DirectoryInfo(Path.Combine(gitRoot.FullName, gitHooksLine));

          if (userGitHookDir.Exists == false)
            continue;

          userGitHookDir.CopyFolderRecursive(gitHookDir, (f1, f2) => f2.Exists == false || f1.IsIdenticalTo(f2) == false);
        }
        catch (IOException) { }

      return true;
    }

    #endregion
  }
}
