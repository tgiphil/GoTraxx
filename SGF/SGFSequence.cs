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
	class SGFSequence : ErrorManagement
	{
		public List<SGFNode> Nodes;
		public List<SGFSequence> Variations;

		public SGFSequence()
		{
			Nodes = new List<SGFNode>();
			Variations = new List<SGFSequence>();
		}

		public SGFSequence(MemFile memFile)
		{
			Nodes = new List<SGFNode>();
			Variations = new List<SGFSequence>();
			Read(memFile);
		}

		public void AddNode(SGFNode pSGFNode)
		{
			Nodes.Add(pSGFNode);
		}

		public void AddVariation(SGFSequence pSGFSequence)
		{
			Variations.Add(pSGFSequence);
		}

		public void Clear()
		{
			Nodes.Clear();
			Variations.Clear();
		}

		public bool Read(MemFile memFile)
		{
			char c = memFile.Get();

			if (c != '(')
				return SetErrorMessage("Expecting open-parentheses, found: " + c.ToString());

			while (true)
			{
				c = memFile.Peek();

				if (c == '(')
				{
					SGFSequence lSGFSequence = new SGFSequence(memFile);

					if (lSGFSequence.IsError())
						return SetErrorMessage(lSGFSequence);

					AddVariation(lSGFSequence);

					char p = memFile.Get();

					if (p != ')') // eat this character 
						return SetErrorMessage("Expecting closing-parenthese, found: " + p.ToString());

				}
				else
				{
					if (c == ')')
						break;

					if (c == ';')
					{
						SGFNode lSGFNode = new SGFNode(memFile);

						if (lSGFNode.IsError())
							return SetErrorMessage(lSGFNode);

						AddNode(lSGFNode);
					}
					else
						memFile.Get();	// eat this character
				}

				if (memFile.EOF)
					return SetErrorMessage("Unexpected EOF.");

			}

			return true;
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder();
			lStringBuilder.Append("(");

			foreach (SGFNode lSGFNode in Nodes)
				lStringBuilder.Append(lSGFNode.ToString());

			// output variations, if any
			foreach (SGFSequence lSGFSequence in Variations)
				lStringBuilder.Append(lSGFSequence.ToString());

			lStringBuilder.AppendLine(")");

			return lStringBuilder.ToString();
		}

		public bool RetrieveGame(GameRecord gameRecord)
		{
			foreach (SGFNode lSGFNode in Nodes)
				if (!lSGFNode.RetrieveGame(gameRecord))
					return false;

			// do 1st (main) variation only
			if (Variations.Count >= 1)
				return Variations[0].RetrieveGame(gameRecord);

			return true;
		}

		public bool RetrieveGames(GameRecords gameRecords, bool includeVariations, GameRecord gameRecord)
		{
			GameRecord lGameRecord = gameRecord.Clone();

			foreach (SGFNode lSGFNode in Nodes)
				if (!lSGFNode.RetrieveGame(lGameRecord))
					return false;

			if (Variations.Count == 0)
			{
				gameRecords.AddGame(lGameRecord);
				return true;
			}
			
			if (includeVariations)
				foreach (SGFSequence lSGFSequence in Variations)
					if (!lSGFSequence.RetrieveGames(gameRecords, includeVariations, lGameRecord))
						return false;

			return true;
		}

		public bool RetrieveGames(GameRecords gameRecords, bool includeVariations)
		{
			GameRecord lGameRecord = new GameRecord();

			foreach (SGFNode lSGFNode in Nodes)
				if (!lSGFNode.RetrieveGame(lGameRecord))
					return false;

			if (Variations.Count == 0)
			{
				gameRecords.AddGame(lGameRecord);
				return true;
			}

			if (includeVariations)
				foreach (SGFSequence lSGFSequence in Variations)
					if (!lSGFSequence.RetrieveGames(gameRecords, includeVariations, lGameRecord))
						return false;

			return true;
		}
	}
}
