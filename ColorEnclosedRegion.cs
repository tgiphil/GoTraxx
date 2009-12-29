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

	class ColorEnclosedRegion : Intersection.IContains
	{
		protected GoBoard Board;
		public Color Defender;

		public List<GoBlockBase> Members;
		public List<GoEmptyBlock> EmptyBlocks;
		public List<GoBlock> InteriorAttackerBlocks;
		public List<GoBlock> Neighbors;

		public List<GoBlock> EnclosingBlocks;
		public List<GoBlock> InteriorDefenderBlocks;

		protected Region _EnclosedArea;		// lazy evaluation
		protected Region _EmptyRegion;		// lazy evaluation
		protected Region _EnclosingBlocksLibertyArea;	// lazy evaluation
		protected Region _AccessibleLiberties;	// lazy evaluation
		protected Region _AttackersEyeArea;		// lazy evaluation

		protected int Size;
		protected TriState Is2Vital;	// for lazy evaluation
		protected TriState Is1Vital;	// for lazy evaluation
		protected TriState IsSmall;		// for lazy evaluation

		protected MiaiStrategy MiaiStrategy = null;

		public int _RegionNbr;

		protected int Version;

		public Region EnclosedArea
		{
			get
			{
				if (_EnclosedArea == null)
				{
					_EnclosedArea = new Region(Board.BoardSize);
					foreach (GoBlockBase lGoBlockBase in Members)
						_EnclosedArea.Add(lGoBlockBase.Members);
				}

				return _EnclosedArea;
			}
		}

		public Region EmptyArea
		{
			get
			{
				if (_EmptyRegion == null)
				{
					_EmptyRegion = new Region(Board.BoardSize);
					foreach (GoEmptyBlock lEmptyBlocks in EmptyBlocks)
						_EmptyRegion.Add(lEmptyBlocks.Members);
				}

				return _EmptyRegion;
			}
		}

		public Region EnclosingBlocksLibertyArea
		{
			get
			{
				if (_EnclosingBlocksLibertyArea == null)
				{
					_EnclosingBlocksLibertyArea = new Region(Board.BoardSize);

					foreach (GoBlock lBlock in EnclosingBlocks)
						_EnclosingBlocksLibertyArea.Add(lBlock.Liberties);
				}

				return _EnclosingBlocksLibertyArea;
			}
		}

		public Region AccessibleLiberties
		{
			get
			{
				if (_AccessibleLiberties == null)
				{
					_AccessibleLiberties = new Region(EnclosingBlocksLibertyArea);
					_AccessibleLiberties.And(EmptyArea);
				}

				return _AccessibleLiberties;
			}
		}

		public Region AttackersEyeArea
		{
			get
			{
				if (_AttackersEyeArea == null)
				{
					if (EnclosedArea.Count < 4)
						_AttackersEyeArea = new Region(Board.BoardSize);
					else
					{
						Region lEmptyArea = EmptyArea;
						Region lEnclosingBlocksLiberties = EnclosingBlocksLibertyArea;
						Region lEnclosedArea = EnclosedArea;

						//	Dump();
						//	Console.WriteLine(lEmptyArea.ToString());
						//	Console.WriteLine(AccessibleLiberties.ToString());

						_AttackersEyeArea = new Region(lEmptyArea);
						_AttackersEyeArea.Remove(lEnclosingBlocksLiberties);		// optional
						//	Console.WriteLine(_AttackersEyeArea.ToString());

						foreach (GoBlock lBlock in InteriorAttackerBlocks)
							_AttackersEyeArea.Remove(lBlock.Members);

						foreach (GoBlock lBlock in InteriorDefenderBlocks)
							_AttackersEyeArea.Add(lBlock.Members);

						//	Console.WriteLine(_AttackersEyeArea.ToString());

						foreach (int lPoint in _AttackersEyeArea.ToList())
						{
							int lAttackerCnt = 0;

							foreach (int lCorner in Board.Coord.GetCorners(lPoint))
								if (!lEnclosedArea.Contains(lCorner))
									//	Console.WriteLine("At: " + GoBoard.Coord.ToString(lCorner) + " @ " + GoBoard.Coord.ToString(lPoint) + " - " + lAttackerCnt.ToString());
									lAttackerCnt++;

							int lNeighborCnt = Board.Coord.GetNeighbors(lPoint).Count;

							if ((lAttackerCnt >= 2) || ((lNeighborCnt < 4) && (lAttackerCnt >= 1)))
							{
								// handle special case where all neighbors are friendly
								int lDefenderCnt = 0;

								foreach (int lNeighbor in Board.Coord.GetNeighbors(lPoint))
									//									if ((!EmptyArea.Contains(lNeighbor)) && _EnclosedArea.Contains(lNeighbor))
									if (Board.Cells[lNeighbor].Color == Defender)
										lDefenderCnt++;

								if (lDefenderCnt != lNeighborCnt)
									_AttackersEyeArea.Remove(lPoint);
							}
						}

						//						Console.WriteLine(_AttackersEyeArea.ToString());

					}
				}

				return _AttackersEyeArea;
			}
		}

		public ColorEnclosedRegion(GoEmptyBlock goEmptyBlock, Color defender)
		{
			Board = goEmptyBlock.Board;
			Defender = defender;
			Size = 0;
			Version = 2004;

			_RegionNbr = goEmptyBlock.BlockNbr;

			Members = new List<GoBlockBase>();
			EmptyBlocks = new List<GoEmptyBlock>();
			Neighbors = new List<GoBlock>();
			InteriorAttackerBlocks = new List<GoBlock>();

			Is2Vital = TriState.Unknown;
			Is1Vital = TriState.Unknown;
			IsSmall = TriState.Unknown;

			_EnclosedArea = null;
			_EmptyRegion = null;

			Stack<GoBlockBase> lWork = new Stack<GoBlockBase>();

			lWork.Push(goEmptyBlock);

			while (lWork.Count != 0)
			{
				GoBlockBase lGoBlockBase = lWork.Pop();

				if (!Members.Contains(lGoBlockBase))
				{
					Members.Add(lGoBlockBase);

					if (lGoBlockBase.IsEmptyBlock())
					{
						EmptyBlocks.Add((GoEmptyBlock)lGoBlockBase);
						Size = Size + ((GoEmptyBlock)lGoBlockBase).EmptySpaceCount;
					}
					else
					{
						InteriorAttackerBlocks.Add((GoBlock)lGoBlockBase);
						Size = Size + ((GoBlock)lGoBlockBase).StoneCount;
					}

					foreach (GoBlockBase lGoBlockBaseAdjacent in lGoBlockBase.AdjacentBlocks.AllBlocks)
						if (lGoBlockBaseAdjacent.BlockColor == Defender)
						{
							if (!Neighbors.Contains((GoBlock)lGoBlockBaseAdjacent))
								Neighbors.Add((GoBlock)lGoBlockBaseAdjacent);
						}
						else
							if (!Members.Contains(lGoBlockBaseAdjacent))
								lWork.Push(lGoBlockBaseAdjacent);
				}
			}

			EnclosingBlocks = new List<GoBlock>(Neighbors.Count);
			InteriorDefenderBlocks = new List<GoBlock>();

			foreach (GoBlock lGoBlock in Neighbors)
			{
				bool lFound = false;

				foreach (GoEmptyBlock lGoEmptyBlock in lGoBlock.AdjacentBlocks.EmptyBlocks)
				{
					if (!EmptyBlocks.Contains(lGoEmptyBlock))
					{
						lFound = true;
						break;
					}
				}

				if (lFound)
					EnclosingBlocks.Add(lGoBlock);
				else
					InteriorDefenderBlocks.Add(lGoBlock);
			}

			if (EnclosingBlocks.Count == 0)
				InteriorDefenderBlocks.Clear();
		}

		public bool IsHealthyFor(GoBlock goBlock)
		{
			foreach (GoEmptyBlock lGoEmptyBlock in EmptyBlocks)
				if (!goBlock.Members.IsOnlyAdjacent(lGoEmptyBlock.Members))
					return false;

			return true;
		}

		public bool IsSmallEnclosed()
		{
			if (IsSmall.IsKnown)
				return (IsSmall.IsTrue);

			if (EmptyArea.HasInterior())
			{
				IsSmall = TriState.False;
				return false;
			}

			IsSmall = TriState.True;
			return true;
		}

		public bool IsNeightbor(GoBlock goBlock)
		{
			return (Neighbors.Contains(goBlock));
		}

		public bool IsTwoVital()
		{
			// evaluation is lazy

			if (Is2Vital.IsKnown)
				return (Is2Vital.IsTrue);

			// two vital - if 1) all the enclosing blocks's empty points in the region are accessible liberties
			// and 2) the region has two intersection points

			if (!AccessibleLiberties.IsSame(EmptyArea))
			{
				Is2Vital = TriState.False;
				return false;
			}

			int lIntersectionCount = 0;

			foreach (GoBlockBase lGoBlockBase in EmptyBlocks)
				foreach (int lPoint in lGoBlockBase.Members)
					if (IsIntersection(lPoint))
					//                        if (EnclosingBlocks.Count > 1)
					{
						bool AdjacentToAll = true;

						//foreach (GoBlock lGoBlock in EnclosingBlocks)
						foreach (GoBlock lGoBlock in Neighbors)
							if (!lGoBlock.Liberties.Contains(lPoint))
							{
								AdjacentToAll = false;
								break;
							}

						if (AdjacentToAll)
						{
							lIntersectionCount++;
							if (lIntersectionCount >= 2)
							{
								Is2Vital = TriState.True;
								return true;
							}
						}
					}

			Is2Vital = TriState.False;
			return false;
		}

		public void SetVersion(int pVersion)
		{
			Version = pVersion;
		}

		public bool IsOneVital()
		{
			if (Is1Vital.IsKnown)
				return (Is1Vital.IsTrue);

			if (EnclosingBlocks.Count == 0)
			{
				Is1Vital = TriState.False;
				return false;
			}

			// step 1

			// build miai stragety 
			MiaiStrategy = new MiaiStrategy();

			Region lAccessibleLibertiesAvailable = new Region(AccessibleLiberties);

			if (!CreateBlocksConnectionStrategy(lAccessibleLibertiesAvailable, MiaiStrategy))
			{
				Is1Vital = TriState.False;
				return false;
			}

			// step 2

			// future: add miai accessible interior empty points to the set of accessible liberties, 
			// and also use protected liberties for the chaining. 

			Region lNewAccessibleRegion = new Region(AccessibleLiberties);

			foreach (GoBlock lGoBlock in InteriorDefenderBlocks)
				lNewAccessibleRegion.Add(lGoBlock.Liberties);

			// step 2a - add miai accessible interior empty points to the set of accessible liberties
			// rough implementation

			Region lMiaiAccessibleInteriorRegion = new Region(Board.BoardSize);

			if (Version >= 2004)
			{
				foreach (int lIndex in EmptyArea)
					if (!lNewAccessibleRegion.Contains(lIndex))
					{
						List<int> llAccessibleNeighborPoints = new List<int>(4);
						foreach (int lNeighbor in Board.Coord.GetNeighbors(lIndex))
							if (lNewAccessibleRegion.Contains(lNeighbor))
								llAccessibleNeighborPoints.Add(lNeighbor);

						if (llAccessibleNeighborPoints.Count >= 2)
						{
							lMiaiAccessibleInteriorRegion.Add(lIndex);
						}
					}
			}

			lNewAccessibleRegion.Add(lMiaiAccessibleInteriorRegion);

			// step 3

			Region lEmptyAndNotAccessible = new Region(EmptyArea);
			lEmptyAndNotAccessible.Remove(lNewAccessibleRegion);

			List<int> lList = lEmptyAndNotAccessible.ToList();
			int lMinAdjacent = 2;

			while (lList.Count != 0)
			{
				List<int> lRemove = new List<int>();

				foreach (int lIndex in lList)
				{
					List<int> lAccessibleLibertiesForPoint = new List<int>(4);

					foreach (int lNeighbor in Board.Coord.GetNeighbors(lIndex))
						if (lNewAccessibleRegion.Contains(lNeighbor))
							lAccessibleLibertiesForPoint.Add(lNeighbor);

					if (lAccessibleLibertiesForPoint.Count < 2)
					{
						Is1Vital = TriState.False;
						return false;
					}

					if ((lAccessibleLibertiesForPoint.Count == 2) || (lAccessibleLibertiesForPoint.Count == lMinAdjacent))
					{
						lNewAccessibleRegion.Remove(lAccessibleLibertiesForPoint[0]);
						lNewAccessibleRegion.Remove(lAccessibleLibertiesForPoint[1]);

						MiaiStrategy.Add(lIndex, lAccessibleLibertiesForPoint[0], lAccessibleLibertiesForPoint[1]);
						lRemove.Add(lIndex);
						lMinAdjacent = 2;
					}
				}

				if (lList.Count == lRemove.Count)
					lList.Clear();
				else
					foreach (int lPoint in lRemove)
						lList.Remove(lPoint);

				if (lRemove.Count == 0)
					lMinAdjacent++;
			}

			Is1Vital = TriState.True;
			return true;
		}

		public bool IsProtectedLiberty(int point)
		{
			// protected liberites are liberties that should not be played by the opponent
			// because of suicide or being directly capturable

			return (Board.IsProtectedLiberty(point, Defender));
		}

		public List<int> GetProtectedLiberties()
		{
			List<int> lProtectedLiberties = new List<int>();

			foreach (int point in EmptyArea)
				if (IsProtectedLiberty(point))
					lProtectedLiberties.Add(point);

			return lProtectedLiberties;
		}

		protected bool CreateBlocksConnectionStrategy(Region accessibleLibertiesAvailable, MiaiStrategy miaiStrategy)
		{
			List<int> lUsedLiberties = new List<int>((EnclosingBlocks.Count + InteriorDefenderBlocks.Count) * 2);

			List<GoChain> lGoChains = new List<GoChain>(EnclosingBlocks.Count + InteriorDefenderBlocks.Count);
			List<GoChain> lCheckList = new List<GoChain>(lGoChains.Count);

			// future, if any the of the blocks only have one liberty in Accessible, then no strategy 

			foreach (GoBlock lBlock in EnclosingBlocks)
			{
				GoChain lGoBlocks = new GoChain(lBlock, accessibleLibertiesAvailable);
				lGoChains.Add(lGoBlocks);
				lCheckList.Add(lGoBlocks);
			}

			foreach (GoBlock lBlock in InteriorDefenderBlocks)
			{
				GoChain lGoBlocks = new GoChain(lBlock, accessibleLibertiesAvailable);
				lGoChains.Add(lGoBlocks);
				lCheckList.Add(lGoBlocks);
			}

			List<int> lProtectedLiberties = GetProtectedLiberties();

			// merge chains based on shared points
			while (lGoChains.Count != 1)
			{
				List<GoChain> lRemoved = new List<GoChain>(lGoChains.Count);
				List<GoChain> lChanged = new List<GoChain>(lGoChains.Count);

				if (Version >= 2004)
				{
					// merge chains based on protected liberty chaining
					if (lGoChains.Count > 1)
					{
						foreach (int lProtected in lProtectedLiberties)
						{
							int lCaptureMove = Board.AnyEmptyLibertyAround(lProtected);

							// Console.Error.WriteLine("Protected: " + GoBoard.Coord.ToString(lProtected) + " Stop: " + (GoBoard.Coord.ToString(lCaptureMove)));

							if (!lUsedLiberties.Contains(lCaptureMove))
							{
								List<GoChain> lChains = new List<GoChain>();

								foreach (GoChain lGoChain in lGoChains)
									if (lGoChain.IsLiberty(lProtected))
										if (!lRemoved.Contains(lGoChain))
											lChains.Add(lGoChain);

								if (lChains.Count >= 2)
								{
									//Console.Error.WriteLine("Merging: " + lChains[0].ToString());

									//Console.Error.WriteLine("   with: " + lChains[1].ToString());

									lChains[0].MergeGoChain(lChains[1]);
									lChanged.Add(lChains[0]);
									lRemoved.Add(lChains[1]);

									if (lChains.Count == 3)
									{
										// Console.Error.WriteLine("    and: " + lChains[1].ToString());

										lChains[0].MergeGoChain(lChains[2]);
										lRemoved.Add(lChains[2]);
									}

									miaiStrategy.Add(lProtected, lCaptureMove);

									lUsedLiberties.Remove(lProtected);
									lUsedLiberties.Remove(lCaptureMove);
								}
							}
						}
					}

					foreach (GoChain lGoChain in lGoChains)
						if (!lRemoved.Contains(lGoChain))
							foreach (GoChain lGoChain2 in lCheckList)
								if (lGoChain != lGoChain2)
									if (!lRemoved.Contains(lGoChain2))
									{
										List<int> lSharedPoints = lGoChain.SharedLibertyPoints(lGoChain2);

										// sort list by best (near optimal) shared points to use

										if (lSharedPoints.Count >= 2)
										{
											List<int> lBestSharedPoints = new List<int>(2);

											// find best share points to use for the Miai Stragety

											// a. find points which only have one empty neightbor
											foreach (int lIndex in lSharedPoints)
											{
												int lEmptyNeighborCount = 0;
												foreach (int lNieghbor in Board.Coord.GetNeighbors(lIndex))
													if (accessibleLibertiesAvailable.Contains(lNieghbor))
														lEmptyNeighborCount++;

												if (lEmptyNeighborCount == 1)
												{
													lBestSharedPoints.Add(lIndex);
													if (lBestSharedPoints.Count == 2)
														break;
												}
											}

											// b. find point near the last use shared point (from previous step), if any
											if (lBestSharedPoints.Count == 1)
											{
												int lBestSharedPoint = lBestSharedPoints[0];
												foreach (int lNieghbor in Board.Coord.GetNeighbors(lBestSharedPoint))
													if (lSharedPoints.Contains(lNieghbor))
													{
														lBestSharedPoints.Add(lNieghbor);
														break;
													}
											}

											// c. use any remaining shared points (not always optimal, but okay)
											if (lBestSharedPoints.Count < 2)
												foreach (int lIndex in lSharedPoints)
													if (!lBestSharedPoints.Contains(lIndex))
													{
														lBestSharedPoints.Add(lIndex);
														if (lBestSharedPoints.Count == 2)
															break;
													}

											lSharedPoints = lBestSharedPoints;

											lGoChain.MergeGoChain(lGoChain2, lSharedPoints);
											lChanged.Add(lGoChain);
											lRemoved.Add(lGoChain2);

											miaiStrategy.Add(lSharedPoints[0], lSharedPoints[1]);

											lUsedLiberties.Remove(lSharedPoints[0]);
											lUsedLiberties.Remove(lSharedPoints[1]);
										}
									}

				}

				if (lChanged.Count == 0)
					break;

				foreach (GoChain lGoChain in lRemoved)
					lGoChains.Remove(lGoChain);

				lCheckList = lChanged;
			}

			if (lGoChains.Count > 1)
				return false;

			foreach (int lPoint in lUsedLiberties)
				accessibleLibertiesAvailable.Remove(lPoint);

			return true;
		}

		public bool IsIntersection(int move)
		{
			return Intersection.IsIntersection(this, move);
		}

		bool Intersection.IContains.Contains(int point)
		{
			foreach (GoBlockBase lGoBlockBase in Members)		// future - slow? try EnclosedArea if Members.Count > 2)
				if (lGoBlockBase.Members.Contains(point))
					return true;

			return false;
		}

		int Intersection.IContains.GetSize()
		{
			return Size;
		}

		int Intersection.IContains.GetWidth()
		{
			return Board.BoardSize;
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder();

			lStringBuilder.AppendLine(".." + Defender.ToString2() + " Color Enclosed Region #" + _RegionNbr + " At: " + Board.Coord.ToString(Members[0].MemberList[0]));

			lStringBuilder.Append("....Members Block: (" + Members.Count + ") ");
			foreach (GoBlockBase lGoBlockBase in Members)
				lStringBuilder.Append(Board.Coord.ToString(lGoBlockBase.Members.GetFirst()) + " ");
			lStringBuilder.AppendLine();

			lStringBuilder.Append("....Neighbors Blocks: (" + Neighbors.Count + ") ");
			foreach (GoBlockBase lGoBlockBase in Neighbors)
				lStringBuilder.Append(Board.Coord.ToString(lGoBlockBase.Members.GetFirst()) + " ");
			lStringBuilder.AppendLine();

			lStringBuilder.Append("....Enclosing Blocks: (" + EnclosingBlocks.Count + ") ");
			foreach (GoBlockBase lGoBlock in EnclosingBlocks)
				lStringBuilder.Append(Board.Coord.ToString(lGoBlock.Members.GetFirst()) + " ");
			lStringBuilder.AppendLine();

			lStringBuilder.Append("....Interior Defender Blocks: (" + InteriorDefenderBlocks.Count + ") ");
			foreach (GoBlockBase lGoBlock in InteriorDefenderBlocks)
				lStringBuilder.Append(Board.Coord.ToString(lGoBlock.Members.GetFirst()) + " ");
			lStringBuilder.AppendLine();

			lStringBuilder.Append("....Interior Attacker Blocks: (" + InteriorAttackerBlocks.Count + ") ");
			foreach (GoBlockBase lGoBlock in InteriorAttackerBlocks)
				lStringBuilder.Append(Board.Coord.ToString(lGoBlock.Members.GetFirst()) + " ");
			lStringBuilder.AppendLine();

			lStringBuilder.AppendLine("....Is Small Enclosed: " + IsSmall.ToString());
			//			lStringBuilder.AppendLine("....Total Empty Region: ");
			//			lStringBuilder.AppendLine(TotalEmptyRegion.ToString());

			lStringBuilder.AppendLine("....Is 1-Vital: " + Is1Vital.ToString());
			lStringBuilder.AppendLine("....Is 2-Vital: " + Is2Vital.ToString());

			lStringBuilder.AppendLine("....Has Interior: " + (EmptyArea.HasInterior() ? "Yes" : "No"));
			//			lStringBuilder.Append("Accessible Liberties:\n" + AccessibleLiberties.ToString());
			lStringBuilder.Append("Empty Area:\n" + EmptyArea.ToString());
			lStringBuilder.Append("Attackers Eye Area:\n" + AttackersEyeArea.ToString());
			lStringBuilder.Append("Enclosed Region:\n" + EnclosedArea.ToString());


			return lStringBuilder.ToString().TrimEnd('\n');
		}

		public void Dump()
		{
			Console.Error.WriteLine(ToString());
		}
	}
}
