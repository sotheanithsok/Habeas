﻿using System.Collections.Generic;
using Search.Index;
using Search.Text;

namespace Search.Query
{
    /// <summary>
    /// Represents a phrase literal consisting of one or more terms that must occur in sequence.
	/// PhraseLiteral stores not processed tokens of a phrase
    /// </summary>
    public class PhraseLiteral : IQueryComponent
    {
        // The list of individual terms in the phrase.
        private List<string> mTerms = new List<string>();

        /// <summary>
        /// Constructs a PhraseLiteral with the given individual phrase terms.
        /// </summary>
        public PhraseLiteral(List<string> terms)
        {
            mTerms.AddRange(terms);
        }

        /// <summary>
        /// Constructs a PhraseLiteral given a string with one or more individual terms separated by spaces.
        /// </summary>
        public PhraseLiteral(string phrase)
        {
            mTerms.AddRange(phrase.Split(' '));
        }

        /// <summary>
        /// Get Postings
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="processor">Tokene processor</param>
        /// <returns></returns>
        public IList<Posting> GetPostings(IIndex index, ITokenProcessor processor)
        {
            //A list of posting lists (postings for each term in the phrase)
            List<IList<Posting>> postingLists = new List<IList<Posting>>();
            //Retrieves the postings for the individual terms in the phrase
            foreach (string term in mTerms)
            {
                //Process the term
                List<string> processedTerms = processor.ProcessToken(term);
                postingLists.Add(index.GetPositionalPostings(processedTerms));
            }
            //positional merge all posting lists
            return Merge.PositionalMerge(postingLists);
        }

        /// <summary>
        /// Convert this query componenet to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "\"" + string.Join(" ", mTerms) + "\"";
        }
    }
}
