﻿namespace Search.Document {
	/// <summary>
	/// Represents a document saved as a file on the local file system.
	/// </summary>
	public interface IFileDocument : IDocument {
		/// <summary>
		/// The absolute path to the file for the document.
		/// </summary>
		string FilePath { get; }
		string FileName { get; }
	}
}