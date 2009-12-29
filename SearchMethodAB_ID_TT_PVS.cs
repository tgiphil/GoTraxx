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
using System.Threading;

namespace GoTraxx
{
	class SearchMethodAB_ID_TT_PVS : SearchMethod, ISearchMethodInterface
	{
		protected PrincipalVariation PrincipalVariation;
		protected bool FollowPV;
		protected TranspositionTable TranspositionTable;
		protected TranspositionTable TranspositionTablePrimary;
		protected TranspositionTable TranspositionTableEndGame;

		protected bool SearchComplete;

		public SearchMethodAB_ID_TT_PVS(ISearchInterface searchInterface)
			: base(searchInterface)
		{
		}

		public int Search(Color playerToMove, int maxPly, int depth, int alpha, int beta)
		{
			ZobristHash lZobristHash = Board.ZobristHash.Clone();
			lZobristHash.Mark(playerToMove);

			if (depth > 1)
			{
				TranspositionTable.Node lNode = TranspositionTable.Retrieve(lZobristHash);

				if ((lNode.Flag != TranspositionTable.NodeType.Unknown) && ((maxPly - depth) <= lNode.Height))
				{
					switch (lNode.Flag)
					{
						case TranspositionTable.NodeType.Exact:
							{
								SearchComplete = false;
								TranspositionTableHits++;
								return lNode.Value;
							}
						case TranspositionTable.NodeType.LowerBound:
							{
								alpha = (alpha > lNode.Value) ? alpha : lNode.Value;
								break;
							}
						case TranspositionTable.NodeType.UpperBound:
							{
								beta = (beta < lNode.Value) ? beta : lNode.Value;
								break;
							}
					}

					if (alpha >= beta)
						return lNode.Value;
				}


			}

			if ((depth == maxPly) || ((depth != 0) && (SearchInterface.IsGameOver())))
			{
				if (depth == maxPly)
					SearchComplete = false;

				NodesEvaluated++;
				int lEval = SearchInterface.Evaluate(playerToMove);

				TranspositionTable.Record(maxPly - depth, lEval, TranspositionTable.NodeType.Exact, lZobristHash);

				return lEval;
			}

			PrincipalVariation.SetPVLength(depth);

			MoveList lMoves = SearchInterface.GetMoveList(playerToMove, SearchOptions.IncludeEndGameMoves);

			SearchInterface.PruneMoveList(lMoves);

			SearchInterface.PrioritizeMoves(lMoves, playerToMove);

			if ((SearchOptions.UsePatterns) && (lMoves.Count != 0))
			{
				PatternMap lPatternMap = SearchOptions.PatternDetector.FindPatterns(Board, playerToMove, lMoves.AllMoves);
				lPatternMap.UpdateMoveList(lMoves);
			}

			if (lMoves.Count == 0)
				lMoves.Add(CoordinateSystem.PASS);

			if ((lMoves.Count == 1) && (lMoves[0] == CoordinateSystem.PASS) && (Board.LastMove == CoordinateSystem.PASS))
			{
				PrincipalVariation.UpdatePV(depth, CoordinateSystem.PASS);

				int lEval = SearchInterface.Evaluate(playerToMove);
				TranspositionTable.Record(maxPly - depth, lEval, TranspositionTable.NodeType.Exact, lZobristHash);
				return lEval;
			}

			// follow the principle variation
			if (FollowPV)
			{
				FollowPV = false;

				int lPVMove = PrincipalVariation.GetMove(depth);

				if (lPVMove != PrincipalVariation.NO_VALUE)
					if (lMoves.Contains(lPVMove))
					{
						lMoves.SetValue(lPVMove, Int32.MaxValue);
						FollowPV = true;
					}
			}

			SearchInterface.SortMoveList(lMoves);

			if (UpdateStatusFlag)
				UpdateStatus();

			if (StopThinkingFlag)
				Stop();

			int lSuperKoCount = 0;
			TranspositionTable.NodeType lFlag = TranspositionTable.NodeType.UpperBound;

			foreach (int lMove in lMoves)
			{
				NodesSearched++;

				bool lPlayed = Board.PlayStone(lMove, playerToMove, true);

				if (!lPlayed)
					throw new ApplicationException("SearchMethodAB_ID_TT_PVS.cs: You hit a bug!");

				if ((CheckSuperKo) && (Board.IsSuperKo()))
				{
					lSuperKoCount++;
					Board.Undo();
				}
				else
				{
					int lScore = -Search(playerToMove.Opposite, maxPly, depth + 1, -beta, -alpha);
					Board.Undo();

					if (lScore > alpha)
					{
						if (lScore >= beta)
						{
							TranspositionTable.Record(maxPly - depth, alpha, TranspositionTable.NodeType.LowerBound, lZobristHash);
							return lScore;
						}

						alpha = lScore;
						lFlag = TranspositionTable.NodeType.Exact;
						PrincipalVariation.UpdatePV(depth, lMove);
					}
				}
			}

			if (lSuperKoCount == lMoves.Count)
				return SearchInterface.Evaluate(playerToMove);

			TranspositionTable.Record(maxPly - depth, alpha, lFlag, lZobristHash);

			return alpha;
		}

		protected void Think(Color playerToMove)
		{
			PrincipalVariation = new PrincipalVariation(Board.BoardSize);

			FollowPV = true;
			TranspositionTableHits = 0;
			int lBestValue = 0;

			if (SearchOptions.IncludeEndGameMoves)
				TranspositionTable = TranspositionTableEndGame;
			else
				TranspositionTable = TranspositionTablePrimary;

			int lStart = (SearchOptions.StartPly <= SearchOptions.MaxPly) ? SearchOptions.StartPly : SearchOptions.MaxPly;

			if (lStart <= 0)
				lStart = 1;

			for (int lDepth = lStart; lDepth <= SearchOptions.MaxPly; lDepth++)
			{
				SearchComplete = true;
				FollowPV = true;
				lBestValue = Search(playerToMove, lDepth, 0, -10000, 10000);
				Console.Error.WriteLine("Ply: " + lDepth + " - " + SearchStatus.Timer.SecondsElapsed + " Seconds - Nodes/TT Hits: " + NodesSearched.ToString() + "/" + TranspositionTableHits.ToString() + " - Best: " + Board.Coord.ToString(PrincipalVariation.BestMove) + " (" + lBestValue + ")");

				SearchStatus.UpdateBestMove(PrincipalVariation.BestMove, lBestValue);
				SearchStatus.CurrentPly = lDepth;
				SearchStatus.MaxPly = lDepth;
				SearchStatus.PercentComplete = (lDepth / SearchOptions.MaxPly) * 100;

				UpdateStatus();

				if (StopThinkingFlag)
					break;

				if (SearchComplete)
				{
					Console.Error.WriteLine("Ply: " + lDepth + " - Search Completed!");
					break;
				}
			}
		}

		public override void GoThink()
		{
			Think(PlayerToMove);

			if (Status == SearchStatusType.Thinking)
			{
				if ((PrincipalVariation.BestMove == CoordinateSystem.PASS) && (SearchOptions.IncludeEndGameMoves))
				{
					SearchOptions.IncludeEndGameMoves = true;
					SearchOptions.MaxPly = SearchOptions.EndGameMovesMaxPly;
					Think(PlayerToMove);	// fast search
				}
			}
		}

		public new void Initialize(GoBoard goBoard, Color playerToMove, SearchOptions searchOptions, OnCompletion onCompletion)
		{
			base.Initialize(goBoard, playerToMove, searchOptions, onCompletion);

			if (TranspositionTable == null)
				TranspositionTablePrimary = new TranspositionTable(SearchOptions.TranspositionTableSize);
			else
				if (TranspositionTable.Size != SearchOptions.TranspositionTableSize)
					TranspositionTablePrimary = new TranspositionTable(SearchOptions.TranspositionTableSize);

			if (TranspositionTableEndGame == null)
				TranspositionTableEndGame = new TranspositionTable(1024 * 1024);

			TranspositionTable = TranspositionTablePrimary;
		}

	}
}
