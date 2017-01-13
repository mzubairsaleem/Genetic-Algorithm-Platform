using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
	public abstract class OperatorGeneBase : ReducibleGeneNode
	{


		protected OperatorGeneBase(
			char op,
			double multiple = 1) : base(multiple)
		{
			_operator = op;
		}

		protected OperatorGeneBase(
			char op,
			double multiple,
			IEnumerable<AlgebraBlackBox.IGene> children) : this(op, multiple)
		{
			if (children != null)
				AddThese(children);
		}

		protected OperatorGeneBase(char op, double multiple, AlgebraBlackBox.IGene child) : this(op, multiple)
		{
            if(child!=null) Add(child);
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
