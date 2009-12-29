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
	interface ISearchInterface
	{
		/// <summary>
		/// Initializes the interface.
		/// </summary>
		/// <param name="goBoard">The Go board.</param>
		/// <param name="searchInstanceOptions">The search instance options.</param>
		void Initialize(GoBoard goBoard, SearchOptions searchInstanceOptions);

		/// <summary>
		/// Gets a list of legal moves.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		MoveList GetMoveList(Color playerToMove, bool forceEndGameMoves);

		/// <summary>
		/// Prunes moves from the list.
		/// </summary>
		/// <param name="moves">The moves.</param>
		void PruneMoveList(MoveList moves);

		/// <summary>
		/// Sorts the move list.
		/// </summary>
		/// <param name="moves">The moves.</param>
		void SortMoveList(MoveList moves);

		/// <summary>
		/// Evaulutes the board
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		int Evaluate(Color playerToMove);

		/// <summary>
		/// Determines whether the game is over, or that the goal has been accomplished
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is over; otherwise, <c>false</c>.
		/// </returns>
		bool IsGameOver();

		void PrioritizeMoves(MoveList moves, Color playerToMove);
	}
}
