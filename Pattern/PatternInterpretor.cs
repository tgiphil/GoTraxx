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
	class PatternInterpretor : ErrorManagement
	{
		public static int TRUE = PatternFunctions.TRUE;
		public static int FALSE = PatternFunctions.FALSE;

		protected PatternCode PatternOperands;

		protected GoBoard Board;
		protected Color Player;
		protected int Transform;
		protected Coordinate OriginPoint;

		protected Pattern Pattern;

		protected int Position;

		public int At(int pX, int pY)
		{
			return Board.At(pX, pY);
		}

		public int At(Coordinate pCoordinate)
		{
			return Board.At(pCoordinate.X, pCoordinate.Y);
		}

		protected PatternToken GetCurrentToken()
		{
			if (Position >= PatternOperands.Count)
				return PatternToken.END;

			return PatternOperands[Position].Token;
		}

		protected PatternToken GetNextToken()
		{
			if ((Position + 1) >= PatternOperands.Count)
				return PatternToken.END;

			return PatternOperands[Position + 1].Token;
		}

		protected PatternFunction GetCurrentFunction()
		{
			return PatternOperands[Position].Function;
		}

		protected int GetTokenInteger()
		{
			return PatternOperands[Position].Integer;
		}

		protected char GetTokenVariable()
		{
			return PatternOperands[Position].Variable;
		}

		protected string CompileErrorLocation()
		{
			return PatternOperands.ToString(Position);
		}

		protected void NextToken()
		{
			Position++;
		}

		protected int EvaulateExpression()
		{
			int lLeft = TRUE;
			int lRight = TRUE;

			while (true)
			{
				PatternToken lLeftToken = GetCurrentToken();

				if (lLeftToken == PatternToken.OPEN_PARA)
				{
					NextToken();
					lLeft = EvaulateExpression();
				}
				else
					if (lLeftToken == PatternToken.INTEGER)
					{
						lLeft = GetTokenInteger();
						NextToken();
					}
					else if (lLeftToken == PatternToken.FUNCTION)
					{
						lLeft = EvaulateFunction();
					}
					else
					{
						return 0;	// ERROR!!!
					}

				PatternToken lOtoken = GetCurrentToken();
				NextToken();

				if ((lOtoken == PatternToken.CLOSE_PARA) || (lOtoken == PatternToken.END))
				{
					return lLeft;	// immediate close (no comparison)
				}

				// short cut boolean evaluation
				if (((lOtoken == PatternToken.AND) && (lLeft == TRUE)) || ((lOtoken == PatternToken.OR) && (lLeft == FALSE)))
				{
					int lPCnt = 1;

					while (lPCnt > 0)
					{
						PatternToken lToken = GetCurrentToken();
						NextToken();

						if (lToken == PatternToken.CLOSE_PARA) lPCnt--;
						else if (lToken == PatternToken.OPEN_PARA) lPCnt++;
						else if (lToken == PatternToken.END) break;

					}

					return (lLeft);
				}

				lRight = EvaulateExpression();

				switch (lOtoken)
				{
					case PatternToken.EQUAL: lLeft = (lLeft == lRight) ? TRUE : FALSE; break;
					case PatternToken.NOT_EQUAL: lLeft = (lLeft != lRight) ? TRUE : FALSE; break;
					case PatternToken.GATHER_THAN: lLeft = (lLeft > lRight) ? TRUE : FALSE; break;
					case PatternToken.LESS_THAN: lLeft = (lLeft < lRight) ? TRUE : FALSE; break;
					case PatternToken.GATHER_THAN_OR_EQUAL: lLeft = (lLeft >= lRight) ? TRUE : FALSE; break;
					case PatternToken.LESS_THAN_OR_EQUAL: lLeft = (lLeft <= lRight) ? TRUE : FALSE; break;
					case PatternToken.AND: lLeft = ((lLeft == TRUE) && (lRight == TRUE)) ? TRUE : FALSE; break;
					case PatternToken.OR: lLeft = ((lLeft == TRUE) || (lRight == TRUE)) ? TRUE : FALSE; break;
					case PatternToken.PLUS: lLeft = (lLeft + lRight); break;
					case PatternToken.MINUS: lLeft = (lLeft - lRight); break;
					case PatternToken.MULTIPLY: lLeft = (lLeft * lRight); break;
					default: return FALSE; // ERROR!
				}

				return lLeft;
			}
		}

		protected int EvaulateFunction()
		{
			PatternFunction lFunction = GetCurrentFunction();

			NextToken();
			NextToken();

			PatternFunctionParameters<int> lPatternFunctionParameters = new PatternFunctionParameters<int>();

			while (true)
			{
				PatternToken lToken = GetCurrentToken();
				int lInt = GetTokenInteger();	// in case this is needed later
				char lVariable = GetTokenVariable();	// in case this is needed later

				NextToken();

				if (lToken == PatternToken.COMMA)
					continue;

				if (lToken == PatternToken.CLOSE_PARA)
					break;
				
				if (lToken == PatternToken.INTEGER)
					lPatternFunctionParameters.Add(lInt);
				else if (lToken == PatternToken.VARIABLE)
				{
					Coordinate lCoordinate = OriginPoint + (Pattern.GetLetterLocation(lVariable) - Pattern.Origin).Transform(Transform);

					lPatternFunctionParameters.Add(Board.Coord.At(lCoordinate.X, lCoordinate.Y));
				}
				else
					lPatternFunctionParameters.Add(EvaulateExpression());
			}

			// Call function
			return lFunction(Board, Player, lPatternFunctionParameters);
		}

		public PatternInterpretor(PatternCode patternOperands, Pattern pattern)
		{
			Position = 0;
			PatternOperands = patternOperands;
			Pattern = pattern;
		}

		public int Execute(GoBoard goBoard, Color player, Coordinate originPoint, int transform)
		{
			Board = goBoard;
			Player = player;
			OriginPoint = originPoint;
			Transform = transform;
			ClearErrorMessages();

			Position = 0;

			if (PatternOperands.Count == 0)
				return TRUE;

			return EvaulateExpression();
		}

	}
}
