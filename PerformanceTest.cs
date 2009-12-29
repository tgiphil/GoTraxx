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
	class PerformanceTest
	{
		public static void GamePlayTest(int tests, bool withUndo, bool withSolver)
		{
			GoBoard lGoBoard = new GoBoard(19);
			GameRecord lGameRecord = new GameRecord();

			SGFCollection lSGFCollection = new SGFCollection();
			lSGFCollection.LoadSGFFromMemory(SGFGameSamples.GAME_1993_ZHONG_JIALIN_HANE_YASUMASA_1);
			lSGFCollection.RetrieveGame(lGameRecord, 0);

			SimpleTimer lSimpleTimer = new SimpleTimer();

			for (int i = 0; i < tests; i++)
			{
				GameRecordBoardAdapter.Apply(lGameRecord, lGoBoard, true);
				//lGameRecord.Apply(lGoBoard, true);

				if (withSolver)
				{
					int lSafePoints = lGoBoard.CountSafePoints(Color.Black);
					lSafePoints += lGoBoard.CountSafePoints(Color.White);
				}

				if (withUndo)
					while (lGoBoard.CanUndo())
						lGoBoard.Undo();
			}

			lSimpleTimer.Stop();

			Console.Write("19x19 Game [ ");
			Console.Write((withUndo ? "+Undo " : ""));
			Console.Write((withSolver ? "+Solver " : ""));
			Console.Write("] # "+tests.ToString()+" times. ");
			Console.Write("Elapsed: " + lSimpleTimer.MilliSecondsElapsed.ToString() + " ms ");
			Console.WriteLine("Avg.: " + (lSimpleTimer.MilliSecondsElapsed / tests).ToString() + " ms ");

			return;
		}

		public static void GamePlayTestSolver(int tests, bool withSolver)
		{
			GoBoard lGoBoard = new GoBoard(19);
			GameRecord lGameRecord = new GameRecord();

			SGFCollection lSGFCollection = new SGFCollection();
			lSGFCollection.LoadSGFFromMemory(SGFGameSamples.GAME_1993_ZHONG_JIALIN_HANE_YASUMASA_1);
			lSGFCollection.RetrieveGame(lGameRecord, 0);

			SimpleTimer lSimpleTimer = new SimpleTimer();

			GameRecordBoardAdapter.Apply(lGameRecord, lGoBoard, true);
			//lGameRecord.Apply(lGoBoard, true);

			for (int i = 0; i < tests; i++)
			{
				if (withSolver)
				{
					lGoBoard.SafetyStatusMap = null;
					int lSafePoints = lGoBoard.CountSafePoints(Color.Black);
					lSafePoints += lGoBoard.CountSafePoints(Color.White);
				}
			}

			lSimpleTimer.Stop();

			Console.Write("19x19 Game - Solver [ ");
			Console.Write("] # " + tests.ToString() + " times. ");
			Console.Write("Elapsed: " + lSimpleTimer.MilliSecondsElapsed.ToString() + " ms ");
			Console.WriteLine("Avg.: " + (lSimpleTimer.MilliSecondsElapsed / tests).ToString() + " ms ");

			return;
		}
	}
}
