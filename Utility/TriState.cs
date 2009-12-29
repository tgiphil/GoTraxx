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

namespace GoTraxx
{
	public struct TriState
	{
		private enum TriStateEnum : byte { True, False, Unknown };

		private TriStateEnum mTriState;

		private TriState(TriStateEnum pTriState)
		{
			mTriState = pTriState;
		}

		public static TriState True
		{
			get
			{
				return new TriState(TriStateEnum.True);
			}
		}

		public static TriState False
		{
			get
			{
				return new TriState(TriStateEnum.False);
			}
		}

		public static TriState Unknown
		{
			get
			{
				return new TriState(TriStateEnum.Unknown);
			}
		}

		public bool IsTrue
		{
			get
			{
				return mTriState == TriStateEnum.True;
			}
		}

		public bool IsFalse
		{
			get
			{
				return mTriState == TriStateEnum.True;
			}
		}

		public TriState Not
		{
			get
			{
				if (IsTrue)
					return TriState.False;
				else
					if (IsFalse)
						return TriState.True;

				return TriState.Unknown;
			}
		}

		public bool IsUnknown
		{
			get
			{
				return mTriState == TriStateEnum.Unknown;
			}
		}

		public bool IsKnown
		{
			get
			{
				return mTriState != TriStateEnum.Unknown;
			}
		}

		public static bool operator ==(TriState l, TriState r)
		{
			if (object.ReferenceEquals(l, r))
				return true;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return false;

			return (l.mTriState == r.mTriState);
		}

		public static bool operator !=(TriState l, TriState r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			return (((TriState)obj) == this);
		}

		public override int GetHashCode()
		{
			return (byte)mTriState;
		}


		public static TriState operator |(TriState l, TriState r)
		{
			if (object.ReferenceEquals(l, r))
				return TriState.True;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return TriState.False;

			if (l.mTriState == TriStateEnum.True)
				return TriState.True;

			if (r.mTriState == TriStateEnum.True)
				return TriState.True;

			return TriState.False;
		}

		public static TriState operator &(TriState l, TriState r)
		{
			if (object.ReferenceEquals(l, r))
				return TriState.True;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return TriState.False;

			if ((l.mTriState == TriStateEnum.True) && (r.mTriState == TriStateEnum.True))
				return TriState.True;

			return TriState.False;
		}

		public static bool operator &(TriState l, bool r)
		{
			if (!r)
				return false;

			if (l.mTriState == TriStateEnum.False)
				return false;

			return true;
		}

		public static bool operator |(TriState l, bool r)
		{
			if (r)
				return true;

			if (l.mTriState == TriStateEnum.True)
				return true;

			return false;
		}

		public override string ToString()
		{
			switch (mTriState)
			{
				case TriStateEnum.False: return "False"; 
				case TriStateEnum.True: return "True"; 
				case TriStateEnum.Unknown: return "Unknown"; 
				default: return "Error"; 
			}
		}

	}
}
