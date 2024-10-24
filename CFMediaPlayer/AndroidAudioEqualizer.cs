using Android.Media.Audiofx;
using CFMediaPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer
{
    public class AndroidAudioEqualizer : IAudioEqualizer
    {
        private Equalizer _equalizer = null;

        private string _equalizerPresetName;

        public AndroidAudioEqualizer()
        {
            
        }

        public Equalizer Equalizer
        {
            get { return _equalizer; }
            set { _equalizer = value; }
        }
        

        public string EqualizerPresetName
        {
            get { return _equalizerPresetName; }
            set
            {
                if (_equalizerPresetName != value)
                {
                    _equalizerPresetName = value;

                    ApplyPreset();
                }
            }
        }

        public short[]? GetEqualizerBandLevelRange()
        {
            if (_equalizer != null)
            {
                return _equalizer.GetBandLevelRange();
            }
            return null;
        }

        public int[]? GetEqualizerBandFrequencyRange(short band)
        {
            if (_equalizer != null)
            {
                return _equalizer.GetBandFreqRange(band);
            }
            return null;
        }

        public short GetEqualizerBandLevel(short band)
        {
            if (_equalizer != null)
            {
                return _equalizer.GetBandLevel(band);
            }
            return 0;
        }

        public short? EqualizerBands
        {
            get
            {
                if (_equalizer != null)
                {
                    return _equalizer.NumberOfBands;
                }
                return null;
            }
        }

        public void SetBandLevel(short band, short level)
        {
            _equalizer.SetBandLevel(band, level);
        }

        /// <summary>
        /// Applies equalizer preset
        /// </summary>        
        public void ApplyPreset()
        {          
            if (!String.IsNullOrEmpty(_equalizerPresetName) && _equalizer != null)
            {
                for (short index = 0; index < _equalizer.NumberOfPresets; index++)
                {
                    var currentPresetName = _equalizer.GetPresetName(index);
                    if (currentPresetName.Equals(_equalizerPresetName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //_equalizer.SetEnabled(false);                       
                        _equalizer.UsePreset(index);
                        //_equalizer.SetEnabled(true);
                        break;
                    }
                }
            }
        }
    }
}
