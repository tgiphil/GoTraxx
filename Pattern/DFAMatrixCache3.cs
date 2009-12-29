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
	class DFAMatrixCache3
	{
		protected struct CacheEntry
		{
			public int Right;
			public int Middle;
			public int Left;
			public int Value;
		};
		 
		protected List<CacheEntry>[] CacheEntries;
		protected int CacheSize;

		public DFAMatrixCache3(int matrixSize)
		{
			CacheSize = (matrixSize / 2) + 2;

			if (CacheSize < 64)
				CacheSize = 64;

			if (CacheSize > 1024 * 128)
				CacheSize = 1024 * 128;

			CacheEntries = new List<CacheEntry>[CacheSize];
		}

		public int Search(int left, int middle, int right)
		{
			int lHash = CacheHash(left, middle, right);

			if (CacheEntries[lHash] == null)
				return 0;

			for (int i = CacheEntries[lHash].Count - 1; i >= 0; i--)
				if ((CacheEntries[lHash][i].Left == left) && (CacheEntries[lHash][i].Middle == middle) && (CacheEntries[lHash][i].Right == right))
					return CacheEntries[lHash][i].Value;

			return 0;
		}

		protected int CacheHash(int left, int middle, int right)
		{
			return (left + right + middle) % CacheSize;
		}

		public void Add(int left, int middle, int right, int value)
		{
			CacheEntry lCacheEntry;
			lCacheEntry.Left = left;
			lCacheEntry.Middle = middle;
			lCacheEntry.Right = right;
			lCacheEntry.Value = value;

			int lHash = CacheHash(left, middle, right);

			if (CacheEntries[lHash] == null)
				CacheEntries[lHash] = new List<CacheEntry>(8);

			CacheEntries[lHash].Add(lCacheEntry);
		}

	}
}
