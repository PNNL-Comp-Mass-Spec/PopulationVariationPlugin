using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using PvmrmViewer.DataObjects;
using TopDownIQGUI.ViewModel;
using MessageBox = System.Windows.Forms.MessageBox;

namespace PvmrmViewer.ViewModels
{
	public class PvmrmViewViewModel : ViewModelBase
	{
		private const int ProteinAccessionIndex = 0;
		private const int ProteinNameIndex = 1;
		private const int VariantCodexIndex = 2;
		private const int MinorAlleleFrequencyIndex = 3;
		private const int ReferencePeptideIndex = 4;
		private const int ModifiedPeptideIndex = 5;
		private const int SnpIdIndex = 6;

		public string InputFileName { get; set; }
		public string Text { get; set; }
		public List<PvmrmEntry> Entries { get; set; }

		public PvmrmViewViewModel()
		{
			Entries = new List<PvmrmEntry>();
		}

		public void Open()
		{
			Text = File.ReadAllText(InputFileName);
			using (StreamReader reader = new StreamReader(InputFileName))
			{
				string header = reader.ReadLine();
				while (reader.Peek() > -1)
				{
					string inputLine = reader.ReadLine();
					string[] splitLine = inputLine.Split('\t');
					Entries.Add(new PvmrmEntry(splitLine[ProteinAccessionIndex], splitLine[ProteinNameIndex], splitLine[VariantCodexIndex],
						splitLine[MinorAlleleFrequencyIndex], splitLine[ReferencePeptideIndex], splitLine[ModifiedPeptideIndex], "rs" + splitLine[SnpIdIndex]));
				}
			}
		}

		public void Save(string filename)
		{
			using (StreamWriter output = new StreamWriter(filename))
			{
				output.Write(Text);
			}
		}

	}
}
