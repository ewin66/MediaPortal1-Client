namespace ProjectInfinity.Music
{
  public class ExtendedMusicStartMessage : MusicStartMessage
  {
    private int _rating;

    public int Rating
    {
      get { return _rating; }
      set { _rating = value; }
    }
  }
}