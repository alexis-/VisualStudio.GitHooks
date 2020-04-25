// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsOutputWindow.cs" company="">
//   
// </copyright>
// <summary>
//   The vs output window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
// Created On:   2020/04/11 15:22
// Modified On:  2020/04/11 15:26
// Modified By:  Alexis

#endregion




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
// Created On:   2020/04/11 15:22
// Modified On:  2020/04/11 15:22
// Modified By:  Alexis

#endregion




// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable InconsistentNaming
namespace VisualStudio.GitHooks.Utils
{
  using System;
  using System.Globalization;
  using System.Threading.Tasks;
  using EnvDTE;
  using EnvDTE80;
  using Microsoft.Build.Framework;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Shell.Interop;

  /// <summary>The vs output window.</summary>
  public static class VsOutputWindow
  {
    #region Constants & Statics

    // {F84CD1E7-0B9E-4928-8B87-D473B960F25B}
    /// <summary>The sma sdk pane guid.</summary>
    private static readonly Guid SMASdkPaneGuid = new Guid("92E55B59-ABF7-44DC-A3D7-5BF6AD6AAA3C");

    #endregion




    #region Methods

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public static bool WriteDebug(this IVsOutputWindowWriter writer,
                                  string                     format,
                                  params object[]            args)
    {
#if DEBUG
      return writer.WriteLine(LoggerVerbosity.Minimal, format, args);
#else
      return true;
#endif
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public static bool WriteError(this IVsOutputWindowWriter writer,
                                  string                     format,
                                  params object[]            args)
    {
      return writer.WriteLine(LoggerVerbosity.Minimal, format, args);
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public static bool WriteDetailed(this IVsOutputWindowWriter writer,
                                     string                     format,
                                     params object[]            args)
    {
      return writer.WriteLine(LoggerVerbosity.Detailed, format, args);
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="verbosity">
    /// The verbosity level.
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public static bool WriteLine(this IVsOutputWindowWriter writer,
                                 LoggerVerbosity            verbosity,
                                 string                     format,
                                 params object[]            args)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (writer.EnsureOutputWindow() == false)
        return false;

      if ((int)writer.CurrentBuildVerbosity < (int)verbosity)
        return false;
      
      var msg = string.Format(CultureInfo.InvariantCulture, format + Environment.NewLine, args);

      writer.OutputWindowPane2.OutputString(msg);
      return writer.OutputWindowPane.OutputString(msg) == VSConstants.S_OK;
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    public static Task<bool> WriteDebugAsync(this IVsOutputWindowWriter writer,
                                             string                     format,
                                             params object[]            args)
    {
#if DEBUG
      return writer.WriteLineAsync(LoggerVerbosity.Minimal, format, args);
#else
      return true;
#endif
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    public static Task<bool> WriteDetailedAsync(this IVsOutputWindowWriter writer,
                                                string                     format,
                                                params object[]            args)
    {
      return writer.WriteLineAsync(LoggerVerbosity.Detailed, format, args);
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    public static Task<bool> WriteErrorAsync(this IVsOutputWindowWriter writer,
                                             string                     format,
                                             params object[]            args)
    {
      return writer.WriteLineAsync(LoggerVerbosity.Minimal, format, args);
    }

    /// <summary>
    /// Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer">
    /// </param>
    /// <param name="verbosity">
    /// The verbosity level.
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <param name="args">
    /// An array of objects to write using format.
    /// </param>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    public static async Task<bool> WriteLineAsync(this IVsOutputWindowWriter writer,
                                                  LoggerVerbosity            verbosity,
                                                  string                     format,
                                                  params object[]            args)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      if (await writer.EnsureOutputWindowAsync().ConfigureAwait(false) == false)
        return false;

      if ((int)writer.CurrentBuildVerbosity < (int)verbosity)
        return false;

      var msg = string.Format(CultureInfo.InvariantCulture, format + Environment.NewLine, args);

      writer.OutputWindowPane2.OutputString(msg);
      return writer.OutputWindowPane.OutputString(msg) == VSConstants.S_OK;
    }

    /// <summary>
    /// Refreshes the value of the VisualStudio MSBuildOutputVerbosity setting.
    /// </summary>
    /// <param name="writer">
    /// The writer.
    /// </param>
    /// <remarks>
    /// 0 is Quiet, while 4 is diagnostic.
    /// </remarks>
    private static void RefreshMSBuildOutputVerbositySetting(this IVsOutputWindowWriter writer)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      Properties properties = writer.Dte2.Properties["Environment", "ProjectsAndSolution"];

      writer.CurrentBuildVerbosity = (LoggerVerbosity)properties.Item("MSBuildOutputVerbosity").Value;
    }

    /// <summary>
    /// The ensure output window async.
    /// </summary>
    /// <param name="writer">
    /// The writer.
    /// </param>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    private static async Task<bool> EnsureOutputWindowAsync(this IVsOutputWindowWriter writer)
    {
      if (writer.OutputWindowPane != null)
        return true;

      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var outputWindow = writer.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;

      return writer.EnsureOutputWindow(outputWindow);
    }

    /// <summary>
    /// The ensure output window.
    /// </summary>
    /// <param name="writer">
    /// The writer.
    /// </param>
    /// <param name="outputWindow">
    /// The output window.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    private static bool EnsureOutputWindow(this IVsOutputWindowWriter writer, IVsOutputWindow outputWindow = null)
    {
      if (writer.OutputWindowPane != null)
        return true;

      ThreadHelper.ThrowIfNotOnUIThread();

      var outputWindowPanes = writer.Dte2.ToolWindows.OutputWindow.OutputWindowPanes;
      outputWindow ??= writer.GetServiceSync(typeof(SVsOutputWindow)) as IVsOutputWindow;

      if (outputWindow == null)
        return false;

      Guid buildPaneGuid = VSConstants.GUID_BuildOutputWindowPane;
      int  hResult       = outputWindow.GetPane(ref buildPaneGuid, out var buildOutputWindowPane);

      writer.OutputWindowPane = buildOutputWindowPane;

      try
      {
        writer.OutputWindowPane2 = outputWindowPanes.Item("GitHooks");
      }
      catch (Exception)
      {
        writer.OutputWindowPane2 = outputWindowPanes.Add("GitHooks");
      }

      writer.RefreshMSBuildOutputVerbositySetting();

      return hResult == VSConstants.S_OK && buildOutputWindowPane != null;
    }

    #endregion
  }

  /// <summary>The VSOutputWindowWriter interface.</summary>
  public interface IVsOutputWindowWriter
  {
    /// <summary>Gets the dte 2.</summary>
    DTE2 Dte2 { get; }

    /// <summary>Gets or sets the output window pane.</summary>
    IVsOutputWindowPane OutputWindowPane { get; set; }

    /// <summary>Gets or sets the output window pane 2.</summary>
    OutputWindowPane OutputWindowPane2 { get; set; }

    /// <summary>Gets or sets the current build verbosity.</summary>
    LoggerVerbosity CurrentBuildVerbosity { get; set; }

    /// <summary>
    /// The get service async.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    Task<object> GetServiceAsync(Type serviceType);

    /// <summary>
    /// The get service sync.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <returns>
    /// The <see cref="object"/>.
    /// </returns>
    object GetServiceSync(Type serviceType);
  }
}
