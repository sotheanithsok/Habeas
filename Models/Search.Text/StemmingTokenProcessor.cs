using System.Collections.Generic;
using Porter2Stemmer;
using System;
using System.Linq;
namespace Search.Text
{
    public class StemmingTokenProcesor : NormalTokenProcessor, ITokenProcessor
    {
        /// <summary>
        /// Process token and stem each terms
        /// </summary>
        /// <param name="token">a token to be processed</param>
        /// <returns>stemmed terms</returns>
        public new List<string> ProcessToken(string token)
        {
            //process token with NormalTokenProcessor first
            List<string> result = base.ProcessToken(token);

            //Stem terms in the result
            for (int i = 0; i < result.Count; i++)
            {
                string[] s = result[i].Split(" ");
                for (int j = 0; j < s.Length; j++)
                {
                    s[j] = this.StemWords(s[j]);
                }
                result[i] = string.Join(" ", s);
            }
            return result;
        }

        /// <summary>
        /// A token processor uses to generate stem of such token
        /// </summary>
        /// <param name="token">Preprocessing token</param>
        /// <returns>Postprocessing term</returns>
        public string StemWords(string token)
        {
            return new EnglishPorter2Stemmer().Stem(token).Value;
        }
    }
}