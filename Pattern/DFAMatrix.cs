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
	interface IPatternAddInterface
	{
		void Add(Pattern pattern, int transformation, Coordinate origin, int location);
	}

	class DFAMatrix : ErrorManagement
	{
		public static string[] Buckets = new string[] { "?$.ox*", "?$Oo", "?$XxY", "#$+-|=" };

		public List<DFANode> DFANodes;

		protected static int GetDestination(char c)
		{
			switch (c)
			{
				case '.': return 0;
				case 'O': return 1;
				case 'X': return 2;
				case '#': return 3;
				default: return 3;	// error
			}
		}

		protected void Product(DFAMatrix dfaLeft, int left, DFAMatrix dfaRight, int right, DFAMatrixCache cache)
		{
			int lState = DFANodes.Count - 1;

			AttributeUnion(DFANodes[lState], dfaLeft.DFANodes[left], dfaRight.DFANodes[right]);

			for (int c = 0; c != 4; c++)
			{
				int lNextL = dfaLeft.DFANodes[left][c];
				int lNextR = dfaRight.DFANodes[right][c];

				if ((lNextL != 0) || (lNextR != 0))
				{
					int lNewValue = cache.Search(lNextL, lNextR);

					if (lNewValue == 0)
					{
						// create it
						int lLastState = DFANodes.Count;
						//DFANodes.Add(new DFANode(DFANodes[lState].Level + 1));
						DFANodes.Add(new DFANode());

						cache.Add(lNextL, lNextR, lLastState);

						DFANodes[lState][c] = lLastState;

						Product(dfaLeft, lNextL, dfaRight, lNextR, cache);
					}
					else
					{
						// link to it
						DFANodes[lState][c] = lNewValue;
					}
				}
				else
				{
					DFANodes[lState][c] = 0;
				}
			}
		}

		protected void Product(DFAMatrix dfaLeft, int left, DFAMatrix dfaMiddle, int middle, DFAMatrix dfaRight, int right, DFAMatrixCache3 cache)
		{
			int lState = DFANodes.Count - 1;

			AttributeUnion(DFANodes[lState], dfaLeft.DFANodes[left], dfaMiddle.DFANodes[middle], dfaRight.DFANodes[right]);

			for (int c = 0; c != 4; c++)
			{
				int lNextL = dfaLeft.DFANodes[left][c];
				int lNextM = dfaMiddle.DFANodes[middle][c];
				int lNextR = dfaRight.DFANodes[right][c];

				if ((lNextL != 0) || (lNextM != 0) || (lNextR != 0))
				{
					int lNewValue = cache.Search(lNextL, lNextM, lNextR);

					if (lNewValue == 0)
					{
						// create it
						int lLastState = DFANodes.Count;
						DFANodes.Add(new DFANode());

						cache.Add(lNextL, lNextM, lNextR, lLastState);

						DFANodes[lState][c] = lLastState;
						Product(dfaLeft, lNextL, dfaMiddle, lNextM, dfaRight, lNextR, cache);
					}
					else
					{
						// link to it
						DFANodes[lState][c] = lNewValue;
					}
				}
				else
				{
					DFANodes[lState][c] = 0;
				}
			}
		}

		protected void AttributeUnion(DFANode parent, DFANode left, DFANode right)
		{
			if (left.Count > right.Count)
			{
				parent.Add(left.Attributes);
				parent.Add(right.Attributes);
			}
			else
			{
				parent.Add(left.Attributes);
				parent.Add(right.Attributes);
			}
		}

		protected void AttributeUnion(DFANode parent, DFANode left, DFANode middle, DFANode right)
		{
			parent.Add(left.Attributes);
			parent.Add(middle.Attributes);
			parent.Add(right.Attributes);
		}

		public DFAMatrix(PatternKey pattern)
		{
			CreatePattern(pattern);
		}

		protected void CreatePattern(PatternKey pattern)
		{
			string lDFA = (new DFAPattern(pattern.Pattern, pattern.Transformation)).DFA;

			DFANodes = new List<DFANode>(lDFA.Length + 1);
			DFANodes.Add(new DFANode());

			for (int lState = 0; lState < lDFA.Length; lState++)
			{
				DFANode lDFANode = new DFANode();

				char c = lDFA[lState];

				for (int z = 0; z < 4; z++)
					if (Buckets[z].IndexOf(c) >= 0)
						lDFANode[z] = lState + 2;

				DFANodes.Add(lDFANode);
			}

			DFANode lDFANodeLast = new DFANode();
			lDFANodeLast.Add(pattern);
			DFANodes.Add(lDFANodeLast);
		}


		public DFAMatrix(DFAMatrix dfaMatrixA, DFAMatrix dfaMatrixB)
		{
			Merge(dfaMatrixA, dfaMatrixB);
		}

		public DFAMatrix(DFAMatrix dfaMatrixA, DFAMatrix dfaMatrixB, DFAMatrix dfaMatrixC)
		{
			Merge(dfaMatrixA, dfaMatrixB, dfaMatrixC);
		}

		protected void Merge(DFAMatrix dfaMatrixA, DFAMatrix dfaMatrixB)
		{
			DFANodes = new List<DFANode>(Compare.Max<int>(dfaMatrixA.DFANodes.Count, dfaMatrixB.DFANodes.Count) + 128);

			DFANodes.Add(new DFANode());
			DFANodes.Add(new DFANode());

			DFAMatrixCache lCache = new DFAMatrixCache(DFANodes.Capacity);

			Product(dfaMatrixA, 1, dfaMatrixB, 1, lCache);
		}

		protected void Merge(DFAMatrix dfaMatrixA, DFAMatrix dfaMatrixB, DFAMatrix dfaMatrixC)
		{
			DFANodes = new List<DFANode>(Compare.Max<int>(dfaMatrixA.DFANodes.Count, dfaMatrixB.DFANodes.Count, dfaMatrixC.DFANodes.Count) + 128);

			DFANodes.Add(new DFANode());
			DFANodes.Add(new DFANode());

			DFAMatrixCache3 cache = new DFAMatrixCache3(DFANodes.Capacity);

			Product(dfaMatrixA, 1, dfaMatrixB, 1, dfaMatrixC, 1, cache);
		}

		public int GetPatterns(int state, char value, List<PatternKey> patternList)
		{
			if (DFANodes[state].Attributes != null)
				foreach (PatternKey lPattern in DFANodes[state].Attributes)
					patternList.Add(lPattern);

			return DFANodes[state][DFAMatrix.GetDestination(value)];
		}

		public int GetPatterns(int state, char value, IPatternAddInterface patternList, Coordinate origin,  int location)
		{
			if (DFANodes[state].Attributes != null)
				foreach (PatternKey lPattern in DFANodes[state].Attributes)
					patternList.Add(lPattern.Pattern, lPattern.Transformation, origin, location);

			return DFANodes[state][DFAMatrix.GetDestination(value)];
		}

		public DFAMatrix(string filename)
		{
			MemFile lMemFile = new MemFile(filename);

			if (lMemFile.IsError())
			{
				SetErrorMessage(lMemFile);
				return;
			}

			PatternCollection lPatternCollection = new PatternCollection(lMemFile);

			if (lPatternCollection.IsError())
				SetErrorMessage(lPatternCollection);

			_LoadMatrix(lMemFile, lPatternCollection);
		}

		protected bool _LoadMatrix(MemFile memFile, PatternCollection patternCollection)
		{
			try
			{
				int lMatrixSize = Convert.ToInt32(memFile.ReadLine());

				DFANodes = new List<DFANode>(lMatrixSize);

				for (int i = 1; i < lMatrixSize; i++)
				{
					//			Console.Error.Write(memFile.LineNbr.ToString() + " - " + i.ToString() + " - "+lMatrixSize.ToString());

					DFANode lDFANode = new DFANode();

					for (int z = 0; z < 4; z++)
						lDFANode[z] = Convert.ToInt32(memFile.ReadPart('\t').Trim());

					int lNodePatterns = Convert.ToInt32(memFile.ReadPart('\t').Trim());

					if (lNodePatterns > 0)
						lDFANode.Attributes = new List<PatternKey>(lNodePatterns);

					for (int p = 0; p < lNodePatterns; p++)
					{
						int lPatternTransformation = Convert.ToInt32(memFile.ReadPart(':').Trim());
						string lPatternKey = memFile.ReadPart('\t').Trim();

						int lTransformation = Convert.ToInt32(memFile.ReadPart('\t').Trim());

						Pattern lPattern = patternCollection.FindByKey(lPatternKey); 

						lDFANode.Add(new PatternKey(lPattern, lTransformation));
					}

					memFile.ReadLine();
					DFANodes.Add(lDFANode);
				}
			}
			catch (Exception e)
			{
				return SetErrorMessage("Matrix Error - Line # " + memFile.LineNbr.ToString() + ":", e);
			}

			return false;
		}

		public override string ToString()
		{
			StringBuilder lMatrix = new StringBuilder(1024);

			Dictionary<Pattern, Pattern> lPatternList = new Dictionary<Pattern, Pattern>();

			lMatrix.AppendLine(DFANodes.Count.ToString());

			for (int state = 1; state < DFANodes.Count; state++)
			{
				for (int z = 0; z < 4; z++)
				{
					lMatrix.Append(DFANodes[state][z].ToString());
					lMatrix.Append("\t");
				}

				if (DFANodes[state].Attributes == null)
				{
					lMatrix.Append(0.ToString());
					lMatrix.Append("\t");
				}
				else
				{
					lMatrix.Append(DFANodes[state].Attributes.Count.ToString());
					lMatrix.Append("\t");

					foreach (PatternKey lPattern in DFANodes[state].Attributes)
					{
						if (!lPatternList.ContainsKey(lPattern.Pattern))
							lPatternList.Add(lPattern.Pattern, lPattern.Pattern);

						lMatrix.Append(lPattern.Pattern.UniquePatternName);
						lMatrix.Append("\t");
						lMatrix.Append(lPattern.Transformation.ToString());
						lMatrix.Append("\t");
					}
				}
				lMatrix.AppendLine();
			}

			StringBuilder lPatterns = new StringBuilder(1024);

			foreach (Pattern lPattern in lPatternList.Keys)
				lPatterns.AppendLine(lPattern.ToString());

			StringBuilder lResult = new StringBuilder(lPatterns.Length + lMatrix.Length + 1024);

			lResult.AppendLine("### COMPILED PATTERN MATRIX");
			lResult.AppendLine();
			lResult.AppendLine("### DO NOT MODIFY THIS FILE");
			lResult.AppendLine();
			lResult.AppendLine("### Date: " + DateTime.Now.ToString());
			lResult.AppendLine("### Count: " + lPatternList.Count.ToString());
			lResult.AppendLine();
			lResult.AppendLine("###PATTERNS");
			lResult.AppendLine();
			lResult.AppendLine(lPatterns.ToString());
			lResult.AppendLine();
			lResult.AppendLine("###MATRIX");
			lResult.AppendLine(lMatrix.ToString());

			return lResult.ToString();
		}

		public void Dump()
		{
			Console.Error.WriteLine();
			Console.Error.WriteLine("***DFA MATRIX***");
			Console.Error.WriteLine();

			Console.Error.WriteLine("State  | Level |   .   |   O   |   X   |   #   ");
			Console.Error.WriteLine("===============================================");

			for (int state = 1; state < DFANodes.Count; state++)
			{
				StringBuilder lString = new StringBuilder();

				lString.Append(state.ToString().PadLeft(6));

				lString.Append(" : ");

				//				lString.Append(DFANodes[state].Level.ToString().PadLeft(5));
				lString.Append("N/A".PadLeft(5));

				lString.Append(" : ");

				for (int z = 0; z < 4; z++)
				{
					lString.Append(DFANodes[state][z].ToString().PadLeft(5));

					if (z != 3)
						lString.Append(" | ");
				}

				lString.Append("  - ");
				Console.Error.Write(lString);

				if (DFANodes[state].Attributes != null)
					foreach (PatternKey lPattern in DFANodes[state].Attributes)
					{
						Console.Error.Write(lPattern.Pattern.PatternName + " (" + lPattern.Transformation.ToString() + ") ");
					}

				Console.Error.WriteLine();
			}
			Console.Error.WriteLine();
		}
	}
}
