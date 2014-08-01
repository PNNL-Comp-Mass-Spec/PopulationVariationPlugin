using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace PvmrmExecutor
{
	public class Options
	{
		#region Command Line Arguements

		[Option('f', "FastaPath", Required = false, HelpText = "Fasta file path")]
		public string FastaPath { get; set; }

		[Option('d', "DatabasePath", Required = false, HelpText = "DB Snip Database path")]
		public string DatabasePath { get; set; }

		[Option('o', "OutfilePath", Required = false, HelpText = "Outfile Path")]
		public string OutfilePath { get; set; }

		[Option('i', "InputReportPath", Required = true, HelpText = "Input Report Path")]
		public string InputReportPath { get; set; }

		#endregion
	}
}
