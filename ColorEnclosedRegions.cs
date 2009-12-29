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
	class ColorEnclosedRegions
	{
		public List<ColorEnclosedRegion>[] Regions;
		protected GoBoard Board;

		public ColorEnclosedRegions(GoBoard goBoard)
		{
			Board = goBoard;
			Regions = new List<ColorEnclosedRegion>[2];

			foreach (Color lColor in Color.Colors)
				CreateRegions(lColor);
		}
		
		protected void CreateRegions(Color color)
		{
			int lColorIndex = color.ToInteger();

			Regions[lColorIndex] = new List<ColorEnclosedRegion>();
			List<GoEmptyBlock> lReviewed = new List<GoEmptyBlock>();

			foreach (GoBlockBase lGoBlockBase in Board.AllBlocks)
			{
				if (lGoBlockBase.IsEmptyBlock())
				{
					GoEmptyBlock lGoEmptyBlock = (GoEmptyBlock)lGoBlockBase;
					if (!lReviewed.Contains(lGoEmptyBlock))
					{
						ColorEnclosedRegion lColorEnclosedRegion = new ColorEnclosedRegion(lGoEmptyBlock, color);

						Regions[lColorIndex].Add(lColorEnclosedRegion);

						foreach (GoBlockBase lGoBlockBase2 in lColorEnclosedRegion.Members)
							if (lGoBlockBase2.IsEmptyBlock())
								lReviewed.Add((GoEmptyBlock)lGoBlockBase2);
					}
				}
			}
		}

		public ColorEnclosedRegion FindRegion(Color color, int index)
		{
			foreach (ColorEnclosedRegion lColorEnclosedRegion in Regions[color.ToInteger()])
				foreach (GoBlockBase lGoBlockBase in lColorEnclosedRegion.Members)
					if (lGoBlockBase.Members.Contains(index))
						return lColorEnclosedRegion;

			return null; // should never get here
		}
	}
}
