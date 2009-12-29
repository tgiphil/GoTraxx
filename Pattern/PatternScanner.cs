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

	public enum PatternToken : byte
	{
		UNKNOWN = 0,
		CLOSE_PARA,
		OPEN_PARA,
		EQUAL,
		NOT_EQUAL,
		GATHER_THAN,
		LESS_THAN,
		GATHER_THAN_OR_EQUAL,
		LESS_THAN_OR_EQUAL,
		AND,
		OR,
		PLUS,
		MINUS,
		MULTIPLY,
		INTEGER,
		NAME,
		FUNCTION,
		VARIABLE,
		COMMA,
		END
	};

	static class PatternScanner
	{
		public enum TokenCharacterType : byte
		{
			UNKNOWN = 0,
			SYMBOL,
			NUMBER,
			WORD,
			SEPERATOR,
			STAR,
		};

		static bool IsCharNumber(char c)
		{
			return ((c >= '0') && (c <= '9'));
		}

		static bool IsCharWord(char c)
		{
			return ((((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z'))) || (c == '_'));
		}

		static bool IsCharSymbol(char c)
		{
			return ("=!<>+-&|*".IndexOf(c) >= 0);
		}

		static bool IsStar(char c)
		{
			return ("*".IndexOf(c) >= 0);
		}

		static bool IsCharParen(char c)
		{
			return ("()".IndexOf(c) >= 0);
		}

		static bool IsCharComma(char c)
		{
			return (c == ',');
		}

		static TokenCharacterType GetCharType(char c)
		{
			if (IsCharNumber(c))
				return TokenCharacterType.NUMBER;
			else if (IsCharWord(c))
				return TokenCharacterType.WORD;
			else if (IsStar(c))
				return TokenCharacterType.STAR;
			else if (IsCharSymbol(c))
				return TokenCharacterType.SYMBOL;
			else if (IsCharParen(c))
				return TokenCharacterType.SEPERATOR;
			else if (IsCharComma(c))
				return TokenCharacterType.SEPERATOR;

			return TokenCharacterType.UNKNOWN;	/* unknown */
		}

		static string ReadTokenString(string code, ref int position)
		{
			string lToken = string.Empty;

			TokenCharacterType lType = TokenCharacterType.UNKNOWN;

			while (position < code.Length)
			{
				char c = code[position++];

				if (c != ' ')
				{
					TokenCharacterType lCurType = GetCharType(c);

					if (lCurType != TokenCharacterType.UNKNOWN)
					{
						if (lType == TokenCharacterType.UNKNOWN)
						{
							lType = lCurType;
							lToken = lToken + c;

							if (lCurType == TokenCharacterType.SEPERATOR)
								return lToken;
						}
						else if (lType == lCurType)
						{
							lToken = lToken + c;
						}
						else if (lType != lCurType)
						{
							position--;
							return lToken;
						}
					}
				}
			}

			return lToken;
		}

		static PatternToken GetToken(string token)
		{
			return GetToken(token, PatternToken.UNKNOWN);	
		}

		static PatternToken GetToken(string token, PatternToken previousToken)
		{
			if (string.IsNullOrEmpty(token))
				return PatternToken.UNKNOWN;

			TokenCharacterType lType = GetCharType(token[0]);

			if (lType == TokenCharacterType.UNKNOWN)
				return PatternToken.UNKNOWN;

			if (lType == TokenCharacterType.WORD)
				if (token.Length == 1)
					return PatternToken.VARIABLE;	// variables are one character only (at least in this language)
				else
					return PatternToken.FUNCTION;	// functions are always more than on character (in this language)

			if (lType == TokenCharacterType.NUMBER)
				return PatternToken.INTEGER;

			if ((lType == TokenCharacterType.STAR) && (previousToken == PatternToken.COMMA || previousToken == PatternToken.OPEN_PARA))
				return PatternToken.VARIABLE;

			if (PatternTokens.Instance.ContainsKey(token))
				return PatternTokens.Instance[token];

			return PatternToken.UNKNOWN;
		}

		static int GetParameterCount(string code, int position)
		{
			int lParamCnt = 0;
			int lLevel = 0;

			int lPosition = position;

			// skip past the first open para.
			while (lPosition < code.Length)
			{
				string lTokenString = ReadTokenString(code, ref lPosition);
				PatternToken lToken = GetToken(lTokenString);

				if (lToken == PatternToken.OPEN_PARA)
					break;
			}

			while (lPosition < code.Length)
			{
				string lTokenString = ReadTokenString(code, ref lPosition);
				PatternToken lToken = GetToken(lTokenString);

				if (lLevel == 0)
				{
					if (lParamCnt == 0)
						if ((lToken != PatternToken.UNKNOWN) && (lToken != PatternToken.CLOSE_PARA))
							lParamCnt++;

					if (lToken == PatternToken.COMMA)
						lParamCnt++;
					else
						if (lToken == PatternToken.OPEN_PARA)
							lLevel++;
						else
							if (lToken == PatternToken.CLOSE_PARA)
								return lParamCnt;
				}
				else
					if (lToken == PatternToken.OPEN_PARA)
						lLevel++;
					else
						if (lToken == PatternToken.CLOSE_PARA)
							lLevel--;

			}

			return -1;	// something went wrong
		}

		public static PatternCode Scan(string code)
		{
			PatternCode lPatternCode = new PatternCode();
			int lPosition = 0;

			PatternToken lPreviousToken = PatternToken.UNKNOWN;

			// tokenize code
			while (lPosition < code.Length)
			{
				string lTokenString = ReadTokenString(code, ref lPosition);
				PatternToken lToken = GetToken(lTokenString, lPreviousToken);

				// "*" could also be a variable
				if ((lToken == PatternToken.MULTIPLY) && (lPreviousToken == PatternToken.COMMA || lPreviousToken == PatternToken.OPEN_PARA))
					lToken = PatternToken.VARIABLE;

				int lParameterCount = (lToken == PatternToken.FUNCTION) ? GetParameterCount(code, lPosition) : 0;

				PatternOperand patternOperand = new PatternOperand(lToken, lTokenString, lParameterCount);

				if (lToken == PatternToken.UNKNOWN)
				{
					lPatternCode.SetErrorMessage("Unknown Token: " + lTokenString.ToString());
					return lPatternCode;
				}
				lPatternCode.Add(patternOperand);

				lPreviousToken = lToken;
			}

			return lPatternCode;
		}
	}
}
