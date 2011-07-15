using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using BioCSharp;
using System.Text.RegularExpressions;
using System.Windows.Media;
using iMoBio.PlasmidCanvas;
using BioCSharp.Seqs;
using BioCSharp.Bio;
using System.Windows;

namespace iMoBio.PlasmidCanvas
{
    public enum RangeType { None = 0, Sequence, Feature }

    /// <summary>
    /// A <code>Range</code> object refers to a contiguous area in the plasmid. 
    /// Type of <code>Range</code>, <code>RangeType</code> could be sequence or
    /// feature.
    /// For sequence range <code>Start</code> and <code>End</code> completely portion of
    /// sequence. A Range object can be as small as the insertion point 
    /// or as large as the entire sequence.
    /// For annotation range <code>Start</code> and <code>End</code> specify index
    /// of <code>InsdcRecord.Features</code>. Usually <code>Start</code> and <code>End</code>
    /// is the same to indicate single <code>Feature</code>. 
    /// </summary>
    public class Range
    {

        public long Start { get; set; }
        public long End { get; set; }
        public RangeType Type { get; set; }

        /// <summary>
        /// Define a sequence range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Range(long start, long end)
        {
            Start = start;
            End = end;
            Type = RangeType.Sequence;
        }

        public Range(RangeType type, long start)
        {
            Type = type;
            Start = start;
            End = start;
        }

    }



    /// <summary>
    /// <code>PlasmidRecord</code> wrap <code>InsdcRecord</code> and support functionality for
    /// visualization and minupulation of this in various plasmids simultanuously. 
    ///  
    /// 
    /// </summary>
    public class PlasmidRecord : InsdcRecord
    {

        private List<Range> selection;
        private string[] _enzymes;
        private CuttingSite[][] _cuttingSites;


        public static readonly DependencyProperty SelectionProperty;

        
        public static readonly RoutedEvent SelectionChangedEvent;
        public static readonly RoutedEvent EnzymesChangedEvent;
        private List<RoutedEventHandler> selectionEvents = new List<RoutedEventHandler>();
        private List<RoutedEventHandler> enzymeChangedEvents = new List<RoutedEventHandler>();

        static PlasmidRecord()
        {
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(
                new List<Range>(), FrameworkPropertyMetadataOptions.Inherits,
                new PropertyChangedCallback(OnSelectionChanged));
            SelectionProperty = DependencyProperty.Register("Selection",
                typeof(List<Range>), typeof(PlasmidRecord), metadata);

            SelectionChangedEvent = EventManager.RegisterRoutedEvent(
                "SelectTo", RoutingStrategy.Direct,
                typeof(RoutedEventHandler), typeof(PlasmidRecord));


            FrameworkPropertyMetadata enzymeAddedMetadata = new FrameworkPropertyMetadata(
               new string[0], FrameworkPropertyMetadataOptions.Inherits,
               new PropertyChangedCallback(OnEnzymeChanged));

            EnzymesChangedEvent = EventManager.RegisterRoutedEvent(
                "EnzymeAdd", RoutingStrategy.Direct,
                typeof(RoutedEventHandler), typeof(PlasmidRecord));
        }

        /// <summary>
        /// Assume <code>PlasmidRecord</code> from base class <code>InsdeRecord</code>. Member fields
        /// are not copy but instead reference, so that the super object is identical to supplied
        /// base class. If you don't want to this feature use <code>InsdcRecord.Clone()</code> to
        /// do deep copy. User should register in change notification listeners.
        /// </summary>
        /// <param name="record"></param>
        public PlasmidRecord(InsdcRecord record)
            : base(record.Seq, record.Name, record.Description, record.Features,
            record.Keys, record.ListKeys)
        {

            // set default enzyme
            SetEnzymes(EnzymeListCommon);
        }

        public event RoutedEventHandler SelectionToEvent
        {
            add
            {
                selectionEvents.Add(value);
            }
            remove
            {
                selectionEvents.Remove(value);
            }
        }

        public event RoutedEventHandler EnzymeChangedEvent
        {
            add
            {
                enzymeChangedEvents.Add(value);
            }
            remove
            {
                enzymeChangedEvents.Remove(value);
            }
        }

        /// <summary>
        /// Define a new selection. This will raise <code>OnSelectionChanged</code> event.
        /// </summary>
        public List<Range> Selection
        {
            // FIXME: usual GetValue and SetValue doesn't work.
            set { selection = value; }
            get { return selection; }
        }

        


        public void RemoveEnzyme(string enzymeName)
        {
        }

        /// <summary>
        /// Get or get name of enzymes. This will raise <see cref="OnEnzymeChangedEvent"/>
        /// 
        /// <seealso cref="AddEnzyme"/>, <seealso cref="RemoveEnzyme"/> to added or remove 
        /// one by one.
        /// </summary>
        public string[] Enzymes
        {
            set
            {
                SetEnzymes(value);

                // raise the event
                RoutedEventArgs args = new RoutedEventArgs(PlasmidRecord.EnzymesChangedEvent, this);

                RaiseEvent(args);
            }
            get
            {
                return _enzymes;
            }
        }

        private void SetEnzymes(string[] enzymes)
        {
            _enzymes = enzymes;
            _cuttingSites = new CuttingSite[_enzymes.Length][];

            List<Feature> fts = new List<Feature>();
            for (int i = 0; i < _enzymes.Length; ++i)
            {
                _cuttingSites[i] = Enzyme.Restrict(Seq, _enzymes[i]);
                foreach (CuttingSite site in _cuttingSites[i])
                {
                    string location;
                    if (site.Length >= 0) location = site.Index + ".." + (site.Index + site.Length);
                    else location = "complement(" + site.Index + ".." + (site.Index - site.Length) + ")";
                    Dictionary<string, string> qualifiers = new Dictionary<string, string>();
                    qualifiers.Add(Feature.QUALIFIER_label, site.Enzyme);
                    Feature ft = new Feature(Feature.KEY_restriction_site, location, qualifiers);
                    fts.Add(ft);
                }
            }
            AddFeatures(fts);
        }


        /// <summary>
        /// Get cutting sites of enzyme in <see cref="Enzymes"/> list.
        /// </summary>
        /// <param name="enzymeName"></param>
        /// <returns></returns>
        public CuttingSite[] GetCuttingSite(string enzymeName)
        {
            int idx = Array.IndexOf(_enzymes, enzymeName);
            return _cuttingSites[idx];

        }

        /// <summary>
        /// Digest the plasmid with a enzyme. After finding the restriction size,
        /// this will raise <see cref="OnDigest"/> event. 
        /// 
        /// <seealso cref="Enzymes"/>, <seealso cref="EnzymeRemove"/>
        /// </summary>
        /// <param name="enzymeName">Name of enzyem as in rebase file</param>
        public void AddEnzyme(string enzymeName)
        {
            // make room for one more site
            CuttingSite[][] oldSites = _cuttingSites;
            _cuttingSites = new CuttingSite[_cuttingSites.Length + 1][];
            for (int i = 0; i < oldSites.Length; ++i)
                _cuttingSites[i] = oldSites[i];

            _cuttingSites[oldSites.Length] = Enzyme.Restrict(Seq, enzymeName);

            // raise the event
            RaiseEvent(new RoutedEventArgs(PlasmidRecord.EnzymesChangedEvent, this));
        }

        private static void OnEnzymeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PlasmidRecord record = (PlasmidRecord)sender;

            record.Enzymes = (string[])e.NewValue;
            record.RaiseEvent(new RoutedEventArgs(PlasmidRecord.EnzymesChangedEvent, record));
        }


        /// <summary>
        /// Extend current selection to <code>location</code>. If there no selection, a 
        /// new selection range will be created starting from <code>location</code>. 
        /// This shall raise <code>OnSelectionChanged</code> event.
        /// </summary>
        /// <param name="location"></param>
        public void SelectTo(long location)
        {
            if (selection == null)
            {
                selection = new List<Range>();
                if (location < 0)
                    location = BaseCount - location;
                location = location % BaseCount;
                selection.Add(new Range(location, location));
            }
            else
            {
                Range lastRange = selection[selection.Count - 1];
                if (location == lastRange.End)
                {
                    return;
                }
                lastRange.End = location;
            }

            //Console.WriteLine(this.ToString() + ": Number of selection: " + selection.Count);

            // raise the event
            RoutedEventArgs args = new RoutedEventArgs(PlasmidRecord.SelectionChangedEvent, this);

            RaiseEvent(args);
        }

        /// <summary>
        /// Start a new selection. If <code>location</code> is negative or larger than
        /// <code>BaseCount</code>, clear the current selection.
        /// </summary>
        /// <param name="location"></param>
        public void SelectStart(long location)
        {
            selection = null;
            if (location > 0 && location < BaseCount)
                SelectTo(location);
        }

        private void RaiseEvent(RoutedEventArgs args)
        {
            foreach (RoutedEventHandler handler in selectionEvents)
            {
                handler.Invoke(this, args);
            }
        }

        private static void OnSelectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PlasmidRecord record = (PlasmidRecord)sender;

            record.selection = (List<Range>)e.NewValue;
            record.RaiseEvent(new RoutedEventArgs(PlasmidRecord.SelectionChangedEvent, record));
        }

        public string[] EnzymeListCommon = new string[] { "EcoRI", "SmaI", "SalI", "PstI", "XhoI", "EcoRV", "BglII", "XbaI", "HindIII", "BamHI" };

    }

}
