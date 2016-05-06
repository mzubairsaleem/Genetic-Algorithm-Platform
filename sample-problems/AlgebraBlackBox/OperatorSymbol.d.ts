/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/TypeScript.NET/blob/master/LICENSE.md
 */

declare type ADDITION       = "+"; // Subtraction is simply a negative multiplier.
declare type MULTIPLICATION = "*";
declare type DIVISION       = "/";
declare type SQUARE_ROOT    = "√";

declare type OperatorSymbol
	= ADDITION
	| MULTIPLICATION
	| DIVISION
	| SQUARE_ROOT;