using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace PVMRM
{
    /// <summary>
    /// The sole purpose of this class is to query the trimmed database I created from dbSNP's data.
    /// </summary>
    public class QueryDbSnpLite
    {
        /// <summary>
        /// FindAllSnps: Wrapper for the MissenseQueryDbSNPLite
        /// foreach will loop through all listed peptides in the protein,
        /// running them one at a time through the query.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="protein"></param>
        public void FindAllSnpsLite(SQLiteConnection cn, Protein protein)
        {
            foreach (Peptide peptide in protein.PeptideList)
            {
                peptide.Snps = MissenseQueryDbSNPLite(cn, protein.ProteinAccession, peptide.IndexStart, peptide.IndexStop);
                    //Expression to set the bool of FoundVariantInQuery to true if Snps list length is not 0.
                peptide.FoundVariantInQuery = peptide.Snps.Count >= 1;

            	foreach (var snp in peptide.Snps)
            	{
					snp.SetModifiedPeptideStringAndCodex(peptide.PeptideString, snp.aa_position, peptide.IndexStart, snp.NewResidue);
            	}
            }

            //now do a call to a new method that looks for variants that affect the whole protein sequence
            protein.ProteinLevelSnps = ProteinLevelQueryDbSNPLite(cn, protein.ProteinAccession); //does query and attaches to the protein
            foreach (Snp s in protein.ProteinLevelSnps)
            {
                    //expression to change ModifiedPeptideString to "Stop-Gain" if NewResidue == "*", otherwise it's a "Frameshift".
                s.ModifiedPeptideString = s.NewResidue == "*" ? "Stop-Gain" : "Frameshift";
            }
                //expression to set the bool of FoundProteinLevelChage to true if Snps list length is not 0.
            protein.FoundProteinLevelChange = protein.ProteinLevelSnps.Count >= 1;
        }

        /// <summary>
        ///  MissenseQueryDbSNP: queries dbSNP database, finds all non-synonymous SNPs for each given peptide.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="protAcc"></param>
        /// <param name="peptideStart"></param>
        /// <param name="peptideStop"></param>
        /// <returns> List of Snps, populates the Snp list in the Peptide class. </returns>
        public List<Snp> MissenseQueryDbSNPLite(SQLiteConnection cn, string protAcc, int peptideStart, int peptideStop)
        {
			    // Looks up SNP information for the given Accession
                List<Snp> results = new List<Snp>();

			    StringBuilder sqlStr = new StringBuilder();

                sqlStr.Append(" SELECT distinct locus.residue, locus.aa_pos, locus.snp_id, locus.minorAlleleFreq");
                sqlStr.Append(" from SNPwithMAF AS locus");
		        sqlStr.Append(" WHERE locus.prot_acc = '" + protAcc + "'");
			    sqlStr.Append(" AND locus.aa_pos BETWEEN " + peptideStart + " AND " + peptideStop);
                sqlStr.Append(" AND locus.fxn_code = 42 ");
			    sqlStr.Append(" ORDER BY locus.aa_pos;");

			    DataTable dt = null;

			    //Get a table to hold the results of the query

                using (SQLiteDataAdapter Da = new SQLiteDataAdapter(sqlStr.ToString(), cn))
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
			    else
			    {
				    foreach (DataRow curRow in dt.Rows)
				    {
                        Snp result = new Snp();

					    result.NewResidue = DbCStr(curRow["residue"]);	
                        result.aa_position = DbCInt(curRow["aa_pos"]);
                        result.MinorAlleleFrequency = DbCDouble(curRow["minorAlleleFreq"]);
					    result.SnpID = DbCInt(curRow["snp_id"]);

					    if (result.MinorAlleleFrequency != -1)
					    {
						    results.Add(result);
					    }
				    }

				    dt.Dispose();

			    }

			    return results;
		   }//End Missense query


        /// <summary>
        /// ProteinLevel query: query dbSNP database for protein-level variants for a given accession
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="protAcc"></param>
        /// <returns>List of Snps that populates the Snp list in the Protein class (not the Peptide class for this one).</returns>
        public List<Snp> ProteinLevelQueryDbSNPLite(SQLiteConnection cn, string protAcc)
        {
            // Looks up SNP information for the given Accession
            List<Snp> results = new List<Snp>();

            StringBuilder sqlStr = new StringBuilder();

            sqlStr.Append(" SELECT distinct locus.residue, locus.aa_pos, locus.snp_id, locus.minorAlleleFreq");
            sqlStr.Append(" from SNPwithMAF AS locus");
            sqlStr.Append(" WHERE locus.prot_acc = '" + protAcc + "'");
            sqlStr.Append(" AND fxn_code in (41, 44) ");
            sqlStr.Append(" ORDER BY locus.aa_pos;");

            System.Data.DataTable dt = null;

            //Get a table to hold the results of the query

            using (SQLiteDataAdapter Da = new SQLiteDataAdapter(sqlStr.ToString(), cn))
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
            else
            {
                foreach (DataRow curRow in dt.Rows)
                {
                    Snp result = new Snp();

                    result.NewResidue = DbCStr(curRow["residue"]);
                    result.aa_position = DbCInt(curRow["aa_pos"]);
                    result.MinorAlleleFrequency = DbCDouble(curRow["minorAlleleFreq"]);
					result.SnpID = DbCInt(curRow["snp_id"]);

					if (result.MinorAlleleFrequency != -1)
					{
						results.Add(result);
					}
                }

                dt.Dispose();

            }

            return results;
        } //End ProteinLevel query
        
        // Converts a database field value to a string, checking for null values
        public string DbCStr(object inpObj)
        {
            //If input object is DbNull, returns "", otherwise returns String representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
            {
                return string.Empty;
            }
            else
            {
                return Convert.ToString(inpObj);
            }
        }
        
        // Converts a database field value to an integer (int32), checking for null values
        public int DbCInt(object inpObj)
		{
			//If input object is DbNull, returns -1, otherwise returns Integer representation of object
			if (ReferenceEquals(inpObj, DBNull.Value))
			{
				return -1;
			}
			else
			{
				return Convert.ToInt32(inpObj);
			}
		}

        // Converts a database field value to a Double, checking for null values
        public double DbCDouble(object inpObj)
        {
            //If input object is DbNull, returns -1, otherwise returns Double representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
            {
                return -1;
            }
            else
            {
                return Convert.ToDouble(inpObj);
            }
        }
    }
}
