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
	class DFAPattern
	{
		public string DFA;
		public int VariationCount = 0;

		public int Length
		{
			get
			{
				return DFA.Length;
			}
		}

		public DFAPattern(Pattern pattern, int transform)
		{
			DFA = ToDFA(pattern, transform);
			VariationCount = 0;

			foreach (char c in DFA)
			{
				int lValue = pattern.VariationCount(c);
				VariationCount += (lValue * lValue);
			}
		}

		public static string ToDFA(Pattern pattern, int transform)
		{
			StringBuilder lDFA = new StringBuilder(200);
			Coordinate lSpiral = new Coordinate(0, 0);

			int lArea = pattern.Width * pattern.Height;

			while (lArea > 0)
			{
				Coordinate p = pattern.Origin + lSpiral.Transform(transform);
				lSpiral.SpiralNext();

				lDFA.Append(Pattern.ToStandard(pattern.GetCell(p)));

				if (pattern.IsInPattern(p))
					lArea--;
			}

			return lDFA.ToString().TrimEnd('$');
		}

		public static string ToVariableDFA(Pattern pattern, int transform)
		{
			StringBuilder lDFA = new StringBuilder(200);
			Coordinate lSpiral = new Coordinate(0, 0);

			int lArea = pattern.Width * pattern.Height;

			while (lArea > 0)
			{
				Coordinate p = pattern.Origin + lSpiral.Transform(transform);
				lSpiral.SpiralNext();

				lDFA.Append(pattern.GetVariable(p));

				if (pattern.IsInPattern(p))
					lArea--;
			}

			return lDFA.ToString().TrimEnd();
		}

		public static bool operator ==(DFAPattern l, DFAPattern r)
		{
			if (object.ReferenceEquals(l, r))
				return true;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return false;

			return (l.DFA == r.DFA);
		}

		public static bool operator !=(DFAPattern l, DFAPattern r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			return (((DFAPattern)obj) == this);
		}

		public override int GetHashCode()
		{
			return DFA.GetHashCode();
		}

	}
}
