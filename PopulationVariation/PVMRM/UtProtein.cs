using System;
using NUnit.Framework;

namespace PVMRM
{
    [TestFixture]
    class UtProtein
    {
        [Test]
        public void AssertAllElementsOfOneProtein()
        {
            string proteinFullString = "MYVWPCAVVLAQYLWFHRRSLPGKAILEIGAGVSLPGILAAKCGAEVILSDSSELPHCLEVCRQSCQMNNLPHLQVVGLTWGHISWDLLALPPQDIILASDVFFEPEDFEDILATIYFLMHKNPKVQLWSTYQVRSADWSLEALLYKWDMKCVHIPLESFDADKEDIAESTLPGRHTVEMLVISFAKDSL";
           
            Protein testProtein = new Protein("NP_056019");
            testProtein.ProteinFullString = proteinFullString;
            testProtein.AddPeptide("PCAVVLAQYLFH");
            testProtein.FoundInFasta = true;
            testProtein.RefSource = "ref";
            testProtein.ProteinName = "thrombospondin type-1 domain-containing protein 7A precursor [Homo sapiens]";

            string expectedAccession = "NP_056019";
            string expectedName = "thrombospondin type-1 domain-containing protein 7A precursor [Homo sapiens]";
            string expectedPeptideString = "PCAVVLAQYLFH";
            string expectedSource = "ref";

            Assert.True(testProtein.PeptideList[0].PeptideString.Equals(expectedPeptideString, StringComparison.Ordinal));
            Assert.True(testProtein.ProteinAccession.Equals(expectedAccession, StringComparison.Ordinal));
            Assert.True(testProtein.ProteinFullString.Equals(proteinFullString, StringComparison.Ordinal));
            
            //Hard-coded, so these are only showing due diligence (true test comes from Fasta method):
            Assert.True(testProtein.ProteinName.Equals(expectedName, StringComparison.Ordinal));
            Assert.True(testProtein.RefSource.Equals(expectedSource, StringComparison.Ordinal));
            Assert.True(testProtein.FoundInFasta);
        }

        [Test]
        public void AddInvalidPeptide()
        {
            //expect that the peptide will not be found in the protein
            string proteinFullString = "MYVWPCAVVLAQYLWFHRRSLPGKAILEIGAGVSLPGILAAKCGAEVILSDSSELPHCLEVCRQSCQMNNLPHLQVVGLTWGHISWDLLALPPQDIILASDVFFEPEDFEDILATIYFLMHKNPKVQLWSTYQVRSADWSLEALLYKWDMKCVHIPLESFDADKEDIAESTLPGRHTVEMLVISFAKDSL";
           
            Protein testProtein = new Protein("NP_056019");
            testProtein.ProteinFullString = proteinFullString;
            testProtein.AddPeptide("PCAVVLAQYLFH"); //Removed a W from the actual string snippet.
            
            int expectedInvalidIndexStart = -1; //this peptide is not found within the protein.
            
            Assert.AreEqual(expectedInvalidIndexStart, testProtein.PeptideList[0].IndexStart);
        }

        [Test]
        public void ReturnPeptideIndexStartForValidPeptide()
        {
            string proteinFullString = "MDVGSKEVLMESPPDYSAAPRGRFGIPCCPVHLKRLLIVVVVVVLIVVVIVGALLMGLHMSQKHTEMVLEMSIGAPEAQQRLALSEHLVTTATFSIGSTGLVVYDYQQLLIAYKPAPGTCCYIMKIAPESIPSLEALTRKVHNFQMECSLQAKPAVPTSKLGQAEGRDAGSAPSGGDPAFLGMAVSTLCGEVPLYYI";
            
            Protein testProtein = new Protein("NP_003009");
            testProtein.ProteinFullString = proteinFullString;
            testProtein.AddPeptide("AVPTSKLGQAEGRDAGSAPSGGDPAFLG");
            
            int expectedIndexStart = 154;

            Assert.AreEqual(expectedIndexStart, testProtein.PeptideList[0].IndexStart);
        }

        [Test]
        public void ReturnPeptideIndexStopForValidPeptide()
        {
            string proteinFullString = "MYVWPCAVVLAQYLWFHRRSLPGKAILEIGAGVSLPGILAAKCGAEVILSDSSELPHCLEVCRQSCQMNNLPHLQVVGLTWGHISWDLLALPPQDIILASDVFFEPEDFEDILATIYFLMHKNPKVQLWSTYQVRSADWSLEALLYKWDMKCVHIPLESFDADKEDIAESTLPGRHTVEMLVISFAKDSL";

            Protein testProtein = new Protein("NP_056019");
            testProtein.ProteinFullString = proteinFullString;
            testProtein.AddPeptide("PCAVVLAQYLWFH");
            
            int expectedIndexStop = 16;

            Assert.AreEqual(expectedIndexStop, testProtein.PeptideList[0].IndexStop);
        }

        [Test]
        public void ReturnPeptideIndexStopForTwoValidPeptides()
        {
            string proteinFullString = "MDVGSKEVLMESPPDYSAAPRGRFGIPCCPVHLKRLLIVVVVVVLIVVVIVGALLMGLHMSQKHTEMVLEMSIGAPEAQQRLALSEHLVTTATFSIGSTGLVVYDYQQLLIAYKPAPGTCCYIMKIAPESIPSLEALTRKVHNFQMECSLQAKPAVPTSKLGQAEGRDAGSAPSGGDPAFLGMAVSTLCGEVPLYYI";
            Protein testProtein = new Protein("NP_003009");
            testProtein.ProteinFullString = proteinFullString;
            testProtein.AddPeptide("AVPTSKLGQAEGRDAGSAPSGGDPAFLG");
            testProtein.AddPeptide("MDVGSKEVLMESPPDYSAAPRGRFGIPCCP");
            
            int expectedIndexStopOne = 181;
            int expectedIndexStopTwo = 29;

            Assert.AreEqual(expectedIndexStopOne, testProtein.PeptideList[0].IndexStop);
            Assert.AreEqual(expectedIndexStopTwo, testProtein.PeptideList[1].IndexStop);
        }
    }
}
