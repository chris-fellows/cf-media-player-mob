namespace CFMediaPlayer.Models
{
    public class NameValuePair<TValueType>
    {
        public string Name { get; set; } = String.Empty;

        public TValueType Value { get; set; } = default(TValueType);
    }
}
