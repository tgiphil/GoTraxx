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
	static class SimpleBoardEvaluator
	{
		/// <summary>
		/// Gets the score.
		/// </summary>
		/// <param name="goBoard">The go board.</param>
		/// <returns></returns>
		public static int GetScore2(GoBoard goBoard)
		{
			// Score should be positive if black is winning
			int lStone = 0;
			int lCaptures = goBoard.CapturedStoneCnt[1] - goBoard.CapturedStoneCnt[0];

			for (int i = 0; i < goBoard.Coord.BoardArea; i++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(i);

				if (lSafetyStatus.IsAlive)
					if (lSafetyStatus.IsBlack)
						lStone++;
					else
						lStone--;
				else
					if (lSafetyStatus.IsDead)
						if (lSafetyStatus.IsBlack)
							lStone--;
						else
							lStone++;
					else
						if (lSafetyStatus.IsTerritory)
							if (lSafetyStatus.IsBlack)
								lStone++;
							else
								lStone--;

			}

			return lStone + lCaptures;
		}

		/// <summary>
		/// Gets the score.
		/// </summary>
		/// <param name="goBoard">The go board.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public static int GetScore(GoBoard goBoard)
		{
			return GetScore2(goBoard);
		}

		/// <summary>
		/// Gets the score.
		/// </summary>
		/// <param name="goBoard">The go board.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public static int GetScore(GoBoard goBoard, Color color)
		{
			if (color.IsBlack)
				return GetScore2(goBoard);
			else
				return -GetScore2(goBoard);
		}

		/// <summary>
		/// Evaulates the board position.
		/// </summary>
		/// <param name="goBoard">The go board.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public static int EvaulateBoardPosition(GoBoard goBoard)
		{
			int[] StoneCaptured = new int[2];
			int[] AliveStones = new int[2];
			int[] DeadStones = new int[2];
			int[] Territory = new int[2];
			//			int[] UnknownStones = new int[2];

			// Score should be positive if black is winning
			StoneCaptured[0] = goBoard.CapturedStoneCnt[0];
			StoneCaptured[1] = goBoard.CapturedStoneCnt[1];

			for (int i = 0; i < goBoard.Coord.BoardArea; i++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(i);

				if (lSafetyStatus.IsAlive)
					AliveStones[lSafetyStatus.Player.ToInteger()]++;
				else
					if (lSafetyStatus.IsDead)
						DeadStones[lSafetyStatus.Player.ToInteger()]++;
					else
						if (lSafetyStatus.IsTerritory)
							Territory[lSafetyStatus.Player.ToInteger()]++;
				//						else
				//							if ((lSafetyStatus.IsUndecided) && (lSafetyStatus.Player != Color.Empty))
				//								UnknownStones[lSafetyStatus.Player.ToInteger()]++;
			}

			double lScore = Territory[0] - Territory[1] + AliveStones[0] - AliveStones[1] - DeadStones[0] + DeadStones[1];

			if (StoneCaptured[0] != StoneCaptured[1])
				if (StoneCaptured[0] > StoneCaptured[1])
					lScore = lScore + 0.5;
				else
					lScore = lScore - 0.5;

			return Convert.ToInt32(lScore * 10);

		}
	}
}
