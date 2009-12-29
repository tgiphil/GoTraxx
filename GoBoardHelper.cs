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
	static class GoBoardHelper
	{

		public static List<int> GetMovesInOpponentsTerritory(GoBoard goBoard, Color playerToMove)
		{
			// useful for force moves in endgame
			List<int> lMoves = new List<int>();

			for (int lPoint = 0; lPoint < goBoard.Coord.BoardArea; lPoint++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(lPoint);

				if ((lSafetyStatus.IsTerritory) && (lSafetyStatus.Player == playerToMove.Opposite))
					if (goBoard.IsLegal(lPoint, playerToMove))
						lMoves.Add(lPoint);
			}

			return lMoves;
		}

		public static List<int> GetProtectedLiberites(GoBoard goBoard, Color color)
		{
			List<int> lProtected = new List<int>();

			for (int lIndex = 0; lIndex < goBoard.Coord.BoardArea; lIndex++)
				if (goBoard.IsProtectedLiberty(lIndex, color))
					lProtected.Add(lIndex);

			return lProtected;
		}

		public static List<int> GetNeighborsOfProtectedLiberites(GoBoard goBoard, Color color)
		{
			List<int> lNeighbors = new List<int>();

			for (int lPoint = 0; lPoint < goBoard.Coord.BoardArea; lPoint++)
				if (goBoard.IsProtectedLiberty(lPoint, color))
					foreach (int lNeighbor in goBoard.Coord.GetNeighbors(lPoint))
						if (!lNeighbors.Contains(lNeighbor))
							lNeighbors.Add(lNeighbor);

			return lNeighbors;
		}

		public static List<int> GetNeighbors(GoBoard goBoard, List<int> points)
		{
			List<int> lNeighbors = new List<int>();

			foreach (int lPoint in points)
				foreach (int lNeighbor in goBoard.Coord.GetNeighbors(lPoint))
					if (!lNeighbors.Contains(lNeighbor))
						lNeighbors.Add(lNeighbor);

			return lNeighbors;
		}

		// marked to be depreciated
		public static List<int> GetLegalSafeMoves(GoBoard goBoard, Color playerToMove)
		{
			List<int> lMoves = new List<int>();

			for (int i = 0; i < goBoard.Coord.BoardArea; i++)
				if (goBoard.IsLegal(i, playerToMove))
				{
					SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(i);

					if (lSafetyStatus.IsUndecided || (lSafetyStatus.IsUnsurroundable && !lSafetyStatus.IsDame))
						lMoves.Add(i);
				}

			return lMoves;
		}

		// marked to be depreciated
		public static List<int> FillDame(GoBoard goBoard, Color playerToMove)
		{
			List<int> lMoves = new List<int>();
			for (int i = 0; i < goBoard.Coord.BoardArea; i++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(i);

				if (lSafetyStatus.IsDame || lSafetyStatus.IsUnsurroundable)
					lMoves.Add(i);
			}

			return lMoves;
		}

		// marked to be depreciated
		public static List<int> CaptureDeadStones(GoBoard goBoard, Color playerToMove)
		{
			List<int> lMoves = new List<int>();
			for (int i = 0; i < goBoard.Coord.BoardArea; i++)
				if (goBoard.GetSafetyStatus(i).IsDead)
					if (goBoard.Cells[i].Color != playerToMove)
						lMoves.Add(i);

			return lMoves;
		}

		// marked to be depreciated
		public static List<int> GetSafeMovesInOwnTerritory(GoBoard goBoard, Color playerToMove)
		{
			// useful for force moves in endgame
			List<int> lMoves = new List<int>();

			for (int lPoint = 0; lPoint < goBoard.Coord.BoardArea; lPoint++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(lPoint);

				if ((lSafetyStatus.IsTerritory) && (lSafetyStatus.Player == playerToMove))
					if (goBoard.IsLegal(lPoint, playerToMove))
					{
						goBoard.PlayStone(lPoint, playerToMove, true);

						SafetyStatus lSafetyStatusAfter = goBoard.GetSafetyStatus(lPoint);

						// check if still safe after move
						if (lSafetyStatusAfter.IsAlive) // && (lSafetyStatus.Player == playerToMove))
							lMoves.Add(lPoint);

						goBoard.Undo();
					}

			}
			return lMoves;
		}

		// marked to be depreciated
		public static int FindAnyMoveInOpponentsTerritory(GoBoard goBoard, Color playerToMove)
		{
			// useful for force moves in endgame
			for (int lPoint = 0; lPoint < goBoard.Coord.BoardArea; lPoint++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(lPoint);

				if ((lSafetyStatus.IsTerritory) && (lSafetyStatus.Player == playerToMove.Opposite))
					if (goBoard.IsLegal(lPoint, playerToMove))
						return lPoint;
			}

			return CoordinateSystem.PASS;
		}

		// marked to be depreciated
		public static int FindAnySafeInOwnTerritory(GoBoard goBoard, Color playerToMove)
		{
			// useful for force moves in endgame
			for (int lPoint = 0; lPoint < goBoard.Coord.BoardArea; lPoint++)
			{
				SafetyStatus lSafetyStatus = goBoard.GetSafetyStatus(lPoint);

				if ((lSafetyStatus.IsTerritory) && (lSafetyStatus.Player == playerToMove))
					if (goBoard.IsLegal(lPoint, playerToMove))
					{
						goBoard.PlayStone(lPoint, playerToMove, true);

						SafetyStatus lSafetyStatusAfter = goBoard.GetSafetyStatus(lPoint);

						goBoard.Undo();

						// it's still safe after move, so return it
						if ((lSafetyStatus.IsAlive) && (lSafetyStatus.Player == playerToMove))
							return lPoint;
					}
			}

			return CoordinateSystem.PASS;
		}

	}
}
