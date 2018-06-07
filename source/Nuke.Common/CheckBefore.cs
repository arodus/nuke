using System;
using System.Linq;

namespace Nuke.Common
{
    /// <summary>
    /// Defines when a target condition is checked.
    /// </summary>
    public enum CheckBefore
    {
        /// <summary>
        /// The condition is checked right before executing the Target. If the result is false the target gets skipped.
        /// </summary>
        ThisTarget,

        /// <summary>
        /// The condition is checked before any target is executed. If the result is false and the target is the invoked target both, the target and all dependet targets get skipped. If the target is a dependency of a different target and the result is false just the target gets skipped.
        /// </summary>
        AllTargets
    }
}
