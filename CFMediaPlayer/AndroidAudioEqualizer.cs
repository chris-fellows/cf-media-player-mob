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
        private Equalizer? _equalizer = null;
        private string _equalizerPresetName;
        private List<short> _customBandLevels = new List<short>();

        public AndroidAudioEqualizer()
        {
            
        }

        public Equalizer? Equalizer
        {
            get { return _equalizer; }
            set { _equalizer = value; }
        }

        public List<string> PresetNames
        {
            get
            {
                var mediaPlayer = new Android.Media.MediaPlayer();
                var equalizer = new Equalizer(0, mediaPlayer.AudioSessionId);
                var presetNames = new List<string>();
                if (equalizer != null)
                {
                    for (short preset = 0; preset < equalizer.NumberOfPresets; preset++)
                    {
                        presetNames.Add(equalizer.GetPresetName(preset));
                    }
                }
                presetNames.Sort(); // Alphabetic
                equalizer.Release();
                mediaPlayer.Release();
                return presetNames;
            }
        }

        public string DefaultPresetName
        {
            get { return _equalizerPresetName; }
            set
            {
                _equalizerPresetName = value;
            }
        }

        public List<short> DefaultCustomBandLevels
        {
            get { return _customBandLevels; }
            set
            {
                _customBandLevels = value;
            }
        }

        public short[]? GetEqualizerBandLevelRange()
        {
            var mediaPlayer = new Android.Media.MediaPlayer();
            var equalizer = new Equalizer(0, mediaPlayer.AudioSessionId);                       

            var range = equalizer.GetBandLevelRange();
          
            equalizer.Release();
            mediaPlayer.Release();
            return range;
         
            //if (_equalizer != null)
            //{
            //    return _equalizer.GetBandLevelRange();
            //}
            //return null;
        }

        public List<int[]?> GetEqualizerBandFrequencyRanges()
        {
            var ranges = new List<int[]?>();

            var mediaPlayer = new Android.Media.MediaPlayer();
            var equalizer = new Equalizer(0, mediaPlayer.AudioSessionId);

            for (short band = 0; band < equalizer.NumberOfBands; band++)
            {
                ranges.Add(equalizer.GetBandFreqRange(band));
            }
            
            equalizer.Release();
            mediaPlayer.Release();            

            return ranges;
        }

        //public int[]? GetEqualizerBandFrequencyRange(short band)
        //{
        //    if (_equalizer != null)
        //    {
        //        return _equalizer.GetBandFreqRange(band);
        //    }
        //    return null;
        //}

        //public short GetEqualizerBandLevel(short band)
        //{
        //    if (_equalizer != null)
        //    {
        //        return _equalizer.GetBandLevel(band);
        //    }
        //    return 0;
        //}

        //public short? EqualizerBands
        //{
        //    get
        //    {
        //        if (_equalizer != null)
        //        {
        //            return _equalizer.NumberOfBands;
        //        }
        //        return null;
        //    }
        //}

        //public void SetBandLevel(short band, short level)
        //{
        //    _equalizer.SetBandLevel(band, level);
        //}

        /// <summary>
        /// Applies equalizer preset
        /// </summary>        
        public void ApplyDefault()
        {          
            if (!String.IsNullOrEmpty(_equalizerPresetName) && _equalizer != null)
            {
                if (_equalizerPresetName.Equals("Custom"))
                {
                    if (_customBandLevels.Any() && _equalizer != null)
                    {
                        for (short band = 0; band < _customBandLevels.Count; band++)
                        {
                            _equalizer.SetBandLevel(band, _customBandLevels[band]);
                        }
                    }
                }
                else
                {
                    for (short index = 0; index < _equalizer.NumberOfPresets; index++)
                    {
                        var currentPresetName = _equalizer.GetPresetName(index);
                        if (currentPresetName.Equals(_equalizerPresetName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _equalizer.UsePreset(index);
                            break;
                        }
                    }
                }
            }
        }

        public List<short> GetBandLevelsForPreset(string presetName)
        {
            var mediaPlayer = new Android.Media.MediaPlayer();
            var equalizer = new Equalizer(0, mediaPlayer.AudioSessionId);

            var bandLevels = new List<short>();
            for(short preset = 0; preset < _equalizer.NumberOfPresets; preset++)
            {
                var currentPresetName = equalizer.GetPresetName(preset);
                if (presetName == currentPresetName)
                {
                    equalizer.UsePreset(preset);
                    for (short band = 0; band < _equalizer.NumberOfBands; band++)
                    {
                        bandLevels.Add(equalizer.GetBandLevel(band));
                    }
                    break;
                }
            }            
            equalizer.Release();
            mediaPlayer.Release();

            return bandLevels;
        }

        //public List<short[]> GetBandLevelRangeForPreset(string presetName)
        //{
        //    var mediaPlayer = new Android.Media.MediaPlayer();
        //    var equalizer = new Equalizer(0, mediaPlayer.AudioSessionId);

        //    equalizer.Get

        //    var ranges = new List<short[]>();
        //    for (short preset = 0; preset < equalizer.NumberOfPresets; preset++)
        //    {
        //        var currentPresetName = equalizer.GetPresetName(preset);
        //        if (presetName == currentPresetName)
        //        {
        //            equalizer.UsePreset(preset);                                     
        //            for (short band = 0; band < _equalizer.NumberOfBands; band++)
        //            {
        //                ranges.Add(_equalizer.GetBandLevelRange()
        //            }
        //            break;
        //        }
        //    }
        //    equalizer.Release();
        //    mediaPlayer.Release();

        //    return ranges;
        //}

        //public void ApplyDefaultCustomBandLevels()
        //{
        //    if (_customBandLevels.Any() && _equalizer != null)
        //    {
        //        for (short band = 0; band < _customBandLevels.Count; band++)
        //        {
        //            _equalizer.SetBandLevel(band, _customBandLevels[band]);
        //        }
        //    }
        //}
    }
}
