///<reference path="IGene.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Enumeration/IEnumerable.d.ts"/>
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";

abstract class GeneBase implements IGene {
	abstract getChildren():GeneBase[];

	abstract serialize():string;

	abstract clone():GeneBase;

	get children():GeneBase[] {
		return this.getChildren();
	}
	
	get descendants():Enumerable<GeneBase> {
		var c = Enumerable.from(this.getChildren());
		return c.concat(c.selectMany(s=>s.descendants));
	}
}

export default GeneBase;