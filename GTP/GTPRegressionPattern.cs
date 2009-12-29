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
	public enum GTPRegressionResult { passed = 0, PASSED = 1, failed = 2, FAILED = 3, ignore = 4 };

	class GTPRegressionPattern
	{
		protected List<string> Parameters;
		protected bool ParsedSuccessfully;
		protected bool ExpectFailure;
		protected bool IgnoreTest;

		public GTPRegressionPattern(string parameters)
		{
			Parse(parameters);
		}

		public bool Parse(string parameters)
		{
			ParsedSuccessfully = false;
			ExpectFailure = false;
			IgnoreTest = false;
			Parameters = new List<string>();

			String lResult = parameters.Trim();

			int lAt = lResult.IndexOf("[");

			if (lAt < 0)
				return false;

			lResult = lResult.Substring(lAt+1).Trim();

			lAt = lResult.IndexOf("]");

			if (lAt < 0)
				return false;

			string lRest = lResult.Substring(lAt + 1);

			lResult = lResult.Substring(0,lAt).Trim();

			if (lRest.IndexOf("&") >= 0)
				IgnoreTest = true;

			if (lRest.IndexOf("*") >= 0)
				ExpectFailure = true;

			string[] lSplit = lResult.Split(new Char[] { '|' });

			foreach (string pParam in lSplit)
				Parameters.Add(pParam.Trim());
			
			ParsedSuccessfully = true;

			return true;
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder("[");

			foreach (string lParam in Parameters)
			{
				if (lStringBuilder.Length != 1)
					lStringBuilder.Append("|");
				lStringBuilder.Append(lParam);
			}

			lStringBuilder.Append("]");

			return lStringBuilder.ToString();
		}

		protected static string NormalizeInput(string pInput)
		{
			String lResult = pInput.Trim();

			if (String.IsNullOrEmpty(lResult))
				return String.Empty;

			int lAt = lResult.IndexOf(" ");

			if (lAt >= 0)
				lResult = lResult.Substring(lAt + 1).Trim();

			return lResult;
		}

		public GTPRegressionResult Test(string pInput)
		{
			return Test(this, pInput);
		}

		public static GTPRegressionResult Test(GTPRegressionPattern pattern, string pInput)
		{
			string lInput = NormalizeInput(pInput);

			if (string.IsNullOrEmpty(lInput))
				return GTPRegressionResult.FAILED;

			bool lPass = (lInput == "PASS");

			if (!pattern.ParsedSuccessfully)
				return GTPRegressionResult.FAILED;	// no pattern found

			if (pattern.IgnoreTest)
				return GTPRegressionResult.ignore;

			foreach (string lParam in pattern.Parameters)
			{
				int lAt = lParam.IndexOf("!");

				bool lNegate = (lAt >= 0);

				string lParam2 = (lNegate) ? lParam.Substring(lAt + 1) : lParam;

				bool lMatch = (lInput == lParam2);

				if ((!lNegate) && (lMatch))
					return (!pattern.ExpectFailure) ? GTPRegressionResult.passed : GTPRegressionResult.PASSED;

				if ((lNegate) && (!lMatch) && (!lPass))
					return (!pattern.ExpectFailure) ? GTPRegressionResult.passed : GTPRegressionResult.PASSED;

				if ((lNegate) && (lMatch))
					return (!pattern.ExpectFailure) ? GTPRegressionResult.failed : GTPRegressionResult.FAILED;
			}

			return (pattern.ExpectFailure) ? GTPRegressionResult.failed : GTPRegressionResult.FAILED;
		}
	}
}
