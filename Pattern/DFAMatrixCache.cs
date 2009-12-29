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
	class DFAMatrixCache
	{
		protected struct CacheEntry
		{
			public int Right;
			public int Left;
			public int Value;
		};

		protected List<CacheEntry>[] CacheEntries;

		protected int CacheSize;

		public DFAMatrixCache(int matrixSize)
		{
			CacheSize = (matrixSize / 8) + 2;
			if (CacheSize < 64)
				CacheSize = 64;

			if (CacheSize > 1024 * 256)
				CacheSize = 1024 * 256;

			CacheEntries = new List<CacheEntry>[CacheSize];
		}

		public int Search(int left, int right)
		{
			int lHash = CacheHash(left, right);

			if (CacheEntries[lHash] == null)
				return 0;

			for (int i = CacheEntries[lHash].Count - 1; i >= 0; i--)
				if ((CacheEntries[lHash][i].Left == left) && (CacheEntries[lHash][i].Right == right))
					return CacheEntries[lHash][i].Value;

			return 0;
		}

		protected int CacheHash(int left, int right)
		{
			return (left + right) % CacheSize;
		}

		public void Add(int left, int right, int value)
		{
			CacheEntry lCacheEntry;
			lCacheEntry.Left = left;
			lCacheEntry.Right = right;
			lCacheEntry.Value = value;

			int lHash = CacheHash(left, right);

			if (CacheEntries[lHash] == null)
				CacheEntries[lHash] = new List<CacheEntry>(8);

			CacheEntries[lHash].Add(lCacheEntry);
		}

	}
}
