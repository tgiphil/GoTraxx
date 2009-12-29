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
	class GoEmptyBlock : GoBlockBase
	{

		public int EmptySpaceCount
		{
			get
			{
				return MemberList.Count;
			}
		}

		public GoEmptyBlock(GoBoard goBoard)
			: base(goBoard, Color.Empty)
		{
			Members.AddAll();

			for (int i = 0; i < Board.Coord.BoardArea; i++)
				MemberList.Add(i);
		}

		public GoEmptyBlock(GoBlock goBlock)
			: base(goBlock.Board, Color.Empty)
		{
			Members = new Region(goBlock.Members);
			MemberList = Members.ToList();	// slow might be a faster way
		}

		public GoEmptyBlock(GoEmptyBlock goEmptyBlock, int pCutPoint, int direction)
			: base(goEmptyBlock.Board, Color.Empty)
		{
			Members = new Region(goEmptyBlock.Members, pCutPoint, direction);
			MemberList = Members.ToList();
		}

		public bool IsMember(int index)
		{
			return Members.Contains(index);
		}

		public bool IsNeighbor(int index)
		{
			foreach (int lIndex in Board.Coord.GetNeighbors(index))
				if (Members.Contains(lIndex))
					return true;

			return false;
		}

		public void RemoveMember(int move)
		{
			Members.Remove(move);
			MemberList.Remove(move);
		}

		public bool IsCutPoint(int move)
		{
			return Members.IsIntersection(move);
		}

		public override bool _SelfTest()
		{
			if (!_SelfTest2())
			{
				Console.WriteLine("GoEmptyBlock.SelfTest Failed");
				Dump2();
				Board.Dump();
				//				Dump();
				//				Dump2();
				return false;
			}
			return true;
		}

		public void Dump2()
		{
			Console.WriteLine("Empty Block #" + BlockNbr.ToString());
			Console.WriteLine("Empty:");
			Console.WriteLine(Members.ToString());
			
		}

	}
}
