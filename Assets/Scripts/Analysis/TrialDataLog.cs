using System.Collections.Generic;

namespace BrushingAndLinking
{
    public class TrialDataLog
    {
        public float CompletionTime;
        public float TimeUntilLastTabletInteraction;
        public float TimeUntilFirstObjectSelected;
        public float TimeUntilLastObjectSelected;
        public float TimeOutTabletViewing;
        public int CountOfSelectedObjects;
        public int CountOfWrongSelectedObjects;
        public List<string> SelectedObjectNames = new();
    }
}