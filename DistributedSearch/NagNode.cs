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

	class NagNode
	{
		public int Alpha;
		public int Beta;
		public int Depth;
		public Color PlayerToMove;
		public List<KeyValuePair<Color, int>> MoveList = new List<KeyValuePair<Color, int>>();
		public ZobristHash ZobristHash;

		public int StartDepth;

		public int NarrowedAlpha;
		public int NarrowedBeta;

		public int PermutationNbr;

		public WorkerProxy Worker = null;

		public int Result;
		public int BestMove;

		public NagNode(GoBoard goBoard, int alpha, int beta, Color playerToMove, int startDepth, int depth, int permutationNbr)
		{
			foreach (KeyValuePair<Color, int> lMove in goBoard.MoveList)
				MoveList.Add(new KeyValuePair<Color, int>(lMove.Key, lMove.Value));

			Alpha = alpha;
			Beta = beta;
			PlayerToMove = playerToMove;
			Depth = depth;
			ZobristHash = goBoard.ZobristHash.Clone();
			NarrowedAlpha = alpha;
			NarrowedBeta = beta;
			StartDepth = startDepth;
			PermutationNbr = 1;
		}

		public NagNode(NagNode nagNode, int permutationNbr)
		{
			MoveList = nagNode.MoveList;

			Alpha = nagNode.Alpha;
			Beta = nagNode.Beta;
			PlayerToMove = nagNode.PlayerToMove;
			Depth = nagNode.Depth;
			ZobristHash = nagNode.ZobristHash;
			NarrowedAlpha = nagNode.NarrowedAlpha;
			NarrowedBeta = nagNode.NarrowedBeta;
			StartDepth = nagNode.StartDepth;
			PermutationNbr = permutationNbr;
		}

		public void SetResult(int result, int bestMove)
		{
			Result = result;
			BestMove = bestMove;
		}

		public bool IsNarrowed()
		{
			return (Alpha != NarrowedAlpha) || (Beta != NarrowedBeta);
		}
	}
}

