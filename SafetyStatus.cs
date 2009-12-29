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
	[Flags]
	public enum SafetyFlag
	{
		Undecided = 0,
		Alive = 1,
		Dead = 2,
		Territory = 4,
		Dame = 8,
		Unsurroundable = 16,
		Seki = 32,
		Black = 64,
		White = 128,
	};

	public struct SafetyStatus
	{
		private SafetyFlag Status;

		public SafetyStatus(SafetyFlag safetyFlag)
		{
			Status = safetyFlag;
		}

		public bool CompareTo(SafetyFlag safetyFlag)
		{
			if (safetyFlag == SafetyFlag.Undecided)
				return (Status == SafetyFlag.Undecided);

			return (Status & safetyFlag) == safetyFlag;
		}

		public SafetyStatus Add(SafetyFlag safetyFlag)
		{
			return new SafetyStatus((Status | safetyFlag));
		}

		public SafetyStatus Remove(SafetyFlag safetyFlag)
		{
			return new SafetyStatus((Status & ~safetyFlag));
		}

		public bool IsUndecided
		{
			get
			{
				return (Status == SafetyFlag.Undecided);
			}
		}

		public bool IsAlive
		{
			get
			{
				return (Status & SafetyFlag.Alive) != 0;
			}
		}

		public bool IsDead
		{
			get
			{
				return (Status & SafetyFlag.Dead) != 0;
			}
		}

		public bool IsTerritory
		{
			get
			{
				return (Status & SafetyFlag.Territory) != 0;
			}
		}

		public bool IsSeki
		{
			get
			{
				return (Status & SafetyFlag.Seki) != 0;
			}
		}

		public bool IsDame
		{
			get
			{
				return (Status & SafetyFlag.Dame) != 0;
			}
		}

		public bool IsUnsurroundable
		{
			get
			{
				return (Status & SafetyFlag.Unsurroundable) != 0;
			}
		}

		public bool IsBlack
		{
			get
			{
				return (Status & SafetyFlag.Black) != 0;
			}
		}

		public bool IsWhite
		{
			get
			{
				return (Status & SafetyFlag.White) != 0;
			}
		}

		public Color Player
		{
			get
			{
				if (IsBlack) return Color.Black;
				else if (IsWhite) return Color.White;
				else return Color.Empty;
			}
		}

		public override string ToString()
		{
			return Status.ToString();
		}

		public int ToInteger()
		{
			return (int)Status;
		}

		public string GTPString
		{
			get
			{
				if (IsUndecided) return "undecided";
				else if (IsAlive) return "alive";
				else if (IsDead) return "dead";
				else if (IsSeki) return "seki";
				else if (IsDame) return "dame";
				else if (IsUnsurroundable) return "unsurrounded";
				else if (IsTerritory && IsBlack) return "black_territory";
				else if (IsTerritory && IsWhite) return "white_territory";
				else return "undecided";
			}
		}

	}
}
