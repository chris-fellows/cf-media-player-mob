using CFMediaPlayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    public class UserSettings
    {
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Default play mode
        /// </summary>
        public PlayModes PlayMode { get; set; }
    }
}
