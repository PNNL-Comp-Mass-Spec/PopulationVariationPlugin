//Obsolete, but might be useful for sample data.
using System.Collections.Generic;

namespace PVMRM
{
    public class HardCodedVars
    {
        public string HardCodedAccession { get; set; }
        public List<string> HardCodedPeptideList { get; set; }
        public string HardCodedFilePath { get; set; }

        public void SetHardCodedVars()
        {
            HardCodedAccession = "NP_001165881";
            HardCodedPeptideList = new List<string>();
            HardCodedFilePath = @"\\protoapps\UserData\RodriguezL\PVMRM\Protein.fa";
            HardCodedPeptideList.Add("VGALLMGLHMSQKHTEMVLEMSIGA");
            HardCodedPeptideList.Add("GLVVYDYQQLLIAYKPAPG");
            HardCodedPeptideList.Add("AVPTSKLGQAEGRDAGSAPSGGDPAFLG");
        }
    }
}
