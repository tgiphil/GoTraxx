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
	class SolverExtended : ISafetySolverInterface
	{
		protected SafetySolverType SafetySolverType;
		protected ISafetySolverInterface SafetySolver;
		protected GoBoard Board;
		protected Color Color;
		protected List<int> ProtectedLiberitiesLeft;

		public SolverExtended(SafetySolverType safetySolverType)
		{
			SafetySolverType = safetySolverType;
			SafetySolver = SafetySolverFactory.CreateFactory(safetySolverType);
		}

		public void Solve(GoBoard goBoard, Color color)
		{
			Board = goBoard;
			Color = color;
			SafetySolver.Solve(goBoard, color);
			ProtectedLiberitiesLeft = GoBoardHelper.GetProtectedLiberites(Board, color);
		}

		public void UpdateSafetyKnowledge(SafetyMap safetyMap)
		{
			SafetySolver.UpdateSafetyKnowledge(safetyMap);
			MarkSafe(safetyMap, Color);
		}

		// marked to be depreciated
		private List<int> _GetProtectedLiberites(Color color)
		{
			List<int> lProtected = new List<int>();

			for (int lIndex = 0; lIndex < Board.Coord.BoardArea; lIndex++)
				if (Board.IsProtectedLiberty(lIndex, color))
					lProtected.Add(lIndex);

			return lProtected;
		}

		// mark as alive:

		// TODO - 1. chains with at least two shared liberities of alive stones
		// DONE - 2. chains with at least one shared protected liberties neighboring an alive chain

		public void MarkSafe(SafetyMap safetyMap, Color color)
		{
			bool lDone = false;

			while (!lDone)
			{
				if (ProtectedLiberitiesLeft.Count == 0)
					return;

				List<int> lNewProtectedLiberitiesLeft = new List<int>(ProtectedLiberitiesLeft.Count);

				foreach (int lIndex in ProtectedLiberitiesLeft)
				{
					bool lSafeNeighbor = false;
					List<GoBlock> lNonSafeNeighbors = new List<GoBlock>();

					foreach (int lNeighbor in Board.Coord.Neighbors[lIndex])
						if (Board.Cells[lNeighbor].Color == color)
						{
							if (safetyMap[lNeighbor].IsAlive)
								lSafeNeighbor = true;
							else
								if (!lNonSafeNeighbors.Contains((GoBlock)Board.Cells[lNeighbor].Block))
									lNonSafeNeighbors.Add((GoBlock)Board.Cells[lNeighbor].Block);
						}

					if ((lSafeNeighbor) && (lNonSafeNeighbors.Count != 0))
						foreach (GoBlock lGoBlock in lNonSafeNeighbors)
							safetyMap.AddAliveBlock(lGoBlock);

					if ((lSafeNeighbor) && (lNonSafeNeighbors.Count == 0))
						lNewProtectedLiberitiesLeft.Add(lIndex);
				}

				lDone = (ProtectedLiberitiesLeft.Count == lNewProtectedLiberitiesLeft.Count);

				ProtectedLiberitiesLeft = lNewProtectedLiberitiesLeft;
			}
		}

	}
}
