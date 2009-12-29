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
	class PrincipalVariation
	{
		static public int MAX_DEPTH = 19 * 19; // unlikely to have more than MAX_DEPTH
		static public int NO_VALUE = -256;

		// triangular PV array
		protected int[,] pv;
		protected int[] pvLength;
		protected int BoardSize;

		public PrincipalVariation(int boardSize)
		{
			Clear(MAX_DEPTH);
			BoardSize = boardSize;
		}

		/// <summary>
		/// Clears the Principal Variation.
		/// </summary>
		/// <param name="maxDepth">The max depth.</param>
		public void Clear(int maxDepth)
		{
			pv = new int[maxDepth, maxDepth];
			pvLength = new int[maxDepth];
			pv[0, 0] = NO_VALUE;
		}

		/// <summary>
		/// Updates the Principial Variation, which contains the best moves.
		/// </summary>
		/// <param name="depth">The Depth.</param>
		/// <param name="move">The Move.</param>
		public void UpdatePV(int depth, int move)
		{
			// update the PV
			pv[depth, depth] = move;
			for (int j = depth + 1; j < pvLength[depth + 1]; j++)
				pv[depth, j] = pv[depth + 1, j];

			pvLength[depth] = pvLength[depth + 1];
		}

		/// <summary>
		/// Sets the length of the current depth to the depth itself 
		/// </summary>
		/// <param name="depth">The Depth.</param>
		public void SetPVLength(int depth)
		{
			pvLength[depth] = depth;
		}

		/// <summary>
		/// Gets the best move.
		/// </summary>
		/// <value>The best move.</value>
		public int BestMove
		{
			get
			{
				return pv[0, 0];
			}
		}

		/// <summary>
		/// Returns the move in the Principial Variation line, for the given depth.
		/// </summary>
		/// <param name="depth">The Depth.</param>
		/// <returns></returns>
		public int GetMove(int depth)
		{
			return pv[0, depth];
		}

		/// <summary>
		/// Gets the principle variation line.
		/// </summary>
		/// <returns></returns>
		public string GetPVLine()
		{
			StringBuilder lPVLine = new StringBuilder();

			for (int lPly = 0; lPly < pvLength[0]; lPly++)
				lPVLine.Append(CoordinateSystem.ToString2(pv[0,lPly],BoardSize)+" ");

			return lPVLine.ToString();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterPriority>2</filterPriority>
		public override string ToString()
		{
			StringBuilder lTriangle = new StringBuilder();

			for (int lCol = 0; lCol < 7; lCol++)
				lTriangle.Append(lCol + "       ");

			lTriangle.AppendLine();


			for (int lRow = 0; lRow < 7; lRow++)
			{
				lTriangle.Append(lRow);

				for (int lCol = 0; lCol < 7; lCol++)
				{
					if (pv[lRow, lCol] != NO_VALUE)
					{
						lTriangle.Append(CoordinateSystem.ToString2(pv[lRow,lCol],BoardSize));
					}
					else
					{
						lTriangle.Append("------");
					}
					lTriangle.Append(" ");
				}

				lTriangle.Append("(" + pvLength[lRow] + ") \n");
			}

			return lTriangle.ToString();
		}
	}
}
