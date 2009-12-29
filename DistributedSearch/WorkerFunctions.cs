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

	class WorkerFunctions
	{
		protected delegate void WorkerFunction(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy);
		protected Dictionary<string, WorkerFunction> Dictionary;
		protected static WorkerFunctions Instance = new WorkerFunctions();

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerFunctions"/> class.
		/// </summary>
		protected WorkerFunctions()
		{
			Dictionary = new Dictionary<string, WorkerFunction>();

			Add("set_boardsize", SetBoardSize, 1);
			Add("version", Version, 0);
			Add("writeline", WriteLine);
			Add("dump_board", DumpBoard, 0);
			Add("play_sequence", PlaySequence);
			Add("set_alpha_beta", SetAlphaBeta, 2);
			Add("clearboard", ClearBoard);
			Add("set_depth", SetDepth, 1);
			Add("set_permutation", SetPermutation, 1);
			Add("set_searchmethod", SetSearchMethod, 1);
			Add("abort", Abort, 0);
			Add("search", Search, 1);
			Add("add_patterns", AddPatterns);
			Add("clear_patterns", ClearPatterns, 1);
		}

		protected void Add(string functionName, WorkerFunction WorkerFunction)
		{
			Dictionary.Add(functionName, WorkerFunction);
		}

		protected void Add(string functionName, WorkerFunction WorkerFunction, int parameters)
		{
			Dictionary.Add(functionName + ":" + parameters.ToString(), WorkerFunction);
		}

		protected static WorkerFunction GetFunction(string command, int parameters)
		{
			return Instance.FindFunction(command, parameters);
		}

		protected WorkerFunction FindFunction(string method, int parameters)
		{
			WorkerFunction lFunction = null;

			if (Dictionary.TryGetValue(method, out lFunction))
				return lFunction;

			if (Dictionary.TryGetValue(method + ":" + parameters.ToString(), out lFunction))
				return lFunction;

			return null;
		}

		public static bool Execute(string command, string id, List<string> parameters, GoBoard goBoard, SearchEngine searchEngine, Worker.SendResponse proxy)
		{
			WorkerFunction lFunction = GetFunction(command, parameters.Count);

			if (lFunction == null)
				return false;

			try
			{
				lFunction(goBoard, searchEngine, parameters, id, proxy);
				return true;
			}
			catch (Exception e)
			{
				Console.Error.Write("ERROR: " + e.ToString());
				Respond(proxy, id, false, command, true); // async response
				return false;
			}
		}

		protected static void Respond(Worker.SendResponse proxy, string id)
		{
			Respond(proxy, id, true, string.Empty, false);
		}

		protected static void Respond(Worker.SendResponse proxy, string id, string message)
		{
			Respond(proxy, id, true, message, false);
		}

		protected static void Respond(Worker.SendResponse proxy, string id, bool success)
		{
			Respond(proxy, id, success, string.Empty, false);
		}

		protected static void Respond(Worker.SendResponse proxy, string id, bool success, string message)
		{
			Respond(proxy, id, success, message, false);
		}

		protected static void Respond(Worker.SendResponse proxy, string id, bool success, string message, bool async)
		{
			proxy((async ? "!" : "") + (success ? "=" : "?") + "\t" + id + "\t" + message);
		}

		protected static void WriteLine(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			foreach (string lString in parameters)
				Console.Error.WriteLine("DEBUG: " + lString);

			Respond(proxy, id);
		}

		protected static void DumpBoard(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			Console.Error.WriteLine(goBoard.ToString());

			Respond(proxy, id);
		}

		protected static void Version(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			Respond(proxy, id, Worker.Version);
		}

		protected static void SetBoardSize(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			goBoard.SetBoardSize(Convert.ToInt32(parameters[0]));

			Respond(proxy, id);
		}

		protected static void ClearBoard(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			goBoard.ClearBoard();

			Respond(proxy, id);
		}

		protected static void PlaySequence(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			if (parameters.Count > 0)
			{
				string[] lMoves = parameters[0].Split(' ');

				for (int i = 0; i < lMoves.Length / 2; i++)
					goBoard.PlayStone(lMoves[i * 2 + 1], Color.ToColor(lMoves[(i * 2)]), false);
			}

			Respond(proxy, id);
		}

		protected static void SetDepth(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			searchEngine.SearchOptions.MaxPly = Convert.ToInt32(parameters[0]);

			Respond(proxy, id);
		}

		protected static void SetPermutation(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			searchEngine.SearchOptions.Permutations = Convert.ToInt32(parameters[0]);

			Respond(proxy, id);
		}
		
		protected static void SetAlphaBeta(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			searchEngine.SearchOptions.AlphaValue = Convert.ToInt32(parameters[0]);
			searchEngine.SearchOptions.BetaValue = Convert.ToInt32(parameters[1]);

			Respond(proxy, id);
		}

		protected static void AddPatterns(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			PatternCollection lPatternCollection = new PatternCollection();

			foreach (string lString in parameters)
			{
				string lPattern = lString.Replace("~", "\n");
				lPatternCollection.Add(new Pattern(lPattern));
			}

			if (lPatternCollection.Count != 0)
			{
				if (searchEngine.SearchOptions.PatternDetector == null)
					searchEngine.SearchOptions.PatternDetector = new PatternDetector();

				searchEngine.SearchOptions.PatternDetector.Add(lPatternCollection);
			}

			Respond(proxy, id);
		}

		protected static void ClearPatterns(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			searchEngine.SearchOptions.PatternDetector = new PatternDetector();

			Respond(proxy, id);
		}


		protected static void SetSearchMethod(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			searchEngine.SetSearchMethod(SearchMethodFactory.ToType(parameters[0]));

			Respond(proxy, id);
		}

		protected static void Abort(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			searchEngine.StopSearch();

			Respond(proxy, id);
		}

		protected static void Search(GoBoard goBoard, SearchEngine searchEngine, List<string> parameters, string id, Worker.SendResponse proxy)
		{
			Color color = Color.ToColor(parameters[0]);

			AsyncResponseHelper lAsyncResponseHelper = new AsyncResponseHelper(proxy, id);

			Respond(proxy, id);

			searchEngine.SimpleAsyncSearch(color, lAsyncResponseHelper.OnCompletion);
		}

		protected class AsyncResponseHelper
		{
			protected Worker.SendResponse Proxy;
			protected string ID;

			public AsyncResponseHelper(Worker.SendResponse proxy, string id)
			{
				Proxy = proxy;
				ID = id;
			}

			public void OnCompletion(SearchStatus searchStatus)
			{
				WorkerFunctions.Respond(Proxy, ID, (searchStatus.Status == SearchStatusType.Completed), CoordinateSystem.ToString2(searchStatus.BestMove, searchStatus.BoardSize) + "\t" + searchStatus.BestValue.ToString(), true);
			}
		}
	}
}
