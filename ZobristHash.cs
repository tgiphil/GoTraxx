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
	class ZobristHash
	{
		protected Int64 HashKey;

		protected static Int64[] HashValues = InitializeHash();

		/// <summary>
		/// Gets the hash value.
		/// </summary>
		/// <value>The hash value.</value>
		public Int64 HashValue
		{
			get
			{
				return HashKey;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZobristHash"/> class.
		/// </summary>
		public ZobristHash()
		{
			HashKey = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZobristHash"/> class.
		/// </summary>
		/// <param name="lZobristHash">Zobrist Hash.</param>
		protected ZobristHash(ZobristHash lZobristHash)
		{
			HashKey = lZobristHash.HashKey;
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns></returns>
		public ZobristHash Clone()
		{
			return new ZobristHash(this);
		}

		/// <summary>
		/// Initializes the hash table.
		/// </summary>
		/// <returns></returns>
		protected static Int64[] InitializeHash()
		{
			Random lRandom = new Random();

			Int64[] lHashValues = new Int64[CoordinateSystem.MAX_BOARD_SIZE * CoordinateSystem.MAX_BOARD_SIZE * 3 + 2];

			for (int i = 0; i < lHashValues.Length; i++)
			{
				Int64 lRand = 0;

				while ((lRand == 0) || (lRand == ~0))
					lRand = (lRandom.Next() << 32) | (lRandom.Next());

				lHashValues[i] = lRand;
			}

			return lHashValues;
		}

		protected static Int64 GetValue(Color color, int index)
		{
			return HashValues[2 + index + (CoordinateSystem.MAX_BOARD_SIZE * CoordinateSystem.MAX_BOARD_SIZE * color.ToInteger())];
		}

		public void Delta(Color color, int index)
		{
			if (color.IsEmpty)
				return;

			HashKey = HashKey ^ GetValue(color, index);
		}

		public void Mark(Color color)
		{
			if (color.IsBlack)
				HashKey = HashKey ^ HashValues[0];
			else
				HashKey = HashKey ^ HashValues[1];
		}

		public static bool operator ==(ZobristHash l, ZobristHash r)
		{
			if (object.ReferenceEquals(l, r))
				return true;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return false;

			return (l.HashKey == r.HashKey);
		}

		public static bool operator !=(ZobristHash l, ZobristHash r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			return (((ZobristHash)obj) == this);
		}

		public int CompareTo(object obj)
		{
			ZobristHash lZobristHash = (ZobristHash)obj;

			if (lZobristHash == this)
				return 0;
			else
				return -1;
		}

		public override int GetHashCode()
		{
			return Convert.ToInt32(HashKey % Int32.MaxValue);
		}

		public override string ToString()
		{
			return HashKey.ToString();
		}
	}
}
