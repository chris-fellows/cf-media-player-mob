using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    public class AudioBand
    {
        public short Index { get; set; }

        public string Description { get; set; } = String.Empty;

        public short LevelRangeMin { get; set; }

        public short LevelRangeMax { get; set; }

        private short _level;
        public short Level
        {
            get { return _level; }
            set
            {
                _level = value;

                if (SetLevelAction != null)
                {
                    SetLevelAction(Index, _level);
                }                  
            }
        }

        /// <summary>
        /// Action when Level property is updated
        /// </summary>
        public Action<short, short>? SetLevelAction { get; set; }
    }
}
