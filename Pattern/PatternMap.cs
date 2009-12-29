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
	class PatternMap : IPatternAddInterface
	{
		protected struct PatternHit
		{
			public Pattern Pattern;
			public int Transformation;
			public Coordinate Origin;

			public PatternHit(Pattern pattern, int transformation, Coordinate origin)
			{
				Pattern = pattern;
				Transformation = transformation;
				Origin = origin;
			}
		}

		protected GoBoard Board;
		protected Color Player;
		protected PatternMergedAttributes[] MergedAttributes;
		protected List<PatternHit> Patterns;

		public PatternMap(GoBoard goBoard, Color player)
		{
			Board = goBoard;
			Player = player;
			Patterns = new List<PatternHit>();
			MergedAttributes = new PatternMergedAttributes[Board.Coord.BoardArea];
		}

		public void Add(Pattern pattern, int transformation, Coordinate origin, int location)
		{
			if (pattern.PatternCompiled.Match(Board, Player, origin, transformation) == PatternFunctions.FALSE)
				return;

			Patterns.Add(new PatternHit(pattern, transformation, origin));

			if (MergedAttributes[location] == null)
				MergedAttributes[location] = new PatternMergedAttributes();

			MergedAttributes[location].Merge(pattern);
		}

		public int GetValue(int location)
		{
			if (MergedAttributes[location] == null)
				return 0;

			return MergedAttributes[location].Value;
		}

		public void UpdateMoveList(MoveList moves)
		{
			foreach (PatternHit lPatternHit in Patterns)
				lPatternHit.Pattern.PatternCompiled.Execute(Board, Player, lPatternHit.Origin, lPatternHit.Transformation, moves);
		}

	}
}
