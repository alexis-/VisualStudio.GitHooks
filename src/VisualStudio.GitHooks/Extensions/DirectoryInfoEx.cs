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
// Modified On:  2020/04/04 18:15
// Modified By:  Alexis

#endregion




namespace VisualStudio.GitHooks.Extensions
{
  using System;
  using System.IO;
  using System.Linq;
  using Utils;

  public static class DirectoryInfoEx
  {
    #region Methods

    public static void EnsureExists(this DirectoryInfo dir)
    {
      if (dir.Exists == false)
        dir.Create();
    }

    public static DirectoryInfo FindInParent(
      this DirectoryInfo        currentDir,
      Func<DirectoryInfo, bool> predicate)
    {
      predicate.ThrowIfArgumentNull(nameof(predicate));

      while (currentDir != null && predicate(currentDir) == false)
        currentDir = currentDir.Parent;

      return currentDir;
    }

    public static bool Contains(
      this DirectoryInfo rootDir,
      string             searchPattern)
    {
      return rootDir.GetDirectories(searchPattern, SearchOption.TopDirectoryOnly).Any()
        || rootDir.GetFiles(searchPattern, SearchOption.TopDirectoryOnly).Any();
    }
    
    public static void CopyFolderRecursive(
      this DirectoryInfo srcDir,
      DirectoryInfo destDir,
      Func<FileInfo, FileInfo, bool> predicate = null,
      Func<FileInfo, string> processFile = null)
    {
      destDir.EnsureExists();

      foreach (var srcFile in srcDir.GetFiles())
      {
        var destFilePath = Path.Combine(destDir.FullName, srcFile.Name);
        var destFile = new FileInfo(destFilePath);

        if (predicate != null && predicate(srcFile, destFile) == false)
          continue;

        if (processFile == null)
          srcFile.CopyTo(destFilePath, true);

        else
        {
          var content = processFile(srcFile);

          File.WriteAllText(destFilePath, content);
        }
      }

      foreach (var subSrcDir in srcDir.GetDirectories())
      {
        var subDestDir = new DirectoryInfo(Path.Combine(destDir.FullName, subSrcDir.Name));

        subSrcDir.CopyFolderRecursive(subDestDir, predicate, processFile);
      }
    }

    public static bool IsIdenticalTo(this FileInfo f1, FileInfo f2)
    {
      var f1Crc32 = f1.GetCrc32();
      var f2Crc32 = f2.GetCrc32();

      return f1Crc32 == f2Crc32;
    }

    public static string GetCrc32(this FileInfo fileInfo)
    {
      Crc32  crc32 = new Crc32();
      string hash  = string.Empty;

      using (FileStream fs = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();

      return hash;
    }

    #endregion
  }
}
