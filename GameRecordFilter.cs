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
	class GameRecordFilter
    {
        public delegate bool FilterFunction(GoBoard goBoard, Color currentPlayer, int currentMove, Color nextPlayer, int nextMove);

        public struct Result
        {
            public GameRecord GameRecord;
            public int MoveNbr;

            public Result(GameRecord gameRecord, int moveNbr)
            {
                GameRecord = gameRecord;
                MoveNbr = moveNbr;
            }
        }

        public static FilterFunction ToFunction(string name)
        {
            switch (name.Trim().ToLower())
            {
                case "findmovesinprotectedareas": return FindMovesInProtectedAreas;
                case "everything": return Everything;
                case "none": return None;
                default: return null;
            }
        }

        public static bool Everything(GoBoard goBoard, Color currentPlayer, int currentMove, Color nextPlayer, int nextMove)
        {
            return true;
        }

        public static bool None(GoBoard goBoard, Color currentPlayer, int currentMove, Color nextPlayer, int nextMove)
        {
            return false;
        }

        public static bool FindMovesInProtectedAreas(GoBoard goBoard, Color currentPlayer, int currentMove, Color nextPlayer, int nextMove)
        {
            if (nextPlayer == Color.Empty)
                return false;

            if ((nextMove == CoordinateSystem.PASS) || (nextMove == CoordinateSystem.RESIGN))
                return false;

            return (goBoard.IsProtectedLiberty(nextMove, nextPlayer));
        }

        public static List<Result> Filter(GameRecords gameRecords, FilterFunction filterFunction)
        {
            List<Result> lResults = new List<Result>();

            foreach (GameRecord lGameRecord in gameRecords)
            {
                CoordinateSystem lCoord = new CoordinateSystem(lGameRecord.BoardSize);
                GoBoard lGoBoard = new GoBoard(lGameRecord.BoardSize);

                for (int i = 0; i < lGameRecord.Count - 1; i++)
                {
                    lGoBoard.PlayStone(lGameRecord[i].Move, lGameRecord[i].Player, false);

                    int lNextMove;
                    Color lNextPlayer;

                    if (i >= lGameRecord.Count)
                    {
                        lNextMove = CoordinateSystem.PASS;
                        lNextPlayer = Color.Empty;
                    }
                    else
                    {
                        lNextMove = lGameRecord[i + 1].Move;
                        lNextPlayer = lGameRecord[i + 1].Player;
                    }

                    if (filterFunction(lGoBoard, lGameRecord[i].Player, lGameRecord[i].Move, lNextPlayer, lNextMove))
//                    {
//                        Console.WriteLine(lGameRecord.GameName + " (" + i.ToString() + ") ->  " + lCoord.ToString(lNextMove));
                        lResults.Add(new Result(lGameRecord, i));
//                    }
                }
            }

            return lResults;
        }
    }
}
