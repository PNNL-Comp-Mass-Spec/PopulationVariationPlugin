using System.IO;

namespace PVMRM
{
    /// <summary>
    /// ProcessFasta
    /// Purpose: Creates a new Protein object. Populates RefSource, ProteinName, 
    ///     ProteinFullString and FoundInFasta from Fasta file for given accession
    /// </summary>
    public class Fasta
    {
        
        private string FastaFilePath { get; set; }
        
        public Fasta(string filePath)
        {
            this.FastaFilePath = filePath;
        }
        
        //populates RefSource, ProteinName, ProteinFullString, FoundInFasta
        public Protein ProcessFasta(string proteinAcc)
        {
            Protein fromFasta = new Protein(proteinAcc);
            string line = "";
            string sourceFilePath = FastaFilePath;
           
            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
				while ((line = reader.ReadLine()) != null)
				{
					if (line[0] == '>')
					{
						if (line.Contains(proteinAcc))
						{
							fromFasta.FoundInFasta = true;
							string[] headerEntities = line.Split('|');
							fromFasta.RefSource = headerEntities[2];
								// to ensure that it's RefSeq, or 'ref', as opposed to SwissProt, or 'sp'
							fromFasta.ProteinName = headerEntities[4].Trim();
								// if we get the name here, we avoid a table join later in the SQL query.

							// add current line into ProteinFullString
							fromFasta.ProteinFullString = reader.ReadLine();
							line = reader.ReadLine(); // inside the "if" loop we pop off more lines from the file.
							while (line[0] != '>')
							{
								fromFasta.ProteinFullString += line;
								line = reader.ReadLine();
							}

							break;
						}
					}
				}
            }
           
            return fromFasta;
        }

        //populates RefSource, ProteinName, ProteinFullString, FoundInFasta
        public string ProcessFastaProteinName(string proteinName)
        {
            string protAccession = "";
            string sourceFilePath = FastaFilePath;

            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
                string line = "";
				while ((line = reader.ReadLine()) != null)
				{
					if (line[0] == '>')
					{
						if (line.Contains(proteinName))
						{
							string[] headerEntities = line.Split('|');
							protAccession = headerEntities[3];
							break;
						}
					}
				}
            }
            
            return protAccession;
        }
    }
}
