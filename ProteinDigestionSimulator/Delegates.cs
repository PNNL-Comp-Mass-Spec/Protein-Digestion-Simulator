namespace ProteinDigestionSimulator
{
    public delegate void LogEventEventHandler(string Message, PeakMatching.MessageTypeConstants EventType);

    public delegate void ErrorEventEventHandler(string message);

    /// <summary>
    /// Progress changed event
    /// </summary>
    /// <param name="taskDescription"></param>
    /// <param name="percentComplete">ranges from 0 to 100, but can contain decimal percentage values</param>
    public delegate void ProgressChangedEventHandler(string taskDescription, float percentComplete);

    public delegate void ProgressResetEventHandler();

    public delegate void SortingListEventHandler();

    public delegate void SortingMappingsEventHandler();
}
