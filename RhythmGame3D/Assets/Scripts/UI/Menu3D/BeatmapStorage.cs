using RhythmGame3D.Beatmap;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// Static storage for beatmap data to pass between scenes
    /// </summary>
    public static class BeatmapStorage
    {
        public static BeatmapData currentBeatmap { get; set; }
        public static string currentMusicPath { get; set; }
        
        public static void Clear()
        {
            currentBeatmap = null;
            currentMusicPath = null;
        }
    }
}
