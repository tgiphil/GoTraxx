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
	class SolverMuller : ISafetySolverInterface
	{
		protected GoBoard Board;
		protected Color Color;

		protected List<ColorEnclosedRegion> PotentialVitalRegions;
		protected Dictionary<GoBlock, List<ColorEnclosedRegion>> Blocks;

		private static bool Debug = false;
		private int Version;

		public SolverMuller(int pVersion)
		{
			Version = pVersion;
		}

		public void Solve(GoBoard goBoard, Color color)
		{
			Board = goBoard;
			Color = color;

			PotentialVitalRegions = new List<ColorEnclosedRegion>();
			Blocks = new Dictionary<GoBlock, List<ColorEnclosedRegion>>();

			CreateRegions();
			CreateBlockToRegionMap();
			FindAliveBlocks();

			MoreSafeBlocks();
		}

		protected void CreateRegions()
		{
			foreach (ColorEnclosedRegion lColorEnclosedRegion in Board.ColorEnclosedRegions.Regions[Color.ToInteger()])
			{
				lColorEnclosedRegion.SetVersion(Version);
				PotentialVitalRegions.Add(lColorEnclosedRegion);
			}
		}

		protected void CreateBlockToRegionMap()
		{
			// create a map between blocks and healthy small color enclosed regions		

			foreach (GoBlockBase lGoBlockBase in Board.AllBlocks)
			{
				if (!lGoBlockBase.IsEmptyBlock())
				{
					GoBlock lGoBlock = (GoBlock)lGoBlockBase;

					List<ColorEnclosedRegion> lUsedColorEnclosedRegions = new List<ColorEnclosedRegion>();

					foreach (ColorEnclosedRegion lColorEnclosedRegion in PotentialVitalRegions)
						if (lColorEnclosedRegion.Neighbors.Contains(lGoBlock))
							if ((lColorEnclosedRegion.IsSmallEnclosed()) && (lColorEnclosedRegion.IsHealthyFor(lGoBlock)))
								lUsedColorEnclosedRegions.Add(lColorEnclosedRegion);
							else
								if (lColorEnclosedRegion.IsOneVital())
									lUsedColorEnclosedRegions.Add(lColorEnclosedRegion);
								else
									if (lColorEnclosedRegion.IsTwoVital())
										lUsedColorEnclosedRegions.Add(lColorEnclosedRegion);

					Blocks.Add(lGoBlock, lUsedColorEnclosedRegions);
				}
			}
		}

		protected void FindAliveBlocks()
		{
			if (Debug)
			{
				Console.WriteLine("Before---> " + Color.ToChar2());
				Dump2();
				Console.WriteLine("<---");
			}

			List<GoBlock> lRemoveBlocks = new List<GoBlock>();

			bool lChange = true;

			while (lChange)
			{
				lChange = false;
				lRemoveBlocks.Clear();

				foreach (GoBlock lGoBlock in Blocks.Keys)
				{
					if (Blocks[lGoBlock].Count < 2)
					{
						bool lRemove = false;

						if (Blocks[lGoBlock].Count == 0)
							lRemove = true;
						else				// //  && (!Blocks[lGoBlock][0].IsOneVital()))		
							if ((!Blocks[lGoBlock][0].IsTwoVital()))
							{
								//if ((Blocks[lGoBlock][0].IsOneVital()) && (Blocks[lGoBlock][0].EmptyArea.Count > 1))
								//	;
								//else
								if (!((Blocks[lGoBlock][0].IsOneVital()) && (Blocks[lGoBlock][0].InteriorDefenderBlocks.Contains(lGoBlock))))
									lRemove = true;
							}

						if (lRemove)
							lRemoveBlocks.Add(lGoBlock);
					}
				}

				foreach (GoBlock lGoBlock in lRemoveBlocks)
				{
					lChange = true;

					if (Debug)
						Console.WriteLine("Remove Block #" + lGoBlock.BlockNbr + " At: " + Board.Coord.ToString(lGoBlock.MemberList[0]));

					foreach (ColorEnclosedRegion lColorEnclosedRegion in PotentialVitalRegions)
						if (lColorEnclosedRegion.Neighbors.Contains(lGoBlock))
						{
							if (Debug)
								Console.WriteLine("Remove Region #" + lColorEnclosedRegion._RegionNbr + " At: " + Board.Coord.ToString(lColorEnclosedRegion.Members[0].MemberList[0]));

							// remove this region from all of blocks
							foreach (GoBlock lGoBlock2 in Blocks.Keys)
								Blocks[lGoBlock2].Remove(lColorEnclosedRegion);
						}

					Blocks.Remove(lGoBlock);
				}
			}

			if (Debug)
			{
				Console.WriteLine("After--->");
				Dump2();
				Console.WriteLine("<---");
			}
		}

		public void UpdateSafetyKnowledge(SafetyMap safetyMap)
		{
			foreach (GoBlock lGoBlock in Blocks.Keys)
			{
				safetyMap.AddAliveBlock(lGoBlock);

				foreach (ColorEnclosedRegion lColorEnclosedRegion in Blocks[lGoBlock])
				{
					safetyMap.AddTerritoryBlocks(lColorEnclosedRegion.EmptyBlocks, Color);
					safetyMap.AddDeadBlocks(lColorEnclosedRegion.InteriorAttackerBlocks);
				}
			}
		}

		public void MoreSafeBlocks()
		{
			List<ColorEnclosedRegion> lVitalRegions = new List<ColorEnclosedRegion>();

			// create a list of vital regions 
			foreach (List<ColorEnclosedRegion> lColorEnclosedRegions in Blocks.Values)
				foreach (ColorEnclosedRegion lColorEnclosedRegion in lColorEnclosedRegions)
					if (!lVitalRegions.Contains(lColorEnclosedRegion))
						lVitalRegions.Add(lColorEnclosedRegion);

			// regions which are too small to support life are either alive or territory for the color that encloses them

			foreach (ColorEnclosedRegion lColorEnclosedRegion in Board.ColorEnclosedRegions.Regions[Color.ToInteger()])
				if (!lVitalRegions.Contains(lColorEnclosedRegion))
				{
					bool lAllEnclosingBlocksAlive = true;

					foreach (GoBlock lGoBlock in lColorEnclosedRegion.Neighbors)
						if (!Blocks.ContainsKey(lGoBlock))
						{
							lAllEnclosingBlocksAlive = false;
							break;
						}

					if (!lAllEnclosingBlocksAlive)
						continue;


					// an attacker eye area greater than 4 must have at least two points where are not adjacent
					if (lColorEnclosedRegion.AttackersEyeArea.Count > 4)
						continue;

					bool lProtentialEyes = true;

					if (lColorEnclosedRegion.AttackersEyeArea.Count <= 1)
						lProtentialEyes = false;

					if (lColorEnclosedRegion.AttackersEyeArea.Count >= 2)
					{
						lProtentialEyes = false;
						List<int> lAttackersEyePoints = lColorEnclosedRegion.AttackersEyeArea.ToList();
						// find any two points where are not adjacent - then no protential
						foreach (int lPoint in lAttackersEyePoints)
						{
							foreach (int lPoint2 in lAttackersEyePoints)
								if ((lPoint != lPoint2) && (!Board.Coord.IsNeighbor(lPoint, lPoint2)))
								{
									lProtentialEyes = true;
									break;
								}

							if (lProtentialEyes)
								break;
						}
					}
					
					if (!lProtentialEyes)
					{
						foreach (GoBlock lGoBlock in lColorEnclosedRegion.EnclosingBlocks)
							if (!Blocks[lGoBlock].Contains(lColorEnclosedRegion))
								Blocks[lGoBlock].Add(lColorEnclosedRegion);

						foreach (GoBlock lGoBlock in lColorEnclosedRegion.InteriorDefenderBlocks)
							if (!Blocks.ContainsKey(lGoBlock))
								Blocks.Add(lGoBlock, new List<ColorEnclosedRegion>(0));
					}

				}
		}

		public void Dump()
		{
			Console.WriteLine("Muller Analysis for: " + Color.ToString());
			foreach (GoBlock lGoBlock in Blocks.Keys)
				Console.WriteLine("Alive Block #" + lGoBlock.BlockNbr + " At: " + Board.Coord.ToString(lGoBlock.MemberList[0]));
		}

		protected void Dump2()
		{
			foreach (GoBlock lGoBlock in Blocks.Keys)
			{
				Console.WriteLine("Block #" + lGoBlock.BlockNbr + " At: " + Board.Coord.ToString(lGoBlock.MemberList[0]) + " (" + Blocks[lGoBlock].Count + ")");

				//foreach (ColorEnclosedRegion lColorEnclosedRegion in Blocks[lGoBlock])
				//	lColorEnclosedRegion.Dump();
			}
		}

	}
}
