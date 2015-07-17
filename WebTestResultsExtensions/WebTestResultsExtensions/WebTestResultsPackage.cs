﻿//------------------------------------------------------------------------------
// <copyright file="WebTestResultsPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace WebTestResultsExtensions
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio is to
    /// implement the IVsPackage interface and register itself with the shell. This package uses the
    /// helper classes defined inside the Managed Package Framework (MPF) to do it: it derives from
    /// the Package class that provides the implementation of the IVsPackage interface and uses the
    /// registration attributes defined in the framework to register itself and its components with
    /// the shell. These attributes tell the pkgdef creation utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset
    /// Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(WebTestResultsPackageGuids.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class WebTestResultsPackage : Package
    {
        #region Private Fields

        private Dictionary<Guid, List<UserControl>> m_controls = new Dictionary<Guid, List<UserControl>>();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTestResultsPackage"/> class.
        /// </summary>
        public WebTestResultsPackage()
        {
            // Inside this method you can place any initialization code that does not require any
            // Visual Studio service because at this point the package object is created but not
            // sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #endregion Public Constructors

        #region Protected Methods

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited,
        /// so this is the place where you can put all the initialization code that rely on services
        /// provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            LoadTestPackageExt loadTestPackageExt = dte.GetObject("Microsoft.VisualStudio.TestTools.LoadTesting.LoadTestPackageExt") as LoadTestPackageExt;

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

        #endregion Protected Methods

        #region Private Methods

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
                            resultControl.Update();
                            break;

                        default:
                            resultControl.Update();
                            break;
                    }
                }
            }
        }

        private void WebTestResultViewerExt_TestCompleted(object sender, WebTestResultViewerExt.TestCompletedEventArgs e)
        {
            string x = "";
        }

        private void WebTestResultViewerExt_WindowCreated(object sender, WebTestResultViewerExt.WindowCreatedEventArgs e)
        {
            WindowCreated(e.WebTestResultViewer);
        }

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

        #endregion Private Methods
    }
}