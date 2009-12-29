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
	class PatternActionAttribute : ErrorManagement
	{
		public bool MakeOrSecuresTerritory = false;		// e
		public bool CreateOrExpandsMoyo = false;		// E
		public bool ConnectMove = false;				// C
		public bool MinorConnectMove = false;			// c
		public bool CutMove = false;					// B
		public bool DefenseMove = false;				// d
		public bool AttackMove = false;					// a
		public bool JosekiMove = false;					// J
		public bool FusekiMove = false;					// F
		public bool MinorJosekiMove = false;			// j
		public bool MinorFusekiMove = false;			// f
		public bool UrgentJosekiMove = false;			// U

		public PatternActionAttribute(string attributes)
		{
			//			if (!Apply(attributes))
			//				SetErrorMessage("Unable to parse pattern attributes: " + attributes);				
		}

		public bool Apply(string attributes)
		{
			if (string.IsNullOrEmpty(attributes))
				return true;

			foreach (char lChar in attributes)
				if (!Apply(lChar))
					return false;

			return true;
		}

		public bool Apply(char c)
		{
			switch (c)
			{
				case 'e': MakeOrSecuresTerritory = true; return true;
				case 'E': CreateOrExpandsMoyo = true; return true;
				case 'C': ConnectMove = true; return true;
				case 'c': MinorConnectMove = true; return true;
				case 'B': CutMove = true; return true;
				case 'd': DefenseMove = true; return true;
				case 'a': AttackMove = true; return true;
				case 'J': JosekiMove = true; return true;
				case 'F': FusekiMove = true; return true;
				case 'j': MinorJosekiMove = true; return true;
				case 'f': MinorFusekiMove = true; return true;
				case 'U': UrgentJosekiMove = true; return true;

				default: return false;
			}
		}

		public override string ToString()
		{
			StringBuilder lString = new StringBuilder();

			if (MakeOrSecuresTerritory) lString.Append("e");		// e
			if (CreateOrExpandsMoyo) lString.Append("E");		// E
			if (ConnectMove) lString.Append("C");		// C
			if (MinorConnectMove) lString.Append("c");		// c
			if (CutMove) lString.Append("B");		// B
			if (MakeOrSecuresTerritory) lString.Append("d");		// d
			if (AttackMove) lString.Append("a");		// a
			if (JosekiMove) lString.Append("J");		// J
			if (FusekiMove) lString.Append("F");		// F
			if (MinorJosekiMove) lString.Append("j");		// j
			if (MinorFusekiMove) lString.Append("f");		// f
			if (UrgentJosekiMove) lString.Append("U");		// U

			return lString.ToString();
		}
	}
}
