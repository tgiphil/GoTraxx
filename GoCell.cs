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
	class GoCell
	{
		public GoBoard Board;
		public Color Color;
		public GoBlockBase Block;
		public List<GoCell> Neighbors;
		public int Index;
//		public int NeighborCnt;

		public GoCell(int index, GoBoard goBoard, GoEmptyBlock goEmptyBlock)
		{
			Board = goBoard;
			Color = Color.Empty;
			Block = goEmptyBlock;
			Index = index;

			Board.ZobristHash.Delta(Color, Index);
		}

		public void SetNeighbors()
		{
			Neighbors = Board.GetNeighboringCells(Index);

			foreach (GoCell lCell in Neighbors)
				Block.AdjacentBlocks.Plus(lCell.Block);

//			NeighborCnt = Neighbors.Count;
		}

		// area of bottleneck
		public void AssignCell(GoBlockBase goBlock)
		{
			if (goBlock == Block)
				return;

			foreach (GoCell lCell in Neighbors)
			{
				Block.AdjacentBlocks.Minus(lCell.Block);

				//lCell.OnNeighborChange(Block, goBlock);
				if (Block != goBlock)
				{
					lCell.Block.AdjacentBlocks.Minus(Block);
					lCell.Block.AdjacentBlocks.Plus(goBlock);
				}
			}

			// update Zobrish Hash
			if (Color != goBlock.BlockColor)
			{
				Board.ZobristHash.Delta(Color, Index);
				Board.ZobristHash.Delta(goBlock.BlockColor, Index);
			}

			Block = goBlock;
			Color = goBlock.BlockColor;

			foreach (GoCell lCell in Neighbors)
				Block.AdjacentBlocks.Plus(lCell.Block);
		}

		public bool IsEmpty()
		{
			return (Color.IsEmpty);
		}

	}
}
