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
	class StringMap
	{
		string[] Map;
		public int Width;
		public int Height;

		public StringMap(string[] pMap)
		{
			Map = pMap;
			CalcDimensions();
		}
		/*
		public StringMap(string s1)
		{
			Map = new string[1]; Map[0] = s1; CalcDimensions();
		}

		public StringMap(string s1, string s2)
		{
			Map = new string[2]; Map[0] = s1; Map[1] = s2; CalcDimensions();
		}

		public StringMap(string s1, string s2, string s3)
		{
			Map = new string[3]; Map[0] = s1; Map[1] = s2; Map[2] = s3; CalcDimensions(); 
		}
		*/
		protected void CalcDimensions()
		{
			Height = Map.Length;
			Width = 0;
			foreach (string lString in Map)
				if (Width < lString.Length)
					Width = lString.Length;
		}

		public char Get(int x, int y)
		{
			if ((x >= Width) || (y >= Height))
				return ' ';	// empty

			return Map[Map.Length-y-1][x];
		}


		public override string ToString()
		{
			StringBuilder lString = new StringBuilder(250);

			for (int y = Height; y > 0; y--)
			{
				lString.Append(y);

				if (y < 10)
					lString.Append(" ");

				lString.Append(" : ");

				for (int x = 0; x < Width; x++)
					lString.Append(Get(x,y-1));

				lString.Append("\n");
			}
			lString.Append("     ");
			lString.Append("ABCDEFGHJKLMNOPQURS".Substring(0, Width));
			lString.Append("\n");

			return lString.ToString();
		}
	}
}
