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
	class Region : System.Collections.IEnumerable, Intersection.IContains
	{
		public enum MergeType { And, Or, NAnd };
		protected int Width;
		protected bool[] Grid;
		public int Count;

		public void Add(int index)
		{
			if (!Grid[index])
			{
				Grid[index] = true;
				Count++;
			}
		}

		public void Remove(int index)
		{
			if (Grid[index])
			{
				Count--;
				Grid[index] = false;
			}
		}

		public Region(int regionSize)
		{
			Width = regionSize;
			Grid = new bool[Width * Width];
			Count = 0;
		}

		public Region(Region region)
		{
			Width = region.Width;
			Count = region.Count;
			Grid = new bool[Width * Width];
			region.Grid.CopyTo(Grid, 0);
		}


		public Region(Region region1, Region region2, MergeType mergeType)
		{
			Width = region1.Width;
			Count = region1.Count;
			Grid = new bool[Width * Width];

			if (region1.Width != region2.Width)
				return;	// error: can't merge unlike regions

			if (mergeType == MergeType.And)
			{
				for (int i = 0; i < Width * Width; i++)
					if ((region1.Grid[i]) && (region2.Grid[i]))
						Add(i);
			}
			else
				if (mergeType == MergeType.Or)
				{
					for (int i = 0; i < Width * Width; i++)
						if ((region1.Grid[i]) || (region2.Grid[i]))
							Add(i);
				}
				else
					if (mergeType == MergeType.NAnd)
					{
						for (int i = 0; i < Width * Width; i++)
							if ((region1.Grid[i]) && (!region2.Grid[i]))
								Add(i);
					}
		}

		public Region(StringMap stringMap)
		{
			Width = (stringMap.Height > stringMap.Width) ? stringMap.Height : stringMap.Width;
			Grid = new bool[Width * Width];
			Count = 0;

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Width; y++)
					if ((stringMap.Get(x, y) != ' ') && (stringMap.Get(x, y) != '.'))
						Add(CoordinateSystem.At(x, y, Width));

			//			Console.WriteLine(pStringMap.ToString());
		}

		public Region(Region region, int cutPoint, int start)
		{
			Width = region.Width;
			Count = 0;
			Grid = new bool[Width * Width];

			CoordinateSystem lCoord = CoordinateSystem.GetCoordinateSystem(Width);

			if (!lCoord.OnBoard(start))
				return;

			Stack<int> lFill = new Stack<int>(); 
			lFill.Push(start);

			while (lFill.Count > 0)
			{
				int lAt = lFill.Pop();
				Add(lAt);

				foreach (int lNeighbor in lCoord.GetNeighbors(lAt))
					if ((lNeighbor != cutPoint)
						&& (region.Contains(lNeighbor))
						&& (!Contains(lNeighbor)))
						lFill.Push(lNeighbor);
			}
		}

		public void Add(Region region)
		{
			if (region.Width != Width)
				return;	// error: can't merge unlike regions

			for (int i = 0; i < Width * Width; i++)
				if (region.Grid[i] == true)
					Add(i);
		}

		public void Add(List<Region> regions)
		{
			foreach (Region lRegion in regions)
				Add(lRegion);
		}

		public void AddAll()
		{
			Count = Width * Width;

			for (int i = 0; i < Count; i++)
				Grid[i] = true;
		}

		public void Clear()
		{
			Count = 0;
			for (int i = 0; i < Count; i++)
				Grid[i] = true;
		}

		public bool Contains(int index)
		{
			return Grid[index];
		}

		public void And(Region region)
		{
			if ((Count == 0) || (region.Count == 0))
				return;

			if (region.Width != Width)
				return;	// error: can't merge unlike regions

			for (int i = 0; i < Width * Width; i++)
				if ((region.Contains(i)) && (Contains(i)))
					Add(i);
				else
					Remove(i);
		}

		public void Or(Region region)
		{
			if ((Count == 0) && (region.Count == 0))
				return;

			if (region.Width != Width)
				return;	// error: can't merge unlike regions

			for (int i = 0; i < Width * Width; i++)
				if (region.Contains(i))
					Add(i);
		}

		public void Remove(Region region)
		{
			if (region.Width != Width)
				return;	// error: can't merge unlike regions

			if ((Count == 0) || (region.Count == 0))
				return;

			for (int i = 0; i < Width * Width; i++)
				if (region.Contains(i))
					Remove(i);
		}

		public bool Intersects(Region region)
		{
			if ((Count == 0) || (region.Count == 0))
				return false;

			for (int i = 0; i < Width * Width; i++)
				if (region.Contains(i) && Contains(i))
					return true;

			return false;
		}

		public bool IsSame(Region region)
		{
			if (Count != region.Count)
				return false;

			for (int i = 0; i < Width * Width; i++)
				if (region.Contains(i) != Contains(i))
					return false;

			return true;
		}

		public bool IsInterior(int x, int y)
		{
			return IsInterior(CoordinateSystem.At(x, y, Width));
		}

		public bool IsInterior(int index)
		{
			if (Count == 0)
				return false;

			if (!Contains(index))
				return false;

			foreach (int lNeighbot in CoordinateSystem.GetCoordinateSystem(Width).GetNeighbors(index))
				if (!Contains(lNeighbot))
					return false;

			return true;
		}

		public Region GetInterior()
		{
			Region lRegion = new Region(this.Width);

			if (Count != 0)
				for (int i = 0; i < Width * Width; i++)
					if (IsInterior(i))
						lRegion.Add(i);

			return lRegion;
		}

		public Region GetEdges()
		{
			Region lRegion = new Region(this.Width);

			for (int i = 0; i < Width * Width; i++)
				if (!IsInterior(i))
					lRegion.Add(i);

			return lRegion;
		}

		public bool HasInterior()
		{
			if (Count == 0)
				return false;

			for (int i = 0; i < Width * Width; i++)
				if (IsInterior(i))
					return true;

			return false;
		}

		public bool IsAdjacent(int index)
		{
			foreach (int lNeighbor in CoordinateSystem.GetCoordinateSystem(Width).GetNeighbors(index))
				if (Contains(lNeighbor))
					return true;

			return false;
		}

		public bool IsOnlyAdjacent(Region region)
		{
			if ((Count == 0) || (region.Count == 0))
				return false;

			for (int i = 0; i < Width * Width; i++)
				if (region.Contains(i))
					if (!IsAdjacent(i))
						return false;

			return true;
		}

		public bool IsIntersection(int x, int y)
		{
			return IsIntersection(CoordinateSystem.At(x, y, Width));
		}

		public bool IsIntersection(int move)
		{
			return Intersection.IsIntersection(this, move);
		}

		public int GetFirst()
		{
			if (Count == 0)
				return CoordinateSystem.PASS;

			for (int i = 0; i < Width * Width; i++)
				if (Grid[i])
					return i;

			return CoordinateSystem.PASS;	// should never get here
		}

		public List<int> ToList()
		{
			List<int> lResult = new List<int>(Count);

			if (Count == 0)
				return lResult;

			int lLeft = Count;

			for (int i = 0; i < Width * Width; i++)
				if (Grid[i])
				{
					lResult.Add(i);
					if (--lLeft == 0)
						return lResult;
				}

			return lResult; // should never get here
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			if (Count == 0)
				yield break;

			int lLeft = Count;

			for (int i = 0; i < Width * Width; i++)
				if (Grid[i])
				{
					yield return i;
					if (--lLeft == 0)
						yield break;
				}
		}

		bool Intersection.IContains.Contains(int point)
		{
			return Contains(point);
		}

		int Intersection.IContains.GetSize()
		{
			return Count;
		}

		int Intersection.IContains.GetWidth()
		{
			return Width;
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder();

			for (int y = Width; y > 0; y--)
			{
				lStringBuilder.Append(y);

				if (y < 10)
					lStringBuilder.Append(" ");

				lStringBuilder.Append(" : ");

				for (int x = 0; x < Width; x++)
					if (Contains(CoordinateSystem.At(x, y - 1, Width)))
						lStringBuilder.Append('X');
					else
						lStringBuilder.Append('.');

				lStringBuilder.AppendLine();
			}
			lStringBuilder.Append("     ");
			lStringBuilder.AppendLine("ABCDEFGHJKLMNOPQURS".Substring(0, Width));

			return lStringBuilder.ToString();
		}
	}
}
