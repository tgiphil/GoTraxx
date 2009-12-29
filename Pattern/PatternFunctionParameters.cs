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
	class PatternFunctionParameters<T> where T : IComparable
	{
		protected List<T> Parameters;

		protected int Hash;	// used for comparing parameter lists

		public int Count
		{
			get
			{
				return Parameters.Count;
			}
		}

		public T this[int arg]
		{
			get
			{
				return Parameters[arg];
			}
		}

		public PatternFunctionParameters()
		{
			Parameters = new List<T>();
			Hash = 0;
		}

		public void Add(T value)
		{
			Parameters.Add(value);

			Hash = Hash + (value.GetHashCode() * Parameters.Count * 1009) + Parameters.Count; // 1009 is a prime number
		}

		public override int GetHashCode()
		{
			return Hash;
		}

		public static bool operator ==(PatternFunctionParameters<T> l, PatternFunctionParameters<T> r)
		{
			if (l.Hash != r.Hash)
				return false;	// shortcut - hashes don't match

			if (l.Parameters.Count != r.Parameters.Count)
				return false;

			for (int i = 0; i < l.Parameters.Count; i++)
				if (l.Parameters[i].CompareTo(r.Parameters[i]) == 0)
					return false;

			return true;
		}

		public static bool operator !=(PatternFunctionParameters<T> l, PatternFunctionParameters<T> r)
		{
			return !(l == r);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			return (((PatternFunctionParameters<T>)obj) == this);
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			foreach (T lType in Parameters)
				yield return lType;
		}
	}
}
