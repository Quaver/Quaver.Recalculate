using Quaver.API.Enums;

namespace Quaver.Recalculate.Scores
{
    public class Score
    {
        public int Id { get; set; }

        public ModIdentifier Mods { get; set; }

        public double Accuracy { get; set; }

        public int MapId { get; set; }
    }
}