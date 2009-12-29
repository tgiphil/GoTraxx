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
	
	static class Intersection
	{
		public interface IContains
		{
			bool Contains(int point);
			int GetSize();
			int GetWidth();
		}

		public static bool IsIntersection(IContains inter, int move)
		{
			if (inter.GetSize() <= 2)
				return false;

			int lWidth = inter.GetWidth();

			int ActiveMice = 0;

			int[] lMousePosition = new int[4];
			int[] lMouseDirection = new int[4];

			int[] lMouseNextDirection = new int[4];
			int[] lMouseByDirection = new int[4];

			CoordinateSystem lCoord = CoordinateSystem.GetCoordinateSystem(lWidth); 
			
			for (int i = 0; i < 4; i++)
			{
				int lNeightbor = lCoord.GetNeighbor(move, i);

				if (lCoord.OnBoard(lNeightbor) && inter.Contains(lNeightbor))
				{
					lMousePosition[ActiveMice] = lNeightbor;
					lMouseDirection[ActiveMice] = i;
					lMouseByDirection[i] = ActiveMice;

					if (ActiveMice > 0)
						lMouseNextDirection[ActiveMice - 1] = i;

					ActiveMice++;
				}
				else
					lMouseByDirection[i] = -1;	// no mouse
			}

			if (ActiveMice == 1)
				return false;

			lMouseNextDirection[ActiveMice - 1] = 0; // it's never used!

			for (int CurrentMouse = 0; CurrentMouse < ActiveMice - 1; CurrentMouse++)
			{
				int CurrentMousePosition = lMousePosition[CurrentMouse];
				int CurrentMouseDirection = lMouseDirection[CurrentMouse];
				int CurrentMouseOriginalDirection = CurrentMouseDirection;

				while (CurrentMousePosition != move)
				{
					int lRightDirection = CoordinateSystem.TurnClockWise(CurrentMouseDirection);
					int lRightPosition = lCoord.GetNeighbor(CurrentMousePosition, lRightDirection);

					// if (can turn right & move)
					if (CoordinateSystem.OnBoard(lRightPosition, lWidth) && inter.Contains(lRightPosition))
					{
						// yes, turn turn and move
						CurrentMousePosition = lRightPosition;
						CurrentMouseDirection = lRightDirection;
					}
					else
					{
						while (true)
						{
							int lStraightPosition = lCoord.GetNeighbor(CurrentMousePosition, CurrentMouseDirection);

							// if (can go straight) (
							if (CoordinateSystem.OnBoard(lStraightPosition, lWidth) && inter.Contains(lStraightPosition))
							{
								// yes, go straight
								CurrentMousePosition = lStraightPosition;

								break;
							}
							else
							{
								// else, change direction clockwise and try-again (loop)
								CurrentMouseDirection = CoordinateSystem.TurnCounterClockWise(CurrentMouseDirection);
							}
						}
					}
				}

				// right back where we started
				int FromDirection = CoordinateSystem.TurnAround(CurrentMouseDirection);

				// if mice returned from a direction other than the next direction, then there is a split
				if (lMouseNextDirection[CurrentMouse] != FromDirection)
					return true;

				// if mouse came back from original direction then there is a split
				if (CurrentMouseOriginalDirection == FromDirection)
					return true;	// should never reach here

				// by now we know that the next mouse must merge, so no split
				if (CurrentMouse == ActiveMice - 1)
					return false;
			}

			return false;
		}
	}
}
