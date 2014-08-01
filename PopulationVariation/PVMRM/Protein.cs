using System.Collections.Generic;

namespace PVMRM
{
    public class Protein
    {
        public string RefSource { get; set; } //Indicates ref for refseq. This variable may go away.
        public string ProteinName { get; set; } //Refseq protein name, can be returned to the user as a way of validating what was found
        public string ProteinFullString { get; set; } //Full protein string from Fasta file
        public string ProteinAccession { get; private set; } //NP_ number (RefSeq) from user
        public bool FoundInFasta { get; set; } //Set to true if accession is found in Fasta
        public List<Peptide> PeptideList { get; set; } //One protein, many possible peptide entries.
        public bool FoundProteinLevelChange { get; set; } //Stop-Gain or a frameshift was found in the query
        public List<Snp> ProteinLevelSnps { get; set; } //these are for SNPs that affect the sequence as a whole, like a frame shift of stop-Gain.

       public Protein(string protAccession )
        {
            this.ProteinAccession = protAccession;
            PeptideList = new List<Peptide>();
            ProteinLevelSnps = new List<Snp>();
        }

        ///AddPeptide
        ///Parameters: string peptideString
        ///Purpose: The purpose of this method is to take in a string (the peptide sequence) and map it to
        ///the protein.  In the process we will create peptide objects and link them to this protein.
        public void AddPeptide(string peptideString)
        {
            Peptide peptideInstance = new Peptide(peptideString); //new and not fully-formed peptide
            
            // Find 0-based index beginning and end for a given peptide string in an associated protein string
            // "IndexOf" returns -1 if the string/char cannot be found, 
            //WHAT DO WE DO? with SNAFU?
            int indexInProtein = ProteinFullString.IndexOf(peptideString, System.StringComparison.Ordinal);
            peptideInstance.IndexStart = indexInProtein;
            if (indexInProtein != -1)
            {
                peptideInstance.FoundInProtein = true;
                peptideInstance.IndexStop = (peptideInstance.IndexStart + peptideString.Length - 1);
            }
            else
            {
                peptideInstance.FoundInProtein = false;
                peptideInstance.IndexStop = -1;
            }
            PeptideList.Add(peptideInstance);
        } //end AddPeptide


        /*
         * This method for directly adding peptides is used in the unit tests.  Maybe you'll find a use for it too
         */
        public void DirectAddPeptide(Peptide pep)
        {
            PeptideList.Add(pep);
            
        }

    }
}
