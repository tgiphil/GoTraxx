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
	public abstract class GoalBase
	{
		abstract public TriState IsGoalAccomplished();
	}

	class GoalSave : GoalBase
	{
		protected GoBoard Board;
		protected Color Player;
		protected int BlockPosition;

		public GoalSave(GoBoard goBoard, int pBlockPosition)
		{
			Board = goBoard;
			BlockPosition = pBlockPosition;
			Player = Board.GetColor(pBlockPosition);
		}

		public override TriState IsGoalAccomplished()
		{
			Color lColor = Board.GetColor(BlockPosition);
			SafetyStatus lSafety = Board.GetSafetyStatus(BlockPosition);

			if (lColor == Player)
			{
				if (lSafety.IsAlive)
					return TriState.True;

				if (lSafety.IsDead)
					return TriState.False;
			}
			else
				if (lColor == Player.Opposite)
				{
					if (lSafety.IsAlive)
						return TriState.False;

					if (lSafety.IsDead)
						return TriState.True;	// well, maybe (turned into safe territory)
				}
				else
					if (lColor == Color.Empty)
					{
						if (lSafety.IsTerritory && lSafety.Player == Player)
							return TriState.True;	// well, maybe (turned into safe territory)

						if (lSafety.IsTerritory && lSafety.Player == Player.Opposite)
							return TriState.False;	
					}

			return TriState.Unknown;
		}
	}

	class GoalNot : GoalBase
	{
		protected GoalBase Goal;

		public GoalNot(GoalBase pGoal)
		{
			Goal = pGoal;
		}

		public override TriState IsGoalAccomplished()
		{
			return Goal.IsGoalAccomplished().Not;
		}

	}

	class GoalBoth : GoalBase
	{
		protected GoalBase Goal1;
		protected GoalBase Goal2;

		public GoalBoth(GoalBase pGoal1, GoalBase pGoal2)
		{
			Goal1 = pGoal1;
			Goal2 = pGoal2;
		}

		public override TriState IsGoalAccomplished()
		{
			return (Goal1.IsGoalAccomplished() & Goal2.IsGoalAccomplished());
		}

	}

	class GoalEither : GoalBase
	{
		protected GoalBase Goal1;
		protected GoalBase Goal2;

		public GoalEither(GoalBase pGoal1, GoalBase pGoal2)
		{
			Goal1 = pGoal1;
			Goal2 = pGoal2;
		}

		public override TriState IsGoalAccomplished()
		{
			return (Goal1.IsGoalAccomplished() | Goal2.IsGoalAccomplished());
		}

	}
}
