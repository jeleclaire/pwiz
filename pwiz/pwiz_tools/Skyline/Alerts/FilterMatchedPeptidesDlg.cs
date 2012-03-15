/*
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

using System;
using System.Windows.Forms;
using pwiz.Skyline.Model.Proteome;
using pwiz.Skyline.Properties;
using pwiz.Skyline.SettingsUI;

namespace pwiz.Skyline.Alerts
{
    public sealed partial class FilterMatchedPeptidesDlg : Form
    {

        public FilterMatchedPeptidesDlg(int numWithDuplicates, int numUnmatched, int numFiltered, bool single)
        {
            InitializeComponent();

            UnmatchedCount = numUnmatched;
            DuplicateMatchesCount = numWithDuplicates;
            FilteredCount = numWithDuplicates;

            if(ViewLibraryPepMatching.FilterMultipleProteinMatches == BackgroundProteome.DuplicateProteinsFilter.AddToAll)
                radioAddToAll.Checked = true;
            else if (ViewLibraryPepMatching.FilterMultipleProteinMatches == BackgroundProteome.DuplicateProteinsFilter.FirstOccurence)
                radioFirstOccurence.Checked = true;
            else
                radioNoDuplicates.Checked = true;

            radioAddUnmatched.Checked = Settings.Default.LibraryPeptidesAddUnmatched;
            radioFilterUnmatched.Checked = !radioAddUnmatched.Checked;

            radioKeepFiltered.Checked = Settings.Default.LibraryPeptidesKeepFiltered;
            radioDoNotAddFiltered.Checked = !radioKeepFiltered.Checked;
            
            if (numWithDuplicates != 0)
                msgDuplicatePeptides.Text = single ? "This peptide matches multiple proteins."
                    : (numWithDuplicates == 1 ? "1 peptide matching multiple proteins."
                    : string.Format("{0} peptides matching multiple proteins.", numWithDuplicates));
            else
            {
                int delta = panelUnmatched.Top - panelMultiple.Top;
                panelMultiple.Hide();
                panelUnmatched.Top -= delta;
                panelFiltered.Top -= delta;
                Height -= delta;
            }
            if (numUnmatched != 0)
                msgUnmatchedPeptides.Text = single ? "This peptide does not have a matching protein."
                    : (numUnmatched == 1 ? "1 peptide without a matching protein."
                    : string.Format("{0} peptides without matching proteins.", numUnmatched));
            else
            {
                int delta = panelFiltered.Top - panelUnmatched.Top;
                panelUnmatched.Hide();
                panelFiltered.Top -= delta;
                Height -= delta;
            }
            if (numFiltered != 0)
                msgFilteredPeptides.Text = single ? "This peptide does not match the current filter settings."
                    : (numFiltered == 1 ? "1 peptide not matching the current filter settings."
                    : string.Format("{0} peptides not matching the current filter settings.", numFiltered));
            else
            {
                int delta = panelFiltered.Bottom - panelFiltered.Top;
                panelFiltered.Hide();
                Height -= delta;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            OkDialog();
        }

        public void OkDialog()
        {
            var duplicateProteinsFilter = BackgroundProteome.DuplicateProteinsFilter.AddToAll;
            if (radioNoDuplicates.Checked)
                duplicateProteinsFilter = BackgroundProteome.DuplicateProteinsFilter.NoDuplicates;
            else if (radioFirstOccurence.Checked)
                duplicateProteinsFilter = BackgroundProteome.DuplicateProteinsFilter.FirstOccurence;

            Settings.Default.LibraryPeptidesAddDuplicatesEnum = duplicateProteinsFilter.ToString();

            Settings.Default.LibraryPeptidesAddUnmatched = radioAddUnmatched.Checked;
            Settings.Default.LibraryPeptidesKeepFiltered = radioKeepFiltered.Checked;
            
            DialogResult = DialogResult.OK;
        }

        // For testing.
        public BackgroundProteome.DuplicateProteinsFilter DuplicateProteinsFilter
        {
            get
            {
                if (radioNoDuplicates.Checked)
                    return BackgroundProteome.DuplicateProteinsFilter.NoDuplicates;
                if (radioFirstOccurence.Checked)
                    return BackgroundProteome.DuplicateProteinsFilter.FirstOccurence;
                return BackgroundProteome.DuplicateProteinsFilter.AddToAll;
            }
            set
            {
                if (Equals(value, BackgroundProteome.DuplicateProteinsFilter.NoDuplicates))
                    radioNoDuplicates.Checked = true;
                else if (Equals(value, BackgroundProteome.DuplicateProteinsFilter.FirstOccurence))
                    radioFirstOccurence.Checked = true;
                else
                    radioAddToAll.Checked = true;
            }
        }

        public bool AddUnmatched
        {
            get
            {
                return radioAddUnmatched.Checked;
            }
            set 
            { 
                radioAddUnmatched.Checked = value;
                radioFilterUnmatched.Checked = !radioAddUnmatched.Checked;
            }
        }

        public bool AddFiltered
        {
            get { return radioKeepFiltered.Checked = true; }
            set 
            { 
                radioKeepFiltered.Checked = value;
                radioDoNotAddFiltered.Checked = !radioKeepFiltered.Checked;
            }
        }

        public int DuplicateMatchesCount { get; set; }

        public int UnmatchedCount { get; set; }
        
        public int FilteredCount { get; set; }
    }
}