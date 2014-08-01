using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;

namespace PVMRM
{
    public class Pvmrm
    {
        private static SqlConnection databaseConnection;
        private static SQLiteConnection lDatabaseConnection;
        private static QueryDbSnp testQuery;
        private static QueryDbSnpLite testQueryLite;  

		/// <summary>
		/// Executes PVMRM on a single protein accession
		/// </summary>
		/// <param name="FastaFilePath"></param>
		/// <param name="SNPdbFilePath"></param>
		/// <param name="OutputFilePath"></param>
		/// <param name="proteinAccession"></param>
		/// <param name="peptides"></param>
        public static void Execute(string FastaFilePath, string SNPdbFilePath, string OutputFilePath, string proteinAccession, List<string> peptides )
        {
            try
            {
                //0. variables from command line
                //var cmdl = new CmdLineOptions();
                //bool goodArgs = CommandLine.Parser.Default.ParseArguments(args, cmdl);

                //1. Set up variables and stuff for connecting to the database
                
                //****Server
                //SetUpDatabaseConnection("Daffy", "dbSNP"); //These are the server name and database name for PNNL
                //****SQLite    
                SetUpSQLiteConnection(SNPdbFilePath);
                        
                //2. get the peptide and protein objects ready
                Protein testProtein = SetUpData(FastaFilePath, proteinAccession, peptides);
                 
                //3. now do the acutal querying
                //****Server
                //QueryDatabase(testProtein);
                //****SQLite
                QueryLiteDatabase(testProtein);
                
                WriteResultsToFile(OutputFilePath, testProtein);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadKey();
            }
        }

		/// <summary>
		/// Executes PVMRM on multiple protein accessions
		/// </summary>
		/// <param name="FastaFilePath"></param>
		/// <param name="SNPdbFilePath"></param>
		/// <param name="OutputFilePath"></param>
		/// <param name="accessionDictionary"></param>
	    public static void Execute(string FastaFilePath, string SNPdbFilePath, string OutputFilePath, Dictionary<string, List<string>> accessionDictionary)
		{
			bool faqWrite = false;
		    try
		    {
				List<Protein> proteinList = new List<Protein>();
				SetUpSQLiteConnection(SNPdbFilePath);

			    foreach (var accession in accessionDictionary)
			    {
				    try
				    {
						Protein testProtein = SetUpData(FastaFilePath, accession.Key, accession.Value);
						QueryLiteDatabase(testProtein);
						proteinList.Add(testProtein);
				    }
				    catch (Exception)
					{
						if (!faqWrite)
						{
							Console.WriteLine("Please see F.A.Q for proper accession naming.");
							faqWrite = true;
						}
					    Console.WriteLine("Unable to process accession: " + accession.Key);
				    }
			    }

				WriteResultsToFile(OutputFilePath, proteinList);

		    }
		    catch (Exception ex)
		    {
			    Console.WriteLine("Error: " + ex.Message);
                Console.ReadKey();
		    }

	    }
        
        /// <summary>
        /// Query database with known protein data (protein accession and peptide list),
        ///     fill in SNP list for each peptide, flag "FoundInQuery" as true if SNP list is returned,
        ///     after getting data back from the query, fill in codex and modifiedPeptideString.
        /// </summary>
        /// <param name="protein"></param>
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

        /// <summary>
        /// LITE Query:
        /// Query lite database with known protein data (protein accession and peptide list),
        ///     fill in SNP list for each peptide, flag "FoundInQuery" as true if SNP list is returned,
        ///     after getting data back from the query, fill in codex and modifiedPeptideString.
        /// </summary>
        /// <param name="lProtein"></param>
        public static void QueryLiteDatabase(Protein lProtein)
        {
            using (lDatabaseConnection)
            {
                testQueryLite.FindAllSnpsLite(lDatabaseConnection, lProtein);
            }
            //now we have SNPs get their new sequence and codex
            foreach (Peptide lPep in lProtein.PeptideList)
            {
                foreach (Snp ls in lPep.Snps)
                {
                    ls.SetModifiedPeptideStringAndCodex(lPep.PeptideString, ls.aa_position, lPep.IndexStart, ls.NewResidue);
                }
            }
        }

        /// <summary>
        /// Set up database connection with server and database names of local dbSNP.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        public static void SetUpDatabaseConnection(string server, string database)
        {
           string  connectionString = "Data Source=" + server + ";Initial Catalog=" + database + ";Integrated Security=SSPI;";
            testQuery = new QueryDbSnp();
            databaseConnection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// LITE db connection
        /// Set up database connection for SQLite using trimmed database dbSNPdatabase.db
        /// </summary>
        /// <param name="dbFilePath"></param>
        public static void SetUpSQLiteConnection(string dbLiteFilePath)
        {
            string lConnectionString = "Data Source=" + dbLiteFilePath;
            testQueryLite = new QueryDbSnpLite();
            lDatabaseConnection = new SQLiteConnection(lConnectionString);
        }

        /// <summary>
        /// SetUpData
        /// Purpose: this simple method takes data and makes the objects
        ///     we expect that this may be the API interface for people programmatically using the PVMRM
        /// </summary>
        /// <param name="fastaInstance"></param>
        /// <param name="accession"></param>
        /// <param name="listOfPeptides"></param>
        /// <returns>Protein, including protein accession, protein name, full protein string, list of peptides from user,
        ///     and a flag if the peptide was found in the protein full string.</returns>
        public static Protein SetUpData(string fastaFilePath, string accession, List<string> listOfPeptides)
        {
            Fasta fastaHandle = new Fasta(fastaFilePath);

            //make blank protein objects
            Protein testProtein = fastaHandle.ProcessFasta(accession);
            //now tell the protein to make and attach peptideobjects
            foreach (string peptide in listOfPeptides)
            {
	            if (testProtein.FoundInFasta)
	            {
		            testProtein.AddPeptide(peptide);
	            }
	            else
	            {
		            Console.WriteLine("Accession " + testProtein.ProteinAccession + " not found in fasta file!");
	            }
            }

            return testProtein;
        }

        /// <summary>
        /// Write results to a tab separated file using stringbuilder and streamwriter.
        /// Results where no variant was found or where the peptide was not found in the protein string
        ///     are indicated in the final output ("No variant found" appears in the Modified Peptide column,
        ///     "String not found in protein: XXX" appears in the Reference Peptide column).
        /// Stop-gains and frameshifts appear in the Modified Peptide column.
        /// </summary>
        /// <param name="proteinInstance"></param>
        public static void WriteResultsToFile(string outputFilePath, Protein proteinInstance)
        {
            string filePath = outputFilePath;
            StringBuilder sb = new StringBuilder();

            //Column headers
            sb.Append("ProteinAccession\tProteinName\tVariantCodex\tMinorAlleleFrequency\tReferencePeptide\tModifiedPeptide\tSNPID\n");
            
        	sb.Append(CreateResultsOutputString(proteinInstance));
            
            using (StreamWriter fileOut = new StreamWriter(filePath))
            {
                fileOut.Write(sb.ToString());   
            }
        }

		public static void WriteResultsToFile(string outputFilePath, IEnumerable<Protein> proteinInstance)
		{
			StringBuilder sb = new StringBuilder();

			//Column headers
			sb.Append("ProteinAccession\tProteinName\tVariantCodex\tMinorAlleleFrequency\tReferencePeptide\tModifiedPeptide\tSNPID\n");

			foreach (var protein in proteinInstance)
			{
				sb.Append(CreateResultsOutputString(protein));
			}

			using (StreamWriter fileOut = new StreamWriter(outputFilePath))
			{
				fileOut.Write(sb.ToString());
			}
		}

		private static string CreateResultsOutputString(Protein proteinInstance)
		{
			StringBuilder sb = new StringBuilder();

			if (proteinInstance.FoundProteinLevelChange == true)
			{
				foreach (Snp ps in proteinInstance.ProteinLevelSnps)
				{
					sb.Append(proteinInstance.ProteinAccession + "\t" + proteinInstance.ProteinName + "\t" + ps.Codex +
							  "\t" + ps.MinorAlleleFrequency + "\t\t" + ps.ModifiedPeptideString + "\t" + ps.SnpID + "\n");
				}
			}

			foreach (Peptide pep in proteinInstance.PeptideList)
			{
				//peptide string was found in the fasta protein string AND found a variant in the query
				if (pep.FoundInProtein == true && pep.FoundVariantInQuery == true)
				{
					foreach (Snp snp in pep.Snps)
					{
						string MAF = "";
						//Expression to write "N/A" if minor allele frequency is not given.
						MAF = snp.MinorAlleleFrequency > 0 ? snp.MinorAlleleFrequency.ToString(CultureInfo.InvariantCulture) : "N/A";
						sb.Append(proteinInstance.ProteinAccession + "\t" + proteinInstance.ProteinName + "\t" + snp.Codex +
								  "\t" + MAF + "\t" + pep.PeptideString + "\t" + snp.ModifiedPeptideString + "\t" + snp.SnpID + "\n");
					}
				}
				////peptide string was found in the fasta protein but NO variants were found in the query
				//else if (pep.FoundInProtein == true && pep.FoundVariantInQuery == false)
				//{
				//	sb.Append(proteinInstance.ProteinAccession + "\t" + proteinInstance.ProteinName + "\t\t\t" + pep.PeptideString + "\t" + "No variant found for this peptide.\n");
				//}
				////peptide string was NOT found in the fasta protein
				//else if (pep.FoundInProtein == false)
				//{
				//	sb.Append(proteinInstance.ProteinAccession + "\t" + proteinInstance.ProteinName + "\t\t\t\t" + "String not found in protein: " + pep.PeptideString + "\t\n");
				//}
			}

			return sb.ToString();
		}
    }
}
