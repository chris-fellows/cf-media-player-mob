namespace CFMediaPlayer.Interfaces
{
    public interface IAudioEqualizer
    {       
        string EqualizerPresetName { get; set; }     

        short[]? GetEqualizerBandLevelRange();

        int[]? GetEqualizerBandFrequencyRange(short band);

        short GetEqualizerBandLevel(short band);

        short? EqualizerBands { get; }

        void SetBandLevel(short band, short level);

        void ApplyPreset();
    }
}
