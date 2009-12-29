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
	class GoBlock : GoBlockBase
	{
		public Region Liberties;

		/// <summary>
		/// Gets the stone count.
		/// </summary>
		/// <value>The stone count.</value>
		public int StoneCount
		{
			get
			{
				return Members.Count;
			}
		}

		/// <summary>
		/// Gets the liberty count.
		/// </summary>
		/// <value>The liberty count.</value>
		public int LibertyCount
		{
			get
			{
				return Liberties.Count;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GoBlock"/> class.
		/// </summary>
		/// <param name="goBoard">The board.</param>
		/// <param name="color">The color of the block.</param>
		public GoBlock(GoBoard goBoard, Color color)
			: base(goBoard, color)
		{
			Liberties = new Region(goBoard.BoardSize);
		}

		public bool InAtari()
		{
			return (LibertyCount == 1);
		}

		/// <summary>
		/// Determines whether the specified stone is member.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns>
		/// 	<c>true</c> if the specified stone is member; otherwise, <c>false</c>.
		/// </returns>
		public bool IsMember(int index)
		{
			return Members.Contains(index);
		}

		/// <summary>
		/// Determines whether the specified position is liberty.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns>
		/// 	<c>true</c> if the specified position is liberty; otherwise, <c>false</c>.
		/// </returns>
		public bool IsLiberty(int index)
		{
			return Liberties.Contains(index);
		}

		public bool IsEnemy(int index)
		{
			if (IsMember(index))
				return false;

			if (!IsNeighbor(index))
				return false;

			if (Board.Cells[index].Color != BlockColor.Opposite)
				return false;

			return true;
		}

		public bool IsAtariPoint(int index)
		{
			return ((LibertyCount == 1) && Liberties.Contains(index));
		}

		public bool IsNeighbor(int index)
		{
			foreach (int lIndex in Board.Coord.GetNeighbors(index))
				if (Members.Contains(lIndex))
					return true;

			return false;
		}

		public void AddStone(int index)
		{
			MemberList.Add(index);
			Members.Add(index);
			Liberties.Remove(index);

			foreach (int lIndex in Board.Coord.GetNeighbors(index))
				if (Board.Cells[lIndex].Color.IsEmpty)
					Liberties.Add(lIndex);
		}

		public void EnemyStoneCaptured(int index)
		{
			if (!IsNeighbor(index))
				return;

			Liberties.Add(index);
		}

		public void EnemyStonePlaced(int index)
		{
			if (!IsNeighbor(index))
				return;

			Liberties.Remove(index);
		}

		public override bool _SelfTest()
		{
			if (!_SelfTest2())
			{
				Console.Error.WriteLine("GoBlock.SelfTest Failed");
				Dump2();
				Board.Dump();

				return false;
			}
			return true;
		}

		public override bool _SelfTest2()
		{
			if (!base._SelfTest2())
				return false;

			for (int i = 0; i < Board.BoardSize * Board.BoardSize; i++)
				if (Liberties.Contains(i))
				{
					if (!Board.Cells[i].Color.IsEmpty)
						return false;

					if (!IsNeighbor(i))
						return false;
				}

			return true;
		}

		public void Dump2()
		{
			Console.Error.WriteLine("Block Color #" + BlockNbr.ToString() + " : " + BlockColor.ToString());
			Console.Error.WriteLine("Stones:");
			Console.Error.WriteLine(Members.ToString());
			Console.Error.WriteLine("Liberties:");
			Console.Error.WriteLine(Liberties.ToString());
		}

		public void Dump()
		{
			Console.Error.WriteLine("Block Color #" + BlockNbr.ToString() + " : " + BlockColor.ToString());

			for (int y = Board.BoardSize; y > 0; y--)
			{
				Console.Error.Write(y);

				if (y < 10)
					Console.Write(" ");

				Console.Error.Write(" : ");

				for (int x = 0; x < Board.BoardSize; x++)
				{
					int lIndex = Board.Coord.At(x, y - 1);

					if (IsMember(lIndex))
						Console.Error.Write(BlockColor.ToString());
					else if (IsLiberty(lIndex))
						Console.Error.Write('*');
					else if (IsEnemy(lIndex))
						Console.Error.Write(BlockColor.Opposite.ToString());
					else
						Console.Error.Write('.');
				}

				Console.Error.WriteLine();
			}
			Console.Error.Write("     ");
			Console.Error.WriteLine("ABCDEFGHJKLMNOPQURS".Substring(0, Board.BoardSize));

		}
	}
}
