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

namespace GoTraxx
{
	class GTPRegression
	{

		protected static string Proper(string str)
		{
			string lResult = str.Trim();

			int lAt = lResult.IndexOf(" ");

			if (lAt > 0)
				lResult = lResult.Substring(lAt + 1);

			return lResult.Replace('\n', ' ').Replace('\r', ' ').Trim();
		}

		public static bool ExecuteTests(string directory, string mask, bool subDirectories)
		{
			DirectoryFiles lDirectoryFiles = new DirectoryFiles(directory, mask, subDirectories);

			bool lAllOk = true;

			foreach (string lFile in lDirectoryFiles)
				if (!ExecuteTest(lFile, Path.GetFullPath(lFile)))
					lAllOk = false;

			return lAllOk;
		}

		public static bool ExecuteTest(string filename)
		{
			return ExecuteTest(filename, Path.GetDirectoryName(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + filename));
		}

		public static bool ExecuteTest(string filename, string directory)
		{
			MemFile lMemFile = new MemFile(filename);

			return ExecuteTest(lMemFile, directory);
		}

		public static bool ExecuteTest(MemFile memFile, string directory)
		{
			GoBoard lGoBoard = new GoBoard(19);
			GTPGoBoard lGTPGoBoard = new GTPGoBoard(lGoBoard);
			GTPCommInternal lGTPCommInternal = new GTPCommInternal();
			GTPEngine lGTPEngine = new GTPEngine(lGTPGoBoard, lGTPCommInternal);

			lGTPGoBoard.Directory = directory;

			GTPCommand lGTcommand = new GTPCommand("");
			string lInput = string.Empty;

			int[] lRegressionResults = new int[5];

			while (!memFile.EOF)
			{
				string lBuffer = memFile.ReadLine('\n');

				if ((lBuffer.Length >= 4) && (lBuffer.Substring(0, 4) == "quit"))
					break;

				if ((lBuffer.Length >= 2) && (lBuffer.Substring(0, 2) == "#?"))
				{
					GTPRegressionPattern lPattern = new GTPRegressionPattern(lBuffer);

					GTPRegressionResult lRegressionResult = GTPRegressionPattern.Test(lPattern, lInput);

					lRegressionResults[(int)lRegressionResult]++;

					switch (lRegressionResult)
					{
						case GTPRegressionResult.passed: /* Console.WriteLine("PASSED");*/ break;
						case GTPRegressionResult.PASSED: Console.WriteLine(lGTcommand.ToString()); Console.WriteLine(lGTcommand.CommandNbr.ToString() + " unexpected PASS!"); break;
						case GTPRegressionResult.failed: Console.WriteLine(lGTcommand.ToString()); Console.WriteLine(lGTcommand.CommandNbr.ToString() + " failed: Correct '" + lPattern + "', got '" + Proper(lInput) + "'"); break;
						case GTPRegressionResult.FAILED: Console.WriteLine(lGTcommand.ToString()); Console.WriteLine(lGTcommand.CommandNbr.ToString() + " unexpected failure: Correct '" + lPattern + "', got '" + Proper(lInput) + "'"); break;
						case GTPRegressionResult.ignore: Console.WriteLine(lGTcommand.ToString()); Console.WriteLine(lGTcommand.CommandNbr.ToString() + " ignoring '" + lPattern + "', got '" + Proper(lInput) + "'"); break;
						default: Console.WriteLine("ERROR!!!"); break;
					}
				}
				else
					if (lBuffer.Length > 0)
					{
						GTPCommand lGTcommand2 = new GTPCommand(lBuffer);

						if (lGTcommand2.Command.Length != 0)
						{
							lGTPCommInternal.SendToEngine(lBuffer);
							lGTPCommInternal.SendToEngine("\n");

							lInput = lGTPCommInternal.GetResponse();

							lGTcommand = lGTcommand2;
						}
					}
			}

			for (int lRegressionResultIndex = 0; lRegressionResultIndex < 5; lRegressionResultIndex++)
				Console.WriteLine((GTPRegressionResult)lRegressionResultIndex + " " + lRegressionResults[lRegressionResultIndex]);


			return (lRegressionResults[3] == 0); // false, only for unexpected failures
		}
	}
}
