using System.Reflection.Emit;

namespace PvmrmViewer.DataObjects
{
	public class PvmrmEntry
	{
		public string ProteinAccession { get; set; }
		public string ProteinName { get; set; }
		public string VariantCodex { get; set; }
		public string MinorAlleleFrequency { get; set; }
		public string ReferencePeptide { get; set; }
		public string ModifiedPeptide { get; set; }
		public string SnpId { get; set; }

		public PvmrmEntry(string accession, string name, string variant, string maf, string refPeptide, string modPeptide, string snpId)
		{
			ProteinAccession = accession;
			ProteinName = name;
			VariantCodex = variant;
			MinorAlleleFrequency = maf;
			ReferencePeptide = refPeptide;
			ModifiedPeptide = modPeptide;
			SnpId = snpId;
		}
	}
}
