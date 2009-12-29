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
using System.IO;
using System.Threading;

namespace GoTraxx
{
	class Program
	{
		public static int Main(string[] args)
		{
			if (args.Length == 0)
				return ConsoleGTP();
			else
				return ProcessOptions(args);
		}

		public static int ProcessOptions(string[] args)
		{
			foreach (string lArg in args)
			{
				string lOption = lArg;
				string lParameters = string.Empty;
				int lPos = lOption.IndexOf(':');

				if (lPos > 0)
				{
					lParameters = lArg.Substring(lPos + 1);
					lOption = lArg.Substring(0, lPos);
				}

				if ((lOption[0] != '-') && (lOption[0] != '/'))
					return -1;

				lOption = lOption.Substring(1);

				string[] lParameter = lParameters.Split(':');

				switch (lOption)
				{
					case "gtp": if (lParameters != string.Empty) return ConsoleGTP(lParameters, false); else return ConsoleGTP();
					case "silentgtp": if (lParameters != string.Empty) return ConsoleGTP(lParameters, true); else return ConsoleGTP();
					case "cgos": return ConnectToCGOS(lParameter[0], lParameter[1], lParameter.Length <= 3 ? 1 : Convert.ToInt32(lParameter[2]));
					case "regression": return Regression(lParameters);
					case "regressions": return Regressions(lParameter[0], lParameter[1]);
					case "igs31": return IGS31();
					case "czd": return CZD();
					case "test": return Test();
					case "selfplay": return SelfPlay();
					case "performance": return Performance();
					case "profilertest": return ProfilerTest();
					case "testserver": return LaunchTest();
					case "worker": return LaunchWorker(lParameter[0], Convert.ToInt32(lParameter[1]));
					default: return -1;
				}
			}

			return -1;
		}

		public static int ConsoleGTP()
		{
			return ConsoleGTP((MemFile)null, false);
		}

		public static int ConsoleGTP(string filename, bool loadSilent)
		{
			MemFile lMemFile = new MemFile(filename);

			if (!lMemFile.IsError())
				return ConsoleGTP(lMemFile, loadSilent);

			Console.WriteLine("unable to load file: " + filename + " because: " + lMemFile.GetErrorMessage());
			return -1;
		}

		public static int ConsoleGTP(MemFile file, bool loadSilent)
		{
			GoBoard lGoBoard = new GoBoard(9);
			GTPGoBoard lGTPGoBoard = new GTPGoBoard(lGoBoard);
			GTPCommConsole lGTPCommConsole = new GTPCommConsole();
			GTPEngine lGTPEngine = new GTPEngine(lGTPGoBoard, lGTPCommConsole);

			lGTPCommConsole.Silent = loadSilent;

			if (file != null)
				while (!file.EOF)
				{
					string lLine = file.ReadLine();
					Console.Error.WriteLine(lLine.Trim('\n'));
					lGTPCommConsole.SendToEngine(lLine + '\n');
				}

			lGTPCommConsole.Silent = false;

			lGTPCommConsole.Listen();

			return 0;
		}

		public static int ConnectToCGOS(string name, string pPwd, int pNbr)
		{
			GoBoard lGoBoard = new GoBoard(9);
			GTPGoBoard lGTPGoBoard = new GTPGoBoard(lGoBoard);
			GTPCommCGOS lGTPCommCGOS = new GTPCommCGOS("cgos.boardspace.net", 6867, name, pPwd, pNbr, true);
			GTPEngine lGTPEngine = new GTPEngine(lGTPGoBoard, lGTPCommCGOS);

			lGTPCommCGOS.Run();

			return 0;
		}

		public static int Regression(string pFile)
		{
			return GTPRegression.ExecuteTest(pFile) ? 0 : 1;
		}

		public static int Regressions(string directory, string mask)
		{
			return GTPRegression.ExecuteTest(directory, mask) ? 0 : 1;
		}

		public static int IGS31()
		{
			GameRecords lGameRecords = new GameRecords();
			SGFCollection lSGFCollection = new SGFCollection(@"Regression\IGS_31\Source\IGS_31_counted.sgf");
			lGameRecords.Load(lSGFCollection, true);

			Dictionary<SafetyStatus, int> lSafetyUsage = new Dictionary<SafetyStatus, int>();

			foreach (GameRecord lGameRecord in lGameRecords.Games)
			{
				GoBoard lGoBoard = new GoBoard(19);
				GameRecordBoardAdapter.Apply(lGameRecord, lGoBoard);
				//lGameRecord.Apply(lGoBoard);

				foreach (GoCell lCell in lGoBoard.Cells)
				{
					SafetyStatus lSafetyStatus = lGoBoard.GetSafetyStatus(lCell.Index);

					if (lSafetyUsage.ContainsKey(lSafetyStatus))
						lSafetyUsage[lSafetyStatus] += 1;
					else
						lSafetyUsage[lSafetyStatus] = 1;
				}

				Console.Write(".");
			}
			Console.WriteLine();
			foreach (SafetyStatus lSafetyStatus in lSafetyUsage.Keys)
			{
				Console.Write(lSafetyStatus.ToInteger());
				Console.Write(" | {0}", lSafetyStatus);
				Console.WriteLine(" | " + lSafetyUsage[lSafetyStatus].ToString());
			}

			MemFile lMemFile = new MemFile();
			lMemFile.WriteLine(lGameRecords.ToString());
			lMemFile.SaveFile(@"Regression\IGS_31\IGS_31-combined.sgf");

			SafetySolverType lSafetySolverType = SafetySolverType.Muller97;

			for (int i = 0; i < lGameRecords.Games.Count; i++)
			{
				GoBoard lGoBoard = new GoBoard(19);
				GameRecordBoardAdapter.Apply(lGameRecords[i], lGoBoard);
				//lGameRecords[i].Apply(lGoBoard);
				GameRecordBoardAdapter.UpdateTerritory(lGameRecords[i], lGoBoard);
				//lGameRecords[i].UpdateTerritory(lGoBoard);
				//Console.WriteLine(lGoBoard.ToString());
				Console.Write(i.ToString());
				Console.Write(" : ");
				Console.Write(lGoBoard.CountSafePoints(Color.Both, lSafetySolverType).ToString());
				Console.Write(" ");
				Console.Write(lGoBoard.CountSafePoints(Color.Black, lSafetySolverType).ToString());
				Console.Write(" ");
				Console.WriteLine(lGoBoard.CountSafePoints(Color.White, lSafetySolverType).ToString());
			}

			for (int i = 0; i < lGameRecords.Games.Count; i++)
			{

				MemFile lMemFile2 = new MemFile();
				lMemFile2.WriteLine(lGameRecords[i].ToString());
				lMemFile2.SaveFile(@"Regression\IGS_31\IGS_31-" + ((i + 1).ToString()) + ".sgf");
			}

			return 0;
		}

		public static int CZD()
		{
			GameRecords lGameRecords = new GameRecords();
			lGameRecords.Load(@"Regression\CZD\Source", @"CZD_*.sgf", true);

			Dictionary<SafetyStatus, int> lSafetyUsage = new Dictionary<SafetyStatus, int>();

			foreach (GameRecord lGameRecord in lGameRecords.Games)
			{
				GoBoard lGoBoard = new GoBoard(19);
				GameRecordBoardAdapter.Apply(lGameRecord, lGoBoard);
				//lGameRecord.Apply(lGoBoard);

				foreach (GoCell lCell in lGoBoard.Cells)
				{
					SafetyStatus lSafetyStatus = lGoBoard.GetSafetyStatus(lCell.Index);

					if (lSafetyUsage.ContainsKey(lSafetyStatus))
						lSafetyUsage[lSafetyStatus] += 1;
					else
						lSafetyUsage[lSafetyStatus] = 1;
				}

				Console.Write(".");
			}
			Console.WriteLine();
			foreach (SafetyStatus lSafetyStatus in lSafetyUsage.Keys)
			{
				Console.Write(lSafetyStatus.ToInteger());
				Console.Write(" | {0}", lSafetyStatus);
				Console.WriteLine(" | " + lSafetyUsage[lSafetyStatus].ToString());
			}

			MemFile lMemFile = new MemFile();
			lMemFile.WriteLine(lGameRecords.ToString());
			lMemFile.SaveFile(@"Regression\CZD\CZD-combined.sgf");

			SafetySolverType lSafetySolverType = SafetySolverType.Muller04;

			for (int i = 0; i < lGameRecords.Games.Count; i++)
			{
				GoBoard lGoBoard = new GoBoard(19);
				GameRecordBoardAdapter.Apply(lGameRecords[i], lGoBoard);
				//lGameRecords[i].Apply(lGoBoard);
				GameRecordBoardAdapter.UpdateTerritory(lGameRecords[i], lGoBoard);
				//lGameRecords[i].UpdateTerritory(lGoBoard);
				//Console.WriteLine(lGoBoard.ToString());
				Console.Write(i.ToString());
				Console.Write(" : ");
				Console.Write(lGoBoard.CountSafePoints(Color.Both, lSafetySolverType).ToString());
				Console.Write(" ");
				Console.Write(lGoBoard.CountSafePoints(Color.Black, lSafetySolverType).ToString());
				Console.Write(" ");
				Console.WriteLine(lGoBoard.CountSafePoints(Color.White, lSafetySolverType).ToString());
			}

			for (int i = 0; i < lGameRecords.Games.Count; i++)
			{
				MemFile lMemFile2 = new MemFile();
				lMemFile2.WriteLine(lGameRecords[i].ToString());
				lMemFile2.SaveFile(@"Regression\CZD\" + lGameRecords[i].GameName.Insert(3, "-") + ".sgf");
			}

			return 0;
		}

		public static int Performance()
		{
			PerformanceTest.GamePlayTestSolver(10000, false);
			PerformanceTest.GamePlayTestSolver(10000, true);
			PerformanceTest.GamePlayTest(100, false, false);
			PerformanceTest.GamePlayTest(100, false, true);
			PerformanceTest.GamePlayTest(100, true, false);
			PerformanceTest.GamePlayTest(100, true, true);
			PerformanceTest.GamePlayTest(1000, false, false);
			PerformanceTest.GamePlayTest(1000, false, true);
			PerformanceTest.GamePlayTest(1000, true, false);
			PerformanceTest.GamePlayTest(1000, true, true);
			return 0;
		}

		public static int ProfilerTest()
		{
			PerformanceTest.GamePlayTest(2, false, true);
			return 0;
		}

		public static int Debug()
		{
			GoBoard lGoBoard = new GoBoard(9);
			GTPGoBoard lGTPGoBoard = new GTPGoBoard(lGoBoard);
			GTPCommConsole lGTPCommConsole = new GTPCommConsole();
			GTPEngine lGTPEngine = new GTPEngine(lGTPGoBoard, lGTPCommConsole);

			lGTPEngine.Receive("boardsize 9\n");
			lGTPEngine.Receive("gogui-play_sequence b A3 b A5 b A6 b A7 b A8 b A9 b B3 b B5 b B9 b C2 b C3 b C4 b C5 b C6 b C7 b D1 b D2 b D4 b D6 b D7 b E5 b E6 b F6 b G3 b G4 b G5 b G6 b G7 b H3 b H5 b H7 b H9 b J5 b J6 b J7 w A1 w A2 w B1 w B2 w B6 w B7 w B8 w C8 w C9 w D3 w D8 w E1 w E2 w E3 w E4 w E7 w E8 w F1 w F3 w F4 w F7 w F8 w F9 w G2 w G8 w G9 w H2 w H4 w H8 w J1 w J2 w J3 w J4 w J8\n");
			lGTPEngine.Receive("play b PASS\n");
			lGTPEngine.Receive("showboard\n");
			lGTPEngine.Receive("top_moves white\n");

			//			lGTPCommConsole.Listen();

			return 0;
		}


		public static int SelfPlay()
		{
			GoBoard lGoBoard = new GoBoard(19);
			GTPGoBoard lGTPGoBoard = new GTPGoBoard(lGoBoard);
			GTPCommConsole lGTPCommConsole = new GTPCommConsole();
			GTPEngine lGTPEngine = new GTPEngine(lGTPGoBoard, lGTPCommConsole);

			lGTPEngine.Receive("boardsize 9\n");

			while (!lGoBoard.GameOver)
			{
				lGTPCommConsole.SendToEngine("genmove b\n");
				lGTPCommConsole.SendToEngine("showboard\n");
				lGTPCommConsole.SendToEngine("genmove w\n");
				lGTPCommConsole.SendToEngine("showboard\n");
			}

			return 0;
		}

		public static int Test()
		{
			return Investigate(@"C:\Go Games", "ProfessionalGoGames-01.sgc");
		}

		public static int Investigate(string directory, string mask)
		{
			GameRecords lGameRecords = new GameRecords();
			lGameRecords.Load(directory, mask, true);

			List<GameRecordFilter.Result> lResults = GameRecordFilter.Filter(lGameRecords, GameRecordFilter.FindMovesInProtectedAreas);

			foreach (GameRecordFilter.Result lResult in lResults)
				Console.WriteLine(lResult.GameRecord.GameName + " (" + lResult.MoveNbr.ToString() + ") ->  " + CoordinateSystem.ToString2(lResult.GameRecord[lResult.MoveNbr + 1].Move, lResult.GameRecord.BoardSize));

			return 0;
		}

		public static int LaunchWorker(string address, int port)
		{
			Worker lWorker = new Worker(address, port, string.Empty, string.Empty, 5, true);
			lWorker.Run();
			return 0;
		}

		// temp. for testing
		static void MatrixCompileToFile()
		{
			PatternCollection lPatterns = new PatternCollection();

			if (!lPatterns.Load(@"Patterns/fuseki9.db"))
			{
				Console.WriteLine("ERROR: Unable to load patterns");
				return;
			}

			Console.WriteLine("STATUS: Loaded " + lPatterns.Count + " Patterns.");

			SimpleTimer lTimer = new SimpleTimer();

			Console.WriteLine(lTimer.StartTime.ToString());

			DFAMatrixBuilder lDFAMatrixBuilder = new DFAMatrixBuilder();

			lDFAMatrixBuilder.Add(lPatterns);

			Console.WriteLine(DateTime.Now.ToString());

			lDFAMatrixBuilder.BuildThreaded();

			Console.WriteLine(DateTime.Now.ToString());

			Console.WriteLine("Seconds: " + lTimer.SecondsElapsed.ToString());

			MemFile lMemFile = new MemFile();

			lMemFile.Write(lDFAMatrixBuilder.GetMatrix().ToString());

			lMemFile.SaveFile(@"Patterns/fuseki9.cdb");
		}

		// temp. for testing
		static void LoadCompiledMatrix()
		{
			Console.WriteLine(DateTime.Now.ToString());
			SimpleTimer lTimer = new SimpleTimer();

			DFAMatrix lDFAMatrix = new DFAMatrix(@"Patterns/fuseki9.cdb");

			Console.WriteLine(lDFAMatrix.DFANodes.Count);

			Console.WriteLine(lTimer.SecondsElapsed.ToString());

			GC.Collect();
		}


		// temp. for testing
		public static void LaunchWorker(int port)
		{
			LaunchWorker("localhost", port);
		}

		// temp. for testing
		public static void Nag()
		{
		}

		// temp. for testing
		/// <summary>
		/// Launches the test.
		/// </summary>
		/// <returns></returns>
		public static int LaunchTest2()
		{
			GoBoard lGoBoard = new GoBoard(9);
			GameRecord lGameRecord = new GameRecord();

			SGFCollection lSGFCollection = new SGFCollection();
			lSGFCollection.LoadSGFFromMemory(SGFGameSamples.DYER);
			lSGFCollection.RetrieveGame(lGameRecord);
			GameRecordBoardAdapter.Apply(lGameRecord, lGoBoard, false);

			lGoBoard.Dump();

			PatternCollection lPatternCollection = new PatternCollection(@"Patterns\test.db");

			NagCoordinator lNagCoordinator = new NagCoordinator(9999, lPatternCollection);

			lNagCoordinator.Initialize(lGoBoard);
			ThreadPoolHelperWithParam<int>.Execute(LaunchWorker, 9999);
			ThreadPoolHelperWithParam<int>.Execute(LaunchWorker, 9999);
			ThreadPoolHelperWithParam<int>.Execute(LaunchWorker, 9999);
			ThreadPoolHelperWithParam<int>.Execute(LaunchWorker, 9999);
			Thread.Sleep(1000 * 1);

			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);
			lNagCoordinator.CreateNagPoints(lGoBoard, -10000, 10000, Color.Black, 30, 0, 0, 1);


			Thread.Sleep(1000 * 300);

			return 0;
		}


		// temp. for testing
		/// <summary>
		/// Launches the test.
		/// </summary>
		/// <returns></returns>
		public static int LaunchTest()
		{
			GoBoard lGoBoard = new GoBoard(9);
			GameRecord lGameRecord = new GameRecord();

			SGFCollection lSGFCollection = new SGFCollection();			
			//lSGFCollection.LoadSGFFromMemory(SGFGameSamples.DYER);
//			lSGFCollection.LoadSGFFile(@"x:\CodePlex\test2.sgf");
//			lSGFCollection.RetrieveGame(lGameRecord);
//			GameRecordBoardAdapter.Apply(lGameRecord, lGoBoard, false);

			lGoBoard.Dump();

			PatternCollection lPatternCollection = new PatternCollection(@"Patterns\test.db");

			NagCoordinator lNagCoordinator = new NagCoordinator(9999, lPatternCollection);

			//ThreadPoolHelperWithParam<int>.Execute(LaunchWorker, 9999);

			Thread.Sleep(1000 * 14);

			SearchEngine lSearchEngine = new SearchEngine(lGoBoard);

			lSearchEngine.SetSearchMethod(SearchMethodType.AlphaBeta_NAG_ID_TT);
			lSearchEngine.SetNagCoordinator(lNagCoordinator);
			lSearchEngine.SearchOptions.MaxPly = 40;
			lSearchEngine.SearchOptions.MaxSeconds = 2000;
			lSearchEngine.SearchOptions.PatternDetector = new PatternDetector(lPatternCollection);

			lSearchEngine.SimpleSearch(Color.Black);

			Thread.Sleep(1000 * 10);

			return 0;
		}

	}
}
