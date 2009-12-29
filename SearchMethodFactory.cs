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
	public enum SearchMethodType
	{
		Unassigned = 0,
		MinMax = 1,
		AlphaBeta = 2,
		AlphaBeta_ID_PVS = 3,
		AlphaBeta_ID_TT_PVS = 4,
		AlphaBeta_ID_TT = 5,
		AlphaBeta_NAG_ID_TT = 6,
	};

	delegate void OnCompletion(SearchStatus searchStatus);

	interface ISearchMethodInterface
	{
		void Initialize(GoBoard goBoard, Color playerToMove, SearchOptions searchOptions, OnCompletion onCompletion);
		void StartThinking();
		void StopThinking();
		SearchStatus GetStatus();
		void SetNagCoordinator(NagCoordinator nagCoordinator);
	}

	class SearchMethodFactory
	{
		protected Dictionary<string, SearchMethodType> Dictionary;
		protected static SearchMethodFactory Instance = new SearchMethodFactory();

		protected SearchMethodFactory()
		{
			Dictionary = new Dictionary<string, SearchMethodType>();

			Add("minmax", SearchMethodType.MinMax);
			Add("alphabeta", SearchMethodType.AlphaBeta);
			Add("alphabetaiterative", SearchMethodType.AlphaBeta_ID_PVS);
			Add("alphabetaiterativetransposition", SearchMethodType.AlphaBeta_ID_TT_PVS);
			Add("alphabetatranposition", SearchMethodType.AlphaBeta_ID_TT);
			Add("nag", SearchMethodType.AlphaBeta_NAG_ID_TT);
		}

		protected void Add(string typeName, SearchMethodType searchType)
		{
			Dictionary.Add(typeName, searchType);
		}

		public static ISearchMethodInterface CreateFactory(SearchMethodType searchType)
		{
			switch (searchType)
			{
				case SearchMethodType.MinMax: return new SearchMethodMinMax(new SearchStandard());
				case SearchMethodType.AlphaBeta: return new SearchMethodAlphaBeta(new SearchStandard());
				case SearchMethodType.AlphaBeta_ID_PVS: return new SearchMethodAB_ID_PVS(new SearchStandard());
				case SearchMethodType.AlphaBeta_ID_TT_PVS: return new SearchMethodAB_ID_TT_PVS(new SearchStandard());
				case SearchMethodType.AlphaBeta_ID_TT: return new SearchMethodAB_ID_TT(new SearchStandard());
				case SearchMethodType.AlphaBeta_NAG_ID_TT: return new SearchMethodAB_NAG_ID_TT(new SearchStandard());
				default: return new SearchMethodAB_ID_TT(new SearchStandard());
			}
		}

		public static ISearchMethodInterface CreateFactory(string name)
		{
			return CreateFactory(ToType(name));
		}

		public static SearchMethodType ToType(string name)
		{
			SearchMethodType lSearchMethodType = SearchMethodType.Unassigned;

			if (!Instance.Dictionary.TryGetValue(name.Trim().ToLower(), out lSearchMethodType))
				return SearchMethodType.Unassigned;

			return lSearchMethodType;
		}
	}
}
