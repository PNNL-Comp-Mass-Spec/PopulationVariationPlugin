using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PVMRM
{
    public class QueryDbSnp
    {
        /// <summary>
        /// OBSOLETE
        /// FindAllSnps: Wrapper for the MissenseQueryDbSNP
        /// foreach will loop through all listed peptides in the protein,
        /// running them one at a time through the query.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="protein"></param>
        public void FindAllSnps(System.Data.SqlClient.SqlConnection cn, Protein protein)
        {
            foreach (Peptide p in protein.PeptideList)
            {
                p.Snps = MissenseQueryDbSNP(cn, protein.ProteinAccession, p.IndexStart, p.IndexStop);
                    //Expression to set the bool of FoundVariantInQuery to true if Snps list length is not 0.
                p.FoundVariantInQuery = p.Snps.Count >= 1;
            }

            //now do a call to a new method that looks for variants that affect the whole protein sequence
            protein.ProteinLevelSnps = ProteinLevelQueryDbSNP(cn, protein.ProteinAccession); //does query and attaches to the protein
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
        public List<Snp> MissenseQueryDbSNP(System.Data.SqlClient.SqlConnection cn, string protAcc, int peptideStart, int peptideStop)
        {
			    // Looks up SNP information for the given Accession
                List<Snp> results = new List<Snp>();

			    StringBuilder sqlStr = new StringBuilder();

                sqlStr.Append(" SELECT distinct locus.residue, locus.aa_position, locus.snp_id, snp.freq as MinorAlleleFrequency");
                sqlStr.Append(" from b137_SNPContigLocusIdTRIMMED AS locus");
                sqlStr.Append(" left join");
                sqlStr.Append("     (select tgp.snp_id, round(tgp.freq,3) as freq, tgp.is_minor_allele, lll.allele");
                sqlStr.Append("     from SNPAlleleFreq_TGP_TRIMMED_FreqPt01Plus AS tgp");
                sqlStr.Append("     left join Allele lll on tgp.allele_id = lll.allele_id");
                //sqlStr.Append("     where tgp.is_minor_allele = 1"); //TRIMMED_Pt1 table is already restricted to only minor alleles.
                sqlStr.Append("     ) snp");
                sqlStr.Append(" on snp.snp_id = locus.snp_id");
		        sqlStr.Append(" WHERE locus.protein_acc = '" + protAcc + "'");
			    sqlStr.Append(" AND locus.aa_position BETWEEN " + peptideStart + " AND " + peptideStop);
                sqlStr.Append(" AND fxn_class = 42 ");
			    sqlStr.Append(" ORDER BY locus.aa_position;");

			    System.Data.DataTable dt = null;

			    //Get a table to hold the results of the query

			    using (SqlDataAdapter Da = new SqlDataAdapter(sqlStr.ToString(), cn))
			    {
				    using (DataSet Ds = new System.Data.DataSet())
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
				    foreach (System.Data.DataRow curRow in dt.Rows)
				    {
                        Snp result = new Snp();

					    result.NewResidue = DbCStr(curRow["residue"]);	
                        result.aa_position = DbCInt(curRow["aa_position"]);
                        result.MinorAlleleFrequency = DbCDouble(curRow["MinorAlleleFrequency"]);
					
					    results.Add(result);
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
        public List<Snp> ProteinLevelQueryDbSNP(System.Data.SqlClient.SqlConnection cn, string protAcc)
        {
            // Looks up SNP information for the given Accession
            List<Snp> results = new List<Snp>();

            StringBuilder sqlStr = new StringBuilder();

            sqlStr.Append(" SELECT distinct locus.residue, locus.aa_position, locus.snp_id, snp.freq as MinorAlleleFrequency");
            sqlStr.Append(" from b137_SNPContigLocusIdTRIMMED AS locus");
            sqlStr.Append(" left join");
            sqlStr.Append("     (select tgp.snp_id, round(tgp.freq,3) as freq, tgp.is_minor_allele, lll.allele");
            sqlStr.Append("     from SNPAlleleFreq_TGP_TRIMMED_FreqPt01Plus AS tgp");
            sqlStr.Append("     left join Allele lll on tgp.allele_id = lll.allele_id");
            //sqlStr.Append("     where tgp.is_minor_allele = 1"); //TRIMMED_...Pt01 table is already restricted to only minor alleles.
            sqlStr.Append("     ) snp");
            sqlStr.Append(" on snp.snp_id = locus.snp_id");
            sqlStr.Append(" WHERE locus.protein_acc = '" + protAcc + "'");
            sqlStr.Append(" AND fxn_class in (41, 44) ");
            sqlStr.Append(" ORDER BY locus.aa_position;");

            System.Data.DataTable dt = null;

            //Get a table to hold the results of the query

            using (SqlDataAdapter Da = new SqlDataAdapter(sqlStr.ToString(), cn))
            {
                using (DataSet Ds = new System.Data.DataSet())
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
                foreach (System.Data.DataRow curRow in dt.Rows)
                {
                    Snp result = new Snp();

                    result.NewResidue = DbCStr(curRow["residue"]);
                    result.aa_position = DbCInt(curRow["aa_position"]);
                    result.MinorAlleleFrequency = DbCDouble(curRow["MinorAlleleFrequency"]);

                    results.Add(result);
                }

                dt.Dispose();

            }

            return results;
        } //End ProteinLevel query
        
        // Converts a database field value to a string, checking for null values
        public string DbCStr(object inpObj)
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
        
        // Converts a database field value to an integer (int32), checking for null values
        public int DbCInt(object inpObj)
		{
			//If input object is DbNull, returns -1, otherwise returns Integer representation of object
			if (object.ReferenceEquals(inpObj, DBNull.Value))
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
            if (object.ReferenceEquals(inpObj, DBNull.Value))
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
