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
	public class GameRecord
	{
		public struct GameMove
		{
			public Color Player;
			public int Move;	// future: back to string with standard GTP coordinates
			public bool SetupMove;
			public string Comment; // for node in SGF tree

			public GameMove(Color player, int move)
			{
				Player = player;
				Move = move;
				SetupMove = false;
				Comment = string.Empty;
			}

			public GameMove(Color player, int move, bool setupMove)
			{
				Player = player;
				Move = move;
				SetupMove = setupMove;
				Comment = string.Empty;
			}
		};

		public List<GameMove> Moves;
		public int BoardSize;
		public string Date;
		public string BlackPlayerName;
		public string WhitePlayerName;
		public string GameName;
		public string Result;
		public string Comment;
		public string Rules;
		public string Place;
		public string WhiteRank;
		public string BlackRank;
		public int TimeLimit;
		public int HandicapStones;
		public float Komi;
		public Color NextPlayer;
		public string Identification;

		public List<int>[] Territory;

		public GameRecord()
		{
			BoardSize = 19;
			Clear();
		}

		public GameRecord(int boardSize)
		{
			BoardSize = boardSize;
			Clear();
		}

		public void Clear()
		{
			TimeLimit = 0;
			HandicapStones = 0;
			Moves = new List<GameMove>();
			Date = String.Empty;
			BlackPlayerName = String.Empty;
			WhitePlayerName = String.Empty;
			GameName = String.Empty;
			Result = String.Empty;
			Comment = String.Empty;
			GameName = String.Empty;
			Place = String.Empty;
			Rules = String.Empty;
			WhiteRank = String.Empty;
			BlackRank = String.Empty;
			TimeLimit = 0;
			Komi = 0;
			NextPlayer = Color.Black;
			Identification = string.Empty;
			Territory = new List<int>[2];
			Territory[0] = new List<int>();
			Territory[1] = new List<int>();
		}

		public GameRecord Clone()
		{
			GameRecord lGameRecord = new GameRecord();
			lGameRecord.BoardSize = BoardSize;
			lGameRecord.Date = Date;
			lGameRecord.BlackPlayerName = BlackPlayerName;
			lGameRecord.WhitePlayerName = WhitePlayerName;
			lGameRecord.GameName = GameName;
			lGameRecord.Result = Result;
			lGameRecord.Comment = Comment;
			lGameRecord.GameName = GameName;
			lGameRecord.Place = Place;
			lGameRecord.Rules = Rules;
			lGameRecord.WhiteRank = WhiteRank;
			lGameRecord.BlackRank = BlackRank;
			lGameRecord.TimeLimit = TimeLimit;
			lGameRecord.Komi = Komi;
			lGameRecord.NextPlayer = NextPlayer;
			lGameRecord.Identification = Identification;
			lGameRecord.Territory = new List<int>[2];
			lGameRecord.Territory[0] = new List<int>();
			lGameRecord.Territory[1] = new List<int>();

			foreach (GameMove lGameMove in Moves)
				lGameRecord.Moves.Add(lGameMove);

			for (int lColor = 0; lColor < 2; lColor++)
				foreach (int lMove in Territory[lColor])
					lGameRecord.Territory[lColor].Add(lMove);

			return lGameRecord;
		}

		protected void AddMove(GameMove gameMove)
		{
			Moves.Add(gameMove);
			NextPlayer = gameMove.Player.Opposite;
		}

		public void PlayStone(int index)
		{
			AddMove(new GameMove(NextPlayer, index, false));
		}

		public void PlayStone(Color color, int index)
		{
			AddMove(new GameMove(color, index, false));
		}

		public void SetupStone(Color color, int index)
		{
			AddMove(new GameMove(color, index, true));
		}

		public void PlayStone(Color color, string move)
		{
			AddMove(new GameMove(color, CoordinateSystem.At(move, BoardSize), false));
		}

		public void SetupStone(Color color, string move)
		{
			AddMove(new GameMove(color, CoordinateSystem.At(move, BoardSize), true));
		}

		public void SetTerritory(Color color, int index)
		{
			Territory[color.ToInteger()].Add(index);
		}

		public void ClearTerritory()
		{
			Territory[0].Clear();
			Territory[1].Clear();
		}

		public int Count
		{
			get
			{
				return Moves.Count;
			}
		}

		public GameMove this[int arg]
		{
			get
			{
				return Moves[arg];
			}
		}

		public override string ToString()
		{
			SGFCollection lSGFCollection = new SGFCollection();
			SGFSequence lSGFSequence = new SGFSequence();
			SGFNode lSGFNode = new SGFNode();

			CoordinateSystem lCoord = CoordinateSystem.GetCoordinateSystem(BoardSize);

			lSGFNode.AddProperty(new SGFProperty("GM", "1"));
			lSGFNode.AddProperty(new SGFProperty("FF", "4"));
			lSGFNode.AddProperty(new SGFProperty("CA", "UTF-8"));
			lSGFNode.AddProperty(new SGFProperty("SZ", BoardSize));
			lSGFNode.AddProperty(new SGFProperty("KM", Convert.ToString(Komi)));

			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("GN", GameName));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("ID", Identification));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("DT", Date));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("PB", BlackPlayerName));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("PW", WhitePlayerName));
			//			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("RS", Result));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("C", Comment));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("RU", Rules));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("PC", Place));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("WR", WhiteRank));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("BR", BlackRank));
			lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("RE", Result));

			if (TimeLimit != 0)
				lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("TM", TimeLimit));

			if (HandicapStones > 0)
				lSGFNode.AddPropertyIfNotEmpty(new SGFProperty("HE", HandicapStones));

			lSGFSequence.AddNode(lSGFNode);

			foreach (GameMove lGameMove in Moves)
			{
				lSGFNode = new SGFNode();

				lSGFNode.AddProperty(new SGFProperty(
					(lGameMove.SetupMove ? "A" : "") + (lGameMove.Player.IsBlack ? "B" : "W")
					, lCoord.ToSGFString(lGameMove.Move)));

				lSGFSequence.AddNode(lSGFNode);
			}

			if ((Territory[0].Count != 0) || (Territory[1].Count != 0))
			{
				lSGFNode = new SGFNode();

				for (int lTerritoryIndex = 0; lTerritoryIndex < 2; lTerritoryIndex++)
				{
					if (Territory[lTerritoryIndex].Count != 0)
					{

						foreach (int lIndex in Territory[lTerritoryIndex])
						{
							lSGFNode.AddProperty(new SGFProperty(
								"T" + (lTerritoryIndex == 0 ? "B" : "W"), lCoord.ToSGFString(lIndex)));
						}
					}

				}
				lSGFSequence.AddNode(lSGFNode);
			}

			lSGFCollection.AddSequence(lSGFSequence);

			return lSGFCollection.ToString();
		}

	}
}