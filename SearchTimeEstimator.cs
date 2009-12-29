using System;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	static class SearchTimeEstimator
	{

		/// <summary>
		/// Gets the search time.
		/// </summary>
		/// <param name="timeLeft">The time left.</param>
		/// <param name="estimatedMovesRemaining">The estimated moves remaining.</param>
		/// <returns></returns>
		public static double GetSearchTime(double timeLeft, int estimatedMovesRemaining)
		{
			double lTimeLeft = ((timeLeft - 10) / estimatedMovesRemaining);

			if (timeLeft < 10)
				return 1.0;

			if (timeLeft < 5)
				return 0.5;

			return lTimeLeft;
		}

		/// <summary>
		/// Gets the search depth.
		/// </summary>
		/// <param name="boardSize">Size of the board.</param>
		/// <param name="moveCnt">The move count.</param>
		/// <param name="estimatedMovesRemaining">The estimated moves remaining.</param>
		/// <returns></returns>
		public static int GetSearchDepth(int boardSize, int moveCnt, int estimatedMovesRemaining)
		{
			// assumes iteration depth increase search

			if (estimatedMovesRemaining < 10)  // near the end of the game
				return boardSize * boardSize;  // max depth!

			if (moveCnt <= 10) // near the beginning of the game
				return 5; // 1-ply search

			return (moveCnt - 1) / 5; // one more depth per every 5 moves
		}

		/// <summary>
		/// Gets the estimated moves remaining.
		/// </summary>
		/// <param name="boardSize">Size of the board.</param>
		/// <param name="moveCnt">The move count.</param>
		/// <returns></returns>
		public static int GetEstimatedMovesRemaining(int boardSize, int moveCnt)
		{
			int lMovesReminaing = Convert.ToInt32(((boardSize * boardSize) - moveCnt) / 2);

			if (lMovesReminaing < 3)
				return 3;

			return lMovesReminaing;
		}
	}
}
