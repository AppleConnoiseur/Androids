using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Interface for Pawn crafters.
    /// </summary>
    public interface IPawnCrafter
    {
        /// <summary>
        /// Gets the current Pawn being crafted.
        /// </summary>
        /// <returns>Pawn being crafted or null.</returns>
        Pawn PawnBeingCrafted();

        /// <summary>
        /// Gets the status of the crafter.
        /// </summary>
        /// <returns>Crafter status.</returns>
        CrafterStatus PawnCrafterStatus();

        /// <summary>
        /// Starts the printing.
        /// </summary>
        void InitiatePawnCrafting();

        /// <summary>
        /// Stops the printing.
        /// </summary>
        void StopPawnCrafting();
    }
}
