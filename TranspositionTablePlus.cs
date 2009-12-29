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
	class TranspositionTablePlus
	{
		public enum NodeType : byte { Unknown = 0, Exact, UpperBound, LowerBound };
		public static int Unknown = Int32.MaxValue;
		public static Node EmptyNode = new Node(0, 0, NodeType.Unknown, 0, 0);

		public struct Node
		{
			public Int64 ZobristKey;
			public int Value;
			public byte Height;
			public NodeType Flag;
			public int Move;

			public Node(int height, int value, NodeType flag, int move, Int64 zobristKey)
			{
				Height = (byte)height;
				Value = value;
				Flag = flag;
				ZobristKey = zobristKey;
				Move = move;
			}
		}

		protected Node[] Table;
		protected int TableSize;

		public int Size
		{
			get
			{
				return TableSize;
			}
		}

		public TranspositionTablePlus(int tableSize)
		{
			TableSize = tableSize;
			Table = new Node[tableSize];
		}

		public Node Retrieve(ZobristHash zobristKey)
		{
			Node lEntry = Table[(int)zobristKey.HashValue % TableSize];

			if (lEntry.Flag != NodeType.Unknown)
				if (lEntry.ZobristKey == zobristKey.HashValue)
					return lEntry;

			return EmptyNode;
		}

		public void Record(int height, int value, NodeType flag, int move, ZobristHash zobristKey)
		{
			int lIndex = (int)zobristKey.HashValue % TableSize;

			bool lReplace = true;

			if (Table[lIndex].Flag != NodeType.Unknown)
				if (Table[lIndex].ZobristKey == zobristKey.HashValue)
					if (height <= Table[lIndex].Height)
						lReplace = false;

			if (lReplace)
			{
				Table[lIndex].ZobristKey = zobristKey.HashValue;
				Table[lIndex].Value = value;
				Table[lIndex].Height = (byte)height;
				Table[lIndex].Flag = flag;
				Table[lIndex].Move = move;
			}
		}

	}
}
