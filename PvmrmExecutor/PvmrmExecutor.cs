using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using CommandLine.Text;
using PVMRM;
using PvmrmViewer.Views;

namespace PvmrmExecutor
{
	public class PvmrmExecutor
	{

		#region Properties

		public static Options Options { get; set; }
		public static Dictionary<string, List<string>> AccessionDictionary { get; set; }
		public static PvmrmView Viewer { get; set; }

		#endregion

		#region Constructor

		static PvmrmExecutor()
		{
			Options = new Options();
			AccessionDictionary = new Dictionary<string, List<string>>();
			Options.FastaPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\\DataFiles\protein.fa";
			Options.DatabasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\\DataFiles\dbSNP01MAF.db";
			Options.OutfilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) +@"\\temp.txt";
		}

		#endregion

		[STAThread]
		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments(args, Options);

			Console.WriteLine(@"Importing Data From Skyline...");
			try
			{
				Import();
			}
			catch (Exception ex)
			{
			}

			Console.WriteLine(@"Querying Database...");

			Pvmrm.Execute(Options.FastaPath, Options.DatabasePath, Options.OutfilePath, AccessionDictionary);

			Console.WriteLine(@"Processing Complete...");

			Application app = new Application();
			app.Run(new PvmrmView(Options.OutfilePath));
			
		}

		public static void Import()
		{
			const int indexOfSharedIdAccessionInProteinName = 1;
			const int indexOfRefSeqAccessionInProteinName = 3;

			using (StreamReader input = new StreamReader(Options.InputReportPath))
			{
				int proteinHeaderIndex = 0, peptideHeaderIndex = 0;

				string header = input.ReadLine();
				if (header != null)
				{
					string[] splitHeader = header.Split(',');
					int i = 0;
					foreach (var column in splitHeader)
					{
						if (column.Contains("ProteinName"))
						{
							proteinHeaderIndex = i;
						}
						else if (column.Contains("PeptideSequence"))
						{
							peptideHeaderIndex = i;
						}
						i++;
					}
				}

				while (input.Peek() > -1)
				{
					string entry = input.ReadLine();
					if (entry != null)
					{
						string[] entrySplit = entry.Split(',');
						string[] accession = entrySplit[proteinHeaderIndex].Split('|');
						List<string> refSeqList = new List<string>();

						if ((accession[0] == "sp") || (accession[0] == "tr"))
						{
							refSeqList = ConvertToRefSeq(accession[indexOfSharedIdAccessionInProteinName]);
						}
						else if (accession[0] == "gi")
						{
							refSeqList.Add(accession[indexOfRefSeqAccessionInProteinName]);
						}
						else
						{
							refSeqList = ConvertToRefSeq(accession[0]);
							if (refSeqList.Count == 0)
							{
								refSeqList.Add(accession[0]);
							}
						}

						foreach (string id in refSeqList)
						{
							string[] noDotAcession = id.Split('.');
							if (!AccessionDictionary.ContainsKey(noDotAcession[0]))
							{
								AccessionDictionary.Add(noDotAcession[0], new List<string>());
							}
							AccessionDictionary[noDotAcession[0]].Add(entrySplit[peptideHeaderIndex]);
						}
						
					}
				}
			}
		}


		public static List<string> ConvertToRefSeq(string sharedId)
		{
			// Looks up SNP information for the given Accession
			List<string> results = new List<string>();
			StringBuilder sqlStr = new StringBuilder();

			sqlStr.Append("SELECT RefSeqId FROM AccessionMap ");
			sqlStr.Append("WHERE SharedId = '" + sharedId + "';");

			DataTable dt = null;

			SQLiteConnection connection = new SQLiteConnection("Data Source=" + Options.DatabasePath);

			//Get a table to hold the results of the query
			using (SQLiteDataAdapter Da = new SQLiteDataAdapter(sqlStr.ToString(), connection))
			{
				using (DataSet Ds = new DataSet())
				{
					Da.Fill(Ds);
					dt = Ds.Tables[0];
				}
			}

			//Verify at least one row returned
			if (dt.Rows.Count < 1)
			{
				// No data was returned
				dt.Dispose();
				return results;
			}

			foreach (DataRow curRow in dt.Rows)
			{
				string accession = DbCStr(curRow["RefSeqId"]);
				results.Add(accession);
			}

			dt.Dispose();

			return results;
		}

		// Converts a database field value to a string, checking for null values
		public static string DbCStr(object inpObj)
		{
			//If input object is DbNull, returns "", otherwise returns String representation of object
			if (object.ReferenceEquals(inpObj, DBNull.Value))
			{
				return string.Empty;
			}
			else
			{
				return Convert.ToString(inpObj);
			}
		}
	}
}
