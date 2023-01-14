using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fisobs.Saves
{
    readonly struct FisobSaveSlot
    {
        public FisobSaveSlot(List<string> unlocked)
        {
            Unlocked = new(unlocked);
        }

        public readonly ReadOnlyCollection<string> Unlocked;
    }
}
