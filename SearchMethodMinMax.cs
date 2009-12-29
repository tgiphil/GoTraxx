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
using System.Collections.ObjectModel;

using System.Text;

namespace GoTraxx
{
	class SearchMethodMinMax : SearchMethod, ISearchMethodInterface
	{

		protected MoveList MoveList;

		public SearchMethodMinMax(ISearchInterface searchInterface)
			: base(searchInterface)
		{
		}

		protected int Search(Color playerToMove, int maxPly, int depth)
		{
			if ((depth == maxPly) || ((depth != 0) && (SearchInterface.IsGameOver())))
			{
				NodesEvaluated++;
				return SearchInterface.Evaluate(playerToMove);
			}

			MoveList lMoves = SearchInterface.GetMoveList(playerToMove, SearchOptions.IncludeEndGameMoves);

			SearchInterface.PruneMoveList(lMoves);

			if ((SearchOptions.UsePatterns) && (lMoves.Count != 0))
			{
				PatternMap lPatternMap = SearchOptions.PatternDetector.FindPatterns(Board, playerToMove, lMoves.AllMoves);
				lPatternMap.UpdateMoveList(lMoves);
			}

			if (lMoves.Count == 0)
				lMoves.Add(CoordinateSystem.PASS);

			if ((lMoves.Count == 1) && (lMoves[0] == CoordinateSystem.PASS) && (Board.LastMove == CoordinateSystem.PASS))
			{
				NodesEvaluated++;
				return SearchInterface.Evaluate(playerToMove);
			}

			if (UpdateStatusFlag)
				UpdateStatus();

			if (StopThinkingFlag)
				Stop();

			int lSuperKoCount = 0;

			int lBest = Int32.MinValue;
			int lBestMove = CoordinateSystem.PASS; // dummy value

			foreach (int lMove in lMoves)
			{
				NodesSearched++;

				bool lPlayed = Board.PlayStone(lMove, playerToMove, true);

				if (!lPlayed)
					throw new ApplicationException("SearchMethodAlphaBetaIterative.cs: You hit a bug!");

				if ((CheckSuperKo) && (Board.IsSuperKo()))
				{
					lSuperKoCount++;
					Board.Undo();
				}
				else
				{
					int lScore = -Search(playerToMove.Opposite, maxPly, depth + 1);
					Board.Undo();

					if (depth == 0)
					{
						MoveList.Add(lMove, lScore);

						if (lScore > SearchStatus.BestValue)
							SearchStatus.UpdateBestMove(lMove, lScore);

						Console.Error.WriteLine("# " + MoveList.Count + " - Move: " + Board.Coord.ToString(lMove) + " (" + lScore.ToString() + ")");
					}

					if (lScore > lBest)
					{
						lBest = lScore;
						lBestMove = lMove;
					}

				}
			}

			if (lSuperKoCount == lMoves.Count)
				return SearchInterface.Evaluate(playerToMove);

			return lBest;
		}

		protected void Think(Color playerToMove)
		{
			MoveList = new MoveList(Board.BoardSize);

			int lBestValue = Search(playerToMove, SearchOptions.MaxPly, 0);

			SearchStatus.BestMove = MoveList.GetBestMove();
			SearchStatus.BestValue = lBestValue;
			SearchStatus.CurrentPly = SearchOptions.MaxPly;
			SearchStatus.MaxPly = SearchOptions.MaxPly;
			SearchStatus.PercentComplete = 100;
		}

		public override void GoThink()
		{
			Think(PlayerToMove);

			if (Status == SearchStatusType.Thinking)
			{
				if ((MoveList.GetBestMove() == CoordinateSystem.PASS) && (SearchOptions.IncludeEndGameMoves))
				{
					SearchOptions.IncludeEndGameMoves = true;
					SearchOptions.MaxPly = SearchOptions.EndGameMovesMaxPly;
					Think(PlayerToMove);	// fast search
				}
			}
		}
	}
}
