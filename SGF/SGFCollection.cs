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
	class SGFCollection : ErrorManagement
	{
		public List<SGFSequence> Sequence;

		public SGFCollection()
		{
			Sequence = new List<SGFSequence>();
		}

		public SGFCollection(string filename)
		{
			Sequence = new List<SGFSequence>();
			LoadSGFFile(filename);
		}

        public int Count
        {
            get
            {
                return Sequence.Count;
            }
        }

		public void AddSequence(SGFSequence pSGFSequence)
		{
			Sequence.Add(pSGFSequence);
		}

		public bool LoadSGFFile(string filename)
		{
			MemFile lMemFile = new MemFile(filename);

			if (lMemFile.IsError())
				return SetErrorMessage(lMemFile);

			return LoadSGFFile(lMemFile);
		}

		public bool LoadSGFFile(MemFile memFile)
		{
			Sequence.Clear();

			while (!memFile.EOF)
			{
				char c = memFile.Peek();

				if (c == '(')
				{
					SGFSequence lSGFSequence = new SGFSequence(memFile);

					if (lSGFSequence.IsError())
						return SetErrorMessage(lSGFSequence);

					Sequence.Add(lSGFSequence);
				}
				else
					memFile.Get();	// eat this character

			}

			return true;
		}

		public bool LoadSGFFromMemory(string pSGF)
		{
			MemFile lMemFile = new MemFile();

			lMemFile.Write(pSGF);
			lMemFile.Reset();

			return LoadSGFFile(lMemFile);
		}

		public bool SaveSGFFile(string filename)
		{
			MemFile lMemFile = new MemFile();

			lMemFile.Clear();
			lMemFile.Write(ToString());

			return lMemFile.SaveFile(filename);
		}

		public bool RetrieveGame(GameRecord gameRecord, int pGameNbr)
		{
			gameRecord.Clear();

			if (pGameNbr < Count)
				return Sequence[pGameNbr].RetrieveGame(gameRecord);

			return false;
		}

		public bool RetrieveGame(GameRecord gameRecord)
		{
			return RetrieveGame(gameRecord, 0);
		}

		public bool RetrieveGames(GameRecords gameRecords, bool includeVariations)
		{
			foreach (SGFSequence lSGFSequence in Sequence)
				if (!lSGFSequence.RetrieveGames(gameRecords, includeVariations))
					return false;

			return true;
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder();

			foreach (SGFSequence lSGFSequence in Sequence)
				lStringBuilder.AppendLine(lSGFSequence.ToString());

			return lStringBuilder.ToString();
		}

	}
}
