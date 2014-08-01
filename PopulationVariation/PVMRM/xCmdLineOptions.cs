using System.Collections.Generic;
using CommandLine; // for command line parsing, from http://commandline.codeplex.com

namespace PVMRM
{
    /// <summary>
    /// Commandline input options (if using commandline interface)
    /// </summary>
    public class CmdLineOptions
    {
        [Option('f', "FilePath", Required = true, HelpText = "Fasta input file to read.")]
        public string FastaFilePath { get; set; }

        [Option('p', "ProteinAccession", Required = true, HelpText = "Single protein accession number beginning with NP_")]
        public string ProteinAccession { get; set; }

        [Option('n', "ProteinName", Required = false, HelpText = "Long protein name")]
        public string ProteinName { get; set; }

        [OptionList('l', "PeptideList", Required = true, HelpText = "List of peptide strings associated with protein accession number, peptides separated by a ':'")]
        public List<string> PeptideList { get; set; }
    }
}
