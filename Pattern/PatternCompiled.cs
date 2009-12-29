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
	class PatternCompiled : ErrorManagement
	{
		public Pattern Pattern;

		public PatternCode PatternCode;
		public PatternAttribute PatternAttribute;
		public PatternActionAttribute PatternActionAttribute;
		public PatternConstraintAttribute PatternConstraintAttribute;
		public PatternActionCode PatternActionCode;

		public int OptimalDFATransformation = 0;
		public bool[] UniqueTransformation = new bool[8] { false, false, false, false, false, false, false, false };

		protected PatternInterpretor PatternInterpretor;

		public bool CompilationErrorFlag
		{
			get
			{
				return this.IsError();
			}
		}

		public PatternCompiled(Pattern pattern)
		{
			Pattern = pattern;

			PatternCode = PatternCompiler.Compile(Pattern);
			if (PatternCode.IsError())
			{
				SetErrorMessage(PatternCode.GetErrorMessage());
				return;
			}

			PatternInterpretor = new PatternInterpretor(PatternCode, Pattern);

			PatternAttribute = new PatternAttribute(Pattern.PatternAttributes);
			if (PatternAttribute.IsError())
			{
				SetErrorMessage(PatternAttribute.GetErrorMessage());
				return;
			}

			PatternActionAttribute = new PatternActionAttribute(Pattern.ClassificationAttributes);
			if (PatternActionAttribute.IsError())
			{
				SetErrorMessage(PatternActionAttribute.GetErrorMessage());
				return;
			}

			PatternConstraintAttribute = new PatternConstraintAttribute(Pattern.ClassificationAttributes);
			if (PatternConstraintAttribute.IsError())
			{
				SetErrorMessage(PatternConstraintAttribute.GetErrorMessage());
				return;
			}

			PatternActionCode = PatternActionCompiler.Compile(pattern, pattern.ActionCode);
			if (PatternActionCode.IsError())
			{
				SetErrorMessage(PatternActionCode.GetErrorMessage());
				return;
			}

			OptimalDFATransformation = FindMinimizedDFATransformation();
		}

		protected int FindMinimizedDFATransformation()
		{
			int lSmallestTranslationSize = Int32.MaxValue;
			int lMinimizedDFATransformation = 0;

			for (int t = 0; t < 8; t++)
			{
				DFAPattern lDFAPattern = new DFAPattern(Pattern, t);

				if (lDFAPattern.VariationCount < lSmallestTranslationSize)
				{
					lSmallestTranslationSize = lDFAPattern.VariationCount;
					lMinimizedDFATransformation = t;
				}
			}

			return lMinimizedDFATransformation;
		}

		public int Match(GoBoard goBoard, Color player, Coordinate originPoint, int transform)
		{
			return PatternInterpretor.Execute(goBoard, player, originPoint, transform);
		}

		public void Execute(GoBoard goBoard, Color player, Coordinate originPoint, int transform, MoveList moves)
		{
			PatternActionCode.Execute(goBoard, originPoint, transform, moves);
		}
	}
}
