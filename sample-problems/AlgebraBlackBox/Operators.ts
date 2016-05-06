/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/TypeScript.NET/blob/master/LICENSE.md
 */

///<reference path="OperatorSymbol.d.ts"/>

export const
	ADD:OperatorSymbol         = "+",
	MULTIPLY:OperatorSymbol    = "*",
	DIVIDE:OperatorSymbol      = "/",
	SQUARE_ROOT:OperatorSymbol = "√";

export module Available
{
	export const Operators:OperatorSymbol[] = Object.freeze([
		ADD,
		MULTIPLY,
		DIVIDE
	]);

	export const Functions:OperatorSymbol[] = Object.freeze([
		SQUARE_ROOT
	]);
}


