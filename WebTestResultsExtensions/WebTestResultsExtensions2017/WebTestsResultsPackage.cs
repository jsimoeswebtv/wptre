using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Microsoft.Win32;
using WebTestResultsExtensions;
using Task = System.Threading.Tasks.Task;

namespace WebTestResultsExtensions2017
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(WebTestsResultsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class WebTestsResultsPackage : AsyncPackage
    {
        #region Public Fields

        /// <summary>
        /// WebTestsResultsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "89c3a0e4-0919-4e34-b209-31cc8185874b";

        #endregion Public Fields

        #region Private Fields

        private readonly Dictionary<Guid, List<UserControl>> m_controls = new Dictionary<Guid, List<UserControl>>();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTestsResultsPackage"/> class.
        /// </summary>
        public WebTestsResultsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #endregion Public Constructors

        #region Protected Methods

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            LoadTestPackageExt loadTestPackageExt = dte.GetObject("Microsoft.VisualStudio.TestTools.LoadTesting.LoadTestPackageExt") as LoadTestPackageExt;

            if (loadTestPackageExt != null)
            {
                foreach (WebTestResultViewer webTestResultViewer in loadTestPackageExt.WebTestResultViewerExt.ResultWindows)
                {
                    WindowCreated(webTestResultViewer);
                }

                // Create event handlers.
                loadTestPackageExt.WebTestResultViewerExt.WindowCreated += new EventHandler<WebTestResultViewerExt.WindowCreatedEventArgs>(WebTestResultViewerExt_WindowCreated);
                loadTestPackageExt.WebTestResultViewerExt.WindowClosed += new EventHandler<WebTestResultViewerExt.WindowClosedEventArgs>(WebTesResultViewer_WindowClosed);
                loadTestPackageExt.WebTestResultViewerExt.SelectionChanged += new EventHandler<WebTestResultViewerExt.SelectionChangedEventArgs>(WebTestResultViewer_SelectedChanged);
                loadTestPackageExt.WebTestResultViewerExt.TestCompleted += WebTestResultViewerExt_TestCompleted;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void WindowCreated(WebTestResultViewer viewer)
        {
            // Instantiate an instance of the resultControl referenced in the
            // WebPerfTestResultsViewerControl project.
            WebTestResultControl resultControl = new WebTestResultControl();
            //resultControl totalResultControl= new resultControl();
            // totalResultControl.Name="full";

            // Add to the dictionary of open playback windows.
            System.Diagnostics.Debug.Assert(!m_controls.ContainsKey(viewer.TestResultId));
            List<UserControl> userControls = new List<UserControl>();
            userControls.Add(resultControl);
            //userControls.Add(totalResultControl);

            // Add Guid to the m_control List to manage Result viewers and controls.
            m_controls.Add(viewer.TestResultId, userControls);

            // Add tabs to the playback control.
            resultControl.Dock = DockStyle.Fill;
            // totalResultControl.Dock=DockStyle.Fill;
            viewer.AddResultPage(new Guid(), "WebTest Log", resultControl);
            // viewer.AddResultPage(new Guid(), "WebTest full Log", totalResultControl);
        }

        private void WebTesResultViewer_WindowClosed(object sender, WebTestResultViewerExt.WindowClosedEventArgs e)
        {
            if (m_controls.ContainsKey(e.WebTestResultViewer.TestResultId))
            {
                m_controls.Remove(e.WebTestResultViewer.TestResultId);
            }
        }

        private void WebTestResultViewer_SelectedChanged(object sender, WebTestResultViewerExt.SelectionChangedEventArgs e)
        {
            WebTestResultViewer x = (WebTestResultViewer)sender;

            foreach (UserControl userControl in m_controls[e.TestResultId])
            {
                // Update the userControl in each result viewer.
                WebTestResultControl resultControl = userControl as WebTestResultControl;
                if (resultControl != null)
                {
                    // Call the resultControl's Update method (This will be added in the next procedure).
                    if (e.SelectedItem != null)
                    {
                        switch (e.SelectedItem.GetType().Name)
                        {
                            case "WebTestResultComment":
                                WebTestResultComment u = (WebTestResultComment)e.SelectedItem;
                                resultControl.UpdateComment(u);
                                break;

                            case "WebTestResultPage":
                                resultControl.Update(e.WebTestRequestResult);
                                break;

                            case "WebTestResultLoopIteration":
                                resultControl.UpdateGrid();
                                break;

                            default:
                                resultControl.UpdateGrid();
                                break;
                        }
                    }
                }
            }
        }

        private void WebTestResultViewerExt_TestCompleted(object sender, WebTestResultViewerExt.TestCompletedEventArgs e)
        {
        }

        private void WebTestResultViewerExt_WindowCreated(object sender, WebTestResultViewerExt.WindowCreatedEventArgs e)
        {
            WindowCreated(e.WebTestResultViewer);
        }

        #endregion Private Methods
    }
}