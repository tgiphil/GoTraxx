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
	class GoBlocksAdjacentCollection
	{
		protected GoBlockBase Block;

		protected List<GoBlockBase> AdjacentBlocks;
		protected List<int> AdjacentBlocksPosition;
		protected List<int> AdjacentBlocksCount;

		public List<GoBlockBase> AllBlocks
		{
			get
			{
				return AdjacentBlocks;
			}
		}
		
		public List<GoBlock> StoneBlocks
		{
			get
			{
				List<GoBlock> lCollection = new List<GoBlock>();

				foreach (GoBlockBase lBlock in AdjacentBlocks)
					if (!lBlock.IsEmptyBlock())
						lCollection.Add((GoBlock) lBlock);

				return lCollection;
			}
		}

		public List<GoEmptyBlock> EmptyBlocks
		{
			get
			{
				List<GoEmptyBlock> lCollection = new List<GoEmptyBlock>();

				foreach (GoBlockBase lBlock in AdjacentBlocks)
					if (lBlock.IsEmptyBlock())
						lCollection.Add((GoEmptyBlock) lBlock);

				return lCollection;
			}
		}

		public GoBlocksAdjacentCollection(GoBlockBase goBlockBase)
		{
			AdjacentBlocks = new List<GoBlockBase>(10);
			AdjacentBlocksPosition = new List<int>(10); 
			AdjacentBlocksCount = new List<int>(10);
			Block = goBlockBase;
		}

		// area of bottleneck
		public void Minus(GoBlockBase goBlock)
		{
			int iIndex = AdjacentBlocksPosition.IndexOf(goBlock.BlockNbr);

			if (AdjacentBlocksCount[iIndex] == 1)
			{
				AdjacentBlocks.RemoveAt(iIndex);
				AdjacentBlocksCount.RemoveAt(iIndex);
				AdjacentBlocksPosition.RemoveAt(iIndex);
			}
			else
				AdjacentBlocksCount[iIndex]--;

			if (AdjacentBlocks.Count == 0)
				Block.Board.AllBlocks.Remove(Block);
		}

		// area of bottleneck
		public void Plus(GoBlockBase goBlock)
		{
			if (AdjacentBlocks.Count == 0)
				Block.Board.AllBlocks.Add(Block);

			int iIndex = AdjacentBlocksPosition.IndexOf(goBlock.BlockNbr);

			if (iIndex >= 0)
				AdjacentBlocksCount[iIndex]++;
			else
			{
				AdjacentBlocks.Add(goBlock);
				AdjacentBlocksPosition.Add(goBlock.BlockNbr);
				AdjacentBlocksCount.Add(1);
			}
		}
	}
}
