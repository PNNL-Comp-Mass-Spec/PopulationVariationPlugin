using System;
using System.Data.SqlClient;
using NUnit.Framework;

namespace PVMRM
{
    [TestFixture]
    public class UtSnp
    {
        [Test]
        public void ModifyPeptideString()
        {
            string testPeptide = "VGALLMGLHMSQKHTEMVLEMSIGA"; //changing A in the thrid position to T
            int testAAPos = 52;
            int testIndexStart = 50;
            string testResidue = "T";
            string expectedNewPeptideString = "VGTLLMGLHMSQKHTEMVLEMSIGA";

            Snp testSnp = new Snp();
            testSnp.SetModifiedPeptideStringAndCodex(testPeptide, testAAPos, testIndexStart, testResidue);

            Assert.True(expectedNewPeptideString.Equals(testSnp.ModifiedPeptideString, StringComparison.Ordinal));
        } // end testCombo1

        [Test]
        public void returnSingleResidueModifiedPeptideString()
        {
           //this is designed to
            string origPeptide = "PCAVVLAQYLWFH";
            int aa_pos = 10;
            int indexStart = 4;
            string residue = "Q";
            string expectedModified = "PCAVVLQQYLWFH";

            Snp samplePeptide = new Snp();

            samplePeptide.SetModifiedPeptideStringAndCodex(origPeptide, aa_pos, indexStart, residue);
            Assert.True(samplePeptide.ModifiedPeptideString.Equals(expectedModified, StringComparison.Ordinal));
        }

        [Test]
        public void returnMultiResidueModifiedPeptideString()
        {   // NOTE: This code must include the reference residue to be accurate.
            string origPeptide = "PCAVVLAQYLWFH";
            int aa_pos = 10;
            int indexStart = 4;
            string residue = "QQQ";
            string expectedModified = "PCAVVLQQQLWFH";

            Snp samplePeptide = new Snp();

            samplePeptide.SetModifiedPeptideStringAndCodex(origPeptide, aa_pos, indexStart, residue);
            StringAssert.IsMatch(expectedModified, samplePeptide.ModifiedPeptideString);
        }

        [Test]
        public void returnInvalidSingleResidueModifiedPeptideString()
        {
            string origPeptide = "PCAVVLAQYLWFH";
            int aa_pos = 10;
            int indexStart = -1;
            string residue = "Q";

            Snp samplePeptide = new Snp();

            samplePeptide.SetModifiedPeptideStringAndCodex(origPeptide, aa_pos, indexStart, residue);
            StringAssert.IsMatch("", samplePeptide.ModifiedPeptideString);
        }

        [Test]
        public void returnCodexForValidPeptide()
        {
            string origPeptide = "PCAVVLAQYLWFH";
            int aa_pos = 10;
            int indexStart = 4;
            string residue = "Q";
            string expectedCodex = "A11Q";

            Snp samplePeptide = new Snp();

            samplePeptide.SetModifiedPeptideStringAndCodex(origPeptide, aa_pos, indexStart, residue);
            Assert.True(samplePeptide.Codex.Equals(expectedCodex, StringComparison.Ordinal));
        }

        [Test]
        public void returnCodexForInvalidPeptide()
        {
            string origPeptide = "PCAVVLAQYLWFH";
            int aa_pos = 10;
            int indexStart = -1; //Invalid peptides would return with this flag.
            string residue = "Q";
            string expectedCodex = "";

            Snp samplePeptide = new Snp();

            samplePeptide.SetModifiedPeptideStringAndCodex(origPeptide, aa_pos, indexStart, residue);
            Assert.True(samplePeptide.Codex.Equals(expectedCodex, StringComparison.Ordinal));
        }

        [Test]
        public void returnCodexForValidPeptideList()
        {
            //the purpose of this one is to make a protein that has multiple peptides and ensure that 
            //our query gets all of them

            //1. hard create a protein and push in the peptides 
            string accession = "NP_001165881";
            Peptide pep1 = new Peptide("VGALLMGLHMSQKHTEMVLEMSIGA");
            pep1.IndexStart = 50;
            pep1.IndexStop = 74;

            Peptide pep2 = new Peptide("GLVVYDYQQLLIAYKPAPG");
            pep2.IndexStart = 99;
            pep2.IndexStop = 117;
            Protein prot = new Protein(accession);
            prot.DirectAddPeptide(pep1);
            prot.DirectAddPeptide(pep2);
            
            //2 test the FindAllSnps method to make sure that each peptide gets the right SNP object

            string m_Server = "Daffy";
            string m_Database = "dbSNP";
            string connectionString = "Data Source=" + m_Server + ";Initial Catalog=" + m_Database +
                                      ";Integrated Security=SSPI;";

            QueryDbSnp testQuery = new QueryDbSnp();
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                testQuery.FindAllSnps(cn, prot);
                    //does the query and attaches results to the peptides within the protein
            }

            //populate the codex and modified string variables
            foreach (Peptide pep in prot.PeptideList)
            {
                foreach (Snp s in pep.Snps)
                {
                    s.SetModifiedPeptideStringAndCodex(pep.PeptideString, s.aa_position, pep.IndexStart, s.NewResidue);
                }
            }

            //3. check the list of snps in each peptide for proper 1-based codex variables
                //first peptide
            string knownCodex0ForPep1 = "A53T";
            string knownCodex1ForPep1 = "H59R";
            string knownCodex2ForPep1 = "E66K";
            string knownCodex3ForPep1 = "I73T";

                //second peptide
            string knownCodex0ForPep2 = "A116D";
            
                //first peptide asserts
            Assert.True(pep1.Snps[0].Codex.Equals(knownCodex0ForPep1));
            Assert.True(pep1.Snps[1].Codex.Equals(knownCodex1ForPep1));
            Assert.True(pep1.Snps[2].Codex.Equals(knownCodex2ForPep1));
            Assert.True(pep1.Snps[3].Codex.Equals(knownCodex3ForPep1));

                //second peptide asserts
            Assert.True(pep2.Snps[0].Codex.Equals(knownCodex0ForPep2));
        }

    }
}
