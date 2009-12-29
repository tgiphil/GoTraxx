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
using System.Threading;

namespace GoTraxx
{
	class SafetyMap
	{
		protected GoBoard Board;
		protected SafetyStatus[] Safety;
		public SafetySolverType SafetySolverType;

		public SafetyMap(GoBoard goBoard, SafetySolverType pSafetySolverType)
		{
			Board = goBoard;
			Safety = new SafetyStatus[Board.Coord.BoardArea];

			SafetySolverType = pSafetySolverType;
			Execute();
		}

		public SafetyStatus this[int index]
		{
			get
			{
				return Safety[index];
			}
			set
			{
				Safety[index] = value;
			}
		}

		protected void Execute()
		{
			ISafetySolverInterface lSolver = SafetySolverFactory.CreateFactory(SafetySolverType);

			lSolver.Solve(Board, Color.White);
			lSolver.UpdateSafetyKnowledge(this);
			lSolver.Solve(Board, Color.Black);
			lSolver.UpdateSafetyKnowledge(this);

			MarkUnsurroundablePoints();
			MarkDamePoints();
		}

		public int CountSafePoints(Color color)
		{
			int lCount = 0;

			foreach (SafetyStatus lSafetyStatus in Safety)
				if ((lSafetyStatus.IsAlive) || (lSafetyStatus.IsTerritory))
				{
					if (color.IsBoth)
						lCount++;
					else
						if (lSafetyStatus.Player == color)
							lCount++;
				}
				else if (lSafetyStatus.IsDead)
				{
					if (color.IsBoth)
						lCount++;
					else
						if (lSafetyStatus.Player.Opposite == color)
							lCount++;
				}

			return lCount;
		}

		protected void MarkUnsurroundablePoints()
		{
			for (int lIndex = 0; lIndex < Board.Coord.BoardArea; lIndex++)
				if (Safety[lIndex].IsUndecided)
				{
					bool[] lFoundAlive = new bool[2];
					lFoundAlive[0] = lFoundAlive[1] = false;

					foreach (GoCell lGoCell in Board.GetNeighboringCells(lIndex))
						if (Safety[lGoCell.Index].IsAlive)
							lFoundAlive[lGoCell.Color.ToInteger()] = true;

					if (lFoundAlive[0] && lFoundAlive[1])
						Safety[lIndex] = new SafetyStatus(SafetyFlag.Unsurroundable);
				}
		}

		protected void MarkDamePoints()
		{
			for (int lIndex = 0; lIndex < Board.Coord.BoardArea; lIndex++)
				if (Safety[lIndex].IsUnsurroundable)
				{
					bool lDame = true;

					foreach (GoCell lGoCell in Board.GetNeighboringCells(lIndex))
						if (Safety[lGoCell.Index].IsUndecided)
						{
							lDame = false;
							break;
						}

					if (lDame)
						Safety[lIndex] = new SafetyStatus(SafetyFlag.Dame);
				}
		}

		protected void Update(GoBlockBase goBlock, SafetyFlag safetyFlag)
		{
			SafetyStatus lSafetyStatus = new SafetyStatus(safetyFlag);

			foreach (int lIndex in goBlock.MemberList)
				Safety[lIndex] = lSafetyStatus;
		}

		public void AddAliveBlock(GoBlock goBlock)
		{
			Update(goBlock, SafetyFlag.Alive | (goBlock.BlockColor.IsBlack ? SafetyFlag.Black : SafetyFlag.White));
		}

		public void AddAliveBlocks(List<GoBlock> goBlocks)
		{
			foreach (GoBlock lGoBlock in goBlocks)
				AddAliveBlock(lGoBlock);
		}

		public void AddDeadBlock(GoBlock goBlock)
		{
			Update(goBlock, SafetyFlag.Dead | (goBlock.BlockColor.IsBlack ? SafetyFlag.Black : SafetyFlag.White));
		}

		public void AddDeadBlocks(List<GoBlock> goBlocks)
		{
			foreach (GoBlock lGoBlock in goBlocks)
				AddDeadBlock(lGoBlock);
		}

		public void AddTerritoryBlock(GoEmptyBlock goEmptyBlock, Color color)
		{
			Update(goEmptyBlock, SafetyFlag.Territory | (color.IsBlack ? SafetyFlag.Black : SafetyFlag.White));
		}

		public void AddTerritoryBlocks(List<GoEmptyBlock> pGoEmptyBlocks, Color color)
		{
			foreach (GoEmptyBlock lGoEmptyBlock in pGoEmptyBlocks)
				AddTerritoryBlock(lGoEmptyBlock, color);
		}

	}
}
