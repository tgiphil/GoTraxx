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
	class PatternAttribute : ErrorManagement
	{
		public double MinTerritorialValue = 0;
		public double MaxTerritorialValue = Double.MaxValue;
		public double MinValue = 0;
		public double MaxValue = Double.MaxValue;

		public double AdjShapeFactor = 0;
		public double AdjFollowupFactor = 0;

		public PatternAttribute(string[] attributes)		
		{
			if (!Apply(attributes))
				SetErrorMessage("Unable to parse pattern attributes: " + attributes);				
		}

		public bool Apply(string[] attributes)
		{
			if (attributes != null)
				foreach (string lString in attributes)
					if (!Apply(lString))
						return false;

			return true;
		}

		protected bool Apply(string attribute)
		{
			if (string.IsNullOrEmpty(attribute))
				return true;

			int lOpenParan = attribute.IndexOf('(');

			string lName = null;
			string lValue = null;

			if (lOpenParan < 0)
				lName = attribute.Trim();
			else
			{
				lName = attribute.Substring(0, lOpenParan).Trim();

				lValue = attribute.Substring(lOpenParan+1);

				int lCloseParan = lValue.IndexOf(')');

				if (lCloseParan >= 0)
					lValue = lValue.Substring(0, lCloseParan);
			}

			double lValue2;

			if (string.IsNullOrEmpty(lValue))
				lValue2 = 0;
			else
				if (!Double.TryParse(lValue, out lValue2))
					return false;

			return Apply(lName, lValue2);
		}

		protected bool Apply(string attributeName, double value)
		{
			return PatternAttributeFunctions.Execute(attributeName, this, value);
		}

	}
}
