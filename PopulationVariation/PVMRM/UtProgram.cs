using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using CommandLine;
using NUnit.Framework;

namespace PVMRM
{
    [TestFixture]
    class UtProgram
    {
        private static SqlConnection databaseConnection;
        private static QueryDbSnp testQuery;

        [Test]
        public void CallFirstThreeMethodsWithGoodVars()
        {
            //Using actual variables from real proteins and peptides:
            //0. variables from command line
            var cmdl = new CmdLineOptions();
            bool goodArgs = Parser.Default.ParseArguments(new string[] { "-f", @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\Protein.fa", "-p", "NP_001165881", "-l", "VGALLMGLHMSQKHTEMVLEMSIGA:GLVVYDYQQLLIAYKPAPG:AVPTSKLGQAEGRDAGSAPSGGDPAFLG" }, cmdl);
            //1. Set up variables and stuff for connecting to the database

            SetUpDatabaseConnection("Daffy", "dbSNP"); //These are the server name and database name for PNNL

            //2. get the peptide and protein objects ready
            Protein testProtein = SetUpData(cmdl.FastaFilePath, cmdl.ProteinAccession, cmdl.PeptideList);

            //3. now do the acutal querying
            QueryDatabase(testProtein);
            //expected protein accession
            string expectedAccession = "NP_001165881";
            //expected peptide strings
            string expectedPeptide0 = "VGALLMGLHMSQKHTEMVLEMSIGA";
            string expectedPeptide1 = "GLVVYDYQQLLIAYKPAPG";
            string expectedPeptide2 = "AVPTSKLGQAEGRDAGSAPSGGDPAFLG";
            //expected index starts
            int expectedIndexStart0 = 50;
            int expectedIndexStart1 = 99;
            int expectedIndexStart2 = 154;
            //expected index stops
            int expectedIndexStop0 = 74;
            int expectedIndexStop1 = 117;
            int expectedIndexStop2 = 181;
            //expected aa_positions
            int expectedAaPos00 = 52;
            int expectedAaPos01 = 58;
            int expectedAaPos02 = 65;
            int expectedAaPos03 = 72;
            int expectedAaPos10 = 115;
            int expectedAaPos20 = 154;
            int expectedAaPos21 = 166;
            int expectedAaPos22 = 180;
            //expected residues
            string expectedNewResidue00 = "T";
            string expectedNewResidue01 = "R";
            string expectedNewResidue02 = "K";
            string expectedNewResidue03 = "T";
            string expectedNewResidue10 = "D";
            string expectedNewResidue20 = "P";
            string expectedNewResidue21 = "Q";
            string expectedNewResidue22 = "V";
            //expected codex *********(1-based!)*******
            string expectedCodex00 = "A53T";
            string expectedCodex01 = "H59R";
            string expectedCodex02 = "E66K";
            string expectedCodex03 = "I73T";
            string expectedCodex10 = "A116D";
            string expectedCodex20 = "A155P";
            string expectedCodex21 = "R167Q";
            string expectedCodex22 = "L181V";
            //expected modified peptide strings
            string expectedModifiedPep00 = "VGTLLMGLHMSQKHTEMVLEMSIGA";
            string expectedModifiedPep01 = "VGALLMGLRMSQKHTEMVLEMSIGA";
            string expectedModifiedPep02 = "VGALLMGLHMSQKHTKMVLEMSIGA";
            string expectedModifiedPep03 = "VGALLMGLHMSQKHTEMVLEMSTGA";
            string expectedModifiedPep10 = "GLVVYDYQQLLIAYKPDPG";
            string expectedModifiedPep20 = "PVPTSKLGQAEGRDAGSAPSGGDPAFLG";
            string expectedModifiedPep21 = "AVPTSKLGQAEGQDAGSAPSGGDPAFLG";
            string expectedModifiedPep22 = "AVPTSKLGQAEGRDAGSAPSGGDPAFVG";
            //expected protein-level modified peptide strings
            string expectedProteinLevelModifiedPeptideString0 = "Stop-Gain";

            //Test that accession is as expected
            Assert.True(testProtein.ProteinAccession.Equals(expectedAccession, StringComparison.Ordinal));
            //Test that peptides listed are as expected
            Assert.True(testProtein.PeptideList[0].PeptideString.Equals(expectedPeptide0, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[1].PeptideString.Equals(expectedPeptide1, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].PeptideString.Equals(expectedPeptide2, StringComparison.Ordinal));
            //Test that index starts (where peptide appears in protein string) is as expected
            Assert.AreEqual(testProtein.PeptideList[0].IndexStart, expectedIndexStart0);
            Assert.AreEqual(testProtein.PeptideList[1].IndexStart, expectedIndexStart1);
            Assert.AreEqual(testProtein.PeptideList[2].IndexStart, expectedIndexStart2);
            //Test that index stops (where peptide ends in protein string) is as expected
            Assert.AreEqual(testProtein.PeptideList[0].IndexStop, expectedIndexStop0);
            Assert.AreEqual(testProtein.PeptideList[1].IndexStop, expectedIndexStop1);
            Assert.AreEqual(testProtein.PeptideList[2].IndexStop, expectedIndexStop2);
            //Test that all aa_positions that come back from the query are as expected
            Assert.AreEqual(testProtein.PeptideList[0].Snps[0].aa_position, expectedAaPos00);
            Assert.AreEqual(testProtein.PeptideList[0].Snps[1].aa_position, expectedAaPos01);
            Assert.AreEqual(testProtein.PeptideList[0].Snps[2].aa_position, expectedAaPos02);
            Assert.AreEqual(testProtein.PeptideList[0].Snps[3].aa_position, expectedAaPos03);
            Assert.AreEqual(testProtein.PeptideList[1].Snps[0].aa_position, expectedAaPos10);
            Assert.AreEqual(testProtein.PeptideList[2].Snps[0].aa_position, expectedAaPos20);
            Assert.AreEqual(testProtein.PeptideList[2].Snps[1].aa_position, expectedAaPos21);
            Assert.AreEqual(testProtein.PeptideList[2].Snps[2].aa_position, expectedAaPos22);
            //Test that all new residues match with expected
            Assert.True(testProtein.PeptideList[0].Snps[0].NewResidue.Equals(expectedNewResidue00, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[1].NewResidue.Equals(expectedNewResidue01, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[2].NewResidue.Equals(expectedNewResidue02, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[3].NewResidue.Equals(expectedNewResidue03, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[1].Snps[0].NewResidue.Equals(expectedNewResidue10, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[0].NewResidue.Equals(expectedNewResidue20, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[1].NewResidue.Equals(expectedNewResidue21, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[2].NewResidue.Equals(expectedNewResidue22, StringComparison.Ordinal));
            //Test that *********(1-based!)******* codex match with expected
            Assert.True(testProtein.PeptideList[0].Snps[0].Codex.Equals(expectedCodex00, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[1].Codex.Equals(expectedCodex01, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[2].Codex.Equals(expectedCodex02, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[3].Codex.Equals(expectedCodex03, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[1].Snps[0].Codex.Equals(expectedCodex10, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[0].Codex.Equals(expectedCodex20, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[1].Codex.Equals(expectedCodex21, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[2].Codex.Equals(expectedCodex22, StringComparison.Ordinal));
            //Test that modified peptides match with expected
            Assert.True(testProtein.PeptideList[0].Snps[0].ModifiedPeptideString.Equals(expectedModifiedPep00, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[1].ModifiedPeptideString.Equals(expectedModifiedPep01, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[2].ModifiedPeptideString.Equals(expectedModifiedPep02, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[0].Snps[3].ModifiedPeptideString.Equals(expectedModifiedPep03, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[1].Snps[0].ModifiedPeptideString.Equals(expectedModifiedPep10, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[0].ModifiedPeptideString.Equals(expectedModifiedPep20, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[1].ModifiedPeptideString.Equals(expectedModifiedPep21, StringComparison.Ordinal));
            Assert.True(testProtein.PeptideList[2].Snps[2].ModifiedPeptideString.Equals(expectedModifiedPep22, StringComparison.Ordinal));
            //Test that protein-level modified peptide matches with expected
            Assert.True(expectedProteinLevelModifiedPeptideString0.Equals(testProtein.ProteinLevelSnps[0].ModifiedPeptideString, StringComparison.Ordinal));

        }

        //Exception handling follows:
        [ExpectedException(typeof(IOException))]
        [Test]
        public void CallFirstThreeMethodsWithBadFastaPath()
        {
            //Error: File path is bad. Should say "UserData" not "User\Data"
            //0. variables from command line
            var cmdl = new CmdLineOptions();
            bool goodArgs = Parser.Default.ParseArguments(new string[] { "-f", @"\\protoapps\User\Data\RodriguezL\PVMRMhelperDocs\Protein.fa", "-p", "NP_001165881", "-l", "VGALLMGLHMSQKHTEMVLEMSIGA:GLVVYDYQQLLIAYKPAPG:AVPTSKLGQAEGRDAGSAPSGGDPAFLG" }, cmdl);
            //1. Set up variables and stuff for connecting to the database

            SetUpDatabaseConnection("Daffy", "dbSNP"); //These are the server name and database name for PNNL

            //2. get the peptide and protein objects ready
            Protein testProtein = SetUpData(cmdl.FastaFilePath, cmdl.ProteinAccession, cmdl.PeptideList);

            //3. now do the acutal querying
            QueryDatabase(testProtein);

            //string actualException = ex.ToString();
            //Console.WriteLine(actualException);
            //string expectedException = "Error: The network name cannot be found.";
            //Assert.True(actualException.Equals(expectedException, StringComparison.Ordinal));
        }

        [ExpectedException(typeof(SqlException))]
        [Test]
        public void CallFirstThreeMethodsWithBadServerName()
        {
            //Error: Database connection should be "Daffy", not "Duffy".
            //0. variables from command line
            var cmdl = new CmdLineOptions();
            bool goodArgs = Parser.Default.ParseArguments(new string[] { "-f", @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\Protein.fa", "-p", "NP_001165881", "-l", "VGALLMGLHMSQKHTEMVLEMSIGA:GLVVYDYQQLLIAYKPAPG:AVPTSKLGQAEGRDAGSAPSGGDPAFLG" }, cmdl);
            //1. Set up variables and stuff for connecting to the database

            SetUpDatabaseConnection("Duffy", "dbSNP"); //These are the server name and database name for PNNL

            //2. get the peptide and protein objects ready
            Protein testProtein = SetUpData(cmdl.FastaFilePath, cmdl.ProteinAccession, cmdl.PeptideList);

            //3. now do the acutal querying
            QueryDatabase(testProtein);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void CallFirstThreeMethodsWithMissingNpNumber()
        {
            //Error: protein accession number "NP_001165881" is missing from ParseArguments.
            //0. variables from command line
            var cmdl = new CmdLineOptions();
            bool goodArgs = Parser.Default.ParseArguments(new string[] { "-f", @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\Protein.fa", "-p", "-l", "VGALLMGLHMSQKHTEMVLEMSIGA:GLVVYDYQQLLIAYKPAPG:AVPTSKLGQAEGRDAGSAPSGGDPAFLG" }, cmdl);
            //1. Set up variables and stuff for connecting to the database

            SetUpDatabaseConnection("Duffy", "dbSNP"); //These are the server name and database name for PNNL

            //2. get the peptide and protein objects ready
            Protein testProtein = SetUpData(cmdl.FastaFilePath, cmdl.ProteinAccession, cmdl.PeptideList);

            //3. now do the acutal querying
            QueryDatabase(testProtein);
        }

        //Supporting methods follow (QueryDatabase and SetUpDatabaseConnection)
        public static void QueryDatabase(Protein protein)
        {
            using (databaseConnection)
            {
                testQuery.FindAllSnps(databaseConnection, protein);
            }
            //now we have SNPs get their new sequence and codex
            foreach (Peptide pep in protein.PeptideList)
            {
                foreach (Snp s in pep.Snps)
                {
                    s.SetModifiedPeptideStringAndCodex(pep.PeptideString, s.aa_position, pep.IndexStart, s.NewResidue);
                }
            }
        }

        public static void SetUpDatabaseConnection(string server, string database)
        {
            string connectionString = "Data Source=" + server + ";Initial Catalog=" + database + ";Integrated Security=SSPI;";
            testQuery = new QueryDbSnp();
            databaseConnection = new SqlConnection(connectionString);
        }

        public static Protein SetUpData(string fastaFilePath, string accession, List<string> listOfPeptides)
        {
            Fasta fastaHandle = new Fasta(fastaFilePath);

            //make blank protein objects
            Protein testProtein = fastaHandle.ProcessFasta(accession);
            //now tell the protein to make and attach peptideobjects
            foreach (string peptide in listOfPeptides)
            {
                testProtein.AddPeptide(peptide);
            }

            return testProtein;
        }

    }
}

