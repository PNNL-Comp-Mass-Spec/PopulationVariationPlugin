
namespace PVMRM
{
   public class Snp
   {
		// Auto-properties
		public int SnpID { get; set; }
        public int aa_position { get; set; }
        public string NewResidue { get; set; }
        public double MinorAlleleFrequency { get; set; }
        public string ModifiedPeptideString { get; set; }
        public string Codex { get; set; } 
       
      // "Calculates" the modified peptide string and codex after the query results are returned.
        // ******NOTE: "Position" will now be 1-based, not 0-based 
        // [e.g. protein NP_001165881 (rs121917836) with variant at position 65 will read "E66K" in the codex]*******
       // only used inside of Main right now. Change or keep?
        public void SetModifiedPeptideStringAndCodex(string origPeptide, int aa_position, int indexStart, string residue)
        {
            if (indexStart > -1)
            {
                ModifiedPeptideString = (origPeptide.Substring(0, (aa_position - indexStart))
                                         + residue + origPeptide.Substring(aa_position + (residue.Length) - indexStart));
                Codex = origPeptide.Substring(aa_position - indexStart, 1) + (aa_position+1) + residue;
            }
            else
            {
                ModifiedPeptideString = "";
                Codex = "";
            }
        }
   }
}
