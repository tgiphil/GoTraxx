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
	class FusekiDetector
	{
		protected Dictionary<Int64, List<int>>[] lHashedMoves = new Dictionary<long, List<int>>[CoordinateSystem.MAX_BOARD_SIZE];

		public FusekiDetector(PatternCollection patterns)
		{
			foreach (Pattern lPattern in patterns)
				if (lPattern.IsFixedFuseki())
					Add(lPattern);
		}

		protected void Add(Pattern pattern)
		{
			int lSize = pattern.GetFullBoardSize();

			if (lHashedMoves[lSize] == null)
				lHashedMoves[lSize] = new Dictionary<long, List<int>>();

		}

		protected ZobristHash ComputeZobrist(Pattern pattern, Color color)
		{
			int lSize = pattern.GetFullBoardSize();

			ZobristHash lZobristHash = new ZobristHash();

			for (int x = 0; x < lSize; x++)
				for (int y = 0; y < lSize; y++)
					switch (pattern.GetCell(x, y))
					{
						case 'X': lZobristHash.Delta(color.IsBlack ? Color.Black : Color.White, CoordinateSystem.At(x, y, lSize)); break;
						case 'O': lZobristHash.Delta(color.IsBlack ? Color.White : Color.Black, CoordinateSystem.At(x, y, lSize)); break;
					}

			return lZobristHash;
		}
	}
}
