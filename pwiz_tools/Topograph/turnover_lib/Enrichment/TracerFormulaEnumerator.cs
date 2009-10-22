﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pwiz.Common.Chemistry;

namespace pwiz.Topograph.Enrichment
{
    /// <summary>
    /// Holds the number of tracers in a peptide.
    /// For instance, if there are tracers called "Argsix" and "Argten", and the peptide has 3 Argenines in it,
    /// then the following are valid tracer formulas:
    /// Argsix2
    /// ArgsixArgten2
    /// but the following are not:
    /// Argsix2Argten2
    /// Argsix4
    /// </summary>
    public class TracerFormula : Formula<TracerFormula>
    {
    }
    public class TracerFormulaEnumerator : IEnumerator<TracerFormula>
    {
        private readonly Dictionary<String, int> _tracerSymbolCounts = new Dictionary<string, int>();
        private readonly Dictionary<String, TracerDef> _tracerDefs = new Dictionary<string, TracerDef>();
        private readonly List<String> _tracerDefNames = new List<string>();
        public TracerFormulaEnumerator(String peptideSequence, ICollection<TracerDef> tracerDefs)
        {
            foreach (var tracerDef in tracerDefs)
            {
                _tracerDefs.Add(tracerDef.Name, tracerDef);
                if (!tracerDef.AminoAcidSymbol.HasValue)
                {
                    continue;
                }
                _tracerDefNames.Add(tracerDef.Name);
                if (_tracerSymbolCounts.ContainsKey(tracerDef.TraceeSymbol))
                {
                    continue;
                }
                String newPeptideSequence = peptideSequence.Replace("" + tracerDef.AminoAcidSymbol.Value, "");
                _tracerSymbolCounts.Add(tracerDef.TraceeSymbol, peptideSequence.Length - newPeptideSequence.Length);
                peptideSequence = newPeptideSequence;
            }
            _tracerDefNames.Sort();
            Molecule remainingFormula = AminoAcidFormulas.Default.GetFormula(peptideSequence);
            var elementTracerNames = new List<string>();
            foreach (var tracerDef in tracerDefs)
            {
                if (tracerDef.AminoAcidSymbol.HasValue)
                {
                    continue;
                }
                elementTracerNames.Add(tracerDef.Name);
                if (_tracerSymbolCounts.ContainsKey(tracerDef.TraceeSymbol))
                {
                    continue;
                }
                _tracerSymbolCounts.Add(tracerDef.TraceeSymbol, remainingFormula.GetElementCount(tracerDef.TraceeSymbol));
            }
            elementTracerNames.Sort();
            _tracerDefNames.AddRange(elementTracerNames);
        }

        public TracerFormula Current { get; private set; }
        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (Current == null)
            {
                Current = TracerFormula.Empty;
                return true;
            }
            var remainingSymbolCounts = new Dictionary<String, int>(_tracerSymbolCounts);
            foreach (var tracerDefName in _tracerDefNames)
            {
                var tracerDef = _tracerDefs[tracerDefName];
                remainingSymbolCounts[tracerDef.TraceeSymbol]
                    = remainingSymbolCounts[tracerDef.TraceeSymbol]
                      - Current.GetElementCount(tracerDef.Name);
            }
            foreach (String tracerDefName in _tracerDefNames)
            {
                var tracerDef = _tracerDefs[tracerDefName];
                int currentTracerCount = Current.GetElementCount(tracerDefName);
                int remainingTracerCount = remainingSymbolCounts[tracerDef.TraceeSymbol];
                if (remainingTracerCount > 0)
                {
                    Current = Current.SetElementCount(tracerDefName, currentTracerCount + 1);
                    return true;
                }
                Current = Current.SetElementCount(tracerDefName, 0);
                remainingSymbolCounts[tracerDef.TraceeSymbol] = remainingSymbolCounts[tracerDef.TraceeSymbol] +
                                                               currentTracerCount;
            }
            return false;
        }

        public void Reset()
        {
            Current = null;
        }

        object IEnumerator.Current { get { return Current;} }
    }
}
