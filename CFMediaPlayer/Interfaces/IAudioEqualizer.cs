namespace CFMediaPlayer.Interfaces
{
    public interface IAudioEqualizer
    {        
        public Android.Media.Audiofx.Equalizer? Equalizer { get; set; }

        List<string> PresetNames { get; }

        /// <summary>
        /// Default preset name. "" if custom settings being used
        /// </summary>
        string DefaultPresetName { get; set; } 
        
        /// <summary>
        /// Default custom band levels. Empty if preset used
        /// </summary>
        List<short> DefaultCustomBandLevels { get; set; }

        short[]? GetEqualizerBandLevelRange();

        List<int[]?> GetEqualizerBandFrequencyRanges();

        //int[]? GetEqualizerBandFrequencyRange(short band);

        //short GetEqualizerBandLevel(short band);

        //short? EqualizerBands { get; }

        //void SetBandLevel(short band, short level);
        
        /// <summary>
        /// Apploies preset or custom settings
        /// </summary>
        void ApplyDefault();

        //void ApplyDefaultCustomBandLevels();

        List<short> GetBandLevelsForPreset(string presetName);        
    }
}
