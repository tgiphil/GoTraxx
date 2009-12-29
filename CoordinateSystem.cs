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
    public class CoordinateSystem
    {
		public static int MAX_BOARD_SIZE = 19;
        public static int PASS = -1;
        public static int RESIGN = -2;
//		public static int NO_MOVE = -3;
		protected static CoordinateSystem[] CoordinateSystems = CoordinateSystem.BuildCoordinateMatrix(MAX_BOARD_SIZE);

        public int BoardSize;
        public int BoardArea;

        public List<int>[] Neighbors;
        public List<int>[] Corners;
        public int[,] NeighborsEx;

        public CoordinateSystem(int boardSize)
        {
            BoardSize = boardSize;
            BoardArea = BoardSize * BoardSize;
            Neighbors = new List<int>[BoardArea];
            Corners = new List<int>[BoardArea];
            NeighborsEx = new int[BoardArea, 4];

            for (int lIndex = 0; lIndex < BoardArea; lIndex++)
            {
                Neighbors[lIndex] = GetNeighborsEx(lIndex);
                Corners[lIndex] = GetCornersEx(lIndex);

                for (int lDirection = 0; lDirection < 4; lDirection++)
                    NeighborsEx[lIndex, lDirection] = GetNeighborEx(lIndex, lDirection);
            }
        }

        public static CoordinateSystem GetCoordinateSystem(int boardSize)
        {
            return CoordinateSystems[boardSize];
        }

        public int At(int pX, int pY)
        {
            return (pX + (pY * BoardSize));
        }

        public int At(char pA, int pY)
        {
            char lA = Char.ToLower(pA);

            return At((lA < 'I') ? lA - 'a' : lA - 'a' - 1, pY - 1);
        }

        // From SGF coordinates
        public static int AtFromSGF(string str, int boardSize)
        {
            string lString = str.Trim().ToLower();

            if (lString.Length < 2)
                return PASS;

            if (lString == "tt") // pass
                return PASS;

            // for SGF coordinates in standard Go format
            if (lString == "pass") // pass
                return PASS;

            // for SGF coordinates in standard Go format
            if (lString == "resign") // pass
                return RESIGN;

            char lA = lString[0];

            if (!Char.IsDigit(lString[1]))
            {
                char lB = lString[1];

                int lY = lB - 'a';

                return At(lA - 'a', boardSize - lY - 1, boardSize);

            }
            else
            {
                string lB = lString.Substring(1);

                return At((lA < 'i') ? lA - 'a' : lA - 'a' - 1, Convert.ToInt32(lB) - 1, boardSize);
            }
        }

        public int At(string str)
        {
            if ((str.Length == 2) || (str.Length == 3))
            {
                char lA = Char.ToLower(str[0]);
                int lY = Convert.ToInt32(str.Substring(1)) - 1;

                return At((lA < 'i') ? lA - 'a' : lA - 'a' - 1, lY);
            }
            else
                if (str.ToLower() == "resign")
                    return RESIGN;
                else
                    return PASS;
        }

        public static int At(int x, int y, int boardSize)
        {
            return (x + (y * boardSize));
        }

        public static int At(string str, int boardSize)
        {
            if ((str.Length == 2) || (str.Length == 3))
            {
                char lA = Char.ToLower(str[0]);
                int lY = Convert.ToInt32(str.Substring(1)) - 1;

                return At((lA < 'i') ? lA - 'a' : lA - 'a' - 1, lY, boardSize);
            }
            else
                if (str.ToLower() == "resign")
                    return RESIGN;
                else
                    return PASS;
        }

        public bool OnBoard(int index)
        {
            return ((index >= 0) && (index < BoardArea));
        }

        public static bool OnBoard(int index, int boardSize)
        {
            return ((index >= 0) && (index < (boardSize * boardSize)));
        }

        public bool OnEdge(int index)
        {
            int lColumn = GetColumn(index);
            int lRow = GetRow(index);

            return ((lColumn == 0) || (lRow == 0) || (lColumn == BoardSize - 1) || (lRow == BoardSize - 1));
        }

        public bool OnCorner(int index)
        {
            int lColumn = GetColumn(index);
            int lRow = GetRow(index);

            return (((lColumn == 0) || (lColumn == BoardSize - 1)) && ((lRow == 0) || (lRow == BoardSize - 1)));
        }

        static public bool IsPass(int index)
        {
            return index == PASS;
        }

        public int GetColumn(int index)
        {
            return index % BoardSize;
        }

        public int GetRow(int index)
        {
            return index / BoardSize;
        }

        public static int GetColumn(int index, int boardSize)
        {
            return index % boardSize;
        }

        public static int GetRow(int index, int boardSize)
        {
            return index / boardSize;
        }

        public bool IsNeighbor(int index1, int index2)
        {
            if ((!OnBoard(index1)) || (!OnBoard(index2)))
                return false;

            int lColumn1 = GetColumn(index1);
            int lRow1 = GetRow(index1);
            int lColumn2 = GetColumn(index2);
            int lRow2 = GetRow(index2);

            if ((lColumn1 == lColumn2 - 1) && (lRow1 == lRow2) && (lColumn2 > 0)) return true;
            if ((lColumn1 == lColumn2 + 1) && (lRow1 == lRow2) && (lColumn1 < BoardSize)) return true;
            if ((lColumn1 == lColumn2) && (lRow1 == lRow2 - 1) && (lRow2 > 0)) return true;
            if ((lColumn1 == lColumn2) && (lRow1 == lRow2 + 1) && (lRow1 < BoardSize)) return true;

            return false;
        }

        protected int GetNeighborEx(int index, int direction)
        {
            int lColumn = GetColumn(index);
            int lRow = GetRow(index);

            switch (direction)
            {
                case 0: lColumn++; break;	/* North */
                case 1: lRow++; break;		/* East */
                case 2: lColumn--; break;	/* South */
                case 3: lRow--; break;		/* West */
            }

            if ((lColumn < 0) || (lRow < 0) || (lColumn >= BoardSize) || (lRow >= BoardSize))
                return PASS;
            else
                return At(lColumn, lRow);
        }

        protected static int GetNeighborEx(int index, int direction, int boardSize)
        {
            int lColumn = GetColumn(index, boardSize);
            int lRow = GetRow(index, boardSize);

            switch (direction)
            {
                case 0: lColumn++; break;	/* North */
                case 1: lRow++; break;		/* East */
                case 2: lColumn--; break;	/* South */
                case 3: lRow--; break;		/* West */
            }

            if ((lColumn < 0) || (lRow < 0) || (lColumn >= boardSize) || (lRow >= boardSize))
                return PASS;
            else
                return At(lColumn, lRow, boardSize);
        }

        protected int GetCornerEx(int index, int direction)
        {
            int lColumn = GetColumn(index);
            int lRow = GetRow(index);

            switch (direction)
            {
                case 0: lColumn++; lRow--; break;	/* North-West */
                case 1: lColumn++; lRow++; break;	/* North-East */
                case 2: lColumn--; lRow++; break;	/* South-East */
                case 3: lColumn--; lRow--; break;	/* South-West */
            }

            if ((lColumn < 0) || (lRow < 0) || (lColumn >= BoardSize) || (lRow >= BoardSize))
                return PASS;
            else
                return At(lColumn, lRow);
        }

        public static int TurnClockWise(int direction)
        {
            return (direction < 3) ? ++direction : 0;
        }

        public static int TurnCounterClockWise(int direction)
        {
            return (direction > 0) ? --direction : 3;
        }

        public static int TurnAround(int direction)
        {
            return TurnClockWise(TurnClockWise(direction));
        }

        public List<int> GetNeighbors(int index)
        {
            return Neighbors[index];
        }

        public int GetNeighbor(int index, int direction)
        {
            return NeighborsEx[index, direction];
        }

        public List<int> GetCorners(int index)
        {
            return Corners[index];
        }

        protected List<int> GetNeighborsEx(int index)
        {
            List<int> lNeighbors = new List<int>(4);

            for (int lDirection = 0; lDirection < 4; lDirection++)
            {
                int lIndex = GetNeighborEx(index, lDirection);
                if (lIndex != PASS)
                    lNeighbors.Add(lIndex);
            }

            return lNeighbors;
        }

        protected static List<int> GetNeighborsEx(int index, int boardSize)
        {
            List<int> lNeighbors = new List<int>(4);
            for (int lDirection = 0; lDirection < 4; lDirection++)
            {
                int lIndex = GetNeighborEx(index, lDirection, boardSize);
                if (lIndex != PASS)
                    lNeighbors.Add(lIndex);
            }

            return lNeighbors;
        }

        protected List<int> GetCornersEx(int index)
        {
            List<int> lCorners = new List<int>(4);

            for (int lDirection = 0; lDirection < 4; lDirection++)
            {
                int lIndex = GetCornerEx(index, lDirection);
                if (lIndex != PASS)
                    lCorners.Add(lIndex);
            }

            return lCorners;
        }

        public static String ToString(int pX, int pY)
        {
            if (pX < 0)
                return "PASS";

            int lColumn = pX;

            if (lColumn >= 'I' - 'A')
                lColumn++;

            string lCol = Convert.ToString(Convert.ToChar(lColumn + 'A'));

            return lCol + Convert.ToString(pY + 1);
        }

        public String ToString(int index)
        {
            if (index == PASS)
                return "PASS";

            if (index == RESIGN)
                return "RESIGN";

			int lColumn = GetColumn(index);

            if (lColumn >= 'I' - 'A')
                lColumn++;

            string lCol = Convert.ToString(Convert.ToChar(lColumn + 'A'));
            int lRow = GetRow(index) + 1;

            return lCol + Convert.ToString(lRow);
        }

        public static String ToString2(int index, int boardsize)
        {
            if (index == PASS)
                return "PASS";

            if (index == RESIGN)
                return "RESIGN";

            int lColumn = GetColumn(index, boardsize);

            if (lColumn >= 'I' - 'A')
                lColumn++;

            string lCol = Convert.ToString(Convert.ToChar(lColumn + 'A'));
            int lRow = GetRow(index, boardsize) + 1;

            return lCol + Convert.ToString(lRow);
        }

        public String ToSGFString(int index)
        {
            if (index == PASS)
                return "";

            if (index == RESIGN)
                return "";

            int lColumn = GetColumn(index);
            int lRow = BoardSize - GetRow(index) - 1;

            return Convert.ToString(Convert.ToChar(lColumn + 'a')) + Convert.ToString(Convert.ToChar(lRow + 'a'));
        }

        private static CoordinateSystem[] BuildCoordinateMatrix(int pUpToSize)
        {
            CoordinateSystem[] lCoordinateSystem = new CoordinateSystem[pUpToSize+1];

            for (int i = 1; i <= pUpToSize; i++)
                lCoordinateSystem[i] = new CoordinateSystem(i);

            return lCoordinateSystem;
        }
    }
}
