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
using NUnit.Framework;

namespace GoTraxx
{
	//[TestFixture]
	public class PatternInterpretorTest
	{

		protected class PatternTest
		{
			public string Expression;
			public bool ScanFlag;
			public bool SyntaxFlag;
			public int Result;
			public bool ExecuteFlag;
			public int PatternNbr;

			public PatternTest(string expression, bool scanFlag, bool syntaxFlag, int result, bool executeFlag, int patternNbr)
			{
				Expression = expression;
				ScanFlag = scanFlag;
				SyntaxFlag = syntaxFlag;
				Result = result;
				ExecuteFlag = executeFlag;
				PatternNbr = patternNbr;
			}

			public PatternTest(string expression, bool scanFlag, bool syntaxFlag, bool result, bool executeFlag, int patternNbr)
			{
				Expression = expression;
				ScanFlag = scanFlag;
				SyntaxFlag = syntaxFlag;
				Result = result ? 1 : 0;
				ExecuteFlag = executeFlag;
				PatternNbr = patternNbr;
			}
		};

		//[Test]
		public void Test()
		{

			GoBoard lGoBoard = new GoBoard(13);

			lGoBoard.PlayStone("A2", Color.Black, false);
			lGoBoard.PlayStone("B1", Color.Black, false);
			lGoBoard.PlayStone("B2", Color.Black, false);
			lGoBoard.PlayStone("C2", Color.Black, false);
			lGoBoard.PlayStone("D2", Color.Black, false);
			lGoBoard.PlayStone("E2", Color.Black, false);
			lGoBoard.PlayStone("F2", Color.Black, false);
			lGoBoard.PlayStone("F1", Color.Black, false);
			lGoBoard.PlayStone("D1", Color.Black, false);
			lGoBoard.PlayStone("G8", Color.Black, false);
			lGoBoard.PlayStone("H7", Color.Black, false);
			lGoBoard.PlayStone("H9", Color.Black, false);
			lGoBoard.PlayStone("J8", Color.Black, false);
			lGoBoard.PlayStone("H2", Color.Black, false);
			lGoBoard.PlayStone("J1", Color.Black, false);
			lGoBoard.PlayStone("K2", Color.Black, false);

			lGoBoard.Dump();

			string lString = "Pattern LinkPattern4\n\n??o??\n?XoX?\n..X*.\n-----\n\n:\n\n??o??\n?Xoa?\n.*X*.\n-----\n\n;libertycount(a)>1\n\n";

			Pattern lPattern = new Pattern(lString);

			lPattern.Dump();

			PatternTest[] lPatternTests =   { 				 
	//			new PatternTest( "1+2*3", true, true, 7, true, -1 ),		// bug: right to left (instead of left to right), but okay for now
				new PatternTest( "", true, true, 1, false, -1 ),
				new PatternTest( "", true, true, true, true, -1 ),
				new PatternTest( "1", true, true, 1, true, -1 ),
				new PatternTest( "1+1", true, true, 2, true, -1 ),
				new PatternTest( "(1+1)", true, true, 2, true, -1 ),
				new PatternTest( "1>2", true, true, false, true, -1 ),
				new PatternTest( "(1>2)", true, true, false, true, -1 ),
				new PatternTest( "((1)>(1+2))", true, true, false, true, -1 ),
				new PatternTest( "1+2+3+4+5+6+7+8+9", true, true, 45, true, -1 ),
				new PatternTest( "1+2+(3+4)+5+6+7+(8+9)", true, true, 45, true, -1 ),
				new PatternTest( "(1+2+(3+4)+5+6+7+(8+9))", true, true, 45, true, -1 ),
				new PatternTest( "0&&1&&1", true, true, false, true, -1 ),
				new PatternTest( "1||1||0", true, true, true, true, -1 ),
				new PatternTest( "0&&1&&1||1", true, true, false, true, -1 ),
				new PatternTest( "libertycount(a)", true, true, 4, false, -1 ),
				new PatternTest( "libertycount(a) > 1", true, true, true, false, -1 ),
				new PatternTest( "1", true, true, 1, true, -1 ),
				new PatternTest( "1+1", true, true, 2, true, -1 ),
				new PatternTest( "((1)>(1+2))", true, true, false, true, -1 ),
				new PatternTest( " (1+2) > libertycount(a)", true, true, 0, false, -1 ),
				new PatternTest( " (1+2) > libertycount(a,b)", true, true, 0, false, -1 ),
				new PatternTest( " (1+2) > libertycount()", true, false, 0, false, -1 ),
				new PatternTest( " (1+2) != libertycount()+1", true, false, 0, false, -1 ),
				new PatternTest( " (1+2) >= libertycount()+1+2", true, false, 0, false, -1 ),
				new PatternTest( " (libertycount(a) == 1)", true, true, 0, false, -1 ),
				new PatternTest( " libertycount(a) == 1 && libertycount(b) == 1", true, true, 0, false, -1 ),
				new PatternTest( " (libertycount(a) == 1) || (libertycount(b) == 1)", true, true, 0, false, -1 ),
				new PatternTest( " ((libertycount(a) == 1) || (libertycount(b) == 1))", true, true, 0, false, -1 ),
				new PatternTest( "1+", true, false, 0, true, -1 ),
				new PatternTest( " libertycount(a) == 1)", true, false, 0, false, 100 ),
				new PatternTest( "libertycount(a) >" , true, false, 0, false, 101 ),
				new PatternTest( "(((1)>(1+2))", true, false, 0, true, -1 ),
				new PatternTest( "((1)>(1+2)))", true, false, 0, true, -1 ),
				new PatternTest( " (1+2 > libertycount(a)", true, false, 0, false, -1 ),
				new PatternTest( " 1+2) > libertycount(a,b)", true, false, 0, false, -1 ),
				new PatternTest( " libertycount(a) libertycount(b)", true, false, 0, false, -1 ),
				new PatternTest( " libertycount(a) == 1 libertycount(b)", true, false, 0, false, -1 ),
				new PatternTest( " libertycount(a) libertycount(b) == 1", true, false, 0, false, -1 ),
				new PatternTest(" libertycount(a) == 1 & libertycount(b) == 1", false, false, 0, false, -1 ) 
			};

			foreach (PatternTest lPatternTest in lPatternTests)
			{
				PatternCode lPatternCode = new PatternCode();

				//	PatternScanner lPatternScanner = new PatternScanner(lPatternTest.Expression, lPatternCode);

				bool lScan = !lPatternCode.IsError();

				Assert.IsTrue(lScan == lPatternTest.ScanFlag, "1:" + lPatternTest.Expression);

				if (lScan != lPatternTest.ScanFlag)
				{
					Console.Error.WriteLine("PatternInterpretor::SelfTest (Failed)");
					Console.Error.WriteLine("Failed Test: " + lPatternTest.Expression);
					Console.Error.WriteLine("Scan  : " + (lScan ? "YES" : "NO") + " Expected: " + (lPatternTest.ScanFlag ? "YES" : "NO"));
				}

				if (lScan)
				{
					PatternSyntax lPatternSyntax = new PatternSyntax(lPatternCode);

					bool lSyntax = lPatternSyntax.SyntaxCheck();

					Assert.IsTrue(lSyntax == lPatternTest.SyntaxFlag, "2:" + lPatternTest.Expression);

					if (lSyntax != lPatternTest.SyntaxFlag)
					{
						Console.Error.WriteLine("PatternInterpretor::SelfTest (Failed)");
						Console.Error.WriteLine("Failed Test: " + lPatternTest.Expression);
						Console.Error.WriteLine("Syntax: " + (lSyntax ? "YES" : "NO") + " Expected: " + (lPatternTest.SyntaxFlag ? "YES" : "NO"));
					}

					if ((lSyntax) && (lPatternTest.ExecuteFlag))
					{
						PatternInterpretor lPatternInterpretor = new PatternInterpretor(lPatternCode, lPattern);

						int lResult = lPatternInterpretor.Execute(lGoBoard, Color.Black, new Coordinate('K', 1), 0);

						Assert.IsTrue(lResult == lPatternTest.Result, "3:" + lPatternTest.Expression + " Got: " + lResult.ToString() + " Expected: " + lPatternTest.Result);
						Assert.IsTrue(!lPatternInterpretor.IsError(), "4:Interpretor Error: " + lPatternTest.Expression);

						if ((lResult != lPatternTest.Result) || (lPatternInterpretor.IsError()))
						{
							Console.Error.WriteLine("PatternInterpretor::SelfTest (Failed)");
							Console.Error.WriteLine("Failed Test: " + lPatternTest.Expression);
							Console.Error.Write("Got : " + lResult.ToString());
							Console.Error.WriteLine(" Expected: " + lPatternTest.Result);
						}
					}

					if ((lSyntax) && (!lPatternTest.ExecuteFlag))
					{
						PatternInterpretor lPatternInterpretor = new PatternInterpretor(lPatternCode, lPattern);

						int lResult = lPatternInterpretor.Execute(lGoBoard, Color.Black, new Coordinate('K', 1), 0);

						Assert.IsTrue(lResult == lPatternTest.Result, "5:" + lPatternTest.Expression);

						if ((lResult != lPatternTest.Result) || (lPatternInterpretor.IsError()))
						{
							Console.Error.WriteLine("PatternInterpretor::SelfTest (Failed)");
							Console.Error.WriteLine("Failed Test: " + lPatternTest.Expression);
							Console.Error.Write("Got : " + lResult.ToString());
							Console.Error.WriteLine(" Expected: " + lPatternTest.Result);
						}
					}
				}
			}

		}

	}
}
