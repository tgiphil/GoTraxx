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
using System.Threading;

namespace GoTraxx
{

	class DFAMatrixBuilder
	{
		protected Stack<DFAMatrix> DFAMatrixes;

		public DFAMatrixBuilder()
		{
			DFAMatrixes = new Stack<DFAMatrix>();
		}

		public DFAMatrixBuilder(DFAMatrix dfaMatrix)
		{
			DFAMatrixes = new Stack<DFAMatrix>();
			Add(dfaMatrix);
		}

		public DFAMatrixBuilder(Stack<DFAMatrix> dfaMatrix, int pCount)
		{
			DFAMatrixes = new Stack<DFAMatrix>(pCount);

			if (pCount > dfaMatrix.Count)
				pCount = dfaMatrix.Count;

			for (int i = 0; i < pCount; i++)
				DFAMatrixes.Push(dfaMatrix.Pop());
		}

		public DFAMatrixBuilder(PatternCollection patternCollection)
		{
			DFAMatrixes = new Stack<DFAMatrix>();
			Add(patternCollection);
			Build();
		}

		public void Add(Pattern pattern)
		{
			if (!pattern.IsOk())
				return;

			List<string> lPatternSignatures = new List<string>(8);

			for (int lTransform = 0; lTransform < 8; lTransform++)
			{
				string lSignature = DFAPattern.ToDFA(pattern, lTransform) + "\t" + DFAPattern.ToVariableDFA(pattern, lTransform);

				if (!lPatternSignatures.Contains(lSignature))
				{
					DFAMatrixes.Push(new DFAMatrix(new PatternKey(pattern, lTransform)));
					lPatternSignatures.Add(lSignature);
				}
			}
		}

		public void Add(PatternCollection patternCollection)
		{
			foreach (Pattern lPattern in patternCollection)
				Add(lPattern);
		}

		public void Add(DFAMatrix dfaMatrix)
		{
			if (dfaMatrix != null)
				DFAMatrixes.Push(dfaMatrix);
		}

		protected bool _BuildPass2()
		{
			int lNodes = DFAMatrixes.Count;

			if (DFAMatrixes.Count <= 1)
				return true;

			Stack<DFAMatrix> lDFAMatrix = new Stack<DFAMatrix>(DFAMatrixes.Count / 2 + 2);

			while (DFAMatrixes.Count >= 2)
				lDFAMatrix.Push(new DFAMatrix(DFAMatrixes.Pop(), DFAMatrixes.Pop()));

			if (DFAMatrixes.Count == 1)
				lDFAMatrix.Push(DFAMatrixes.Pop());

			DFAMatrixes = lDFAMatrix;

			return (DFAMatrixes.Count == 1);
		}

		public void Build()
		{
			BuildPass3();
		}

		protected void BuildPass3()
		{
			Stack<DFAMatrix> lDFAMatrix = new Stack<DFAMatrix>(DFAMatrixes.Count / 3 + 3);
			Stack<DFAMatrix> lDFAMatrixTemp = null;

			while (DFAMatrixes.Count > 1)
			{
				while (DFAMatrixes.Count >= 3)
					lDFAMatrix.Push(new DFAMatrix(DFAMatrixes.Pop(), DFAMatrixes.Pop(), DFAMatrixes.Pop()));

				if (DFAMatrixes.Count == 2)
					lDFAMatrix.Push(new DFAMatrix(DFAMatrixes.Pop(), DFAMatrixes.Pop()));

				if (DFAMatrixes.Count == 1)
					lDFAMatrix.Push(DFAMatrixes.Pop());

				lDFAMatrixTemp = DFAMatrixes;

				DFAMatrixes = lDFAMatrix;

				lDFAMatrix = lDFAMatrixTemp;
			}
		}

		public void BuildThreaded()
		{
			Console.Error.WriteLine("Building...");
			SimpleTimer lTimer = new SimpleTimer();

			if (DFAMatrixes.Count < 64)
				Build();
			else
			{
				DFAMatrixBuilder lDFAMatrixBuilder1 = new DFAMatrixBuilder(DFAMatrixes, DFAMatrixes.Count / 3);
				Thread lThread1 = new Thread(new ThreadStart(lDFAMatrixBuilder1.Build));
				lThread1.Start();

				DFAMatrixBuilder lDFAMatrixBuilder2 = new DFAMatrixBuilder(DFAMatrixes, DFAMatrixes.Count / 2);
				Thread lThread2 = new Thread(new ThreadStart(lDFAMatrixBuilder2.Build));
				lThread2.Start();

				Build();

				lThread1.Join();
				lThread2.Join();

				DFAMatrixes.Push(lDFAMatrixBuilder1.DFAMatrixes.Pop());
				DFAMatrixes.Push(lDFAMatrixBuilder2.DFAMatrixes.Pop());

				Build();
			}

			Console.Error.WriteLine("Build Time: " + lTimer.SecondsElapsed.ToString() + " Seconds");
			Console.Error.WriteLine("Nodes: " + GetMatrix().DFANodes.Count.ToString());
		}

		public DFAMatrix GetMatrix()
		{
			if (DFAMatrixes.Count == 0)
				return null;

			if (DFAMatrixes.Count > 1)
				BuildThreaded();

			DFAMatrix lDFAMatrix = DFAMatrixes.Pop();
			DFAMatrixes.Push(lDFAMatrix);

			return lDFAMatrix;
		}
	}
}
