using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using NUnit.Framework;

namespace PVMRM
{
    [TestFixture]
    class UtQueryDbSNPLite
    {
        //static string lFilePath = @"C:\Users\rodr657\Desktop\dbSNPAndFreq.db";
		static string lFilePath = @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\SQLiteDatabases\dbSNPOneTableSelectColumnsAndFxnAnyMAF.db";
        static string connectionString = "Data Source=" + lFilePath;
        
        [Test]
        public void AccessionAndIndexCombo()
        {
            //test one protein accession with a given index start and stop
            
            QueryDbSnpLite myQueryDbSnp = new QueryDbSnpLite();

            using (SQLiteConnection cn = new SQLiteConnection(connectionString))
            {
                string proteinAccession = "NP_058519";
                int indexStart = 430;
                int indexStop = 450;

                List<Snp> results = myQueryDbSnp.MissenseQueryDbSNPLite(cn, proteinAccession, indexStart, indexStop);

                List<Snp> expected = new List<Snp>();

                expected.Add(new Snp { NewResidue = "H", aa_position = 440 });
                expected.Add(new Snp { NewResidue = "P", aa_position = 446 });
				expected.Add(new Snp { NewResidue = "Q", aa_position = 447 });

                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.AreEqual(expected[i].aa_position, results[i].aa_position);
                    Assert.AreEqual(expected[i].NewResidue, results[i].NewResidue);
                }

	            foreach (var x in results)
	            {
		            Console.WriteLine(x.NewResidue);
	            }

                Assert.AreEqual(3, results.Count);
            }
        } // end testCombo

        [Test]
        public void SynonymousChangeReturned()
        {
            //Query should not return any synonymous change SNPs

            QueryDbSnpLite myQueryDbSnp = new QueryDbSnpLite();

            using (SQLiteConnection cn = new SQLiteConnection(connectionString))
            {
                // should return "No match found" because the following has only a synonymous change associated with this aa_position.
                string proteinAccession = "NP_000991";
                int indexStart = 50;
                int indexStop = 50;

                List<Snp> results = myQueryDbSnp.MissenseQueryDbSNPLite(cn, proteinAccession, indexStart, indexStop);
                Assert.AreEqual(0, results.Count);
            }
        } // end synonymousTest

        [Test]
        public void checkMinorAlleleFrequency()
        {
            // Test minor allele frequency by using one protein accession and an index start and 
            // stop with a known minor allele frequency at that location
            
            QueryDbSnpLite myQueryDbSnp = new QueryDbSnpLite();

            using (SQLiteConnection cn = new SQLiteConnection(connectionString))
            {
                string proteinAccession = "NP_002091";
                int indexStart = 37;
                int indexStop = 53;
                double expectedMAF = 0.242;

                List<Snp> results = myQueryDbSnp.MissenseQueryDbSNPLite(cn, proteinAccession, indexStart, indexStop);
                Assert.AreEqual(expectedMAF, results[0].MinorAlleleFrequency);
            }
        } // end checkMinorAlleleFrequency

        [Test]
        public void checkProteinLevelQuery()
        {
            // Test minor allele frequency by using one protein accession and an index start and 
            // stop with a known minor allele frequency at that location
            //string lFilePath = @"C:\Users\rodr657\Desktop\dbSNPAndFreq.db";
            string connectionString = "Data Source=" + lFilePath;
            
            QueryDbSnpLite myQueryDbSnp = new QueryDbSnpLite();

            using (SQLiteConnection cn = new SQLiteConnection(connectionString))
            {
                string proteinAccession = "NP_058519";
                string expectedNewResidue = "*";

                List<Snp> results = myQueryDbSnp.ProteinLevelQueryDbSNPLite(cn, proteinAccession);
                //Should be null because Stop/Gains and frameshifts should not have a codex:
                Assert.IsNull(results[0].Codex);
                Assert.True(expectedNewResidue.Equals(results[0].NewResidue));
            }
        } // end checkProteinLevelQuery

        [Test]
        public void MultiplePeptideTest()
        {
            //the purpose of this one is to make a protein that has multiple peptides and ensure that 
            //our query gets all of them
            //string lFilePath = @"C:\Users\rodr657\Desktop\dbSNPAndFreq.db";
            string connectionString = "Data Source=" + lFilePath;

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

            QueryDbSnpLite myQueryDbSnp = new QueryDbSnpLite();
            using (SQLiteConnection cn = new SQLiteConnection(connectionString))
            {
                myQueryDbSnp.FindAllSnpsLite(cn, prot); //does the query and attaches results to the peptides within the protein
            }
            //3. we check the peptides to make sure that they have SNP objects
            int knownListLengthForPeptide1 = 4; //these are all missense SNPs
            int knownListLengthForPeptide2 = 1; //there is one missense SNP for this, but there is also one Stop-Gain, which should be put onto the protein directly
            Assert.AreEqual(knownListLengthForPeptide1, pep1.Snps.Count);
            Assert.AreEqual(knownListLengthForPeptide2, pep2.Snps.Count);
        }
    }

}
