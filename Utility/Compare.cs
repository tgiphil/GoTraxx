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
	public class Compare
	{
		/// <summary>
		/// Returns the maximum of the specified values.
		/// </summary>
		/// <param name="val1">The val1.</param>
		/// <param name="val2">The val2.</param>
		/// <returns></returns>
		static public T Max<T>(T val1, T val2) where T : IComparable
		{
			T retVal = val2;
			if (val2.CompareTo(val1) < 0)
				retVal = val1;
			return retVal;
		}

		/// <summary>
		/// Returns the maximum of the specified values.
		/// </summary>
		/// <param name="val1">The val1.</param>
		/// <param name="val2">The val2.</param>
		/// <returns></returns>
		static public T Min<T>(T val1, T val2) where T : IComparable
		{
			T retVal = val2;
			if (val2.CompareTo(val1) > 0)
				retVal = val1;
			return retVal;
		}

		/// <summary>
		/// Returns the maximum of the specified values.
		/// </summary>
		/// <param name="val1">The val1.</param>
		/// <param name="val2">The val2.</param>
		/// <param name="val3">The val3.</param>
		/// <returns></returns>
		static public T Max<T>(T val1, T val2, T val3) where T : IComparable
		{
			return Max<T>(Max<T>(val1, val2), val3);
		}

		/// <summary>
		/// Returns the maximum of the specified values.
		/// </summary>
		/// <param name="val1">The val1.</param>
		/// <param name="val2">The val2.</param>
		/// <param name="val3">The val3.</param>
		/// <returns></returns>
		static public T Min<T>(T val1, T val2, T val3) where T : IComparable
		{
			return Min<T>(Min<T>(val1, val2), val3);
		}
	}
}
