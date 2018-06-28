using pdfforge.DataStorage.Storage;
using pdfforge.DataStorage;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System;

// ! This file is generated automatically.
// ! Do not edit it outside the sections for custom code.
// ! These changes will be deleted during the next generation run

namespace pdfforge.PDFCreator.Conversion.Settings
{
	/// <summary>
	/// Adds a page background to the resulting document
	/// </summary>
	[ImplementPropertyChanged]
	public partial class BackgroundPage : INotifyPropertyChanged {
		#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore 67
		
		
		/// <summary>
		/// Enables the BackgroundPage action
		/// </summary>
		public bool Enabled { get; set; } = false;
		
		/// <summary>
		/// Filename of the PDF that will be used as background
		/// </summary>
		public string File { get; set; } = "";
		
		/// <summary>
		/// If true, the background will be placed on the attachment as well
		/// </summary>
		public bool OnAttachment { get; set; } = false;
		
		/// <summary>
		/// If true, the background will be placed on the cover as well
		/// </summary>
		public bool OnCover { get; set; } = false;
		
		/// <summary>
		/// Defines the way the background document is repeated.
		/// </summary>
		public BackgroundRepetition Repetition { get; set; } = BackgroundRepetition.RepeatLastPage;
		
		
		public void ReadValues(Data data, string path)
		{
			try { Enabled = bool.Parse(data.GetValue(@"" + path + @"Enabled")); } catch { Enabled = false;}
			try { File = Data.UnescapeString(data.GetValue(@"" + path + @"File")); } catch { File = "";}
			try { OnAttachment = bool.Parse(data.GetValue(@"" + path + @"OnAttachment")); } catch { OnAttachment = false;}
			try { OnCover = bool.Parse(data.GetValue(@"" + path + @"OnCover")); } catch { OnCover = false;}
			try { Repetition = (BackgroundRepetition) Enum.Parse(typeof(BackgroundRepetition), data.GetValue(@"" + path + @"Repetition")); } catch { Repetition = BackgroundRepetition.RepeatLastPage;}
		}
		
		public void StoreValues(Data data, string path)
		{
			data.SetValue(@"" + path + @"Enabled", Enabled.ToString());
			data.SetValue(@"" + path + @"File", Data.EscapeString(File));
			data.SetValue(@"" + path + @"OnAttachment", OnAttachment.ToString());
			data.SetValue(@"" + path + @"OnCover", OnCover.ToString());
			data.SetValue(@"" + path + @"Repetition", Repetition.ToString());
		}
		
		public BackgroundPage Copy()
		{
			BackgroundPage copy = new BackgroundPage();
			
			copy.Enabled = Enabled;
			copy.File = File;
			copy.OnAttachment = OnAttachment;
			copy.OnCover = OnCover;
			copy.Repetition = Repetition;
			
			return copy;
		}
		
		public override bool Equals(object o)
		{
			if (!(o is BackgroundPage)) return false;
			BackgroundPage v = o as BackgroundPage;
			
			if (!Enabled.Equals(v.Enabled)) return false;
			if (!File.Equals(v.File)) return false;
			if (!OnAttachment.Equals(v.OnAttachment)) return false;
			if (!OnCover.Equals(v.OnCover)) return false;
			if (!Repetition.Equals(v.Repetition)) return false;
			
			return true;
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("Enabled=" + Enabled.ToString());
			sb.AppendLine("File=" + File.ToString());
			sb.AppendLine("OnAttachment=" + OnAttachment.ToString());
			sb.AppendLine("OnCover=" + OnCover.ToString());
			sb.AppendLine("Repetition=" + Repetition.ToString());
			
			return sb.ToString();
		}
		
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}
		
	}
}
