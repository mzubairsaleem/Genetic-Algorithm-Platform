using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
	public abstract class OperatorGeneBase : ReducibleGeneNode
	{


		protected OperatorGeneBase(
			char op,
			double multiple = 1,
			IEnumerable<AlgebraBlackBox.IGene> children = null) : base(multiple)
		{
			_operator = op;
			if (children != null)
				AddThese(children);
		}


		char _operator;
		public char Operator
		{
			get
			{
				AssertIsLiving();
				return _operator;
			}
			protected set
			{
				SetOperator(value);
			}
		}

		protected bool SetOperator(char value)
		{
			AssertIsLiving();
			return Sync.Modifying(ref _operator, value);
		}

		public override string ToStringContents()
		{
			return GroupedString(Operator);
		}

		protected string GroupedString(string separator, string internalPrefix = "")
		{
			AssertIsLiving();
			return "(" + internalPrefix + String.Join(separator, this.OrderBy(g => g).Select(s => s.ToString())) + ")";
		}
		protected string GroupedString(char separator, string internalPrefix = "")
		{
			return GroupedString(separator.ToString(), internalPrefix);
		}

		public new OperatorGeneBase Clone()
		{
			return (OperatorGeneBase)CloneInternal();
		}


	}
}
