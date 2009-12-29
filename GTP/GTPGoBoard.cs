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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GoTraxx
{
	delegate GTPInternalResponse GTPFunction(GTPGoBoard GTPGoBoard, GTPCommand lGTcommand);

	class GTPGoBoard
	{
		protected static string ProgramName = "GoTraxx";
		protected static string ProgramVersion = "1.5.2 (beta)";

		public class AnalyzeCommand
		{
			public string Command;
			public string Type;
			public string Label;
			public string Parameters;

			public AnalyzeCommand(string command, string type, string label, string parameters)
			{
				Command = command;
				Type = type;
				Label = label;
				Parameters = parameters;
			}
		}

		protected GoBoard Board;

		protected Dictionary<string, GTPFunction> RegisteredCommands;
		protected List<AnalyzeCommand> AnalyzeCommands;

		protected static Stopwatch Timer = new Stopwatch();
		public string Directory;

		protected IScoreFactoryInterface ScoreInterface = new ScoreChineseRules();

		protected GameRecords GameRecords = new GameRecords();
		protected List<GameRecordFilter.Result> GameRecordResults = null;
		protected GameRecordFilter.FilterFunction GameFilterFunction = null;

		protected string CGOSAddress = GTPCommCGOS.DefaultAddress;
		protected int CGOSPortNbr = GTPCommCGOS.DefaultPortNbr;

		protected SearchEngine SearchEngine;

		public GTPGoBoard(GoBoard goBoard)
		{
			RegisteredCommands = new Dictionary<string, GTPFunction>();
			AnalyzeCommands = new List<AnalyzeCommand>();
			Directory = string.Empty;
			RegisterBuildInCommands();
			Board = goBoard;
			SearchEngine = new SearchEngine(Board);
		}

		private void RegisterBuildInCommands()
		{
			RegisterCommand("protocol_version", GTPProtocolVersion);
			RegisterCommand("known_command", GTPKnownComand);
			RegisterCommand("list_commands", GTPListCommands);
			RegisterCommand("cputime", GTPCPUTime);
			RegisterCommand("echo", GTPEcho);
			RegisterCommand("quit", GTPQuit);
			RegisterCommand("gogui-analyze_commands", GTPAnalyzeCommands);
			RegisterCommand("gogui_analyze_commands", GTPAnalyzeCommands); // deprecated
			RegisterCommand("time_left", GTPTimeLeft);

			RegisterCommand("name", GTPName);
			RegisterCommand("version", GTPVersion);
			RegisterCommand("clear_board", GTPClearBoard);
			RegisterCommand("boardsize", GTPGoBoardSize);
			RegisterCommand("komi", GTPKomi);
			RegisterCommand("play", GTPPlay);
			RegisterCommand("black", GTPBlack);
			RegisterCommand("white", GTPWhite);
			RegisterCommand("undo", GTPUndo);
			RegisterCommand("gg-undo", GTPGGUndo);
			RegisterCommand("loadsgf", GTPLoadSGF);
			RegisterCommand("query_boardsize", GTPQueryBoardSize);
			RegisterCommand("what_color", GTPWhatColor);
			RegisterCommand("is_legal", GTPIsLegal);
			RegisterCommand("all_legal", GTPAllLegal);
			RegisterCommand("showboard", GTPShowBoard);
			RegisterCommand("list_stones", GTPListStones);
			RegisterCommand("captures", GTPCaptures);
			RegisterCommand("set_free_handicap", GTPSetFreeHandicap);
			RegisterCommand("unconditional_status", GTPUnconditionalStatus);
			RegisterCommand("final_status", GTPUnconditionalStatus);
			RegisterCommand("genmove", GTPGenMove);
			RegisterCommand("reg_genmove", GTPRegMove);
			RegisterCommand("final_status_list", GTPFinalStatusList);
			RegisterCommand("countlib", GTPCountLib);
			RegisterCommand("genmove_cleanup", GTPGenMoveCleanUp);
			RegisterCommand("kgs-genmove_cleanup", GTPGenMoveCleanUp);
			RegisterCommand("play_sequence", GTPPlaySequence); // deprecated
			RegisterCommand("gogui-play_sequence", GTPPlaySequence);
			RegisterCommand("gogui-title", GTPName);
			RegisterCommand("top_moves", GTPTopMoves);
			RegisterCommand("top_moves_black", GTPTopMovesBlack);
			RegisterCommand("top_moves_white", GTPTopMovesWhite);
			RegisterCommand("final_score", GTPFinalScore);
			RegisterCommand("get_komi", GTPGetKomi);
			RegisterCommand("scoring_system", GTPScoringSystem);
			RegisterCommand("score_now", GTPTestScoreNow);
			RegisterCommand("level", GTPLevel);
			RegisterCommand("max_time", GTPMaxTime);
			//            RegisterCommand("goal_kill", GTPGoalKill);
			//            RegisterCommand("goal_save", GTPGoalSave);
			//            RegisterCommand("goal_clear", GTPGoalClear);
			RegisterCommand("ex_safe", GTPExSafe);
			RegisterCommand("gotraxx-safety_solver", GTPSafetySolver);
			RegisterCommand("gotraxx-search_method", GTPSearchMethod);
			RegisterCommand("gotraxx-dump_region", GTPDumcolorEnclosedRegion);
			RegisterCommand("gotraxx-protected_liberties", GTPProtectedLiberties);

			RegisterCommand("gotraxx-load_games", GTPLoadGames);
			RegisterCommand("gotraxx-clear_games", GTPClearGames);
			RegisterCommand("gotraxx-filter_games", GTPFilterGames);
			RegisterCommand("gotraxx-dump_filter_results", GTPDumpFilterResults);

			RegisterCommand("gotraxx-clear_patterns", GTPClearPatterns);
			RegisterCommand("gotraxx-load_patterns", GTPLoadPatterns);
			RegisterCommand("gotraxx-pattern_values", GTPPatternValues);

			RegisterCommand("cgos-connect", GTPCGOSConnect);
			RegisterCommand("cgos-server", GTPCGOSServer);

			RegisterCommand("gotraxx-continue_thinking", GTPContinueThinkingOption);
			RegisterCommand("gotraxx-ponder", GTPPonderOption);
			RegisterCommand("gotraxx-sleep", GTPSleep);
			RegisterCommand("gotraxx-just_think", GTPJustThink);
			RegisterCommand("gotraxx-stop_thinking", GTPStopThinking);
			RegisterCommand("gotraxx-set_start_ply", GTPSetStartPly);
			RegisterCommand("gotraxx-set_transposition_table_size", GTPSetTranspositionTableSize);

			RegisterAnalyzeCommand("showboard", "none", "Show Board", "");
			RegisterAnalyzeCommand("captures", "none", "Captures", "%c");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Unsurroundable", "unsurroundable");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Undecided", "undecided");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Alive", "alive");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Dead", "dead");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Seki", "seki");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Dame", "dame");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Unsurroundable", "unsurroundable");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Territory Black", "black_territory");
			RegisterAnalyzeCommand("final_status_list", "plist", "Final Status List Territory White", "white_territory");
			RegisterAnalyzeCommand("scoring_system", "none", "Score System Territory", "territory");
			RegisterAnalyzeCommand("scoring_system", "none", "Score System Area", "area");
			RegisterAnalyzeCommand("score_now", "none", "Score Now", "");
			RegisterAnalyzeCommand("gotraxx-pattern_values", "pspairs", "Patterns Values White", "white");
			RegisterAnalyzeCommand("gotraxx-pattern_values", "pspairs", "Patterns Values Black", "black");
			//			RegisterAnalyzeCommand("goal_kill", "none", "Goal Kill", "%p");
			//			RegisterAnalyzeCommand("goal_save", "none", "Goal Save", "%p");
			//			RegisterAnalyzeCommand("goal_clear", "none", "Goal Clear", "");
			RegisterAnalyzeCommand("gotraxx-safety_solver", "none", "Safety Solver Benson", "benson");
			RegisterAnalyzeCommand("gotraxx-safety_solver", "none", "Safety Solver Muller97", "muller97");
			RegisterAnalyzeCommand("gotraxx-safety_solver", "none", "Safety Solver Muller04", "muller04");
			RegisterAnalyzeCommand("gotraxx-safety_solver", "none", "Safety Solver Muller Plus", "muller+");
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search MinMax", "minmax");
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search AlphaBeta", "alphabeta");
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search AB ID", "alphabetaiterative");
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search AB ID TT PVS", "alphabetaiterativetransposition");
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search AB ID TT", "alphabetatranposition");
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search With Nag", "nag");
			
			RegisterAnalyzeCommand("gotraxx-search_method", "none", "Set Search Best", "best");

			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe Both Benson", "both benson");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe Black Benson", "b benson");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe White Benson", "w benson");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe Both Muller97", "both muller97");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe Black Muller97", "b muller97");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe White Muller97", "w muller97");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe Both Muller04", "both muller04");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe Black Muller04", "b muller04");
			RegisterAnalyzeCommand("ex_safe", "none", "Ex Safe White Muller04", "w muller04");
			RegisterAnalyzeCommand("gotraxx-dump_region", "none", "Dump Region", "%p");
			RegisterAnalyzeCommand("gotraxx-protected_liberties", "plist", "Protected Liberties Black", "black");
			RegisterAnalyzeCommand("gotraxx-protected_liberties", "plist", "Protected Liberties White", "white");
			RegisterAnalyzeCommand("top_moves", "pspairs", "Top Moves", "%c");
			RegisterAnalyzeCommand("top_moves", "pspairs", "Top Moves Black", "black");
			RegisterAnalyzeCommand("top_moves", "pspairs", "Top Moves White", "white");
			RegisterAnalyzeCommand("level", "none", "Level", "%s");
			RegisterAnalyzeCommand("max_time", "none", "Max Time", "%s");

			RegisterAnalyzeCommand("gotraxx-continue_thinking", "none", "Continue Thinking Enabled", "yes");
			RegisterAnalyzeCommand("gotraxx-continue_thinking", "none", "Continue Thinking Disabled", "no");

			RegisterAnalyzeCommand("gotraxx-ponder", "none", "Ponder Enabled", "yes");
			RegisterAnalyzeCommand("gotraxx-ponder", "none", "Ponder Disabled", "no");

			RegisterAnalyzeCommand("gotraxx-just_think", "none", "Just Think", "");
			RegisterAnalyzeCommand("gotraxx-stop_thinking", "none", "Stop Thinking", "");
			RegisterAnalyzeCommand("gotraxx-set_start_ply", "none", "Set Start Ply", "%s");
			RegisterAnalyzeCommand("gotraxx-set_transposition_table_size", "none", "Set TT Size", "%s");
		
			RegisterAnalyzeCommand("gotraxx-clear_patterns", "none", "Clear Patterns", "");
			RegisterAnalyzeCommand("gotraxx-load_patterns", "none", "Load Test Patterns", "patterns\\test.db");

			RegisterCommand("tt", GTPTest);
		}

		protected void RegisterCommand(string command, GTPFunction gtpFunction)
		{
			RegisteredCommands.Add(command.ToLower(), gtpFunction);
		}

		protected void RegisterCommand(string command, GTPFunction gtpFunction, string type, string label, string parameters)
		{
			RegisteredCommands.Add(command.ToLower(), gtpFunction);
			AnalyzeCommands.Add(new AnalyzeCommand(command, type, label, parameters));
		}

		protected void RegisterAnalyzeCommand(string command, string type, string label, string parameters)
		{
			AnalyzeCommands.Add(new AnalyzeCommand(command, type, label, parameters));
		}

		public GTPInternalResponse ExecuteComand(GTPCommand gtpCommand)
		{
			Timer.Start();

			GTPInternalResponse lGTPInternalResponse = ExecuteComand2(gtpCommand);

			Timer.Stop();

			return lGTPInternalResponse;
		}

		protected GTPInternalResponse ExecuteComand2(GTPCommand gtpCommand)
		{
			GTPFunction lGTPFunction = null;

			RegisteredCommands.TryGetValue(gtpCommand.Command.ToLower(), out lGTPFunction);

			if (lGTPFunction == null)
				return NotImplemented();

			return lGTPFunction(this, gtpCommand);
		}

		public static GTPInternalResponse NotImplemented()
		{
			return new GTPInternalResponse(false, "not implemented");
		}

		public static GTPInternalResponse NotSupportedResponse()
		{
			return new GTPInternalResponse(false, "not supported");
		}

		public static GTPInternalResponse MissingParameterResponse()
		{
			return new GTPInternalResponse(false, "missing parameter");
		}

		public static GTPInternalResponse MissingParametersResponse()
		{
			return new GTPInternalResponse(false, "missing parameters");
		}

		public static GTPInternalResponse InvalidParameterResponse()
		{
			return new GTPInternalResponse(false, "invalid parameter");
		}

		public static GTPInternalResponse TooManyParameters()
		{
			return new GTPInternalResponse(false, "too many parameters");
		}

		protected int At(string point)
		{
			return Board.At(point);
		}

		protected int At(int x, int y)
		{
			return Board.At(x, y);
		}

		protected string ToString(int x, int y)
		{
			return CoordinateSystem.ToString(x, y);
		}

		public static GTPInternalResponse GTPProtocolVersion(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return new GTPInternalResponse(true, "2");
		}

		public static GTPInternalResponse GTPCPUTime(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			double ElapsedSeconds = Timer.ElapsedMilliseconds / 1000;

			return new GTPInternalResponse(true, ElapsedSeconds.ToString());
		}

		public static GTPInternalResponse GTPKnownComand(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			return new GTPInternalResponse(gtpGoBoard.RegisteredCommands.ContainsKey(gtpCommand.GetParameter(0)));
		}

		public static GTPInternalResponse GTPListCommands(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			string lCommands = string.Empty;

			foreach (string lString in gtpGoBoard.RegisteredCommands.Keys)
				lCommands = lCommands + ((lCommands.Length != 0) ? "\n" : "") + lString;

			return new GTPInternalResponse(true, lCommands);
		}

		public static GTPInternalResponse GTPQuit(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.SearchEngine.StopSearch();
			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPEcho(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return new GTPInternalResponse(true, gtpCommand.Parameters);
		}

		public static GTPInternalResponse GTPAnalyzeCommands(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			StringBuilder lCommands = new StringBuilder();

			foreach (AnalyzeCommand lAnalyzeCommand in gtpGoBoard.AnalyzeCommands)
				lCommands.AppendLine(lAnalyzeCommand.Type + "/" + lAnalyzeCommand.Label + "/" + lAnalyzeCommand.Command + " " + lAnalyzeCommand.Parameters);

			return new GTPInternalResponse(true, lCommands.ToString().TrimEnd('\n'));
		}

		public static GTPInternalResponse GTPTimeLeft(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 2)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			double lTimeLeft = 0;

			if (!gtpCommand.GetParameter(1, ref lTimeLeft))
				return InvalidParameterResponse();

			if ((lTimeLeft <= 0))
				return InvalidParameterResponse();

			int lStonesLeft = 0;

			if (gtpCommand.GetParameterCount() >= 3)
				if (!gtpCommand.GetParameter(2, ref lStonesLeft))
					return InvalidParameterResponse();

			if (lColor.IsBoth)
			{
				gtpGoBoard.SearchEngine.TimeLeft[0] = lTimeLeft;
				gtpGoBoard.SearchEngine.TimeLeft[1] = lTimeLeft;
				gtpGoBoard.SearchEngine.StonesLeft[0] = lStonesLeft;
				gtpGoBoard.SearchEngine.StonesLeft[1] = lStonesLeft;
			}
			else
			{
				gtpGoBoard.SearchEngine.TimeLeft[lColor.ToInteger()] = lTimeLeft;
				gtpGoBoard.SearchEngine.StonesLeft[lColor.ToInteger()] = lStonesLeft;
			}

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPName(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return new GTPInternalResponse(true, ProgramName);
		}

		public static GTPInternalResponse GTPVersion(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return new GTPInternalResponse(true, ProgramVersion);
		}

		public static GTPInternalResponse GTPClearBoard(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.Board.ClearBoard();
			gtpGoBoard.SearchEngine.GoalBase = null;
			return new GTPInternalResponse();
		}

		public static GTPInternalResponse GTPGoBoardSize(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			if (gtpCommand.GetParameterCount() > 1)
				return TooManyParameters();

			int lBoardSize = 0;

			if (!gtpCommand.GetParameter(0, ref lBoardSize))
				return InvalidParameterResponse();

			if (!((lBoardSize == 19) || (lBoardSize == 17) || (lBoardSize == 15) || (lBoardSize == 13) || (lBoardSize == 11) || (lBoardSize == 9) || (lBoardSize == 7) || (lBoardSize == 5)))
				return NotSupportedResponse();

			gtpGoBoard.Board.SetBoardSize(lBoardSize);
			gtpGoBoard.SearchEngine.GoalBase = null;

			return new GTPInternalResponse();
		}

		public static GTPInternalResponse GTPUndo(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (!gtpGoBoard.Board.CanUndo())
				return new GTPInternalResponse(false, "unable to undo");

			gtpGoBoard.Board.Undo();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPGGUndo(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			int lUndos = 0;

			if (!gtpCommand.GetParameter(0, ref lUndos))
				return InvalidParameterResponse();

			if (lUndos <= 0)
				return InvalidParameterResponse();

			while (lUndos-- > 0)
				if (!gtpGoBoard.Board.CanUndo())
					return new GTPInternalResponse(false, "unable to complete undo");
				else
					gtpGoBoard.Board.Undo();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPKomi(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			double lKomi = 0;

			if (!gtpCommand.GetParameter(0, ref lKomi))
				return InvalidParameterResponse();

			if (lKomi >= 0)
				gtpGoBoard.Board.Komi = lKomi;

			return new GTPInternalResponse();
		}

		public static GTPInternalResponse GTPGetKomi(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			double lKomi = gtpGoBoard.Board.Komi;
			return new GTPInternalResponse(true, lKomi.ToString());
		}

		public static GTPInternalResponse GTPPlay(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 2)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			string lPoint = gtpCommand.GetParameter(1);

			if (gtpGoBoard.Board.PlayStone((gtpGoBoard.At(lPoint)), lColor, true))
				return new GTPInternalResponse();
			else
				return new GTPInternalResponse(false, "invalid move");
		}

		public static GTPInternalResponse GTPPlaySequence(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() % 2 != 0)
				return MissingParametersResponse();

			for (int i = 0; i < gtpCommand.GetParameterCount() / 2; i++)
			{
				Color lColor = new Color();

				if (!gtpCommand.GetParameter(i * 2, ref lColor))
					return InvalidParameterResponse();

				string lPoint = gtpCommand.GetParameter((i * 2) + 1);

				if (!gtpGoBoard.Board.PlayStone((gtpGoBoard.At(lPoint)), lColor, true))
					return new GTPInternalResponse(false, "invalid move - " + lPoint);
			}

			return new GTPInternalResponse();
		}

		public static GTPInternalResponse GTPBlack(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			if (gtpGoBoard.Board.PlayStone((gtpGoBoard.At(lPoint)), Color.Black, true))
				return new GTPInternalResponse();
			else
				return new GTPInternalResponse(false, "invalid move");
		}

		public static GTPInternalResponse GTPWhite(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			if (gtpGoBoard.Board.PlayStone((gtpGoBoard.At(lPoint)), Color.White, true))
				return new GTPInternalResponse();
			else
				return new GTPInternalResponse(false, "invalid move");
		}

		public static GTPInternalResponse GTPShowBoard(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return new GTPInternalResponse(true, "Board:\n" + gtpGoBoard.Board.ToString().TrimEnd('\n'));
		}

		public static GTPInternalResponse GTPLoadSGF(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			int lMoves = 0;

			if (gtpCommand.GetParameterCount() >= 2)
				if (!gtpCommand.GetParameter(1, ref lMoves))
					return InvalidParameterResponse();

			if (lMoves < 0)
				return InvalidParameterResponse();

			string lFullPath = (!string.IsNullOrEmpty(gtpGoBoard.Directory)) ?
				gtpGoBoard.Directory + Path.DirectorySeparatorChar + gtpCommand.GetParameter(0)
				: gtpCommand.GetParameter(0);

			SGFCollection lSGFCollection = new SGFCollection(lFullPath);

			if (lSGFCollection.IsError())
				return new GTPInternalResponse(false, "unable to load file");

			GameRecord lGameRecord = new GameRecord();

			if (!lSGFCollection.RetrieveGame(lGameRecord))
				return new GTPInternalResponse(false, "unable to load file");

			if (!GameRecordBoardAdapter.Apply(lGameRecord, gtpGoBoard.Board, true))
//			if (!lGameRecord.Apply(gtpGoBoard.Board, true, lMoves))
				return new GTPInternalResponse(false, "unable to load file");

			return new GTPInternalResponse(true, "sgf loaded");
		}

		public static GTPInternalResponse GTPQueryBoardSize(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return new GTPInternalResponse(true, gtpGoBoard.Board.BoardSize.ToString());
		}

		public GTPInternalResponse GTPWhatColor(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			string lPoint = gtpCommand.GetParameter(0);

			Color lColor = gtpGoBoard.Board.Cells[gtpGoBoard.At(lPoint)].Color;

			if (lColor.IsEmpty)
				return new GTPInternalResponse(true, "empty");
			else
				if (lColor.IsBlack)
					return new GTPInternalResponse(true, "black");
				else
					if (lColor.IsWhite)
						return new GTPInternalResponse(true, "white");

			return new GTPInternalResponse(false, "unknown error");
		}

		public static GTPInternalResponse GTPIsLegal(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 2)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			string lPoint = gtpCommand.GetParameter(1);

			return new GTPInternalResponse(true, gtpGoBoard.Board.IsLegal(gtpGoBoard.At(lPoint), lColor) ? "1" : "0");
		}

		public GTPInternalResponse GTPAllLegal(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			StringBuilder s = new StringBuilder(512);

			for (int x = 0; x < Board.BoardSize; x++)
				for (int y = 0; y < Board.BoardSize; y++)
					if (gtpGoBoard.Board.IsLegal(gtpGoBoard.At(x, y), lColor))
					{
						if (s.Length != 0)
							s.Append(' ');

						s.Append(gtpGoBoard.ToString(x, y));
					}

			return new GTPInternalResponse(true, s.ToString());
		}

		public static GTPInternalResponse GTPListStones(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			StringBuilder s = new StringBuilder(512);

			for (int x = 0; x < gtpGoBoard.Board.BoardSize; x++)
				for (int y = 0; y < gtpGoBoard.Board.BoardSize; y++)
					if (gtpGoBoard.Board.Cells[gtpGoBoard.At(x, y)].Color == lColor)
					{
						if (s.Length != 0)
							s.Append(' ');

						s.Append(gtpGoBoard.ToString(x, y));
					}

			return new GTPInternalResponse(true, s.ToString());
		}

		public static GTPInternalResponse GTPCaptures(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			return new GTPInternalResponse(true, gtpGoBoard.Board.CapturedStoneCnt[lColor.ToInteger()].ToString());
		}

		public static GTPInternalResponse GTPSetFreeHandicap(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.Board.ClearBoard();
			gtpGoBoard.SearchEngine.GoalBase = null;

			foreach (string lPoint in gtpCommand.ParameterParts)
				if (!gtpGoBoard.Board.PlayStone(gtpGoBoard.At(lPoint), Color.Black, false))
					return new GTPInternalResponse(false, "unknown error");

			return new GTPInternalResponse();
		}


		public GTPInternalResponse FixedHandicap(int handicap)
		{
			return new GTPInternalResponse(false, "not implemented");

			/*
					GoBoard.ClearBoard();

					GoBoard.PlaceFixedHandicap(pHandicap);

					string s;

					for (int x = 0; x < GoBoard.BoardSize; x++)
						for (int y = 0; y < GoBoard.BoardSize; y++)
							if (!GoBoard.IsEmpty(At(x, y)))
							{
								if (!s.Length == 0)
									s.Add(' ');

								s.Add(Point(x, y).ToString());
							}

					return new GTPInternalResponse(true, s);
		 */
		}

		public static GTPInternalResponse GTPUnconditionalStatus(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			return new GTPInternalResponse(true, gtpGoBoard.Board.GetSafetyStatus((gtpGoBoard.At(lPoint))).GTPString);
		}

		public static GTPInternalResponse GTPCountLib(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			int lLiberties = gtpGoBoard.Board.GetBlockLibertyCount((gtpGoBoard.At(lPoint)));

			return new GTPInternalResponse(true, lLiberties.ToString());
		}

		public static GTPInternalResponse GTPRegMove(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			MoveList lMoveList = gtpGoBoard.SearchEngine.SimpleSearch(lColor);

			int lMove = lMoveList.GetBestMove();

			return new GTPInternalResponse(true, gtpGoBoard.Board.Coord.ToString(lMove));
		}

		public static GTPInternalResponse GTPGenMove(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			MoveList lMoveList = gtpGoBoard.SearchEngine.SimpleSearch(lColor);
			int lMove = lMoveList.GetBestMove();

			gtpGoBoard.Board.PlayStone(lMove, lColor, true);

			return new GTPInternalResponse(true, gtpGoBoard.Board.Coord.ToString(lMove));
		}

		public static GTPInternalResponse GTPGenMoveCleanUp(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			MoveList lMoveList = gtpGoBoard.SearchEngine.SimpleEndGameSearch(lColor);
			int lMove = lMoveList.GetBestMove();

			gtpGoBoard.Board.PlayStone(lMove, lColor, true);

			return new GTPInternalResponse(true, gtpGoBoard.Board.Coord.ToString(lMove));
		}

		public static GTPInternalResponse GTPFinalStatusList(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lString = gtpCommand.GetParameter(0).ToLower().Trim();

			SafetyFlag lSafetyFlag;

			switch (lString)
			{
				case "dead": lSafetyFlag = SafetyFlag.Dead; break;
				case "alive": lSafetyFlag = SafetyFlag.Alive; break;
				case "white_territory": lSafetyFlag = SafetyFlag.White | SafetyFlag.Territory; break;
				case "black_territory": lSafetyFlag = SafetyFlag.Black | SafetyFlag.Territory; break;
				case "w_territory": lSafetyFlag = SafetyFlag.White | SafetyFlag.Territory; break;
				case "b_territory": lSafetyFlag = SafetyFlag.Black | SafetyFlag.Territory; break;
				case "dame": lSafetyFlag = SafetyFlag.Dame; break;
				case "seki": lSafetyFlag = SafetyFlag.Seki; break;
				case "undecided": lSafetyFlag = SafetyFlag.Undecided; break;
				case "unsurroundable": lSafetyFlag = SafetyFlag.Unsurroundable; break;
				default: return new GTPInternalResponse(false, "error");
			}

			StringBuilder s = new StringBuilder(512);

			for (int x = 0; x < gtpGoBoard.Board.BoardSize; x++)
				for (int y = 0; y < gtpGoBoard.Board.BoardSize; y++)

					if (gtpGoBoard.Board.GetSafetyStatus((gtpGoBoard.At(x, y))).CompareTo(lSafetyFlag))
					{
						if (s.Length != 0)
							s.Append(' ');

						s.Append(gtpGoBoard.ToString(x, y));
					}

			return new GTPInternalResponse(true, s.ToString());
		}

		protected static GTPInternalResponse GTPTopMoves(GTPGoBoard GTPGoBoard, Color color)
		{
			MoveList lMoveList = GTPGoBoard.SearchEngine.SimpleSearch(color);

			lMoveList.QuickSort();

			StringBuilder s = new StringBuilder(512);
			foreach (int lMove in lMoveList)
				s.Append(GTPGoBoard.Board.Coord.ToString(lMove) + " " + (lMoveList.GetValue(lMove)).ToString() + " ");

			return new GTPInternalResponse(true, s.ToString().Trim());
		}

		public static GTPInternalResponse GTPTopMoves(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			return GTPTopMoves(gtpGoBoard, lColor);
		}

		public static GTPInternalResponse GTPTopMovesBlack(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return GTPTopMoves(gtpGoBoard, Color.Black);
		}

		public static GTPInternalResponse GTPTopMovesWhite(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			return GTPTopMoves(gtpGoBoard, Color.White);
		}

		public static GTPInternalResponse GTPFinalScore(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			double lScore = gtpGoBoard.ScoreInterface.GetScore(gtpGoBoard.Board);

			return new GTPInternalResponse(true, Score.ToString(lScore));
		}

		public static GTPInternalResponse GTPTestScoreNow(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			int lScore = SimpleBoardEvaluator.EvaulateBoardPosition(gtpGoBoard.Board);

			return new GTPInternalResponse(true, lScore.ToString());
		}

		public static GTPInternalResponse GTPScoringSystem(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lString = gtpCommand.GetParameter(0);

			if (ScoreFactoryFactory.ToType(lString) == ScoreType.Unassigned)
				return InvalidParameterResponse();

			gtpGoBoard.ScoreInterface = ScoreFactoryFactory.CreateFactory(lString);

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPLevel(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			int lLevel = 0;

			if (!gtpCommand.GetParameter(0, ref lLevel))
				return InvalidParameterResponse();

			if ((lLevel < 0) || (lLevel > 50))
				return InvalidParameterResponse();
			else
				gtpGoBoard.SearchEngine.SearchOptions.MaxPly = lLevel;

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPMaxTime(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			int lMaxTime = 0;

			if (!gtpCommand.GetParameter(0, ref lMaxTime))
				return InvalidParameterResponse();

			if ((lMaxTime < 0))
				return InvalidParameterResponse();
			else
				gtpGoBoard.SearchEngine.SearchOptions.MaxSeconds = lMaxTime;

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPPatternValues(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			PatternMap lPatternMap = gtpGoBoard.SearchEngine.SearchOptions.PatternDetector.FindPatterns(gtpGoBoard.Board, lColor);

			StringBuilder s = new StringBuilder(512);

			for (int lPosition = 0; lPosition < gtpGoBoard.Board.Coord.BoardArea; lPosition++)
			{
				int lValue = lPatternMap.GetValue(lPosition);

				if (lValue != 0)
					s.Append(gtpGoBoard.Board.Coord.ToString(lPosition) + " " + (lValue).ToString() + " ");

			}

			return new GTPInternalResponse(true, s.ToString());
		}

		public static GTPInternalResponse GTPGoalKill(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			gtpGoBoard.SearchEngine.GoalBase = new GoalNot(new GoalSave(gtpGoBoard.Board, gtpGoBoard.At(lPoint)));

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPGoalClear(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.SearchEngine.GoalBase = null;

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPGoalSave(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			gtpGoBoard.SearchEngine.GoalBase = new GoalSave(gtpGoBoard.Board, gtpGoBoard.At(lPoint));

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPSafetySolver(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() != 1)
				return MissingParametersResponse();

			string lMethod = gtpCommand.GetParameter(0);

			gtpGoBoard.Board.SetSafetySolver(SafetySolverFactory.ToType(lMethod));

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPSearchMethod(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() != 1)
				return MissingParametersResponse();

			string lMethod = gtpCommand.GetParameter(0);

			if (SearchMethodFactory.ToType(lMethod) == SearchMethodType.Unassigned)
				return InvalidParameterResponse();

			gtpGoBoard.SearchEngine.SetSearchMethod(SearchMethodFactory.ToType(lMethod));

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPExSafe(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			string lMethod = string.Empty;

			if (gtpCommand.GetParameterCount() == 2)
				lMethod = gtpCommand.GetParameter(1);

			SafetySolverType lSafetySolverType = SafetySolverFactory.ToType(lMethod);

			int lCount = gtpGoBoard.Board.CountSafePoints(lColor, lSafetySolverType);

			return new GTPInternalResponse(true, lCount.ToString());
		}

		public static GTPInternalResponse GTPDumcolorEnclosedRegion(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			string lPoint = gtpCommand.GetParameter(0);

			string lResponse = gtpGoBoard.Board.ToStringEnclosedBlockDump(gtpGoBoard.At(lPoint));

			return new GTPInternalResponse(true, lResponse);
		}

		public static GTPInternalResponse GTPProtectedLiberties(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			Color lColor = new Color();

			if (!gtpCommand.GetParameter(0, ref lColor))
				return InvalidParameterResponse();

			StringBuilder s = new StringBuilder(512);

			for (int x = 0; x < gtpGoBoard.Board.BoardSize; x++)
				for (int y = 0; y < gtpGoBoard.Board.BoardSize; y++)
					if (gtpGoBoard.Board.IsProtectedLiberty((gtpGoBoard.At(x, y)), lColor))
					{
						if (s.Length != 0)
							s.Append(' ');

						s.Append(gtpGoBoard.ToString(x, y));
					}

			return new GTPInternalResponse(true, s.ToString());
		}

		public static GTPInternalResponse GTPClearGames(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.GameRecords = new GameRecords();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPLoadGames(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			string lFullPath = (!string.IsNullOrEmpty(gtpGoBoard.Directory)) ?
				gtpGoBoard.Directory + Path.DirectorySeparatorChar + gtpCommand.GetParameter(0)
				: gtpCommand.GetParameter(0);

			string lMask = "*.*";

			if (gtpCommand.GetParameterCount() >= 2)
				lMask = gtpCommand.GetParameter(1);

			bool lVariations = false;

			if (gtpCommand.GetParameterCount() >= 3)
				if (!gtpCommand.GetParameter(2, ref lVariations))
					return InvalidParameterResponse();

			if (!gtpGoBoard.GameRecords.Load(lFullPath, lMask, lVariations))
				return new GTPInternalResponse(false, "unable to load games");

			return new GTPInternalResponse(true, "loaded");
		}

		public static GTPInternalResponse GTPFilterGames(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			gtpGoBoard.GameFilterFunction = GameRecordFilter.ToFunction(gtpCommand.GetParameter(0));

			if (gtpGoBoard.GameFilterFunction == null)
				return new GTPInternalResponse(false, "unknown game filter");

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPDumpFilterResults(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.GameRecordResults = GameRecordFilter.Filter(gtpGoBoard.GameRecords, gtpGoBoard.GameFilterFunction);

			StringBuilder s = new StringBuilder(512);

			foreach (GameRecordFilter.Result lResult in gtpGoBoard.GameRecordResults)
				s.AppendLine(lResult.GameRecord.GameName + " (" + lResult.MoveNbr.ToString() + ") ->  " + CoordinateSystem.ToString2(lResult.GameRecord[lResult.MoveNbr + 1].Move, lResult.GameRecord.BoardSize));

			return new GTPInternalResponse(true, s.ToString());
		}

		public static GTPInternalResponse GTPClearPatterns(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.SearchEngine.SearchOptions.PatternDetector = new PatternDetector();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPLoadPatterns(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			string lFullPath = (!string.IsNullOrEmpty(gtpGoBoard.Directory)) ?
				gtpGoBoard.Directory + Path.DirectorySeparatorChar + gtpCommand.GetParameter(0)
				: gtpCommand.GetParameter(0);

			PatternCollection lPatternCollection = new PatternCollection(lFullPath);

			if (!lPatternCollection.Load(lFullPath))
				return new GTPInternalResponse(false, "unable to load patterns");

			gtpGoBoard.SearchEngine.SearchOptions.PatternDetector.Add(lPatternCollection);

			return new GTPInternalResponse(true, "loaded");
		}

		public static GTPInternalResponse GTPCGOSConnect(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 2)
				return MissingParameterResponse();

			string lName = gtpCommand.GetParameter(0);
			string lPwd = gtpCommand.GetParameter(1);

			int lNbrGames = 1;

			if (gtpCommand.GetParameterCount() >= 3)
				if (!gtpCommand.GetParameter(2, ref lNbrGames))
					return InvalidParameterResponse();

			if (lNbrGames <= 0)
				return InvalidParameterResponse();

			GTPCommCGOS lGTPCommCGOS = new GTPCommCGOS(gtpGoBoard.CGOSAddress, gtpGoBoard.CGOSPortNbr, lName, lPwd, lNbrGames, true);

			//			if (!lGTPCommCGOS.Connected)
			//				return new GTPInternalResponse(false, "unable to connect");

			GTPEngine lGTPEngine = new GTPEngine(gtpGoBoard, lGTPCommCGOS);

			lGTPCommCGOS.Run();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPCGOSServer(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParameterResponse();

			gtpGoBoard.CGOSAddress = gtpCommand.GetParameter(0);

			if (gtpCommand.GetParameterCount() < 2)
				if (!gtpCommand.GetParameter(1, ref gtpGoBoard.CGOSPortNbr))
					return InvalidParameterResponse();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPPonderOption(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			bool lPonder = false;

			if (gtpCommand.GetParameterCount() < 1)
				lPonder = true;
			else
				if (!gtpCommand.GetParameter(0, ref lPonder))
					return InvalidParameterResponse();

			gtpGoBoard.SearchEngine.SearchOptions.PonderOnOpponentsTime = lPonder;

			if (!lPonder)
				gtpGoBoard.SearchEngine.StopSearch();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPContinueThinkingOption(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			bool lContinue = false;

			if (gtpCommand.GetParameterCount() < 1)
				lContinue = true;
			else
				if (!gtpCommand.GetParameter(0, ref lContinue))
					return InvalidParameterResponse();

			gtpGoBoard.SearchEngine.SearchOptions.ContinueThinkingAfterTimeOut = lContinue;

			if (!lContinue)
				gtpGoBoard.SearchEngine.StopSearch();

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPSleep(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			int lSleepTime = 0;

			if (!gtpCommand.GetParameter(0, ref lSleepTime))
				return InvalidParameterResponse();

			Thread.Sleep(new TimeSpan(0,0,lSleepTime));

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPSetStartPly(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			int lStartPly = 0;

			if (!gtpCommand.GetParameter(0, ref lStartPly))
				return InvalidParameterResponse();

			if (lStartPly <= 0)
				return InvalidParameterResponse();

			gtpGoBoard.SearchEngine.SearchOptions.StartPly = lStartPly;

			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPSetTranspositionTableSize(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			if (gtpCommand.GetParameterCount() < 1)
				return MissingParametersResponse();

			int lTranspositionTableSize = 0;

			if (!gtpCommand.GetParameter(0, ref lTranspositionTableSize))
				return InvalidParameterResponse();

			if (lTranspositionTableSize <= 0)
				return InvalidParameterResponse();

			gtpGoBoard.SearchEngine.SearchOptions.TranspositionTableSize = lTranspositionTableSize;

			return new GTPInternalResponse(true);
		}
		
		public static GTPInternalResponse GTPJustThink(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.SearchEngine.JustStartThinking(Color.Black);
			return new GTPInternalResponse(true);
		}

		public static GTPInternalResponse GTPStopThinking(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			gtpGoBoard.SearchEngine.StopSearch();
			return new GTPInternalResponse(true);
		}
		
		public static GTPInternalResponse GTPTest(GTPGoBoard gtpGoBoard, GTPCommand gtpCommand)
		{
			PatternCollection lPatternCollection = new PatternCollection(@"patterns\test.db");
			gtpGoBoard.SearchEngine.SearchOptions.PatternDetector.Add(lPatternCollection);

			gtpGoBoard.SearchEngine.SearchOptions.MaxPly = 3;

			return GTPTopMoves(gtpGoBoard, Color.Black);
		}
	}
}
