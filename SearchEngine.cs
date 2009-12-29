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

	class SearchEngine
	{
		protected GoBoard Board;

		protected Search Search = new Search();
		protected SearchMethodType SearchMethodType = SearchMethodType.AlphaBeta_ID_TT;

		public double[] TimeLeft = new double[2] { 600, 600 };
		public int[] StonesLeft = new int[2] { 0, 0 };
		public SearchOptions SearchOptions = new SearchOptions();

		public GoalBase GoalBase = null;	// unused

		public SearchEngine(GoBoard goBoard)
		{
			Board = goBoard;
		}

		public void SetSearchMethod(SearchMethodType searchMethodType)
		{
			SearchMethodType = searchMethodType;
		}

		public void ReInitialize()
		{
			Search.Stop();
			Search = new Search();
		}

		public SearchOptions GetSearchOptions(Color playerColor)
		{
			SearchOptions lSearchOptions = SearchOptions.Clone();

			int lMovesRemaining = SearchTimeEstimator.GetEstimatedMovesRemaining(Board.BoardSize, Board.MoveNbr);

			if (lSearchOptions.MaxSeconds == 0)
				lSearchOptions.MaxSeconds = (int)SearchTimeEstimator.GetSearchTime(TimeLeft[playerColor.ToInteger()], lMovesRemaining);

			if (lSearchOptions.MaxPly == 0)
				lSearchOptions.MaxPly = 4;

			return lSearchOptions;
		}

		public MoveList SimpleSearch(Color playerToMove)
		{
			return SimpleSearch(playerToMove, false);
		}

		public void JustStartThinking(Color playerToMove)
		{
			SearchOptions lSearchOptions = GetSearchOptions(playerToMove);

			lSearchOptions.MaxSeconds = 0;
			lSearchOptions.MaxPly = 20;
			lSearchOptions.IncludeEndGameMoves = false;
			lSearchOptions.ContinueThinkingAfterTimeOut = true;

			Search.Start(Board, playerToMove, lSearchOptions, SearchMethodType, null);
		}

		public MoveList SimpleSearch(Color playerToMove, bool endGame)
		{
			SearchOptions lSearchOptions = GetSearchOptions(playerToMove);

			lSearchOptions.IncludeEndGameMoves = endGame;

			if (endGame)
				lSearchOptions.UsePatterns = false;

			Board.Dump();
			
			Console.Error.WriteLine("Move Search: ");
			Console.Error.WriteLine("Max Level: " + lSearchOptions.MaxPly.ToString() + " - Max Time: " + lSearchOptions.MaxSeconds.ToString() + " Alpha: " + lSearchOptions.AlphaValue.ToString() + " Beta: " + lSearchOptions.BetaValue.ToString());

			Search.Start(Board, playerToMove, lSearchOptions, SearchMethodType, null);

			Search.RunUntil(lSearchOptions.MaxSeconds, lSearchOptions.EarlyTimeOut);

			SearchStatus lSearchStatus = Search.GetStatus();

			Console.Error.WriteLine("Best Move: " + Board.Coord.ToString(lSearchStatus.BestMove) + " (" + lSearchStatus.BestValue.ToString() + ")");
			Console.Error.WriteLine("Nodes: " + lSearchStatus.Nodes.ToString() + " - " + (lSearchStatus.Nodes / (lSearchStatus.Timer.SecondsElapsed != 0 ? lSearchStatus.Timer.SecondsElapsed : 1)).ToString() + " per second ");
			Console.Error.WriteLine("Evals: " + lSearchStatus.Evals.ToString() + " - " + (lSearchStatus.Evals / (lSearchStatus.Timer.SecondsElapsed != 0 ? lSearchStatus.Timer.SecondsElapsed : 1)).ToString() + " per second");
			Console.Error.WriteLine("Total: " + (lSearchStatus.Evals + lSearchStatus.Nodes).ToString() + " - " + ((lSearchStatus.Evals + lSearchStatus.Nodes) / (lSearchStatus.Timer.SecondsElapsed != 0 ? lSearchStatus.Timer.SecondsElapsed : 1)).ToString() + " per second");
			Console.Error.WriteLine("TT Hits: " + lSearchStatus.TranspositionTableHits.ToString() + " - " + (lSearchStatus.TranspositionTableHits / (lSearchStatus.Timer.SecondsElapsed != 0 ? lSearchStatus.Timer.SecondsElapsed : 1)).ToString() + " per second");
			Console.Error.WriteLine("Time: " + (lSearchStatus.Timer.MilliSecondsElapsed / 1000.0).ToString() + " seconds");

			if ((lSearchStatus.BestMove == CoordinateSystem.PASS) && (!endGame) && (!lSearchOptions.IncludeEndGameMoves))
			{
				Console.Error.WriteLine("End Game Search:");
				return SimpleSearch(playerToMove, true);
			}

			if (SearchOptions.PonderOnOpponentsTime)
				Search.StartPonder(lSearchStatus.BestMove, playerToMove);
			else
				if (!SearchOptions.ContinueThinkingAfterTimeOut)
					Search.RequestStop();

			MoveList lMoveList = new MoveList(Board.BoardSize);

			lMoveList.Add(lSearchStatus.BestMove, lSearchStatus.BestValue);

			return lMoveList;
		}

		public void StopSearch()
		{
			Search.Stop();
		}

		public void SetNagCoordinator(NagCoordinator nagCoordinator)
		{
			Search.SetNagCoordinator(nagCoordinator);
		}

		protected static int GetEndGameMove(GoBoard goBoard, Color playerToMove)
		{
			Random lRandom = new Random();
			List<int> lList = GoBoardHelper.GetMovesInOpponentsTerritory(goBoard, playerToMove);

			if (lList.Count != 0)
				return lList[lRandom.Next(lList.Count - 1)];

			return CoordinateSystem.PASS;
		}

		public MoveList SimpleEndGameSearch(Color playerToMove)
		{
			MoveList lMoveList = SimpleSearch(playerToMove);

			if ((lMoveList.Count == 1) && (lMoveList[0] == CoordinateSystem.PASS))
			{
				Console.Error.WriteLine("End Game Extended:");
				lMoveList = new MoveList(Board.BoardSize);
				lMoveList.Add(GetEndGameMove(Board, playerToMove));
			}

			return lMoveList;
		}
		
		public void SimpleAsyncSearch(Color playerToMove, OnCompletion onCompletion)
		{
			SearchOptions lSearchOptions = GetSearchOptions(playerToMove);

			lSearchOptions.IncludeEndGameMoves = false;
			lSearchOptions.UsePatterns = false;

			Board.Dump();

			Console.Error.WriteLine("Move Search: ");
			Console.Error.WriteLine("Max Level: " + lSearchOptions.MaxPly.ToString() + " - Max Time: " + lSearchOptions.MaxSeconds.ToString() + " Alpha: " + lSearchOptions.AlphaValue.ToString() + " Beta: " + lSearchOptions.BetaValue.ToString());

			Search.Start(Board, playerToMove, lSearchOptions, SearchMethodType, onCompletion);			
		}

		

	}
}
