using System;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	class SearchOptions
	{
		public int MaxPly = 5;
		public int MaxSeconds = 0;

		public bool SortMoves = true;
		public bool IncludeEndGameMoves = true; // TODO: only makes sense under Chinese Rules 

		public int EndGameMovesMaxPly = 5;
		public int EndGameMaxSeconds = 2;

		public bool CheckSuperKo = false;
		public bool EarlyTimeOut = true;	// used to stop a search if very unlikely to get another best moving by the timeout

		public bool PrunePassMove = true;
		public bool UsePatterns = true;
		public bool SuperKo = false;

		public int StartPly = 1;

		public bool ContinueThinkingAfterTimeOut = false;
		public bool PonderOnOpponentsTime = false;
		public int TranspositionTableSize = 1024 * 1024; // default: ~1 million entries

		// still debating if these should go here
		public int AlphaValue = -10000;
		public int BetaValue = 10000;

		public int Permutations = 0;

		public PatternDetector PatternDetector = new PatternDetector();

		/// <summary>
		/// Clone this instance.
		/// </summary>
		/// <returns></returns>
		public SearchOptions Clone()
		{
			SearchOptions lSearchOptions = new SearchOptions();

			lSearchOptions.MaxPly = MaxPly;
			lSearchOptions.MaxSeconds = MaxSeconds;
			lSearchOptions.SortMoves = SortMoves;
			lSearchOptions.IncludeEndGameMoves = IncludeEndGameMoves;
			lSearchOptions.EndGameMovesMaxPly = EndGameMovesMaxPly;
			lSearchOptions.CheckSuperKo = CheckSuperKo;
			lSearchOptions.EarlyTimeOut = EarlyTimeOut;
			lSearchOptions.EndGameMaxSeconds = EndGameMaxSeconds;
			lSearchOptions.PrunePassMove = PrunePassMove;
			lSearchOptions.UsePatterns = UsePatterns;
			lSearchOptions.StartPly = StartPly;
			lSearchOptions.SuperKo = SuperKo;
			lSearchOptions.ContinueThinkingAfterTimeOut = ContinueThinkingAfterTimeOut;
			lSearchOptions.PonderOnOpponentsTime = PonderOnOpponentsTime;
			lSearchOptions.TranspositionTableSize = TranspositionTableSize;
			lSearchOptions.PatternDetector = PatternDetector.Clone();
			lSearchOptions.AlphaValue = AlphaValue;
			lSearchOptions.BetaValue = BetaValue;
			lSearchOptions.Permutations = Permutations;

			return lSearchOptions;
		}
	}
}
