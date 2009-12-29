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
	class SearchMethodAB_NAG_ID_TT : SearchMethod, ISearchMethodInterface
	{
		protected TranspositionTablePlus TranspositionTable;
		protected TranspositionTablePlus TranspositionTablePrimary;
		protected TranspositionTablePlus TranspositionTableEndGame;

		protected bool SearchComplete;
		protected int BestMove;

		protected int SolvedDepth = -1;
		protected int SolvedValue = 0;

		NagCoordinator NagCoordinator;

		protected volatile bool Nag = false;

		public SearchMethodAB_NAG_ID_TT(ISearchInterface searchInterface)
			: base(searchInterface)
		{
		}

		public int Search(Color playerToMove, int maxPly, int depth, int alpha, int beta)
		{
			ZobristHash lZobristHash = Board.ZobristHash.Clone();
			lZobristHash.Mark(playerToMove);
			int lTryMove = CoordinateSystem.PASS;

			if (depth > 1)
			{
				TranspositionTablePlus.Node lNode = TranspositionTable.Retrieve(lZobristHash);

				if ((lNode.Flag != TranspositionTablePlus.NodeType.Unknown) && (lNode.Height > 0))
				{
					if ((maxPly - depth) <= lNode.Height)
					{
						TranspositionTableHits++;
						switch (lNode.Flag)
						{
							case TranspositionTablePlus.NodeType.Exact:
								{
									SearchComplete = false;
									return lNode.Value;
								}
							case TranspositionTablePlus.NodeType.LowerBound:
								{
									alpha = (alpha > lNode.Value) ? alpha : lNode.Value;
									break;
								}
							case TranspositionTablePlus.NodeType.UpperBound:
								{
									beta = (beta < lNode.Value) ? beta : lNode.Value;
									break;
								}
						}

						if (alpha >= beta)
							return lNode.Value;
					}

					lTryMove = lNode.Move;
				}
			}

			if ((depth == maxPly) || ((depth != 0) && (SearchInterface.IsGameOver())))
			{
				if (depth == maxPly)
					SearchComplete = false;

				NodesEvaluated++;
				int lEval = SearchInterface.Evaluate(playerToMove);

				if (lEval <= alpha)
					TranspositionTable.Record(maxPly - depth, lEval, TranspositionTablePlus.NodeType.LowerBound, CoordinateSystem.PASS, lZobristHash);
				else if (lEval >= beta)
					TranspositionTable.Record(maxPly - depth, lEval, TranspositionTablePlus.NodeType.UpperBound, CoordinateSystem.PASS, lZobristHash);
				else
					TranspositionTable.Record(maxPly - depth, lEval, TranspositionTablePlus.NodeType.Exact, CoordinateSystem.PASS, lZobristHash);

				return lEval;
			}

			MoveList lMoves = SearchInterface.GetMoveList(playerToMove, SearchOptions.IncludeEndGameMoves);

			SearchInterface.PruneMoveList(lMoves);

			SearchInterface.PrioritizeMoves(lMoves, playerToMove);

			if (lTryMove != CoordinateSystem.PASS)
				if (lMoves.Contains(lTryMove))
					lMoves.SetValue(lTryMove, Int32.MaxValue);

			if ((SearchOptions.UsePatterns) && (lMoves.Count != 0))
			{
				PatternMap lPatternMap = SearchOptions.PatternDetector.FindPatterns(Board, playerToMove, lMoves.AllMoves);
				lPatternMap.UpdateMoveList(lMoves);
			}

			if (lMoves.Count == 0)
				lMoves.Add(CoordinateSystem.PASS);

			if ((lMoves.Count == 1) && (lMoves[0] == CoordinateSystem.PASS) && (Board.LastMove == CoordinateSystem.PASS))
			{
				int lEval = SearchInterface.Evaluate(playerToMove);
				//TranspositionTable.Record(maxPly - depth, lEval, TranspositionTablePlus.NodeType.Exact, CoordinateSystem.PASS, lZobristHash);

				return lEval;
			}

			if (Nag)
			{
				lock (this)
				{
					NagNode lNagNode = NagCoordinator.GetResult(depth);

					if (lNagNode != null)
					{
						if (!lNagNode.IsNarrowed())
						{
							Console.Error.WriteLine("*** Pruning *** "+lNagNode.StartDepth.ToString()+"/"+lNagNode.Depth.ToString());

							if (lNagNode.Depth == 1)
							{
								BestMove = lNagNode.BestMove;
								NagCoordinator.Abort(depth);
								return lNagNode.Result;
							}

							SolvedDepth = lNagNode.StartDepth;
							SolvedValue = lNagNode.Result;
							NagCoordinator.Abort(depth);
							return alpha;
						}
					}
					else
					{
						Nag = false;
					}
				}
			}

			SearchInterface.SortMoveList(lMoves);

			if (UpdateStatusFlag)
				UpdateStatus();

			if (StopThinkingFlag)
				Stop();

			NagCoordinator.CreateNagPoints(Board, alpha, beta, playerToMove, depth, maxPly, lMoves.Count, 2);

			int lSuperKoCount = 0;
			TranspositionTablePlus.NodeType lFlag = TranspositionTablePlus.NodeType.UpperBound;

			int lBestMove = lMoves[0];

			foreach (int lMove in lMoves)
			{
				NodesSearched++;

				bool lPlayed = Board.PlayStone(lMove, playerToMove, true);

				if (!lPlayed)
					throw new ApplicationException("SearchMethodAB_ID_TT.cs: You hit a bug!");

				if ((CheckSuperKo) && (Board.IsSuperKo()))
				{
					lSuperKoCount++;
					Board.Undo();
				}
				else
				{
					int lScore = -Search(playerToMove.Opposite, maxPly, depth + 1, -beta, -alpha);
					Board.Undo();

					if (SolvedDepth != -1)
					{
						if (SolvedDepth != depth)
							return alpha;

						alpha = SolvedValue;
						BestMove = lMove;
						SolvedDepth = -1;
						break;
					}

					if (lScore > alpha)
					{
						if (depth == 0)
							BestMove = lMove;

						if (lScore >= beta)
						{
							TranspositionTable.Record(maxPly - depth, alpha, TranspositionTablePlus.NodeType.LowerBound, lMove, lZobristHash);

							NagCoordinator.Abort(depth);
							return beta;
						}

						alpha = lScore;
						lFlag = TranspositionTablePlus.NodeType.Exact;
						lBestMove = lMove;
					}
				}
			}

			if (lSuperKoCount == lMoves.Count)
			{
				NagCoordinator.Abort(depth);
				return SearchInterface.Evaluate(playerToMove);
			}

			TranspositionTable.Record(maxPly - depth, alpha, lFlag, lBestMove, lZobristHash);

			NagCoordinator.Abort(depth);
			return alpha;
		}

		protected void Think(Color playerToMove)
		{
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
				lBestValue = Search(playerToMove, lDepth, 0, -10000, 10000);
				Console.Error.WriteLine("+Ply: " + lDepth + " - " + SearchStatus.Timer.SecondsElapsed + " Seconds - Nodes/TT Hits: " + NodesSearched.ToString() + "/" + TranspositionTableHits.ToString() + " - Best: " + Board.Coord.ToString(BestMove) + " (" + lBestValue + ")");

				SearchStatus.UpdateBestMove(BestMove, lBestValue);
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

			NagCoordinator.StopAll();
		}

		public override void GoThink()
		{
			Think(PlayerToMove);

			if (Status == SearchStatusType.Thinking)
			{
				if ((BestMove == CoordinateSystem.PASS) && (SearchOptions.IncludeEndGameMoves))
				{
					SearchOptions.IncludeEndGameMoves = true;
					SearchOptions.MaxPly = SearchOptions.EndGameMovesMaxPly;
					Think(PlayerToMove);	// fast search
				}
			}
		}

		public void SetNag()
		{
			lock (this)
			{
				Nag = true;
			}
		}

		public new void Initialize(GoBoard goBoard, Color playerToMove, SearchOptions searchOptions, OnCompletion onCompletion)
		{
			base.Initialize(goBoard, playerToMove, searchOptions, onCompletion);

			// setup distributed search coordinator & initialize workers
			if (NagCoordinator == null)
				NagCoordinator = new NagCoordinator(9999, SearchOptions.PatternDetector.Patterns);	// default port			

			NagCoordinator.SetNagCallBack(SetNag);
			NagCoordinator.Initialize(goBoard);

			if (TranspositionTable == null)
				TranspositionTablePrimary = new TranspositionTablePlus(SearchOptions.TranspositionTableSize);
			else
				if (TranspositionTable.Size != SearchOptions.TranspositionTableSize)
					TranspositionTablePrimary = new TranspositionTablePlus(SearchOptions.TranspositionTableSize);

			if (TranspositionTableEndGame == null)
				TranspositionTableEndGame = new TranspositionTablePlus(1024 * 1024);

			TranspositionTable = TranspositionTablePrimary;
		}

		public new void StopThinking()
		{
			base.StopThinking();
			NagCoordinator.StopAll();
		}

		public new void SetNagCoordinator(NagCoordinator nagCoordinator)
		{
			if (nagCoordinator != null)	// hack!
				NagCoordinator = nagCoordinator;
		}
	}
}
