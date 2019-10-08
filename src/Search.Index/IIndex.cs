﻿using System.Collections.Generic;

namespace Search.Index {
	/// <summary>
	/// An IIndex can retrieve postings for a term from a data structure associating terms and the documents that contain
	/// them.
	/// </summary>
	public interface IIndex {
		/// <summary>
		/// Retrieves a list of Postings of documents that contain the given term.
		/// </summary>
		/// <param name="term">a processed string</param>
		IList<Posting> GetPostings(string term);

		/// <summary>
		/// Retrieves a list of Postings of documents that contain the given list of terms.
		/// </summary>
		/// <param name="terms">a list of processed strings</param>
		IList<Posting> GetPostings(List<string> term);

		/// <summary>
		/// A (sorted) list of all terms in the index vocabulary.
		/// </summary>
		IReadOnlyList<string> GetVocabulary();
	}
}