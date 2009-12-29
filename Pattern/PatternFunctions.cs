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
	delegate int PatternFunction(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters);

	class PatternFunctions
	{
		protected Dictionary<string, PatternFunction> Dictionary;

		public static int TRUE = 1;
		public static int FALSE = 0;

		protected static PatternFunctions Instance = new PatternFunctions();

		protected PatternFunctions()
		{
			Dictionary = new Dictionary<string, PatternFunction>();

			Add("samestring", SameString, 2);
			Add("libertycount", LibertyCount, 1);
			Add("suicide_xmove", IsSuicideXMove, 1);
			Add("suicide_omove", IsSuicideOMove, 1);

			Add("lib", LibertyCount, 1);
			Add("safe", Safe, 1);
			Add("dead", Dead, 1);
			Add("unknown", Unknown, 1);
			Add("xlib", LibertyCountAfterMove, 1);
			Add("olib", LibertyCountAfterMove, 1);
			Add("ko", IsKo, 1);
			Add("legal_xmove", IsLegalXMove, 1);
			Add("legal_omove", IsLegalOMove, 1);
			Add("safe_xmove", IsSafeXMove, 1);
			Add("safe_omove", IsSafeOMove, 1);			
		}

		protected void Add(string functionName, PatternFunction patternFunction, int parameters)
		{
			Dictionary.Add(functionName + ":" + parameters.ToString(), patternFunction);
		}

		static public PatternFunction GetFunction(string tokenString, int parameters)
		{
			return Instance.FindFunction(tokenString, parameters);
		}

		protected PatternFunction FindFunction(string tokenString, int parameters)
		{
			PatternFunction lFunction = null;

			if (Dictionary.TryGetValue(tokenString + ":" + parameters.ToString(), out lFunction))
				return lFunction;

			return null;
		}

		public static int LibertyCount(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.GetBlockLibertyCount(parameters[0]);
		}

		public static int SameString(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.IsSameString(parameters[0], parameters[1]) ? TRUE : FALSE;
		}

		public static int Safe(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.GetSafetyStatus(parameters[0]).IsAlive ? TRUE : FALSE;
		}

		public static int Dead(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.GetSafetyStatus(parameters[0]).IsDead ? TRUE : FALSE;
		}

		public static int Unknown(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.GetSafetyStatus(parameters[0]).IsUndecided ? TRUE : FALSE;
		}

		public static int IsLegalOMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return (goBoard.IsLegal(parameters[0], player)) ? TRUE : FALSE;
		}

		public static int IsLegalXMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			Color lPlayer = player.Opposite;

			return (goBoard.IsLegal(parameters[0], lPlayer)) ? TRUE : FALSE;
		}

		public static int IsSafeOMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return ((goBoard.IsLegal(parameters[0], player))
				&& (LibertyCountAfterMove(goBoard, player, parameters[0]) > 1)) ? TRUE : FALSE;
		}

		public static int IsSafeXMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			Color lPlayer = player.Opposite;

			return ((goBoard.IsLegal(parameters[0], lPlayer))
				&& (LibertyCountAfterMove(goBoard, lPlayer, parameters[0]) > 1)) ? TRUE : FALSE;
		}

		public static int IsSuicideOMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.IsSuicide(parameters[0], player) ? TRUE : FALSE;
		}

		public static int IsSuicideXMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.IsSuicide(parameters[0], player.Opposite) ? TRUE : FALSE;
		}

		public static int IsKo(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return goBoard.IsKo(parameters[0]) ? TRUE : FALSE;
		}

		public static int LibertyCountAfterMove(GoBoard goBoard, Color player, int move)
		{
			if (!goBoard.GetColor(move).IsEmpty)
				return 0;   // invalid

			if (goBoard.IsSuicide(move, player))
				return 0;

			goBoard.PlayStone(move, player, true);

			int lLiberities = goBoard.GetBlockLibertyCount(move);

			goBoard.Undo();

			return lLiberities;
		}

		public static int LibertyCountAfterMove(GoBoard goBoard, Color player, PatternFunctionParameters<int> parameters)
		{
			return LibertyCountAfterMove(goBoard, player, parameters[0]);
		}

	}
}
