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

	class PatternActionFunctions
	{
		public delegate void PatternActionFunction(MoveList moves, PatternFunctionParameters<int> patternFunctionParameters);

		protected Dictionary<string, PatternActionFunction> Dictionary;

		protected static PatternActionFunctions Instance = new PatternActionFunctions();

		protected PatternActionFunctions()
		{
			Dictionary = new Dictionary<string, PatternActionFunction>();

			Add("antisuji", AntiSuji, 1);
			Add("replace", Replace, 2);

			Add("test", Test, 0);
			//			Add("prevent_attack_threat", Replace, 1);
			//			Add("threaten_to_save", Replace, 1);
			//			Add("defend_against_atari", Replace, 1);
			//			Add("add_defend_both_move", Replace, 2);
			//			Add("add_cut_move", Replace, 2);
		}

		protected void Add(string functionName, PatternActionFunction patternActionFunction, int parameters)
		{
			Dictionary.Add(functionName + ":" + parameters.ToString(), patternActionFunction);
		}

		public static PatternActionFunction GetFunction(string method, int parameters)
		{
			return Instance.FindFunction(method, parameters);
		}

		protected PatternActionFunction FindFunction(string method, int parameters)
		{
			PatternActionFunction lFunction = null;

			if (!Dictionary.TryGetValue(method + ":" + parameters.ToString(), out lFunction))
				return null;

			return lFunction;
		}

		public static bool Execute(string method, MoveList moves, PatternFunctionParameters<int> patternFunctionParameters)
		{
			PatternActionFunction lFunction = GetFunction(method, patternFunctionParameters.Count);

			if (lFunction == null)
				return false;

			try
			{
				lFunction(moves, patternFunctionParameters);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void Test(MoveList moves, PatternFunctionParameters<int> patternFunctionParameters)
		{
			Console.Error.WriteLine("DEBUG: Test Action!");
		}

		public static void AntiSuji(MoveList moves, PatternFunctionParameters<int> patternFunctionParameters)
		{
			moves.RemoveMove(patternFunctionParameters[0]);
		}

		public static void Replace(MoveList moves, PatternFunctionParameters<int> patternFunctionParameters)
		{
			double lValue = moves.GetValue(patternFunctionParameters[0]);

			moves.RemoveMove(patternFunctionParameters[0]);
			moves.SetMinValue(patternFunctionParameters[1], lValue);	// redistribution	(limited)
		}

	}
}