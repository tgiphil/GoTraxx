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
	class DFANode
	{		
		protected int Dest0, Dest1, Dest2, Dest3;

		public List<PatternKey> Attributes;

		public DFANode()
		{
			Dest0 = Dest1 = Dest2 = Dest3 = 0;
		}

		public int this[int i]
		{
			get
			{
				switch (i)
				{
					case 0: return Dest0;
					case 1: return Dest1;
					case 2: return Dest2;
					case 3: return Dest3;
				}
				return 0;
			}
			set
			{
				switch (i)
				{
					case 0: Dest0 = value; return;
					case 1: Dest1 = value; return;
					case 2: Dest2 = value; return;
					case 3: Dest3 = value; return;
				}
			}
		}

		public int Count
		{
			get
			{
				if (Attributes == null)
					return 0;
				else
					return Attributes.Count;
			}
		}

		public void Add(PatternKey pattern)
		{
			if (Attributes == null)
				Attributes = new List<PatternKey>();

			Attributes.Add(pattern);
		}

		public void Add(List<PatternKey> attributes)
		{
			if (attributes == null)
				return;

			if (Attributes == null)
			{
				Attributes = new List<PatternKey>(attributes.Count);

				foreach (PatternKey pattern in attributes)
					Attributes.Add(pattern);
			}
			else
			{
				Attributes = new List<PatternKey>(attributes.Count);

				foreach (PatternKey pattern in attributes)
					if (!Attributes.Contains(pattern))
						Attributes.Add(pattern);
			}
		}

		public void Add2(List<PatternKey> attributes)
		{
			Attributes = Merge(Attributes, attributes);
		}

		static public List<PatternKey> Merge(List<PatternKey> pAttributes1, List<PatternKey> pAttributes2)
		{
			if (pAttributes1 == null)
				return pAttributes2;

			if (pAttributes2 == null)
				return pAttributes1;

			List<PatternKey> lAttributes = new List<PatternKey>(pAttributes1.Count + pAttributes2.Count);

			foreach (PatternKey pattern in pAttributes1)
				lAttributes.Add(pattern);

			foreach (PatternKey pattern in pAttributes2)
				if (!pAttributes1.Contains(pattern))
					lAttributes.Add(pattern);

			return lAttributes;
		}

		static public List<PatternKey> Merge(List<PatternKey> pAttributes1, List<PatternKey> pAttributes2, List<PatternKey> pAttributes3)
		{
			if ((pAttributes1 == null) && (pAttributes2 == null) && (pAttributes3 == null))
				return null;

			if ((pAttributes1 == null) && (pAttributes2 == null))
				return pAttributes3;

			if ((pAttributes2 == null) && (pAttributes3 == null))
				return pAttributes1;

			if ((pAttributes1 == null) && (pAttributes3 == null))
				return pAttributes2;

			if (pAttributes1 == null)
				return Merge(pAttributes2, pAttributes3);

			if (pAttributes2 == null)
				return Merge(pAttributes1, pAttributes3);

			if (pAttributes3 == null)
				return Merge(pAttributes1, pAttributes2);

			List<PatternKey> lAttributes = new List<PatternKey>(pAttributes1.Count + pAttributes2.Count + pAttributes3.Count);

			foreach (PatternKey pattern in pAttributes1)
				lAttributes.Add(pattern);

			foreach (PatternKey pattern in pAttributes2)
				if (!pAttributes1.Contains(pattern))
					lAttributes.Add(pattern);

			foreach (PatternKey pattern in pAttributes3)
				if (!pAttributes1.Contains(pattern))
					if (!pAttributes2.Contains(pattern))
						lAttributes.Add(pattern);

			return lAttributes;
		}
	};
}
