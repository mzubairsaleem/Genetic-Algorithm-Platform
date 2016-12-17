/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
// using System.Runtime.Serialization;

namespace GeneticAlgorithmPlatform
{

    public interface IGene :  ICollection<IGene> /*, ISerializable, ICloneable<IGene>*/
    {
        //children:IGene[]; Just use .toArray();
        IEnumerable<IGene> Descendants {
            get;
        }

        IGene FindParent(IGene child);

        bool replace(IGene target, IGene replacement);

        void resetToString();
    }

}
