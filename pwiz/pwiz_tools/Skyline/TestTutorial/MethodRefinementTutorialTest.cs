﻿/*
 * Original author: Alana Killeen <killea .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2010 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Skyline.Controls;
using pwiz.Skyline.Controls.SeqNode;
using pwiz.Skyline.EditUI;
using pwiz.Skyline.FileUI;
using pwiz.Skyline.Model;
using pwiz.Skyline.SettingsUI;
using pwiz.SkylineTestUtil;


namespace pwiz.SkylineTestTutorial
{
    /// <summary>
    /// Testing the tutorial for Skyline Targeted Method Refinement
    /// </summary>
    [TestClass]
    public class MethodRefinementTutorialTest : AbstractFunctionalTest
    {
        [TestMethod]
        public void TestMethodRefinementTutorial()
        {
            string supplementZip = (ExtensionTestContext.CanImportThermoRaw ?
                @"https://brendanx-uw1.gs.washington.edu/tutorials/MethodRefineSupplement.zip" :
                @"https://brendanx-uw1.gs.washington.edu/tutorials/MethodRefineSupplementMzml.zip");

            TestFilesZipPaths = new[] { supplementZip, ExtensionTestContext.CanImportThermoRaw ?
                    @"https://brendanx-uw1.gs.washington.edu/tutorials/MethodRefine.zip" :
                    @"https://brendanx-uw1.gs.washington.edu/tutorials/MethodRefineMzml.zip"
                 };

         
            RunFunctionalTest();
        }

        protected override void DoTest()
        {
            // Skyline Targeted Method Refinement

            var folderMethodRefine = ExtensionTestContext.CanImportThermoRaw ? "MethodRefine" : "MethodRefineMzml";

            // Results Data, p. 2
            RunUI(() =>
            {
              SkylineWindow.OpenFile(TestFilesDirs[1].GetTestPath(folderMethodRefine + @"\WormUnrefined.sky"));
              SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0].Nodes[0];
            });

            // Unrefined Methods, p. 3
           RunDlg<ExportMethodDlg>(() => SkylineWindow.ShowExportMethodDialog(ExportFileType.List), exportDlg =>
            {
                exportDlg.ExportStrategy = ExportStrategy.Buckets;
                exportDlg.MethodType = ExportMethodType.Standard;
                exportDlg.OptimizeType = ExportOptimize.NONE;
                exportDlg.MaxTransitions = 59;
                exportDlg.OkDialog(TestFilesDirs[1].GetTestPath(folderMethodRefine + @"\worm"));
            });

            // Importing Multiple Injection Data, p. 4
           RunDlg<ManageResultsDlg>(SkylineWindow.ManageResults, manageResultsDlg =>
           {
                manageResultsDlg.Remove();
                manageResultsDlg.OkDialog();
           });
           RunUI(() => SkylineWindow.SaveDocument());
           RunDlg<ImportResultsDlg>(SkylineWindow.ImportResults, importResultsDlg =>
           {
                 importResultsDlg.RadioAddNewChecked = true;
                 importResultsDlg.NamedPathSets = ImportResultsDlg.GetDataSourcePathsDir(TestFilesDirs[0].FullPath).Take(15).ToArray();
                 importResultsDlg.NamedPathSets[0] =
                     new KeyValuePair<string, string[]>("Unrefined", importResultsDlg.NamedPathSets[0].Value);
                 importResultsDlg.OptimizationName = ExportOptimize.CE;
                 importResultsDlg.OkDialog();
           });
           WaitForCondition(() => SkylineWindow.Document.Settings.MeasuredResults.IsLoaded);
           RunDlg<ImportResultsDlg>(SkylineWindow.ImportResults, importResultsDlg =>
           {
                importResultsDlg.RadioAddExistingChecked = true;
                importResultsDlg.NamedPathSets = ImportResultsDlg.GetDataSourcePathsDir(TestFilesDirs[0].FullPath).Skip(15).ToArray();
                importResultsDlg.OptimizationName = ExportOptimize.CE;
                importResultsDlg.OkDialog();
           });
           WaitForCondition(() => SkylineWindow.Document.Settings.MeasuredResults.IsLoaded);
           
           // Simple Manual Refinement, p. 6 
           RunUI(() =>
           {
                 SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0].Nodes[0];
                 SkylineWindow.AutoZoomNone();
                 SkylineWindow.AutoZoomBestPeak();
                 SkylineWindow.EditDelete();
                 SkylineWindow.ShowRTLinearRegressionGraph();
           });
           RunDlg<ShowRTThresholdDlg>(SkylineWindow.ShowRTThresholdDlg, rtThresholdDlg =>
           {
                rtThresholdDlg.Threshold = 0.95;
                rtThresholdDlg.OkDialog();
           });
           WaitForConditionUI(() => SkylineWindow.RTGraphController.RegressionRefined != null);
           RunDlg<EditRTDlg>(SkylineWindow.CreateRegression, editRTDlg => editRTDlg.OkDialog());

           // Missing Data, p. 10
           RunUI(() =>
           {
                 SkylineWindow.RTGraphController.SelectPeptide(SkylineWindow.Document.GetPathTo(1, 163));
                 Assert.AreEqual("YLAEVASEDR", SkylineWindow.SequenceTree.SelectedNode.Text);
                 var nodePep = (PeptideDocNode)((SrmTreeNode)SkylineWindow.SequenceTree.SelectedNode).Model;
                 Assert.AreEqual(null,
                                 nodePep.GetPeakCountRatio(
                                     SkylineWindow.SequenceTree.GetDisplayResultsIndex(nodePep)));
                 SkylineWindow.RTGraphController.GraphSummary.DocumentUIContainer.FocusDocument();
                 SkylineWindow.SequenceTree.SelectedPath = SkylineWindow.Document.GetPathTo(1, 157);
                 Assert.AreEqual("VTVVDDQSVILK", SkylineWindow.SequenceTree.SelectedNode.Text);
           });
           WaitForGraphs();
           RunUI(() =>
           {
                 var graphChrom = SkylineWindow.GetGraphChromatogram("Unrefined");
                 Assert.AreEqual(2, graphChrom.Files.Count);
                 SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0];
                 SkylineWindow.RTGraphController.GraphSummary.Close();
                 
                 // Picking Measurable Peptides and Transitions, p. 12
                 SkylineWindow.ExpandPeptides();
                 SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0].Nodes[0];
                 Assert.IsTrue(SkylineWindow.SequenceTree.SelectedNode.Nodes[0].Text.Contains((0.78).ToString()));
                 SkylineWindow.EditDelete();
                 SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0].Nodes[0];
                 Assert.IsTrue(SkylineWindow.SequenceTree.SelectedNode.Nodes[0].Text.Contains((0.63).ToString()));
                 SkylineWindow.EditDelete();
                 PeptideTreeNode nodePep;
                 for (int i = 0; i < 2; i++)
                 {
                     nodePep = (PeptideTreeNode) SkylineWindow.SequenceTree.Nodes[0].Nodes[i];
                     nodePep.ExpandAll();
                     foreach (TransitionTreeNode nodeTran in nodePep.Nodes[0].Nodes)
                     {
                         TransitionDocNode nodeTranDoc = (TransitionDocNode) nodeTran.Model;
                         Assert.AreEqual((int) SequenceTree.StateImageId.peak,
                                         TransitionTreeNode.GetPeakImageIndex(nodeTranDoc,
                                                                              (PeptideDocNode) nodePep.Model,
                                                                              SkylineWindow.SequenceTree));
                         var resultsIndex = SkylineWindow.SequenceTree.GetDisplayResultsIndex(nodePep);
                         var rank = nodeTranDoc.GetPeakRank(resultsIndex);
                         if (rank == null || rank > 3)
                             SkylineWindow.SequenceTree.SelectedNode = nodeTran;
                         SkylineWindow.SequenceTree.KeysOverride = Keys.Control;
                     }
                 }
                 nodePep = (PeptideTreeNode) SkylineWindow.SequenceTree.Nodes[0].Nodes[2];
                 nodePep.ExpandAll();
                 foreach (TransitionTreeNode nodeTran in nodePep.Nodes[0].Nodes)
                 {
                     TransitionDocNode nodeTranDoc = (TransitionDocNode) nodeTran.Model;
                     Assert.AreEqual((int) SequenceTree.StateImageId.peak,
                                     TransitionTreeNode.GetPeakImageIndex(nodeTranDoc,
                                                                          (PeptideDocNode) nodePep.Model,
                                                                          SkylineWindow.SequenceTree));
                     var name = ((TransitionDocNode) nodeTran.Model).FragmentIonName;
                     if (!(name == "y11" || name == "y13" || name == "y14"))
                         SkylineWindow.SequenceTree.SelectedNode = nodeTran;
                     SkylineWindow.SequenceTree.KeysOverride = Keys.Control;
                 }
                 SkylineWindow.SequenceTree.KeysOverride = Keys.None;
                 SkylineWindow.EditDelete();
                 for (int i = 0; i < 3; i++)
                     Assert.IsTrue(SkylineWindow.SequenceTree.Nodes[0].Nodes[i].Nodes[0].Nodes.Count == 3);
                 SkylineWindow.AutoZoomNone();
           });

           // Automated Refinement, p. 16
           RunDlg<RefineDlg>(SkylineWindow.ShowRefineDlg, refineDlg =>
           {
                refineDlg.MaxTransitionPeakRank = 3;
                refineDlg.PreferLargerIons = true;
                refineDlg.RemoveMissingResults = true;
                refineDlg.RTRegressionThreshold = 0.95;
                refineDlg.DotProductThreshold = 0.95;
                refineDlg.OkDialog();
           });
           WaitForCondition(() => SkylineWindow.Document.PeptideCount == 72);
           RunUI(() =>
           {
                Assert.AreEqual(72, SkylineWindow.Document.PeptideCount);
                Assert.AreEqual(216, SkylineWindow.Document.TransitionCount);
                SkylineWindow.CollapsePeptides();
                SkylineWindow.Undo();
           });
           RunDlg<RefineDlg>(SkylineWindow.ShowRefineDlg, refineDlg =>
           {
                refineDlg.MaxTransitionPeakRank = 6;
                refineDlg.RemoveMissingResults = true;
                refineDlg.RTRegressionThreshold = 0.90;
                refineDlg.DotProductThreshold = 0.90;
                refineDlg.OkDialog();
           });
           WaitForCondition(() => SkylineWindow.Document.PeptideCount == 110);
           RunUI(() =>
           {
                Assert.AreEqual(110, SkylineWindow.Document.PeptideCount);

                // Scheduling for Efficient Acquisition, p. 17 
                SkylineWindow.Undo();
           });
           RunDlg<ManageResultsDlg>(SkylineWindow.ManageResults, manageResultsDlg =>
           {
                manageResultsDlg.Remove();
                manageResultsDlg.OkDialog();
           });
           var importResultsDlg0 = ShowDialog<ImportResultsDlg>(SkylineWindow.ImportResults);
           RunUI(() =>
           {
                SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0];
                importResultsDlg0.RadioCreateMultipleMultiChecked = true;
                var pathSets = new KeyValuePair<string, string[]>[2];
                pathSets[0] = new KeyValuePair<string, string[]>("Unscheduled01",
                 new[] {string.Format("{0}\\{1}\\Unscheduled01\\Unscheduled_REP01_0001{2}", 
                    TestFilesDirs[1].FullPath, folderMethodRefine, ExtensionTestContext.ExtThermoRaw),
                    string.Format("{0}\\{1}\\Unscheduled01\\Unscheduled_REP01_0002{2}", 
                    TestFilesDirs[1].FullPath, folderMethodRefine, ExtensionTestContext.ExtThermoRaw)});
                pathSets[1] = new KeyValuePair<string, string[]>("Unscheduled02",
                 new[] {string.Format("{0}\\{1}\\Unscheduled02\\Unscheduled_REP02_0001{2}", 
                    TestFilesDirs[1].FullPath, folderMethodRefine, ExtensionTestContext.ExtThermoRaw),
                    string.Format("{0}\\{1}\\Unscheduled02\\Unscheduled_REP02_0002{2}", 
                    TestFilesDirs[1].FullPath, folderMethodRefine, ExtensionTestContext.ExtThermoRaw)});
                importResultsDlg0.NamedPathSets = pathSets;
           });
           var importResultsNameDlg = ShowDialog<ImportResultsNameDlg>(importResultsDlg0.OkDialog);
           RunUI(importResultsNameDlg.NoDialog);
           WaitForCondition(() => SkylineWindow.Document.Settings.HasResults && SkylineWindow.Document.Settings.MeasuredResults.IsLoaded);
           var docCurrent = SkylineWindow.Document;
           RunUI(SkylineWindow.RemoveMissingResults);
           WaitForDocumentChange(docCurrent);
           Assert.AreEqual(86, SkylineWindow.Document.PeptideCount);
           Assert.AreEqual(255, SkylineWindow.Document.TransitionCount);

           // Measuring Retention Times, p. 17
           RunDlg<ExportMethodDlg>(() => SkylineWindow.ShowExportMethodDialog(ExportFileType.List), exportMethodDlg =>
           {
                exportMethodDlg.MaxTransitions = 130;
                exportMethodDlg.OkDialog(TestFilesDirs[1].FullPath + "\\unscheduled");
           });

           // Reviewing Retention Time Runs, p. 18
           RunUI(() =>
           {
                SkylineWindow.ShowGraphSpectrum(false);
                SkylineWindow.ArrangeGraphsTiled();
                SkylineWindow.AutoZoomNone();
                SkylineWindow.AutoZoomBestPeak();
                SkylineWindow.ShowRTSchedulingGraph();
           });
           WaitForCondition(() => SkylineWindow.GraphRetentionTime != null);
           
           // Creating a Scheduled Transition List, p. 20 
           RunDlg<PeptideSettingsUI>(SkylineWindow.ShowPeptideSettingsUI, peptideSettingsUI =>
           {
                peptideSettingsUI.TimeWindow = 4;
                peptideSettingsUI.OkDialog();
           });
           RunDlg<ExportMethodDlg>(() => SkylineWindow.ShowExportMethodDialog(ExportFileType.List), exportMethodDlg =>
           {
                exportMethodDlg.MethodType = ExportMethodType.Scheduled;
                exportMethodDlg.OkDialog(TestFilesDirs[1].FullPath + "\\scheduled");
           });

           // Reviewing Multi-Replicate Data, p. 22
           RunDlg<ManageResultsDlg>(SkylineWindow.ManageResults, manageResultsDlg =>
           {
                manageResultsDlg.RemoveAll();
                manageResultsDlg.OkDialog();
           });
           var importResultsDlg1 = ShowDialog<ImportResultsDlg>(SkylineWindow.ImportResults);
           RunDlg<OpenDataSourceDialog>(() => importResultsDlg1.NamedPathSets = importResultsDlg1.GetDataSourcePathsFile(null),
               openDataSourceDialog =>
               {
                   openDataSourceDialog.SelectAllFileType(ExtensionTestContext.ExtThermoRaw);
                       openDataSourceDialog.Open();
               });
           RunDlg<ImportResultsNameDlg>(importResultsDlg1.OkDialog, importResultsNameDlg0 =>
           {
                importResultsNameDlg0.Prefix = "Scheduled_";
                importResultsNameDlg0.YesDialog();
           });
           WaitForCondition(() => SkylineWindow.Document.Settings.HasResults && SkylineWindow.Document.Settings.MeasuredResults.IsLoaded);
           Assert.AreEqual(5, SkylineWindow.GraphChromatograms.Count(graphChrom => !graphChrom.IsHidden));
           RunUI(() =>
           {
                SkylineWindow.RemoveMissingResults();
                SkylineWindow.ArrangeGraphsTiled();
                SkylineWindow.ShowGraphRetentionTime(false);
           });
           WaitForCondition(() => SkylineWindow.GraphRetentionTime.IsHidden);
           RunUI(() =>
           {
                SkylineWindow.SequenceTree.SelectedNode = SkylineWindow.SequenceTree.Nodes[0].Nodes[0];
                SkylineWindow.ShowRTReplicateGraph();
                SkylineWindow.ShowPeakAreaReplicateComparison();
           });
           WaitForGraphs();
        }
    }
}