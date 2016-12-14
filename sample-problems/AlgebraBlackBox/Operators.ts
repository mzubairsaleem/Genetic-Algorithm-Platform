/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/TypeScript.NET/blob/master/LICENSE.md
 */
export const
	ADD:ADDITION            = "+",
	MULTIPLY:MULTIPLICATION = "*",
	DIVIDE:DIVISION         = "/",
	SQUARE_ROOT:SQUARE_ROOT = "√";

export module Available
{
	export const Operators:ReadonlyArray<OperatorSymbol> = Object.freeze([
		ADD,
		MULTIPLY,
		DIVIDE
	]);

	export const Functions:ReadonlyArray<OperatorSymbol> = Object.freeze([
		SQUARE_ROOT
	]);

	export const FunctionActual:ReadonlyArray<string> = Object.freeze([
		"Math.sqrt"
	]);
}




