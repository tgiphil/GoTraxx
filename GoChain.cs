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
	class GoChain
    {
        protected List<GoBlock> Blocks;
        protected Region Liberities;
        protected List<int> UsedLiberities;

        public GoChain(GoBlock goBlock, Region AccessibleLiberties)
        {
            Blocks = new List<GoBlock>();
            Blocks.Add(goBlock);
            Liberities = new Region(goBlock.Liberties, AccessibleLiberties, Region.MergeType.And);
            UsedLiberities = new List<int>();
        }

        public void AddBlock(GoBlock goBlock)
        {
            Blocks.Add(goBlock);

            if (Liberities != null)
                Liberities.Add(goBlock.Liberties);
            else
                Liberities = new Region(goBlock.Liberties);
        }

        public void MergeGoChain(GoChain goChain, List<int> sharedPoints)
        {
            foreach (GoBlock lGoBlock in goChain.Blocks)
                AddBlock(lGoBlock);

            foreach (int lPoint in goChain.UsedLiberities)
                UsedLiberities.Add(lPoint);

            UsedLiberities.Add(sharedPoints[0]);
            UsedLiberities.Add(sharedPoints[1]);
        }

        public void MergeGoChain(GoChain goChain)
        {
            foreach (GoBlock lGoBlock in goChain.Blocks)
                AddBlock(lGoBlock);

            foreach (int lPoint in goChain.UsedLiberities)
                UsedLiberities.Add(lPoint);
        }

        public List<int> SharedLibertyPoints(GoChain goChain)
        {
            List<int> lSharedLibertyPoints = new List<int>();

            if ((Liberities.Count <= 1) || (goChain.Liberities.Count <= 1))
                return lSharedLibertyPoints;

            Region lShared = new Region(Liberities, goChain.Liberities, Region.MergeType.And);

            foreach (int lPoint in UsedLiberities)
                lShared.Remove(lPoint);

            foreach (int lPoint in goChain.UsedLiberities)
                lShared.Remove(lPoint);

            lSharedLibertyPoints = lShared.ToList();

            return lSharedLibertyPoints;
        }

        public bool IsLiberty(int point)
        {
            return Liberities.Contains(point);
        }

        public override string ToString()
        {
            StringBuilder lStringBuilder = new StringBuilder();

            lStringBuilder.Append("Blocks: (" + Blocks.Count + ")");
            foreach (GoBlock lGoBlock in Blocks)
                lStringBuilder.Append(" " + lGoBlock.Board.Coord.ToString(lGoBlock.Members.GetFirst()));

            return lStringBuilder.ToString();
        }
    }
}
