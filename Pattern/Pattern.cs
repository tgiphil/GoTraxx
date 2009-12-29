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
	class Pattern : ErrorManagement
	{
		public string PatternName;

		public Coordinate Origin;
		protected Coordinate[] LetterLocations;

		protected string SourceCode;

		protected string Transformations; // unused - but recorded
		public string[] PatternAttributes;
		public string ClassificationAttributes;
		protected string[] ActionAttributes;
		public List<string> ActionCode;
		//		protected string ConstraintAttributes; // todo

		protected int EdgeMinX, EdgeMaxX, EdgeMinY, EdgeMaxY;

		protected char[,] PatternMap;
		protected char[,] VariableMap;

		public int Height, Width;
		protected int VariableMapHeight;
		protected bool PatternMapCompleteFlag;
		public int LineNbr;
		protected bool ContainsWild;

		public PatternCompiled PatternCompiled;

		public string PatternSourceCode
		{
			get
			{
				return SourceCode;
			}
		}

		public string UniquePatternName
		{
			get
			{
				return PatternName; // +"%" + LineNbr.ToString();
			}
		}

		public Pattern()
		{
			Height = 0;
			Width = 0;
			Clear();
		}

		public Pattern(string str)
		{
			Clear();
			LoadPattern(str);
		}

		public Pattern(string str, int lineNbr)
		{
			Clear();
			LoadPattern(str);
			LineNbr = lineNbr;
		}

		public static char ToStandard(char c)
		{
			if ((c == '-') || (c == '|') || (c == '=') || (c == '+')) return '#';

			if (c == 'Y') return 'O';	// Y is anchor for opponents stone
			if (c == '*') return '.';	// special case

			return c;
		}

		protected bool IsValid(char c)
		{
			return (".XOxo?*$#|-+=Y".IndexOf(c) >= 0);
		}

		protected bool IsWild(char c)
		{
			return ("xo?$".IndexOf(c) >= 0);
		}

		public int VariationCount(char c)
		{
			return (DFAMatrix.Buckets[0].IndexOf(c) >= 0 ? 1 : 0) +
				(DFAMatrix.Buckets[1].IndexOf(c) >= 0 ? 1 : 0) +
				(DFAMatrix.Buckets[2].IndexOf(c) >= 0 ? 1 : 0) +
				(DFAMatrix.Buckets[3].IndexOf(c) >= 0 ? 1 : 0);
		}

		protected void Clear()
		{
			PatternMap = new char[21, 21];
			VariableMap = new char[21, 21];
			LetterLocations = new Coordinate[26];

			Height = Width = 0;

			for (int x = 0; x < 20; x++)
				for (int y = 0; y < 20; y++)
				{
					PatternMap[x, y] = '?';
					VariableMap[x, y] = ' ';
				}

			Origin = new Coordinate(-1, -1);

			for (int i = 0; i < 26; i++)
				LetterLocations[i] = new Coordinate(-1, -1);

			PatternName = string.Empty;
			SourceCode = string.Empty;

			ActionAttributes = null;
			ActionCode = null;
			PatternAttributes = null;
			Transformations = string.Empty;
			ClassificationAttributes = string.Empty;

			PatternMapCompleteFlag = false;

			VariableMapHeight = 0; // only used during loading

			EdgeMinX = Int32.MinValue;
			EdgeMaxX = Int32.MaxValue;
			EdgeMinY = Int32.MinValue;
			EdgeMaxY = Int32.MaxValue;

			ContainsWild = false;
		}

		protected void FlipOnXAxis()
		{
			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height / 2; y++)
				{
					char t = PatternMap[x, y];
					PatternMap[x, y] = PatternMap[x, Height - y - 1];
					PatternMap[x, Height - y - 1] = t;

					t = VariableMap[x, y];
					VariableMap[x, y] = VariableMap[x, Height - y - 1];
					VariableMap[x, Height - y - 1] = t;
				}

			Origin = new Coordinate(Origin.X, Height - Origin.Y - 1);

			for (int i = 0; i < 26; i++)
				LetterLocations[i] = new Coordinate(LetterLocations[i].X, Height - LetterLocations[i].Y - 1);

			int lEdgeMinY = EdgeMinY;
			int lEdgeMaxY = EdgeMaxY;

			if (lEdgeMaxY == Int32.MaxValue)
				EdgeMinY = Int32.MinValue;
			else
				EdgeMinY = lEdgeMaxY - Height + 1;

			if (lEdgeMinY == Int32.MinValue)
				EdgeMaxY = Int32.MaxValue;
			else
				EdgeMaxY = Height - 1;
		}

		public void SetDimensions(int h, int w)
		{
			Height = h;
			Width = w;
		}

		public void SetCell(int x, int y, char c)
		{
			PatternMap[x, y] = c;

			if (c == '*')
				Origin = new Coordinate(x, y);

			if ((c == '|') || (c == '+'))
				if (x == 0)
					EdgeMinX = 0;
				else
					EdgeMaxX = x;

			if ((c == '-') || (c == '+'))
				if (y == 0)
					EdgeMinY = 0;
				else
					EdgeMaxY = y;

			if (IsWild(c))
				ContainsWild = true;
		}

		protected void SetVariable(int x, int y, char c)
		{
			VariableMap[x, y] = c;

			if ((c >= 'a') && (c <= 'z'))
				LetterLocations[c - 'a'] = new Coordinate(x, y);
		}

		public Coordinate GetLetterLocation(char c)
		{
			if (c == '*')
				return Origin;
			else
				return LetterLocations[c - 'a'];
		}

		protected void SetVariable(Coordinate c, char s)
		{
			SetVariable(c.X, c.Y, s);
		}

		protected void SetCell(Coordinate c, char s)
		{
			SetCell(c.X, c.Y, s);
		}

		public bool IsInPattern(int x, int y)
		{
			if ((x >= Width) || (y >= Height) || (x < 0) || (y < 0))
				return false;

			return true;
		}

		public bool IsInPattern(Coordinate c)
		{
			return IsInPattern(c.X, c.Y);
		}

		public bool IsFullBoardPattern()
		{
			if ((EdgeMinX != 0) || (EdgeMinY != 0) || (EdgeMaxX == Int32.MaxValue) || (EdgeMaxY == Int32.MaxValue) || (EdgeMaxX != EdgeMaxY))
				return false;
			else
				return true;
		}

		public int GetFullBoardSize()
		{
			if (IsFullBoardPattern())
				return EdgeMaxX - EdgeMinX;
			else
				return 0;
		}

		public bool IsFixedFuseki()
		{
			if (ContainsWild)
				return false;

			if (!IsFullBoardPattern())
				return false;

			return true;
		}

		public char GetCell(int x, int y)
		{
			if (IsInPattern(x, y))
				return PatternMap[x, y];
			else
			{
				if ((x >= EdgeMaxX) || (x <= EdgeMinX) || (y >= EdgeMaxY) || (y <= EdgeMinY))
					return '#'; // off the pattern and off the board

				return '$';	/* don't care */
			}
		}

		public char GetVariable(int x, int y)
		{
			if (IsInPattern(x, y))
				return VariableMap[x, y];
			else
				return ' ';
		}

		public char GetCell(Coordinate c)
		{
			return GetCell(c.X, c.Y);
		}

		public char GetVariable(Coordinate c)
		{
			return GetVariable(c.X, c.Y);
		}

		public int GetMaxDimension()
		{
			return Height > Width ? Height : Width;
		}

		protected bool LoadPattern(string str)
		{
			return LoadPattern(str, -1);
		}

		protected bool LoadPattern(string str, int lineNbr)
		{
			Clear();
			LineNbr = lineNbr;

			int linestart = 0;

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				if ((c == 13) || (c == 10))	// newline or carriage return
				{
					string lLine = str.Substring(linestart, i - linestart);

					if (!LoadByLine(lLine))
						return false;

					// reset for next line
					linestart = i + 1;
				}
			}

			FlipOnXAxis();

			PatternCompiled = new PatternCompiled(this);

			if (PatternCompiled.IsError())
				return SetErrorMessage(PatternCompiled);

			return true;
		}

		protected bool LoadByLine(string str)
		{
			if (str.Length < 1)
				return true;	// ignore blank lines

			char s = str[0];

			if ((s == '#') || (s == ' '))	// comment (or space) - ignore line
			{
				return true;
			}

			if (s == ':')
			{
				PatternMapCompleteFlag = true;
				return LoadColonLine(str);
			}

			if (s == ';')
			{
				PatternMapCompleteFlag = true;
				return LoadPatternCode(str);
			}

			if (s == '>')
			{
				PatternMapCompleteFlag = true;
				return LoadPatternActionHelper(str);
			}

			if ((s == 'P') || (s == 'p'))
			{
				return LoadPatternName(str);
			}

			if (Height >= 21)
			{
				return false;
			}

			if (!PatternMapCompleteFlag)
				return LoadPatternByLine(str);
			else
				return LoadVariableByLine(str);
		}

		protected bool LoadColonLine(string str)
		{
			string lString = str.Substring(1).Trim();	// remove : from string

			string[] lWords = lString.Split(',');

			if (lWords.Length > 0)
			{
				Transformations = lWords[0].Trim();

				if (lWords.Length > 1)
				{
					ClassificationAttributes = lWords[1].Trim();

					if (lWords.Length > 2)
					{
						PatternAttributes = new string[lWords.Length - 2];

						for (int i = 2; i < lWords.Length; i++)
							PatternAttributes[i - 2] = lWords[i];
					}
				}
			}

			return true;
		}

		protected bool LoadPatternActionHelper(string str)
		{
			if (ActionCode == null)
				ActionCode = new List<string>();

			ActionCode.Add(str.Substring(1).Trim());

			return true;
		}

		public override string ToString()
		{
			StringBuilder lString = new StringBuilder(1024);

			lString.Append("Pattern ");
			lString.AppendLine(PatternName);

			for (int y = Height - 1; y >= 0; y--)
			{
				for (int x = 0; x < Width; x++)
					lString.Append(GetCell(x, y).ToString());
				lString.AppendLine();
			}

			if (PatternSourceCode.Length != 0)
			{
				lString.Append("; ");
				lString.AppendLine(PatternCompiled.PatternCode.ToString()); //PatternSourceCode);
			}

			if (VariableMapHeight != 0)
				for (int y = Height - 1; y >= 0; y--)
				{
					for (int x = 0; x < Width; x++)
						lString.Append(GetVariable(x, y));
					lString.AppendLine();
				}

			lString.AppendLine(":8," + PatternCompiled.PatternActionAttribute.ToString());

			if (ActionCode != null)
				foreach (string lActionHelper in ActionCode)
				{
					lString.Append("> ");
					lString.AppendLine(lActionHelper);
				}

			return lString.ToString();
		}

		public string ToString2()
		{
			StringBuilder lString = new StringBuilder(1024);

			lString.Append("Pattern: " + PatternName + " - (Height = " + Height.ToString() + ", Width = " + Width.ToString() + ")");
			lString.Append("\n");

			for (int y = Height - 1 + 1; y >= -1; y--)
			{
				if (y <= 9)
					lString.Append(" ");

				lString.Append(y.ToString() + " : ");

				for (int x = -1; x < Width + 1; x++)
					lString.Append(GetCell(x, y).ToString());

				if (VariableMapHeight != 0)
				{
					lString.Append("  :  ");

					for (int x = 0; x < Width; x++)
						lString.Append(GetVariable(x, y));
				}

				if (y == 1)
					if (PatternSourceCode.Length != 0)
						lString.Append(" ; " + PatternSourceCode);

				lString.Append("\n");
			}

			foreach (string lActionHelper in ActionCode)
			{
				lString.Append("> ");
				lString.AppendLine(lActionHelper);
			}

			return lString.ToString();
		}

		protected bool LoadPatternByLine(string str)
		{
			Height++;

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				if ((c == ' ') || (c == '\t'))
					break;

				if (i > 21)
					return false;	// to large for pattern

				if (!IsValid(c))
					return false;

				if (i >= Width)		// expand width
					Width = i + 1;

				SetCell(i, Height - 1, c);
			}

			return true;
		}

		protected bool LoadVariableByLine(string str)
		{
			VariableMapHeight++;

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				if ((c == ' ') || (c == '\t'))
					break;

				if (i > 21)
					return false;	// to large for pattern

				SetVariable(i, VariableMapHeight - 1, c);
			}

			return true;
		}

		protected bool LoadPatternName(string str)
		{
			PatternName = string.Empty;

			if (str.Length < 7)
				return false;

			if (str.Length > 8)
				PatternName = str.Substring(8, str.Length - 8);

			return true;
		}

		protected bool LoadPatternCode(string str)
		{
			SourceCode = string.Empty;

			if (str.Length <= 1)
				return true;

			SourceCode = str.Substring(1, str.Length - 1);

			return true;
		}

		public void Dump()
		{
			Console.WriteLine(ToString());
		}

	}
}
