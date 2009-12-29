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
	class PatternSyntax : ErrorManagement
	{
		protected PatternCode PatternOperands;

		protected int mPosition;

		protected PatternToken GetCurrentToken()
		{
			if (mPosition >= PatternOperands.Count)
				return PatternToken.END;

			return PatternOperands[mPosition].Token;
		}

		protected PatternToken GetNextToken()
		{
			if ((mPosition + 1) >= PatternOperands.Count)
				return PatternToken.END;

			return PatternOperands[mPosition + 1].Token;
		}

		protected PatternFunction GetCurrentFunction()
		{
			return PatternOperands[mPosition].Function;
		}

		protected int GetTokenInteger()
		{
			return PatternOperands[mPosition].Integer;
		}

		protected string CompileErrorLocation()
		{
			return PatternOperands.ToString(mPosition);
		}

		protected void NextToken()
		{
			mPosition++;
		}

		protected bool SyntaxCheckExpression()
		{
			PatternToken lToken = GetCurrentToken();

			if (lToken == PatternToken.COMMA)
				return SetErrorMessage("Unexpected ',': " + CompileErrorLocation());

			if (lToken == PatternToken.CLOSE_PARA)
				return SetErrorMessage("Unexpected ')': " + CompileErrorLocation());

			if (lToken == PatternToken.FUNCTION)
			{
				if (!SyntaxCheckFunction())
					return false;
			}
			else if (lToken == PatternToken.OPEN_PARA)
			{
				NextToken();

				if (!SyntaxCheckExpression())
					return false;

				lToken = GetCurrentToken();

				if (lToken != PatternToken.CLOSE_PARA)
					return SetErrorMessage("Expected ')': " + CompileErrorLocation());

				NextToken();

			}
			else if (lToken == PatternToken.INTEGER) // || (lToken == PatternToken.REAL))
				NextToken();
			else
				return SetErrorMessage("Unexpected token: " + CompileErrorLocation());

			PatternToken lNextToken = GetCurrentToken();

			if ((lNextToken == PatternToken.EQUAL) ||
				(lNextToken == PatternToken.NOT_EQUAL) ||
				(lNextToken == PatternToken.GATHER_THAN) ||
				(lNextToken == PatternToken.LESS_THAN) ||
				(lNextToken == PatternToken.GATHER_THAN_OR_EQUAL) ||
				(lNextToken == PatternToken.LESS_THAN_OR_EQUAL) ||
				(lNextToken == PatternToken.AND) ||
				(lNextToken == PatternToken.MULTIPLY) ||
				(lNextToken == PatternToken.OR) ||
				(lNextToken == PatternToken.PLUS) ||
				(lNextToken == PatternToken.MINUS))
			{
				NextToken();
				return SyntaxCheckExpression();
			}

			if ((lNextToken == PatternToken.END) || (lNextToken == PatternToken.CLOSE_PARA))
				return true;

			return SetErrorMessage("Expected: " + CompileErrorLocation());
		}

		protected bool SyntaxCheckFunction()
		{
			PatternFunction lFunction = GetCurrentFunction();

			if (lFunction == null)
				return SetErrorMessage("Unknown Function: " + CompileErrorLocation());

			NextToken();
			PatternToken lToken = GetCurrentToken();

			if (lToken != PatternToken.OPEN_PARA)
				return SetErrorMessage("Expected ')': " + CompileErrorLocation());

			NextToken();

			if (!SyntaxCheckParameters())
				return false;

			lToken = GetCurrentToken();

			if (lToken != PatternToken.CLOSE_PARA)
				return SetErrorMessage("Expected ')': " + CompileErrorLocation());

			NextToken();

			return true;
		}

		protected bool SyntaxCheckParameters()
		{
			PatternToken lToken = GetCurrentToken();

			if (lToken == PatternToken.CLOSE_PARA)
				return true;	// no parameters

			if (!SyntaxCheckParameter())
				return false;

			lToken = GetCurrentToken();

			if (lToken == PatternToken.CLOSE_PARA)
				return true;

			if (lToken != PatternToken.COMMA)
				return SetErrorMessage("Expected ',': " + CompileErrorLocation());

			NextToken();
			return SyntaxCheckParameters();
		}

		protected bool SyntaxCheckParameter()
		{
			PatternToken lToken = GetCurrentToken();

			if (lToken == PatternToken.VARIABLE)
			{
				NextToken();
				return true;
			}

			return SyntaxCheckExpression();
		}

		public PatternSyntax(PatternCode patternOperands)
		{
			PatternOperands = patternOperands;
		}

		public bool SyntaxCheck()
		{
			ClearErrorMessages();

			mPosition = 0;

			if (PatternOperands.Count == 0)
				return true;

			if (!SyntaxCheckExpression())
				return false;

			PatternToken lToken = GetCurrentToken();

			if (lToken != PatternToken.END)
				return SetErrorMessage("Unexpected token: " + CompileErrorLocation());

			return true;
		}
	}
}
