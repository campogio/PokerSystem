using System;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class UniformModel
    {
        public Enum FactionJob { get; set; }
        public Sex CharacterSex { get; set; }
        public int UniformSlot { get; set; }
        public int UniformDrawable { get; set; }
        public int UniformTexture { get; set; }

        public UniformModel(Enum factionJob, Sex characterSex, int uniformSlot, int uniformDrawable, int uniformTexture)
        {
            FactionJob = factionJob;
            CharacterSex = characterSex;
            UniformSlot = uniformSlot;
            UniformDrawable = uniformDrawable;
            UniformTexture = uniformTexture;
        }
    }
}
