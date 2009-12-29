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

	class PatternAttributeFunctions
	{
		public delegate void PatternAttributeFunction(PatternAttribute patternAttribute, double value);

		protected Dictionary<string, PatternAttributeFunction> Dictionary;

		protected static PatternAttributeFunctions Instance = new PatternAttributeFunctions();

		protected PatternAttributeFunctions()
		{
			Dictionary = new Dictionary<string, PatternAttributeFunction>();

			Dictionary.Add("terri", MinTerri);
			Dictionary.Add("minterri", MinTerri);
			Dictionary.Add("maxterri", MaxTerri);
			Dictionary.Add("value", Value);
			Dictionary.Add("minvalue", MinValue);
			Dictionary.Add("maxvalue", MaxValue);
			Dictionary.Add("shape", Shape);
			Dictionary.Add("followup", FollowUp);
		}

		public static PatternAttributeFunction GetFunction(string method)
		{
			return Instance.FindFunction(method);
		}

		protected PatternAttributeFunction FindFunction(string method)
		{
			PatternAttributeFunction lFunction = null;

			if (Dictionary.TryGetValue(method, out lFunction))
				return lFunction;

			return null;
		}

		public static bool Execute(string attributeName, PatternAttribute patternAttribute, double value)
		{
			PatternAttributeFunction lFunction = GetFunction(attributeName);

			if (lFunction == null)
				return false;
			
			try
			{
				lFunction(patternAttribute, value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void MinTerri(PatternAttribute patternAttribute, double value)
		{
			patternAttribute.MinTerritorialValue = Compare.Max(patternAttribute.MinTerritorialValue, value);
		}

		public static void MaxTerri(PatternAttribute patternAttribute, double value)
		{
			patternAttribute.MaxTerritorialValue = Compare.Min(patternAttribute.MaxTerritorialValue, value);
		}

		public static void Value(PatternAttribute patternAttribute, double value)
		{
			if (value > 0)
				patternAttribute.MinValue = Compare.Max(patternAttribute.MinValue, value);
			else
				patternAttribute.MaxValue = Compare.Min(patternAttribute.MaxValue, value);
		}

		public static void MinValue(PatternAttribute patternAttribute, double value)
		{
			patternAttribute.MinValue = Compare.Max(patternAttribute.MinValue, value);
		}

		public static void MaxValue(PatternAttribute patternAttribute, double value)
		{
			patternAttribute.MaxValue = Compare.Min(patternAttribute.MaxValue, value);
		}

		public static void Shape(PatternAttribute patternAttribute, double value)
		{
			patternAttribute.AdjShapeFactor = Compare.Max(patternAttribute.AdjShapeFactor, value);
		}

		public static void FollowUp(PatternAttribute patternAttribute, double value)
		{
			patternAttribute.AdjFollowupFactor = Compare.Max(patternAttribute.AdjFollowupFactor, value);
		}

	}
}
