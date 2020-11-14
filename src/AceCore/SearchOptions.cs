namespace AceCore {
    public class SearchOptions {
        public string Key { get; set; } = "/Game/";
        public int Window { get; set; } = 64;
        public int MaxBytes = 4096;
        public bool RewindOnMatch {get;set;} = false;
    }
}