/*
 * Copyright (c) 2007 Philipp Garcia (phil@gotraxx.org)
 * 
 * This file is part of GoTraxx (www.gotraxx.org).
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * This license governs use of the accompanying software. If you use the software, you 
 * accept this license. If you do not accept the license, do not use the software.
 * 
 * Permission is granted to anyone to use this software for any noncommercial purpose, 
 * and to alter it and redistribute it freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not claim that 
 *    you wrote the original software. 
 * 
 * 2. Altered source versions must be plainly marked as such, and must not be 
 *    misrepresented as being the original software.
 * 
 * 3. If you bring a patent claim against the original author or any contributor over 
 *    patents that you claim are infringed by the software, your patent license from 
 *    such contributor to the software ends automatically.
 * 
 * 4. This software may not be used in whole, nor in part, to enter any competition 
 *    without written permission from the original author. 
 * 
 * 5. This notice may not be removed or altered from any source distribution.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	class SearchStandard : ISearchInterface
	{
		protected GoBoard Board;
		protected SearchOptions Options;
		protected Random Random = new Random();

		public SearchStandard()
		{
		}

		/// <summary>
		/// Initializes the interface.
		/// </summary>
		/// <param name="goBoard">The Go board.</param>
		/// <param name="searchInstanceOptions">The search instance options.</param>
		public void Initialize(GoBoard goBoard, SearchOptions searchInstanceOptions)
		{
			Board = goBoard;
			Options = searchInstanceOptions;
		}

		/// <summary>
		/// Gets a list of legal moves.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public MoveList GetMoveList(Color playerToMove, bool forceEndGameMoves)
		{
			MoveList lMoves = new MoveList(Board.BoardSize);

			if (!forceEndGameMoves)
			{
				for (int lMove = 0; lMove < Board.Coord.BoardArea; lMove++)
					if (Board.IsLegal(lMove, playerToMove))
					{
						SafetyStatus lSafetyStatus = Board.GetSafetyStatus(lMove);

						if (lSafetyStatus.IsUndecided || (lSafetyStatus.IsUnsurroundable && !lSafetyStatus.IsDame))
							lMoves.Add(lMove);
					}

				lMoves.Add(CoordinateSystem.PASS);

				return lMoves;
			}

			// during the very final end game - when players fill dame and capture dead stones

			for (int lMove = 0; lMove < Board.Coord.BoardArea; lMove++)
			{
				SafetyStatus lSafetyStatus = Board.GetSafetyStatus(lMove);

				if ((lSafetyStatus.IsUnsurroundable) || (lSafetyStatus.IsDame))
					if (Board.IsLegal(lMove, playerToMove))
					{
						if (lSafetyStatus.IsUnsurroundable)
							lMoves.Add(lMove);
						else
							if (lSafetyStatus.IsDame)
								lMoves.Add(lMove);
					}
			}

			if (lMoves.Count != 0)
				return lMoves;

			for (int lMove = 0; lMove < Board.Coord.BoardArea; lMove++)
			{
				SafetyStatus lSafetyStatus = Board.GetSafetyStatus(lMove);

				if ((lSafetyStatus.IsDead) && (Board.Cells[lMove].Color == playerToMove.Opposite))
					foreach (int lMove2 in ((GoBlock)Board.Cells[lMove].Block).Liberties)
						if (Board.IsLegal(lMove2, playerToMove))
							lMoves.Add(lMove2);
			}

			// just in case / should still be fast
			lMoves.Add(CoordinateSystem.PASS);

			return lMoves;
		}

		/// <summary>
		/// Prunes moves from the list.
		/// </summary>
		/// <param name="moves">The moves.</param>
		public void PruneMoveList(MoveList moves)
		{
			if ((Options.PrunePassMove) && (moves.Count > 2))
				moves.RemoveMove(CoordinateSystem.PASS);

			return;
		}

		/// <summary>
		/// Sorts the move list.
		/// </summary>
		/// <param name="moves">The moves.</param>
		/// <returns></returns>
		public void SortMoveList(MoveList moves)
		{
			moves.QuickSort();
		}

		/// <summary>
		/// Evaulutes the Board
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public int Evaluate(Color playerToMove)
		{
			return (SimpleBoardEvaluator.GetScore(Board, playerToMove));
		}

		/// <summary>
		/// Determines whether the game is over, or that the goal has been accomplished
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is over; otherwise, <c>false</c>.
		/// </returns>
		public bool IsGameOver()
		{
			return Board.GameOver;
		}

		public void PrioritizeMoves(MoveList moves, Color playerToMove)
		{
			List<int>[] lProtected = new List<int>[2];
			lProtected[0] = GoBoardHelper.GetProtectedLiberites(Board, playerToMove);
			lProtected[1] = GoBoardHelper.GetProtectedLiberites(Board, playerToMove.Opposite);

			List<int>[] lNeighborsOfProtected = new List<int>[2];
			lNeighborsOfProtected[0] = GoBoardHelper.GetNeighbors(Board, lProtected[0]);
			lNeighborsOfProtected[1] = GoBoardHelper.GetNeighbors(Board, lProtected[1]);

			foreach (int lMove in moves)
				moves.SetValue(lMove, GetMovePriority(lMove, playerToMove, lProtected[0].Contains(lMove), lProtected[1].Contains(lMove), lNeighborsOfProtected[0].Contains(lMove), lNeighborsOfProtected[1].Contains(lMove)));
		}

		private double GetMovePriority(int move, Color playerToMove, bool selfProtected, bool oppProtected, bool selfProtectedNeighbor, bool oppProtectedNeighbor)
		{
			if (move == CoordinateSystem.PASS)
				return -100;

			if (Board.Coord.OnCorner(move))
				return -2;

			if (Board.Coord.OnEdge(move))
				return -1;

			if (selfProtected)
				return -4;

			if (oppProtected)
				return -3;

			if (oppProtectedNeighbor)
				return 99;

			if (selfProtectedNeighbor)
				return 90;

			return Random.Next(50) + 10;
		}

	}
}
