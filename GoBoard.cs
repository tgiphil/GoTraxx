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
using System.IO;

namespace GoTraxx
{
	class GoBoard
	{
		public Color Turn;
		public int SimpleKoPoint;
		public int MoveNbr;
		public double Komi;
		public CoordinateSystem Coord;
		public int LastMove;
		public List<KeyValuePair<Color, int>> MoveList;

		public int[] CapturedStoneCnt;
		public GoCell[] Cells;

		public List<GoBlockBase> AllBlocks;
		public ColorEnclosedRegions ColorEnclosedRegions;
		public SafetyMap SafetyStatusMap;

		public bool GameOver;

		public ZobristHash ZobristHash;
		protected Dictionary<ZobristHash, int> ZobrishHashes;

		protected SafetySolverType SafetySolver;

		protected UndoStack UndoStack;

		public int BoardSize
		{
			get
			{
				return Coord.BoardSize;
			}
		}

		public GoBoard(int boardSize)
		{
			Coord = CoordinateSystem.GetCoordinateSystem(boardSize);
			SafetySolver = SafetySolverType.Muller04;
			Clear();
		}

		public GoBoard(GoBoard goBoard, bool includeUndo)
		{
			Coord = CoordinateSystem.GetCoordinateSystem(goBoard.BoardSize);
			SafetySolver = goBoard.SafetySolver;
			Komi = goBoard.Komi;
			Clear();

			foreach (KeyValuePair<Color, int> lMove in goBoard.MoveList)
				PlayStone(lMove.Value, lMove.Key, includeUndo);
		}

		public GoBoard Clone()
		{
			return new GoBoard(this, false);
		}

		public GoBoard Clone(bool includeUndo)
		{
			return new GoBoard(this, includeUndo);
		}

		public int At(int x, int y)
		{
			return Coord.At(x, y);
		}

		public int At(string pAt)
		{
			return Coord.At(pAt);
		}

		protected void Clear()
		{
			AllBlocks = new List<GoBlockBase>();

			GoEmptyBlock lEmptyBlock = new GoEmptyBlock(this);

			UndoStack = new UndoStack(this);

			Turn = Color.Black;
			ZobrishHashes = new Dictionary<ZobristHash, int>();
			LastMove = CoordinateSystem.PASS;
			MoveList = new List<KeyValuePair<Color, int>>();
			SimpleKoPoint = CoordinateSystem.PASS;
			MoveNbr = 0;
			Komi = 0;
			GameOver = false;
			CapturedStoneCnt = new int[2];
			ColorEnclosedRegions = null;
			SafetyStatusMap = null;

			ZobristHash = new ZobristHash();
			Cells = new GoCell[Coord.BoardArea];

			for (int i = 0; i < Coord.BoardArea; i++)
				Cells[i] = new GoCell(i, this, lEmptyBlock);

			for (int i = 0; i < Coord.BoardArea; i++)
				Cells[i].SetNeighbors();
		}

		public void SetBoardSize(int boardSize)
		{
			Coord = CoordinateSystem.GetCoordinateSystem(boardSize);
			Clear();
		}

		public void ClearBoard()
		{
			Clear();
		}

		public bool IsKo(int index)
		{
			if (SimpleKoPoint == CoordinateSystem.PASS)
				return (SimpleKoPoint == index);

			return false;
		}

		public bool IsSuperKo()
		{
			if (LastMove == CoordinateSystem.PASS)
				return false;

			if (ZobrishHashes.ContainsKey(ZobristHash))
				return true;

			return false;
		}

		protected bool IsSimpleKoViolation(int index, Color player)
		{
			if (Turn != player)
				if (SimpleKoPoint != CoordinateSystem.PASS)
					return (SimpleKoPoint == index);

			return false;
		}

		public bool IsEmpty(int index)
		{
			return Cells[index].IsEmpty();
		}

		public bool IsSuicide(int index, Color player)
		{
			// Is this a suicidal move?

			foreach (GoCell lCell in Cells[index].Neighbors)
			{
				if (lCell.IsEmpty())
					return false;

				if (lCell.Block.BlockColor == player)
				{
					// No, if any of any friendly neighboring blocks are not in atari
					if (!((GoBlock)lCell.Block).IsAtariPoint(index))
						return false;
				}
				else
				{
					// No, if the move captures an enemy string (ie, fills in the last liberty)	
					if (((GoBlock)lCell.Block).IsAtariPoint(index))
						return false;
				}
			}

			return true;
		}

		public bool IsSuicide(int index)
		{
			return IsSuicide(index, Turn);
		}

		public bool IsProtectedLiberty(int index, Color color)
		{
			if (Cells[index].Color != Color.Empty)
				return false;

			// quick trivial case - two empty neighbors means point is not directly capturable
			int lEmptyCnt = 0;

			foreach (GoCell lCell in Cells[index].Neighbors)
				if (lCell.Color == Color.Empty)
					lEmptyCnt++;

			if (lEmptyCnt >= 2)
				return false;

			foreach (GoCell lCell in Cells[index].Neighbors)
			{
				if (lCell.Color == color.Opposite)
				{
					int lLibertyCount = ((GoBlock)lCell.Block).LibertyCount;
					if ((lLibertyCount >= 3) || ((lLibertyCount >= 2) && (lEmptyCnt >= 1)))
						return false;
				}
				else
					if (lCell.Color == color)
						//						if (((GoBlock)lCell.Block).LibertyCount <= 2)
						if (((GoBlock)lCell.Block).IsAtariPoint(index))
							return false;

			}

			return true;
		}

		public int AnyEmptyLibertyAround(int index)
		{
			foreach (GoCell lCell in Cells[index].Neighbors)
				if (lCell.Color == Color.Empty)
					return lCell.Index;

			return CoordinateSystem.PASS;
		}

		public bool IsLegal(int index, Color color)
		{
			if (index == CoordinateSystem.PASS)
				return true;

			if (index == CoordinateSystem.RESIGN)
				return true;

			if (!Coord.OnBoard(index))
				return false;

			if (!IsEmpty(index))
				return false;

			if (IsSimpleKoViolation(index, color))
				return false;

			if (IsSuicide(index, color))
				return false;

			return true;
		}

		public Color GetColor(int index)
		{
			return Cells[index].Color;
		}

		public bool IsSameString(int index1, int index2)
		{
			GoBlockBase lGoBlock1 = Cells[index1].Block;
			GoBlockBase lGoBlock2 = Cells[index2].Block;

			if (
				(lGoBlock1.IsEmptyBlock()) ||
				(lGoBlock2.IsEmptyBlock())
			)
				return false;
			return (lGoBlock1 == lGoBlock2);
		}

		public bool IsSameString(int index1, int index2, int index3)
		{
			GoBlockBase lGoBlock1 = Cells[index1].Block;
			GoBlockBase lGoBlock2 = Cells[index2].Block;
			GoBlockBase lGoBlock3 = Cells[index3].Block;

			if (
				(lGoBlock1.IsEmptyBlock()) ||
				(lGoBlock2.IsEmptyBlock()) ||
				(lGoBlock3.IsEmptyBlock())
				)
				return false;

			return (lGoBlock1 == lGoBlock2);
		}

		public int GetBlockLibertyCount(int index)
		{
			GoBlockBase lGoBlock = Cells[index].Block;

			if (lGoBlock.IsEmptyBlock())
				return 0;

			return ((GoBlock)lGoBlock).LibertyCount;
		}

		public List<GoCell> GetNeighboringCells(int index)
		{
			List<GoCell> lGoCells = new List<GoCell>();

			foreach (int lIndex in Coord.GetNeighbors(index))
				lGoCells.Add(Cells[lIndex]);

			return lGoCells;
		}

		public bool PlayStone(int index, Color color, bool allowUndo)
		{
			if (!IsLegal(index, color))
				return false;

			if (allowUndo)
			{
				UndoStack.Mark();
				UndoStack.Add(new GoBoardUndoState(this));
			}
			else
				UndoStack.Clear();

			if (index == CoordinateSystem.RESIGN)
				GameOver = true;

			if ((index == CoordinateSystem.PASS) && (LastMove == CoordinateSystem.PASS) && (MoveNbr > 0))
				GameOver = true;

			ColorEnclosedRegions = null;
			SafetyStatusMap = null;

			// set turn
			Turn = color;
			LastMove = index;
			MoveList.Add(new KeyValuePair<Color, int>(color, index));

			// save hash 
			if (LastMove != CoordinateSystem.PASS)
				if (!ZobrishHashes.ContainsKey(ZobristHash))
					ZobrishHashes.Add(ZobristHash.Clone(), MoveNbr);

			// increment move number
			MoveNbr++;

			//			Console.WriteLine("# " + MoveNbr + " " + color.ToString() + " " + Coord.ToString(index));
			//			Dump();

			SimpleKoPoint = CoordinateSystem.PASS;

			if (!(index == CoordinateSystem.PASS) || (index == CoordinateSystem.RESIGN))
			{
				// update blocks
				ExecutePlay(index, color, allowUndo);
			}

			//#if DEBUG
			//			if (!_SelfTest())
			//				Console.WriteLine("GoBoard::_SelfTest() Failed");
			//#endif

			return true;
		}

		public bool CanUndo()
		{
			return UndoStack.CanUndo();
		}

		public void Undo()
		{
			int lUndoMove = LastMove;

			ColorEnclosedRegions = null;
			SafetyStatusMap = null;

			UndoStack.Undo();

			if (ZobrishHashes.Count > 0)
			{
				int lMove;

				if (!ZobrishHashes.TryGetValue(ZobristHash, out lMove))
					return;

				if (lMove == MoveNbr)
					ZobrishHashes.Remove(ZobristHash);
			}

			//#if DEBUG
			//			if (!_SelfTest())
			//				Console.WriteLine("GoBoard::_SelfTest() Failed");
			//#endif
		}

		public bool PlayStone(int pX, int pY, Color color, bool allowUndo)
		{
			return PlayStone(At(pX, pY), color, allowUndo);
		}

		public bool PlayStone(int pX, int pY, Color color)
		{
			return PlayStone(At(pX, pY), color, false);
		}

		public bool PlayStone(string move, Color color, bool allowUndo)
		{
			return PlayStone(At(move), color, allowUndo);
		}

		protected void CreateEnclosedRegions()
		{
			if (ColorEnclosedRegions == null)
				ColorEnclosedRegions = new ColorEnclosedRegions(this);
		}

		protected void SolveSafety()
		{
			CreateEnclosedRegions();

			if (SafetyStatusMap == null)
				SafetyStatusMap = new SafetyMap(this, SafetySolver);
		}

		public SafetyStatus GetSafetyStatus(int index)
		{
			SolveSafety(); // does nothing is already computed

			return SafetyStatusMap[index];
		}

		protected List<GoBlockBase> GetNeighboringBlocks(int index)
		{
			List<GoBlockBase> Result = new List<GoBlockBase>(4);

			foreach (GoCell lGoCell in GetNeighboringCells(index))
				if (!(Result.Contains(lGoCell.Block)))
					Result.Add(lGoCell.Block);

			return Result;
		}

		protected void ExecutePlay(int index, Color color, bool allowUndo)
		{
			// get empty block that this stone is being placed on
			GoEmptyBlock lInEmptyBlock = (GoEmptyBlock)Cells[index].Block;

			// if the stone empties the EmptyBlock, remove the EmptyBlock
			if (lInEmptyBlock.EmptySpaceCount == 1)
			{
				// save undo operation onto stack
				if (allowUndo) UndoStack.Add(new GoEmptyBlockUndoRemove(lInEmptyBlock));
			}
			else
			{
				// if empty block is split, divide empty block
				if (lInEmptyBlock.IsCutPoint(index))
				{
					// save undo operation onto stack
					if (allowUndo) UndoStack.Add(new GoEmptyBlockUndoRemove(lInEmptyBlock));

					List<GoEmptyBlock> lNewEmptyBlocks = new List<GoEmptyBlock>(4);

					foreach (int lNeighbor in Coord.GetNeighbors(index))
						// is move point in lInEmptyBlock?
						if (lInEmptyBlock.IsMember(lNeighbor))
						{
							// is move not any new block?
							bool lFound = false;

							foreach (GoEmptyBlock lEmptyBlock in lNewEmptyBlocks)
								if (lEmptyBlock.IsMember(lNeighbor))
								{
									lFound = true;
									break;
								}

							// if yes to all above then, do this:
							if (!lFound)
								lNewEmptyBlocks.Add(new GoEmptyBlock(lInEmptyBlock, index, lNeighbor));
						}

					foreach (GoEmptyBlock lEmptyBlock in lNewEmptyBlocks)
						foreach (int lIndex in lEmptyBlock.MemberList)
							Cells[lIndex].AssignCell(lEmptyBlock);
				}
				else
				{
					// save undo operation onto stack
					if (allowUndo) UndoStack.Add(new GoEmptyBlockUndoRemoveLiberty(lInEmptyBlock, index));

					// remove member of in the EmptyBlock
					lInEmptyBlock.RemoveMember(index);
				}
			}

			List<GoBlockBase> lNeighboringBlocks = GetNeighboringBlocks(index);
			List<GoBlock> lFriendlyBlocks = new List<GoBlock>(4);
			List<GoBlock> lEnemyBlocks = new List<GoBlock>(4);

			foreach (GoBlockBase lBlock in lNeighboringBlocks)
			{
				// get a list of friendly blocks
				if (lBlock.BlockColor == color)
					lFriendlyBlocks.Add((GoBlock)lBlock);
				else
					// get a list of enemy blocks
					if (lBlock.BlockColor == color.Opposite)
						lEnemyBlocks.Add((GoBlock)lBlock);
			}

			List<GoBlock> lNotCapturedBlocks = new List<GoBlock>(4);
			List<GoBlock> lCapturedBlocks = new List<GoBlock>(4);

			if (lEnemyBlocks.Count > 0)
			{
				// get a sub-list of enemy blocks in atari 

				foreach (GoBlock lEnemyBlock in lEnemyBlocks)
					if (lEnemyBlock.InAtari())
						lCapturedBlocks.Add(lEnemyBlock);
					else
						lNotCapturedBlocks.Add(lEnemyBlock);

				int lStonesCaptured = 0;

				foreach (GoBlock lCapturedBlock in lCapturedBlocks)
				{
					// save undo operation onto stack
					if (allowUndo) UndoStack.Add(new GoBlockUndoCapture(lCapturedBlock));

					GoEmptyBlock lNewEmptyBlock = new GoEmptyBlock(lCapturedBlock);

					// remove captured blocks from enemy adjacent lists
					foreach (GoBlock lGoBlock in lCapturedBlock.AdjacentBlocks.StoneBlocks)
						foreach (GoBlock lBlock in lCapturedBlocks)
							foreach (int lStone in lBlock.MemberList)
								if (lGoBlock.IsEnemy(lStone))
									lGoBlock.EnemyStoneCaptured(lStone);

					// removing points from board
					foreach (int lStone in lCapturedBlock.MemberList)
						Cells[lStone].AssignCell(lNewEmptyBlock);

					lStonesCaptured = lStonesCaptured + lCapturedBlock.StoneCount;
				}

				// adjust capture count (undoable because board state was saved earlier)
				CapturedStoneCnt[color.Opposite.ToInteger()] += lStonesCaptured;

				// save undo operation onto stack
				if (allowUndo) UndoStack.Add(new GoBlockUndoEnemyStone(lNotCapturedBlocks, index));

				// 	fill liberties of enemy blocks that were not captured
				foreach (GoBlock lNotCapturedBlock in lNotCapturedBlocks)
					lNotCapturedBlock.EnemyStonePlaced(index);
			}

			// setup simple ko point, if any
			if ((lCapturedBlocks.Count == 1) && (lFriendlyBlocks.Count == 0))
				if (lCapturedBlocks[0].StoneCount == 1)
					SimpleKoPoint = lCapturedBlocks[0].Members.GetFirst();	// future improve

			// save undo operation onto stack
			if (allowUndo) UndoStack.Add(new GoCellUndoChange(Cells[index]));

			if (lFriendlyBlocks.Count == 0)
			{
				// create a block for the stone
				GoBlock lNewGoBlock = new GoBlock(this, color);

				// set stone to color
				Cells[index].AssignCell(lNewGoBlock);

				// add the connecting stone to new block
				lNewGoBlock.AddStone(index);
			}
			else
				if (lFriendlyBlocks.Count == 1)
				{
					GoBlock lFriendlyBlock = lFriendlyBlocks[0];

					// set stone to color
					Cells[index].AssignCell(lFriendlyBlock);

					// save undo operation onto stack
					if (allowUndo) UndoStack.Add(new GoBlockUndoAddStone(lFriendlyBlock, index));

					// add stone to block
					lFriendlyBlock.AddStone(index);
				}
				else
				{
					// create merger block
					GoBlock lMergedGoBlock = new GoBlock(this, color);

					// set stone to color
					Cells[index].AssignCell(lMergedGoBlock);

					// save undo operation onto stack
					if (allowUndo) UndoStack.Add(new GoBlockUndoMerge(lMergedGoBlock, lFriendlyBlocks));

					// add the connecting stone to new block
					lMergedGoBlock.AddStone(index);

					// add stones to merged block
					foreach (GoBlock lFriendlyBlock in lFriendlyBlocks)
						foreach (int lStone in lFriendlyBlock.MemberList)
						{
							lMergedGoBlock.AddStone(lStone);
							Cells[lStone].AssignCell(lMergedGoBlock);
						}
				}
		}

		public override string ToString()
		{
			StringBuilder lString = new StringBuilder(250);

			for (int y = BoardSize; y > 0; y--)
			{
				lString.Append(y);

				if (y < 10)
					lString.Append(" ");

				lString.Append(" : ");

				for (int x = 0; x < BoardSize; x++)
					lString.Append(Cells[At(x, y - 1)].Color.ToString());

				lString.Append("\n");
			}
			lString.Append("     ");
			lString.Append("ABCDEFGHJKLMNOPQURS".Substring(0, BoardSize));
			lString.Append("\n");

			return lString.ToString();
		}

		public string ToString2()
		{
			StringBuilder lString = new StringBuilder(250);

			for (int y = BoardSize; y > 0; y--)
			{
				lString.Append(y);

				if (y < 10)
					lString.Append(" ");

				lString.Append(" : ");

				for (int x = 0; x < BoardSize; x++)
				{
					Color lColor = Cells[At(x, y - 1)].Color;

					lString.Append(lColor.ToString());

					lString.Append(Cells[At(x, y - 1)].Block.BlockNbr.ToString().PadRight(3));
				}

				lString.Append("\n");
			}
			lString.Append("     ");
			lString.Append("A   B   C   D   E   F   G   H   J   K   L   M   N   O   P   Q   U   R   S   ".Substring(0, BoardSize * 4));
			lString.Append("\n");

			foreach (GoBlockBase lGoBlock in AllBlocks)
			{
				lString.Append(lGoBlock.BlockNbr.ToString() + " " + lGoBlock.BlockColor.ToString() + " : ");
				foreach (GoBlockBase lAdjacentBlock in lGoBlock.AdjacentBlocks.AllBlocks)
					lString.Append(lAdjacentBlock.BlockNbr.ToString() + " ");
				lString.Append("\n");
			}

			return lString.ToString();
		}


		public int CountSafePoints(Color color)
		{
			SolveSafety();
			return SafetyStatusMap.CountSafePoints(color);
		}


		public int CountSafePoints(Color color, SafetySolverType pSafetySolverType)
		{
			if ((SafetyStatusMap != null) && (SafetyStatusMap.SafetySolverType == pSafetySolverType))
			{
				CreateEnclosedRegions();
				return SafetyStatusMap.CountSafePoints(color);
			}
			else
			{
				ColorEnclosedRegions lColorEnclosedRegions = ColorEnclosedRegions;
				ColorEnclosedRegions = null;
				CreateEnclosedRegions();
				SafetyMap lSafetyStatusMap = new SafetyMap(this, pSafetySolverType);

				ColorEnclosedRegions = lColorEnclosedRegions;

				return lSafetyStatusMap.CountSafePoints(color);
			}
		}

		public void SetSafetySolver(SafetySolverType pSafetySolverType)
		{
			SafetySolver = pSafetySolverType;
			SafetyStatusMap = null;
			ColorEnclosedRegions = null;
		}

		public string ToStringEnclosedBlockDump(int index)
		{
			CreateEnclosedRegions();
			StringBuilder lStringBuilder = new StringBuilder();

			ColorEnclosedRegion ColorEnclosedRegion0 = ColorEnclosedRegions.FindRegion(Color.Black, index);
			if (ColorEnclosedRegion0 != null)
			{
				lStringBuilder.AppendLine("Black: ");
				lStringBuilder.Append(ColorEnclosedRegion0.ToString());
			}

			ColorEnclosedRegion ColorEnclosedRegion1 = ColorEnclosedRegions.FindRegion(Color.White, index);
			if (ColorEnclosedRegion1 != null)
			{
				lStringBuilder.AppendLine("White: ");
				lStringBuilder.Append(ColorEnclosedRegion1.ToString());
			}

			return lStringBuilder.ToString();
		}

		public void Dump()
		{
			Console.Error.Write(ToString());
		}

		public void Dump2()
		{
			Console.Error.Write(ToString2());
		}

		public bool _SelfTest()
		{
			foreach (GoBlockBase lGoBlockBase in AllBlocks)
				if (!lGoBlockBase._SelfTest2())
					return false;

			foreach (GoCell lGoCell in Cells)
				if (!lGoCell.Block.Members.Contains(lGoCell.Index))
					return false;

			return true;
		}
	}
}
