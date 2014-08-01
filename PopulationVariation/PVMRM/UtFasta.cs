using System;
using NUnit.Framework;

namespace PVMRM
{
    [TestFixture]
    public class UtFasta
    {
        [Test]
        public void findFirstAccession()
        {
            string filePath = @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\shortFasta.txt";
            string proteinAccession = "NP_001005484";
            Fasta testFasta = new Fasta(filePath);
            Protein fastaResults = testFasta.ProcessFasta(proteinAccession);
            string expectedFullString = "MVTEFIFLGLSDSQELQTFLFMLFFVFYGGIVFGNLLIVITVVSDSHLHSPMYFLLANLSLIDLSLSSVTAPKMITDFFSQRKVISFKGCLVQIFLLHFFGGSEMVILIAMGFDRYIAICKPLHYTTIMCGNACVGIMAVTWGIGFLHSVSQLAFAVHLLFCGPNEVDSFYCDLPRVIKLACTDTYRLDIMVIANSGVLTVCSFVLLIISYTIILMTIQHRPLDKSSKALSTLTAHITVVLLFFGPCVFIYAWPFPIKSLDKFLAVFYSVITPLLNPIIYTLRNKDMKTAIRQLRKWDAHSSVKF";
            string expectedPartialText = "AHSSVKF";
            string expectedName = "olfactory receptor 4F5 [Homo sapiens]";
            string expectedSource = "ref";

            StringAssert.Contains(expectedPartialText, fastaResults.ProteinFullString);
            Assert.True(expectedFullString.Equals(fastaResults.ProteinFullString, StringComparison.Ordinal));
            Assert.True(expectedName.Equals(fastaResults.ProteinName, StringComparison.Ordinal));
            Assert.True(expectedSource.Equals(fastaResults.RefSource, StringComparison.Ordinal));
            Assert.True(fastaResults.FoundInFasta);
        }

        [Test]
        public void findBigProteinString()
        {
			String filePath = @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\shortFasta.txt";
            string proteinAccession = "NP_940978";
            Fasta testFasta = new Fasta(filePath);
            string expectedFullText =
                "MAGRSHPGPLRPLLPLLVVAACVLPGAGGTCPERALERREEEANVVLTGTVEEILNVDPVQHTYSCKVRVWRYLKGKDLVARESLLDGGNKVVISGFGDPLICDNQVSTGDTRIFFVNPAPPYLWPAHKNELMLNSSLMRITLRNLEEVEFCVEDKPGTHFTPVPPTPPDACRGMLCGFGAVCEPNAEGPGRASCVCKKSPCPSVVAPVCGSDASTYSNECELQRAQCSQQRRIRLLSRGPCGSRDPCSNVTCSFGSTCARSADGLTASCLCPATCRGAPEGTVCGSDGADYPGECQLLRRACARQENVFKKFDGPCDPCQGALPDPSRSCRVNPRTRRPEMLLRPESCPARQAPVCGDDGVTYENDCVMGRSGAARGLLLQKVRSGQCQGRDQCPEPCRFNAVCLSRRGRPRCSCDRVTCDGAYRPVCAQDGRTYDSDCWRQQAECRQQRAIPSKHQGPCDQAPSPCLGVQCAFGATCAVKNGQAACECLQACSSLYDPVCGSDGVTYGSACELEATACTLGREIQVARKGPCDRCGQCRFGALCEAETGRCVCPSECVALAQPVCGSDGHTYPSECMLHVHACTHQISLHVASAGPCETCGDAVCAFGAVCSAGQCVCPRCEHPPPGPVCGSDGVTYGSACELREAACLQQTQIEEARAGPCEQAECGSGGSGSGEDGDCEQELCRQRGGIWDEDSEDGPCVCDFSCQSVPGSPVCGSDGVTYSTECELKKARCESQRGLYVAAQGACRGPTFAPLPPVAPLHCAQTPYGCCQDNITAARGVGLAGCPSACQCNPHGSYGGTCDPATGQCSCRPGVGGLRCDRCEPGFWNFRGIVTDGRSGCTPCSCDPQGAVRDDCEQMTGLCSCKPGVAGPKCGQCPDGRALGPAGCEADASAPATCAEMRCEFGARCVEESGSAHCVCPMLTCPEANATKVCGSDGVTYGNECQLKTIACRQGLQISIQSLGPCQEAVAPSTHPTSASVTVTTPGLLLSQALPAPPGALPLAPSSTAHSQTTPPPSSRPRTTASVPRTTVWPVLTVPPTAPSPAPSLVASAFGESGSTDGSSDEELSGDQEASGGGSGGLEPLEGSSVATPGPPVERASCYNSALGCCSDGKTPSLDAEGSNCPATKVFQGVLELEGVEGQELFYTPEMADPKSELFGETARSIESTLDDLFRNSDVKKDFRSVRLRDLGPGKSVRAIVDVHFDPTTAFRAPDVARALLRQIQVSRRRSLGVRRPLQEHVRFMDFDWFPAFITGATSGAIAAGATARATTASRLPSSAVTPRAPHPSHTSQPVAKTTAAPTTRRPPTTAPSRVPGRRPPAPQQPPKPCDSQPCFHGGTCQDWALGGGFTCSCPAGRGGAVCEKVLGAPVPAFEGRSFLAFPTLRAYHTLRLALEFRALEPQGLLLYNGNARGKDFLALALLDGRVQLRFDTGSGPAVLTSAVPVEPGQWHRLELSRHWRRGTLSVDGETPVLGESPSGTDGLNLDTDLFVGGVPEDQAAVALERTFVGAGLRGCIRLLDVNNQRLELGIGPGAATRGSGVGECGDHPCLPNPCHGGAPCQNLEAGRFHCQCPPGRVGPTCADEKSPCQPNPCHGAAPCRVLPEGGAQCECPLGREGTFCQTASGQDGSGPFLADFNGFSHLELRGLHTFARDLGEKMALEVVFLARGPSGLLLYNGQKTDGKGDFVSLALRDRRLEFRYDLGKGAAVIRSREPVTLGAWTRVSLERNGRKGALRVGDGPRVLGESPVPHTVLNLKEPLYVGGAPDFSKLARAAAVSSGFDGAIQLVSLGGRQLLTPEHVLRQVDVTSFAGHPCTRASGHPCLNGASCVPREAAYVCLCPGGFSGPHCEKGLVEKSAGDVDTLAFDGRTFVEYLNAVTESEKALQSNHFELSLRTEATQGLVLWSGKATERADYVALAIVDGHLQLSYNLGSQPVVLRSTVPVNTNRWLRVVAHREQREGSLQVGNEAPVTGSSPLGATQLDTDGALWLGGLPELPVGPALPKAYGTGFVGCLRDVVVGRHPLHLLEDAVTKPELRPCPTP";
            string expectedStartText = "MAGRSHPGPLRP";
            string expectedInnerText = "ACLQQTQIEEARAGPCE";
            string expectedEndText = "LRPCPTP";

            Protein fastaResults = testFasta.ProcessFasta(proteinAccession);

            StringAssert.StartsWith(expectedStartText, fastaResults.ProteinFullString);
            StringAssert.Contains(expectedInnerText, fastaResults.ProteinFullString);
            StringAssert.EndsWith(expectedEndText, fastaResults.ProteinFullString);
            Assert.True(expectedFullText.Equals(fastaResults.ProteinFullString));
        }

        [Test]
        // Find an accession somewhere in the middle of the file
        public void findMiddleAccession()
        {
			String filePath = @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\shortFasta.txt";
            Fasta testFasta = new Fasta(filePath);
            string proteinAccession = "NP_066993";
            string expectedEndText = "PWRPWLR";
            string expectedProteinName = "transcription factor HES-4 isoform 2 [Homo sapiens]";

            Protein fastaResults = testFasta.ProcessFasta(proteinAccession);
            string actualFullSequence = fastaResults.ProteinFullString;

            // The following will Assert that the expected and actual protein names match, case is not ignored (StringAssert.IsMatch is for regex; will not work here.)
            Assert.True(fastaResults.ProteinName.Equals(expectedProteinName, StringComparison.Ordinal));
            Assert.True(fastaResults.ProteinAccession.Equals(proteinAccession, StringComparison.Ordinal));
            StringAssert.EndsWith(expectedEndText, actualFullSequence);
        }

        [Test]
        public void findFakeAccession()
        {
			String filePath = @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\shortFasta.txt";
            Fasta testFasta = new Fasta(filePath);
            string proteinAccession = "NP_067993";
            bool expectedBool = false;
            Protein fastaResults = testFasta.ProcessFasta(proteinAccession);

            Assert.AreEqual(expectedBool, fastaResults.FoundInFasta);
            Assert.True(fastaResults.ProteinAccession.Equals(proteinAccession, StringComparison.Ordinal));
        }

        [Test]
        public void findAccessionInBigFile()
        {
			String filePath = @"\\protoapps\UserData\RodriguezL\PVMRMhelperDocs\Protein.fa";
            Fasta testFasta = new Fasta(filePath);
            string testAccession = "NP_001165881";
            string expectedEndText = "VGALLMGLHMSQKHTEMVLEMSIG";
            Protein fastaResults = testFasta.ProcessFasta(testAccession);

            StringAssert.Contains(expectedEndText, fastaResults.ProteinFullString);
        }
    }
}

