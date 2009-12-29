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
	class PatternDetector
	{
		protected DFAMatrix DFAMatrix = null;
		protected PatternCollection PatternCollection = new PatternCollection();

		public PatternCollection Patterns
		{
			get
			{
				return PatternCollection;
			}

		}

		public PatternDetector()
		{
		}

		public PatternDetector(PatternDetector patternDetector)
		{
			lock (patternDetector)
			{
				DFAMatrix = patternDetector.DFAMatrix;
			}
		}

		public PatternDetector(PatternCollection patterns)
		{
			PatternCollection = patterns;
			DFAMatrixBuilder lDFAMatrixBuilder = new DFAMatrixBuilder(patterns);
			DFAMatrix = lDFAMatrixBuilder.GetMatrix();
		}

		public PatternDetector Clone()
		{
			return new PatternDetector(this);
		}

		public void Add(PatternCollection patterns)
		{
			DFAMatrixBuilder lDFAMatrixBuilder = new DFAMatrixBuilder(DFAMatrix);
			lDFAMatrixBuilder.Add(patterns);
			PatternCollection.Add(patterns);

			DFAMatrix lDFAMatrix = lDFAMatrixBuilder.GetMatrix();

			lock (this)
			{
				DFAMatrix = lDFAMatrix;
			}
		}

		public PatternMap FindPatterns(GoBoard goBoard, Color playerToMove)
		{
			PatternMap lPatternMap = new PatternMap(goBoard, playerToMove);

			if (DFAMatrix == null)
				return lPatternMap;

			for (int lOrigin = 0; lOrigin < goBoard.Coord.BoardArea; lOrigin++)
			{
				if (goBoard.GetSafetyStatus(lOrigin).IsUndecided)
					if (goBoard.IsLegal(lOrigin, playerToMove))
					{
						Coordinate lStart = new Coordinate(goBoard.Coord.GetColumn(lOrigin), goBoard.Coord.GetRow(lOrigin));

						int lState = 1;

						Coordinate lSpiral = new Coordinate(0, 0);

						while (true)
						{
							Coordinate lAt = lStart + lSpiral;
							lSpiral.SpiralNext();

							char c = '#';

							if (lAt.IsOnBoard(goBoard.BoardSize))
							{
								c = goBoard.GetColor(goBoard.Coord.At(lAt.X, lAt.Y)).ToChar();

								if (playerToMove.IsBlack)
								{
									// patterns are stored in white moves next 
									// so flip colors when is black's turn
									if (c == 'O') c = 'X'; else if (c == 'X') c = 'O';
								}
							}

							lState = DFAMatrix.GetPatterns(lState, c, lPatternMap, lStart, lOrigin);

							if (lState == 0)
								break;
						}

					}
			}
			return lPatternMap;
		}

		public PatternMap FindPatterns(GoBoard goBoard, Color playerToMove, List<int> legalMoves)
		{
			PatternMap lPatternMap = new PatternMap(goBoard, playerToMove);

			if (DFAMatrix == null)
				return lPatternMap;

			foreach (int lOrigin in legalMoves)
			{
				int lState = 1;

				Coordinate lStart = new Coordinate(goBoard.Coord.GetColumn(lOrigin), goBoard.Coord.GetRow(lOrigin));
				Coordinate lSpiral = new Coordinate(0, 0);

				while (true)
				{
					Coordinate lAt = lStart + lSpiral;
					lSpiral.SpiralNext();

					char c = '#';

					if (lAt.IsOnBoard(goBoard.BoardSize))
					{
						c = goBoard.GetColor(goBoard.Coord.At(lAt.X, lAt.Y)).ToChar();

						if (playerToMove.IsBlack)
						{
							// patterns are stored in white moves next 
							// so flip colors when is black's turn
							if (c == 'O') c = 'X'; else if (c == 'X') c = 'O';
						}

					}

					lState = DFAMatrix.GetPatterns(lState, c, lPatternMap, lStart, lOrigin);

					if (lState == 0)
						break;
				}
			}
			return lPatternMap;
		}

	}
}
