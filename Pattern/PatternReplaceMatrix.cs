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
	class PatternMergeMatrix
	{
		protected int[] Map;
		protected bool Conflicted = false;

		protected static int AVAILABLE = -1;
		protected static int CONFLICTED = -2;

		public PatternMergeMatrix(int boardSize)
		{
			Map = new int[boardSize];

			for (int i = 0; i < boardSize; i++)
				Map[i] = AVAILABLE;

			Conflicted = false;
		}

		public void Replace(int a, int b)
		{
			if (a == b)
				return;

			if (Map[a] == AVAILABLE)
				Map[a] = b;
			else
				if (Map[a] != b)
					Map[a] = CONFLICTED;
		}

		public int this[int arg]
		{
			get
			{
				return Map[arg];
			}
		}

		public bool IsConflicted()
		{
			return Conflicted;
		}

		public void Compact()
		{
			for (int a = 0; a < Map.Length; a++)
				if (Map[a] == CONFLICTED)
					Conflicted = true;
				else
					if (Map[a] != AVAILABLE)
					{
						for (; ; )
						{
							int b = Map[Map[a]];

							if (b == AVAILABLE)
								break;

							if (a == b)
							{
								Map[a] = CONFLICTED;
								Conflicted = true;
								break;
							}

							Map[a] = b;
						}

					}
		}
	}
}
